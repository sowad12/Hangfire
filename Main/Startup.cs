
using Hangfire;
using Main.Extensions;
using Microsoft.Net.Http.Headers;
using System;

namespace Main
{
    public class Startup
    {
        public IConfiguration _configuration { get; }
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDependencies(_configuration);
            

            //services.AddControllers(options =>
            //{
            //    options.Filters.Add(typeof(InputValidationFilter));             //options.Filters.Add(type of(ExceptionFilter));
            //})
            //.AddFluentValidation(fv =>
            //{
            //    fv.RegisterValidatorsFromAssemblyContaining<CreateClubCommandValidator>();
            //})
            //.AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore) //(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
            //.AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<CreateClubCommandValidator>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            IRecurringJobManager recurringJobManager,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
             if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();

                }
            app.UseDependencies(configuration, recurringJobManager, serviceProvider);
            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}
