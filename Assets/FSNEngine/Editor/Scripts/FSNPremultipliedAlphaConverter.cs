using UnityEngine;
using UnityEditor;
using System.Collections;

public static class FSNPremultipliedTextureGenerator
{
	/// <summary>
	/// 특정 경로에 있는 텍스쳐들을 검색해서 premultiplied alpha를 적용하여 빌드한다.
	/// </summary>
	/// <param name="searchPath"></param>
	public static void ConvertWithinPath(string searchPath, string destPath)
	{
		var guids	= AssetDatabase.FindAssets("t:Texture2D", new string[] { searchPath });
		var count   = guids.Length;
		for (var i = 0; i < count; i++)
		{
			var path    = AssetDatabase.GUIDToAssetPath(guids[i]);
			string dir, name;
			FSNUtils.StripPathAndName(path, out dir, out name);		// 텍스쳐 경로, 이름 분리
			var subdir  = searchPath.Length == dir.Length? "" : dir.Substring(searchPath.Length + 1);	// 검색 경로의 하위 경로를 뽑아낸다.

			var importer			= AssetImporter.GetAtPath(path) as TextureImporter; // 텍스쳐 타입을 제대로 설정한다.
			importer.npotScale      = TextureImporterNPOTScale.None;
			importer.textureFormat  = TextureImporterFormat.AutomaticTruecolor;
			importer.SaveAndReimport();

			
			var completeDestPath    = destPath;
			if (subdir.Length > 0 && subdir[0] != '/')
			{
				subdir = "/" + subdir.Substring(0, subdir.Length - 1);
			}
			completeDestPath        += subdir;

			Debug.Log("completeDestPath : " + completeDestPath);

			Convert(path, name, completeDestPath);
		}

		AssetDatabase.Refresh();
	}

	static void Convert(string origpath, string filename, string destpath)
	{
		FSNEditorUtils.MakeTargetDirectory(destpath);					// 타겟 경로 확보
		var assetpath       = destpath + "/" + filename;
		var absolutepath    = Application.dataPath + "/../" + assetpath;
		Debug.LogFormat("asset path : {0}, absolute target path : {1}", assetpath, absolutepath);
		AssetDatabase.CopyAsset(origpath, assetpath);                   // 변경하려는 어셋을 복제하여 타겟 경로에 넣는다.
		AssetDatabase.Refresh();

		var texture		= AssetDatabase.LoadAssetAtPath<Texture2D>(assetpath);
		var converted   = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
		var origcolors  = converted.GetPixels();
		var len         = origcolors.Length;
		for (int i = 0; i < len; i++)
		{
			var color       = origcolors[i];
			var alpha       = color.a;
			color.r         = (color.r * alpha);
			color.g			= (color.g * alpha);
			color.b         = (color.b * alpha);
			origcolors[i]   = color;
		}
		converted.SetPixels(origcolors);
		
		System.IO.File.WriteAllBytes(absolutepath, converted.EncodeToPNG());
		AssetDatabase.ImportAsset(assetpath);
		var importer        = AssetImporter.GetAtPath(assetpath) as TextureImporter;
		importer.alphaIsTransparency    = false;            // premultiplied alpha texture는 이 옵션을 꺼줘야 한다.
		importer.SaveAndReimport();
	}
}
