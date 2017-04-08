namespace GMaster
{
    using Tools;

    public class DebugCategoryEnable
    {
        public string Name { get; set; }

        public bool Enabled
        {
            get => Debug.Categories[Name];
            set => Debug.Categories[Name] = value;
        }
    }
}