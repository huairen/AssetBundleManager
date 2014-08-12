using UnityEngine;
using Object=UnityEngine.Object;

public class CharacterElement : Object
{
    Object mesh;
    Object material;
    Object bones;

    public CharacterElement(Object mesh, Object mat, Object bones)
    {
        this.mesh = mesh;
        this.material = mat;
        this.bones = bones;
    }

    public SkinnedMeshRenderer GetSkinnedMeshRenderer()
    {
        GameObject go = (GameObject)Object.Instantiate(mesh);
        go.renderer.material = (Material)material;
        return (SkinnedMeshRenderer)go.renderer;
    }

    public string[] GetBoneNames()
    {
        var holder = (StringHolder)bones;
        return holder.content; 
    }
}