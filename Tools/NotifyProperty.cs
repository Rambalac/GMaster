namespace GMaster.Tools
{
    using System;

    public class NotifyProperty<TValue> : AbstractNotifyProperty
    {
        public NotifyProperty()
        {
            InnerValue = default(TValue);
        }

        public TValue Value
        {
            get => (TValue)InnerValue;

            set => SetValue(value);
        }

        public static implicit operator NotifyProperty<TValue>(TValue val)
        {
            return new NotifyProperty<TValue> { Value = val };
        }

        public static implicit operator TValue(NotifyProperty<TValue> prop)
        {
            return prop.Value;
        }

        public override void SetValue(object val)
        {
            if (val != null && !(val is TValue))
            {
                throw new InvalidCastException($"Cannot cast {val.GetType()} into {typeof(TValue)}");
            }

            base.SetValue(val);
        }
    }
}