using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class ShaderGraphURPBatchConverter : EditorWindow
{
    private DefaultAsset targetFolder;
    private Vector2 scroll;

    private readonly List<Result> results = new();

    private enum AssetKind
    {
        ShaderGraph,
        AmplifyShader,
        OtherShader
    }

    private enum Status
    {
        Compatible,
        AlreadyUniversal,
        Manual,
        Unsupported,
        Converted,
        Failed
    }

    private class Result
    {
        public string path;
        public AssetKind kind;
        public Status status;
        public string message;
    }

    [MenuItem("Tools/Shader Graph/Batch Add Universal Target")]
    public static void Open ( )
    {
        GetWindow<ShaderGraphURPBatchConverter>(
            "Shader / ShaderGraph → URP"
        );
    }

    private void OnGUI ( )
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Vefects → URP Batch Converter",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Analiza .shadergraph y .shader dentro de una carpeta.\n\n" +
            "IMPORTANTE:\n" +
            "• NO renombra archivos.\n" +
            "• NO mueve archivos.\n" +
            "• NO modifica los .meta.\n" +
            "• Mantiene el nombre interno Shader \"...\".\n" +
            "• Crea una copia .urpbackup antes de modificar un .shader.\n\n" +
            "Los Amplify con GrabPass, Standard u otros casos dudosos " +
            "se reportan pero NO se convierten automáticamente.",
            MessageType.Info
        );

        EditorGUILayout.Space(8);

        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Carpeta",
            targetFolder,
            typeof(DefaultAsset),
            false
        );

        EditorGUILayout.Space(8);

        using (new EditorGUI.DisabledScope(targetFolder == null))
        {
            if (GUILayout.Button("Analizar carpeta", GUILayout.Height(34)))
            {
                Analyze();
            }
        }

        if (results.Count > 0)
        {
            DrawResults();

            int amplifySafe =
                results.Count(r =>
                    r.kind == AssetKind.AmplifyShader &&
                    r.status == Status.Compatible);

            EditorGUILayout.Space(10);

            using (new EditorGUI.DisabledScope(amplifySafe == 0))
            {
                if (GUILayout.Button(
                    $"Convertir Amplify BIRP seguros ({amplifySafe})",
                    GUILayout.Height(40)))
                {
                    ConvertAmplifySafe();
                }
            }
        }
    }

    private void DrawResults ( )
    {
        EditorGUILayout.Space(10);

        int compatible =
            results.Count(x => x.status == Status.Compatible);

        int universal =
            results.Count(x => x.status == Status.AlreadyUniversal);

        int manual =
            results.Count(x => x.status == Status.Manual);

        int converted =
            results.Count(x => x.status == Status.Converted);

        int failed =
            results.Count(x => x.status == Status.Failed);

        EditorGUILayout.LabelField(
            $"Compatibles: {compatible}   " +
            $"Ya URP: {universal}   " +
            $"Manual: {manual}   " +
            $"Convertidos: {converted}   " +
            $"Errores: {failed}",
            EditorStyles.boldLabel
        );

        scroll = EditorGUILayout.BeginScrollView(
            scroll,
            GUILayout.Height(400)
        );

        foreach (Result result in results)
        {
            string prefix = result.status switch
            {
                Status.Compatible => "[COMPATIBLE]",
                Status.AlreadyUniversal => "[YA URP]",
                Status.Manual => "[MANUAL]",
                Status.Unsupported => "[IGNORADO]",
                Status.Converted => "[CONVERTIDO]",
                Status.Failed => "[ERROR]",
                _ => "[?]"
            };

            string type = result.kind switch
            {
                AssetKind.ShaderGraph => "ShaderGraph",
                AssetKind.AmplifyShader => "Amplify",
                _ => "Shader"
            };

            EditorGUILayout.LabelField(
                $"{prefix} [{type}] {result.path}",
                EditorStyles.wordWrappedLabel
            );

            if (!string.IsNullOrEmpty(result.message))
            {
                EditorGUILayout.LabelField(
                    "    " + result.message,
                    EditorStyles.miniLabel
                );
            }

            EditorGUILayout.Space(4);
        }

        EditorGUILayout.EndScrollView();
    }

    private void Analyze ( )
    {
        results.Clear();

        string folder =
            AssetDatabase.GetAssetPath(targetFolder);

        if (!AssetDatabase.IsValidFolder(folder))
        {
            Debug.LogError(
                "Selecciona una carpeta válida dentro de Assets."
            );
            return;
        }

        string[] guids =
            AssetDatabase.FindAssets("", new[] { folder });

        IEnumerable<string> paths = guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p =>
                p.EndsWith(
                    ".shadergraph",
                    StringComparison.OrdinalIgnoreCase) ||
                p.EndsWith(
                    ".shader",
                    StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .OrderBy(p => p);

        foreach (string path in paths)
        {
            if (path.EndsWith(
                ".shadergraph",
                StringComparison.OrdinalIgnoreCase))
            {
                results.Add(AnalyzeShaderGraph(path));
            }
            else
            {
                results.Add(AnalyzeShader(path));
            }
        }

        Repaint();
    }

    private Result AnalyzeShaderGraph ( string path )
    {
        string text;

        try
        {
            text = File.ReadAllText(path);
        }
        catch (Exception e)
        {
            return Make(
                path,
                AssetKind.ShaderGraph,
                Status.Unsupported,
                e.Message
            );
        }

        if (text.Contains(
            "UnityEditor.Rendering.Universal.ShaderGraph.UniversalTarget"))
        {
            return Make(
                path,
                AssetKind.ShaderGraph,
                Status.AlreadyUniversal,
                "Ya contiene UniversalTarget."
            );
        }

        if (text.Contains(
            "\"m_Type\": \"UnityEditor.ShaderGraph.VFXTarget\""))
        {
            return Make(
                path,
                AssetKind.ShaderGraph,
                Status.Manual,
                "Shader Graph antiguo con VFXTarget. " +
                "Se detecta, pero esta versión no modifica su JSON automáticamente."
            );
        }

        return Make(
            path,
            AssetKind.ShaderGraph,
            Status.Unsupported,
            "No coincide con el VFXTarget antiguo esperado."
        );
    }

    private Result AnalyzeShader ( string path )
    {
        string text;

        try
        {
            text = File.ReadAllText(path);
        }
        catch (Exception e)
        {
            return Make(
                path,
                AssetKind.OtherShader,
                Status.Unsupported,
                e.Message
            );
        }

        if (text.Contains("\"RenderPipeline\"") &&
            text.Contains("UniversalPipeline"))
        {
            return Make(
                path,
                AssetKind.OtherShader,
                Status.AlreadyUniversal,
                "Ya parece ser un shader URP."
            );
        }

        bool amplify =
            text.Contains("Amplify Shader Editor") ||
            text.Contains("ASEBEGIN");

        if (!amplify)
        {
            return Make(
                path,
                AssetKind.OtherShader,
                Status.Unsupported,
                "No parece ser un shader generado por Amplify."
            );
        }

        if (text.Contains("GrabPass"))
        {
            return Make(
                path,
                AssetKind.AmplifyShader,
                Status.Manual,
                "Usa GrabPass. En URP necesita Scene Color / Opaque Texture."
            );
        }

        if (Regex.IsMatch(
            text,
            @"#pragma\s+surface\s+surf\s+Standard\b"))
        {
            return Make(
                path,
                AssetKind.AmplifyShader,
                Status.Manual,
                "Usa Surface Standard. No se convierte automáticamente."
            );
        }

        if (!Regex.IsMatch(
            text,
            @"#pragma\s+surface\s+surf\s+Unlit\b"))
        {
            return Make(
                path,
                AssetKind.AmplifyShader,
                Status.Manual,
                "No es un Surface Shader Unlit reconocido."
            );
        }

        if (!text.Contains("void surf("))
        {
            return Make(
                path,
                AssetKind.AmplifyShader,
                Status.Unsupported,
                "No encontré la función surf."
            );
        }

        return Make(
            path,
            AssetKind.AmplifyShader,
            Status.Compatible,
            DescribeBlend(text)
        );
    }

    private string DescribeBlend ( string text )
    {
        float src = ReadDefaultFloat(text, "_Src", -999);
        float dst = ReadDefaultFloat(text, "_Dst", -999);

        if (src == 1 && dst == 1)
            return "Unlit Transparent Additive. Conversión segura.";

        if (src == 5 && dst == 10)
            return "Unlit Transparent Alpha Blend. Conversión segura.";

        if (text.Contains("\"RenderType\" = \"Opaque\""))
            return "Unlit Opaque. Conversión segura.";

        return
            $"Unlit. Se conservará Blend [_Src] [_Dst] " +
            $"(defaults actuales: {src}/{dst}).";
    }

    private void ConvertAmplifySafe ( )
    {
        List<Result> targets = results
            .Where(r =>
                r.kind == AssetKind.AmplifyShader &&
                r.status == Status.Compatible)
            .ToList();

        if (targets.Count == 0)
            return;

        bool ok = EditorUtility.DisplayDialog(
            "Convertir Amplify → URP",
            $"Se convertirán {targets.Count} shaders.\n\n" +
            "NO se cambiará ningún nombre, ruta ni .meta.\n" +
            "El nombre interno Shader \"...\" también se conservará.\n\n" +
            "Antes de modificar cada shader se creará:\n" +
            "nombre.shader.urpbackup",
            "Convertir",
            "Cancelar"
        );

        if (!ok)
            return;

        try
        {
            for (int i = 0; i < targets.Count; i++)
            {
                Result result = targets[i];

                EditorUtility.DisplayProgressBar(
                    "Amplify → URP",
                    result.path,
                    (float)i / targets.Count
                );

                ConvertAmplifyShader(result);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.Refresh();

        Repaint();
    }

    private void ConvertAmplifyShader ( Result result )
    {
        string path = result.path;
        string backup = path + ".urpbackup";

        try
        {
            string original =
                File.ReadAllText(path, Encoding.UTF8);

            // Backup del archivo ORIGINAL.
            File.Copy(path, backup, true);

            string converted =
                BuildURPShaderFromAmplify(original);

            if (string.IsNullOrEmpty(converted))
            {
                result.status = Status.Failed;
                result.message =
                    "No se pudo construir el shader URP.";
                return;
            }

            // MUY IMPORTANTE:
            // se escribe en EL MISMO archivo.
            // No tocamos path ni .meta.
            File.WriteAllText(
                path,
                converted,
                new UTF8Encoding(false)
            );

            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceSynchronousImport |
                ImportAssetOptions.ForceUpdate
            );

            Shader shader =
                AssetDatabase.LoadAssetAtPath<Shader>(path);

            if (shader == null)
            {
                Restore(path, backup);

                result.status = Status.Failed;
                result.message =
                    "Unity no pudo importar el shader. Restaurado.";

                return;
            }

            result.status = Status.Converted;
            result.message =
                "Convertido in-place. Nombre, ruta y GUID conservados.";

            Debug.Log(
                "[Vefects URP] Convertido: " + path
            );
        }
        catch (Exception e)
        {
            try
            {
                Restore(path, backup);
            }
            catch
            {
                // Conservamos el error original.
            }

            result.status = Status.Failed;
            result.message = e.Message;

            Debug.LogError(
                $"[Vefects URP] Error en {path}\n{e}"
            );
        }
    }

    private string BuildURPShaderFromAmplify ( string original )
    {
        string shaderName =
            Regex.Match(
                original,
                @"Shader\s+""([^""]+)"""
            ).Groups[1].Value;

        if (string.IsNullOrEmpty(shaderName))
            return null;

        string properties =
            ExtractBraceBlockAfterWord(
                original,
                "Properties"
            );

        if (string.IsNullOrEmpty(properties))
            return null;

        string cg =
            ExtractCgSection(original);

        if (string.IsNullOrEmpty(cg))
            return null;

        cg = CleanAmplifyCG(cg);

        bool hasUV =
            Regex.IsMatch(
                cg,
                @"\buv_texcoord\b");

        bool hasUV2 =
            Regex.IsMatch(
                cg,
                @"\buv2_texcoord2\b");

        bool hasVertexColor =
            Regex.IsMatch(
                cg,
                @"\bvertexColor\b");

        bool opaque =
            original.Contains(
                "\"RenderType\" = \"Opaque\"") ||
            original.Contains(
                "\"RenderType\"=\"Opaque\"");

        bool hasCullProperty =
            HasProperty(original, "_Cull");

        bool hasZWrite =
            HasProperty(original, "_ZWrite");

        bool hasZTest =
            HasProperty(original, "_ZTest");

        bool hasSrc =
            HasProperty(original, "_Src");

        bool hasDst =
            HasProperty(original, "_Dst");

        string renderType =
            opaque ? "Opaque" : "Transparent";

        string queue =
            opaque ? "Geometry" : "Transparent";

        StringBuilder sb = new StringBuilder();

        // MISMO nombre interno.
        sb.AppendLine($"Shader \"{shaderName}\"");
        sb.AppendLine("{");

        sb.AppendLine("    Properties");
        sb.AppendLine(properties);

        sb.AppendLine("    SubShader");
        sb.AppendLine("    {");

        sb.AppendLine("        Tags");
        sb.AppendLine("        {");
        sb.AppendLine(
            "            \"RenderPipeline\"=\"UniversalPipeline\"");
        sb.AppendLine(
            $"            \"RenderType\"=\"{renderType}\"");
        sb.AppendLine(
            $"            \"Queue\"=\"{queue}\"");
        sb.AppendLine("        }");

        if (hasCullProperty)
            sb.AppendLine("        Cull [_Cull]");
        else
            sb.AppendLine("        Cull Back");

        if (hasZWrite)
            sb.AppendLine("        ZWrite [_ZWrite]");
        else
            sb.AppendLine(
                opaque ? "        ZWrite On" : "        ZWrite Off");

        if (hasZTest)
            sb.AppendLine("        ZTest [_ZTest]");

        if (!opaque && hasSrc && hasDst)
            sb.AppendLine("        Blend [_Src] [_Dst]");

        sb.AppendLine();
        sb.AppendLine("        Pass");
        sb.AppendLine("        {");
        sb.AppendLine("            Name \"UniversalForward\"");
        sb.AppendLine(
            "            Tags { \"LightMode\"=\"UniversalForward\" }");
        sb.AppendLine();
        sb.AppendLine("            HLSLPROGRAM");
        sb.AppendLine("            #pragma target 3.5");
        sb.AppendLine("            #pragma vertex vert");
        sb.AppendLine("            #pragma fragment frag");
        sb.AppendLine(
            "            #pragma multi_compile_instancing");
        sb.AppendLine();
        sb.AppendLine(
            "            #include \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl\"");
        sb.AppendLine();

        sb.AppendLine(
@"            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv0        : TEXCOORD0;
                float4 uv1        : TEXCOORD1;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv0         : TEXCOORD0;
                float4 uv1         : TEXCOORD1;
                float4 color       : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct ASESurfaceOutput
            {
                half3 Albedo;
                half3 Normal;
                half3 Emission;
                half Metallic;
                half Smoothness;
                half Occlusion;
                half Alpha;
            };");

        sb.AppendLine();
        sb.AppendLine(Indent(cg, 12));
        sb.AppendLine();

        sb.AppendLine(
@"            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs pos =
                    GetVertexPositionInputs(input.positionOS.xyz);

                output.positionHCS = pos.positionCS;
                output.uv0 = input.uv0;
                output.uv1 = input.uv1;
                output.color = input.color;

                return output;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                Input i = (Input)0;");

        if (hasUV)
            sb.AppendLine(
                "                i.uv_texcoord = IN.uv0;");

        if (hasUV2)
            sb.AppendLine(
                "                i.uv2_texcoord2 = IN.uv1;");

        if (hasVertexColor)
            sb.AppendLine(
                "                i.vertexColor = IN.color;");

        sb.AppendLine(
@"                ASESurfaceOutput o =
                    (ASESurfaceOutput)0;

                o.Albedo = 0;
                o.Emission = 0;
                o.Alpha = 1;
                o.Occlusion = 1;
                o.Smoothness = 0;

                surf(i, o);

                half3 finalColor =
                    o.Albedo + o.Emission;

                return half4(finalColor, o.Alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}");

        return sb.ToString();
    }

    private string CleanAmplifyCG ( string cg )
    {
        // Quitamos includes específicos del Built-in RP.
        cg = Regex.Replace(
            cg,
            @"^\s*#include\s+""[^""]*\.cginc""\s*$",
            "",
            RegexOptions.Multiline
        );

        cg = Regex.Replace(
            cg,
            @"^\s*#pragma\s+surface.*$",
            "",
            RegexOptions.Multiline
        );

        cg = Regex.Replace(
            cg,
            @"^\s*#pragma\s+target.*$",
            "",
            RegexOptions.Multiline
        );

        cg = Regex.Replace(
            cg,
            @"^\s*#define\s+ASE_VERSION.*$",
            "",
            RegexOptions.Multiline
        );

        // LightingUnlit depende de SurfaceOutput de Built-in.
        cg = RemoveFunction(
            cg,
            "LightingUnlit"
        );

        cg = cg.Replace(
            "SurfaceOutputStandard",
            "ASESurfaceOutput"
        );

        cg = cg.Replace(
            "SurfaceOutput",
            "ASESurfaceOutput"
        );

        return cg.Trim();
    }

    private string ExtractCgSection ( string text )
    {
        int begin =
            text.IndexOf(
                "CGINCLUDE",
                StringComparison.Ordinal);

        int tokenLength;

        if (begin >= 0)
        {
            tokenLength = "CGINCLUDE".Length;
        }
        else
        {
            begin =
                text.IndexOf(
                    "CGPROGRAM",
                    StringComparison.Ordinal);

            tokenLength = "CGPROGRAM".Length;
        }

        if (begin < 0)
            return null;

        int contentStart = begin + tokenLength;

        int end =
            text.IndexOf(
                "ENDCG",
                contentStart,
                StringComparison.Ordinal);

        if (end < 0)
            return null;

        return text.Substring(
            contentStart,
            end - contentStart
        );
    }

    private string ExtractBraceBlockAfterWord (
        string text,
        string word )
    {
        int wordIndex =
            text.IndexOf(
                word,
                StringComparison.Ordinal);

        if (wordIndex < 0)
            return null;

        int open =
            text.IndexOf('{', wordIndex);

        if (open < 0)
            return null;

        int close =
            FindMatchingBrace(text, open);

        if (close < 0)
            return null;

        return text.Substring(
            open,
            close - open + 1
        );
    }

    private int FindMatchingBrace (
        string text,
        int openIndex )
    {
        int depth = 0;

        for (int i = openIndex; i < text.Length; i++)
        {
            if (text[i] == '{')
                depth++;
            else if (text[i] == '}')
            {
                depth--;

                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    private string RemoveFunction (
        string text,
        string functionName )
    {
        Match m =
            Regex.Match(
                text,
                @"\b" +
                Regex.Escape(functionName) +
                @"\s*\("
            );

        if (!m.Success)
            return text;

        int brace =
            text.IndexOf('{', m.Index);

        if (brace < 0)
            return text;

        int end =
            FindMatchingBrace(text, brace);

        if (end < 0)
            return text;

        // Buscamos el inicio de la línea/declaración.
        int start =
            text.LastIndexOf('\n', m.Index);

        if (start < 0)
            start = 0;

        return text.Remove(
            start,
            end - start + 1
        );
    }

    private bool HasProperty (
        string text,
        string property )
    {
        return Regex.IsMatch(
            text,
            @"(?m)^\s*" +
            Regex.Escape(property) +
            @"\s*\("
        );
    }

    private float ReadDefaultFloat (
        string text,
        string property,
        float fallback )
    {
        Match m = Regex.Match(
            text,
            Regex.Escape(property) +
            @"\s*\([^)]*\)\s*=\s*(-?\d+(?:\.\d+)?)"
        );

        if (!m.Success)
            return fallback;

        if (float.TryParse(
            m.Groups[1].Value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float value))
        {
            return value;
        }

        return fallback;
    }

    private string Indent (
        string text,
        int spaces )
    {
        string prefix =
            new string(' ', spaces);

        return string.Join(
            "\n",
            text
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(l => prefix + l)
        );
    }

    private void Restore (
        string path,
        string backup )
    {
        if (!File.Exists(backup))
            return;

        File.Copy(
            backup,
            path,
            true
        );

        AssetDatabase.ImportAsset(
            path,
            ImportAssetOptions.ForceSynchronousImport |
            ImportAssetOptions.ForceUpdate
        );
    }

    private Result Make (
        string path,
        AssetKind kind,
        Status status,
        string message )
    {
        return new Result
        {
            path = path,
            kind = kind,
            status = status,
            message = message
        };
    }
}