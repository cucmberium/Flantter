using Flantter.MilkyWay.Plugin;
using Jint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Flantter.MilkyWay.Models.Plugin
{
    public class Core
    {
        private static Core _Instance = new Core();

        public static Core Instance
        {
            get { return _Instance; }
        }

        private Core()
        {
            _Plugins = new Dictionary<string, Plugin>();
        }

        private Dictionary<string, Plugin> _Plugins;

        public async Task Initialize()
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Plugin system initializing...");
#endif

            var folders = await ApplicationData.Current.LocalFolder.GetFoldersAsync();
            StorageFolder pluginFolder = null;
            if (!folders.Any(x => x.Name == "plugins"))
                pluginFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("plugins");
            else
                pluginFolder = folders.First(x => x.Name == "plugins");

            foreach (var folder in await pluginFolder.GetFoldersAsync())
            {
                var name = folder.Name;
                try
                {
                    var file = await pluginFolder.GetFileAsync(folder.Name + ".js");
                    var script = System.IO.File.ReadAllText(file.Path);

                    var engine = new Engine(clr => clr
                        .AllowClr(new Assembly[] { typeof(Flantter.MilkyWay.Plugin.Debug).GetTypeInfo().Assembly,
                                                   typeof(Flantter.MilkyWay.Plugin.Utility).GetTypeInfo().Assembly,
                                                   typeof(Flantter.MilkyWay.Plugin.Event).GetTypeInfo().Assembly,
                                                   typeof(Flantter.MilkyWay.Plugin.Filter).GetTypeInfo().Assembly}));
                    engine.Global.FastAddProperty("Windows", new Jint.Runtime.Interop.NamespaceReference(engine, "Windows"), false, false, false);
                    //engine.Global.FastAddProperty("Flantter", new Jint.Runtime.Interop.NamespaceReference(engine, "Flantter"), false, false, false);

                    _Plugins[name] = new Plugin() { Engine = engine };

                    engine.SetValue("registerPlugin", new Action<string, string, string>((pname, description, version) =>
                    {
                        if (!_Plugins.ContainsKey(pname))
                            return;

                        _Plugins[pname].Name = pname;
                        _Plugins[pname].Description = description;
                        _Plugins[pname].Version = version;

                        try
                        {
                            _Plugins[pname].Engine.Invoke("load");
                        }
                        catch
                        {
                        }
                    }));

                    // Load requrejs for load external library
                    var reqirejs = System.IO.File.ReadAllText("ms-appx:///Models/Plugins/Script/require.js");
                    engine.Execute(reqirejs);

                    engine.Execute(script);
                }
                catch
                {
                    if (_Plugins.ContainsKey(name))
                        _Plugins.Remove(name);
                }
            }
        }
    }

    public class Plugin
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        public Engine Engine { get; set; }
    }
}
