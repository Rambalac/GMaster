namespace GMaster.Views.Models
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Annotations;
    using Core.Camera;
    using Core.Camera.Panasonic;
    using Windows.Foundation;

    public class FocusArea : INotifyPropertyChanged
    {
        private bool visible;

        public event PropertyChangedEventHandler PropertyChanged;

        public FocusAreas.Box.BoxProps Props { get; set; }

        public Rect Rect { get; set; }

        public bool Visible
        {
            get => visible;
            set
            {
                if (value == visible)
                {
                    return;
                }

                visible = value;
                OnPropertyChanged();
            }
        }

        public void Hide()
        {
            Visible = false;
        }

        public void Update(FocusAreas.Box box, Rect image)
        {
            Visible = true;
            Rect = new Rect(
                image.Left + (box.X1 * image.Width),
                image.Top + (box.Y1 * image.Height),
                (box.X2 - box.X1) * image.Width,
                (box.Y2 - box.Y1) * image.Height);

            OnPropertyChanged(nameof(Rect));

            if (!Equals(Props, box.Props))
            {
                Props = box.Props;
                OnPropertyChanged(nameof(Props));
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}