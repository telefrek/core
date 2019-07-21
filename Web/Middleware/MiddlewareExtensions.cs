using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telefrek.Web.Middleware;

namespace Telefrek.Web
{
    /// <summary>
    /// Middleware extensions
    /// </summary>
    public static partial class TelefrekWebExtensions
    {
        public static void AddLoadShedding(this IApplicationBuilder appBuilder)
            => appBuilder.UseMiddleware<LoadSheddingMiddleware>();

        public static void ConfigureLoadShedding(this IServiceCollection services, IConfiguration configuration)
            => services.Configure<LoadSheddingConfiguration>(configuration.GetSection("load_shedding"));
    }
}