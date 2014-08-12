using System.IO;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;

class BundleXml
{
    static string ASSETINFO_FILE = "assetinfo.xml";
    XmlDocument xmlDoc = null;
    XmlNode xmlRoot = null;
    XmlElement xmlBundle = null;

    public void Open()
    {
        string path = Application.streamingAssetsPath + "/" + ASSETINFO_FILE;
        xmlDoc = new XmlDocument();
        if (File.Exists(path))
            xmlDoc.Load(path);

        xmlRoot = xmlDoc.SelectSingleNode("AssetBundles");
        if (xmlRoot == null)
        {
            xmlRoot = xmlDoc.CreateElement("AssetBundles");
            xmlDoc.AppendChild(xmlRoot);
        }
    }

    public void UpdateAssetBundle(string bundleName, string dir = null)
    {
        XmlNodeList nodeList = xmlRoot.ChildNodes;

        XmlElement xe = null;
        foreach (XmlNode xn in nodeList)
        {
            xe = (XmlElement)xn;
            if (xe.GetAttribute("name") == bundleName)
                break;
            xe = null;
        }

        if (xe == null)
        {
            xe = xmlDoc.CreateElement("AssetBundle");
            xmlRoot.AppendChild(xe);
        }
        else
        {
            xe.RemoveAll();
        }

        xe.SetAttribute("name", bundleName);
        xmlBundle = xe;

        // calc md5
        string path = Application.streamingAssetsPath + Path.DirectorySeparatorChar + bundleName;
        FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if(file != null)
        {
            MD5CryptoServiceProvider md5Generator = new MD5CryptoServiceProvider();
            byte[] hash = md5Generator.ComputeHash(file);
            string strMD5 = System.BitConverter.ToString(hash);
            file.Close();

            xe.SetAttribute("md5", strMD5);
        }
    }

    public void UpdateAsset(string assetName, string[] depbundle)
    {
        //查找asset
        XmlNodeList nodeList = xmlBundle.ChildNodes;
        XmlElement assetEle = null;
        foreach (XmlNode xn in nodeList)
        {
            assetEle = (XmlElement)xn;
            if (assetEle.GetAttribute("name") == assetName)
                break;
            assetEle = null;
        }

        if (assetEle == null)
        {
            assetEle = xmlDoc.CreateElement("Asset");
            xmlBundle.AppendChild(assetEle);
        }
        else
        {
            assetEle.RemoveAll();
        }

        assetEle.SetAttribute("name", assetName);

        if (depbundle != null)
        {
            foreach (string dep in depbundle)
            {
                XmlElement depEle = xmlDoc.CreateElement("Dep");
                depEle.InnerText = dep;
                assetEle.AppendChild(depEle);
            }
        }
    }

    public void Close()
    {
        string path = Application.streamingAssetsPath + "/" + ASSETINFO_FILE;
        if (xmlDoc != null)
            xmlDoc.Save(path);

        xmlDoc = null;
        xmlRoot = null;
        xmlBundle = null;
    }
}
