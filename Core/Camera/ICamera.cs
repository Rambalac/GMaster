namespace GMaster.Core.Camera
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Annotations;

    public delegate void LiveviewReceiver(ArraySegment<byte> segment);

    public enum UpdateStateFailReason
    {
        RequestFailed = 0,
        LumixException = 1,
        NotConnected = 2
    }

    public interface ICamera : IDisposable
    {
        event Action<ICamera, UpdateStateFailReason> StateUpdateFailed;

        string CameraHost { get; }

        DeviceInfo Device { get; }

        CameraState State { get; }

        Task<bool> Connect(int liveViewPort, string lang);

        void Disconnect();

        void ProcessUdpMessage(byte[] argsData);

        Task StartLiveview(LiveviewReceiver receiver);

        Task StopLiveview();
        Task CaptureStart();
        Task CaptureStop();
        Task ChangeFocus(ChangeDirection changeDirection);
        Task MfAssistZoom(PinchStage stop, FloatPoint floatPoint, float f);
        Task MfAssistMove(PinchStage stage, FloatPoint fp);
        Task FocusPointMove(FloatPoint fp);
        Task FocusPointResize(PinchStage stage, FloatPoint point, float extend);
    }

    public interface IFocusAreas
    {
        IReadOnlyList<IBox> Boxes { get; }
    }

    public enum PinchStage
    {
        Start = 0,
        Stop = 1,
        Continue = 2,
        Single = 3
    }

    public interface IBox
    {
        float Height { get; }
        BoxProps Props { get; }
        float Width { get; }
        float X1 { get; }
        float X2 { get; }
        float Y1 { get; }
        float Y2 { get; }

        int GetHashCode();
    }

    public struct BoxProps
    {
        public bool Failed { get; internal set; }

        public FocusAreaType Type { get; internal set; }

        public bool Equals(BoxProps other)
        {
            return Failed == other.Failed && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is BoxProps && Equals((BoxProps)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Failed.GetHashCode() * 397) ^ (int)Type;
            }
        }
    }

    public enum FocusAreaType
    {
        OneAreaSelected = 0x0001,
        FaceOther = 0xff01,
        MainFace = 0x0002,
        Eye = 0xff09,
        TrackUnlock = 0xff03,
        TrackLock = 0x0003,
        MfAssistSelection = 0x0005,
        MfAssistPinP = 0x0006,
        MfAssistFullscreen = 0x0007,
        MfAssistLimit = 0x0008,
        Box = 0xff02,
        Cross = 0xff04
    }
}