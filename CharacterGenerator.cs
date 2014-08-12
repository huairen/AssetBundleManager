using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class CharacterGenerator
{
    GameObject root;
    string currentCharacter;
    Dictionary<string, CharacterElement> currentConfiguration = new Dictionary<string, CharacterElement>();

    public Action OnCharacterLoaded;


    public static CharacterGenerator CreateWithConfig(string config)
    {
        CharacterGenerator gen = new CharacterGenerator();
        gen.PrepareConfig(config);
        return gen;
    }

    public void PrepareConfig(string config)
    {
        config = config.ToLower();
        string[] settings = config.Split('|');
        currentCharacter = settings[0];
        Asset asset = AssetMgr.Load(currentCharacter);
        asset.OnAssetLoaded += delegate(Object obj)
        {
            root = (GameObject)GameObject.Instantiate(obj);
        };


        currentConfiguration = new Dictionary<string, CharacterElement>();
        for (int i = 1; i < settings.Length; )
        {
            string elementName = settings[i++];
            asset = AssetMgr.Load(elementName);
            if (asset.LoadAssetAsync == null)
            {
                asset.LoadAssetAsync = delegate(Bundle bundle)
                {
                    return new CharacterRequest(elementName, bundle);
                };
            }

            asset.OnAssetLoaded += delegate(Object obj)
            {
                currentConfiguration.Add(elementName, obj as CharacterElement);
                if ((OnCharacterLoaded != null) && (currentConfiguration.Count == settings.Length - 1))
                    OnCharacterLoaded();
            };
        }
    }
    public GameObject Generate()
    {
        return Generate(root);
    }

    public GameObject Generate(GameObject root)
    {
        // The SkinnedMeshRenderers that will make up a character will be
        // combined into one SkinnedMeshRenderers using multiple materials.
        // This will speed up rendering the resulting character.
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        List<Material> materials = new List<Material>();
        List<Transform> bones = new List<Transform>();
        Transform[] transforms = root.GetComponentsInChildren<Transform>();

        foreach (CharacterElement element in currentConfiguration.Values)
        {
            SkinnedMeshRenderer smr = element.GetSkinnedMeshRenderer();
            materials.AddRange(smr.materials);
            for (int sub = 0; sub < smr.sharedMesh.subMeshCount; sub++)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = smr.sharedMesh;
                ci.subMeshIndex = sub;
                combineInstances.Add(ci);
            }

            // As the SkinnedMeshRenders are stored in assetbundles that do not
            // contain their bones (those are stored in the characterbase assetbundles)
            // we need to collect references to the bones we are using
            foreach (string bone in element.GetBoneNames())
            {
                foreach (Transform transform in transforms)
                {
                    if (transform.name != bone) continue;
                    bones.Add(transform);
                    break;
                }
            }

            Object.Destroy(smr.gameObject);
        }

        // Obtain and configure the SkinnedMeshRenderer attached to
        // the character base.
        SkinnedMeshRenderer r = root.GetComponent<SkinnedMeshRenderer>();
        r.sharedMesh = new Mesh();
        r.sharedMesh.CombineMeshes(combineInstances.ToArray(), false, false);
        r.bones = bones.ToArray();
        r.materials = materials.ToArray();

        return root;
    }

}
