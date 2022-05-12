using Fab.Lua.Core;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Fab.Geo.Lua.Interop
{
	[LuaHelpInfo("Module for loading image and text")]
	public class IO : LuaObject, ILuaObjectInitialize
	{
		private const string DefaultImageFormat = "RGBA8";
		private const GraphicsFormat DefaultImageGraphicsFormats = GraphicsFormat.R8G8B8A8_UNorm;
		private static readonly HashSet<string> imageExtensions = new HashSet<string>() { ".jpg", ".jpeg", ".png" };
		private static readonly HashSet<string> textExtensions = new HashSet<string>() { ".txt", ".json", ".geojson" };

		[LuaHelpInfo("Returns the directory path that data can be loaded from (read only)")]
		public string data_dir => LuaEnvironment.DataDirectory;

		public void Initialize()
		{

		}

		[LuaHelpInfo("Loads a text(txt, json, geojson) or image file(jpg, png) from the data path")]
		public object load(string file_path)
		{
			CheckLoadPath(file_path, out string loadPath, out string ext);

			if (imageExtensions.Contains(ext))
				return load_image_internal(loadPath);
			else if (textExtensions.Contains(ext))
				return load_text_internal(loadPath);

			throw new ArgumentException($"Loading failed. The file extension \"{ext}\" is not supported");
		}

		[LuaHelpInfo("Loads an image file (jpg, png) from the data path. " +
			"You can optionally specify a format for the image. " +
			"Currently supported formats are: R8, RG8, and RGBA8 (default)")]
		public ImageProxy load_image(string file_path, string format = DefaultImageFormat)
		{
			CheckLoadPath(file_path, out string loadPath, out string ext);
			if (imageExtensions.Contains(ext))
				return load_image_internal(loadPath, format);

			throw new ArgumentException($"Loading image failed. The image file type \"{ext}\" is not supported. Supported file types are: "
				+ string.Join(", ", imageExtensions));
		}

		public static void CheckLoadPath(string path, out string loadPath, out string extension)
		{
			loadPath = Path.Combine(LuaEnvironment.DataDirectory, path);

			if (!File.Exists(loadPath))
				throw new ArgumentException($"The file path \"{loadPath}\" does not exist");

			extension = Path.GetExtension(path);

			if (string.IsNullOrEmpty(extension))
				throw new ArgumentException($"The file path is missing a file extension");
		}

		private ImageProxy load_image_internal(string loadPath, string format = DefaultImageFormat)
		{
			Texture2D tex = new Texture2D(2, 2);
			tex.LoadImage(File.ReadAllBytes(loadPath));
			ImageProxy proxy = new ImageProxy();
			if (format == DefaultImageFormat)
			{
				tex.name = Path.GetFileNameWithoutExtension(loadPath);
				proxy.SetTarget(tex);
				return proxy;
			}

			GraphicsFormat graphicsFormat = GetGraphicsFormatFromString(format);

			if (graphicsFormat == GraphicsFormat.None)
				throw new ArgumentException("The supplied image format is not supported");

			Texture2D dst = new Texture2D(tex.width, tex.height, graphicsFormat, TextureCreationFlags.None);
			dst.SetPixels32(tex.GetPixels32(0));
			dst.Apply();
			dst.name = Path.GetFileNameWithoutExtension(loadPath);
			UnityEngine.Object.Destroy(tex);
			proxy.SetTarget(dst);
			return proxy;
		}

		private string load_text_internal(string loadPath)
		{
			return File.ReadAllText(loadPath);
		}

		private GraphicsFormat GetGraphicsFormatFromString(string format)
		{
			switch (format.ToUpper())
			{
				case "R":
				case "R8":
					return GraphicsFormat.R8_UNorm;
				case "RG":
				case "RG8":
					return GraphicsFormat.R8G8_UNorm;
				case "RGBA":
				case "RGBA8":
					return GraphicsFormat.R8G8B8A8_UNorm;
				default:
					return GraphicsFormat.None;
			}
		}
	}
}
