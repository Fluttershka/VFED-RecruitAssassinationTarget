using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFED_RecruitAssassinationTarget
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("Kuwah.VFED.RecruitAssassinationTarget");
            harmony.PatchAll();
            Log.Message("[VFED - Recruit As Assassination] Patches initialized successfully.");
        }
    }

    [HarmonyPatch(typeof(QuestUtility), nameof(QuestUtility.SendQuestTargetSignals), new[] { typeof(List<string>), typeof(string), typeof(SignalArgs) })]
    public static class Patch_QuestUtility_SendQuestTargetSignals
    {
        [HarmonyPrefix]
        public static bool Prefix(List<string> questTags, string signalPart, SignalArgs args)
        {
            if (signalPart == "LeftMap" && VFED_RecruitMod.Settings.preventPrematureLeftMap)
            {
                Pawn subject = null;
                if (args.TryGetArg("SUBJECT", out Pawn p1))
                {
                    subject = p1;
                }
                else if (args.Args != null)
                {
                    foreach (var arg in args.Args)
                    {
                        if (arg.arg is Pawn p2)
                        {
                            subject = p2;
                            break;
                        }
                    }
                }

                if (subject != null && DesertersRecruitHelper.IsAssassinationTargetInPlayerCustody(subject))
                {
                    // Suppress LeftMap signal while the target is captive or in player custody
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RecruitUtility), nameof(RecruitUtility.Recruit))]
    public static class Patch_RecruitUtility_Recruit
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Faction faction, Pawn recruiter)
        {
            if (VFED_RecruitMod.Settings.countRecruit && faction == Faction.OfPlayer)
            {
                DesertersRecruitHelper.Notify_PawnRecruitedAsColonist(pawn);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
    public static class Patch_Pawn_SetFaction
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, Faction newFaction)
        {
            if (VFED_RecruitMod.Settings.countRecruit && newFaction == Faction.OfPlayer && __instance.IsColonist && !__instance.IsSlave)
            {
                DesertersRecruitHelper.Notify_PawnRecruitedAsColonist(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(GenGuest), nameof(GenGuest.EnslavePrisoner))]
    public static class Patch_GenGuest_EnslavePrisoner
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn warden, Pawn prisoner)
        {
            if (VFED_RecruitMod.Settings.countEnslavement && prisoner != null)
            {
                DesertersRecruitHelper.Notify_PawnRecruitedAsColonist(prisoner);
            }
        }
    }
}
