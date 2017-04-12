namespace GMaster.Camera
{
    using System.Collections.Generic;
    using System.Linq;
    using LumixData;

    public class GH4Parser : CameraParser
    {
        public override IReadOnlyDictionary<int, string> IsoBinary { get; } = new Dictionary<int, string>
        {
            { 3, "auto" },
            { 8963, "100" },
            { 515, "125" },
            { 771, "160" },
            { 1027, "200" },
            { 1283, "250" },
            { 1539, "320" },
            { 1795, "400" },
            { 2051, "500" },
            { 2307, "640" },
            { 2563, "800" },
            { 2819, "1000" },
            { 3075, "1250" },
            { 3331, "1600" },
            { 3587, "2000" },
            { 3843, "2500" },
            { 4099, "3200" },
            { 4355, "4000" },
            { 4611, "5000" },
            { 4867, "6400" },
            { 5123, "8000" },
            { 5635, "10000" },
            { 6147, "12800" },
            { 8195, "16000" },
            { 8451, "20000" },
            { 8707, "25600" }
        };

        protected override bool InnerParseMenuSet(MenuSet result, RawMenuSet menuset, string lang)
        {
            var photosettings = menuset?.Photosettings?.Items;
            if (photosettings == null)
            {
                return false;
            }

            var qmenu = menuset.Qmenu2.Items;
            DefaultLanguage = menuset.TitleList.Languages.Single(l => l.Default == YesNo.Yes).Titles;
            CurrentLanguage = menuset.TitleList.Languages.TryGetValue(lang, out var cur) ? cur.Titles : null;

            result.LiveviewQuality = ToMenuItems(menuset.MainMenu
                .Items["menu_item_id_liveview_settings"]
                .Items["menu_item_id_liveview_quality"]);

            result.CreativeControls = ToMenuItems(photosettings["menu_item_id_crtv_ctrl"]);
            result.PhotoStyles = ToMenuItems(photosettings["menu_item_id_ph_sty"]);
            result.PhotoSizes = ToMenuItems(photosettings["menu_item_id_pctsiz"]);
            result.PhotoQuality = ToMenuItems(photosettings["menu_item_id_quality"]);
            result.MeteringMode = ToMenuItems(photosettings["menu_item_id_lightmet"]);
            result.VideoFormat = ToMenuItems(photosettings["menu_item_id_videoformat"]);
            result.VideoQuality = ToMenuItems(photosettings["menu_item_id_v_quality"]);
            result.FlashModes = ToMenuItems(photosettings["menu_item_id_flash"]);

            result.Angles = ToMenuItems(qmenu["menu_item_id_f_and_ss_angle"]);
            result.ExposureShifts = ToMenuItems(qmenu["menu_item_id_exposure3"] ?? qmenu["menu_item_id_exposure2"]);
            result.AutofocusModes = ToMenuItems(qmenu["menu_item_id_afmode"]);
            result.CustomMultiModes = ToMenuItems(qmenu["menu_item_id_afmode"].Items["menu_item_id_afmode_custom_multi"]);
            result.IsoValues = ToMenuItems(qmenu["menu_item_id_sensitivity"]);
            result.DbValues = ToMenuItems(qmenu["menu_item_id_sensitivity_db"]);
            result.WhiteBalances = ToMenuItems(qmenu["menu_item_id_whitebalance"]);
            result.BurstModes = ToMenuItems(qmenu["menu_item_id_burst"]);
            result.PeakingModes = ToMenuItems(qmenu["menu_item_id_peaking"]);

            return true;
        }
    }
}