using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEditorInternal;

public class CreateAssetbundles
{
    static string ASSETBUNDLE_EXT = ".assetbundle";
    static string SCENE_EXT = ".unity3d";

    [MenuItem("Assets/AssetMgr/Create AssetBunldes/Alone")]
    static void CreateAssetBunldesForAlone()
    {
        if (Directory.Exists(Application.streamingAssetsPath) == false)
            Directory.CreateDirectory(Application.streamingAssetsPath);

        string share_name = "Share" + ASSETBUNDLE_EXT;

        //共同依赖资源列表
        List<string> share_depList = new List<string>();
        //所有依赖资源列表
        List<string> depList = new List<string>();

        //查找共同依赖的资源
        Object[] SelectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        foreach (Object obj in SelectedAsset)
        {
            string prefPath = AssetDatabase.GetAssetPath(obj);
            if (Directory.Exists(prefPath))
            {
                share_name = obj.name + share_name;
                continue;
            }

            string[] depPath = AssetDatabase.GetDependencies(new string[1] { prefPath });
            foreach (string dep in depPath)
            {
                if (dep == prefPath)
                    continue;

                if (!depList.Contains(dep))
                    depList.Add(dep);
                else if (!share_depList.Contains(dep))
                    share_depList.Add(dep);
            }
        }

        //先打包共同依赖资源
        if(share_depList.Count > 0)
        {
            List<Object> objs = new List<Object>();
            foreach (string path in share_depList)
            {
                objs.Add(AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
            }

            BuildPipeline.PushAssetDependencies();
            string targetPath = Application.streamingAssetsPath + "/" + share_name;
            BuildPipeline.BuildAssetBundle(null, objs.ToArray(), targetPath,
                BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
                EditorUserBuildSettings.activeBuildTarget);

            BuildPipeline.PushAssetDependencies();
        }

        BundleXml xml = new BundleXml();
        xml.Open();

        foreach(Object obj in SelectedAsset)
        {
            if (Directory.Exists(AssetDatabase.GetAssetPath(obj)))
                continue;

            string targetPath = Application.streamingAssetsPath + "/" + obj.name + ASSETBUNDLE_EXT;
            if (BuildPipeline.BuildAssetBundle(null, new Object[]{obj}, targetPath,
                BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
                EditorUserBuildSettings.activeBuildTarget))
            {
                xml.UpdateAssetBundle(obj.name + ASSETBUNDLE_EXT);
                xml.UpdateAsset(obj.name, share_depList.Count > 0 ? new string[] {share_name} : null);
                Debug.Log("Saved " + obj.name + ". (" + targetPath + ")");
            }
        }

        xml.Close();

        if (share_depList.Count > 0)
        {
            BuildPipeline.PopAssetDependencies();
            BuildPipeline.PopAssetDependencies();
        }
        
        //刷新编辑器
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/AssetMgr/Create AssetBunldes/One")]
    static void CreateAssetBunldesForOne()
    {
        Caching.CleanCache();
        string bundleName;
		string objs_name = null;
		List<Object> saveList = new List<Object>();

        Object[] SelectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        bundleName = SelectedAsset[0].name;

        //用文件夹名字命名，但不保存文件夹对象
        foreach (Object obj in SelectedAsset)
        {
            if (Directory.Exists(AssetDatabase.GetAssetPath(obj)))
            {
                bundleName = obj.name;
            }
            else
            {
                if (objs_name != null)
                    objs_name += ",";
                objs_name += obj.name;
                saveList.Add(obj);
            }
        }

        if (!Directory.Exists(Application.streamingAssetsPath))
            Directory.CreateDirectory(Application.streamingAssetsPath);

        //加入打包列表
		//AssetDatabase.ImportAsset("Assets/StreamingAssets/objects_names.txt", ImportAssetOptions.ForceSynchronousImport);
		//TextAsset text_asset = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/StreamingAssets/objects_names.txt", typeof(TextAsset));
        //saveList.Add(text_asset);

        //编译打包
        string path = Application.streamingAssetsPath + "/" + bundleName + ASSETBUNDLE_EXT;
        Object[] objs = saveList.ToArray();

        if (BuildPipeline.BuildAssetBundle(null, objs, path,
                BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
                EditorUserBuildSettings.activeBuildTarget))
        {
            BundleXml xml = new BundleXml();
            xml.Open();
            xml.UpdateAssetBundle(bundleName + ASSETBUNDLE_EXT);

            foreach (Object obj in objs)
            {
                xml.UpdateAsset(obj.name, null);
            }

            xml.Close();

            Debug.Log("Saved " + objs_name + ". (" + path + ")");
        }

        //删除对象名文件
        //AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(text_asset));
        //UnityEngine.Object.DestroyImmediate(text_asset);
    }

    [MenuItem("Assets/AssetMgr/Create Scene")]
    static void CreateScene()
    {
        string exportPath = Application.streamingAssetsPath;
        if (Directory.Exists(exportPath) == false)
            Directory.CreateDirectory(exportPath);

        string currentScene = EditorApplication.currentScene;
        string currentSceneName = currentScene.Substring(currentScene.LastIndexOf('/') + 1, currentScene.LastIndexOf('.') - currentScene.LastIndexOf('/') - 1);
        string fileName = exportPath + "/" + currentSceneName + SCENE_EXT;
        BuildPipeline.BuildStreamedSceneAssetBundle(new string[1] { EditorApplication.currentScene }, fileName, EditorUserBuildSettings.activeBuildTarget);

        BundleXml xml = new BundleXml();
        xml.Open();
        xml.UpdateAssetBundle(currentSceneName + SCENE_EXT);
        xml.UpdateAsset(currentSceneName, null);
        xml.Close();
    }

    [MenuItem("Assets/AssetMgr/Create Character")]
    static void CreateCharacter()
    {
        bool createdBundle = false;

        // 创建一个文件夹保存生成的assetbundles.
        string exportPath = Application.streamingAssetsPath + "/character/";
        if (!Directory.Exists(exportPath))
            Directory.CreateDirectory(exportPath);

        BundleXml xml = new BundleXml();
        xml.Open();

        foreach (Object o in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        {
            if (!(o is GameObject)) continue;
            if (o.name.Contains("@")) continue;
            if (!AssetDatabase.GetAssetPath(o).Contains("/characters/")) continue;

            GameObject characterFBX = (GameObject)o;
            string name = characterFBX.name.ToLower();

            Debug.Log("******* Creating assetbundles for: " + name + " *******");

            GameObject characterClone = (GameObject)Object.Instantiate(characterFBX);

            foreach (Animation anim in characterClone.GetComponentsInChildren<Animation>())
                anim.cullingType = AnimationCullingType.BasedOnClipBounds;

            // 把网格对象删除只留下骨架.
            foreach (SkinnedMeshRenderer smr in characterClone.GetComponentsInChildren<SkinnedMeshRenderer>())
                Object.DestroyImmediate(smr.gameObject);

            // 把骨架保存到assetbundle.
            string assetName = name;// +"_characterbase";
            string bundleName = assetName + ASSETBUNDLE_EXT;
            string path = exportPath + bundleName;

            characterClone.AddComponent<SkinnedMeshRenderer>();
            Object characterBasePrefab = GetPrefab(characterClone, assetName);

            BuildPipeline.BuildAssetBundle(null, new Object[]{characterBasePrefab}, path, BuildAssetBundleOptions.CollectDependencies, EditorUserBuildSettings.activeBuildTarget);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(characterBasePrefab));

            xml.UpdateAssetBundle("character/" + bundleName);
            xml.UpdateAsset(assetName, null);

            // 收集角色文件夹下的所有材质.
            List<Material> materials = EditorHelpers.CollectAll<Material>(GenerateMaterials.MaterialsPath(characterFBX));

            // 把每个SkinnedMeshRenderer单独保存到assetbundle.
            foreach (SkinnedMeshRenderer smr in characterFBX.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                List<Object> toinclude = new List<Object>();

                // Save the current SkinnedMeshRenderer as a prefab so it can be included
                // in the assetbundle. As instantiating part of an fbx results in the
                // entire fbx being instantiated, we have to dispose of the entire instance
                // after we detach the SkinnedMeshRenderer in question.
                GameObject rendererClone = (GameObject)PrefabUtility.InstantiatePrefab(smr.gameObject);
                GameObject rendererParent = rendererClone.transform.parent.gameObject;
                rendererClone.transform.parent = null;
                Object.DestroyImmediate(rendererParent);
                Object rendererPrefab = GetPrefab(rendererClone, "rendererobject");
                toinclude.Add(rendererPrefab);

                // 收集对应的材质
                foreach (Material m in materials)
                    if (m.name.Contains(smr.name.ToLower())) toinclude.Add(m);

                // When assembling a character, we load SkinnedMeshRenderers from assetbundles,
                // and as such they have lost the references to their bones. To be able to
                // remap the SkinnedMeshRenderers to use the bones from the characterbase assetbundles,
                // we save the names of the bones used.
                List<string> boneNames = new List<string>();
                foreach (Transform t in smr.bones)
                    boneNames.Add(t.name);
                string stringholderpath = "Assets/bonenames.asset";

                StringHolder holder = ScriptableObject.CreateInstance<StringHolder>();
                holder.content = boneNames.ToArray();
                AssetDatabase.CreateAsset(holder, stringholderpath);
                toinclude.Add(AssetDatabase.LoadAssetAtPath(stringholderpath, typeof(StringHolder)));

                // Save the assetbundle.
                bundleName = name + "_" + smr.name.ToLower();
                path = exportPath + bundleName + ASSETBUNDLE_EXT;
                BuildPipeline.BuildAssetBundle(null, toinclude.ToArray(), path, BuildAssetBundleOptions.CollectDependencies, EditorUserBuildSettings.activeBuildTarget);
                Debug.Log("Saved " + bundleName + " with " + (toinclude.Count - 2) + " materials");

                xml.UpdateAssetBundle("character/" + bundleName + ASSETBUNDLE_EXT);
                foreach(Object m in toinclude)
                {
                    if(m is Material)
                        xml.UpdateAsset((m as Material).name, null);
                }

                // Delete temp assets.
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(rendererPrefab));
                AssetDatabase.DeleteAsset(stringholderpath);
                createdBundle = true;
            }
        }

        xml.Close();

        if (!createdBundle)
            EditorUtility.DisplayDialog("Character Generator", "No Asset Bundles created. Select the characters folder in the Project pane to process all characters. Select subfolders to process specific characters.", "Ok");
    }
    static Object GetPrefab(GameObject go, string name)
    {
        Object tempPrefab = PrefabUtility.CreateEmptyPrefab("Assets/" + name + ".prefab");
        tempPrefab = PrefabUtility.ReplacePrefab(go, tempPrefab);
        Object.DestroyImmediate(go);
        return tempPrefab;
    }
}
