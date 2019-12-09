using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pixeval.Objects;
using PropertyChanged;

namespace Pixeval.Caching.Persisting
{
    [AddINotifyPropertyChangedInterface]
    public class Settings
    {
        public static Settings Global = new Settings();

        public bool SyncOnStart { get; set; }

        public bool QueryR18 { get; set; }

        public bool SortOnInserting { get; set; }

        public bool CachingThumbnail { get; set; }

        public int MinBookmark { get; set; }

        public string DownloadLocation { get; set; }

        public IEnumerable<string> ExceptTags { get; set; }

        public IEnumerable<string> ContainsTags { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }

        public async Task Store()
        {
            if (!Directory.Exists(PixevalEnvironment.SettingsFolder))
                Directory.CreateDirectory(PixevalEnvironment.SettingsFolder);
            await File.WriteAllTextAsync(Path.Combine(PixevalEnvironment.SettingsFolder, "settings.json"), Global.ToString());
        }

        public async Task<Settings> Restore()
        {
            if (File.Exists(Path.Combine(PixevalEnvironment.SettingsFolder, "settings.json")))
            {
                Global = (await File.ReadAllTextAsync(Path.Combine(PixevalEnvironment.SettingsFolder, "settings.json"))).FromJson<Settings>();
            }
            return Global;
        }

        public void Clear()
        {
            File.Delete(Path.Combine(PixevalEnvironment.SettingsFolder, "settings.json"));
            Global.DownloadLocation = null;
            Global.SortOnInserting = false;
            Global.CachingThumbnail = false;
            Global.ContainsTags = null;
            Global.ExceptTags = null;
            Global.QueryR18 = false;
            Global.SyncOnStart = false;
            Global.MinBookmark = 0;
        }
    }
}