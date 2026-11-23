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

        public override void LoadedGame()
        {
            base.LoadedGame();
            CheckExistingColonistTargets();
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
            bool checkRecruit = VFED_RecruitMod.CountRecruit;
            bool checkEnslave = VFED_RecruitMod.CountEnslavement;
            if (!checkRecruit && !checkEnslave) return;

            if (WorldComponent_Deserters.Instance == null || !WorldComponent_Deserters.Instance.Active) return;

            var plots = WorldComponent_Deserters.Instance.PlotMissions;
            if (plots.NullOrEmpty()) return;

            foreach (var plot in plots.ToList())
            {
                if (!DesertersRecruitHelper.IsValidAssassinationPlot(plot)) continue;

                Pawn target = DesertersRecruitHelper.GetPlotTargetPawn(plot);
                if (target == null || target.Dead) continue;

                if (checkRecruit && target.IsFreeNonSlaveColonist)
                {
                    DesertersRecruitHelper.Notify_TargetNeutralizedByColony(target, viaEnslavement: false);
                }
                else if (checkEnslave && target.IsSlaveOfColony)
                {
                    DesertersRecruitHelper.Notify_TargetNeutralizedByColony(target, viaEnslavement: true);
                }
            }
        }
    }
}
