KotOR Droids compat module (source-only scaffold)

This folder mirrors the OuterRim_DroidCompat_HumanlikeSilencer architecture,
retargeted for:
- guy762.KotORDroids
- Killathon.ArtificialBeings
- Killathon.ArtificialBeings.SynCore

Included:
- Death Rattle filtering patches
- RimPsyche conversation skip patches
- Optional Nice Health Tab hooks

Excluded intentionally:
- AsimovChargepackChargerFix

Build:
- Open Source/KotORDroidCompat.csproj
- Compile (Release)
- Output DLL goes to KotORDroidscompats/Assemblies/

Load:
- KotORFDP LoadFolders.xml already includes "KotORDroidscompats"
  so Assemblies in this folder can be loaded by RimWorld.
