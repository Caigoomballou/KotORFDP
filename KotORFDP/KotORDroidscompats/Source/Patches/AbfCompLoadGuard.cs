using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace KotORFDP.KotORDroidsCompat.Patches
{
    /// <summary>
    /// Shared reflection for Artificial Beings <c>CompArtificialPawn</c> load/save guards.
    /// </summary>
    internal static class AbfCompLoadGuard
    {
        internal static readonly MethodBase GetStatOffset;
        internal static readonly MethodBase GetStatFactor;
        internal static readonly MethodBase ActiveDirectivesGetter;

        private static readonly PropertyInfo ParentProperty;
        private static readonly FieldInfo ParentField;

        static AbfCompLoadGuard()
        {
            var t = AccessTools.TypeByName("ArtificialBeings.CompArtificialPawn");
            if (t == null)
                return;

            GetStatOffset = AccessTools.Method(t, "GetStatOffset", new[] { typeof(StatDef) });
            GetStatFactor = AccessTools.Method(t, "GetStatFactor", new[] { typeof(StatDef) });
            ActiveDirectivesGetter = AccessTools.PropertyGetter(t, "ActiveDirectives");
            ParentProperty = AccessTools.Property(t, "parent");
            ParentField = AccessTools.Field(t, "parent") ?? AccessTools.Field(typeof(ThingComp), "parent");
        }

        internal static Pawn GetParentPawn(object instance)
        {
            if (instance == null)
                return null;

            var viaProp = ParentProperty?.GetValue(instance, null);
            if (viaProp is Pawn pawnFromProp)
                return pawnFromProp;

            var viaField = ParentField?.GetValue(instance);
            if (viaField is Pawn pawnFromField)
                return pawnFromField;

            return null;
        }
    }
}
