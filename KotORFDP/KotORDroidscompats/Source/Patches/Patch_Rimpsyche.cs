using System.Reflection;
using HarmonyLib;
using Verse;

namespace KotORFDP.KotORDroidsCompat.Patches
{
    /// <summary>Prevents RimPsyche conversation memory from running when a droid is involved.</summary>
    [HarmonyPatch]
    public static class Patch_Rimpsyche_GainConversationMemoryFast
    {
        private static MethodBase _method;

        static bool Prepare()
        {
            var t = AccessTools.TypeByName("Maux36.RimPsyche.Rimpsyche_Utility");
            if (t == null)
                return false;

            _method = AccessTools.Method(
                t,
                "GainCoversationMemoryFast",
                new[] { typeof(string), typeof(string), typeof(float), typeof(Pawn), typeof(Pawn) });

            return _method != null;
        }

        static MethodBase TargetMethod() => _method;

        static bool Prefix(Pawn parentPawn, Pawn otherPawn)
        {
            if (parentPawn != null && DroidCompatDetector.ShouldSilenceHumanlikeMods(parentPawn))
                return false;

            if (otherPawn != null && DroidCompatDetector.ShouldSilenceHumanlikeMods(otherPawn))
                return false;

            return true;
        }
    }

    /// <summary>Skips RimPsyche start-conversation interactions when either pawn is a droid.</summary>
    [HarmonyPatch]
    public static class Patch_Rimpsyche_InteractionWorker_StartConversation
    {
        private static MethodBase _method;

        static bool Prepare()
        {
            var t = AccessTools.TypeByName("Maux36.RimPsyche.InteractionWorker_StartConversation");
            if (t == null)
                return false;

            _method = AccessTools.Method(t, "Interacted");
            return _method != null;
        }

        static MethodBase TargetMethod() => _method;

        static bool Prefix(Pawn initiator, Pawn recipient)
        {
            if (initiator != null && DroidCompatDetector.ShouldSilenceHumanlikeMods(initiator))
                return false;

            if (recipient != null && DroidCompatDetector.ShouldSilenceHumanlikeMods(recipient))
                return false;

            return true;
        }
    }
}
