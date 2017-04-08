namespace GMaster.Views
{
    using Tools;

    public class CameraSettings : SettingsContainer, IStringIdItem
    {
        public CameraSettings(string id)
        {
            Id = id;
        }

        public NotifyProperty<string> Aspect { get; } = "1";

        public NotifyProperty<bool> Autoconnect { get; } = true;

        public GeneralSettings GeneralSettings { get; set; }

        public string Id { get; }

        public NotifyProperty<bool> IsAspectAnamorphingVideoOnly { get; } = true;

        public NotifyProperty<string> LutId { get; } = string.Empty;
    }
}