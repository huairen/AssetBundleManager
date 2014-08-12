using UnityEngine;
using System;
using System.Collections;

public class Bundle
{
	public string name;
	public string md5;

    int loadReq = 0;
	AssetBundle bundle = null;

    public Bundle(string _name, string _md5)
    {
        name = _name;
        md5 = _md5;
    }

    public void incRef()
    {
        loadReq++;
    }

    public void decRef()
    {
        if(--loadReq == 0)
        {
            bundle.Unload(false);
            bundle = null;
        }
    }

    public IEnumerator Load()
    {
        if (bundle != null)
        {
            Debug.Log("Bundle::Load - bundle is loaded " + name);
            yield break;
        }

        using(WWW www = new WWW(AssetMgr.LOCAL_ASSET_URL + name))
        {
            yield return www;
            bundle = www.assetBundle;
        }
	}

    public void UnLoad()
    {
        loadReq = 0;
        if(bundle != null)
        {
            bundle.Unload(false);
            bundle = null;
        }
    }

    public AssetBundleRequest LoadAssetAsync(string name, Type type)
    {
        return bundle.LoadAsync(name, type);
    }

    public bool IsLoaded
    {
        get
        {
            if (bundle == null)
                return false;

            return true;
        }
    }
}
