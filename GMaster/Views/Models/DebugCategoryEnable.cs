namespace GMaster.Views.Models
{
    using Tools;

    public class DebugCategoryEnable
    {
        public bool Enabled
        {
            get => Debug.Categories[Name];
            set => Debug.Categories[Name] = value;
        }

        public string Name { get; set; }
    }
}