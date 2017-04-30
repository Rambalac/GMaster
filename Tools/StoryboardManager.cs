using GMaster.Core.Tools;

namespace GMaster.Tools
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Media.Animation;

    public class StoryboardManager : DependencyObject
    {
        public static readonly DependencyProperty StoryboardProperty = DependencyProperty.Register(
            "Storyboard",
            typeof(Storyboard),
            typeof(StoryboardManager),
            new PropertyMetadata(default(Storyboard)));

        public static readonly DependencyProperty TriggerProperty = DependencyProperty.RegisterAttached("Trigger", typeof(bool), typeof(StoryboardManager), new PropertyMetadata(default(bool), TriggerChanged));

        public static Storyboard GetStoryboard(DependencyObject target)
        {
            return (Storyboard)target.GetValue(StoryboardProperty);
        }

        public static bool GetTrigger(DependencyObject element)
        {
            return (bool)element.GetValue(TriggerProperty);
        }

        public static void SetStoryboard(DependencyObject target, Storyboard value)
        {
            target.SetValue(StoryboardProperty, value);
            Storyboard.SetTarget(value, target);
        }

        public static void SetTrigger(DependencyObject element, bool value)
        {
            element.SetValue(TriggerProperty, value);
        }

        private static void TriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;

            var storyboard = GetStoryboard(d);
            if (value)
            {
                storyboard.Begin();
            }
            else
            {
                storyboard.Stop();
            }
        }
    }
}