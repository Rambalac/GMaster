namespace GMaster.Camera
{
    using System.Collections.Generic;
    using System.Linq;
    using LumixResponces;

    public class GH3Parser : AbstractMenuSetParser
    {
        public override MenuSet ParseMenuSet(RawMenuSet menuset, string lang)
        {
            DefaultLanguage = menuset.TitleList.Languages.Single(l => l.Default == YesNo.Yes).Titles;
            CurrentLanguage = menuset.TitleList.Languages.TryGetValue(lang, out var cur) ? cur.Titles : null;
            var photosettings = menuset.MainMenu.Items["menu_item_id_photo_settings"].Items;
            var qmenu = menuset.Qmenu.Items;
            var driveMode = menuset.DriveMode.Items;

            return new MenuSet
            {
                LiveviewQuality = ToMenuItems(menuset.MainMenu
                    .Items["menu_item_id_liveview_settings"]
                    .Items["menu_item_id_liveview_quality"]),

                CreativeControls = ToMenuItems(photosettings["menu_item_id_crtv_ctrl"]),
                AutofocusModes = ToMenuItems(photosettings["menu_item_id_afmode"]),
                PhotoStyles = ToMenuItems(photosettings["menu_item_id_ph_sty"]),
                FlashModes = ToMenuItems(photosettings["menu_item_id_flash"]),
                PhotoAspects = ToMenuItems(photosettings["menu_item_id_asprat"]),
                PhotoSizes = ToMenuItems(photosettings["menu_item_id_pctsiz"]),
                VideoQuality = ToMenuItems(photosettings["menu_item_id_v_quality"]),
                MeteringMode = ToMenuItems(photosettings["menu_item_id_lightmet"]),

                ExposureShifts = ToMenuItems(qmenu["menu_item_id_exposure2"]),
                IsoValues = ToMenuItems(qmenu["menu_item_id_sensitivity"]),
                WhiteBalances = ToMenuItems(qmenu["menu_item_id_whitebalance"]),

                SingleShootMode = ToMenuItem(driveMode["menu_item_id_1shoot"]),
                BurstModes = ToMenuItems(driveMode["menu_item_id_burst"]),

                VideoFormat = null,
                
                Angles = null,
                CustomMultiModes = null,
                DbValues = null,
                PeakingModes = null
            };
        }
    }
}