using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

class FSNSpritePackerData : FSNRectPacker.IData
{
	public int height
	{
		get
		{	return texture.height;	}
	}

	public int width
	{
		get
		{	return texture.width;	}
	}

	public Texture2D texture	{ get; private set; }
	public string spriteName	{ get; private set; }

	public FSNSpritePackerData(Texture2D tex, string sprname)
	{
		texture		= tex;
		spriteName  = sprname;
	}
}

class FSNSpritePacker : FSNRectPacker.BaseRectPacker<FSNSpritePackerData>
{
	/// <summary>
	/// 출력 경로
	/// </summary>
	public string outputPath { get; set; }


	protected override void OnFailure()
	{
		//throw new NotImplementedException();
	}

	protected override void OnSuccess(int width, int height, Output[] output)
	{
		var outtex	= new Texture2D(width, height, TextureFormat.ARGB32, false);

		var pixels	= outtex.GetPixels32();						// 텍스쳐를 0으로 채우기
		var pixcnt	= pixels.Length;
		var zeroCol	= new Color32(0, 0, 0, 0);
		for (int i = 0; i < pixcnt; i++)
		{
			pixels[i]	= zeroCol;
		}
		outtex.SetPixels32(pixels);

		int count   = output.Length;
		for(int i = 0; i < count; i++)							// 텍스쳐를 조립한다.
		{
			var entry   = output[i];
			var texture = entry.data.texture;
			outtex.SetPixels32(entry.xMin, entry.yMin, texture.width, texture.height, texture.GetPixels32());
		}

		//var textureOutAssetPath	= "Resources/" + outputPath + ".png";							// 실제 출력할 텍스쳐 경로 (asset)
		var textureOutAssetPath = outputPath + ".png";
		var textureOutRealPath  = Application.dataPath + "/" + textureOutAssetPath;				// 실제 출력할 텍스쳐 경로 (절대경로)
		string outPath, outName;
		FSNUtils.StripPathAndName(textureOutAssetPath, out outPath, out outName);
		FSNEditorUtils.MakeTargetDirectory("Assets/" + outPath);											// 타겟 디렉토리 확보
		System.IO.File.WriteAllBytes(textureOutRealPath, outtex.EncodeToPNG());                 // 텍스쳐를 파일로 기록한다.
		AssetDatabase.Refresh();

		var importer            = AssetImporter.GetAtPath("Assets/" + textureOutAssetPath) as TextureImporter;
		var settings            = new TextureImporterSettings();
		importer.ReadTextureSettings(settings);
		settings.ApplyTextureType(TextureImporterType.Sprite, false);
		settings.spriteMeshType	= SpriteMeshType.FullRect;
		settings.spriteExtrude	= 0;
		settings.rgbm			= TextureImporterRGBMMode.Off;
		importer.SetTextureSettings(settings);

		importer.textureType	= TextureImporterType.Advanced;		                           // 출력한 텍스쳐의 임포트 옵션 설정
		importer.anisoLevel		= 0;
		importer.filterMode     = FilterMode.Bilinear;
		importer.mipmapEnabled  = false;
		importer.npotScale      = TextureImporterNPOTScale.None;
		importer.isReadable		= true;
		importer.spriteImportMode	= SpriteImportMode.Single;	// 어떤 경우, Single로 바꾼 다음 다시 Multi로 바꿔야한다.
		importer.textureFormat  = TextureImporterFormat.AutomaticTruecolor;
		importer.linearTexture	= true;

		var spritesheet			= new List<SpriteMetaData>();                                   // 스프라이트 시트 생성 (각 스프라이트 영역 구분)
		for (int i = 0; i < count; i++)
		{
			var entry           = output[i];
			spritesheet.Add(new SpriteMetaData()
			{
				pivot			= new Vector2(0.5f, 0.5f),
				alignment		= 0,
				name			= entry.data.spriteName,
				rect            = new Rect(entry.xMin, entry.yMin, entry.data.width, entry.data.height)
			});
		}
		importer.spriteImportMode	= SpriteImportMode.Multiple;
		importer.spritesheet    = spritesheet.ToArray();
		
		importer.SaveAndReimport();
	}
}

/// <summary>
/// 조합 이미지를 자동으로 생성한다.
/// </summary>
public static class FSNCombinedImageGenerator
{
	const string        c_combinedImageConfigFileName   = "combinedimage";	// 조합 이미지 빌드용 설정 파일


	class Config
	{
		const string    c_jsonfield_outputPath		= "OutputPath";
		const string    c_jsonfield_targetDimension = "ResultSize";

		/// <summary>
		/// 이 설정파일로 묶이는 텍스쳐들 경로
		/// </summary>
		public string[] spritePathList { get; private set; }
		/// <summary>
		/// 최종 조합 스프라이트 경로
		/// </summary>
		public string outputPath { get; private set; }
		/// <summary>
		/// 스프라이트 가로
		/// </summary>
		public int outputWidth { get; private set; }
		/// <summary>
		/// 스프라이트 세로
		/// </summary>
		public int outputHeight { get; private set; }

		public Config(string configPath)
		{
			//Debug.Log("Config file read : " + configPath);
			var data		= AssetDatabase.LoadAssetAtPath<TextAsset>(configPath).text;
			var json		= new JSONObject(data);

			// 설정파일을 읽는다.
			outputPath		= json[c_jsonfield_outputPath].str;
			var pair		= json[c_jsonfield_targetDimension].list;
			outputWidth		= (int)pair[0].n;
			outputHeight	= (int)pair[1].n;

			// 설정파일과 같은 경로에 있는 텍스쳐들을 검색
			string dir, filename;
			FSNUtils.StripPathAndName(configPath, out dir, out filename);
			var textureGUIDs    = AssetDatabase.FindAssets("t:Texture2D", new string[] { dir });
			var texcount        = textureGUIDs.Length;
			var texturePaths    = new string[texcount];
			for (int i = 0; i < texcount; i++)
			{
				texturePaths[i] = AssetDatabase.GUIDToAssetPath(textureGUIDs[i]);
			}
			spritePathList      = texturePaths;
		}
	}

	/// <summary>
	/// 어셋 폴더를 검색해서 조합 이미지 소스들을 합친다
	/// </summary>
	public static void BuildCombinedImageSources()
	{
		var configGUIDs = AssetDatabase.FindAssets(c_combinedImageConfigFileName + " t:TextAsset"); // 설정 파일 모으기
		int count       = configGUIDs.Length;
		int successCount    = 0;
		for(int i = 0; i < count; i++)
		{
			var path    = AssetDatabase.GUIDToAssetPath(configGUIDs[i]);
			if (!path.EndsWith("/" + c_combinedImageConfigFileName + ".json"))	// 확실하게 combinedimage.json 파일인지 체크한다
				continue;
			var config  = new Config(path);

			if (!BuildCombinedImage(config))					// 패킹 시도
			{
				Debug.LogErrorFormat("sprite packing failed! {0} - maybe target size is too small?", config.outputPath);
			}
			else
			{
				successCount++;
            }
		}
		AssetDatabase.Refresh();

		Debug.LogFormat("조합 이미지 생성 완료. 총 {0} 개 조합 이미지를 생성했습니다.", successCount);
	}

	/// <summary>
	/// 조합 이미지 하나를 빌드한다.
	/// </summary>
	/// <param name="config"></param>
	/// <returns></returns>
	static bool BuildCombinedImage(Config config)
	{
		var packer			= new FSNSpritePacker();
		packer.outputPath   = config.outputPath;
		var paths           = config.spritePathList;
		var count           = paths.Length;
		for(int i = 0; i < count; i++)									// 각 텍스쳐 경로에 대해 실행
		{
			EnsureSourceTextureImportSetting(paths[i]);                 // 소스 텍스쳐 임포트 세팅 맞추기

			var origpath    = paths[i];
			string path, name;
			FSNUtils.StripPathAndName(origpath, out path, out name);

			var texture     = AssetDatabase.LoadAssetAtPath<Texture2D>(origpath);
			var data        = new FSNSpritePackerData(texture, FSNUtils.RemoveFileExt(name));
			packer.PushData(data);										// 팩커에 텍스쳐 하나씩 밀어넣기
		}

		return packer.Pack(config.outputWidth, config.outputHeight);
	}

	/// <summary>
	/// 소스 텍스쳐 import setting을 적절하게 맞춰준다.
	/// </summary>
	/// <param name="texture"></param>
	static void EnsureSourceTextureImportSetting(string path)
	{
		var importer		= AssetImporter.GetAtPath(path) as TextureImporter;
		var settings		= new TextureImporterSettings();
		importer.ReadTextureSettings(settings);
		settings.readable   = true;                                     // 읽기 가능하게
		settings.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		settings.spriteMode = 1;
		settings.mipmapEnabled	= false;
		settings.ApplyTextureType(TextureImporterType.Sprite, false);	// 나머지는 Sprite 설정으로
		importer.SetTextureSettings(settings);
		importer.isReadable	= true;
		importer.SaveAndReimport();
	}
}
