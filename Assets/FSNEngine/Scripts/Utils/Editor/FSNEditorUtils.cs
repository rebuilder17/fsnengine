using UnityEngine;
using UnityEditor;
using System.Collections;

public static class FSNEditorUtils
{
	/// <summary>
	/// 경로를 분리하여 디렉토리 부분과 파일 이름으로 나눈다
	/// </summary>
	/// <param name="origpath"></param>
	/// <param name="path"></param>
	/// <param name="name"></param>
	public static void StripPathAndName(string origpath, out string path, out string name)
	{
		var pathdel = origpath.LastIndexOf('/');
		if (pathdel != -1)
		{
			path    = origpath.Substring(0, pathdel);
			name    = origpath.Substring(pathdel + 1);
		}
		else
		{
			path    = "";
			name    = origpath;
		}
	}

	/// <summary>
	/// 타겟 경로가 유효하도록, 필요할 경우 폴더를 생성한다.
	/// </summary>
	/// <param name="targetpath"></param>
	public static void MakeTargetDirectory(string targetdir)
	{
		if (!string.IsNullOrEmpty(targetdir) && !AssetDatabase.IsValidFolder(targetdir))
		{
			string parent, current;
			StripPathAndName(targetdir, out parent, out current);

			MakeTargetDirectory(parent);
			if (!string.IsNullOrEmpty(current))
			{
				AssetDatabase.CreateFolder(parent, current);
				//Debug.LogFormat("making dir : {0}, {1}", parent, current);
			}
		}
	}
}
