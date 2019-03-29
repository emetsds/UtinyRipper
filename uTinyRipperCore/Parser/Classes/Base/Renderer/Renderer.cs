using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.MeshRenderers;
using uTinyRipper.Classes.Renderers;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public abstract class Renderer : Component
	{
		protected Renderer(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadDynamicOccludee(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadMotionVector(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadRenderingLayerMask(Version version)
		{
			return version.IsGreaterEqual(2018);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadRendererPriority(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool IsReadLightmapIndex(Version version, TransferInstructionFlags flags)
		{
#warning TODO: separate by version
			return version.IsGreaterEqual(2, 1) && (flags.IsRelease() || flags.IsForInspector());
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLightmapIndexDynamic(Version version, TransferInstructionFlags flags)
		{
#warning TODO: separate by version
			return version.IsGreaterEqual(5) && (flags.IsRelease() || flags.IsForInspector());
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool IsReadLightmapTilingOffset(Version version, TransferInstructionFlags flags)
		{
#warning TODO: separate by version
			return version.IsGreaterEqual(2, 1) && (flags.IsRelease() || flags.IsForInspector());
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLightmapTilingOffsetDynamic(Version version, TransferInstructionFlags flags)
		{
#warning TODO: separate by version
			return version.IsGreaterEqual(5) && (flags.IsRelease() || flags.IsForInspector());
		}
		/// <summary>
		/// 3.0.0 to 5.5.0
		/// </summary>
		public static bool IsReadSubsetIndices(Version version)
		{
			return version.IsGreaterEqual(3) && version.IsLess(5, 5);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadStaticBatchInfo(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 3.0.0
		/// </summary>
		public static bool IsReadStaticBatchRoot(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 3.5.0 to 5.4.0
		/// </summary>
		public static bool IsReadUseLight(Version version)
		{
			return version.IsGreaterEqual(3, 5) && version.IsLess(5, 4);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadReflectUsage(Version version)
		{
			return version.IsGreaterEqual(5, 0);
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadProbeAnchor(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadLightOverride(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		public static bool IsReadScaleInLightmap(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}
		/// <summary>
		/// 4.5.0 and greater but exluding 5.6.0b
		/// </summary>
		public static bool IsReadSortingLayerID(Version version)
		{
			if (version.IsGreaterEqual(4, 5))
			{
				if (version.IsLess(5, 6))
				{
					return true;
				}
				if (version.IsGreater(5, 6, 0, VersionType.Beta))
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 4.3.x or 5.6.0 and greater
		/// </summary>
		public static bool IsReadSortingLayer(Version version)
		{
			return version.IsEqual(4, 3) || version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadSortingOrder(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		
		/// <summary>
		/// 5.0.0 to 5.3.x
		/// </summary>
		private static bool IsAlignEnabled(Version version)
		{
			return version.IsGreaterEqual(5) && version.IsLess(5, 4);
		}
		/// <summary>
		/// Less 5.0.0
		/// </summary>
		private static bool IsByteLightIndex(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		private static bool IsReadMaterialFirst(Version version)
		{
			return version.IsLess(3);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsAlignLightProbe(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		private static bool IsAlignSortingOrder(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadReflectUsageFirst(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}

		public string FindMaterialPropertyNameByCRC28(uint crc)
		{
			foreach(PPtr<Material> materialPtr in m_materials)
			{
				Material material = materialPtr.FindAsset(File);
				if(material == null)
				{
					continue;
				}
				string property = material.FindPropertyNameByCRC28(crc);
				if (property == null)
				{
					continue;
				}

				return property;
			}
			return null;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Enabled = reader.ReadBoolean();
			if (IsAlignEnabled(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			CastShadows = (ShadowCastingMode)reader.ReadByte();
			ReceiveShadows = reader.ReadByte();
			if (IsReadDynamicOccludee(reader.Version))
			{
				DynamicOccludee = reader.ReadByte();
			}
			if (IsAlignEnabled(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadMotionVector(reader.Version))
			{
				MotionVectors = (MotionVectorGenerationMode)reader.ReadByte();
				LightProbeUsage = (LightProbeUsage)reader.ReadByte();
			}
			if (IsReadReflectUsage(reader.Version))
			{
				if (IsReadReflectUsageFirst(reader.Version))
				{
					ReflectionProbeUsage = (ReflectionProbeUsage)reader.ReadByte();
					reader.AlignStream(AlignType.Align4);
				}
			}

			if (IsReadRenderingLayerMask(reader.Version))
			{
				RenderingLayerMask = reader.ReadUInt32();
			}
			if (IsReadRendererPriority(reader.Version))
			{
				RendererPriority = reader.ReadInt32();
			}

			if (IsReadLightmapIndex(reader.Version, reader.Flags))
			{
				LightmapIndex = IsByteLightIndex(reader.Version) ? reader.ReadByte() : reader.ReadUInt16();
			}

			if (IsReadLightmapIndexDynamic(reader.Version, reader.Flags))
			{
				LightmapIndexDynamic = reader.ReadUInt16();
			}

			if (IsReadMaterialFirst(reader.Version))
			{
				m_materials = reader.ReadAssetArray<PPtr<Material>>();
			}

			if (IsReadLightmapTilingOffset(reader.Version, reader.Flags))
			{
				LightmapTilingOffset.Read(reader);
			}
			if (IsReadLightmapTilingOffsetDynamic(reader.Version, reader.Flags))
			{
				LightmapTilingOffsetDynamic.Read(reader);
			}

			if (!IsReadMaterialFirst(reader.Version))
			{
				m_materials = reader.ReadAssetArray<PPtr<Material>>();
			}

			if (IsReadSubsetIndices(reader.Version))
			{
				m_subsetIndices = reader.ReadUInt32Array();
			}
			if (IsReadStaticBatchInfo(reader.Version))
			{
				StaticBatchInfo.Read(reader);
			}

			if (IsReadStaticBatchRoot(reader.Version))
			{
				StaticBatchRoot.Read(reader);
			}

			if (IsReadUseLight(reader.Version))
			{
				UseLightProbes = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadReflectUsage(reader.Version))
			{
				if (!IsReadReflectUsageFirst(reader.Version))
				{
					ReflectionProbeUsage = (ReflectionProbeUsage)reader.ReadInt32();
				}
			}

			if (IsReadProbeAnchor(reader.Version))
			{
				ProbeAnchor.Read(reader);
			}
			if (IsReadLightOverride(reader.Version))
			{
				LightProbeVolumeOverride.Read(reader);
			}
#if UNIVERSAL
			if (IsReadScaleInLightmap(reader.Flags))
			{
#warning TODO: separate by version
				ScaleInLightmap = reader.ReadSingle();
				PreserveUVs = reader.ReadBoolean();
				IgnoreNormalsForChartDetection = reader.ReadBoolean();
				ImportantGI = reader.ReadBoolean();
				StitchLightmapSeams = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);

				SelectedEditorRenderState = (EditorSelectedRenderState)reader.ReadInt32();
				MinimumChartSize = reader.ReadInt32();
				AutoUVMaxDistance = reader.ReadSingle();
				AutoUVMaxAngle = reader.ReadSingle();
				LightmapParameters.Read(reader);
			}
#endif
			if (IsAlignLightProbe(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadSortingLayerID(reader.Version))
			{
				SortingLayerID = reader.ReadInt32();
			}
			if (IsReadSortingLayer(reader.Version))
			{
				SortingLayer = reader.ReadInt16();
			}
			if (IsReadSortingOrder(reader.Version))
			{
				SortingOrder = reader.ReadInt16();
			}
			if (IsAlignSortingOrder(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			foreach (PPtr<Material> material in Materials)
			{
				yield return material.FetchDependency(file, isLog, ToLogString, "m_Materials");
			}
			if (!StaticBatchRoot.IsNull)
			{
				yield return StaticBatchRoot.GetAsset(file);
			}
			if(!ProbeAnchor.IsNull)
			{
				yield return ProbeAnchor.GetAsset(file);
			}
			if(!LightProbeVolumeOverride.IsNull)
			{
				yield return LightProbeVolumeOverride.GetAsset(file);
			}
		}

		protected void ReadBase(AssetReader reader)
		{
			base.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(EnabledName, Enabled);
			node.Add(CastShadowsName, (byte)CastShadows);
			node.Add(CastShadowsName, ReceiveShadows);
			node.Add(DynamicOccludeeName, GetDynamicOccludee(container.Version));
			node.Add(MotionVectorsName, (byte)GetMotionVectors(container.Version));
			node.Add(LightProbeUsageName, (byte)LightProbeUsage);
			node.Add(ReflectionProbeUsageName, (byte)GetReflectionProbeUsage(container.Version));
			if (IsReadRenderingLayerMask(container.ExportVersion))
			{
				node.Add(RenderingLayerMaskName, GetRenderingLayerMask(container.Version));
			}
			if (IsReadRendererPriority(container.ExportVersion))
			{
				node.Add(RendererPriorityName, RendererPriority);
			}
			node.Add(MaterialsName, Materials.ExportYAML(container));
			node.Add(StaticBatchInfoName, GetStaticBatchInfo(container.Version).ExportYAML(container));
			node.Add(StaticBatchRootName, StaticBatchRoot.ExportYAML(container));
			node.Add(ProbeAnchorName, ProbeAnchor.ExportYAML(container));
			node.Add(LightProbeVolumeOverrideName, LightProbeVolumeOverride.ExportYAML(container));
			node.Add(ScaleInLightmapName, GetScaleInLightmap(container.Flags));
			node.Add(PreserveUVsName, GetPreserveUVs(container.Flags));
			node.Add(IgnoreNormalsForChartDetectionName, GetIgnoreNormalsForChartDetection(container.Flags));
			node.Add(ImportantGIName, GetImportantGI(container.Flags));
			node.Add(StitchLightmapSeamsName, GetStitchLightmapSeams(container.Flags));
			node.Add(SelectedEditorRenderStateName, (int)GetSelectedEditorRenderState(container.Flags));
			node.Add(MinimumChartSizeName, GetMinimumChartSize(container.Flags));
			node.Add(AutoUVMaxDistanceName, GetAutoUVMaxDistance(container.Flags));
			node.Add(AutoUVMaxAngleName, GetAutoUVMaxAngle(container.Flags));
			node.Add(LightmapParametersName, GetLightmapParameters(container.Flags).ExportYAML(container));
			node.Add(SortingLayerIDName, SortingLayerID);
			node.Add(SortingLayerName, SortingLayer);
			node.Add(SortingOrderName, SortingOrder);
			return node;
		}

		private int GetDynamicOccludee(Version version)
		{
			return IsReadDynamicOccludee(version) ? DynamicOccludee : 1;
		}
		private MotionVectorGenerationMode GetMotionVectors(Version version)
		{
			return IsReadMotionVector(version) ? MotionVectors : MotionVectorGenerationMode.Object;
		}
		private ReflectionProbeUsage GetReflectionProbeUsage(Version version)
		{
			return IsReadReflectUsage(version) ? ReflectionProbeUsage : ReflectionProbeUsage.BlendProbes;
		}
		private uint GetRenderingLayerMask(Version version)
		{
			return IsReadRenderingLayerMask(version) ? RenderingLayerMask : 1;
		}
		private StaticBatchInfo GetStaticBatchInfo(Version version)
		{
			return IsReadSubsetIndices(version) ? new StaticBatchInfo(SubsetIndices) : StaticBatchInfo;
		}
		private float GetScaleInLightmap(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if(IsReadScaleInLightmap(flags))
			{
				return ScaleInLightmap;
			}
#endif
			return 1.0f;
		}
		private bool GetPreserveUVs(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if(IsReadScaleInLightmap(flags))
			{
				return PreserveUVs;
			}
#endif
			return false;
		}
		private bool GetIgnoreNormalsForChartDetection(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if(IsReadScaleInLightmap(flags))
			{
				return IgnoreNormalsForChartDetection;
			}
#endif
			return false;
		}
		private bool GetImportantGI(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if(IsReadScaleInLightmap(flags))
			{
				return ImportantGI;
			}
#endif
			return false;
		}
		private bool GetStitchLightmapSeams(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if(IsReadScaleInLightmap(flags))
			{
				return StitchLightmapSeams;
			}
#endif
			return false;
		}		
		private EditorSelectedRenderState GetSelectedEditorRenderState(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if(IsReadScaleInLightmap(flags))
			{
				return SelectedEditorRenderState;
			}
#endif
			return (EditorSelectedRenderState)3;
		}
		private int GetMinimumChartSize(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadScaleInLightmap(flags))
			{
				return MinimumChartSize;
			}
#endif
			return 4;
		}
		private float GetAutoUVMaxDistance(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadScaleInLightmap(flags))
			{
				return AutoUVMaxDistance;
			}
#endif
			return 0.5f;
		}
		private float GetAutoUVMaxAngle(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadScaleInLightmap(flags))
			{
				return AutoUVMaxAngle;
			}
#endif
			return 89.0f;
		}
		private PPtr<LightmapParameters> GetLightmapParameters(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			return LightmapParameters;
#else
			return default;
#endif
		}

		public bool Enabled { get; private set; }
		public ShadowCastingMode CastShadows { get; private set; }
		public byte ReceiveShadows { get; private set; }
		public byte DynamicOccludee { get; private set; }
		public MotionVectorGenerationMode MotionVectors { get; private set; }
		public LightProbeUsage LightProbeUsage { get; private set; }
		public ReflectionProbeUsage ReflectionProbeUsage { get; private set; }
		public uint RenderingLayerMask { get; private set; }
		public int RendererPriority { get; private set; }
		public ushort LightmapIndex { get; private set; }
		public ushort LightmapIndexDynamic { get; private set; }
		public IReadOnlyList<PPtr<Material>> Materials => m_materials;
		public IReadOnlyList<uint> SubsetIndices => m_subsetIndices;
		public bool UseLightProbes { get; private set; }
#if UNIVERSAL
		public float ScaleInLightmap { get; private set; }
		public bool PreserveUVs { get; private set; }
		public bool IgnoreNormalsForChartDetection { get; private set; }
		public bool ImportantGI { get; private set; }
		public bool StitchLightmapSeams { get; private set; }

		public EditorSelectedRenderState SelectedEditorRenderState { get; private set; }
		public int MinimumChartSize { get; private set; }
		public float AutoUVMaxDistance { get; private set; }
		public float AutoUVMaxAngle { get; private set; }
#endif
		public int SortingLayerID { get; private set; }
		public short SortingLayer { get; private set; }
		public short SortingOrder { get; private set; }

		public const string EnabledName = "m_Enabled";
		public const string CastShadowsName = "m_CastShadows";
		public const string ReceiveShadowsName = "m_ReceiveShadows";
		public const string DynamicOccludeeName = "m_DynamicOccludee";
		public const string MotionVectorsName = "m_MotionVectors";
		public const string LightProbeUsageName = "m_LightProbeUsage";
		public const string ReflectionProbeUsageName = "m_ReflectionProbeUsage";
		public const string RenderingLayerMaskName = "m_RenderingLayerMask";
		public const string RendererPriorityName = "m_RendererPriority";
		public const string MaterialsName = "m_Materials";
		public const string StaticBatchInfoName = "m_StaticBatchInfo";
		public const string StaticBatchRootName = "m_StaticBatchRoot";
		public const string ProbeAnchorName = "m_ProbeAnchor";
		public const string LightProbeVolumeOverrideName = "m_LightProbeVolumeOverride";
		public const string ScaleInLightmapName = "m_ScaleInLightmap";
		public const string PreserveUVsName = "m_PreserveUVs";
		public const string IgnoreNormalsForChartDetectionName = "m_IgnoreNormalsForChartDetection";
		public const string ImportantGIName = "m_ImportantGI";
		public const string StitchLightmapSeamsName = "m_StitchLightmapSeams";
		public const string SelectedEditorRenderStateName = "m_SelectedEditorRenderState";
		public const string MinimumChartSizeName = "m_MinimumChartSize";
		public const string AutoUVMaxDistanceName = "m_AutoUVMaxDistance";
		public const string AutoUVMaxAngleName = "m_AutoUVMaxAngle";
		public const string LightmapParametersName = "m_LightmapParameters";
		public const string SortingLayerIDName = "m_SortingLayerID";
		public const string SortingLayerName = "m_SortingLayer";
		public const string SortingOrderName = "m_SortingOrder";

		public Vector4f LightmapTilingOffset;
		public Vector4f LightmapTilingOffsetDynamic;
		public StaticBatchInfo StaticBatchInfo;
		public PPtr<Transform> StaticBatchRoot;
		/// <summary>
		/// LightProbeAnchor previously
		/// </summary>
		public PPtr<Transform> ProbeAnchor;
		public PPtr<GameObject> LightProbeVolumeOverride;
#if UNIVERSAL
		public PPtr<LightmapParameters> LightmapParameters;
#endif

		private PPtr<Material>[] m_materials;
		private uint[] m_subsetIndices;
	}
}
