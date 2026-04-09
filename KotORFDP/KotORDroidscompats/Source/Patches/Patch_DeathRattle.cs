using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace KotORFDP.KotORDroidsCompat.Patches
{
    /// <summary>Blocks Death Rattle failure hediffs from being applied to droids / automatons.</summary>
    [HarmonyPatch]
    public static class Patch_HediffSet_AddDirect_DeathRattle
    {
        static bool Prepare()
        {
            return TargetMethod() != null;
        }

        static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(HediffSet),
                nameof(HediffSet.AddDirect),
                new[]
                {
                    typeof(Hediff),
                    typeof(DamageInfo?),
                    typeof(DamageWorker.DamageResult)
                });
        }

        static bool Prefix(HediffSet __instance, Hediff hediff)
        {
            if (hediff?.def == null)
                return true;

            if (!DroidCompatDetector.DeathRattleHediffDefNames.Contains(hediff.def.defName))
                return true;

            var pawn = __instance?.pawn;
            if (pawn == null)
                return true;

            return !DroidCompatDetector.ShouldSilenceHumanlikeMods(pawn);
        }
    }

    /// <summary>Strips any existing Death Rattle hediffs from droids (e.g. after adding this module mid-save).</summary>
    [HarmonyPatch(typeof(Hediff), nameof(Hediff.TickInterval))]
    public static class Patch_Hediff_TickInterval_RemoveDeathRattle
    {
        static void Postfix(Hediff __instance, int delta)
        {
            if (delta <= 0)
                return;

            var defName = __instance?.def?.defName;
            if (defName == null || !DroidCompatDetector.DeathRattleHediffDefNames.Contains(defName))
                return;

            var pawn = __instance.pawn;
            if (pawn == null || !DroidCompatDetector.ShouldSilenceHumanlikeMods(pawn))
                return;

            pawn.health.RemoveHediff(__instance);
        }
    }
}
