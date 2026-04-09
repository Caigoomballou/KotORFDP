using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace KotORFDP.KotORDroidsCompat.Optional.NiceHealthTab
{
    [StaticConstructorOnStartup]
    internal static class NhtPatchBootstrap
    {
        private const string NhtPackageId = "Andromeda.NiceHealthTab";
        private const string HarmonyId = "KotORFDP.KotORDroidsCompat.Optional.NiceHealthTab";
        private static bool _applied;

        static NhtPatchBootstrap()
        {
            TryApply();
        }

        private static void TryApply()
        {
            if (_applied || !ModsConfig.IsActive(NhtPackageId))
                return;

            var candidates = ResolveAllFillTabDecisionWrappers();
            if (candidates.Count == 0)
            {
                _applied = true;
                return;
            }

            var harmony = new Harmony(HarmonyId);
            var prefix = new HarmonyMethod(typeof(NhtPatchBootstrap).GetMethod(
                nameof(PrefixSetBoolDecisionTrue),
                BindingFlags.Static | BindingFlags.NonPublic));

            foreach (var mi in candidates)
            {
                try
                {
                    var miInfo = mi as MethodInfo;
                    if (miInfo != null && miInfo.ReturnType == typeof(bool))
                    {
                        harmony.Patch(mi, prefix: prefix);
                    }
                    else
                    {
                        var voidPrefix = new HarmonyMethod(typeof(NhtPatchBootstrap).GetMethod(
                            nameof(PrefixSetVoidDecisionTrue),
                            BindingFlags.Static | BindingFlags.NonPublic));
                        harmony.Patch(mi, prefix: voidPrefix);
                    }
                }
                catch
                {
                }
            }

            TryPatchNhtHealthCardDollPrefix(harmony);
            _applied = true;
        }

        private static List<MethodBase> ResolveAllFillTabDecisionWrappers()
        {
            var list = new List<MethodBase>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var tabType = typeof(ITab_Pawn_Health);
            var byrefBool = typeof(bool).MakeByRefType();
            var pawnType = typeof(Pawn);

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!LooksLikeNiceHealthAssembly(asm))
                    continue;

                foreach (var type in SafeGetTypes(asm))
                {
                    if (type == null)
                        continue;

                    const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                    foreach (var mi in type.GetMethods(flags))
                    {
                        var p = mi.GetParameters();
                        if (p.Length < 1)
                            continue;

                        var hasTabParam = false;
                        var hasByrefBool = false;
                        var hasPawnParam = false;
                        for (var i = 0; i < p.Length; i++)
                        {
                            if (p[i].ParameterType == tabType) hasTabParam = true;
                            if (p[i].ParameterType == byrefBool) hasByrefBool = true;
                            if (p[i].ParameterType == pawnType) hasPawnParam = true;
                        }

                        if (!hasTabParam && !hasPawnParam)
                            continue;

                        if (mi is MethodInfo mInfo)
                        {
                            var isBoolDecision = mInfo.ReturnType == typeof(bool);
                            var isVoidByrefDecision = mInfo.ReturnType == typeof(void) && hasByrefBool;
                            if (!isBoolDecision && !isVoidByrefDecision)
                                continue;
                        }
                        else
                        {
                            continue;
                        }

                        var dt = mi.DeclaringType;
                        var dtName = dt?.Name ?? "";
                        var dtFull = dt?.FullName ?? "";
                        var methodName = mi.Name ?? "";
                        var nameLooksRelated =
                            ContainsIgnoreCase(dtName, "FillTab")
                            || ContainsIgnoreCase(dtFull, "FillTab")
                            || ContainsIgnoreCase(methodName, "FillTab")
                            || ContainsIgnoreCase(dtName, "Health")
                            || ContainsIgnoreCase(methodName, "Health")
                            || ContainsIgnoreCase(dtName, "Doll")
                            || ContainsIgnoreCase(methodName, "Doll")
                            || ContainsIgnoreCase(dtName, "Vanilla")
                            || ContainsIgnoreCase(methodName, "Vanilla");

                        if (!nameLooksRelated)
                            continue;

                        var key = mi.ToString();
                        if (seen.Add(key))
                            list.Add(mi);
                    }
                }
            }

            return list;
        }

        private static bool LooksLikeNiceHealthAssembly(Assembly asm)
        {
            var name = asm.GetName().Name;
            if (string.IsNullOrEmpty(name))
                return false;
            return name.IndexOf("NiceHealth", StringComparison.OrdinalIgnoreCase) >= 0
                   || name.IndexOf("NiceHediff", StringComparison.OrdinalIgnoreCase) >= 0
                   || name.IndexOf("Andromeda", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Type[] SafeGetTypes(Assembly asm)
        {
            try { return asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types; }
        }

        private static bool ContainsIgnoreCase(string s, string substring)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(substring))
                return false;
            return s.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ShouldSilenceNhtForPawn(Pawn pawn)
        {
            return pawn != null && DroidCompatDetector.ShouldSilenceHumanlikeMods(pawn);
        }

        private static bool PrefixSetBoolDecisionTrue(object[] __args, ref bool __result)
        {
            var pawn = ExtractPawn(__args);
            if (pawn == null || !ShouldSilenceNhtForPawn(pawn))
                return true;

            __result = true;
            return true;
        }

        private static bool PrefixSetVoidDecisionTrue(object[] __args, MethodBase __originalMethod)
        {
            var pawn = ExtractPawn(__args);
            if (pawn == null || !ShouldSilenceNhtForPawn(pawn))
                return true;

            var byrefBool = typeof(bool).MakeByRefType();
            var parameters = __originalMethod?.GetParameters();
            if (parameters != null && __args != null)
            {
                for (var i = 0; i < parameters.Length && i < __args.Length; i++)
                {
                    if (parameters[i].ParameterType == byrefBool)
                        __args[i] = true;
                }
            }

            return true;
        }

        private static Pawn ExtractPawn(object[] args)
        {
            Pawn pawn = null;
            if (args != null)
            {
                foreach (var a in args)
                {
                    pawn = a as Pawn;
                    if (pawn != null)
                        return pawn;
                }
            }

            if (args != null)
            {
                foreach (var a in args)
                {
                    var tab = a as ITab_Pawn_Health;
                    if (tab != null)
                    {
                        pawn = NiceHealthTabOptionalHooks.GetPawnFromHealthTab(tab);
                        if (pawn != null)
                            return pawn;
                    }
                }
            }

            return null;
        }

        private static void TryPatchNhtHealthCardDollPrefix(Harmony harmony)
        {
            var prefixMethodBool = typeof(NhtPatchBootstrap).GetMethod(
                nameof(SuppressNhtHealthCardDollPrefixBool),
                BindingFlags.Static | BindingFlags.NonPublic);
            var prefixMethodVoid = typeof(NhtPatchBootstrap).GetMethod(
                nameof(SuppressNhtHealthCardDollPrefixVoid),
                BindingFlags.Static | BindingFlags.NonPublic);

            if (prefixMethodBool == null || prefixMethodVoid == null)
                return;

            var pawnT = typeof(Pawn);
            var rectT = typeof(Rect);
            var rectByRefT = rectT.MakeByRefType();
            var tabT = typeof(ITab_Pawn_Health);
            const BindingFlags methodFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!LooksLikeNiceHealthAssembly(asm))
                    continue;

                foreach (var type in SafeGetTypes(asm))
                {
                    if (type == null)
                        continue;

                    foreach (var m in type.GetMethods(methodFlags))
                    {
                        if (!(m.Name.StartsWith("Prefix", StringComparison.Ordinal) || m.Name.StartsWith("Postfix", StringComparison.Ordinal)))
                            continue;
                        if (m.ReturnType != typeof(bool) && m.ReturnType != typeof(void))
                            continue;

                        var p = m.GetParameters();
                        if (p == null || p.Length < 2)
                            continue;

                        var hasRect = false;
                        var hasPawn = false;
                        for (var i = 0; i < p.Length; i++)
                        {
                            var pt = p[i].ParameterType;
                            if (pt == rectT || pt == rectByRefT) hasRect = true;
                            else if (pt == pawnT || pawnT.IsAssignableFrom(pt)) hasPawn = true;
                            else if (pt == tabT || tabT.IsAssignableFrom(pt)) hasPawn = true;
                        }

                        if (!hasRect || !hasPawn)
                            continue;

                        try
                        {
                            if (m.ReturnType == typeof(bool))
                                harmony.Patch(m, prefix: new HarmonyMethod(prefixMethodBool));
                            else
                                harmony.Patch(m, prefix: new HarmonyMethod(prefixMethodVoid));
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private static bool SuppressNhtHealthCardDollPrefixBool(object[] __args, ref bool __result)
        {
            var pawn = ExtractPawn(__args);
            if (pawn == null || !ShouldSilenceNhtForPawn(pawn))
                return true;
            __result = true;
            return false;
        }

        private static bool SuppressNhtHealthCardDollPrefixVoid(object[] __args)
        {
            var pawn = ExtractPawn(__args);
            if (pawn == null || !ShouldSilenceNhtForPawn(pawn))
                return true;
            return false;
        }
    }
}
