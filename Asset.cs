using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class Asset
{
	public string name;

    public Func<Bundle, AsyncRequest> LoadAssetAsync;
    public Action<Object> OnAssetLoaded;
    public Action<WWW> OnWWWLoaded;
    public Action<float> OnProgress;

    Bundle mainBundle;
	List<Bundle> depBundles;

    public Asset(string assetName, Bundle bundle)
    {
        name = assetName;
        mainBundle = bundle;
    }

    public void AddDependencies(Bundle bundle)
    {
        if (depBundles == null)
            depBundles = new List<Bundle>();
        depBundles.Add(bundle);
    }

    public void PrepareLoad()
    {
        if(mainBundle != null)
            mainBundle.incRef();

        if(depBundles != null)
        {
            foreach (Bundle dep in depBundles)
                dep.incRef();
        }
    }

    public IEnumerator Load(MonoBehaviour coroutineProvider)
	{
        // 如果asset没有在assetbundle里面，就直接加载
        if(mainBundle == null)
        {
            using (WWW www = new WWW(AssetMgr.LOCAL_ASSET_URL + name))
            {
                yield return www;
                OnWWWLoaded(www);
            }

            yield break;
        }

        // 先加载依赖的所有assetbundles
        if(depBundles != null)
        {
            foreach (Bundle dep in depBundles)
            {
                if (dep.IsLoaded)
                    continue;
                yield return coroutineProvider.StartCoroutine(dep.Load());
            }
        }


        // 协程加载assetbundle
        if(!mainBundle.IsLoaded)
            yield return coroutineProvider.StartCoroutine(mainBundle.Load());

        // 如果有其它加载asset的方法，直接调用，没有的话就使用默认的asset request
        AsyncRequest request;
        if (LoadAssetAsync != null)
            request = LoadAssetAsync(mainBundle);
        else
            request = new AssetRequest(mainBundle.LoadAssetAsync(name, typeof(Object)));

        while (!request.isDone)
        {
            if (OnProgress != null)
                OnProgress(request.progress);
            yield return 0;
        }

        if (OnProgress != null)
            OnProgress(1.0f);

        if(OnAssetLoaded != null)
            OnAssetLoaded(request.asset);

        // 加载完成后减少assetbundle的引用，使其能够释放
        mainBundle.decRef();
        if (depBundles != null)
        {
            foreach (Bundle dep in depBundles)
                dep.decRef();
        }
    }

#region AsyncRequest 用于异步加载的请求接口
    public interface AsyncRequest
    {
        bool isDone { get; }
        float progress { get; }
        Object asset { get; }
    }

    public class AssetRequest : AsyncRequest
    {
        AssetBundleRequest request;
        public AssetRequest(AssetBundleRequest request)
        {
            this.request = request;
        }

        public bool isDone
        {
            get { return request.isDone; }
        }

        public float progress
        {
            get { return request.progress; }
        }

        public Object asset
        {
            get { return request.asset; }
        }
    }

    public class SceneRequest : AsyncRequest
    {
        AsyncOperation request;
        public SceneRequest(AsyncOperation request)
        {
            this.request = request;
            request.allowSceneActivation = false;
        }

        public bool isDone
        {
            get
            {
                if(request.progress >= 0.9f)
                {
                    request.allowSceneActivation = true;
                    return true;
                }
                return false;
            }
        }

        public float progress
        {
            get { return request.progress; }
        }

        public Object asset
        {
            get { return null; }
        }
    }
#endregion
}
