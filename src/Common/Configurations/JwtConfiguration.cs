namespace Common.Configurations;

public class JwtConfiguration
{
    public required string SecretKey { get; set; }
    public required string ValidAudience { get; set; }
    public required string ValidIssuer { get; set; }
    public required TimeSpan TokenLifetime { get; set; }
    public required TimeSpan RefreshTokenLifetime { get; set; }
}
