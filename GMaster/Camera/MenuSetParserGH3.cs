using System.Collections.Generic;

namespace GMaster.Camera
{
    using System;
    using System.Linq;
    using LumixResponces;

    public class MenuSetParserGh3 : AbstractMenuSetParser
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

            var photosettings = menuset.MainMenu.Items["menu_item_id_photo_settings"].Items;
            CreativeControls = ToMenuItems(photosettings["menu_item_id_crtv_ctrl"]);
            AutofocusModes = ToMenuItems(photosettings["menu_item_id_afmode"]);
            PhotoStyles = ToMenuItems(photosettings["menu_item_id_ph_sty"]);
            FlashModes = ToMenuItems(photosettings["menu_item_id_flash"]);
            PhotoAspects = ToMenuItems(photosettings["menu_item_id_asprat"]);
            PhotoSizes = ToMenuItems(photosettings["menu_item_id_pctsiz"]);
            VideoQuality = ToMenuItems(photosettings["menu_item_id_v_quality"]);
            MeteringMode = ToMenuItems(photosettings["menu_item_id_lightmet"]);

            var qmenu = menuset.Qmenu.Items;
            ExposureShifts = ToMenuItems(qmenu["menu_item_id_exposure2"]);
            IsoValues = ToMenuItems(qmenu["menu_item_id_sensitivity"]);
            WhiteBalances = ToMenuItems(qmenu["menu_item_id_whitebalance"]);

            var driveMode = menuset.DriveMode.Items;
            SingleShootMode = ToMenuItem(driveMode["menu_item_id_1shoot"]);
            BurstModes = ToMenuItems(driveMode["menu_item_id_burst"]);

            VideoFormat = null;
            ShutterSpeeds = null;
            CustomMultiModes = null;
            DbValues = null;
            PeakingModes = null;

            return true;
        }
    }
}