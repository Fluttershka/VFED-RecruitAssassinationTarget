using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using VFED;

namespace VFED_RecruitAssassinationTarget
{
    public static class DesertersRecruitHelper
    {
        public static WorldComponent_Deserters.PlotMissionInfo GetPlotForTarget(Pawn pawn)
        {
            if (pawn == null) return null;
            if (WorldComponent_Deserters.Instance == null || !WorldComponent_Deserters.Instance.Active) return null;

            var plots = WorldComponent_Deserters.Instance.PlotMissions;
            if (plots.NullOrEmpty()) return null;

            return plots.FirstOrDefault(p =>
                !p.Complete &&
                p.quest != null &&
                p.quest.State == QuestState.Ongoing &&
                (p.target == pawn || p.quest.PartsListForReading.OfType<QuestPart_ApproachChoices>().Any(ac => ac.noble == pawn))
            );
        }

        public static bool IsAssassinationTargetInPlayerCustody(Pawn pawn)
        {
            if (pawn == null || pawn.Dead) return false;
            if (pawn.guest != null && pawn.guest.Released) return false;

            if (pawn.InMentalState && pawn.MentalStateDef != null)
            {
                string stateName = pawn.MentalStateDef.defName;
                if (stateName.IndexOf("PrisonBreak", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    stateName.IndexOf("PanicFlee", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    stateName.IndexOf("Escaping", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }
            }

            var plot = GetPlotForTarget(pawn);
            if (plot == null) return false;

            if (pawn.IsPrisonerOfColony) return true;
            if (pawn.HostFaction == Faction.OfPlayer) return true;
            if (pawn.Faction == Faction.OfPlayer) return true;
            if (pawn.CarriedBy != null && pawn.CarriedBy.Faction == Faction.OfPlayer) return true;

            var caravan = pawn.GetCaravan();
            if (caravan != null && caravan.Faction == Faction.OfPlayer) return true;

            return false;
        }

        private static bool isProcessing = false;

        public static void Notify_PawnRecruitedAsColonist(Pawn pawn)
        {
            if (pawn == null || isProcessing) return;
            var plot = GetPlotForTarget(pawn);
            if (plot == null) return;

            Quest quest = plot.quest;
            if (quest == null || quest.State != QuestState.Ongoing) return;

            try
            {
                isProcessing = true;

                // 1. Send QuestTargetSignals if pawn has tags
                bool signalSent = false;
                if (pawn.questTags != null && pawn.questTags.Count > 0)
                {
                    string expectedTag = "Quest" + quest.id + ".noble";
                    if (pawn.questTags.Contains(expectedTag))
                    {
                        QuestUtility.SendQuestTargetSignals(pawn.questTags, "Killed", pawn.Named("SUBJECT"));
                        signalSent = true;
                    }
                }

                // 2. Direct signal Quest<id>.noble.Killed to guarantee completion
                if (!signalSent || quest.State == QuestState.Ongoing)
                {
                    string directSignal = "Quest" + quest.id + ".noble.Killed";
                    Find.SignalManager.SendSignal(new Signal(directSignal, pawn.Named("SUBJECT")));
                }

                // 3. Fallback: if quest hasn't ended yet, end with success
                if (quest.State == QuestState.Ongoing)
                {
                    quest.End(QuestEndOutcome.Success, true, true);
                }

                // 4. Send notification letter if enabled
                if (VFED_RecruitMod.Settings.sendNotificationLetter)
                {
                    Faction deserters = Find.FactionManager?.AllFactions?.FirstOrDefault(f => f.def?.defName == "VFEE_Deserters");
                    string titleStr = plot.royalTitle?.LabelCap ?? "ImperialNoble".Translate();
                    Find.LetterStack.ReceiveLetter(
                        "VFED_TargetRecruitedLetterTitle".Translate(pawn.Named("PAWN")),
                        "VFED_TargetRecruitedLetterText".Translate(pawn.Named("PAWN"), titleStr.Named("TITLE")),
                        LetterDefOf.PositiveEvent,
                        pawn,
                        deserters,
                        quest
                    );
                }
            }
            finally
            {
                isProcessing = false;
            }
        }
    }
}
