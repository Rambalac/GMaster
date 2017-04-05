namespace GMaster.Views
{
    using Tools;

    public class CameraSettings : SettingsContainer, IStringIdItem
    {
        public CameraSettings(string id)
        {
            Id = id;
        }

        public NotifyProperty<bool> Autoconnect { get; } = true;

        public NotifyProperty<string> LutName { get; } = string.Empty;

        public GeneralSettings GeneralSettings { get; set; }

        public string Id { get; }
    }
}