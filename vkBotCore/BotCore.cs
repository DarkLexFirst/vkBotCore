using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using vkBotCore.Plugins;
using vkBotCore.Plugins.Attributes;

namespace vkBotCore
{
    public class BotCore
    {
        public LogChat Log { get; private set; }

        private Dictionary<long, Chat> Chats { get; set; }
        public PluginManager PluginManager { get; set; }
        public MessageHandler MessageHandler { get; set; }

        public IConfiguration Configuration { get; private set; }
        public VkCoreApi VkApi { get; private set; }

        public BotCore(IConfiguration configuration)
        {
            Configuration = configuration;

            VkApi = new VkCoreApi(configuration);

            Log = new LogChat(this);

            Chats = new Dictionary<long, Chat>();

            MessageHandler = new MessageHandler(this);

            PluginManager = new PluginManager(this);
            PluginManager.LoadPlugins();
            PluginManager.EnablePlugins();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton(this);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

        public Chat GetChat(long peerId)
        {
            if (!Chats.ContainsKey(peerId))
                Chats.Add(peerId, new Chat(this, peerId));
            return Chats[peerId];
        }
    }
}
