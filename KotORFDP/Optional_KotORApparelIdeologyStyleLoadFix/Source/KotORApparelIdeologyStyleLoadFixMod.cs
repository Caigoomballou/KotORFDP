using HarmonyLib;
using Verse;

namespace KotORFDP.Optional.KotORApparelIdeologyStyleLoadFix
{
    /// <summary>Harmony entry point for optional guy762 CompStyleable load workaround.</summary>
    public class KotORApparelIdeologyStyleLoadFixMod : Mod
    {
        public KotORApparelIdeologyStyleLoadFixMod(ModContentPack content) : base(content)
        {
            if (!ModsConfig.IsActive("ludeon.rimworld.ideology"))
            {
                return;
            }

            var harmony = new Harmony("KotORFDP.Optional.KotORApparelIdeologyStyleLoadFix");
            harmony.PatchAll(typeof(KotORApparelIdeologyStyleLoadFixMod).Assembly);
        }
    }
}
