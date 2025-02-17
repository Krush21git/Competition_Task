﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Talent.Common.Commands;
using Talent.Common.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Talent.Services.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Explicitly add launchSettings.json and build the configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("Properties/launchSettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            // Get the URLs from the configuration, defaulting to a specific URL if not set
            var hostUrl = config["ASPNETCORE_URLS"] ?? "http://0.0.0.0:60880";

            // Pass the configuration and hostUrl to ServiceHost.Create
            ServiceHost.Create<Startup>(args, hostUrl)
                // .UseRabbitMq()
                // .SubcribeToCommand<AuthenticateUser>()
                // .SubcribeToCommand<CreateUser>()
                .Build()
                .Run();
        }

    }
}
