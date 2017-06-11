using System;
using System.Collections.Generic;
using System.Text;

namespace GMaster.Core.Camera
{
    enum ImageBufferType
    {
        Jpeg
    }

    delegate void LiveviewReceiver(byte[] buffer, ImageBufferType type);

    interface ICamera
    {
        void StartLiveview(LiveviewReceiver receiver);
    }
}
