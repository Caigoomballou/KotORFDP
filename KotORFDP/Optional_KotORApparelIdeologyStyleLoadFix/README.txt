KotOR Apparel Ideology style fix (optional folder — see KotORFDP LoadFolders.xml)

Problem
  Star Wars: KotOR Core adds CompProperties_Styleable on apparel ThingDefs. With the Ideology DLC,
  the game also injects CompStyleable. That produced TWO CompStyleable components on the same item,
  both serializing "sourcePrecept" → duplicate lines in saves and load errors (same load ID twice: null).

Fix (two layers)
  1) 1.6/Patches/KotORFDP_IdeologyStyleableDedup.xml
     Removes the mod-added CompProperties_Styleable when Ideology is active (Ideology supplies the comp).
     Without Ideology, KotOR's XML comp is left alone.

  2) KotORApparelIdeologyStyleLoadFix.dll
     During LoadingVars and Saving, only the FIRST CompStyleable on a guy762_* thing runs PostExposeData.
     Covers old saves and any edge case where two comps still exist.

Requires: Harmony, Ideology DLC (same as CompStyleable).

To disable: remove Optional_KotORApparelIdeologyStyleLoadFix from LoadFolders.xml and optionally delete this folder.

Rebuild DLL: dotnet build -c Release from Source\
