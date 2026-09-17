// Unity 2022.3+ / Unity 6. Requires URP in the project.
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AnimeBiomes.Editor
{
    [Serializable] public class SurfaceEntry
    {
        public string biome, role, id, title;
        public float tileSize, normalScale, smoothness;
    }
    [Serializable] public class SurfaceCatalog { public SurfaceEntry[] entries; }

    public sealed class AnimeTextureImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(AnimeBiomesSetup.Root + "/Textures/", StringComparison.Ordinal)) return;
            var t = (TextureImporter)assetImporter;
            bool normal = assetPath.EndsWith("_Normal.png", StringComparison.Ordinal);
            bool mask = assetPath.EndsWith("_Mask.png", StringComparison.Ordinal);
            t.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            t.sRGBTexture = !normal && !mask;
            t.convertToNormalmap = false;
            t.alphaSource = mask ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
            t.alphaIsTransparency = false;
            t.wrapMode = TextureWrapMode.Repeat;
            t.filterMode = FilterMode.Trilinear;
            t.mipmapEnabled = true;
            t.anisoLevel = 4;
            t.maxTextureSize = 1024;
            t.npotScale = TextureImporterNPOTScale.None;
            t.textureCompression = TextureImporterCompression.Uncompressed;
            t.isReadable = false;
        }
    }

    [InitializeOnLoad]
    public static class AnimeBiomesSetup
    {
        public const string Root = "Assets/AnimeBiomes";
        static AnimeBiomesSetup() { EditorApplication.delayCall += InstallAfterImport; }
        static void InstallAfterImport()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            { EditorApplication.delayCall += InstallAfterImport; return; }
            if (!File.Exists(Root + "/catalog.json")) return;
            Install(false);
        }
        public static SurfaceCatalog ReadCatalog()
        { return JsonUtility.FromJson<SurfaceCatalog>(File.ReadAllText(Root + "/catalog.json")); }

        [MenuItem("Tools/Anime Biomes/Crear configuraciones que faltan")]
        public static void InstallMenu() { Install(true); }

        public static void Install(bool report)
        {
            if (!File.Exists(Root + "/catalog.json"))
            { if (report) Debug.LogError("Copia la carpeta AnimeBiomes dentro de Assets sin cambiar su nombre."); return; }
            var catalog = ReadCatalog();
            if (catalog == null || catalog.entries == null) return;
            EnsureFolder(Root + "/Layers");
            EnsureFolder(Root + "/Materials");
            int created = 0, missing = 0;
            foreach (var e in catalog.entries)
            {
                string path = Root + "/Layers/" + e.id + ".terrainlayer";
                if (AssetDatabase.LoadAssetAtPath<TerrainLayer>(path) != null) continue;
                string stem = Root + "/Textures/" + e.biome + "/" + e.id;
                PrepareTexture(stem + "_Albedo.png", false, false);
                PrepareTexture(stem + "_Normal.png", true, false);
                PrepareTexture(stem + "_Mask.png", false, true);
                var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(stem + "_Albedo.png");
                var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(stem + "_Normal.png");
                var mask = AssetDatabase.LoadAssetAtPath<Texture2D>(stem + "_Mask.png");
                if (!albedo || !normal || !mask) { missing++; continue; }
                var layer = new TerrainLayer {
                    name = e.id, diffuseTexture = albedo, normalMapTexture = normal,
                    maskMapTexture = mask, tileSize = new Vector2(e.tileSize, e.tileSize),
                    tileOffset = Vector2.zero, normalScale = e.normalScale,
                    metallic = 0f, smoothness = e.smoothness,
                    diffuseRemapMin = Vector4.zero, diffuseRemapMax = Vector4.one,
                    maskMapRemapMin = Vector4.zero, maskMapRemapMax = Vector4.one
                };
                AssetDatabase.CreateAsset(layer, path); created++;
            }
            string materialPath = Root + "/Materials/AnimeTerrain_URP.mat";
            if (!AssetDatabase.LoadAssetAtPath<Material>(materialPath))
            {
                var shader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
                if (shader)
                {
                    var material = new Material(shader) { name = "AnimeTerrain_URP" };
                    // Weight blending also works when village layers are added as a fifth layer.
                    if (material.HasProperty("_EnableHeightBlend")) material.SetFloat("_EnableHeightBlend", 0f);
                    material.DisableKeyword("_TERRAIN_BLEND_HEIGHT");
                    if (material.HasProperty("_EnableInstancedPerPixelNormal"))
                        material.SetFloat("_EnableInstancedPerPixelNormal", 1f);
                    material.EnableKeyword("_TERRAIN_INSTANCED_PERPIXEL_NORMAL");
                    material.enableInstancing = true;
                    AssetDatabase.CreateAsset(material, materialPath);
                }
                else if (report) Debug.LogWarning("No se encuentra Terrain Lit de URP. Comprueba que URP esté instalado y activo.");
            }
            if (created > 0) AssetDatabase.SaveAssets();
            if (report || created > 0)
                Debug.Log("Anime Biomes: " + created + " Terrain Layers creados. " + missing + " parejas incompletas. Las configuraciones existentes se conservan.");
        }
        static void PrepareTexture(string path, bool normal, bool mask)
        {
            var t = AssetImporter.GetAtPath(path) as TextureImporter;
            if (t == null) return;
            var type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            var alpha = mask ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
            bool changed = t.textureType != type || t.sRGBTexture != (!normal && !mask)
                || t.alphaSource != alpha || t.alphaIsTransparency || t.convertToNormalmap
                || t.wrapMode != TextureWrapMode.Repeat || !t.mipmapEnabled
                || t.filterMode != FilterMode.Trilinear || t.anisoLevel != 4
                || t.maxTextureSize != 1024 || t.isReadable
                || t.npotScale != TextureImporterNPOTScale.None
                || t.textureCompression != TextureImporterCompression.Uncompressed;
            if (!changed) return;
            t.textureType = type; t.sRGBTexture = !normal && !mask;
            t.alphaSource = alpha; t.alphaIsTransparency = false; t.convertToNormalmap = false;
            t.wrapMode = TextureWrapMode.Repeat; t.mipmapEnabled = true;
            t.filterMode = FilterMode.Trilinear; t.anisoLevel = 4; t.maxTextureSize = 1024;
            t.isReadable = false; t.npotScale = TextureImporterNPOTScale.None;
            t.textureCompression = TextureImporterCompression.Uncompressed;
            t.SaveAndReimport();
        }
        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
        [MenuItem("Tools/Anime Biomes/Crear terreno de muestra...")]
        static void OpenDemo() { AnimeBiomeDemo.Open(); }
    }

    public sealed class AnimeBiomeDemo : EditorWindow
    {
        readonly string[] biomes = { "Pradera", "Lava", "Acuatico", "Nieve", "Trueno", "Cielo", "Oscuridad" };
        int selected;
        public static void Open() { GetWindow<AnimeBiomeDemo>("Muestra de bioma"); }
        void OnGUI()
        {
            EditorGUILayout.LabelField("Nuevo terreno de muestra", EditorStyles.boldLabel);
            selected = EditorGUILayout.Popup("Bioma", selected, biomes);
            EditorGUILayout.HelpBox("Crea un Terrain nuevo de 96 x 96 m con cuatro capas distribuidas por inclinación. No modifica tus terrenos. No añade luces ni cambia tu cámara.", MessageType.Info);
            if (GUILayout.Button("Crear muestra")) CreateDemo(biomes[selected]);
        }
        static float Smooth(float a, float b, float x)
        { float t = Mathf.InverseLerp(a, b, x); return t * t * (3f - 2f * t); }
        static void CreateDemo(string biome)
        {
            AnimeBiomesSetup.Install(false);
            var entries = AnimeBiomesSetup.ReadCatalog().entries.Where(e => e.biome == biome).ToArray();
            if (entries.Length != 4) { Debug.LogError("Se esperaban cuatro superficies."); return; }
            var layers = entries.Select(e => AssetDatabase.LoadAssetAtPath<TerrainLayer>(AnimeBiomesSetup.Root + "/Layers/" + e.id + ".terrainlayer")).ToArray();
            var material = AssetDatabase.LoadAssetAtPath<Material>(AnimeBiomesSetup.Root + "/Materials/AnimeTerrain_URP.mat");
            if (layers.Any(l => l == null) || !material)
            { Debug.LogError("Faltan capas o material URP. Usa Crear configuraciones que faltan y revisa URP."); return; }
            string folder = AnimeBiomesSetup.Root + "/Demos";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(AnimeBiomesSetup.Root, "Demos");
            var data = new TerrainData { heightmapResolution = 257, alphamapResolution = 256, size = new Vector3(96, 32, 96) };
            var h = new float[257,257];
            for (int y = 0; y < 257; y++) for (int x = 0; x < 257; x++)
            {
                float u = x / 256f, v = y / 256f;
                float ridge = Smooth(.28f, .69f, u);
                float modulation = .88f + .12f * Mathf.Sin(v * Mathf.PI * 4f);
                h[y,x] = .04f + .69f * ridge * modulation + .035f * Mathf.PerlinNoise(u * 5f, v * 5f);
            }
            data.SetHeights(0,0,h); data.terrainLayers = layers;
            var weights = new float[256,256,4];
            for (int y = 0; y < 256; y++) for (int x = 0; x < 256; x++)
            {
                float slope = data.GetSteepness(x / 255f, y / 255f);
                float s0 = Smooth(5,13,slope), s1 = Smooth(17,29,slope), s2 = Smooth(32,46,slope);
                weights[y,x,0] = 1-s0; weights[y,x,1] = s0*(1-s1);
                weights[y,x,2] = s0*s1*(1-s2); weights[y,x,3] = s0*s1*s2;
            }
            data.SetAlphamaps(0,0,weights);
            string asset = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + biome + "_Demo.asset");
            AssetDatabase.CreateAsset(data,asset);
            var go = Terrain.CreateTerrainGameObject(data); go.name = "Muestra_" + biome;
            Undo.RegisterCreatedObjectUndo(go,"Crear muestra de bioma");
            var terrain = go.GetComponent<Terrain>(); terrain.materialTemplate = material; terrain.drawInstanced = true;
            terrain.heightmapPixelError = 3; terrain.basemapDistance = 500;
            Selection.activeGameObject = go;
            if (SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.FrameSelected();
            AssetDatabase.SaveAssets();
        }
    }
}
