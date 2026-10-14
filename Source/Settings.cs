using UnityEngine;
using Verse;

namespace VFED_RecruitAssassinationTarget
{
    public class VFED_RecruitSettings : ModSettings
    {
        public bool countRecruit = true;
        public bool countEnslavement = false;
        public bool preventPrematureLeftMap = true;
        public bool sendNotificationLetter = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref countRecruit, "countRecruit", true);
            Scribe_Values.Look(ref countEnslavement, "countEnslavement", false);
            Scribe_Values.Look(ref preventPrematureLeftMap, "preventPrematureLeftMap", true);
            Scribe_Values.Look(ref sendNotificationLetter, "sendNotificationLetter", true);
        }
    }

    public class VFED_RecruitMod : Mod
    {
        public static VFED_RecruitSettings Settings;

        public VFED_RecruitMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<VFED_RecruitSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.CheckboxLabeled("VFED_Setting_CountRecruit".Translate(), ref Settings.countRecruit, "VFED_Setting_CountRecruit_Desc".Translate());
            listing.CheckboxLabeled("VFED_Setting_CountEnslavement".Translate(), ref Settings.countEnslavement, "VFED_Setting_CountEnslavement_Desc".Translate());
            listing.CheckboxLabeled("VFED_Setting_PreventLeftMap".Translate(), ref Settings.preventPrematureLeftMap, "VFED_Setting_PreventLeftMap_Desc".Translate());
            listing.CheckboxLabeled("VFED_Setting_SendLetter".Translate(), ref Settings.sendNotificationLetter, "VFED_Setting_SendLetter_Desc".Translate());

            listing.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "VFED_SettingsCategory".Translate();
    }
}
