using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 스프라이트 조합 이미지에 해당하는 리소스
/// </summary>
public class FSNCombinedImage
{
	/// <summary>
	/// 커스텀 리소스 로더
	/// </summary>
	public class Loader : FSNResourceCache.ICustomLoader
	{
		public object LoadResource(string path)
		{
			return FSNCombinedImage.CreateFromResource(path);
		}

		public void UnloadResource(object res)
		{

		}
	}

	/// <summary>
	/// SpriteSheet 커스텀 로더
	/// </summary>
	class SpriteSheetLoader : FSNResourceCache.ICustomLoader
	{
		public object LoadResource(string path)
		{
			return new SpriteSheet(path);
		}

		public void UnloadResource(object res)
		{

		}
	}
	//

	/// <summary>
	/// 스프라이트 시트
	/// </summary>
	class SpriteSheet
	{
		/// <summary>
		/// 원래의 Sprite[] 배열
		/// </summary>
		public Sprite[] rawdata { get; private set; }
		public string sheetPath { get; private set; }


		Dictionary<string, Sprite>	m_lookupTable;		// 이름으로 스프라이트를 검색하기 위한 딕셔너리

		public SpriteSheet(string path)
		{
			sheetPath		= path;
			var sprites		= Resources.LoadAll<Sprite>(path);
			if (sprites == null)
			{
				Debug.LogError("Cannot open spritesheet : " + path);
			}
			rawdata			= sprites;

			m_lookupTable	= new Dictionary<string, Sprite>();
			int count		= sprites.Length;
			for(int i = 0; i < count; i++)
			{
				var spr		= sprites[i];
				m_lookupTable[spr.name] = spr;
			}
		}

		public Sprite this[string key]
		{
			get
			{
				Sprite spr	 = null;
				if(!m_lookupTable.TryGetValue(key, out spr))
				{
					Debug.LogError("Cannot open sprite named " + key + " from sprite sheet : " + sheetPath);
				}
				return spr;
			}
		}
	}

	/// <summary>
	/// 스프라이트 정보
	/// </summary>
	public interface IData
	{
		/// <summary>
		/// 스프라이트 패킹 텍스쳐
		/// </summary>
		Texture2D texture { get; }
		/// <summary>
		/// 가로 크기 (픽셀)
		/// </summary>
		int pixelWidth {get;}
		/// <summary>
		/// 세로 크기 (픽셀)
		/// </summary>
		int pixelHeight {get;}
		/// <summary>
		/// 기본 이미지의 UV 좌표
		/// </summary>
		Rect baseUVRect { get; }

		/// <summary>
		/// 하위 스프라이트
		/// </summary>
		ISubSpriteData[] subSprites { get; }
	}

	/// <summary>
	/// 하위 스프라이트
	/// </summary>
	public interface ISubSpriteData
	{
		/// <summary>
		/// 소스의 UV 좌표
		/// </summary>
		Rect sourceUVRect { get; }
		/// <summary>
		/// 타겟 UV 좌표 (해당 UV 좌표 영역을 소스 UV로 대체한다
		/// </summary>
		Rect targetUVRect { get; }
	}


	/// <summary>
	/// 스프라이트 정보
	/// </summary>
	class Data : IData
	{
		const float             c_subSpritePadding  = 1f;		// 모서리 artifact를 방지하기 위해 이만큼(픽셀) 영역을 잡게 덮어쓴다.

		public Texture2D		texture { get; private set; }
		public int				pixelWidth { get; private set; }
		public int				pixelHeight { get; private set; }
		public Rect				baseUVRect { get; private set; }
		public ISubSpriteData[]	subSprites { get; private set; }


		/// <summary>
		/// 처리되기 전의 스프라이트 정보
		/// </summary>
		struct RawSubSpriteInfo
		{
			public Sprite sprite;
			public int x;
			public int y;
		}

		class SubSpriteData : ISubSpriteData
		{
			public Rect sourceUVRect { get; set; }
			public Rect targetUVRect { get; set; }
		}

		List<RawSubSpriteInfo>	m_rawSubSprites = new List<RawSubSpriteInfo>();			// 아직 처리되기 전의 스프라이트 정보
		Sprite					m_baseSprite;	// 원래 스프라이트 (바탕 이미지)


		/// <summary>
		/// 기본 (바탕) 스프라이트 정보를 세팅한다.
		/// </summary>
		/// <param name="sprite"></param>
		public void SetBaseSprite(Sprite sprite)
		{
			m_baseSprite	= sprite;

			texture		= sprite.texture;
			pixelWidth	= (int)sprite.textureRect.width;
			pixelHeight	= (int)sprite.textureRect.height;
			baseUVRect	= ConvertSpriteRectToUVRect(sprite.textureRect, texture);

			m_rawSubSprites.Clear();				// 서브 스프라이트 정보는 모두 지운다.
			subSprites	= null;
		}

		/// <summary>
		/// 서브 스프라이트를 추가한다. x, y 좌표는 해당 스프라이트의 좌하단을 기준으로 한다.
		/// </summary>
		/// <param name="sprite"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void AddSubSprite(Sprite sprite, int x, int y)
		{
			m_rawSubSprites.Add(new RawSubSpriteInfo() { sprite = sprite, x = x, y = y });
		}

		/// <summary>
		/// 지금까지 추가한 서브 스프라이트 정보를 실제 사용 가능하도록 빌드한다.
		/// </summary>
		public void BuildSubSpriteInfo()
		{
			List<SubSpriteData> m_cooked	= new List<SubSpriteData>();

			var origrect		= m_baseSprite.textureRect;	// 바탕 스프라이트의 rect 가져오기

			int count = m_rawSubSprites.Count;
			for(int i = 0; i < count; i++)				// 각 서브 스프라이트 처리
			{
				var info		= m_rawSubSprites[i];
				var sourcerect	= info.sprite.textureRect;		// 스프라이트가 참조하는 텍스쳐 영역
				var targetrect	= new Rect()					// 스프라이트가 덧그려야할 텍스쳐 영역
				{
					x			= origrect.x + info.x,
					y			= origrect.y + info.y,
					width		= sourcerect.width,
					height		= sourcerect.height,
				};

				m_cooked.Add(new SubSpriteData()				// 전부 UV 좌표료 변경해서 넣는다
				{
					sourceUVRect	= ConvertSpriteRectToUVRect(sourcerect, texture, c_subSpritePadding),
					targetUVRect	= ConvertSpriteRectToUVRect(targetrect, texture, c_subSpritePadding),
				});
			}

			subSprites			= m_cooked.ToArray();			// 최종적으로 array로 변환
			m_rawSubSprites.Clear();
		}

		/// <summary>
		/// 스프라이트의 아틀라스 상의 Rect 를 아틀라스 텍스쳐의 UV 좌표료 변환
		/// </summary>
		/// <param name="sprite"></param>
		/// <returns></returns>
		Rect ConvertSpriteRectToUVRect(Rect rect, Texture texture, float padding = 0)
		{
			var width	= texture.width;
			var height	= texture.height;

			if (padding > 0)
			{
				rect.xMin   += padding;
				rect.yMin   += padding;
				rect.xMax   -= padding;
				rect.yMax   -= padding;
			}

			rect.x		/= width;
			rect.width	/= width;
			rect.y		/= height;
			rect.height	/= height;
			return rect;
		}
	}
	//


	// Constants

	const string		c_json_spriteSheet		= "SpriteSheet";
	const string		c_json_baseSprite		= "BaseSprite";
	const string		c_json_subSprite		= "SubSprite";
	const string		c_json_subSprite_name	= "Name";
	const string		c_json_subSprite_pos	= "Position";



	// Members


	/// <summary>
	/// 스프라이트 정보
	/// </summary>
	public IData spriteData { get; private set; }


	private FSNCombinedImage()
	{

	}


	// Statics

	static bool s_loaderInstalled = false;

	static FSNCombinedImage()
	{
		InstallLoaders();		// static 초기화시에 로더 인스톨해주기
	}

	/// <summary>
	/// 리소스 로더를 설치
	/// </summary>
	public static void InstallLoaders()
	{
		if (s_loaderInstalled)														// 로더를 이미 설치한 경우엔 무시
			return;
		s_loaderInstalled = true;

		FSNResourceCache.InstallLoader<SpriteSheet>(new SpriteSheetLoader());		// 스프라이트 시트 로더
		FSNResourceCache.InstallLoader<FSNCombinedImage>(new Loader());				// FSNCombinedImage 로더
	}

	/// <summary>
	/// json 파일을 통해 생성하기
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public static FSNCombinedImage CreateFromResource(string path)
	{
		var res				= Resources.Load<TextAsset>(path);
		if (res)
		{
			var combined	= new FSNCombinedImage();
			var sprdata		= new Data();
			combined.spriteData	= sprdata;

			var json		= new JSONObject(res.text);				// JSON 파싱하기

			var sprpath		= json[c_json_spriteSheet].str;			// 스프라이트 시트
			var sprsheet	= FSNResourceCache.Load<SpriteSheet>(FSNResourceCache.Category.Script, sprpath);

			var baseimage	= json.GetField(c_json_baseSprite).str;	// 바탕 이미지
			sprdata.SetBaseSprite(sprsheet[baseimage]);

			json.GetField(c_json_subSprite, (subsprarr) =>			// 서브 스프라이트
				{
					var list	= subsprarr.list;
					var count	= list.Count;
					for(int i = 0; i < count; i++)
					{
						var subspr	= subsprarr[i];
						var sprname	= subspr[c_json_subSprite_name].str;
						var poslist	= subspr[c_json_subSprite_pos].list;

						sprdata.AddSubSprite(sprsheet[sprname], (int)poslist[0].n, (int)poslist[1].n);
					}
				});

			sprdata.BuildSubSpriteInfo();							// 서브 스프라이트 데이터 빌드


			return combined;
		}
		else
		{
			Debug.LogError("[FSNCombinedImage.CreateFromResource] Cannot load " + path);
			return null;
		}
	}
}
