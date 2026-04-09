# KotORFDP
For guy and lee
 — Adjusts selected KotOR `FactionDef` entries (layer whitelist / max counts) so the same faction layout isn’t rolled multiple times at worldgen.
 — Removes duplicate `CompProperties_Styleable` from KotOR apparel abstract defs when **Ideology** is active (avoids two style comps + duplicate save nodes / load-ID pain). Optional Harmony in `Optional_KotORApparelIdeologyStyleLoadFix` supports the same problem from the code side.
 — broader hero age bands and related generation tweaks; trait-generation safeguards for certain KotOR droid races vs HAR/ABF.
**Harmony — `KotORDroidscompats/Assemblies/KotORDroidCompat.dll`**
Patches use `Prepare()` and reflection where needed, so **missing optional mods do not break load**.
- **Artificial Beings (ABF)** — During **save load / cross-ref** (when `Scribe` is active), guards `CompArtificialPawn` **stat offset/factor** and **`ActiveDirectives`** for detected KotOR/ABF-style droids to avoid **null-reference cascades** in stat resolution.
- **Death Rattle** — Skips inappropriate organ-failure style hediff logic on droids when that mod is present.
- **RimPsyche** — Skips conversation hooks involving droids when that mod is present.
- **Nice Health Tab** — Optional hooks when that mod is present.
- **Modular Weapons 2** — While **saving**, avoids ability creation side effects that can spam IDs / destabilize save data.
- **Loaded object directory** — Dedupes duplicate registration for certain **KotOR droid weapon verb** load IDs on save/reload.
**Detection** — `DroidCompatDetector` identifies KotOR/ABF droids via race extensions, comps, needs, body defs, and naming heuristics so human-oriented mods aren’t applied where they don’t fit.
