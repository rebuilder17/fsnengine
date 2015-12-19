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
		var processed   = 0;
		for (var i = 0; i < count; i++)
		{
			var path    = AssetDatabase.GUIDToAssetPath(guids[i]);
			string dir, name;
			FSNUtils.StripPathAndName(path, out dir, out name);		// 텍스쳐 경로, 이름 분리
			var subdir  = searchPath.Length == dir.Length? "" : dir.Substring(searchPath.Length + 1);	// 검색 경로의 하위 경로를 뽑아낸다.

			var importer			= AssetImporter.GetAtPath(path) as TextureImporter; // 텍스쳐 타입을 제대로 설정한다.
			importer.npotScale      = TextureImporterNPOTScale.None;
			importer.textureFormat  = TextureImporterFormat.AutomaticTruecolor;
			importer.isReadable		= true;
			importer.SaveAndReimport();

			
			var completeDestPath    = destPath;
			if (subdir.Length > 0 && subdir[0] != '/')
			{
				subdir = "/" + subdir;
			}
			completeDestPath        += subdir;

			//Debug.Log("completeDestPath : " + completeDestPath);

			Convert(path, name, completeDestPath);
			processed++;
		}

		AssetDatabase.Refresh();

		Debug.LogFormat("Premultiplied Alpha 텍스쳐 생성 완료. 총 {0} 개 처리했습니다.", processed);
	}

	static void Convert(string origpath, string filename, string destpath)
	{
		FSNEditorUtils.MakeTargetDirectory(destpath);					// 타겟 경로 확보
		var assetpath       = destpath + "/" + filename;
		var absolutepath    = Application.dataPath + "/../" + assetpath;
		//Debug.LogFormat("asset path : {0}, absolute target path : {1}", assetpath, absolutepath);

		if (AssetDatabase.AssetPathToGUID(assetpath) != null)			// 복사하려는 위치에 해당 어셋이 이미 존재한다면 기존 것은 삭제
			AssetDatabase.DeleteAsset(assetpath);
		AssetDatabase.CopyAsset(origpath, assetpath);                   // 변경하려는 어셋을 복제하여 타겟 경로에 넣는다.
		AssetDatabase.Refresh();

		var texture		= AssetDatabase.LoadAssetAtPath<Texture2D>(assetpath);
		var converted   = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
		var origcolors  = texture.GetPixels32();
		var len         = origcolors.Length;
		for (int i = 0; i < len; i++)									// 픽셀마다 알파곱 계산
		{
			var color       = origcolors[i];
			var alpha       = (int)color.a;
			color.r         = (byte)(color.r * alpha / 255);
			color.g			= (byte)(color.g * alpha / 255);
			color.b         = (byte)(color.b * alpha / 255);
			origcolors[i]   = color;
		}
		converted.SetPixels32(origcolors);
		
		System.IO.File.WriteAllBytes(absolutepath, converted.EncodeToPNG());	// 실제 파일로 write
		AssetDatabase.ImportAsset(assetpath);
		var importer        = AssetImporter.GetAtPath(assetpath) as TextureImporter;    // 텍스쳐 옵션 설정
		importer.textureType= TextureImporterType.Advanced;
		importer.alphaIsTransparency    = false;            // premultiplied alpha texture는 이 옵션을 꺼줘야 한다.
		importer.SaveAndReimport();
	}
}
