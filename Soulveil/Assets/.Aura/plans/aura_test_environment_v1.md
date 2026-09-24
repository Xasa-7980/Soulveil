# Stylized Outdoor Environment — Aura_Test Scene

## Objective
Build a ~120 × 120 m stylized spring/green outdoor area in `AuraScene.unity` from existing prefab instances, framed by cliffs and vegetation, with a dirt path leading to a raised far-east shrine focal point — then Save As `Assets/Scenes/Aura_Test.unity` leaving the original scene file untouched.

## Changes
- `Assets/Project/00_Prefabs/AuraScene/AuraScene.unity` — (modify in memory only) source scene; opened, built in, then Saved As. On-disk file never written by `SaveScene(scene)`.
- `Assets/Scenes/Aura_Test.unity` — (create) final saved scene containing all work.
- `Assets/Scenes/Aura_TerrainData.asset` — (create) TerrainData: 120 × 120 m, heightmap 513, alphamap 512, 3 FFE terrain layers.
- No `.cs`, `.mat`, `.shader`, `.prefab`, or `.asset` in any pack folder is created or edited.

## Relevant Assets and Quirks
- Unity `6000.0.58f2`, URP `17.0.4`, Input System `1.14.2`, Cinemachine `3.1.0`. No compile errors.
- **Folder paths corrected:** `Assets/TriForge` does not exist — it is `Assets/TriForge Assets/Fantasy Forest Environment/…` (referred to below as **FFE**). The other three listed folders exist verbatim.
- **Pipeline verified — zero material work needed:** `BK_Grass.shader` declares `"RenderPipeline"="UniversalPipeline"`; Far East `M_rocks.mat` uses `_BaseMap/_BaseColor`; FFE `M_FFE_Rock_A.mat` likewise. All three packs are URP-native, so no magenta and no material edits.
- `AuraScene.unity` currently holds exactly two roots: `Main Camera` at `(0, 1, -10)` (identity rotation, facing **+Z**) and `Directional Light` at `(0, 3, 0)` rot `(50, -30, 0)`. RenderSettings: fog off, ambient skybox mode. No Volume object. Project-wide URP `Assets/Settings/DefaultVolumeProfile.asset` already applies.
- **Focal axis is therefore +Z:** shrine goes to `(0, ~pad, +42)` with Y rotation `180` so it faces the camera down the path. Corridor `|x| < 10` from `z = -12` to `z = +34` stays free of large objects (satisfies "no large objects in front of the shrine view").
- **Excluded variants:** all FFE `_Autumn`, `_Winter`, and `…_Billboard` prefabs; all `(1)`, `(2)`, `(3)` duplicate Far East prefabs; BK Islands `IslandCliff*_Sand` / `*_underwater` (beach theme). One consistent green palette only.
- **Excluded asset:** `BK/PureNature_Meadows/Prefabs/Mountains/Mountain1|2.prefab` — bounds report ~0.03 m, scale semantics unverified. Perimeter ridge uses Far East `SM_cliff_01…04` (13–42 m) + `BK Cliff1` + `BK Islands IslandCliff1/2` instead.
- **Scale consistency:** FFE trees 8–20 m and Far East trees/buildings 6–30 m; BK Meadows oaks/elms are 10–27 m and would dwarf the 14.5 m `SM_shrine`. Canopy = Far East + FFE; BK Meadows is limited to `Birch1…2`, `Cypres1`, `Bush1…3` and small props.
- **Grounding quirk:** many FFE prefabs report "bounds: unavailable" (root has no Renderer / LODGroup). Y-placement must use aggregated `GetComponentsInChildren<Renderer>()` bounds bottom, never `transform.position.y`.
- `SM_shrine_lanterns.prefab` carries a stray `TMP_InputField` component from the pack — instantiate as-is, no prefab edit.
- `SM_shrine` = 9.6 × 14.5 × 9.6 m (MeshCollider). `Cliff1` = 26.7 × 30.1 × 19.3 m. TerrainLayer tile size is 2 × 2 m (expected repetition).
- URP terrain supports 4 layers; 3 are used, so no conversion is required.

## Steps
- [ ] 1. `manage_scene action=load path=Assets/Project/00_Prefabs/AuraScene/AuraScene.unity`, confirm the two roots, then via `execute_code` create `Assets/Scenes/Aura_TerrainData.asset` (size `(120, 28, 120)`), build the Terrain GameObject with `Terrain.CreateTerrainGameObject`, and **create, never open/save, the source file** [depends: none]
- [ ] 2. In the same `execute_code` pass: sculpt via `SetHeights` — smooth value-noise, amplitude ≤ 2.0 m (gentle), flattened plateau disc r≈16 m at `(0,0,42)` raised +0.35 m for the shrine pad, and a flat corridor strip `|x| < 9`, `z ∈ [-14, 36]` at the noise floor; then `SetAlphamaps` (512², 3 layers) — layer 0 `TL_FFE_Layer_Grass01` base, layer 1 `TL_FFE_Layer_Forestfloor01` under tree rings `r > 30`, layer 2 `TL_FFE_Layer_Dirt01` as a 5–6 m path from `(0, -12)` to `(0, 36)` [depends: 1]
- [ ] 3. `execute_code`: create root `Environment` plus children `Terrain`, `Vegetation`, `Rocks`, `Props`, `Architecture`, `Lighting` (all at identity), parent the Terrain under `Terrain`, and define one deterministic `PlaceInstance(prefabPath, parent, x, z, yaw, scale, seed)` helper — seeded RNG, `Terrain.SampleHeight` + aggregated renderer-bounds bottom snap, `PrefabUtility.InstantiatePrefab` into the active scene [depends: 2]
- [ ] 4. Place **Rocks** framing via `PlaceInstance`: perimeter ridge of `SM_cliff_01…04` (~7, scaled 1.0–1.5) on a radius 45–58 m ring, denser E/W/S, with a deliberate gap for the `|x| < 12` sightline and a backing cluster behind the shrine at `z > 54`; `BK Cliff1` ×2; `IslandCliff1/2` ×4; `Stone1…5` ×14 and `Boulder_8/11/13/15` ×8 at r 8–30 (roadside, not on the path); `P_FFE_Rock1…4` ×6; `SM_stoneblocks_01/02` ×4 [depends: 3]
- [ ] 5. Place **Vegetation** via `PlaceInstance`: canopy r 28–52 — `SM_tree_fareast_01/02/03`, `SM_tree_large_fareast_01/02`, `SM_tree_standard_03` ×22, `P_FFE_Spruce_A1–A4`, `P_FFE_Birch_1/2/3` ×16, `Birch1/2`, `Cypres1` ×6; shrubs `Bush1…3`, `P_FFE_Largebush`, `P_FFE_Bush_1/2` ×14 at r 12–30; ground cover r 6–32 — `P_FFE_Grass_01`, `P_FFE_Grass_Short_01`, `P_FFE_Flowers_Blue/White`, `P_FFE_Flower_Yellow`, `P_FFE_Reeds_Small`, `Grass1`, `GrassMeadows2`, `FlowerMeadow1`, `Daisy`, `Lupin1`, `Reeds`, `SM_grass_01` ×44; mushrooms `Mushroom1/3/6`, `P_FFE_Mushroom01_A`, `P_FFE_Mushroom01_Group_Small` ×8 [depends: 3]
- [ ] 6. Place **Props** + **Architecture** via `PlaceInstance`: focal — `SM_platform_temple` centred `(0, pad, 42)`, `SM_staircase_small` at the south edge facing -Z, `SM_shrine` `(0, pad, 43.5)` yaw `180`, `SM_shrine_lanterns` offset to +X; approach — `SM_fence_temple` ×2 flanking at `x = ±7`, `SM_stonelantern_01` ×3 along the path at `x = ±3.5` (`z = -2, 14, 29`), `SM_prayertablet_preset_01`, `SM_bench_wooden`, `SM_pot_02`, `SM_basket_01`; camp — `P_FFE_Campfire_Lit` + `P_FFE_Fence01_Start/Mid/End`, `P_FFE_Lantern`, `P_FFE_Waterplants` ×2, `Barrel_1`, `Barrel_3` at r 18–26 off-path [depends: 3]
- [ ] 7. **Lighting** via `manage_gameobject` + `manage_graphics`: reparent `Directional Light` under `Lighting`, set rot `(48, -35, 0)` / intensity `1.2` / warm tint; add one warm `Point Light` above the shrine at `(0, pad + 6, 42)`; `skybox_set_ambient` → Trilight with spring-green sky/equator/ground, `skybox_set_fog` → Linear, start 45, end 150, pale green-white; `volume_create` Global Volume under `Lighting` reusing the existing `Assets/BK/PureNature_Meadows/Settings/Meadows_PostProcess.asset` (referenced read-only, URP Bloom + ColorAdjustments) [depends: 3]
- [ ] 8. Save As via `execute_code` → `EditorSceneManager.SaveScene(activeScene, "Assets/Scenes/Aura_Test.unity")` (never the pathless overload, never `manage_scene action=save`), log instance counts per parent with `Debug.Log`, then run the Verify block [depends: 4,5,6,7]

## Verify
- **Source untouched:** compare SHA-256 of `Assets/Project/00_Prefabs/AuraScene/AuraScene.unity` before step 1 and after step 8 — must be identical.
- **No stray assets:** `execute_command "git status --short"` — only `Assets/Scenes/Aura_Test.unity`, `Assets/Scenes/Aura_Test.unity.meta`, `Assets/Scenes/Aura_TerrainData.asset(+.meta)`, and the plan file may appear.
- **No compile/console errors:** `check_compile_errors` + `get_unity_logs` (errors/warnings) filtered on the scene build.
- **Hierarchy:** `list_game_objects_in_hierarchy` — exactly one root `Environment` with the six named children; zero unparented prefab instances at scene root.
- **Visual, layout:** `capture_editor_screenshot mode=scene_view` then `manage_camera action=screenshot camera=Main Camera include_image=true` — shrine visible from the default camera at `(0, 1, -10)`, path reads as a continuous 5–6 m dirt strip, no tall object intersecting the `|x| < 10` corridor, no floating/sunken instances.
- **Focal check:** `manage_camera action=screenshot view_target=Environment/Architecture/SM_shrine view_position=[0, 6, 22]` — shrine unobstructed, framed by cliffs and lanterns.
- **Runtime:** `manage_editor action=play` → `capture_editor_screenshot mode=game_view` → `manage_editor action=stop`; confirm scene renders lit (no pink), then `get_unity_logs` for runtime errors.
