using UnityEngine;

static class GUITool
{
    static UIPanel uiRoot = null;

    static public GameObject Root
    {
        get { return (uiRoot != null) ? uiRoot.gameObject : null; }
    }

    static public Asset Load(string name)
    {
        Asset asset = AssetMgr.Load(name);
        if (asset == null)
            return null;

        asset.OnAssetLoaded = delegate(Object obj)
        {
            if (uiRoot == null)
            {
                uiRoot = NGUITools.CreateUI(false);
                GameObject.DontDestroyOnLoad(uiRoot.gameObject);
                //uiRoot.camera.audio.enabled = false;
                AudioListener al = (AudioListener)uiRoot.GetComponentInChildren<AudioListener>();
                al.enabled = false;
            }

            GameObject go = NGUITools.AddChild(uiRoot.gameObject, obj as GameObject);
            go.name = go.name.Replace("(Clone)", "");
        };

        return asset;
    }

    static public void Destroy(string name)
    {
        GameObject obj = FindControl(name);
        if (obj != null)
        {
            obj.transform.parent = null;
            GameObject.Destroy(obj);
            AssetMgr.UnLoad(name);
        }
    }

    static public void DestroyAll()
    {
        UIPanel[] panel = uiRoot.GetComponentsInChildren<UIPanel>();
        for (int i = 0, imax = panel.Length; i < imax; ++i)
            AssetMgr.UnLoad(panel[i].name);

        GameObject.Destroy(uiRoot.gameObject);
        GameObject rt = GameObject.Find("_RealTime");
        GameObject.Destroy(rt);
    }

    static public void ClearColor(Color bg)
    {
        UICamera camera = uiRoot.GetComponentInChildren<UICamera>();
        camera.camera.clearFlags = CameraClearFlags.SolidColor;
        camera.camera.backgroundColor = bg;
    }
    static public void ClearColor(bool clear)
    {
        UICamera camera = uiRoot.GetComponentInChildren<UICamera>();
        camera.camera.clearFlags = CameraClearFlags.Depth;
    }

    static public GameObject FindControl(string name)
    {
        Transform t = uiRoot.gameObject.transform.Find(name);
        if (t == null)
            return null;
        return t.gameObject;
    }

    static public void AddSubmitEvent(string name, UIEventListener.VoidDelegate callback)
    {
        Transform t = uiRoot.gameObject.transform.Find(name);
        if (t == null)
            Debug.Log("AddSubmitEvent Not Find " + name);
        UIEventListener.Get(t.gameObject).onSubmit += callback;
    }

    static public void AddClickEvent(string name, UIEventListener.VoidDelegate callback)
	{
		Transform t = uiRoot.gameObject.transform.Find(name);
        if (t == null)
            Debug.Log("AddClickEvent Not Find " + name);
		UIEventListener.Get(t.gameObject).onClick += callback;
	}
}
