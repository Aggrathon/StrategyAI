using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName="Data/Map List")]
public class MapList : ScriptableObject {

	public Texture2D[] maps;
	public bool flipX = true;
	public bool flipY = false;
	public bool switchTeams = true;

	private List<Texture2D> mapCache;

	public Texture2D GetMap()
	{
		if (mapCache != null && mapCache.Count > 0)
		{
			var m = mapCache[Random.Range(0, mapCache.Count)];
			if (m == null)
			{
				mapCache.RemoveAll(f => f == null);
				return GetMap();
			}
			return m;
		}
		Texture2D mx, my, ms, mxy, mys, mxs, mxys;
		mapCache = new List<Texture2D>();

		foreach (var map in maps)
		{
			mx = flipX ? new Texture2D(map.width, map.height, map.format, false) : null;
			my = flipY ? new Texture2D(map.width, map.height, map.format, false) : null;
			ms = switchTeams ? new Texture2D(map.width, map.height, map.format, false) : null;
			mxy = (flipX && flipY) ? new Texture2D(map.width, map.height, map.format, false) : null;
			mys = (flipY && switchTeams) ? new Texture2D(map.width, map.height, map.format, false) : null;
			mxs = (flipX && switchTeams) ? new Texture2D(map.width, map.height, map.format, false) : null;
			mxys = (flipX && flipY && switchTeams) ? new Texture2D(map.width, map.height, map.format, false) : null;
			for (int j = 0; j < map.height; j++)
			{
				for (int i = 0; i < map.width; i++)
				{
					Color c = map.GetPixel(i, j);
					int i_ = map.width - 1 - i;
					int j_ = map.height - 1 - j;
					Color c_ = new Color(c.b, c.g, c.r);
					if (mx != null)
						mx.SetPixel(i_, j, c);
					if (my != null)
						my.SetPixel(i, j_, c);
					if (ms != null)
						ms.SetPixel(i, j, c_);
					if (mxy != null)
						mxy.SetPixel(i_, j_, c);
					if (mys != null)
						mys.SetPixel(i, j_, c_);
					if (mxs != null)
						mxs.SetPixel(i_, j, c_);
					if (mxys != null)
						mxys.SetPixel(i_, j_, c_);
				}
			}
			if (mx != null)
			{
				mx.Apply(false, false);
				mapCache.Add(mx);
			}
			if (my != null)
			{
				my.Apply(false, false);
				mapCache.Add(my);
			}
			if (ms != null)
			{
				ms.Apply(false, false);
				mapCache.Add(ms);
			}
			if (mxy != null)
			{
				mxy.Apply(false, false);
				mapCache.Add(mxy);
			}
			if (mys != null)
			{
				mys.Apply(false, false);
				mapCache.Add(mys);
			}
			if (mxs != null)
			{
				mxs.Apply(false, false);
				mapCache.Add(mxs);
			}
			if (mxys != null)
			{
				mxys.Apply(false, false);
				mapCache.Add(mxys);
			}
		}
		return GetMap();
	}

#if UNITY_EDITOR
	[ContextMenu("Format Textures")]
	public void FormatTextures()
	{
		foreach(var m in maps)
		{
			TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(m));
			importer.wrapMode = TextureWrapMode.Clamp;
			importer.textureType = TextureImporterType.Default;
			importer.mipmapEnabled = false;
			importer.textureShape = TextureImporterShape.Texture2D;
			importer.sRGBTexture = true;
			importer.filterMode = FilterMode.Point;
			importer.anisoLevel = 0;
			importer.isReadable = true;
			importer.npotScale = TextureImporterNPOTScale.None;
			importer.textureCompression = TextureImporterCompression.Uncompressed;
			importer.SaveAndReimport();
		}
		mapCache = null;
	}
#endif
}
