using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.Classes;
using uTinyRipper.Classes.SpriteAtlases;

namespace uTinyRipper.AssetExporters
{
	public class TextureExportCollection : AssetsExportCollection
	{
		public TextureExportCollection(
            IAssetExporter assetExporter,
            Texture2D texture,
            bool convert,
            List<Object> depList):
			base(assetExporter, texture, CreateImporter(texture, convert))
		{
			m_convert = convert;
			if (convert)
			{
				TextureImporter textureImporter = (TextureImporter)MetaImporter;
				List<Sprite> sprites = new List<Sprite>();
				foreach (Object asset in depList)
				{
					switch (asset.ClassID)
					{
						case ClassIDType.Sprite:
							{
								Sprite sprite = (Sprite)asset;
								if (sprite.RD.Texture.IsAsset(sprite.File, texture))
								{
									sprites.Add(sprite);
									AddAsset(sprite);
								}
							}
							break;

						case ClassIDType.SpriteAtlas:
							{
								int index = 0;
								SpriteAtlas atlas = (SpriteAtlas)asset;
								foreach (SpriteAtlasData atlasData in atlas.RenderDataMap.Values)
								{
									if (atlasData.Texture.IsAsset(atlas.File, texture))
									{
										Sprite sprite = atlas.PackedSprites[index].GetAsset(atlas.File);
										sprites.Add(sprite);
										AddAsset(sprite);
									}
									index++;
								}
							}
							break;
					}
				}
                //TODO: sort then call AddAsset for proper ids
				textureImporter.Sprites = sprites;
			}
		}

		public static IExportCollection CreateExportCollection(
            IAssetExporter assetExporter,
            Sprite asset,
            List<Object> depList)
		{
			Texture2D texture = asset.RD.Texture.FindAsset(asset.File);
			if (texture == null)
			{
				return new SkipExportCollection(assetExporter, asset);
			}
			return new TextureExportCollection(assetExporter, texture, true, depList);
		}

		private static IAssetImporter CreateImporter(Texture2D texture, bool convert)
		{
			if (convert)
			{
				return new TextureImporter(texture);
			}
			else
			{
				return new IHVImageFormatImporter(texture);
			}
		}

		protected override string ExportInner(ProjectAssetContainer container, string filePath)
		{
			if(m_convert)
			{
				string dirPath = Path.GetDirectoryName(filePath);
				string fileName = Path.GetFileNameWithoutExtension(filePath);
				filePath = $"{Path.Combine(dirPath, fileName)}.png";
			}
			AssetExporter.Export(container, Asset, filePath);
			return filePath;
		}

		protected override long GenerateExportID(Object asset)
		{
			long exportID = GetMainExportID(asset, m_nextExportID);
			m_nextExportID += 2;
			return exportID;
		}

		private uint m_nextExportID = 0;
		private bool m_convert = false;
	}
}
