using System;
using UnityEngine;
using System.Collections.Generic;

namespace ResourceManager
{
	public class ResourceHelper : MonoBehaviour
	{
		public static ResourceHelper Instance = ( new GameObject("Resource Manager") ).AddComponent<ResourceHelper>();
		
		const string VersionFile = "assetinfo.xml";//"Version.txt";
		List<Task> taskList = new List<Task>();
		Dictionary<string, Bundle> bundleMap = new Dictionary<string, Bundle>();

		void Start()
		{
			DontDestroyOnLoad(gameObject);
			LoadText(VersionFile, delegate(string text)
			{
				Debug.Log(text);	
			});
		}
		
		void Update()
		{
			if(taskList.Count > 0 && taskList[0].Execute())
				taskList.RemoveAt(0);
		}
		
		public void LoadText(string name, Action<string> onLoaded)
		{
			Task t = new Task(name);
			t.stringCallback = onLoaded;
			taskList.Add(t);
		}
		
		public void LoadBytes(string name, Action<byte[]> onLoaded)
		{
			Task t = new Task(name);
			t.byteCallback = onLoaded;
			taskList.Add(t);
		}
		
		public void LoadObject(string name, Action<GameObject> onLoaded)
		{
			Task t = new Task(name);
			t.objectCallback = onLoaded;
			taskList.Add(t);
		}
		
		void BeginProgress()
		{
		}
	}
}
