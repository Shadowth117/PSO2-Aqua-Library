using AquaModelLibrary.Data.DataTypes;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Extensions;
using System.Diagnostics;

namespace AquaModelLibrary.Data.Ninja.Model.XJ
{
	
	public class XJMesh
	{
		public AlphaInstruction SourceAlpha;
		public AlphaInstruction DestinationAlpha;
		public int mat2_08; //These might just be padding since all mat types seem to be 0xC
		public int TextureId;
		public int mat3_04;
		public int mat3_08;
		public int mat4_00;
		public int mat4_04;
		public int mat4_08;
		public byte[] DiffuseColor;
		public int mat5_04;
		public int mat5_08;
		public int mat6_00;
		public int mat6_04;
		public int mat6_08;

		public byte[] stripIndices;

		public XJMesh() { }

		public XJMesh(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
			uint materialOffset = sr.ReadBE<uint>();
			uint materialStructCount = sr.ReadBE<uint>();
            uint indexListOffset = sr.ReadBE<uint>();
            uint indexCount = sr.ReadBE<uint>();
            uint int_0x10 = sr.ReadBE<uint>();

            ReadMaterial(sr, materialOffset, materialStructCount, be, offset);
            stripIndices = sr.ReadBytes(indexListOffset + offset, (int)indexCount * 2);
        }

		public static void Write(List<byte> outBytes, List<int> POF0Offsets, List<XJMesh> xjMeshes, int offset = 0)
		{
			List<int> matOffsetList = new List<int>();
			List<int> stripOffsetList = new List<int>();

			for (int i = 0; i < xjMeshes.Count; i++)
			{
                POF0Offsets.Add((outBytes.Count + offset));
                POF0Offsets.Add((outBytes.Count + offset + 0x8));
				outBytes.ReserveInt($"xjMesh{i}MatOffset");
                outBytes.AddValue((int)0);
                outBytes.ReserveInt($"xjMesh{i}StripOffset");
                outBytes.AddValue((int)3);
                outBytes.AddValue((int)0);
                outBytes.AddValue((int)xjMeshes[i].stripIndices.Length / 2);
                outBytes.AddValue((int)0);
			}
            outBytes.AlignWriter(0x20);

			for(int i = 0; i < xjMeshes.Count; i++)
			{
				var mesh = xjMeshes[i];
				//Material
				outBytes.FillInt($"xjMesh{i}MatOffset", outBytes.Count + offset);

                //Blend types
                outBytes.AddValue((int)2);
                outBytes.AddValue((uint)xjMeshes[i].SourceAlpha);
                outBytes.AddValue((uint)xjMeshes[i].DestinationAlpha);
                outBytes.AddValue((int)0);
                //Texture ID
                outBytes.AddValue(3);
                outBytes.AddValue((uint)xjMeshes[i].TextureId);
                outBytes.AddValue(0);
                outBytes.AddValue(0);
                //Diffuse color
                outBytes.AddValue(5);
                outBytes.AddRange(xjMeshes[i].DiffuseColor);
                outBytes.AddValue((int)0);
                outBytes.AddValue((int)0);
                outBytes.AlignWriter(0x10);

                //Strips
                outBytes.SetByteListInt(stripOffsetList[i], (int)(outBytes.Count + offset));
                outBytes.AddRange(mesh.stripIndices);
                outBytes.AlignWriter(0x10);
            }
		}

		//Nobody ever worked out the real way to read these types of strips so these will be returned double sided
		public List<Vector3Int.Vec3Int> GetTriangles()
		{
			List<Vector3Int.Vec3Int> tris = new List<Vector3Int.Vec3Int>();
			for(int i = 0; i < stripIndices.Length - 4; i += 2)
			{
				var a = BitConverter.ToUInt16(stripIndices, i);
				var b = BitConverter.ToUInt16(stripIndices, i + 2);
				var c = BitConverter.ToUInt16(stripIndices, i + 4);

				if(a == b || b == c || c == a)
				{
					continue;
				}

				tris.Add(new Vector3Int.Vec3Int(a, b, c));
				tris.Add(new Vector3Int.Vec3Int(a, c, b));
			}

			return tris;
		}

		public void ReadMaterial(BufferedStreamReaderBE<MemoryStream> sr, uint materialOffset, uint materialStructCount, bool be = true, int offset = 0)
        {
			sr.Seek(materialOffset + offset, SeekOrigin.Begin);
			DiffuseColor = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

            for (int i = 0; i < materialStructCount; i++)
			{
				uint type = sr.ReadBE<uint>();
				switch(type)
				{
					case 2:
						SourceAlpha = (AlphaInstruction)sr.ReadBE<uint>();
                        DestinationAlpha = (AlphaInstruction)sr.ReadBE<uint>();
                        mat2_08 = sr.ReadBE<int>();
                        break;
					case 3:
						TextureId = sr.ReadBE<int>();
                        mat3_04 = sr.ReadBE<int>();
                        mat3_08 = sr.ReadBE<int>();
                        break;
					case 4:
						mat4_00 = sr.ReadBE<int>();
                        mat4_04 = sr.ReadBE<int>();
						mat4_08 = sr.ReadBE<int>();
						break;
					case 5:
						DiffuseColor = sr.Read4Bytes();
						mat5_04 = sr.ReadBE<int>();
                        mat5_08 = sr.ReadBE<int>();
						break;
					case 6:
						mat6_00 = sr.ReadBE<int>();
                        mat6_04 = sr.ReadBE<int>();
						mat6_08 = sr.ReadBE<int>();
						break;
					default:
                        var mat0_0 = sr.ReadBE<int>();
                        var mat0_1 = sr.ReadBE<int>();
						var mat0_2 = sr.ReadBE<int>();
						Debug.WriteLine($"Unexpected xj material type {type} at {(sr.Position - 0x10).ToString("X")}");
						break;
				}
			}
		}
	}
}
