using HarmonyLib;
using RimWorld;
using Verse;

namespace KotORFDP.KotORDroidsCompat.Optional.NiceHealthTab
{
    internal static class NiceHealthTabOptionalHooks
    {
        internal static Pawn GetPawnFromHealthTab(ITab_Pawn_Health tab)
        {
            if (tab == null)
                return null;

            var t = Traverse.Create(tab);
            var pawn = t.Property("SelPawn")?.GetValue<Pawn>();
            if (pawn != null)
                return pawn;

            pawn = t.Field("SelPawn")?.GetValue<Pawn>();
            return pawn;
        }
    }
}
