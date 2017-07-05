using System;
using System.IO;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

[assembly: OwinStartup(typeof(InstantMessagingServerApp.OwinConfig))]

namespace InstantMessagingServerApp
{
    public class OwinConfig
    {
        public void Configuration(IAppBuilder app)
        {
            // map SignalR
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
            // configure static files
            string root = AppDomain.CurrentDomain.BaseDirectory;
            var physicalFileSystem = new PhysicalFileSystem(Path.Combine(root, "wwwroot"));
            var options = new FileServerOptions
            {
                RequestPath = PathString.Empty,
                EnableDefaultFiles = true,
                FileSystem = physicalFileSystem
            };
            options.StaticFileOptions.FileSystem = physicalFileSystem;
            options.StaticFileOptions.ServeUnknownFileTypes = false;
            app.UseFileServer(options);
        }
    }
}
