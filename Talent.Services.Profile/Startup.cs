using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Talent.Common.Auth;
using Talent.Common.Commands;
using Talent.Common.Contracts;
using Talent.Common.Mongo;
using Talent.Common.RabbitMq;
using Talent.Common.Security;
using Talent.Common.Services;
using Talent.Services.Profile.Domain.Contracts;
using Talent.Services.Profile.Domain.Services;
using Talent.Services.Profile.Handler;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Talent.Common.Aws;
using Microsoft.OpenApi.Models;

namespace Talent.Services.Profile
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowWebAppAccess", builder =>
                {
                    //builder
                    //    .WithOrigins("http://localhost:61771", "http://localhost:60998")
                   builder.SetIsOriginAllowed(_ => true)
                       .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            //services.Configure<FormOptions>(x =>
            //{
            //    x.ValueLengthLimit = int.MaxValue;
            //    x.MultipartBodyLengthLimit = int.MaxValue;
            //    x.MultipartHeadersLengthLimit = int.MaxValue;
            //});

            services.AddControllers();
            //.AddNewtonsoftJson(options =>
            //{
            //    options.SerializerSettings.ContractResolver
            //        = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            //});

            // Add Swagger services
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Talent Profile API",
                    Version = "v1",
                    Description = "API documentation for Talent.Services.Profile",
                });
            });


            services.AddJwt(Configuration);
            services.AddMongoDB(Configuration);
            services.AddRabbitMq(Configuration);
            services.AddAws(Configuration);
            services.AddScoped<ICommandHandler<AuthenticateUser>, AuthenticateUserHandler>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            Func<IServiceProvider, IPrincipal> getPrincipal =
                (sp) => sp.GetService<IHttpContextAccessor>().HttpContext.User;

            services.AddScoped(typeof(Func<IPrincipal>), sp => () => getPrincipal(sp));
            services.AddScoped<IUserAppContext, UserAppContext>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IFileService, FileService>();

            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable Swagger middleware
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Talent Profile API v1");
                c.RoutePrefix = string.Empty;
            });



            app.UseCors("AllowWebAppAccess");

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

           

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
