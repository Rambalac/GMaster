namespace GMaster.Views.Models
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Annotations;
    using Camera;
    using Windows.Foundation;

    public class FocusArea : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public double Height { get; private set; }

        public double Width { get; private set; }

        public double Left { get; private set; }

        public double Top { get; private set; }

        public bool Visible { get; private set; }

        public void Hide()
        {
            var old = Visible;
            Visible = false;
            if (old != Visible)
            {
                OnPropertyChanged(nameof(Visible));
            }
        }

        public void Update(FocusAreas.Box box, Rect image)
        {
            Visible = true;
            Left = image.Left + (box.X1 * image.Width);
            Top = image.Top + (box.Y1 * image.Height);
            Width = (box.X2 - box.X1) * image.Width;
            Height = (box.Y2 - box.Y1) * image.Height;

            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Left));
            OnPropertyChanged(nameof(Top));
            OnPropertyChanged(nameof(Visible));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}