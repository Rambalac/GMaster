namespace GMaster.Camera
{
    using System.Linq;
    using LumixResponces;

    public class GH4Parser : AbstractMenuSetParser
    {
        public override MenuSet ParseMenuSet(RawMenuSet menuset, string lang)
        {
            var photosettings = menuset.Photosettings.Items;
            var qmenu = menuset.Qmenu2.Items;
            DefaultLanguage = menuset.TitleList.Languages.Single(l => l.Default == YesNo.Yes).Titles;
            CurrentLanguage = menuset.TitleList.Languages.TryGetValue(lang, out var cur) ? cur.Titles : null;

            return new MenuSet
            {
                LiveviewQuality = ToMenuItems(menuset.MainMenu
                    .Items["menu_item_id_liveview_settings"]
                    .Items["menu_item_id_liveview_quality"]),

                CreativeControls = ToMenuItems(photosettings["menu_item_id_crtv_ctrl"]),
                PhotoStyles = ToMenuItems(photosettings["menu_item_id_ph_sty"]),
                PhotoSizes = ToMenuItems(photosettings["menu_item_id_pctsiz"]),
                PhotoQuality = ToMenuItems(photosettings["menu_item_id_quality"]),
                MeteringMode = ToMenuItems(photosettings["menu_item_id_lightmet"]),
                VideoFormat = ToMenuItems(photosettings["menu_item_id_videoformat"]),
                VideoQuality = ToMenuItems(photosettings["menu_item_id_v_quality"]),
                FlashModes = ToMenuItems(photosettings["menu_item_id_flash"]),

                Angles = ToMenuItems(qmenu["menu_item_id_f_and_ss_angle"]),
                ExposureShifts = ToMenuItems(qmenu["menu_item_id_exposure2"]),
                AutofocusModes = ToMenuItems(qmenu["menu_item_id_afmode"]),
                CustomMultiModes = ToMenuItems(qmenu["menu_item_id_afmode"].Items["menu_item_id_afmode_custom_multi"]),
                IsoValues = ToMenuItems(qmenu["menu_item_id_sensitivity"]),
                DbValues = ToMenuItems(qmenu["menu_item_id_sensitivity_db"]),
                WhiteBalances = ToMenuItems(qmenu["menu_item_id_whitebalance"]),
                BurstModes = ToMenuItems(qmenu["menu_item_id_burst"]),
                PeakingModes = ToMenuItems(qmenu["menu_item_id_peaking"])
            };
        }
    }
}