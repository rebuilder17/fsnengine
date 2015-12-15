using UnityEngine;
using System.Collections;
using FSNRectPacker;
using UnityEditor;

public class TestPackerData : FSNRectPacker.IData
{
	public int width { get; private set; }
	public int height { get; private set; }
	public Color color { get; private set; }
	public TestPackerData(int w, int h)
	{
		width	= w;
		height  = h;
		color   = new Color(Random.Range(0.5f, 1), Random.Range(0.5f, 1), Random.Range(0.5f, 1), 1);
	}
}

class TestPacker : FSNRectPacker.BaseRectPacker<TestPackerData>
{
	protected override void OnSuccess(int width, int height, Output[] output)
	{
		var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
		int count   = output.Length;
		for(int i = 0; i < count; i++)
		{
			var item    = output[i];
			var color   = item.data.color;
			for (int y = item.yMin; y <= item.yMax; y++)
			{
				for (int x = item.xMin; x <= item.xMax; x++)
				{
					texture.SetPixel(x, y, color);
				}
			}
		}

		var pngdata = texture.EncodeToPNG();
		System.IO.File.WriteAllBytes(Application.dataPath + "/../packertest.png", pngdata);
	}

	protected override void OnFailure()
	{
		Debug.LogError("cannot pack data");
	}
}

public class PackerTest
{
	[MenuItem("FSN Engine/Packer Test")]
	static void Start()
	{
		var packer = new TestPacker();
		
		for (int i = 0; i < 80; i++)
		{
			packer.PushData(new TestPackerData(Random.Range(10, 80), Random.Range(10, 80)));
		}

		if (packer.Pack(512, 512))
			Debug.Log("packing end");
	}
}
