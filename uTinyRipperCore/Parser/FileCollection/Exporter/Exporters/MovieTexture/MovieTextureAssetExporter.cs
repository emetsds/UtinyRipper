using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.AssetExporters
{
	public sealed class MovieTextureAssetExporter : BinaryAssetExporter
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset, List<Object> depList)
		{
			return new MovieTextureExportCollection(this, (MovieTexture)asset);
		}
	}
}
