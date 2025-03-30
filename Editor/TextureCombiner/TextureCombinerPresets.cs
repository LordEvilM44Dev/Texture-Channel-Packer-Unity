using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TextureCombiner
{
    [System.Serializable]
    public class TextureCombinerPreset
    {
        public string name;
        public Texture2D[] sourceTextures;
        public ChannelMapping[] channelMappings;
    }

    public class TextureCombinerPresets
    {
        private const string PRESETS_FOLDER = "Assets/Editor/TextureCombiner/Presets/";
        private const string PRESET_EXTENSION = ".tcpreset";

        private List<TextureCombinerPreset> presets = new List<TextureCombinerPreset>();

        public TextureCombinerPresets()
        {
            LoadAllPresets();
        }

        public string[] GetPresetNames()
        {
            string[] names = new string[presets.Count];
            for (int i = 0; i < presets.Count; i++)
            {
                names[i] = presets[i].name;
            }
            return names;
        }

        public TextureCombinerPreset GetPreset(string name)
        {
            return presets.Find(p => p.name == name);
        }

        public void SavePreset(TextureCombinerPreset preset)
        {
            if (string.IsNullOrEmpty(preset.name))
            {
                Debug.LogError("Preset name cannot be empty");
                return;
            }

            // Create directory if it doesn't exist
            if (!Directory.Exists(PRESETS_FOLDER))
            {
                Directory.CreateDirectory(PRESETS_FOLDER);
            }

            // Remove invalid characters from filename
            string fileName = preset.name;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c.ToString(), "");
            }

            string filePath = PRESETS_FOLDER + fileName + PRESET_EXTENSION;
            string json = JsonUtility.ToJson(preset, true);
            File.WriteAllText(filePath, json);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif

            // Reload all presets
            LoadAllPresets();
        }

        public void DeletePreset(string name)
        {
            string fileName = name;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c.ToString(), "");
            }

            string filePath = PRESETS_FOLDER + fileName + PRESET_EXTENSION;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);

#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif

                LoadAllPresets();
            }
        }

        private void LoadAllPresets()
        {
            presets.Clear();

            if (!Directory.Exists(PRESETS_FOLDER))
            {
                return;
            }

            string[] presetFiles = Directory.GetFiles(PRESETS_FOLDER, "*" + PRESET_EXTENSION);
            foreach (string filePath in presetFiles)
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    TextureCombinerPreset preset = JsonUtility.FromJson<TextureCombinerPreset>(json);
                    presets.Add(preset);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load preset {filePath}: {e.Message}");
                }
            }
        }
    }
}
