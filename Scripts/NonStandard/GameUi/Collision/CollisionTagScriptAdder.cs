#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class CollisionTagScriptAdder {
	static CollisionTagScriptAdder() {
		string folder = NonStandard.Utility.File.GetAbsolutePath("Packages"), file = "nonstandard.gameui.generated.asmdef";
		List<string> files = NonStandard.Utility.File.FindFile(folder, file, -1);
		if(files == null) {
			folder = NonStandard.Utility.File.GetAbsolutePath("Assets");
			files = NonStandard.Utility.File.FindFile(folder, file, -1);
			if (files == null) {
				Debug.Log("could not find " + file + " in " + folder + ". something is wrong.");
				return;
			}
		}
		char dir = System.IO.Path.DirectorySeparatorChar;
		string partOfTheOneWeWant = dir + ".NonStandard.GameUi.Generated";
		string path = null;
		for (int i = 0; i < files.Count; ++i) {
			if (files[i].Contains(partOfTheOneWeWant)) {
				path = files[i];
				break;
			}
		}
		if (path == null) {
			Debug.Log("missing the hidden folder...");
		}
		path = path.Substring(0, path.Length - (file.Length + 1));
		string folderName = path.Substring(path.LastIndexOf(dir) + 2);
		//string source = path.Substring(0, path.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
		string destination = System.IO.Path.GetFullPath(".") + dir + "Assets" + dir + folderName;
		NonStandard.Utility.Define.Add("GENERATED_NONSTANDARD_TAGS");
		if (System.IO.Directory.Exists(destination)) {
			return;
		}
		Debug.Log("must move " + path + " to " + destination);
		System.IO.Directory.CreateDirectory(destination);
		NonStandard.Utility.File.CopyFilesRecursively(path, destination);
	}
}
#endif
