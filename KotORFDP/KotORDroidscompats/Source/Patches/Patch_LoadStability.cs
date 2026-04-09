using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace KotORFDP.KotORDroidsCompat.Patches
{
    /// <summary>
    /// During cross-ref / post-load, ABF can call into <c>ActiveDirectives</c> before it is valid.
    /// Short-circuit stat offset/factor for detected droids so vanilla/VEF stat chains do not NRE.
    /// </summary>
    [HarmonyPatch]
    public static class Patch_ArtificialBeings_GetStatOffset_Guard
    {
        static bool Prepare() => AbfCompLoadGuard.GetStatOffset != null;

        static MethodBase TargetMethod() => AbfCompLoadGuard.GetStatOffset;

        static bool Prefix(object __instance, ref float __result)
        {
            if (Scribe.mode == LoadSaveMode.Inactive)
                return true;

            var pawn = AbfCompLoadGuard.GetParentPawn(__instance);
            if (pawn == null || !DroidCompatDetector.ShouldSilenceHumanlikeMods(pawn))
                return true;

            __result = 0f;
            return false;
        }

        static Exception Finalizer(object __instance, Exception __exception, ref float __result)
        {
            if (__exception is not NullReferenceException)
                return __exception;

            try
            {
                var pawn = AbfCompLoadGuard.GetParentPawn(__instance);
                if (pawn == null || !DroidCompatDetector.ShouldSilenceHumanlikeMods(pawn))
                    return __exception;

                __result = 0f;
                return null;
            }
            catch
            {
                return __exception;
            }
        }
    }

    [HarmonyPatch]
    public static class Patch_ArtificialBeings_GetStatFactor_Guard
    {
        static bool Prepare() => AbfCompLoadGuard.GetStatFactor != null;

        static MethodBase TargetMethod() => AbfCompLoadGuard.GetStatFactor;

        static bool Prefix(object __instance, ref float __result)
        {
            if (Scribe.mode == LoadSaveMode.Inactive)
                return true;

            var pawn = AbfCompLoadGuard.GetParentPawn(__instance);
            if (pawn == null || !DroidCompatDetector.ShouldSilenceHumanlikeMods(pawn))
                return true;

            __result = 1f;
            return false;
        }

        static Exception Finalizer(object __instance, Exception __exception, ref float __result)
        {
            if (__exception is not NullReferenceException)
                return __exception;

            try
            {
                var pawn = AbfCompLoadGuard.GetParentPawn(__instance);
                if (pawn == null || !DroidCompatDetector.ShouldSilenceHumanlikeMods(pawn))
                    return __exception;

                __result = 1f;
                return null;
            }
            catch
            {
                return __exception;
            }
        }
    }

    [HarmonyPatch]
    public static class Patch_ArtificialBeings_ActiveDirectives_Guard
    {
        static bool Prepare() => AbfCompLoadGuard.ActiveDirectivesGetter != null;

        static MethodBase TargetMethod() => AbfCompLoadGuard.ActiveDirectivesGetter;

        static Exception Finalizer(object __instance, Exception __exception, ref object __result)
        {
            if (__exception is not NullReferenceException)
                return __exception;

            if (Scribe.mode == LoadSaveMode.Inactive)
                return __exception;

            try
            {
                var pawn = AbfCompLoadGuard.GetParentPawn(__instance);
                if (pawn == null || !DroidCompatDetector.ShouldSilenceHumanlikeMods(pawn))
                    return __exception;

                __result = null;
                return null;
            }
            catch
            {
                return __exception;
            }
        }
    }

    /// <summary>
    /// Prevents MW2 from creating abilities while the game is in save mode.
    /// That "GetNextAbilityID during saving" warning can destabilize save/load IDs.
    /// </summary>
    [HarmonyPatch]
    public static class Patch_MW2_AbilityForReading_NoCreateDuringSave
    {
        private static MethodBase _target;
        private static FieldInfo _abilityField;

        static bool Prepare()
        {
            var t = AccessTools.TypeByName("ModularWeapons2.CompModularWeapon");
            if (t == null)
                return false;

            _target = AccessTools.PropertyGetter(t, "AbilityForReading");
            _abilityField = AccessTools.Field(t, "ability");
            return _target != null;
        }

        static MethodBase TargetMethod() => _target;

        static bool Prefix(object __instance, ref RimWorld.Ability __result)
        {
            if (Scribe.mode != LoadSaveMode.Saving)
                return true;

            if (_abilityField == null)
            {
                __result = null;
                return false;
            }

            __result = _abilityField.GetValue(__instance) as RimWorld.Ability;
            return false;
        }
    }

    /// <summary>
    /// Avoids duplicate ILoadReferenceable registration for KotOR droid weapon verbs.
    /// This keeps cross-ref resolution from exploding on save/reload when duplicate IDs appear.
    /// </summary>
    [HarmonyPatch(typeof(LoadedObjectDirectory), nameof(LoadedObjectDirectory.RegisterLoaded))]
    public static class Patch_LoadedObjectDirectory_RegisterLoaded_DroidVerbDedup
    {
        private static readonly FieldInfo LoadedObjectsField =
            AccessTools.Field(typeof(LoadedObjectDirectory), "loadedObjects")
            ?? AccessTools.Field(typeof(LoadedObjectDirectory), "allObjectsByLoadID")
            ?? AccessTools.Field(typeof(LoadedObjectDirectory), "objectsByLoadID");

        static bool Prefix(LoadedObjectDirectory __instance, ILoadReferenceable reffable)
        {
            var id = reffable?.GetUniqueLoadID();
            if (string.IsNullOrEmpty(id))
                return true;

            if (!LooksLikeKotorDroidVerbId(id))
                return true;

            // Do not call ObjectWithLoadID here (it logs warnings for missing IDs).
            if (!HasLoadedObjectIdSilently(__instance, id))
                return true;

            // Keep first registration; skip duplicate.
            return false;
        }

        private static bool HasLoadedObjectIdSilently(LoadedObjectDirectory directory, string id)
        {
            if (directory == null || string.IsNullOrEmpty(id) || LoadedObjectsField == null)
                return false;

            var value = LoadedObjectsField.GetValue(directory);
            if (value is IDictionary dict)
                return dict.Contains(id);

            return false;
        }

        private static bool LooksLikeKotorDroidVerbId(string id)
        {
            if (id.IndexOf("guy762_DroidWeapon_", StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return id.StartsWith("Verb_Thing_", StringComparison.Ordinal)
                || id.StartsWith("Thing_", StringComparison.Ordinal);
        }
    }
}
