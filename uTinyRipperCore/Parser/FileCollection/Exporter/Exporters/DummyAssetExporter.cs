using System;
using System.Collections.Generic;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.AssetExporters
{
	internal class DummyAssetExporter : IAssetExporter
	{
		public void SetUpClassType(ClassIDType classType, bool isEmptyCollection, bool isMetaType)
		{
			m_emptyTypes[classType] = isEmptyCollection;
			m_metaTypes[classType] = isMetaType;
		}

		public bool IsHandle(Object asset)
		{
			return true;
		}

		public void Export(IExportContainer container, Object asset, string path)
		{
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset, List<Object> depList)
		{
			if (m_metaTypes.TryGetValue(asset.ClassID, out bool isEmptyCollection))
			{
				if (isEmptyCollection)
				{
					return new EmptyExportCollection();
				}
				else
				{
					return new SkipExportCollection(this, asset);
				}
			}
			else
			{
				throw new NotSupportedException(asset.ClassID.ToString());
			}
		}

		public AssetType ToExportType(Object asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			if (m_metaTypes.TryGetValue(classID, out bool isMetaType))
			{
				assetType = isMetaType ? AssetType.Meta : AssetType.Serialized;
				return true;
			}
			else
			{
				throw new NotSupportedException(classID.ToString());
			}
		}

		private readonly Dictionary<ClassIDType, bool> m_emptyTypes = new Dictionary<ClassIDType, bool>();
		private readonly Dictionary<ClassIDType, bool> m_metaTypes = new Dictionary<ClassIDType, bool>();
	}
}
