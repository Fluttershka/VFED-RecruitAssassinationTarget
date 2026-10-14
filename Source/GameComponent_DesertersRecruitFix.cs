using System.Linq;
using RimWorld;
using Verse;
using VFED;

namespace VFED_RecruitAssassinationTarget
{
    public class GameComponent_DesertersRecruitFix : GameComponent
    {
        public GameComponent_DesertersRecruitFix(Game game)
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            // Check periodically (every ~2000 ticks = ~33 seconds)
            if (Find.TickManager.TicksGame % 2000 == 123)
            {
                CheckExistingColonistTargets();
            }
        }

        public static void CheckExistingColonistTargets()
        {
            if (!VFED_RecruitMod.Settings.countRecruit) return;
            if (WorldComponent_Deserters.Instance == null || !WorldComponent_Deserters.Instance.Active) return;

            var plots = WorldComponent_Deserters.Instance.PlotMissions;
            if (plots.NullOrEmpty()) return;

            foreach (var plot in plots)
            {
                if (!plot.Complete && plot.quest != null && plot.quest.State == QuestState.Ongoing && plot.target != null)
                {
                    bool isTargetRecruited = plot.target.Faction == Faction.OfPlayer &&
                                             (plot.target.IsColonist || (VFED_RecruitMod.Settings.countEnslavement && plot.target.IsSlaveOfColony));

                    if (isTargetRecruited)
                    {
                        DesertersRecruitHelper.Notify_PawnRecruitedAsColonist(plot.target);
                    }
                }
            }
        }
    }
}
