using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateSpinePrefab 
{
    [MenuItem("Assets/Create Spine Prefab _g", false, 0)]
    static void ShowInScene()
    {
        AssetDatabase.Refresh();
        Debug.Log("Create Spine Prefab");
        Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets | SelectionMode.ExcludePrefab);

        bool hasMaterial = false, hasJson = false, hasSkeletonDataAssets = false, hasTexture = false, hasSpineAtlasAssets = false;
        UnityEditor.DefaultAsset folder = null;
        Spine.Unity.SkeletonDataAsset skeletonDataAsset = null;
        Spine.Unity.SpineAtlasAsset spineAtlasAsset = null;
        bool fileChanged = false;
        // 修改.atlas的错误文件后缀名为.atlas.txt
        for (int i = 0; i < objs.Length; ++i)
        {
            if (Path.GetExtension(AssetDatabase.GetAssetPath(objs[i])) == ".atlas")
            {
                string path = AssetDatabase.GetAssetPath(objs[i]);
                File.Move(path, path + ".txt");
                fileChanged = true;
            }
        }
        if(fileChanged)
            AssetDatabase.Refresh();

        // 重新获取，判定资源全部都有了
        objs = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets | SelectionMode.ExcludePrefab);
        for (int i = 0; i < objs.Length; ++i)
        {
            // Debug.Log(objs[i].GetType().ToString());
            if (objs[i].GetType() == typeof(Spine.Unity.SkeletonDataAsset))
            {
                skeletonDataAsset = objs[i] as Spine.Unity.SkeletonDataAsset;
                hasSkeletonDataAssets = true;
            }
            if (objs[i].GetType() == typeof(UnityEngine.TextAsset))
            {
                var item = objs[i] as TextAsset;
                //Debug.Log("TextAsset asset: " + item.name);
                hasJson = true;
            }

            if (objs[i].GetType() == typeof(UnityEngine.Material))
                hasMaterial = true;
            if (objs[i].GetType() == typeof(UnityEngine.Texture2D))
                hasTexture = true;
            if (objs[i].GetType() == typeof(Spine.Unity.SpineAtlasAsset))
            {
                spineAtlasAsset = objs[i] as Spine.Unity.SpineAtlasAsset;
                hasSpineAtlasAssets = true;
            }
            if (objs[i].GetType() == typeof(UnityEditor.DefaultAsset))
            {
                folder = objs[i] as UnityEditor.DefaultAsset;
                //Debug.Log("default asset: " + folder.name);
            }

        }
        if (!hasMaterial || !hasSkeletonDataAssets || !hasJson || !hasTexture || !hasSpineAtlasAssets)
        {
            Debug.LogErrorFormat("File ERROR material {0} json {1} texture {2} spineatlasassets {3} skeletondataassets {4}",
                    hasMaterial, hasJson, hasTexture, hasSpineAtlasAssets, hasSkeletonDataAssets);
            return;
        }
        // 对material关联另一个shader
        for (int i = 0; i < objs.Length; ++i)
        {
            if (objs[i].GetType() == typeof(UnityEngine.Material))
            {
                var material = objs[i] as UnityEngine.Material;
                material.shader = Shader.Find("SM/Spine/Skeleton");
            }
        }
        // 对SkeletonDataAsset关联AtlasAsset
        for (int i = 0; i < objs.Length; ++i)
        {
            if (objs[i].GetType() == typeof(Spine.Unity.SkeletonDataAsset))
            {
                skeletonDataAsset = objs[i] as Spine.Unity.SkeletonDataAsset;
                skeletonDataAsset.atlasAssets.SetValue(spineAtlasAsset, 0);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        GameObject go = new GameObject(folder.name);
        Spine.Unity.SkeletonAnimation.AddToGameObject(go, skeletonDataAsset);
        go.layer = LayerMask.NameToLayer("Object");
        string folderPath = AssetDatabase.GetAssetPath(folder);
        PrefabUtility.SaveAsPrefabAsset(go, folderPath + "/" + folder.name + ".prefab");
        GameObject.DestroyImmediate(go);
        Debug.Log("Create Spine Prefab succ");
    }
}
