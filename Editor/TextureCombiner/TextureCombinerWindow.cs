using UnityEngine;
using UnityEditor;
using System.IO;

namespace TextureCombiner
{
    public class TextureCombinerWindow : EditorWindow
    {
        private Texture2D[] sourceTextures = new Texture2D[4];
        private ChannelMapping[] channelMappings = new ChannelMapping[4];
        private Texture2D outputTexture;
        private Vector2 scrollPosition;
        private TextureCombinerPresets presets;
        private TextureCombinerSettings settings;
        private string outputPath = "Assets/CombinedTexture.png";
        private string presetName = "New Preset";
        private int selectedPresetIndex = -1;

        [MenuItem("Window/Texture Combiner")]
        public static void ShowWindow()
        {
            GetWindow<TextureCombinerWindow>("Texture Combiner");
        }

        private void OnEnable()
        {
            presets = new TextureCombinerPresets();
            settings = TextureCombinerSettings.Load();

            // Initialize default channel mappings
            for (int i = 0; i < channelMappings.Length; i++)
            {
                channelMappings[i] = new ChannelMapping();
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Texture Channel Packer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawPresetSection();
            DrawSourceTexturesSection();
            DrawOutputSection();
            DrawPreviewSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawPresetSection()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);

            // Preset selection dropdown
            string[] presetNames = presets.GetPresetNames();
            int newSelectedIndex = EditorGUILayout.Popup("Load Preset", selectedPresetIndex, presetNames);
            if (newSelectedIndex != selectedPresetIndex)
            {
                selectedPresetIndex = newSelectedIndex;
                if (selectedPresetIndex >= 0)
                {
                    var preset = presets.GetPreset(presetNames[selectedPresetIndex]);
                    ApplyPreset(preset);
                }
            }

            EditorGUILayout.BeginHorizontal();
            presetName = EditorGUILayout.TextField("Preset Name", presetName);
            if (GUILayout.Button("Save Preset", GUILayout.Width(100)))
            {
                SaveCurrentPreset();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawSourceTexturesSection()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Source Textures", EditorStyles.boldLabel);

            for (int i = 0; i < sourceTextures.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // Texture slot
                sourceTextures[i] = (Texture2D)EditorGUILayout.ObjectField(
                    $"Texture {i + 1}",
                    sourceTextures[i],
                    typeof(Texture2D),
                    false);

                // Channel mapping dropdown
                if (sourceTextures[i] != null)
                {
                    channelMappings[i].outputChannel = (OutputChannel)EditorGUILayout.EnumPopup(channelMappings[i].outputChannel);
                    channelMappings[i].sourceChannel = (SourceChannel)EditorGUILayout.EnumPopup(channelMappings[i].sourceChannel);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawOutputSection()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Output Settings", EditorStyles.boldLabel);

            outputPath = EditorGUILayout.TextField("Output Path", outputPath);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                string path = EditorUtility.SaveFilePanel(
                    "Save Combined Texture",
                    Path.GetDirectoryName(outputPath),
                    Path.GetFileName(outputPath),
                    "png");

                if (!string.IsNullOrEmpty(path))
                {
                    outputPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }

            GUI.enabled = HasValidTextures();
            if (GUILayout.Button("Combine Textures", GUILayout.Width(120)))
            {
                CombineTextures();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewSection()
        {
            if (outputTexture == null) return;

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(256));
            EditorGUI.DrawPreviewTexture(r, outputTexture);

            EditorGUILayout.EndVertical();
        }

        private bool HasValidTextures()
        {
            foreach (var tex in sourceTextures)
            {
                if (tex != null) return true;
            }
            return false;
        }

        private void CombineTextures()
        {
            try
            {
                // Determine output size based on settings and source textures
                int width = settings.defaultOutputSize;
                int height = settings.defaultOutputSize;

                if (settings.useLargestInputSize)
                {
                    foreach (var tex in sourceTextures)
                    {
                        if (tex != null)
                        {
                            width = Mathf.Max(width, tex.width);
                            height = Mathf.Max(height, tex.height);
                        }
                    }
                }

                // Create output texture
                outputTexture = new Texture2D(width, height, TextureFormat.RGBA32, settings.generateMipMaps, settings.linearColorSpace);

                // Process each pixel
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color outputColor = Color.black;

                        for (int i = 0; i < sourceTextures.Length; i++)
                        {
                            if (sourceTextures[i] != null)
                            {
                                float normalizedX = (float)x / width;
                                float normalizedY = (float)y / height;

                                int srcX = Mathf.FloorToInt(normalizedX * sourceTextures[i].width);
                                int srcY = Mathf.FloorToInt(normalizedY * sourceTextures[i].height);

                                srcX = Mathf.Clamp(srcX, 0, sourceTextures[i].width - 1);
                                srcY = Mathf.Clamp(srcY, 0, sourceTextures[i].height - 1);

                                Color srcColor = sourceTextures[i].GetPixel(srcX, srcY);
                                float channelValue = GetChannelValue(srcColor, channelMappings[i].sourceChannel);

                                SetChannelValue(ref outputColor, channelMappings[i].outputChannel, channelValue);
                            }
                        }

                        outputTexture.SetPixel(x, y, outputColor);
                    }
                }

                outputTexture.Apply();

                // Save to file
                byte[] pngData = outputTexture.EncodeToPNG();
                File.WriteAllBytes(Application.dataPath + outputPath.Substring(6), pngData);
                AssetDatabase.Refresh();

                // Update texture import settings
                string assetPath = outputPath;
                TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    importer.textureType = settings.outputTextureType;
                    importer.sRGBTexture = !settings.linearColorSpace;
                    importer.mipmapEnabled = settings.generateMipMaps;
                    importer.filterMode = settings.filterMode;
                    importer.wrapMode = settings.wrapMode;
                    importer.maxTextureSize = settings.maxTextureSize;
                    importer.SaveAndReimport();
                }

                Debug.Log($"Texture successfully combined and saved to {outputPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error combining textures: {e.Message}");
            }
        }

        private float GetChannelValue(Color color, SourceChannel channel)
        {
            switch (channel)
            {
                case SourceChannel.Red: return color.r;
                case SourceChannel.Green: return color.g;
                case SourceChannel.Blue: return color.b;
                case SourceChannel.Alpha: return color.a;
                case SourceChannel.Grayscale: return color.grayscale;
                case SourceChannel.AverageRGB: return (color.r + color.g + color.b) / 3f;
                default: return 0f;
            }
        }

        private void SetChannelValue(ref Color color, OutputChannel channel, float value)
        {
            switch (channel)
            {
                case OutputChannel.Red: color.r = value; break;
                case OutputChannel.Green: color.g = value; break;
                case OutputChannel.Blue: color.b = value; break;
                case OutputChannel.Alpha: color.a = value; break;
            }
        }

        private void SaveCurrentPreset()
        {
            var preset = new TextureCombinerPreset
            {
                name = presetName,
                sourceTextures = new Texture2D[sourceTextures.Length],
                channelMappings = new ChannelMapping[channelMappings.Length]
            };

            System.Array.Copy(sourceTextures, preset.sourceTextures, sourceTextures.Length);
            System.Array.Copy(channelMappings, preset.channelMappings, channelMappings.Length);

            presets.SavePreset(preset);
            selectedPresetIndex = presets.GetPresetNames().Length - 1;
        }

        private void ApplyPreset(TextureCombinerPreset preset)
        {
            if (preset == null) return;

            System.Array.Copy(preset.sourceTextures, sourceTextures, Mathf.Min(preset.sourceTextures.Length, sourceTextures.Length));
            System.Array.Copy(preset.channelMappings, channelMappings, Mathf.Min(preset.channelMappings.Length, channelMappings.Length));
        }
    }

    [System.Serializable]
    public class ChannelMapping
    {
        public OutputChannel outputChannel = OutputChannel.Red;
        public SourceChannel sourceChannel = SourceChannel.Red;
    }

    public enum OutputChannel { Red, Green, Blue, Alpha }
    public enum SourceChannel { Red, Green, Blue, Alpha, Grayscale, AverageRGB }
}