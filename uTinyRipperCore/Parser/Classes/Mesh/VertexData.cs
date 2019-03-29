using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct VertexData : IAssetReadable, IYAMLExportable
	{
		public VertexData(Version version, IReadOnlyList<Vector3f> vertices, IReadOnlyList<Vector3f> normals, IReadOnlyList<ColorRGBA32> colors,
			IReadOnlyList<Vector2f> uv0, IReadOnlyList<Vector2f> uv1, IReadOnlyList<Vector4f> tangents)
		{
			BitArray curChannels = new BitArray(32);
			byte stride = 0;

			bool isWriteVertices = vertices.Count > 0;
			bool isWriteNormals = normals != null && normals.Count > 0;
			bool isWriteColors = colors.Count > 0;
			bool isWriteUV0 = uv0.Count > 0;
			bool isWriteUV1 = uv1 != null && uv1.Count > 0;
			bool isWriteTangents = tangents != null && tangents.Count > 0;

			if (isWriteVertices)
			{
				curChannels.Set((int)ChannelTypeV4.Vertex, true);
				stride += ChannelTypeV4.Vertex.GetStride();
			}
			if (isWriteNormals)
			{
				curChannels.Set((int)ChannelTypeV4.Normal, true);
				stride += ChannelTypeV4.Normal.GetStride();
			}
			if(isWriteColors)
			{
				curChannels.Set((int)ChannelTypeV4.Color, true);
				stride += ChannelTypeV4.Color.GetStride();
			}
			if (isWriteUV0)
			{
				curChannels.Set((int)ChannelTypeV4.UV0, true);
				stride += ChannelTypeV4.UV0.GetStride();
			}
			if (isWriteUV1)
			{
				curChannels.Set((int)ChannelTypeV4.UV1, true);
				stride += ChannelTypeV4.UV1.GetStride();
			}
			if (isWriteTangents)
			{
				curChannels.Set((int)ChannelTypeV4.Tangent, true);
				stride += ChannelTypeV4.Tangent.GetStride();
			}

			CurrentChannels = curChannels.ToUInt32();
			VertexCount = vertices.Count;
			m_channels = null;

			StreamInfo info = new StreamInfo(CurrentChannels, 0, stride);
			m_streams = new StreamInfo[] { info, default, default, default, };

			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					for (int i = 0; i < VertexCount; i++)
					{
						vertices[i].Write(writer);
						if (isWriteNormals)
						{
							normals[i].Write(writer);
						}
						if(isWriteColors)
						{
							colors[i].Write(writer);
						}
						if (isWriteUV0)
						{
							uv0[i].Write(writer);
						}
						if (isWriteUV1)
						{
							uv1[i].Write(writer);
						}
						if (isWriteTangents)
						{
							tangents[i].Write(writer);
						}
					}
				}
				m_data = stream.ToArray();
			}
		}

		/// <summary>
		/// Less than 2018.1
		/// </summary>
		public static bool IsReadCurrentChannels(Version version)
		{
			return version.IsLess(2018);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadChannels(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadStream(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// 3.5.0
		/// </summary>
		public static bool IsStreamSpecial(Version version)
		{
			return version.IsEqual(3, 5);
		}
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		private static bool IsStreamStatic(Version version)
		{
			return version.IsLess(4);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2018))
			{
				return 2;
			}
			return 1;
		}

		public Vector3f[] GenerateVertices(Version version, SubMesh submesh)
		{
			IReadOnlyList<ChannelInfo> channels = GetChannels(version);
			ChannelInfo channel = channels[(int)ChannelType.Vertex];
			if (!channel.IsSet)
			{
				throw new Exception("Vertices hasn't been found");
			}

			Vector3f[] verts = new Vector3f[submesh.VertexCount];
			int streamStride = channels.Where(t => t.Stream == channel.Stream).Sum(t => t.GetStride());
			int streamOffset = GetStreamOffset(channel.Stream, channels);
			using (MemoryStream memStream = new MemoryStream(m_data))
			{
				using (BinaryReader reader = new BinaryReader(memStream))
				{
					memStream.Position = streamOffset + submesh.FirstVertex * streamStride + channel.Offset;
					for (int v = 0; v < submesh.VertexCount; v++)
					{
						float x = reader.ReadSingle();
						float y = reader.ReadSingle();
						float z = reader.ReadSingle();
						verts[v] = new Vector3f(x, y, z);
						memStream.Position += streamStride - 12;
					}
				}
			}
			return verts;
		}

		public BoneWeights4[] GenerateSkin()
		{
			BoneWeights4[] skin = new BoneWeights4[VertexCount];
			ChannelInfo weightChannel = Channels[(int)ChannelTypeV2018.SkinWeight];
			ChannelInfo boneIndexChannel = Channels[(int)ChannelTypeV2018.SkinBoneIndex];
			if(!weightChannel.IsSet)
			{
				return new BoneWeights4[0];
			}

			int weightVertexSize = Channels.Where(t => t.Stream == weightChannel.Stream).Sum(t => t.GetStride());
			int weightStreamOffset = GetStreamOffset(weightChannel.Stream, Channels);
			int boneIndexVertexSize = Channels.Where(t => t.Stream == boneIndexChannel.Stream).Sum(t => t.GetStride());
			int boneIndexStreamOffset = GetStreamOffset(boneIndexChannel.Stream, Channels);
			using (MemoryStream memStream = new MemoryStream(m_data))
			{
				using (BinaryReader reader = new BinaryReader(memStream))
				{
					for (int v = 0; v < VertexCount; v++)
					{
						memStream.Position = weightStreamOffset + v * weightVertexSize + weightChannel.Offset;
						float w0 = reader.ReadSingle();
						float w1 = reader.ReadSingle();
						float w2 = reader.ReadSingle();
						float w3 = reader.ReadSingle();

						memStream.Position = boneIndexStreamOffset + v * boneIndexVertexSize + boneIndexChannel.Offset;
						int i0 = reader.ReadInt32();
						int i1 = reader.ReadInt32();
						int i2 = reader.ReadInt32();
						int i3 = reader.ReadInt32();

						skin[v] = new BoneWeights4(w0, w1, w2, w3, i0, i1, i2, i3);
					}
				}
			}
			return skin;
		}

		public void Read(AssetReader reader)
		{
			if (IsReadCurrentChannels(reader.Version))
			{
				CurrentChannels = reader.ReadUInt32();
			}
			VertexCount = (int)reader.ReadUInt32();

			if (IsReadChannels(reader.Version))
			{
				m_channels = reader.ReadAssetArray<ChannelInfo>();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadStream(reader.Version))
			{
				if (IsStreamStatic(reader.Version))
				{
					m_streams = new StreamInfo[4];
					for (int i = 0; i < 4; i++)
					{
						StreamInfo streamInfo = new StreamInfo();
						streamInfo.Read(reader);
						m_streams[i] = streamInfo;
					}
				}
				else
				{
					m_streams = reader.ReadAssetArray<StreamInfo>();
				}
			}

			m_data = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(CurrentChannelsName, GetCurrentChannels(container.Version));
			node.Add(VertexCountName, VertexCount);
			node.Add(ChannelsName, GetChannels(container.Version).ExportYAML(container));
			node.Add(DataSizeName, m_data.Length);
			node.Add(TypelessdataName, GetData(container.Version, container.Platform).ExportYAML());
			return node;
		}

		private uint GetCurrentChannels(Version version)
		{
			if(IsReadCurrentChannels(version))
			{
				if (IsReadStream(version))
				{
					BitArray curBits = CurrentChannelsBits;
					curBits.Set((int)ChannelType.Tangent, curBits.Get((int)ChannelTypeV4.Tangent));
					curBits.Set((int)ChannelTypeV4.Tangent, false);
					return curBits.ToUInt32();
				}
				else
				{
					return CurrentChannels;
				}
			}
			else
			{
				BitArray curChannels = new BitArray(32);
				for (int i = 0; i < Channels.Count; i++)
				{
					ChannelTypeV2018 channelType = (ChannelTypeV2018)i;
					switch(channelType)
					{
						case ChannelTypeV2018.Vertex:
						case ChannelTypeV2018.Normal:
						case ChannelTypeV2018.Color:
						case ChannelTypeV2018.UV0:
						case ChannelTypeV2018.UV1:
						case ChannelTypeV2018.UV2:
						case ChannelTypeV2018.UV3:
						case ChannelTypeV2018.Tangent:
							{
								ChannelInfo channel = Channels[i];
								curChannels[(int)channelType.ToChannelType()] |= channel.Dimension != 0;
							}
							break;
					}
				}
				return curChannels.ToUInt32();
			}
		}
		private IReadOnlyList<ChannelInfo> GetChannels(Version version)
		{
			if (IsReadChannels(version))
			{
				if(IsReadStream(version))
				{
					ChannelInfo[] channels = new ChannelInfo[8];
					channels[(int)ChannelType.Vertex] = Channels[(int)ChannelTypeV4.Vertex];
					channels[(int)ChannelType.Normal] = Channels[(int)ChannelTypeV4.Normal];
					// replace ChannelFormat.Color with 1 dimention to ChannelFormat.Byte with 4 dimention
					ChannelInfo cV4C = Channels[(int)ChannelTypeV4.Color];
					ChannelInfo colorChannel = new ChannelInfo(cV4C.Stream, cV4C.Offset, cV4C.IsSet ? ChannelFormat.Byte : 0, cV4C.IsSet ? (byte)4 : (byte)0);
					channels[(int)ChannelType.Color] = colorChannel;
					channels[(int)ChannelType.UV0] = Channels[(int)ChannelTypeV4.UV0];
					channels[(int)ChannelType.UV1] = Channels[(int)ChannelTypeV4.UV1];
					channels[(int)ChannelType.UV2] = new ChannelInfo(0, 0, 0, 0);
					channels[(int)ChannelType.UV3] = new ChannelInfo(0, 0, 0, 0);
					channels[(int)ChannelType.Tangent] = Channels[(int)ChannelTypeV4.Tangent];
					return channels;
				}
				else if (IsReadCurrentChannels(version))
				{
					return Channels;
				}
				else
				{
					ChannelInfo[] channels = new ChannelInfo[8];
					channels[(int)ChannelType.Vertex] = Channels[(int)ChannelTypeV2018.Vertex];
					channels[(int)ChannelType.Normal] = Channels[(int)ChannelTypeV2018.Normal];
					channels[(int)ChannelType.Color] = Channels[(int)ChannelTypeV2018.Color];
					channels[(int)ChannelType.UV0] = Channels[(int)ChannelTypeV2018.UV0];
					channels[(int)ChannelType.UV1] = Channels[(int)ChannelTypeV2018.UV1];
					channels[(int)ChannelType.UV2] = Channels[(int)ChannelTypeV2018.UV2];
					channels[(int)ChannelType.UV3] = Channels[(int)ChannelTypeV2018.UV3];
					channels[(int)ChannelType.Tangent] = Channels[(int)ChannelTypeV2018.Tangent];
					return channels;
				}
			}
			else
			{
				ChannelInfo[] channels = new ChannelInfo[8];
				channels[(int)ChannelType.Vertex] = CreateChannelFromStream(ChannelTypeV4.Vertex);
				channels[(int)ChannelType.Normal] = CreateChannelFromStream(ChannelTypeV4.Normal);
				channels[(int)ChannelType.Color] = CreateChannelFromStream(ChannelTypeV4.Color);
				channels[(int)ChannelType.UV0] = CreateChannelFromStream(ChannelTypeV4.UV0);
				channels[(int)ChannelType.UV1] = CreateChannelFromStream(ChannelTypeV4.UV1);
				channels[(int)ChannelType.UV2] = new ChannelInfo(0, 0, 0, 0);
				channels[(int)ChannelType.UV3] = new ChannelInfo(0, 0, 0, 0);
				channels[(int)ChannelType.Tangent] = CreateChannelFromStream(ChannelTypeV4.Tangent);
				return channels;
			}
		}

		private IReadOnlyList<byte> GetData(Version version, Platform platform)
		{
			if(platform == Platform.XBox360)
			{
				byte[] swapedData = new byte[m_data.Length];
				using (MemoryStream destStream = new MemoryStream(swapedData))
				{
					using (BinaryWriter destination = new BinaryWriter(destStream))
					{
						using (MemoryStream sourceStream = new MemoryStream(m_data))
						{
							using (EndianReader source = new EndianReader(sourceStream, EndianType.BigEndian))
							{
								CopyChannelsData(source, destination, version);
							}
						}
					}
				}
				return swapedData;
			}
			return m_data;
		}

		private BitArray CreateChannelsBits(uint channels)
		{
			return new BitArray(BitConverter.GetBytes(CurrentChannels));
		}

		private ChannelInfo CreateChannelFromStream(ChannelTypeV4 channelType)
		{
			int streamIndex = Streams.IndexOf(t => t.IsMatch(channelType));
			if (streamIndex == -1)
			{
				return new ChannelInfo(0, 0, 0, 0);
			}
			else
			{
				StreamInfo stream = Streams[streamIndex];
				byte offset = 0;
				for (ChannelTypeV4 type = 0; type < channelType; type++)
				{
					if(stream.IsMatch(type))
					{
						offset += type.GetStride();
					}
				}
				return new ChannelInfo((byte)streamIndex, offset, channelType.GetFormat(), channelType.GetDimention());
			}
		}

		private void CopyChannelsData(EndianReader source, BinaryWriter destination, Version version)
		{
			IReadOnlyList<ChannelInfo> channels = GetChannels(version);
			foreach (ChannelInfo channel in channels)
			{
				if (channel.IsSet)
				{
					int offset = GetStreamOffset(channel.Stream, channels) + channel.Offset;
					source.BaseStream.Position = offset;
					destination.BaseStream.Position = offset;
					int streamStride = channels.Where(t => t.Stream == channel.Stream).Sum(t => t.GetStride());
					switch (channel.Format)
					{
						case ChannelFormat.Float:
							for (int i = 0; i < VertexCount; i++)
							{
								for (int j = 0; j < channel.Dimension; j++)
								{
									destination.Write(source.ReadUInt32());
								}
								source.BaseStream.Position += streamStride - 4 * channel.Dimension;
								destination.BaseStream.Position = source.BaseStream.Position;
							}
							break;
						case ChannelFormat.Float16:
							for (int i = 0; i < VertexCount; i++)
							{
								for (int j = 0; j < channel.Dimension; j++)
								{
									destination.Write(source.ReadUInt16());
								}
								source.BaseStream.Position += streamStride - 2 * channel.Dimension;
								destination.BaseStream.Position = source.BaseStream.Position;
							}
							break;
						// color for 4.x.x version
						//case ChannelFormat.Color:
						case ChannelFormat.Byte:
							if (IsReadStream(version))
							{
								for (int i = 0; i < VertexCount; i++)
								{
									destination.Write(source.ReadUInt32());
									source.BaseStream.Position += streamStride - 4 * channel.Dimension;
									destination.BaseStream.Position = source.BaseStream.Position;
								}
							}
							else
							{
								for (int i = 0; i < VertexCount; i++)
								{
									for (int j = 0; j < channel.Dimension; j++)
									{
										destination.Write(source.ReadByte());
									}
									source.BaseStream.Position += streamStride - 1 * channel.Dimension;
									destination.BaseStream.Position = source.BaseStream.Position;
								}
							}
							break;
						case ChannelFormat.ByteV4:
							for (int i = 0; i < VertexCount; i++)
							{
								for (int j = 0; j < channel.Dimension; j++)
								{
									destination.Write(source.ReadByte());
								}
								source.BaseStream.Position += streamStride - 1 * channel.Dimension;
								destination.BaseStream.Position = source.BaseStream.Position;
							}
							break;
						case ChannelFormat.Int:
							for (int i = 0; i < VertexCount; i++)
							{
								for (int j = 0; j < channel.Dimension; j++)
								{
									destination.Write(source.ReadUInt32());
								}
								source.BaseStream.Position += streamStride - 4 * channel.Dimension;
								destination.BaseStream.Position = source.BaseStream.Position;
							}
							break;

						default:
							throw new NotSupportedException(channel.Format.ToString());
					}
				}
			}
		}

		private int GetStreamOffset(int stream, IReadOnlyList<ChannelInfo> channels)
		{
			if (stream == 0)
			{
				return 0;
			}

			// There is a gape between streams (usually 8 bytes )
			// This is NOT an alignment, even if sometimes it may seem so
			int leftSize = channels.Where(t => t.Stream != 0).Sum(t => t.GetStride());
			int offset = m_data.Length - leftSize * VertexCount;

			for(int i = 1; i < stream; i++)
			{
				int streamStride = channels.Where(t => t.Stream == i).Sum(t => t.GetStride());
				offset += streamStride * VertexCount;
			}
			return offset;
		}

		public BitArray CurrentChannelsBits => CreateChannelsBits(CurrentChannels);

		public uint CurrentChannels { get; private set; }
		public int VertexCount { get; private set; }
		public IReadOnlyList<ChannelInfo> Channels => m_channels;
		public IReadOnlyList<StreamInfo> Streams => m_streams;
		public IReadOnlyList<byte> Data => m_data;

		public const string CurrentChannelsName = "m_CurrentChannels";
		public const string VertexCountName = "m_VertexCount";
		public const string ChannelsName = "m_Channels";
		public const string DataSizeName = "m_DataSize";
		public const string TypelessdataName = "_typelessdata";

		private ChannelInfo[] m_channels;
		private StreamInfo[] m_streams;
		private byte[] m_data;
	}
}
