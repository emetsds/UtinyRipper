using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Shaders;
using uTinyRipper.Classes.Shaders.Exporters;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;
using Version = uTinyRipper.Version;

namespace uTinyRipperGUI.Exporters
{
	public class ShaderAssetExporter : IAssetExporter
	{
		public bool IsHandle(Object asset)
		{
			return true;
		}

		public void Export(IExportContainer container, Object asset, string path)
		{
			Export(container, asset, path, null);
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				Shader shader = (Shader)asset;
				shader.ExportBinary(container, fileStream, ShaderExporterInstantiator);
			}
			callback?.Invoke(container, asset, path);
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset, List<Object> depList)
		{
			return new AssetExportCollection(this, asset);
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

		private static ShaderTextExporter ShaderExporterInstantiator(Version version, GPUPlatform graphicApi)
		{
			switch (graphicApi)
			{
				case GPUPlatform.d3d9:
				case GPUPlatform.d3d11_9x:
				case GPUPlatform.d3d11:
					return new ShaderDXExporter(version, graphicApi);

				case GPUPlatform.vulkan:
					return new ShaderVulkanExporter();

				default:
					return Shader.DefaultShaderExporterInstantiator(version, graphicApi);
			}
		}

	}
}
