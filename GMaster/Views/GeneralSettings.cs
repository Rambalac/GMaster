using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using Windows.Storage;
using GMaster.Annotations;
using GMaster.Camera.LumixResponces;
using Newtonsoft.Json;

namespace GMaster.Views
{
    public abstract class AbstractNotifyProperty : INotifyPropertyChanged
    {
        protected object value;

        public virtual void SetValue(object val)
        {
            if (Equals(val, value))
            {
                return;
            }

            value = val;
            OnPropertyChanged();
        }

        public object GetValue()
        {
            return value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NotifyProperty<TValue> : AbstractNotifyProperty
    {
        public TValue Value
        {
            get
            {
                return (TValue)value;
            }

            set
            {
                SetValue(value);
            }
        }

        public static implicit operator TValue(NotifyProperty<TValue> prop)
        {
            return prop.Value;
        }

        public static implicit operator NotifyProperty<TValue>(TValue val)
        {
            return new NotifyProperty<TValue> { Value = val };
        }

        public NotifyProperty()
        {
            value = default(TValue);
        }

        public override void SetValue(object val)
        {
            if (!(val is TValue))
            {
                throw new InvalidCastException($"Cannot cast {val.GetType()} into {typeof(TValue)}");
            }

            base.SetValue(val);
        }
    }

    public class GeneralSettings : SettingsContainer
    {
        public NotifyProperty<bool> Autoconnect { get; } = true;

        public ObservableHashCollection<CameraSettings> Cameras { get; private set; }

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

        private void GeneralSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Save();
        }

        public void Save()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var dict = new Dictionary<string, object>();
            Save(dict);
            localSettings.Values["settings"] = JsonConvert.SerializeObject(dict);
        }
    }

    public class CameraSettings : SettingsContainer, IIdItem
    {
        public NotifyProperty<bool> Autoconnect { get; } = true;

        public string Id { get; set; }

        public GeneralSettings GeneralSettings { get; set; }
    }
}