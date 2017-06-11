using GMaster.Core.Camera.Panasonic;

namespace GMaster.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Annotations;
    using Core.Camera;

    public class ConnectedCamera : INotifyPropertyChanged
    {
        private Lumix camera;
        private bool isAspectAnamorphingVideoOnly;
        private string selectedAspect;
        private LutInfo selectedLut;

        public event PropertyChangedEventHandler PropertyChanged;

        public event Action Removed;

        public string[] Aspects => new[] { "1", "1.33", "1.5", "1.75", "2" };

        public Lumix Camera
        {
            get => camera;
            set
            {
                if (ReferenceEquals(camera, value))
                {
                    return;
                }

                camera = value;
                camera.LumixState.PropertyChanged += LumixState_PropertyChanged2;

                OnPropertyChanged(nameof(Camera));
            }
        }

        public DeviceInfo Device { get; set; }

        public IEnumerable<LutInfo> InstalledLuts => new[] { new LutInfo { Id = null, Title = string.Empty } }.Concat(Model.InstalledLuts);

        public bool IsAspectAnamorphingVideoOnly
        {
            get => isAspectAnamorphingVideoOnly;
            set
            {
                isAspectAnamorphingVideoOnly = value;
                Settings.IsAspectAnamorphingVideoOnly.Value = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy => Camera.LumixState.IsBusy;

        public bool IsRemembered => Settings.GeneralSettings.Cameras.Contains(Settings);

        public MainPageModel Model { get; set; }

        public string Name { get; set; }

        public string SelectedAspect
        {
            get => selectedAspect;
            set
            {
                selectedAspect = value;
                Settings.Aspect.Value = value;
                OnPropertyChanged();
            }
        }

        public LutInfo SelectedLut
        {
            get => selectedLut;

            set
            {
                selectedLut = value;
                Settings.LutId.Value = value?.Id;
                OnPropertyChanged();
            }
        }

        public CameraSettings Settings { get; set; }

        public string Uuid => Device.Uuid;

        public void AddToSettings()
        {
            Settings.GeneralSettings.Cameras.Add(Settings);
            OnPropertyChanged(nameof(IsRemembered));
        }

        public void MakeRemoved()
        {
            Removed?.Invoke();
        }

        public void RemoveFromSettings()
        {
            Settings.GeneralSettings.Cameras.Remove(Settings);
            OnPropertyChanged(nameof(IsRemembered));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LumixState_PropertyChanged2(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LumixState.IsBusy))
            {
                var task = Model.RunAsync(() => OnPropertyChanged(nameof(IsBusy)));
            }
        }
    }
}