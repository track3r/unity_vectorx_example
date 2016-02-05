using System;
using UnityEngine;
using System.Collections;

namespace Vectorx
{
	public class ResourceProvider: StyledStringResourceProvider
	{
		public ResourceProvider ()
		{
		
		}

		public types.Data GetDataFromFile(string file)
		{
			var asset = Resources.Load(file) as TextAsset;
			var stream = new System.IO.MemoryStream (asset.bytes);
			var data = types.Data.fromMemoryStream (stream,  asset.bytes.Length);

			return data;
		}

		public vectorx.ColorStorage GetBitmapDataFromFile(string file)
		{
			Debug.Log ("Loading bitmap: " + file);

			var asset = Resources.Load (file) as TextAsset;
			var texture = new Texture2D (2, 2);
			texture.LoadImage (asset.bytes);
			var bytes = texture.GetRawTextureData ();
			var stream = new System.IO.MemoryStream(bytes);
			var data = types.Data.fromMemoryStream (stream,  bytes.Length);

			return new vectorx.ColorStorage (texture.width, texture.height, data);
		}

		public override types.Data loadFont(string file)
		{
			return GetDataFromFile(file);
		}

		//private static function loadImage(file: String, origDimensions: Vector2, dimensions: Vector2): ColorStorage
		public override vectorx.ColorStorage loadImage(string file, types.Vector2 srcDim, types.Vector2 dstDim)
		{
			if (!file.EndsWith(".svg"))
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
	}
}

