using UnityEngine;
using UnityEditor;
using System.Collections;

public static class FSNEditorUtils
{
	/// <summary>
	/// 타겟 경로가 유효하도록, 필요할 경우 폴더를 생성한다.
	/// </summary>
	/// <param name="targetpath"></param>
	public static void MakeTargetDirectory(string targetdir)
	{
		if (!string.IsNullOrEmpty(targetdir) && !AssetDatabase.IsValidFolder(targetdir))
		{
			string parent, current;
			FSNUtils.StripPathAndName(targetdir, out parent, out current);

			MakeTargetDirectory(parent);
			if (!string.IsNullOrEmpty(current))
			{
				AssetDatabase.CreateFolder(parent, current);
				//Debug.LogFormat("making dir : {0}, {1}", parent, current);
			}
		}
	}
}
