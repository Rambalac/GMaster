namespace GMaster.Views
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Annotations;
    using Newtonsoft.Json;
    using Tools;
    using Windows.Storage;

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

        public ObservableHashCollection<CameraSettings> Cameras
        {
            get;
            [UsedImplicitly]
            private set;
        }

        public NotifyProperty<ICollection<string>> WiFiAutoconnectAccessPoints { get; } = new string[0];

        public NotifyProperty<bool> WiFiAutoconnectAlways { get; } = false;

        public NotifyProperty<SplitMode> LandscapeSplitMode { get; } = SplitMode.One;

        public NotifyProperty<SplitMode> PortraitSplitMode { get; } = SplitMode.Horizontal;

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