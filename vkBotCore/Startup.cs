using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VkBotCore
{
    public class Startup
    {

		internal static Action OnDisable;
        public static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU"); //fix datetime format on linux
            CreateWebHostBuilder(args).Build().Run();
			OnDisable?.Invoke();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            string port = args.Length > 0 ? port = args[0] : "80";
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls($"http://*:{port}")
                .UseStartup<BotCore>();
        }
    }
}
