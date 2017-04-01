namespace GMaster.Views
{
    using Camera.LumixResponces;
    using Tools;

    public class CameraSettings : SettingsContainer, IIdItem
    {
        public CameraSettings(string id)
        {
            Id = id;
        }

        public NotifyProperty<bool> Autoconnect { get; } = true;

        public GeneralSettings GeneralSettings { get; set; }

        public string Id { get; }
    }
}