using AquaModelLibrary.Data.BluePoint.CMSH;
using AquaModelLibrary.Data.DataTypes;
using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Half = AquaModelLibrary.Data.DataTypes.Half;

namespace AquaModelLibrary.Data.BluePoint.CMDL.CMDLBasic
{
    /// <summary>
    /// CMDL variation found in Metal Gear Solid HD, Metal Gear Solid Master Collection, and Shadow of the Colossus + Ico PS3.
    /// </summary>
    public class CMDLBasic
    {
        public int magic;
        public int version;
        public CMSHVertexData cmeshVertData = new CMSHVertexData();
        public List<Vector3Int.Vec3Int> faces = new List<Vector3Int.Vec3Int>();

        public CMDLBasic() { }

        public CMDLBasic(BufferedStreamReaderBE<MemoryStream> sr)
        {
            //All of these, even the triangle offset, are big endian 
            magic = sr.Read<int>();
            version = sr.ReadBE<int>(true);
            var triOffset = sr.ReadBE<int>(true);
            var masterCollection = sr._BEReadActive = sr.Peek<uint>() > sr.ReadBE<uint>(true);

            //From here, we read based on endianness. 
            //All little endian versions of these are 64 bit while all big endian versions are conveniently 32bit.
            sr.Seek(-4, SeekOrigin.Current);
            var vertDefCount = sr.ReadBE<int>();

            for (int i = 0; i < vertDefCount; i++)
            {
                var vertDef = new CMSHVertexDataDefinition();
                vertDef.dataMagic = sr.ReadBE<VertexMagic>();
                vertDef.dataFormat = sr.ReadBE<ushort>();
                vertDef.usht_06 = sr.ReadBE<ushort>();
                if (sr._BEReadActive == true)
                {
                    vertDef.dataStart = sr.ReadBE<int>();
                    vertDef.dataSize = sr.ReadBE<int>();
                }
                else
                {
                    vertDef.dataStart = (int)sr.ReadBE<long>();
                    vertDef.dataSize = (int)sr.ReadBE<long>();
                    sr.ReadBE<long>();
                }
                cmeshVertData.vertDefs.Add(vertDef);
            }

            var vertexCount = cmeshVertData.vertDefs[0].dataSize / 0x10;
            foreach (var vertDef in cmeshVertData.vertDefs)
            {
                sr.Seek(vertDef.dataStart + 0xC, SeekOrigin.Begin);
                switch (vertDef.dataMagic)
                {
                    case VertexMagic.POS0:
                        for (int i = 0; i < vertexCount; i++)
                        {
                            cmeshVertData.positionList.Add(sr.ReadBEV3()); sr.ReadBE<float>();
                        }
                        break;
                    case VertexMagic.NRM0:
                    case VertexMagic.TAN0:
                    case VertexMagic.QUT0:
                        for (int i = 0; i < vertexCount; i++)
                        {
                            sr.Read<int>();
                        }
                        break;
                    case VertexMagic.COL0:
                    case VertexMagic.COL1:
                    case VertexMagic.COL2:
                        var colors = new List<byte[]>();
                        for (int v = 0; v < vertexCount; v++)
                        {
                            var colorTemp = new Vector4(sr.ReadBE<Half>(), sr.ReadBE<Half>(), sr.ReadBE<Half>(), sr.ReadBE<Half>());
                            colors.Add(new byte[] { (byte)(colorTemp.X * 255), (byte)(colorTemp.Y * 255), (byte)(colorTemp.Z * 255), (byte)(colorTemp.W * 255), });
                        }
                        cmeshVertData.colorDict.Add(vertDef.dataMagic, colors);
                        break;
                    case VertexMagic.TEX0:
                    case VertexMagic.TEX1:
                    case VertexMagic.TEX2:
                    case VertexMagic.TEX3:
                    case VertexMagic.TEX4:
                    case VertexMagic.TEX5:
                    case VertexMagic.TEX6:
                    case VertexMagic.TEX7:
                    case VertexMagic.TEX8:
                        var uvList = new List<Vector2>();
                        for (int v = 0; v < vertexCount; v++)
                        {
                            uvList.Add(new Vector2(sr.ReadBE<Half>(), sr.ReadBE<Half>()));
                        }
                        cmeshVertData.uvDict.Add(vertDef.dataMagic, uvList);
                        break;
                    case VertexMagic.BONI:
                        var smolCount = vertDef.dataSize / 0x4;
                        if (vertexCount != smolCount)
                        {
                            for (int v = 0; v < vertexCount; v++)
                            {
                                cmeshVertData.vertWeightIndices.Add(new int[] { sr.ReadBE<ushort>(), sr.ReadBE<ushort>(), sr.ReadBE<ushort>(), sr.ReadBE<ushort>() });
                            }
                        }
                        else
                        {
                            for (int v = 0; v < vertexCount; v++)
                            {
                                var indices = sr.ReadBytes(sr.Position, 4);
                                cmeshVertData.vertWeightIndices.Add(new int[] { indices[0], indices[1], indices[2], indices[3] });
                                sr.Seek(4, SeekOrigin.Current);
                            }
                        }
                        break;
                    case VertexMagic.BONW:
                        for (int v = 0; v < vertexCount; v++)
                        {
                            cmeshVertData.vertWeights.Add(new Vector4(sr.ReadBE<Half>(), sr.ReadBE<Half>(), sr.ReadBE<Half>(), sr.ReadBE<Half>()));
                        }
                        break;
                    case VertexMagic.SAT_:
                        cmeshVertData.unkDict.Add(vertDef.dataMagic, sr.ReadBytes(sr.Position, vertDef.dataSize));
                        break;
                    default:
                        Debug.WriteLine($"Unknown data type {vertDef.dataMagic.ToString("X")} {vertDef.dataMagic} {Encoding.UTF8.GetString(BitConverter.GetBytes((int)vertDef.dataMagic))}");
                        cmeshVertData.unkDict.Add(vertDef.dataMagic, sr.ReadBytes(sr.Position, vertDef.dataSize));
                        break;
                }
            }

            //From here on, both types are big endian, because reasons!
            sr._BEReadActive = true;
            
            //Read Faces
            sr.Seek(triOffset + 0xC, SeekOrigin.Begin);
            var faceIndexCount = sr.ReadBE<int>();
            for (int i = 0; i < faceIndexCount - 2; i+=3)
            {
                faces.Add(new Vector3Int.Vec3Int(sr.ReadBE<int>(), sr.ReadBE<int>(), sr.ReadBE<int>()));
            }
            var unk0 = sr.ReadBE<int>();
            if(unk0 != 0)
            {
                throw new NotImplementedException("Unexpected value for unknown following faces!");
            }

            //Read mesh separation data
            var meshCount = sr.ReadBE<int>();
            for(int i = 0; i < meshCount; i++)
            {
                CMDLBasicMesh mesh = new CMDLBasicMesh();
                mesh.minBounding = sr.ReadBEV3();
                mesh.maxBounding = sr.ReadBEV3();
                mesh.usht0 = sr.ReadBE<ushort>();
                mesh.bt0 = sr.ReadBE<byte>();
                mesh.startingVertIndex = sr.ReadBE<int>();
                mesh.vertexCount = sr.ReadBE<int>();
                mesh.startingFaceIndex = sr.ReadBE<int>();
                mesh.faceIndexCount = sr.ReadBE<int>();
                if(masterCollection)
                {
                    mesh.masterCollectionUnk0 = sr.ReadBE<int>();
                }
                mesh.unk0 = sr.ReadBE<int>();
                mesh.unk80s0 = sr.ReadBE<int>();
                mesh.unk80s1 = sr.ReadBE<int>();

                if(version == 0x1)
                {
                    mesh.unk1 = sr.ReadBE<int>();
                    mesh.unk2 = sr.ReadBE<int>();
                    mesh.unk3 = sr.ReadBE<int>();
                }
            }
        }


    }
}
