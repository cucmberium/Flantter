using Jint;
using Jint.Runtime.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;

namespace Flantter.MilkyWay.Models.Plugin
{
    public class Core
    {
        private readonly Dictionary<string, Plugin> _plugins = new Dictionary<string, Plugin>();
        public static Core Instance { get; } = new Core();

        public async Task Initialize()
        {
#if DEBUG
            Debug.WriteLine("Plugin system initializing...");
#endif

            var folders = await ApplicationData.Current.LocalFolder.GetFoldersAsync();
            StorageFolder pluginFolder;
            if (folders.All(x => x.Name != "plugins"))
                pluginFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("plugins");
            else
                pluginFolder = folders.First(x => x.Name == "plugins");

            foreach (var folder in await pluginFolder.GetFoldersAsync())
            {
                var name = folder.Name;
                try
                {
                    var file = await folder.GetFileAsync(folder.Name + ".js");
                    var script = File.ReadAllText(file.Path);

                    var engine = new Engine(clr => clr
                        .AllowClr(typeof(Flantter.MilkyWay.Plugin.Debug).GetTypeInfo().Assembly,
                            typeof(Flantter.MilkyWay.Plugin.Utility).GetTypeInfo().Assembly, typeof(Flantter.MilkyWay.Plugin.Event).GetTypeInfo().Assembly,
                            typeof(Flantter.MilkyWay.Plugin.Filter).GetTypeInfo().Assembly));
                    engine.Global.FastAddProperty("Windows", new NamespaceReference(engine, "Windows"), false, false,
                        false);
                    engine.Global.FastAddProperty("Flantter", new NamespaceReference(engine, "Flantter"), false, false,
                        false);
                    _plugins[name] = new Plugin {Engine = engine};

                    engine.SetValue("registerPlugin", new Action<string, string, string>(
                        (pname, description, version) =>
                        {
                            if (!_plugins.ContainsKey(pname))
                                return;

                            _plugins[pname].Name = pname;
                            _plugins[pname].Description = description;
                            _plugins[pname].Version = version;

                            try
                            {
                                _plugins[pname].Engine.Invoke("load");
                            }
                            catch
                            {
                            }
                        }));

                    engine.Execute(script);
                }
                catch
                {
                    if (_plugins.ContainsKey(name))
                        _plugins.Remove(name);
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