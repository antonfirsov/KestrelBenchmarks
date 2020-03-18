// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO.Pipelines;
using System.Net;
using IoUring.Transport;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedHat.AspNetCore.Server.Kestrel.Transport.Linux;

namespace PlatformBenchmarks
{
    internal enum Transport
    {
        Sockets,
        IoUringTransport,
        LinuxTransport
    }

    internal enum ApplicationSchedulingMode
    {
        Default,
        Inline
    }
    
    public static class BenchmarkConfigurationHelpers
    {
        public static IWebHostBuilder UseBenchmarksConfiguration(this IWebHostBuilder builder, IConfiguration configuration)
        {
            builder.UseConfiguration(configuration);

            // Handle the transport settings
            builder.ConfigureServices(services => services.ConfigureTransport(configuration));
            return builder;
        }

        public static void ConfigureTransport(this IServiceCollection services, IConfiguration configuration)
        {
             Transport transport = 
                configuration.GetValue(nameof(Transport), Transport.Sockets);
            ApplicationSchedulingMode schedulingMode =
                configuration.GetValue(nameof(ApplicationSchedulingMode),
                    ApplicationSchedulingMode.Default);
            
            var threadCountRaw = configuration.GetValue<string>("threadCount");
            int? theadCount = null;
            
            if (!string.IsNullOrEmpty(threadCountRaw) && Int32.TryParse(threadCountRaw, out var value))
            {
                theadCount = value;
            }
            
            switch (transport)
            {
                case Transport.IoUringTransport:
                    Console.WriteLine($"Setting IoUringTransport");
                    services.AddIoUringTransport(options =>
                    {
                        if (schedulingMode == ApplicationSchedulingMode.Inline)
                        {
                            Console.WriteLine("Setting PipeScheduler.Inline");
                            options.ApplicationSchedulingMode = PipeScheduler.Inline;
                        }
                        if (theadCount.HasValue)
                        {
                            Console.WriteLine($"Setting ThreadCount={theadCount.Value}");
                            options.ThreadCount = theadCount.Value;
                        }
                    });
                    break;
                case Transport.LinuxTransport:
                    Console.WriteLine($"Setting LinuxTransport");
                    services.AddSingleton<IConnectionListenerFactory, LinuxTransportFactory>();
                    services.Configure<LinuxTransportOptions>(options =>
                    {
                        if (schedulingMode == ApplicationSchedulingMode.Inline)
                        {
                            Console.WriteLine("Setting PipeScheduler.Inline");
                            options.ApplicationSchedulingMode = PipeScheduler.Inline;
                        }
                        if (theadCount.HasValue)
                        {
                            Console.WriteLine($"Setting ThreadCount={theadCount.Value}");
                            options.ThreadCount = theadCount.Value;
                        }
                    });
                    break;
                default:
                    Console.WriteLine("Using Socket Transport");
                    services.AddSingleton<IConnectionListenerFactory, SocketTransportFactory>();
                    services.Configure<SocketTransportOptions>(options =>
                    {
                        if (theadCount.HasValue)
                        {
                            Console.WriteLine($"Setting ThreadCount={theadCount.Value}");
                            options.IOQueueCount = theadCount.Value;
                        }
                    });
                    
                    break;
            }
        }

        public static IPEndPoint CreateIPEndPoint(this IConfiguration config)
        {
            var url = config["server.urls"] ?? config["urls"];

            if (string.IsNullOrEmpty(url))
            {
                return new IPEndPoint(IPAddress.Loopback, 8080);
            }

            var address = BindingAddress.Parse(url);

            IPAddress ip;

            if (string.Equals(address.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                ip = IPAddress.Loopback;
            }
            else if (!IPAddress.TryParse(address.Host, out ip))
            {
                ip = IPAddress.IPv6Any;
            }

            return new IPEndPoint(ip, address.Port);
        }
    }
}
