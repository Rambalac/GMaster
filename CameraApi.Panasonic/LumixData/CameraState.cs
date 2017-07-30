using System.Xml.Serialization;

namespace GMaster.Core.Camera.Panasonic.LumixData
{
    public enum SdCardStatus
    {
        Unknown = 0,

        [XmlEnum(Name = "write_protected")]
        WriteProtected = 2,

        [XmlEnum(Name = "write_enable")]
        WriteEnable = 1
    }

    public enum RemainDisplayType
    {
        Unknown = 0,

        [XmlEnum(Name = "time")]
        Time = 1,

        [XmlEnum(Name = "num")]
        Num = 2
    }

    public class CameraState
    {
        [XmlElement(ElementName = "add_location_data")]
        public OnOff AddLocationData { get; set; }

        [XmlElement(ElementName = "batt")]
        public string Battery { get; set; }

        [XmlElement(ElementName = "batt_grip")]
        public string GripBattery { get; set; }

        [XmlElement(ElementName = "burst_interval_status")]
        public string BurstIntervalStatus { get; set; }

        [XmlElement(ElementName = "cammode")]
        public string Cammode { get; set; }

        [XmlElement(ElementName = "interval_status")]
        public OnOff IntervalStatus { get; set; }

        [XmlElement(ElementName = "lens")]
        public string Lens { get; set; }

        [XmlElement(ElementName = "operate")]
        public string Operate { get; set; }

        [XmlElement(ElementName = "progress_time")]
        public int ProgressTime { get; set; }

        [XmlElement(ElementName = "rec")]
        public OnOff Rec { get; set; }

        [XmlElement(ElementName = "remaincapacity")]
        public int RemainCapacity { get; set; }

        [XmlElement(ElementName = "rem_disp_typ")]
        public RemainDisplayType RemainDisplayType { get; set; }

        [XmlElement(ElementName = "sd_access")]
        public OnOff SdAccess { get; set; }

        [XmlElement(ElementName = "sdcardstatus")]
        public SdCardStatus SdCardStatus { get; set; }

        [XmlElement(ElementName = "sd2_access")]
        public OnOff Sd2Access { get; set; }

        [XmlElement(ElementName = "sd2_cardstatus")]
        public SdCardStatus Sd2CardStatus { get; set; }

        [XmlElement(ElementName = "sdi_state")]
        public string SdiState { get; set; }

        [XmlElement(ElementName = "sd_memory")]
        public SdMemorySet SdMemory { get; set; }

        [XmlElement(ElementName = "sd2_memory")]
        public SdMemorySet Sd2Memory { get; set; }

        [XmlElement(ElementName = "stop_motion")]
        public OnOff StopMotion { get; set; }

        [XmlElement(ElementName = "stop_motion_num")]
        public int StopMotionNum { get; set; }

        [XmlElement(ElementName = "temperature")]
        public string Temperature { get; set; }

        [XmlElement(ElementName = "version")]
        public string Version { get; set; }

        [XmlElement(ElementName = "video_remaincapacity")]
        public int VideoRemainCapacity { get; set; }

        [XmlElement(ElementName = "warn_disp")]
        public string WarnDisp { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((CameraState)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Battery?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Cammode?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ RemainCapacity;
                hashCode = (hashCode * 397) ^ (int)SdCardStatus;
                hashCode = (hashCode * 397) ^ (int)SdMemory;
                hashCode = (hashCode * 397) ^ VideoRemainCapacity;
                hashCode = (hashCode * 397) ^ (int)Rec;
                hashCode = (hashCode * 397) ^ (BurstIntervalStatus?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)SdAccess;
                hashCode = (hashCode * 397) ^ (int)RemainDisplayType;
                hashCode = (hashCode * 397) ^ ProgressTime;
                hashCode = (hashCode * 397) ^ (Operate?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ StopMotionNum;
                hashCode = (hashCode * 397) ^ (int)StopMotion;
                hashCode = (hashCode * 397) ^ (Temperature?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Lens?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)AddLocationData;
                hashCode = (hashCode * 397) ^ (int)IntervalStatus;
                hashCode = (hashCode * 397) ^ (SdiState?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (WarnDisp?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Version?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        protected bool Equals(CameraState other)
        {
            return string.Equals(Battery, other.Battery) &&
                   string.Equals(Cammode, other.Cammode) &&
                   RemainCapacity == other.RemainCapacity &&
                   Equals(SdCardStatus, other.SdCardStatus) &&
                   Equals(SdMemory, other.SdMemory) &&
                   VideoRemainCapacity == other.VideoRemainCapacity && Rec == other.Rec &&
                   string.Equals(BurstIntervalStatus, other.BurstIntervalStatus) &&
                   Equals(SdAccess, other.SdAccess) &&
                   Equals(RemainDisplayType, other.RemainDisplayType) &&
                   ProgressTime == other.ProgressTime &&
                   string.Equals(Operate, other.Operate) &&
                   StopMotionNum == other.StopMotionNum &&
                   StopMotion == other.StopMotion &&
                   string.Equals(Temperature, other.Temperature) &&
                   Lens == other.Lens &&
                   AddLocationData == other.AddLocationData &&
                   Equals(IntervalStatus, other.IntervalStatus) &&
                   string.Equals(SdiState, other.SdiState) &&
                   string.Equals(WarnDisp, other.WarnDisp) &&
                   string.Equals(Version, other.Version);
        }
    }
}