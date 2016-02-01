using UnityEngine;
using System.Collections;
using types;

public class VectorxTest : MonoBehaviour, StyledStringResourceProvider
{
	public TextAsset svgXml;

	void Awake()
	{
		
	}

	void TestFloat()
	{
		float f = 1.1f;
		var stream = new System.IO.MemoryStream (16);
		var reader = new System.IO.BinaryReader (stream);
		var writer = new System.IO.BinaryWriter (stream);
		writer.Write (f);
		stream.Seek (0, System.IO.SeekOrigin.Begin);
		float f2 = reader.ReadSingle ();

		stream.Seek (0, System.IO.SeekOrigin.Begin);
		string str = "";
		for (int i = 0; i < 4; i++)
		{
			str  += reader.ReadByte().ToString("X") + " ";
		}

		System.Console.Write ("");
	}

	/*public virtual void writeFloat32(double @value) 
	{

		seek();
		float tmp = (float)value;
		writer.Write(value);

	}*/

	void TestData()
	{
		var data = new types.Data (4);
		data.writeFloat32 (1.1);
		data.memory.Seek (0, System.IO.SeekOrigin.Begin);
		string str = "";
		for (int i = 0; i < 4; i++)
		{
			str  += data.reader.ReadByte().ToString("X") + " ";
		}
		System.Console.Write (str);
	}

	Texture2D createTexture(vectorx.ColorStorage storage)
	{
		var texture = new Texture2D(storage.width, storage.height, TextureFormat.RGBA32, false);
		storage.data.memory.Seek (0, System.IO.SeekOrigin.Begin);
		var bytes = storage.data.memory.ToArray ();
		texture.LoadRawTextureData (bytes);
		texture.Apply ();
		return texture;
	}

	void TestColorStorage()
	{
		var colorStorage = new vectorx.ColorStorage (8, 8, null);
		//System.UInt32 value = 0xff0000ff;
		System.UInt32 value32 = 0xff0000ff;
		System.Byte value8 = 0xff;
		colorStorage.data.memory.Seek (0, System.IO.SeekOrigin.Begin);
		for (int i = 0; i < colorStorage.data.allocedLength/4; i++) 
		{
			//colorStorage.data.offset = i;
			//colorStorage.data.writeUInt8 (value8);
			colorStorage.data.writer.Write(value32);
		}

		colorStorage.data.dump ();

		var texture = createTexture (colorStorage);
		GetComponent<Renderer> ().material.mainTexture = texture;
		System.Console.Write ("");
	}

	void TestPixelFormatRenderer()
	{
		Debug.Log ("TestPixelFormatRenderer");
		var colorStorage = new vectorx.ColorStorage (8, 8, null);
		Debug.Log ("allocated: " + colorStorage.data.allocedLength);
		lib.ha.core.memory.MemoryAccess.domainMemory = colorStorage.data;

		for (uint i = 0; i < colorStorage.data.allocedLength; i+=4) 
		{
			Debug.Log (i / 4);
			lib.ha.aggx.renderer.PixelFormatRenderer.copyOrBlendPix2 (i, 255, 0, 0, 255);
			colorStorage.data.dump ();
		}

		var texture = createTexture (colorStorage);
		GetComponent<Renderer> ().material.mainTexture = texture;
		System.Console.Write ("");
	}

	void TestSvg()
	{
		var xml = Xml.parse (svgXml.text);
		var svg = vectorx.svg.SvgContext.parseSvg (xml);


		//var svgBinData = new Data (1024);
		//vectorx.svg.SvgContext.convertSvgToVectorBin (xml, svgBinData);
		//svgBinData.offset = 0;
		//var svg = new lib.ha.svg.SVGData();
		//vectorx.svg.SvgContext.deserializeVectorBin (svgBinData, svg);

		var colorStorage = new vectorx.ColorStorage (512, 512, null);
		//System.Byte value8 = 0xff;
		colorStorage.data.memory.Seek (0, System.IO.SeekOrigin.Begin);
		for (int i = 0; i < colorStorage.data.allocedLength; i++) 
		{
			//colorStorage.data.writer.Write(value8);
		}

		var context = new vectorx.svg.SvgContext ();
		var transform = lib.ha.core.geometry.AffineTransformer.translator(0, 0);
		context.renderVectorBinToColorStorage (svg, colorStorage, transform);

		//colorStorage.data.dump ();

		var texture = createTexture (colorStorage);
		GetComponent<Renderer> ().material.mainTexture = texture;
		System.Console.Write ("");
	}
		
	types.Data GetDataFromFile(string file)
	{
		var asset = Resources.Load(file) as TextAsset;
		var stream = new System.IO.MemoryStream (asset.bytes);
		var data = types.Data.fromMemoryStream (stream,  asset.bytes.Length);

		return data;
	}

	vectorx.ColorStorage GetBitmapDataFromFile(string file)
	{
		var texture = Resources.Load (file) as Texture2D;
		var bytes = texture.GetRawTextureData ();
		var stream = new System.IO.MemoryStream(bytes);
		var data = types.Data.fromMemoryStream (stream,  bytes.Length);

		return new vectorx.ColorStorage (texture.width, texture.height, data);
	}

	types.Data loadFont(string file)
	{
		return GetDataFromFile(file);
	}

	void TestFontRender()
	{
		var arialData = loadFont ("fonts/arial.ttf");
		var fontCache = new vectorx.font.FontCache (arialData);

		var str = "abcdefghjiklmnopqrstuvwxyz";
		var font = fontCache.createFontWithNameAndSize ("Arial", 16);
		var color = new Color4F (1.0f, 0.0f, 0.0f, 1.0f);
		var attrs = MainCs._createStringAttributes (new vectorx.font.AttributedRange(0, -1), font, color);
		var attrString = new vectorx.font.AttributedString (str, attrs);

		var colorStorage = new vectorx.ColorStorage (512, 512, null);
		var context = new vectorx.font.FontContext ();
		context.renderStringToColorStorage (attrString, colorStorage, null, null, null);

		var texture = createTexture (colorStorage);
		GetComponent<Renderer> ().material.mainTexture = texture;

		System.Console.Write ("");
	}

	//private static function loadImage(file: String, origDimensions: Vector2, dimensions: Vector2): ColorStorage
	vectorx.ColorStorage loadImage(string file, types.Vector2 srcDim, types.Vector2 dstDim)
	{
		if (!file.EndsWith("svg.xml"))
		{
			return GetBitmapDataFromFile (file);
		}

		var colorStorage = new vectorx.ColorStorage ((int)dstDim.x, (int)dstDim.y, null);

		var asset = Resources.Load(file) as TextAsset;
		var xml = Xml.parse (asset.text);
		var svg = vectorx.svg.SvgContext.parseSvg (xml);

		var scaleX = dstDim.x / srcDim.x;
		var scaleY = dstDim.y / srcDim.y;
		var transform = lib.ha.core.geometry.AffineTransformer.scaler (scaleX, scaleY);

		var svgContext = new vectorx.svg.SvgContext ();
		svgContext.renderVectorBinToColorStorage (svg, colorStorage, transform);

		return colorStorage;
	}

	void TestStyledString()
	{
		var configAsset = Resources.Load("Fonts/styledStringContext.json") as TextAsset;
		var context = MainCs.createStyledStringContext (configAsset.text, this);
		var str = "[f=arial_24,c=white]a[f=arial_28]bc[/f]djf{warn}lkdsjf{texture}lkfdsef{calc}[/]";
		var storage = new vectorx.ColorStorage (512, 512, null);
		context.renderStringToColorStorage (str, storage, null, null);
	}

	void Start () 
	{
		UnitySystemConsoleRedirector.Redirect();
		System.Console.WriteLine ("Start () ");
		//TestFloat ();
		//TestData();
		//TestColorStorage();
		//TestPixelFormatRenderer();
		//DataTest.testAll ();

		//TestSvg();
		TestFontRender();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
