using ScreenCaptureService;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Topshelf;

namespace ScreenshotApp
{
    class Program
    {
        

        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<ScreenshotCaptureService>(s =>
                {
                    s.ConstructUsing(name => new ScreenshotCaptureService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();
                x.StartAutomatically();
                x.SetDescription("Screenshot Capture Service");
                x.SetDisplayName("ScreenshotService");
                x.SetServiceName("ScreenshotService");
            });
            Console.WriteLine("Screenshot saved.");
        }

        
    }
}