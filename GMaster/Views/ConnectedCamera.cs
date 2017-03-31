using System.ComponentModel;
using System.Runtime.CompilerServices;
using GMaster.Annotations;
using GMaster.Camera;

namespace GMaster.Views
{
    public class ConnectedCamera : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Lumix Camera { get; set; }

        public bool IsForgetButtonVisible => Settings.GeneralSettings.Cameras.Contains(Settings);

        public bool IsRememberButtonVisible => !Settings.GeneralSettings.Cameras.Contains(Settings);

        public MainPageModel Model { get; set; }

        public string Name => Camera.Device.FriendlyName;

        public CameraSettings Settings { get; set; }

        public string Udn => Camera.Udn;

        public void Add(CameraSettings modelSettings)
        {
            Settings.GeneralSettings.Cameras.Add(modelSettings);
            OnPropertyChanged(nameof(IsForgetButtonVisible));
            OnPropertyChanged(nameof(IsRememberButtonVisible));
        }

        public void Remove(CameraSettings modelSettings)
        {
            Settings.GeneralSettings.Cameras.Remove(modelSettings);
            OnPropertyChanged(nameof(IsForgetButtonVisible));
            OnPropertyChanged(nameof(IsRememberButtonVisible));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}