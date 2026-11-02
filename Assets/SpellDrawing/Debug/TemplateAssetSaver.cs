// "방금 그린 것을 도안으로 저장": 그린 획을 새 SpellTemplate 에셋으로 만들고 도안 목록에 넣는다. 에디터에서만 동작한다.
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VoxelMagic.SpellDrawing.DebugUI
{
    public static class TemplateAssetSaver
    {
        public const string TemplatesFolder = "Assets/SpellDrawing/Data/Templates";

        public static bool IsSupported =>
#if UNITY_EDITOR
            true;
#else
            false;
#endif

        // 저장한 에셋 경로를 돌려준다. 저장할 수 없으면 null.
        public static string Save(StrokeSet strokes, Color guideColor, SpellTemplateLibrary library)
        {
#if UNITY_EDITOR
            if (strokes == null || strokes.IsEmpty) return null;
            string stamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path = AssetDatabase.GenerateUniqueAssetPath($"{TemplatesFolder}/Custom_{stamp}.asset");

            var template = ScriptableObject.CreateInstance<SpellTemplate>();
            template.SetData($"custom_{stamp}", $"직접 그린 도안 {stamp}", strokes, guideColor);
            AssetDatabase.CreateAsset(template, path);
            if (library != null)
            {
                library.Add(template);
                EditorUtility.SetDirty(library);
            }
            AssetDatabase.SaveAssets();
            return path;
#else
            return null;
#endif
        }
    }
}
