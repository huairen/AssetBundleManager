using UnityEditor;
using UnityEngine;

class FBXPostprocessor : AssetPostprocessor
{
    // This method is called just before importing an FBX.
    void OnPreprocessModel()
    {
        Debug.Log("OnPreprocessModel " + assetPath);
        ModelImporter mi = (ModelImporter)assetImporter;
        mi.globalScale = 1;
        if (!assetPath.Contains("/characters/")) return;
        mi.animationCompression = ModelImporterAnimationCompression.Off;
        mi.animationType = ModelImporterAnimationType.Legacy;

        // Materials for characters are created using the GenerateMaterials script.
        mi.importMaterials = false;
    }

    // This method is called immediately after importing an FBX.
    void OnPostprocessModel(GameObject go)
    {
        Debug.Log("OnPostprocessModel " + assetPath);
     
        if (!assetPath.Contains("/characters/")) return;

        // Assume an animation FBX has an @ in its name,
        // to determine if an fbx is a character or an animation.
        if (assetPath.Contains("@"))
        {
            // For animation FBX's all unnecessary Objects are removed.
            // This is not required but improves clarity when browsing assets.

            // Remove SkinnedMeshRenderers and their meshes.
            foreach (SkinnedMeshRenderer smr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Object.DestroyImmediate(smr.sharedMesh, true);
                Object.DestroyImmediate(smr.gameObject);
            }

            // Remove the bones.
            foreach (Transform o in go.transform)
            {
                if (o.parent.gameObject != go)
                    Object.DestroyImmediate(o);
            }
        }
    }

    void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        Debug.Log("OnPostprocessModel " + assetPath);

        foreach (string asset in importedAssets)
            Debug.Log(asset);
        foreach (string asset in deletedAssets)
            Debug.Log(asset);
        foreach (string asset in movedAssets)
            Debug.Log(asset);
        foreach (string asset in movedFromAssetPaths)
            Debug.Log(asset);
    }
}