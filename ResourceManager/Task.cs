using UnityEngine;
using System.Collections;
using System;

namespace ResourceManager
{
	internal class Task
	{
		public string name;
		public Action<string> stringCallback;
		public Action<byte[]> byteCallback;
		public Action<GameObject> objectCallback;
		
		public Task(string name)
		{
			this.name = name;
		}
		
		public bool Execute()
		{
			return true;
		}
	}
}