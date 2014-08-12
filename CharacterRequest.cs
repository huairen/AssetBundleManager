using UnityEngine;

public class CharacterRequest : Asset.AsyncRequest
{
    AssetBundleRequest gameObjectRequest;
    AssetBundleRequest materialRequest;
    AssetBundleRequest boneNameRequest;
    public CharacterRequest(string name, Bundle bundle)
    {
        gameObjectRequest = bundle.LoadAssetAsync("rendererobject", typeof(GameObject));
        materialRequest = bundle.LoadAssetAsync(name, typeof(Material));
        boneNameRequest = bundle.LoadAssetAsync("bonenames", typeof(StringHolder));
    }

    public bool isDone
    {
        get
        {
            if (!gameObjectRequest.isDone)
                return false;
            if (!materialRequest.isDone)
                return false;
            if (!boneNameRequest.isDone)
                return false;
            return true;
        }
    }

    public float progress
    {
        get
        {
            float value = gameObjectRequest.progress;
            value += materialRequest.progress;
            value += boneNameRequest.progress;
            return value / 3.0f;
        }
    }

    public Object asset
    {
        get
        {
            return new CharacterElement(gameObjectRequest.asset, materialRequest.asset, boneNameRequest.asset);
        }
    }
}