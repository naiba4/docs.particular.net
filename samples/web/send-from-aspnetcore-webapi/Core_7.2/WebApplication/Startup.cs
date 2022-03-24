using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<WebCurrentSessionHolderMiddleware>();

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseMiddleware<WebCurrentSessionHolderMiddleware>();
        app.UseEndpoints(c =>
        {
            c.MapControllers();
        });
    }
}