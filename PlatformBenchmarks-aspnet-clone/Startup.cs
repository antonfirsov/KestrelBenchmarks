// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO.Pipelines;
using IoUring.Transport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedHat.AspNetCore.Server.Kestrel.Transport.Linux;

namespace PlatformBenchmarks
{
    internal enum KestrelTransport
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
           
        }
        
        public void Configure(IApplicationBuilder app)
        {
        }
    }
}
