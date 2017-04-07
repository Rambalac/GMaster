namespace Tools
{
    public abstract class AbstractModelUser<TModel> : AbstractModelUserBase
    {
        public TModel Model
        {
            get => (TModel)GetValue(ModelProperty);
            set
            {
                SetValue(ModelProperty, value);
                ModelChanged(null, value);
            }
        }
    }
}