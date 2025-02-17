using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Talent.Api.Handlers;
using Talent.Common.Auth;
using Talent.Common.Events;
using Talent.Common.Mongo;
using Talent.Common.RabbitMq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Features;
using Talent.Common.Aws;
using Talent.Common.Commands;
using Talent.Common.Contracts;
using Talent.Common.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;
using Talent.Common.Security;
using Talent.Api.Domain.Contracts;
using Talent.Api.Domain.Services;
using Microsoft.Extensions.Hosting; // Add this for IWebHostEnvironment

namespace Talent.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowWebAppAccess", builder =>
                {
                    builder.SetIsOriginAllowed(_ => true) // Allows all origins
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
                x.MultipartHeadersLengthLimit = int.MaxValue;
            });

            // Replace AddMvc() with AddControllers() and AddNewtonsoftJson()
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver
                        = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                });

            services.AddJwt(Configuration);
            services.AddMongoDB(Configuration);
            services.AddRabbitMq(Configuration);
            services.AddAws(Configuration);
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            Func<IServiceProvider, IPrincipal> getPrincipal =
                (sp) => sp.GetService<IHttpContextAccessor>().HttpContext.User;
            services.AddScoped(typeof(Func<IPrincipal>), sp =>
            {
                Func<IPrincipal> func = () => getPrincipal(sp);
                return func;
            });

            services.AddScoped<IUserAppContext, UserAppContext>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IProfileService, ProfileService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) // Change to IWebHostEnvironment
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors("AllowWebAppAccess");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
