using UnityEngine;
using UnityEditor;
using System.IO;

/*

    https://github.com/LordEvilM44Dev

*/

namespace TextureCombiner
{
    public class TextureCombinerSettings : ScriptableObject
    {
        private const string SETTINGS_PATH = "Assets/Editor/TextureCombiner/TextureCombinerSettings.asset";

        public int defaultOutputSize = 1024;
        public bool useLargestInputSize = true;
        public bool generateMipMaps = true;
        public bool linearColorSpace = false;
        public TextureImporterType outputTextureType = TextureImporterType.Default;
        public FilterMode filterMode = FilterMode.Bilinear;
        public TextureWrapMode wrapMode = TextureWrapMode.Repeat;
        public int maxTextureSize = 2048;

        public static TextureCombinerSettings Load()
        {
            var settings = AssetDatabase.LoadAssetAtPath<TextureCombinerSettings>(SETTINGS_PATH);

            if (settings == null)
            {
                settings = CreateInstance<TextureCombinerSettings>();

                string directory = Path.GetDirectoryName(SETTINGS_PATH);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        public static void OpenSettingsWindow()
        {
            var settings = Load();
            Selection.activeObject = settings;
            EditorUtility.FocusProjectWindow();
        }
    }

    [CustomEditor(typeof(TextureCombinerSettings))]
    public class TextureCombinerSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Texture Combiner Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Save Settings"))
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
        }
    }
}