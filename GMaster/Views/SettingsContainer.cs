using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using GMaster.Annotations;
using GMaster.Camera.LumixResponces;
using Newtonsoft.Json.Linq;

namespace GMaster.Views
{
    public class SettingsContainer : INotifyPropertyChanged
    {
        public SettingsContainer()
        {
            foreach (var prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (typeof(AbstractNotifyProperty).IsAssignableFrom(prop.PropertyType) ||
                    typeof(IObservableHashCollection).IsAssignableFrom(prop.PropertyType))
                {
                    var propvalue = GetOrCreate<INotifyPropertyChanged>(prop);

                    propvalue.PropertyChanged += (sender, args) => OnPropertyChanged(prop.Name);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Load(IDictionary<string, object> settings)
        {
            foreach (var prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (typeof(AbstractNotifyProperty).IsAssignableFrom(prop.PropertyType))
                {
                    var propvalue = (AbstractNotifyProperty)prop.GetValue(this);
                    if (settings.TryGetValue(prop.Name, out var val))
                    {
                        propvalue.SetValue(val);
                    }
                }
                else if (typeof(IObservableHashCollection).IsAssignableFrom(prop.PropertyType))
                {
                    if (settings.TryGetValue(prop.Name, out var itemsave) && itemsave is JObject jobj)
                    {
                        var col = (IObservableHashCollection)prop.GetValue(this);
                        var collection = jobj.ToObject<Dictionary<string, object>>();
                        foreach (var pair in collection)
                        {
                            var cont = (SettingsContainer)Activator.CreateInstance(prop.PropertyType.GenericTypeArguments[0], pair.Key);
                            if (pair.Value is JObject jObject)
                            {
                                cont.Load(jObject.ToObject<Dictionary<string, object>>());
                            }

                            var iditem = (IIdItem)cont;
                            col.Add(pair.Key, iditem);
                        }
                    }
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Save(IDictionary<string, object> settings)
        {
            foreach (var prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (typeof(AbstractNotifyProperty).IsAssignableFrom(prop.PropertyType))
                {
                    var propvalue = (AbstractNotifyProperty)prop.GetValue(this);
                    settings[prop.Name] = propvalue.GetValue();
                }
                else if (typeof(IObservableHashCollection).IsAssignableFrom(prop.PropertyType))
                {
                    var col = (IObservableHashCollection)prop.GetValue(this);
                    var collection = new Dictionary<string, object>();

                    foreach (var item in col.GetAll())
                    {
                        var itemsave = new Dictionary<string, object>();

                        var cont = (SettingsContainer)item;
                        cont.Save(itemsave);

                        collection[item.Id] = itemsave;
                    }

                    settings[prop.Name] = collection;
                }
            }
        }

        private T GetOrCreate<T>(PropertyInfo prop)
        {
            var propvalue = prop.GetValue(this);
            if (propvalue == null)
            {
                propvalue = Activator.CreateInstance(prop.PropertyType);
                prop.SetValue(this, propvalue);
            }

            return (T)propvalue;
        }
    }
}