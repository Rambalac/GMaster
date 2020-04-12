namespace CameraApi.Core
{
    public abstract class GeneralMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralMode"/> class.
        /// </summary>
        /// <param name="shortdesc">Short Description.</param>
        /// <param name="longdesc">Long Description.</param>
        public GeneralMode(string shortdesc, string longdesc)
        {
            ShortDescription = shortdesc;
            LongDescription = longdesc;
        }

        public string ShortDescription { get; }

        public string LongDescription { get; }

        public override bool Equals(object obj)
        {
            var mode = obj as GeneralMode;
            return mode != null &&
                   ShortDescription == mode.ShortDescription;
        }

        public override int GetHashCode()
        {
            return ShortDescription.GetHashCode();
        }
    }
}
