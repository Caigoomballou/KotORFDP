using HarmonyLib;
using Verse;

namespace KotORFDP.KotORDroidsCompat
{
    public class KotORDroidCompatMod : Mod
    {
        public KotORDroidCompatMod(ModContentPack content) : base(content)
        {
            new Harmony("KotORFDP.KotORDroidsCompat.HumanlikeSilencer").PatchAll();
        }
    }
}
