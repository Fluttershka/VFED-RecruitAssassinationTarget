using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("[VFED - Recruit As Assassination] Patches initialized successfully.");
        }
    }

    [HarmonyPatch(typeof(QuestUtility), nameof(QuestUtility.SendQuestTargetSignals), new[] { typeof(List<string>), typeof(string), typeof(SignalArgs) })]
    public static class Patch_QuestUtility_SendQuestTargetSignals
    {
        [HarmonyPrefix]
        public static bool Prefix(ref List<string> questTags, string signalPart, SignalArgs args)
        {
            if (signalPart != "LeftMap" || questTags.NullOrEmpty() || !VFED_RecruitMod.PreventPrematureLeftMap)
            {
                return true;
            }

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

            if (subject == null) return true;

            var plot = DesertersRecruitHelper.GetPlotForTarget(subject);
            if (plot == null || !DesertersRecruitHelper.IsPawnInPlayerCustody(subject))
            {
                return true;
            }

            // Filter out only the tag(s) belonging to this VFED plot quest so other quests on the pawn are unaffected.
            string questPrefix = "Quest" + plot.quest.id + ".";
            if (!questTags.Any(t => t != null && t.StartsWith(questPrefix)))
            {
                return true;
            }

            var filteredTags = questTags.Where(t => t == null || !t.StartsWith(questPrefix)).ToList();
            if (filteredTags.Count == 0)
            {
                return false;
            }

            questTags = filteredTags;
            return true;
        }
    }

    [HarmonyPatch(typeof(RecruitUtility), nameof(RecruitUtility.Recruit), new[] { typeof(Pawn), typeof(Faction), typeof(Pawn) })]
    public static class Patch_RecruitUtility_Recruit
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Faction faction, Pawn recruiter)
        {
            if (VFED_RecruitMod.CountRecruit && faction == Faction.OfPlayer && pawn != null && pawn.IsFreeNonSlaveColonist)
            {
                DesertersRecruitHelper.Notify_TargetNeutralizedByColony(pawn, viaEnslavement: false);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction), new[] { typeof(Faction), typeof(Pawn) })]
    public static class Patch_Pawn_SetFaction
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, Faction newFaction)
        {
            // IsFreeNonSlaveColonist ensures HostFaction == null and !IsSlave,
            // preventing premature firing during Pawn_GuestTracker.SetGuestStatus(OfPlayer, GuestStatus.Slave).
            if (VFED_RecruitMod.CountRecruit && newFaction == Faction.OfPlayer && __instance != null && __instance.IsFreeNonSlaveColonist)
            {
                DesertersRecruitHelper.Notify_TargetNeutralizedByColony(__instance, viaEnslavement: false);
            }
        }
    }

    [HarmonyPatch(typeof(GenGuest), nameof(GenGuest.TryEnslavePrisoner), new[] { typeof(Pawn), typeof(Pawn) })]
    public static class Patch_GenGuest_TryEnslavePrisoner
    {
        [HarmonyPostfix]
        public static void Postfix(bool __result, Pawn warden, Pawn prisoner)
        {
            if (__result && VFED_RecruitMod.CountEnslavement && prisoner != null && prisoner.IsSlaveOfColony)
            {
                DesertersRecruitHelper.Notify_TargetNeutralizedByColony(prisoner, viaEnslavement: true);
            }
        }
    }
}
