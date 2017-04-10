namespace Tools
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Annotations;
    using Newtonsoft.Json.Linq;

    public class SettingsContainer : INotifyPropertyChanged
    {
        public SettingsContainer()
        {
            foreach (var prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (typeof(AbstractNotifyProperty).IsAssignableFrom(prop.PropertyType))
                {
                    var propvalue = GetOrCreate<INotifyPropertyChanged>(prop);

                    propvalue.PropertyChanged += (sender, args) => OnPropertyChanged(prop.Name);
                }
                else if (typeof(IObservableHashCollection).IsAssignableFrom(prop.PropertyType))
                {
                    var propvalue = GetOrCreate<INotifyCollectionChanged>(prop);

                    propvalue.CollectionChanged += (sender, args) => OnPropertyChanged(prop.Name);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Load(IDictionary<string, object> settings)
        {
            foreach (var prop in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var gentype = prop.PropertyType.GenericTypeArguments[0];
                if (typeof(AbstractNotifyProperty).IsAssignableFrom(prop.PropertyType))
                {
                    var propvalue = (AbstractNotifyProperty)prop.GetValue(this);
                    if (settings.TryGetValue(prop.Name, out var val))
                    {
                        if (typeof(Enum).IsAssignableFrom(gentype))
                        {
                            switch (val)
                            {
                                case int _:
                                    continue;
                                case long _:
                                    propvalue.SetValue(Enum.ToObject(gentype, val));
                                    break;

                                case string str:
                                    propvalue.SetValue(Enum.Parse(gentype, str));
                                    break;
                                default:
                                    throw new InvalidCastException($"Cannot cast {val.GetType()} into {gentype}");
                            }
                        }
                        else
                        {
                            switch (val)
                            {
                                case JObject jo:
                                    propvalue.SetValue(jo.ToObject(gentype));
                                    break;

                                case JArray ja:
                                    propvalue.SetValue(ja.ToObject(gentype));
                                    break;
                                default:
                                    propvalue.SetValue(val);
                                    break;
                            }
                        }
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
                            var cont = (SettingsContainer)Activator.CreateInstance(gentype, pair.Key);
                            if (pair.Value is JObject jObject)
                            {
                                cont.Load(jObject.ToObject<Dictionary<string, object>>());
                            }

                            var iditem = (IStringIdItem)cont;
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
                    var gentype = prop.PropertyType.GenericTypeArguments[0].GetTypeInfo();
                    if (!gentype.IsEnum)
                    {
                        settings[prop.Name] = propvalue.GetValue();
                    }
                    else
                    {
                        settings[prop.Name] = propvalue.GetValue().ToString();
                    }
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