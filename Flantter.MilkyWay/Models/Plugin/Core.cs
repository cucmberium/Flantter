using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Flantter.MilkyWay.Models.Plugin
{
    public class Core
    {
        private readonly Dictionary<string, PluginInfo> _plugins = new Dictionary<string, PluginInfo>();
        public static Core Instance { get; } = new Core();

        public async Task Initialize()
        {
            Debug.WriteLine("Plugin system initializing...");

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
                }
                catch
                {
                    if (_plugins.ContainsKey(name))
                        _plugins.Remove(name);
                }
            }
        }
    }

    public class PluginInfo
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }
    }
}