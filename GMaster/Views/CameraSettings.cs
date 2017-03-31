using GMaster.Camera.LumixResponces;

namespace GMaster.Views
{
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