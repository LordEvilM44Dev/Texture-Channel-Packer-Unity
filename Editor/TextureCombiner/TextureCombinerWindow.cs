using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;

/*

    https://github.com/LordEvilM44Dev

*/

namespace TextureCombiner
{
    public class TextureCombinerWindow : EditorWindow
    {
        private Texture2D[] sourceTextures = new Texture2D[4];
        private ChannelMapping[] channelMappings = new ChannelMapping[4];
        private Texture2D outputTexture;
        private Vector2 scrollPosition;
        private string outputPath = "Assets/CombinedTexture.png";
        private string presetName = "New Preset";
        private int selectedPresetIndex = -1;
        private bool showAdvancedSettings = false;

        private int defaultOutputSize = 1024;
        private bool useLargestInputSize = true;
        private bool generateMipMaps = true;
        private bool linearColorSpace = false;

        private TextureCombinerPresets presetsManager;
        private string[] presetNames;
        private int selectedPresetToDelete = -1;

        private GUIStyle sectionHeaderStyle;
        private GUIStyle boxStyle;
        private bool stylesInitialized = false;

        [MenuItem("Window/Texture Combiner")]
        public static void ShowWindow()
        {
            GetWindow<TextureCombinerWindow>("Texture Combiner");
        }

        private void OnEnable()
        {
            for (int i = 0; i < channelMappings.Length; i++)
            {
                channelMappings[i] = new ChannelMapping();
            }

            presetsManager = new TextureCombinerPresets();
            RefreshPresetNames();
        }

        private void RefreshPresetNames()
        {
            presetNames = presetsManager.GetPresetNames();
            if (presetNames.Length > 0)
            {
                selectedPresetIndex = 0;
            }
            else
            {
                selectedPresetIndex = -1;
            }
        }

        private void InitializeStyles()
        {
            sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                padding = new RectOffset(0, 0, 5, 5)
            };

            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!stylesInitialized)
            {
                InitializeStyles();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Texture Channel Packer", EditorStyles.largeLabel);
            EditorGUILayout.HelpBox("Combine up to 4 textures into a single RGBA texture by mapping channels.", MessageType.Info);
            EditorGUILayout.Space();

            DrawPresetSection();
            DrawSourceTexturesSection();
            DrawOutputSection();
            DrawPreviewSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawPresetSection()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("Presets", sectionHeaderStyle);

            EditorGUILayout.BeginHorizontal();
            {
                if (presetNames.Length > 0)
                {
                    selectedPresetIndex = EditorGUILayout.Popup(selectedPresetIndex, presetNames);
                    if (GUILayout.Button("Load", GUILayout.Width(60)))
                    {
                        LoadSelectedPreset();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No presets available", EditorStyles.helpBox);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                presetName = EditorGUILayout.TextField("Preset Name", presetName, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Save", GUILayout.Width(60)))
                {
                    SaveCurrentPreset();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (presetNames.Length > 0)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    selectedPresetToDelete = EditorGUILayout.Popup(selectedPresetToDelete, presetNames);
                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Preset",
                            $"Are you sure you want to delete '{presetNames[selectedPresetToDelete]}'?",
                            "Delete", "Cancel"))
                        {
                            presetsManager.DeletePreset(presetNames[selectedPresetToDelete]);
                            RefreshPresetNames();
                            selectedPresetToDelete = Mathf.Clamp(selectedPresetToDelete, 0, presetNames.Length - 1);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void LoadSelectedPreset()
        {
            if (selectedPresetIndex >= 0 && selectedPresetIndex < presetNames.Length)
            {
                var preset = presetsManager.GetPreset(presetNames[selectedPresetIndex]);
                if (preset != null)
                {
                    Array.Copy(preset.sourceTextures, sourceTextures, Mathf.Min(preset.sourceTextures.Length, sourceTextures.Length));
                    Array.Copy(preset.channelMappings, channelMappings, Mathf.Min(preset.channelMappings.Length, channelMappings.Length));
                    presetName = preset.name;
                }
            }
        }

        private void SaveCurrentPreset()
        {
            var preset = new TextureCombinerPreset
            {
                name = presetName,
                sourceTextures = (Texture2D[])sourceTextures.Clone(),
                channelMappings = (ChannelMapping[])channelMappings.Clone()
            };

            presetsManager.SavePreset(preset);
            RefreshPresetNames();

            for (int i = 0; i < presetNames.Length; i++)
            {
                if (presetNames[i] == preset.name)
                {
                    selectedPresetIndex = i;
                    selectedPresetToDelete = i;
                    break;
                }
            }
        }

        private void DrawSourceTexturesSection()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("Source Textures", sectionHeaderStyle);

            for (int i = 0; i < sourceTextures.Length; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Texture Slot {i + 1}", EditorStyles.boldLabel);

                sourceTextures[i] = (Texture2D)EditorGUILayout.ObjectField(
                    "Texture",
                    sourceTextures[i],
                    typeof(Texture2D),
                    false);

                if (sourceTextures[i] != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Map", GUILayout.Width(40));
                        channelMappings[i].sourceChannel = (SourceChannel)EditorGUILayout.EnumPopup(channelMappings[i].sourceChannel);
                        EditorGUILayout.LabelField("to", GUILayout.Width(20));
                        channelMappings[i].outputChannel = (OutputChannel)EditorGUILayout.EnumPopup(channelMappings[i].outputChannel);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Clear All", GUILayout.Width(100)))
                {
                    ClearAllTextures();
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawOutputSection()
        {
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("Output Settings", sectionHeaderStyle);

            EditorGUILayout.BeginHorizontal();
            {
                outputPath = EditorGUILayout.TextField("Output Path", outputPath);
                if (GUILayout.Button("...", GUILayout.Width(30)))
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
            }
            EditorGUILayout.EndHorizontal();

            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced Settings");
            if (showAdvancedSettings)
            {
                EditorGUI.indentLevel++;
                defaultOutputSize = EditorGUILayout.IntField("Default Size", defaultOutputSize);
                useLargestInputSize = EditorGUILayout.Toggle("Use Largest Input Size", useLargestInputSize);
                generateMipMaps = EditorGUILayout.Toggle("Generate Mip Maps", generateMipMaps);
                linearColorSpace = EditorGUILayout.Toggle("Linear Color Space", linearColorSpace);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(!HasValidTextures());
                if (GUILayout.Button("Combine Textures", GUILayout.Height(30)))
                {
                    CombineTextures();
                }
                EditorGUI.EndDisabledGroup();

                if (!HasValidTextures())
                {
                    EditorGUILayout.HelpBox("Add at least one texture to combine", MessageType.Warning);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewSection()
        {
            if (outputTexture == null) return;

            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField("Preview", sectionHeaderStyle);

            EditorGUILayout.LabelField($"Size: {outputTexture.width}x{outputTexture.height}");

            float aspect = (float)outputTexture.width / outputTexture.height;
            float previewHeight = Mathf.Min(256, position.width / aspect);

            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(previewHeight));
            EditorGUI.DrawPreviewTexture(r, outputTexture, null, ScaleMode.ScaleToFit);

            EditorGUILayout.EndVertical();
        }

        private void ClearAllTextures()
        {
            for (int i = 0; i < sourceTextures.Length; i++)
            {
                sourceTextures[i] = null;
                channelMappings[i] = new ChannelMapping();
            }
            outputTexture = null;
        }

        private bool HasValidTextures()
        {
            bool hasTexture = false;
            for (int i = 0; i < sourceTextures.Length; i++)
            {
                if (sourceTextures[i] != null)
                {
                    if (!sourceTextures[i].isReadable)
                    {
                        EditorGUILayout.HelpBox($"Texture {i + 1} is not readable. Enable 'Read/Write' in import settings.", MessageType.Error);
                        return false;
                    }
                    hasTexture = true;
                }
            }
            return hasTexture;
        }

        private void CombineTextures()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Combining Textures", "Initializing...", 0);

                int width = defaultOutputSize;
                int height = defaultOutputSize;

                if (useLargestInputSize)
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

                outputTexture = new Texture2D(width, height, TextureFormat.RGBA32, generateMipMaps, linearColorSpace);

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

                    if (y % 10 == 0)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("Combining Textures", "Processing pixels...", (float)y / height))
                        {
                            break;
                        }
                    }
                }

                outputTexture.Apply();

                byte[] pngData = outputTexture.EncodeToPNG();
                File.WriteAllBytes(Application.dataPath + outputPath.Substring(6), pngData);
                AssetDatabase.Refresh();

                Debug.Log($"Texture successfully combined and saved to {outputPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error combining textures: {e.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
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