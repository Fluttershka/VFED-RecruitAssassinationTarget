using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using VFED;

namespace VFED_RecruitAssassinationTarget
{
    public static class DesertersRecruitHelper
    {
        private const string PlotMissionDefName = "VFED_PlotMission";
        private static bool isProcessing = false;

        public static bool IsValidAssassinationPlot(WorldComponent_Deserters.PlotMissionInfo plot)
        {
            return plot != null
                && !plot.Complete
                && plot.quest != null
                && plot.quest.State == QuestState.Ongoing
                && plot.quest.root?.defName == PlotMissionDefName;
        }

        public static Pawn GetPlotTargetPawn(WorldComponent_Deserters.PlotMissionInfo plot)
        {
            if (plot == null) return null;
            if (plot.target != null) return plot.target;

            return plot.quest?.PartsListForReading
                ?.OfType<QuestPart_ApproachChoices>()
                .Select(ac => ac.noble)
                .FirstOrDefault(n => n != null);
        }

        public static WorldComponent_Deserters.PlotMissionInfo GetPlotForTarget(Pawn pawn)
        {
            if (pawn == null) return null;
            if (WorldComponent_Deserters.Instance == null || !WorldComponent_Deserters.Instance.Active) return null;

            var plots = WorldComponent_Deserters.Instance.PlotMissions;
            if (plots.NullOrEmpty()) return null;

            return plots.FirstOrDefault(p =>
                IsValidAssassinationPlot(p) &&
                (p.target == pawn ||
                 p.quest.PartsListForReading.OfType<QuestPart_ApproachChoices>().Any(ac => ac.noble == pawn))
            );
        }

        public static bool IsPawnInPlayerCustody(Pawn pawn)
        {
            if (pawn == null || pawn.Dead) return false;
            if (pawn.guest != null && pawn.guest.Released) return false;

            // Escaping via prison break or slave rebellion
            var lordJob = pawn.GetLord()?.LordJob;
            if (lordJob is LordJob_PrisonBreak || lordJob is LordJob_SlaveRebellion)
                return false;

            // Unsecure slave (e.g. rebelling or escaping)
            if (pawn.IsSlave && (pawn.guest == null || !pawn.guest.SlaveIsSecure))
                return false;

            // Mental states indicating flight or rebellion
            if (pawn.InMentalState && pawn.MentalStateDef != null)
            {
                if (pawn.MentalStateDef == MentalStateDefOf.PanicFlee ||
                    string.Equals(pawn.MentalStateDef.defName, "PanicFleeFire", StringComparison.Ordinal) ||
                    pawn.MentalState is MentalState_SlaveRebellion)
                {
                    return false;
                }
            }

            if (pawn.IsPrisonerOfColony) return true;
            if (pawn.HostFaction == Faction.OfPlayer) return true;
            if (pawn.Faction == Faction.OfPlayer) return true;
            if (pawn.CarriedBy != null && pawn.CarriedBy.Faction == Faction.OfPlayer) return true;

            var caravan = pawn.GetCaravan();
            if (caravan != null && caravan.Faction == Faction.OfPlayer) return true;

            return false;
        }

        public static void Notify_TargetNeutralizedByColony(Pawn pawn, bool viaEnslavement = false)
        {
            if (pawn == null || isProcessing) return;
            var plot = GetPlotForTarget(pawn);
            if (plot == null) return;

            Quest quest = plot.quest;
            if (quest == null || quest.State != QuestState.Ongoing) return;

            try
            {
                isProcessing = true;

                // 1. Clean up stale QuestPart_Notify_PlayerRaidedSomeone whose site map was already closed
                //    so sending the success signal won't emit vanilla Log.Error("Cannot resolve map...").
                var staleRaidParts = quest.PartsListForReading
                    .OfType<QuestPart_Notify_PlayerRaidedSomeone>()
                    .Where(part => part.getRaidersFromMap == null
                                && part.getRaidersFromMapParent != null
                                && part.getRaidersFromMapParent.Map == null)
                    .ToList();

                foreach (var stalePart in staleRaidParts)
                {
                    quest.RemovePart(stalePart);
                }

                // 2. Resolve and send ONLY this plot quest's success signal (do not broadcast to unrelated quests on the pawn).
                string successSignal = quest.PartsListForReading
                    .OfType<QuestPart_QuestEnd>()
                    .FirstOrDefault(part => part.outcome == QuestEndOutcome.Success && !part.inSignal.NullOrEmpty())
                    ?.inSignal;

                if (successSignal.NullOrEmpty())
                {
                    successSignal = "Quest" + quest.id + ".noble.Killed";
                }

                Find.SignalManager.SendSignal(new Signal(successSignal, pawn.Named("SUBJECT")));

                // 3. Fallback: if quest hasn't ended yet, end with success
                if (quest.State == QuestState.Ongoing)
                {
                    quest.End(QuestEndOutcome.Success, sendLetter: true);
                }

                // 4. Send notification letter if enabled
                if (VFED_RecruitMod.SendNotificationLetter)
                {
                    Faction deserters = Find.FactionManager?.AllFactions?.FirstOrDefault(f => f.def?.defName == "VFEE_Deserters");
                    string titleStr = plot.royalTitle != null
                        ? (string)plot.royalTitle.LabelCap
                        : (string)"VFED_DefaultNobleTitle".Translate();

                    string titleKey = viaEnslavement ? "VFED_TargetEnslavedLetterTitle" : "VFED_TargetRecruitedLetterTitle";
                    string textKey = viaEnslavement ? "VFED_TargetEnslavedLetterText" : "VFED_TargetRecruitedLetterText";

                    Find.LetterStack.ReceiveLetter(
                        titleKey.Translate(pawn.Named("PAWN")),
                        textKey.Translate(pawn.Named("PAWN"), titleStr.Named("TITLE")),
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
