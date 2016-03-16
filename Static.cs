using System.IO;
using System.Xml.Serialization;

namespace Weather_Images_Downloader
{
    static class Static
    {
        public static Settings SettingsInstance { get; set; }

        static Static()
        {
            SettingsInstance = new Settings();
        }

        public static void SaveSettings()
        {
            using (var fs = new FileStream("weathersettings.xml", FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(fs, SettingsInstance);
            }
        }

        public static void LoadSettings()
        {
            if (!File.Exists("weathersettings.xml")) return;

            Settings settings;

            using (var fs = new FileStream("weathersettings.xml", FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof (Settings));
                settings = serializer.Deserialize(fs) as Settings;
            }

            var properties = SettingsInstance.GetType().GetProperties();

            foreach (var property in properties)
            {
                SettingsInstance.GetType().GetProperty(property.Name).SetValue(SettingsInstance, property.GetValue(settings));
            }
        }
    }
}
