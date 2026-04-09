using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KotORFDP.KotORDroidsCompat
{
    /// <summary>
    /// Detects KotOR droids and ABF synstruct-backed droids without hard assembly references.
    /// </summary>
    public static class DroidCompatDetector
    {
        public const string AbfArtificialPawnExtensionFullName = "ArtificialBeings.ABF_ArtificialPawnExtension";
        public const string AbfSynstructExtensionFullName = "ArtificialBeings.ABF_SynstructExtension";
        public const string KotorDroidBodyPrefix = "ATR_KotORDroidBody";

        public static bool ShouldSilenceHumanlikeMods(Pawn pawn)
        {
            if (pawn?.def is not ThingDef race)
                return false;

            if (PawnHasArtificialBeingComp(pawn))
                return true;

            if (PawnHasAbfMachineNeed(pawn))
                return true;

            if (RaceHasKotorDroidSignals(race))
                return true;

            var bodyDef = pawn.RaceProps?.body?.defName;
            if (!string.IsNullOrEmpty(bodyDef) && ContainsIgnoreCase(bodyDef, KotorDroidBodyPrefix))
                return true;

            var kindName = pawn.kindDef?.defName;
            if (DefLabelLooksLikeMachine(kindName))
                return true;

            if (DefLabelLooksLikeMachine(race.defName) || DefLabelLooksLikeMachine(race.label) || DefLabelLooksLikeMachine(race.LabelCap))
                return true;

            return false;
        }

        private static bool RaceHasKotorDroidSignals(ThingDef race)
        {
            if (race.modExtensions != null)
            {
                for (var i = 0; i < race.modExtensions.Count; i++)
                {
                    var ext = race.modExtensions[i];
                    if (ext == null)
                        continue;

                    var fullName = ext.GetType().FullName;
                    if (string.Equals(fullName, AbfArtificialPawnExtensionFullName, StringComparison.Ordinal)
                        || string.Equals(fullName, AbfSynstructExtensionFullName, StringComparison.Ordinal))
                        return true;
                }
            }

            var blood = race.race?.BloodDef?.defName;
            if (!string.IsNullOrEmpty(blood) && DefLabelLooksLikeMachine(blood))
                return true;

            return false;
        }

        private static bool PawnHasAbfMachineNeed(Pawn pawn)
        {
            var list = pawn?.needs?.AllNeeds;
            if (list == null)
                return false;

            for (var i = 0; i < list.Count; i++)
            {
                var n = list[i];
                if (n == null)
                    continue;

                var defn = n.def?.defName;
                if (!string.IsNullOrEmpty(defn))
                {
                    if (ContainsIgnoreCase(defn, "ABF_Need_Synstruct_")
                        || (ContainsIgnoreCase(defn, "ABF") && (ContainsIgnoreCase(defn, "Energy") || ContainsIgnoreCase(defn, "Coolant") || ContainsIgnoreCase(defn, "Lubricant"))))
                        return true;
                }

                var tn = n.GetType().FullName;
                if (!string.IsNullOrEmpty(tn) && ContainsIgnoreCase(tn, "ArtificialBeings") && (ContainsIgnoreCase(tn, "Need") || ContainsIgnoreCase(tn, "Synstruct")))
                    return true;
            }

            return false;
        }

        private static bool PawnHasArtificialBeingComp(Pawn pawn)
        {
            var comps = pawn?.AllComps;
            if (comps == null)
                return false;

            for (var i = 0; i < comps.Count; i++)
            {
                var c = comps[i];
                if (c == null)
                    continue;

                var tn = c.GetType().FullName;
                if (string.IsNullOrEmpty(tn))
                    continue;

                if (ContainsIgnoreCase(tn, "ArtificialBeings"))
                    return true;
            }

            return false;
        }

        private static bool ContainsIgnoreCase(string s, string substring)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(substring))
                return false;
            return s.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool DefLabelLooksLikeMachine(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return ContainsIgnoreCase(text, "Droid")
                || ContainsIgnoreCase(text, "Automaton")
                || ContainsIgnoreCase(text, "Synstruct")
                || ContainsIgnoreCase(text, "Artificial")
                || ContainsIgnoreCase(text, "HK")
                || ContainsIgnoreCase(text, "T3")
                || ContainsIgnoreCase(text, "R8009")
                || ContainsIgnoreCase(text, "GE3")
                || ContainsIgnoreCase(text, "KX12")
                || ContainsIgnoreCase(text, "KM1");
        }

        /// <summary>Death Rattle Continued 1.6 hediff defNames from Troopersmith1.DeathRattle.</summary>
        public static readonly HashSet<string> DeathRattleHediffDefNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "IntestinalFailure",
            "LiverFailure",
            "KidneyFailure",
            "ClinicalDeathNoHeartbeat",
            "ClinicalDeathAsphyxiation",
            "Coma",
            "BrainDamage",
            "ArtificialComa"
        };
    }
}
