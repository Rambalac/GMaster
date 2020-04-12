namespace GMaster.Core.Camera.Panasonic
{
    using System.Collections.Generic;
    using System.Linq;
    using LumixData;

    public class GH3Parser : CameraParser
    {
        public override IReadOnlyDictionary<int, string> IsoBinary { get; } = new Dictionary<int, string>
        {
            { 7167, "25600" },
            { 6911, "20000" },
            { 6655, "16000" },
            { 6399, "12800" },
            { 5887, "10000" },
            { 5375, "8000" },
            { 5119, "6400" },
            { 4863, "5000" },
            { 4607, "4000" },
            { 4351, "3200" },
            { 4095, "2500" },
            { 3839, "2000" },
            { 3583, "1600" },
            { 3327, "1250" },
            { 3071, "1000" },
            { 2815, "800" },
            { 2559, "640" },
            { 2303, "500" },
            { 2047, "400" },
            { 1791, "320" },
            { 1535, "250" },
            { 1279, "200" },
            { 1023, "160" },
            { 767,  "125" },
            { -1, "auto" }
        };

        public override CurMenu ParseCurMenu(MenuInfo info)
        {
            var result = new CurMenu();

            foreach (var item in info.MainMenu.Concat(info.Photosettings).Concat(info.Qmenu))
            {
                result.Enabled[item.Id] = item.Enable == YesNo.Yes;
            }

            return result;
        }

        protected override bool InnerParseMenuSet(MenuSet result, RawMenuSet menuset, string lang)
        {
            DefaultLanguage = menuset.TitleList.Languages.Single(l => l.Default == YesNo.Yes).Titles;
            CurrentLanguage = menuset.TitleList.Languages.TryGetValue(lang, out var cur) ? cur.Titles : null;
            var photosettings = menuset.MainMenu.Items["menu_item_id_photo_settings"].Items;
            var qmenu = menuset.Qmenu.Items;
            var driveMode = menuset.DriveMode.Items;

            result.LiveviewQuality = ToMenuItems(menuset.MainMenu
                .Items["menu_item_id_liveview_settings"]
                .Items["menu_item_id_liveview_quality"]);

            result.CreativeControls = ToMenuItems(photosettings["menu_item_id_crtv_ctrl"]);
            result.AutofocusModes = ToMenuItems(photosettings["menu_item_id_afmode"]);
            result.PhotoStyles = ToMenuItems(photosettings["menu_item_id_ph_sty"]);
            result.FlashModes = ToMenuItems(photosettings["menu_item_id_flash"]);
            result.PhotoAspects = ToMenuItems(photosettings["menu_item_id_asprat"]);
            result.PhotoSizes = ToMenuItems(photosettings["menu_item_id_pctsiz"]);
            result.VideoQuality = ToMenuItems(photosettings["menu_item_id_v_quality"]);
            result.MeteringMode = ToMenuItems(photosettings["menu_item_id_lightmet"]);

            result.ExposureShifts = ToMenuItems(qmenu["menu_item_id_exposure2"]);
            result.IsoValues = ToMenuItems(qmenu["menu_item_id_sensitivity"]) ?? DefaultIsoValues;
            result.WhiteBalances = ToMenuItems(qmenu["menu_item_id_whitebalance"]);

            result.SingleShootMode = ToMenuItem(driveMode["menu_item_id_1shoot"]);
            result.BurstModes = ToMenuItems(driveMode["menu_item_id_burst"]) ?? ToMenuItems(photosettings["menu_item_id_burst"]);

            result.VideoFormat = null;

            result.Angles = null;
            result.CustomMultiModes = null;
            result.DbValues = null;
            result.PeakingModes = null;
            return true;
        }
    }
}