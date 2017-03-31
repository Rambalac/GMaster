using System.Collections.Generic;
using System.ComponentModel;
using Windows.Storage;
using GMaster.Camera.LumixResponces;
using Newtonsoft.Json;

namespace GMaster.Views
{
    public class GeneralSettings : SettingsContainer
    {
        public GeneralSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("settings", out var settingsobj) && settingsobj is string settings)
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(settings);
                Load(dict);
            }

            PropertyChanged += GeneralSettings_PropertyChanged;
        }

        public NotifyProperty<bool> Autoconnect { get; } = true;

        public ObservableHashCollection<CameraSettings> Cameras { get; private set; }

        public void Save()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var dict = new Dictionary<string, object>();
            Save(dict);
            localSettings.Values["settings"] = JsonConvert.SerializeObject(dict);
        }

        private void GeneralSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Save();
        }
    }
}