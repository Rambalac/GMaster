namespace CameraApi.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Camera Focus mode
    /// </summary>
    public class FocusMode : GeneralMode
    {
        public FocusMode(string shortdesc, string longdesc)
            : base(shortdesc, longdesc)
        {
        }
    }
}
