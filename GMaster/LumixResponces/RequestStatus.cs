namespace LumixMaster.LumixResponces
{
    using System;
    using System.Xml.Serialization;
    using System.Collections.Generic;
    [XmlRoot(ElementName = "camrply")]
    public class BaseRequestResult
    {
        [XmlElement(ElementName = "result")]
        public string Result { get; set; }
    }

    public enum OnOff
    {
        [XmlEnum(Name = "on")]
        On,

        [XmlEnum(Name = "off")]
        Off
    }

    [XmlRoot(ElementName = "state")]
    public class CameraState
    {
        [XmlElement(ElementName = "batt")]
        public string Batt { get; set; }
        [XmlElement(ElementName = "cammode")]
        public string Cammode { get; set; }
        [XmlElement(ElementName = "remaincapacity")]
        public int Remaincapacity { get; set; }
        [XmlElement(ElementName = "sdcardstatus")]
        public string Sdcardstatus { get; set; }
        [XmlElement(ElementName = "sd_memory")]
        public string SdMemory { get; set; }
        [XmlElement(ElementName = "video_remaincapacity")]
        public int VideoRemaincapacity { get; set; }
        [XmlElement(ElementName = "rec")]
        public OnOff Rec { get; set; }
        [XmlElement(ElementName = "burst_interval_status")]
        public string BurstIntervalStatus { get; set; }
        [XmlElement(ElementName = "sd_access")]
        public string SdAccess { get; set; }
        [XmlElement(ElementName = "rem_disp_typ")]
        public string RemDispTyp { get; set; }
        [XmlElement(ElementName = "progress_time")]
        public int ProgressTime { get; set; }
        [XmlElement(ElementName = "operate")]
        public string Operate { get; set; }
        [XmlElement(ElementName = "stop_motion_num")]
        public int StopMotionNum { get; set; }
        [XmlElement(ElementName = "stop_motion")]
        public OnOff StopMotion { get; set; }
        [XmlElement(ElementName = "temperature")]
        public string Temperature { get; set; }
        [XmlElement(ElementName = "lens")]
        public string Lens { get; set; }
        [XmlElement(ElementName = "add_location_data")]
        public OnOff AddLocationData { get; set; }
        [XmlElement(ElementName = "interval_status")]
        public OnOff IntervalStatus { get; set; }
        [XmlElement(ElementName = "sdi_state")]
        public string SdiState { get; set; }
        [XmlElement(ElementName = "warn_disp")]
        public string WarnDisp { get; set; }
        [XmlElement(ElementName = "version")]
        public string Version { get; set; }

        protected bool Equals(CameraState other)
        {
            return string.Equals(Batt, other.Batt) &&
                string.Equals(Cammode, other.Cammode) &&
                Remaincapacity == other.Remaincapacity &&
                string.Equals(Sdcardstatus, other.Sdcardstatus) &&
                string.Equals(SdMemory, other.SdMemory) &&
                VideoRemaincapacity == other.VideoRemaincapacity && Rec == other.Rec &&
                string.Equals(BurstIntervalStatus, other.BurstIntervalStatus) &&
                string.Equals(SdAccess, other.SdAccess) &&
                string.Equals(RemDispTyp, other.RemDispTyp) &&
                ProgressTime == other.ProgressTime &&
                string.Equals(Operate, other.Operate) &&
                StopMotionNum == other.StopMotionNum &&
                StopMotion == other.StopMotion &&
                string.Equals(Temperature, other.Temperature) &&
                Lens == other.Lens &&
                AddLocationData == other.AddLocationData &&
                string.Equals(IntervalStatus, other.IntervalStatus) &&
                string.Equals(SdiState, other.SdiState) &&
                string.Equals(WarnDisp, other.WarnDisp) &&
                string.Equals(Version, other.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CameraState)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Batt?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Cammode?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Remaincapacity;
                hashCode = (hashCode * 397) ^ (Sdcardstatus?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (SdMemory?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ VideoRemaincapacity;
                hashCode = (hashCode * 397) ^ (int)Rec;
                hashCode = (hashCode * 397) ^ (BurstIntervalStatus?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (SdAccess?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (RemDispTyp?.GetHashCode() ?? 0);
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
    }

    [XmlRoot(ElementName = "camrply")]
    public class CameraStateRequestResult : BaseRequestResult
    {
        [XmlElement(ElementName = "state")]
        public CameraState State { get; set; }
    }



}
