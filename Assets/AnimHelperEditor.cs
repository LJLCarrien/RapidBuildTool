using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

public class AnimHelperEditor : Editor
{

    [MenuItem("Assets/一键测试")]
    private static void Test()
    {
        var obj = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(obj);
        Debug.LogError(path);

        var animPath = path + "/Anim";
        DirectoryInfo direction = new DirectoryInfo(animPath);

        FileInfo[] files = direction.GetFiles("*.overrideController", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; i++)
        {
            var fileFullName = files[i].FullName;
            fileFullName = fileFullName.Replace("\\", "/");
            Debug.LogError(fileFullName);
            animPath = fileFullName.Replace(Application.dataPath, "Assets");
        }

        var anim = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(animPath);
        var prefabPath = path + "/Prefab";

        var guids = AssetDatabase.FindAssets("t:prefab", new string[] { prefabPath });
        for (int i = 0; i < guids.Length; i++)
        {
            var itemPath = AssetDatabase.GUIDToAssetPath(guids[i]);

            var go = AssetDatabase.LoadAssetAtPath<GameObject>(itemPath);
            var prefabAnim = go.GetComponent<Animator>();
            prefabAnim.runtimeAnimatorController = anim;

        }
    }
}
