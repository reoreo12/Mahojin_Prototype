// Tools/SpellDrawing 메뉴: 기본 설정·자리표시 도안 에셋 만들기, 모듈 폴더를 unitypackage로 내보내기.
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VoxelMagic.SpellDrawing.EditorTools
{
    public static class SpellDrawingMenu
    {
        public const string ModuleRoot = "Assets/SpellDrawing";
        public const string PackageVersion = "0.1.0";
        private const string TemplatesFolder = ModuleRoot + "/Data/Templates";
        private const string SettingsFolder = ModuleRoot + "/Data/Settings";

        [MenuItem("Tools/SpellDrawing/Create Default Assets")]
        public static void CreateDefaultAssets()
        {
            var template = LoadOrCreate<SpellTemplate>(TemplatesFolder + "/CapturePlaceholder.asset", out bool created);
            if (created)
            {
                template.SetData(PlaceholderTemplateFactory.Id, PlaceholderTemplateFactory.DisplayName,
                    PlaceholderTemplateFactory.CreateDefault(), new Color(0.45f, 0.8f, 1f, 1f));
                EditorUtility.SetDirty(template);
            }

            var library = LoadOrCreate<SpellTemplateLibrary>(TemplatesFolder + "/SpellTemplateLibrary.asset", out _);
            library.Add(template);
            EditorUtility.SetDirty(library);

            LoadOrCreate<DrawingSessionSettings>(SettingsFolder + "/DrawingSessionSettings.asset", out _);
            LoadOrCreate<JudgeSettings>(SettingsFolder + "/JudgeSettings.asset", out _);

            AssetDatabase.SaveAssets();
            Debug.Log("[SpellDrawing] 기본 에셋 준비 완료");
        }

        [MenuItem("Tools/SpellDrawing/Export Package")]
        public static void ExportPackage()
        {
            string fileName = $"SpellDrawing_v{PackageVersion}.unitypackage";
            AssetDatabase.ExportPackage(ModuleRoot, fileName, ExportPackageOptions.Recurse);
            Debug.Log($"[SpellDrawing] 내보내기 완료: {Path.GetFullPath(fileName)}");
        }

        private static T LoadOrCreate<T>(string path, out bool created) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            created = asset == null;
            if (!created) return asset;

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
