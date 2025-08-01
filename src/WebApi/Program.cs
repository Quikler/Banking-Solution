using BAL.Providers;
using BAL.Services.Account;
using Common.Configurations;
using Contracts.V1.Responses;
using Contracts.V1.Validators;
using DAL;
using DAL.Entities;
using DAL.Seed.User;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(ms => ms.Value is not null && ms.Value.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage) ?? [])
            .ToList();

        var failureResponse = new FailureResponse { Errors = errors };
        return new BadRequestObjectResult(failureResponse);
    };
});

builder.Services.AddValidatorsFromAssemblyContaining<DepositRequestValidator>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services
    .AddIdentity<UserEntity, RoleEntity>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredUniqueChars = 0;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .Configure<SwaggerConfiguration>(builder.Configuration.GetSection(nameof(SwaggerConfiguration)))
    .Configure<JwtConfiguration>(builder.Configuration.GetSection(nameof(JwtConfiguration)));

var jwtConfig = builder.Configuration.GetSection(nameof(JwtConfiguration)).Get<JwtConfiguration>()
    ?? throw new InvalidOperationException("Configuration for JwtConfiguration is missing or invalid.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = jwtConfig.ValidIssuer,
        ValidAudience = jwtConfig.ValidAudience,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtConfig.SecretKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAccontManagement, AccountManagement>();
builder.Services.AddScoped<IAccountTransactionService, AccountTransactionService>();

builder.Services.AddTransient<TokenProvider>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Banking-Solution API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey, // 'Bearer 12345abcdef'
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, []
        }
    });

    var xmlDocFile = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).FirstOrDefault();
    if (xmlDocFile is not null)
    {
        c.IncludeXmlComments(xmlDocFile);
    }
});

var app = builder.Build();

app.MapControllers();

app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    using var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }

    await serviceProvider.SeedDefaultUsersAsync();
}

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var swaggerConfig = app.Services.GetRequiredService<IOptions<SwaggerConfiguration>>().Value;

    app.UseSwagger(options =>
    {
        options.RouteTemplate = swaggerConfig.JsonRoute;
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(swaggerConfig.UIEndpoint, swaggerConfig.Description);
    });
}

app.UseHttpsRedirection();

app.Run();
