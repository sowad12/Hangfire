using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scheduler.Interfaces;
using Scheduler.Options;
using Scheduler.Services;
using System.Reflection;

namespace Scheduler.Extensions
{
    public static class SchedulerExtensions
    {
        private static IServiceCollection _serviceCollection;

        public static IServiceCollection AddScheduler(
            this IServiceCollection services,
            IConfiguration configuration,
            IEnumerable<Assembly> executorAssemblies = null)
        {
            _serviceCollection = services;

            IConfigurationSection configurationSection = configuration.GetSection(nameof(SchedulerOptions));
            SchedulerOptions schedulerOptions = configurationSection.Get<SchedulerOptions>();

            services.Configure<SchedulerOptions>(configurationSection);

            if (!schedulerOptions.Enabled) return services;

            List<Assembly> asseblies = new() { typeof(IJobExecutor<>).Assembly };
            if (executorAssemblies != null && executorAssemblies.Any())
            {
                asseblies.AddRange(executorAssemblies);
            }

            foreach (Assembly assembly in asseblies)
            {
                IEnumerable<TypeInfo> classTypes = assembly.ExportedTypes.Select(t => t.GetTypeInfo()).Where(t => t.IsClass && !t.IsAbstract);
                foreach (TypeInfo type in classTypes)
                {
                    IEnumerable<TypeInfo> interfaces = type.ImplementedInterfaces.Select(i => i.GetTypeInfo());
                    foreach (TypeInfo handlerType in interfaces.Where(i =>
                    (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJobExecutor<>)) || i.Name == nameof(IJobService)))
                    {
                        services.AddTransient(handlerType.AsType(), type.AsType());
                    }
                }
            }

            SchedulerOptions option = new SchedulerOptions();
            configuration.GetSection(nameof(SchedulerOptions)).Bind(option);
            services.AddHangfire(x =>
                x.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSqlServerStorage(option.StorageOptions.ConnectionString)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseDefaultTypeSerializer()
            );
            services.AddHangfireServer();

            return services;
        }

        public static IApplicationBuilder UseScheduler(this IApplicationBuilder app,
            IRecurringJobManager recurringJobManager,
            IServiceProvider serviceProvider,
            IEnumerable<Assembly> scheduleJobsAssemblies = null
            )
        {
            var monitor = app.ApplicationServices.GetService<IOptionsMonitor<SchedulerOptions>>();
            var configuration = app.ApplicationServices.GetService<IConfiguration>();

            if (!monitor.CurrentValue.Enabled)
                return app;

            if (monitor.CurrentValue.StorageType.Equals("sqlserver", StringComparison.OrdinalIgnoreCase))
            {
                GlobalConfiguration
                    .Configuration
                    .UseActivator(new SchedulerJobActivator(app.ApplicationServices));

                GlobalConfiguration
                    .Configuration
                    .UseSqlServerStorage(monitor.CurrentValue.StorageOptions.ConnectionString, new SqlServerStorageOptions
                    {
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        SchemaName = monitor.CurrentValue.StorageOptions.Schema
                    });
            }

            app.UseHangfireDashboard(monitor.CurrentValue.Dashboard, new DashboardOptions
            {
                Authorization = new[] { new BasicAuthorizationFilter() }
            });

            /*          IScheduledJobExecutor service = serviceProvider.GetRequiredService<IScheduledJobExecutor>();
                        recurringJobManager.AddOrUpdate(service.GetUniqueJobId(), () => service.Execute(), service.GetCron());
            */

            // ADD RECURRING JOBS
            AddJobs(recurringJobManager, serviceProvider, configuration, scheduleJobsAssemblies);

            return app;
        }

        public static void AddJobs(
            IRecurringJobManager recurringJobManager,
            IServiceProvider serviceProvider,
            IConfiguration configuration = null,
            IEnumerable<Assembly> jobAssemblies = null)
        {
            List<Assembly> assemblies = new List<Assembly>();

            if (jobAssemblies != null && jobAssemblies.Any())
            {
                assemblies.AddRange(jobAssemblies);
            }

            assemblies.Add(typeof(IScheduledJobExecutor).Assembly);
            TypeInfo interfaceTypeInfo = typeof(IScheduledJobExecutor).GetTypeInfo();

            IEnumerable<TypeInfo> implementedTypes = assemblies
                .SelectMany(x => x.ExportedTypes)
                .Select(t => t.GetTypeInfo())
                .Where(t => t.IsClass && !t.IsAbstract && t.ImplementedInterfaces.Any(y => y.GetTypeInfo() == interfaceTypeInfo));

            foreach (TypeInfo type in implementedTypes)
            {
                object jobInstance = ActivatorUtilities.CreateInstance(serviceProvider, type.AsType());

                string jobId = (string)type.GetMethod(nameof(IScheduledJobExecutor.GetUniqueJobId)).Invoke(jobInstance, null);

                MethodInfo scheduleExecutionMethod = type.GetMethod(nameof(IScheduledJobExecutor.Execute));

                string jobCron = (string)type.GetMethod(nameof(IScheduledJobExecutor.GetCron)).Invoke(jobInstance, null);
                string cron = string.IsNullOrEmpty(jobCron) ? Cron.Daily() : jobCron;

                recurringJobManager.AddOrUpdate(jobId, new Job(scheduleExecutionMethod), cron);
            }
        }
    }

    public class BasicAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }

    /*public class MyScheduledJobExecutor : IScheduledJobExecutor
    {
        private readonly ILogger<MyScheduledJobExecutor> _logger;

        public MyScheduledJobExecutor(ILogger<MyScheduledJobExecutor> logger)
        {
            _logger = logger;
        }

        public async Task Execute()
        {
            _logger.LogWarning("This is an warning!");
            Console.WriteLine("Recurring...");

            await Task.FromResult("");
        }

        public string GetCron()
        {
            return Cron.Daily();
        }

        public string GetUniqueJobId()
        {
            return nameof(MyScheduledJobExecutor);
        }
    }*/


    /* OLD CODE */
    /*    public static class SchedulerExtensions
        {
            private static IServiceCollection _serviceCollection;

            public static IServiceCollection AddScheduler(
                this IServiceCollection services,
                IConfiguration configuration,
                IEnumerable<Assembly> executorAssemblies = null)
            {

                _serviceCollection = services;
                services.AddTransient<IJobService, JobService>();

                services.Configure<SchedulerOptions>(configuration.GetSection(nameof(SchedulerOptions)));
                var isSchedulerEnabled = configuration.GetValue<bool>($"{nameof(SchedulerOptions)}:{nameof(SchedulerOptions.Enabled)}");

                if (!isSchedulerEnabled)
                    return services;

                services.AddTransient<IBackgroundJobClient, BackgroundJobClient>();
                services.AddTransient<IRecurringJobManager, RecurringJobManager>();
                services.AddHangfire(config =>
                {
                    config.UseSerilogLogProvider();
                });

                var asseblies = new List<Assembly> { typeof(IJobExecutor<>).Assembly };
                if (executorAssemblies != null && executorAssemblies.Any())
                {
                    asseblies.AddRange(executorAssemblies);
                }

                foreach (var assembly in asseblies)
                {
                    var classTypes = assembly.ExportedTypes.Select(t => t.GetTypeInfo()).Where(t => t.IsClass && !t.IsAbstract);

                    foreach (var type in classTypes)
                    {
                        var interfaces = type.ImplementedInterfaces.Select(i => i.GetTypeInfo());
                        foreach (var handlerType in interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJobExecutor<>)))
                        {
                            services.AddTransient(handlerType.AsType(), type.AsType());
                        }
                    }
                }

                return services;
            }

            public static IApplicationBuilder UseScheduler(this IApplicationBuilder app, IEnumerable<Assembly> scheduleJobsAssemblies = null)
            {

                var monitor = app.ApplicationServices.GetService<IOptionsMonitor<SchedulerOptions>>();
                var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
                var configuration = app.ApplicationServices.GetService<IConfiguration>();

                if (!monitor.CurrentValue.Enabled)
                    return app;

                if (monitor.CurrentValue.StorageType.Equals("sqlserver", StringComparison.OrdinalIgnoreCase))
                {
                    // For resolving dependency in Job
                    GlobalConfiguration
                        .Configuration
                        .UseActivator(new ChameraJobActivator(app.ApplicationServices));

                    GlobalConfiguration
                        .Configuration
                        .UseSqlServerStorage(monitor.CurrentValue.StorageOptions.ConnectionString, new SqlServerStorageOptions
                        {
                            QueuePollInterval = TimeSpan.FromSeconds(15),
                            SchemaName = monitor.CurrentValue.StorageOptions.Schema
                        });
                }

                //JobHelper.SetSerializerSettings(new JsonSerializerSettings
                //{
                //    TypeNameHandling = TypeNameHandling.Objects
                //});

                app.UseHangfireServer();
                app.UseSchedulerDashboard(loggerFactory, monitor);

                // Add Jobs
                AddJobs(_serviceCollection, configuration, scheduleJobsAssemblies);

                return app;
            }

            public static IApplicationBuilder UseSchedulerDashboard(this IApplicationBuilder app, ILoggerFactory loggerFactory, IOptionsMonitor<SchedulerOptions> monitor)
            {
                if (!monitor.CurrentValue.Enabled)
                    return app;

                //if (monitor.CurrentValue.SecurityEnabled)
                //  {
                //      app.UseHangfireDashboard(monitor.CurrentValue.Dashboard, options: new DashboardOptions
                //      {
                //          Authorization = new[]
                //          {
                //              new HangfireAuthorizationFilter()
                //          }
                //      });
                //  }
                //  else
                //  {
                //      app.UseHangfireDashboard(monitor.CurrentValue.Dashboard);
                //      //app.UseHangfireDashboard();
                //  }

                app.UseHangfireDashboard(monitor.CurrentValue.Dashboard);
                return app;
            }

            public static void AddJobs(this IServiceCollection serviceCollection, IConfiguration configuration = null, IEnumerable<Assembly> jobAssemblies = null)
            {

                List<Assembly> assemblies = new List<Assembly>();

                if (jobAssemblies != null && jobAssemblies.Any())
                {
                    assemblies.AddRange(jobAssemblies);
                }

                assemblies.Add(typeof(IScheduledJobExecutor).Assembly);

                var types = assemblies.SelectMany(x => x.ExportedTypes)
                    .Select(t => t.GetTypeInfo())
                    .Where(t => t.IsClass && !t.IsAbstract);

                var baseJobTypeInfo = typeof(IScheduledJobExecutor).GetTypeInfo();
                var recurringJobManager = new RecurringJobManager();

                var ss = serviceCollection.BuildServiceProvider();

                foreach (var type in types)
                {
                    var interfaces = type
                        .ImplementedInterfaces
                        .Select(i => i.GetTypeInfo());

                    if (interfaces.Any(x => x == baseJobTypeInfo))
                    {
                        var scheduleMethod = type.GetMethod(nameof(IScheduledJobExecutor.Execute));
                        var jobInstance = ActivatorUtilities.CreateInstance(ss, type.AsType());
                        var jobId = (string)type.GetMethod(nameof(IScheduledJobExecutor.GetUniqueJobId)).Invoke(jobInstance, null);
                        var jobCron = (string)type.GetMethod(nameof(IScheduledJobExecutor.GetCron)).Invoke(jobInstance, null);
                        var cron = string.IsNullOrEmpty(jobCron) ? Cron.Daily() : jobCron;
                        recurringJobManager.AddOrUpdate(jobId, new Job(scheduleMethod), cron);
                    }
                }
            }
        }
    */

}
