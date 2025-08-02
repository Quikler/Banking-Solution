namespace WebApi.Extensions;

public static class WebHostEnvironmentExtensions
{
    public static bool IsDocker(this IWebHostEnvironment webHostEnvironment) => webHostEnvironment.IsEnvironment("Docker");
}
