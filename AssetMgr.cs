using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Object = UnityEngine.Object;

static public class AssetMgr
{
    static public readonly string LOCAL_ASSET_URL =
#if UNITY_ANDROID
        "jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IPHONE
        Application.dataPath + "/Raw/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
        "file://" + Application.streamingAssetsPath + "/";
#else
        string.Empty;
#endif


	static Dictionary<string, Asset> assetDict = new Dictionary<string,Asset>();
    static Dictionary<string, Bundle> bundleDict = new Dictionary<string, Bundle>();
    static Queue<Asset> loadQueue = new Queue<Asset>();
    static MonoBehaviour coroutineProvider = null;

    static public Action<float> OnProgress;


    static public IEnumerator Init(MonoBehaviour main, Action onFinish)
    {
        coroutineProvider = main;

        using (WWW www = new WWW(LOCAL_ASSET_URL + "assetinfo.xml"))
        {
            yield return www;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(new StringReader(www.text));
            XmlNode root = xmlDoc.SelectSingleNode("AssetBundles");
            if (root != null)
            {
                foreach (XmlNode xn1 in root.ChildNodes)
                {
                    XmlElement bundleEle = (XmlElement)xn1;
                    string name = bundleEle.GetAttribute("name");
                    Bundle bundle;
                    if (bundleDict.TryGetValue(name, out bundle))
                    {
                        bundle.md5 = bundleEle.GetAttribute("md5");
                    }
                    else
                    {
                        bundle = new Bundle(name, bundleEle.GetAttribute("md5"));
                        bundleDict.Add(name, bundle);
                    }

                    foreach (XmlNode xn2 in bundleEle.ChildNodes)
                    {
                        XmlElement assetEle = (XmlElement)xn2;
                        name = assetEle.GetAttribute("name");
                        Asset asset = new Asset(name, bundle);
                        assetDict.Add(name, asset);

                        foreach (XmlNode xn3 in assetEle.ChildNodes)
                        {
                            XmlElement depEle = (XmlElement)xn3;
                            Bundle dep;
                            if (!bundleDict.TryGetValue(depEle.InnerText, out dep))
                                dep = new Bundle(depEle.InnerText, null);

                            asset.AddDependencies(dep);
                        }
                    }
                    yield return null;
                }
                yield return null;
            }
        }

        if (onFinish != null)
            onFinish();
    }

    static public string LoadStringImmediate(string name)
    {
        string result;

        Asset asset;
        if (assetDict.TryGetValue(name, out asset))
        {
            asset.Load(null);
            return null;
        }

        using(StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/" + name))
        {
            result = reader.ReadToEnd();
        }

        return result;
    }


    static public Asset Load(string name)
	{
        Asset asset;
        if (!assetDict.TryGetValue(name, out asset))
        {
            Debug.Log("AssetMgr::Load - not find " + name);
            return null;
        }

        if (loadQueue.Contains(asset))
            return asset;

        asset.PrepareLoad();
        loadQueue.Enqueue(asset);
        return asset;
    }

    static public Asset LoadScene(string name)
    {
        Asset asset = Load(name);
        if (asset == null)
            return null;

        asset.LoadAssetAsync = delegate(Bundle bundle)
        {
            return new Asset.SceneRequest(Application.LoadLevelAsync(name));
        };

        return asset;
    }


    static public void UnLoad(string name)
    {
        Resources.UnloadUnusedAssets();
    }

    static public void StartLoading(Action onLoaded = null)
    {
        coroutineProvider.StartCoroutine(Loading(onLoaded));
    }

    static IEnumerator Loading(Action onLoaded)
    {
        float total = loadQueue.Count;
        float current = 0;

        while(loadQueue.Count > 0)
        {
            Asset asset = loadQueue.Dequeue();

            if(OnProgress != null)
            {
                asset.OnProgress = delegate(float p)
                {
                    OnProgress((current + p) / total);
                };
            }

            yield return coroutineProvider.StartCoroutine(asset.Load(coroutineProvider));

            current = total - loadQueue.Count;
        }


        if (onLoaded != null)
        {
            // 当有进度条的时候，进度100%渲染不出来，所以等几帧再结束
            yield return new WaitForSeconds(0.2f);
            onLoaded();
        }

        if(loadQueue.Count == 0)
            OnProgress = null;
    }
}
