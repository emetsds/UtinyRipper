using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.AssetExporters
{
	public class BinaryAssetExporter : IAssetExporter
	{
		public virtual bool IsHandle(Object asset, ExportOptions options)
		{
			return true;
		}

		public bool Export(IExportContainer container, Object asset, string path)
		{
			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				asset.ExportBinary(container, fileStream);
			}
			return true;
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public virtual IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset, List<Object> depList)
		{
			switch(asset.ClassID)
			{
				case ClassIDType.Texture2D:
				case ClassIDType.Cubemap:
					return new TextureExportCollection(this, (Texture2D)asset, false, depList);

				default:
					return new AssetExportCollection(this, asset);
			}
		}

		public AssetType ToExportType(Object asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}
