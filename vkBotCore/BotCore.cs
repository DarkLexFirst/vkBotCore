using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VkBotCore.Plugins;
using VkBotCore.Plugins.Attributes;
using VkBotCore.Subjects;

namespace VkBotCore
{
	public class BotCore
	{
		public LogChat Log { get; private set; }

		public PluginManager PluginManager { get; set; }

		public IConfiguration Configuration { get; private set; }
		public VkCoreApi VkApi { get; private set; }

		public BotCore(IConfiguration configuration)
		{
			Configuration = configuration;

			VkApi = new VkCoreApi(this);

			Log = new LogChat(VkApi);

			PluginManager = new PluginManager(this);
			PluginManager.LoadPlugins();
			PluginManager.EnablePlugins();

			Startup.OnDisable = OnDisable;

			StartSaveTimer();
		}

		private void OnDisable()
		{
			Console.WriteLine("Shutdown...");
			PluginManager.DisablePlugins();
			SaveAll(true);
			Console.WriteLine("Disabled");
		}

		private Timer _timer;
		private void StartSaveTimer()
		{
			_timer = new Timer(1000);
			_timer.AutoReset = true;
			_timer.Elapsed += (s, e) => SaveAll();
			_timer.Start();
		}

		private void SaveAll(bool forced = false)
		{
			foreach (var api in VkApi._vkApi)
			{
				foreach (var user in api.Value._usersCache)
					if (user.Value is User u)
						u.Storage.Save(forced);
				foreach (var chat in api.Value._chatsCache)
					if (chat.Value is Chat c)
						c.Storage.Save(forced);
			}
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
			services.AddSingleton(this);

			services.AddMvc(options => options.EnableEndpointRouting = false).AddNewtonsoftJson();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseMvc();
		}
	}
}
