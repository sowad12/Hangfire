using Hangfire;
using Main.Models.Jobs;
using Scheduler.Extensions;
using System.Reflection;

namespace Main.Extensions
{
    public static class DependencyConfiguration
    {
        public static IServiceCollection AddDependencies(
               this IServiceCollection services,
               IConfiguration configuration)
        {
            ILogger<Startup> logger = services
                            .BuildServiceProvider()
                            .GetRequiredService<ILogger<Startup>>();
            try
            {

                services.AddSwaggerService(configuration);
                services.AddControllers();
                services.AddAuthorization();
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        

                services.AddScheduler(configuration, new List<Assembly> { typeof(FireAndForgotJob).Assembly });

             
            }
            catch (System.Exception e)
            {
                logger.LogError("Error Message: " + e.Message + ". AND Stack Trace: " + e.StackTrace);
            }

            return services;
        }

        public static IApplicationBuilder UseDependencies(this IApplicationBuilder app,
            IConfiguration configuration,
            IRecurringJobManager recurringJobManager,
            IServiceProvider serviceProvider)
        {
            app.UseSwaggerService();

            //app.UseCorsServices(configuration);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();
            app.UseScheduler(recurringJobManager, serviceProvider, new List<Assembly> { typeof(FireAndForgotJob).Assembly });

            app.UseHttpsRedirection();
     

            return app;
        }

    }
}
