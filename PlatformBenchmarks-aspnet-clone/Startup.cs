// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using IoUring.Transport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedHat.AspNetCore.Server.Kestrel.Transport.Linux;

namespace PlatformBenchmarks
{
    internal enum Transport
    {
        Default,
        URing,
        LinuxTransport
    }
    
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
            Transport transport = Configuration.GetValue<Transport>("transport", Transport.Default);
            
            switch (transport)
            {
                case Transport.URing:
                    Console.WriteLine($"Setting IoUringTransport");
                    services.AddIoUringTransport();
                    break;
                case Transport.LinuxTransport:
                    Console.WriteLine($"Setting LinuxTransport");
                    services.AddSingleton<IConnectionListenerFactory, LinuxTransportFactory>();
                    break;
            }
        }
        
        public void Configure(IApplicationBuilder app)
        {
        }
    }
}
