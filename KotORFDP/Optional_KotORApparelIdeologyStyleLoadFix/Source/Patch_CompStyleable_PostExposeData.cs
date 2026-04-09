using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace KotORFDP.Optional.KotORApparelIdeologyStyleLoadFix
{
    /// <summary>
    /// KotOR XML adds CompProperties_Styleable; Ideology also injects CompStyleable,
    /// so some items had two comps both saving sourcePrecept → duplicate XML and load errors.
    /// KotORFDP/1.6/Patches/KotORFDP_IdeologyStyleableDedup.xml removes the XML comp when Ideology is active.
    /// This patch keeps only the first <see cref="CompStyleable"/> per <see cref="ThingWithComps"/> for
    /// <c>guy762_*</c> defs during load/save so old saves and edge cases stay clean.
    /// </summary>
    [HarmonyPatch(typeof(CompStyleable), nameof(ThingComp.PostExposeData))]
    public static class Patch_CompStyleable_PostExposeData
    {
        static bool Prepare()
        {
            return ModsConfig.IsActive("ludeon.rimworld.ideology");
        }

        static bool Prefix(CompStyleable __instance)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars && Scribe.mode != LoadSaveMode.Saving)
            {
                return true;
            }

            var parent = __instance.parent as ThingWithComps;
            var defName = parent?.def?.defName;
            if (defName == null || !defName.StartsWith("guy762_", StringComparison.Ordinal))
            {
                return true;
            }

            var styleComps = StyleCompsFor(parent);
            if (styleComps.Count <= 1)
            {
                return true;
            }

            return styleComps[0] == __instance;
        }

        static List<CompStyleable> StyleCompsFor(ThingWithComps parent)
        {
            var list = new List<CompStyleable>();
            foreach (var c in parent.AllComps)
            {
                if (c is CompStyleable s)
                {
                    list.Add(s);
                }
            }

            return list;
        }
    }
}
