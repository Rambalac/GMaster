namespace GMaster.Camera
{
    using System;
    using System.Linq;
    using LumixResponces;

    public class MenuSetParserGh4 : AbstractMenuSetParser
    {
        protected override bool InternalTryParse(MenuSet menuset, string lang)
        {
            DefaultLanguage = menuset.TitleList.Languages.Single(l => l.Default == YesNo.Yes).Titles;
            if (menuset.TitleList.Languages.TryGetValue(lang, out var cur))
            {
                CurrentLanguage = cur.Titles;
            }

            LiveviewQuality = ToMenuItems(menuset.MainMenu
                .Items["menu_item_id_liveview_settings"]
                .Items["menu_item_id_liveview_quality"]);

            var photosettings = menuset.Photosettings.Items;
            CreativeControls = ToMenuItems(photosettings["menu_item_id_crtv_ctrl"]);
            PhotoStyles = ToMenuItems(photosettings["menu_item_id_ph_sty"]);
            PhotoSizes = ToMenuItems(photosettings["menu_item_id_pctsiz"]);
            PhotoQuality = ToMenuItems(photosettings["menu_item_id_quality"]);
            MeteringMode = ToMenuItems(photosettings["menu_item_id_lightmet"]);
            VideoFormat = ToMenuItems(photosettings["menu_item_id_videoformat"]);
            VideoQuality = ToMenuItems(photosettings["menu_item_id_v_quality"]);
            FlashModes = ToMenuItems(photosettings["menu_item_id_flash"]);

            var qmenu = menuset.Qmenu2.Items;
            ShutterSpeeds = ToMenuItems(qmenu["menu_item_id_f_and_ss_angle"]);
            ExposureShifts = ToMenuItems(qmenu["menu_item_id_exposure2"]);
            AutofocusModes = ToMenuItems(qmenu["menu_item_id_afmode"]);
            CustomMultiModes = ToMenuItems(qmenu["menu_item_id_afmode"].Items["menu_item_id_afmode_custom_multi"]);
            IsoValues = ToMenuItems(qmenu["menu_item_id_sensitivity"]);
            DbValues = ToMenuItems(qmenu["menu_item_id_sensitivity_db"]);
            WhiteBalances = ToMenuItems(qmenu["menu_item_id_whitebalance"]);
            BurstModes = ToMenuItems(qmenu["menu_item_id_burst"]);
            PeakingModes = ToMenuItems(qmenu["menu_item_id_peaking"]);

            return true;
        }
    }
}