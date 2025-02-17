using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using System;
using Microsoft.Extensions.Configuration;

namespace Talent.Common.Services
{
    public class ServiceHost : IServiceHost
    {
        private readonly IWebHost _webHost;

        public ServiceHost(IWebHost webHost)
        {
            _webHost = webHost;
        }

        public void Run() => _webHost.Run();

        public static HostBuilder Create<TStartup>(string[] args, string hostUrl) where TStartup : class
        {
            Console.Title = typeof(TStartup).Namespace;

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("Properties/launchSettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var webHostBuilder = WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseUrls(hostUrl) // Use the hostUrl passed from the Main method
                .UseStartup<TStartup>()
                .UseDefaultServiceProvider(options => options.ValidateScopes = false);

            return new HostBuilder(webHostBuilder.Build());
        }


        public abstract class BuilderBase
        {
            public abstract ServiceHost Build();
        }

        public class HostBuilder : BuilderBase
        {
            private readonly IWebHost _webHost;

            public HostBuilder(IWebHost webhost)
            {
                _webHost = webhost;
            }

            public override ServiceHost Build()
            {
                // Implement the method to return a ServiceHost
                return new ServiceHost(_webHost);
            }
        }
    }
}