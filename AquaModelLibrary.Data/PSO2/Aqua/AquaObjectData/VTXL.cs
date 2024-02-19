using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.MathHelpers;
using System.Diagnostics;
using System.Numerics;
using Half = AquaModelLibrary.Data.DataTypes.Half;
using static AquaModelLibrary.Helpers.MiscHelpers;
using AquaModelLibrary.Helpers.PSO2;
using AquaModelLibrary.Helpers;
using static AquaModelLibrary.Helpers.MathHelpers.VectorExtensions;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    //Vertex List. Used similarly in 0xC33, but handled different with different data types being put to use for various types.
    public class VTXL
    {
        #region VertexData
        public List<Vector3> vertPositions = new List<Vector3>();
        public List<ushort[]> vertWeightsNGS = new List<ushort[]>(); //4 ushorts. Total should be FFFF between the 4. FFFF 0000 0000 0000 would be a full rigid weight.
        public List<short[]> vertNormalsNGS = new List<short[]>(); //4 16 bit values, value 4 being 0 always.
        public List<byte[]> vertColors = new List<byte[]>(); //4 bytes, BGRA
        public List<byte[]> vertColor2s = new List<byte[]>(); //4 bytes, BGRA?
        public List<int[]> vertWeightIndices = new List<int[]>(); //4 bytes when written. Prior to processing, store as ints
        public List<short[]> uv1ListNGS = new List<short[]>(); //2 16 bit values. UVs probably need to be vertically flipped for most software. I usually just import v as -v.
        public List<short[]> uv2ListNGS = new List<short[]>(); //Uncommon NGS uv2 variation
        public List<short[]> uv3ListNGS = new List<short[]>();
        public List<short[]> uv4ListNGS = new List<short[]>();
        public List<Vector2> uv2List = new List<Vector2>(); //For some reason 0xC33 seemingly retained vector2 data types for these, though only in some cases.
        public List<Vector2> uv3List = new List<Vector2>();
        public List<Vector2> uv4List = new List<Vector2>();
        public List<short[]> vert0x22 = new List<short[]>(); //This and the following type are 2 shorts seemingly that do... something. Only observed in 0xC33 player models at this time. 
        public List<short[]> vert0x23 = new List<short[]>(); //Possibly UV channels 5 and 6? Typically values are the same for every vertex in a mesh though.
        public List<short[]> vert0x24 = new List<short[]>();
        public List<short[]> vert0x25 = new List<short[]>();

        //Binormals and tangents for each face are calculated and each face's values for a particular vertex are summed and averaged for the result before being normalized
        //Though vertex position is used, due to the nature of the normalization applied during the process, resizing is unneeded.
        //For NGS, these are 4 shorts with value 4 always being 0.
        public List<short[]> vertTangentListNGS = new List<short[]>();
        public List<short[]> vertBinormalListNGS = new List<short[]>();

        //Old style lists
        public List<Vector4> vertWeights = new List<Vector4>();
        public List<Vector3> vertNormals = new List<Vector3>();
        public List<Vector2> uv1List = new List<Vector2>();
        public List<Vector3> vertTangentList = new List<Vector3>();
        public List<Vector3> vertBinormalList = new List<Vector3>();

        public List<ushort> bonePalette = new List<ushort>(); //Indices of particular bones are used for weight indices above
        public List<ushort> edgeVerts = new List<ushort>(); //No idea if this is used, but I fill it anyways

        //Helper variables
        //For raw data from a 3d editor
        public List<List<float>> rawVertWeights = new List<List<float>>();
        public List<List<int>> rawVertWeightIds = new List<List<int>>();

        //These are for help with splitting vertex data from face data. PSO2 does not allow storing data in faces so this is necessary to avoid problems.
        public List<int> rawVertId = new List<int>();
        public List<int> rawFaceId = new List<int>();

        //Holds processed weight info for accessing in external applications
        public List<Vector4> trueVertWeights = new List<Vector4>();
        public List<int[]> trueVertWeightIndices = new List<int[]>();
        #endregion

        #region Constructors
        public VTXL() { }

        public VTXL(List<Dictionary<int, object>> vtxlRaw, VTXE vtxe)
        {
            int vertCount = ((byte[])vtxlRaw[0][0xBA]).Length / vtxe.GetVTXESize();

            using (MemoryStream stream = new MemoryStream((byte[])vtxlRaw[0][0xBA]))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(streamReader, vtxe, vertCount, vtxe.vertDataTypes.Count);
            }
        }

        public VTXL(int vertCount, VTXL modelVtxl)
        {
            vertPositions = new List<Vector3>(new Vector3[vertCount]); //Any vert should honestly have this if it's a proper vertex.
            if (modelVtxl.vertNormals.Count > 0)
            {
                vertNormals = new List<Vector3>(new Vector3[vertCount]);
            }
            if (modelVtxl.vertNormalsNGS.Count > 0)
            {
                vertNormalsNGS = new List<short[]>(new short[vertCount][]);
            }
            if (modelVtxl.vertBinormalList.Count > 0)
            {
                vertBinormalList = new List<Vector3>(new Vector3[vertCount]);
            }
            if (modelVtxl.vertBinormalListNGS.Count > 0)
            {
                vertBinormalListNGS = new List<short[]>(new short[vertCount][]);
            }
            if (modelVtxl.vertTangentList.Count > 0)
            {
                vertTangentList = new List<Vector3>(new Vector3[vertCount]);
            }
            if (modelVtxl.vertTangentListNGS.Count > 0)
            {
                vertTangentListNGS = new List<short[]>(new short[vertCount][]);
            }
            if (modelVtxl.vertColors.Count > 0)
            {
                vertColors = new List<byte[]>(new byte[vertCount][]);
            }
            if (modelVtxl.vertColor2s.Count > 0)
            {
                vertColor2s = new List<byte[]>(new byte[vertCount][]);
            }
            if (modelVtxl.uv1List.Count > 0)
            {
                uv1List = new List<Vector2>(new Vector2[vertCount]);
            }
            if (modelVtxl.uv1ListNGS.Count > 0)
            {
                uv1ListNGS = new List<short[]>(new short[vertCount][]);
            }
            if (modelVtxl.uv2ListNGS.Count > 0)
            {
                uv2ListNGS = new List<short[]>(new short[vertCount][]);
            }
            if (modelVtxl.uv3ListNGS.Count > 0)
            {
                uv3ListNGS = new List<short[]>(new short[vertCount][]);
            }
            if (modelVtxl.uv4ListNGS.Count > 0)
            {
                uv4ListNGS = new List<short[]>(new short[vertCount][]);
            }
            if (modelVtxl.uv2List.Count > 0)
            {
                uv2List = new List<Vector2>(new Vector2[vertCount]);
            }
            if (modelVtxl.uv3List.Count > 0)
            {
                uv3List = new List<Vector2>(new Vector2[vertCount]);
            }
            if (modelVtxl.uv4List.Count > 0)
            {
                uv4List = new List<Vector2>(new Vector2[vertCount]);
            }
            if (modelVtxl.vert0x22.Count > 0)
            {
                vert0x22 = new List<short[]>(new short[vertCount][]);
            }
            if (modelVtxl.vert0x23.Count > 0)
            {
                vert0x23 = new List<short[]>(new short[vertCount][]);
            }
            if (modelVtxl.vert0x24.Count > 0)
            {
                vert0x24 = new List<short[]>(new short[vertCount][]);
            }
            if (modelVtxl.vert0x25.Count > 0)
            {
                vert0x25 = new List<short[]>(new short[vertCount][]);
            }
            //These can... potentially be mutually exclusive, but the use cases for that are kind of limited and I don't and am not interested in handling them.
            if (modelVtxl.rawVertWeights.Count > 0)
            {
                for (int i = 0; i < vertCount; i++)
                {
                    rawVertWeights.Add(new List<float>());
                    rawVertWeightIds.Add(new List<int>());
                }
            }
            else if (modelVtxl.vertWeights.Count > 0)
            {
                vertWeights = new List<Vector4>(new Vector4[vertCount]);
                vertWeightIndices = new List<int[]>(new int[vertCount][]);
                for (int i = 0; i < vertCount; i++)
                {
                    vertWeightIndices[i] = new int[4];
                }
            }
            if (modelVtxl.vertWeightsNGS.Count > 0)
            {
                vertWeightsNGS = new List<ushort[]>(new ushort[vertCount][]);
                for (int i = 0; i < vertCount; i++)
                {
                    vertWeightsNGS[i] = new ushort[4];
                }
            }

        }

        public VTXL(BufferedStreamReaderBE<MemoryStream> streamReader, VTXE vtxeSet, int vertCount, int sizeToFill = -1)
        {
            Read(streamReader, vtxeSet, vertCount, sizeToFill);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> streamReader, VTXE vtxeSet, int vertCount, int sizeToFill = -1)
        {
            for (int vtxlIndex = 0; vtxlIndex < vertCount; vtxlIndex++)
            {
                long startPosition = streamReader.Position;
                for (int vtxeIndex = 0; vtxeIndex < vtxeSet.vertDataTypes.Count; vtxeIndex++)
                {
                    switch (vtxeSet.vertDataTypes[vtxeIndex].dataType)
                    {
                        case (int)VertFlags.VertPosition:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x3:
                                    vertPositions.Add(streamReader.Read<Vector3>());
                                    break;
                                case 0x13:
                                case 0x99: //Nova
                                    vertPositions.Add(new Vector3(streamReader.Read<Half>(), streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    var fillerHalf = streamReader.Read<ushort>(); //Technically another half float. Should always be 1.
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case (int)VertFlags.VertWeight:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x4:
                                    vertWeights.Add(streamReader.Read<Vector4>());
                                    break;
                                case 0x11:
                                    vertWeightsNGS.Add(streamReader.ReadBE4UShorts());
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert weight struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)VertFlags.VertNormal:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x3:
                                    vertNormals.Add(streamReader.Read<Vector3>());
                                    break;
                                case 0xF:
                                    vertNormalsNGS.Add(streamReader.ReadBE4Shorts());
                                    break;
                                case 0x13:
                                    vertNormals.Add(new Vector3(streamReader.Read<Half>(), streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    var fillerHalf = streamReader.Read<ushort>(); //Technically another half float. Should always be 1.
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert normal struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)VertFlags.VertColor:
                            vertColors.Add(streamReader.Read4Bytes());
                            break;
                        case (int)VertFlags.VertColor2:
                            vertColor2s.Add(streamReader.Read4Bytes());
                            break;
                        case (int)VertFlags.VertWeightIndex:
                            vertWeightIndices.Add(streamReader.Read4BytesToIntArray());
                            break;
                        case (int)VertFlags.VertUV1:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x2:
                                    uv1List.Add(streamReader.Read<Vector2>());
                                    break;
                                case 0xE:
                                    uv1ListNGS.Add(streamReader.ReadBE2Shorts());
                                    break;
                                case 0x12:
                                    uv1List.Add(new Vector2(streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    break;
                                case 0x99: //For nova
                                    uv1List.Add(UshortsToVector2(streamReader.ReadBE2Ushorts()));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert uv1 struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)VertFlags.VertUV2:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x2:
                                    uv2List.Add(streamReader.Read<Vector2>());
                                    break;
                                case 0xE:
                                    uv2ListNGS.Add(streamReader.ReadBE2Shorts());
                                    break;
                                case 0x12:
                                    uv2List.Add(new Vector2(streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    break;
                                case 0x99: //For nova
                                    uv2List.Add(UshortsToVector2(streamReader.ReadBE2Ushorts()));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert uv2 struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)VertFlags.VertUV3:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x2:
                                    uv3List.Add(streamReader.Read<Vector2>());
                                    break;
                                case 0xE:
                                    uv3ListNGS.Add(streamReader.ReadBE2Shorts());
                                    break;
                                case 0x12:
                                    uv3List.Add(new Vector2(streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    break;
                                case 0x99: //For nova
                                    uv3List.Add(UshortsToVector2(streamReader.ReadBE2Ushorts()));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert uv3 struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)VertFlags.VertUV4:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x2:
                                    uv4List.Add(streamReader.Read<Vector2>());
                                    break;
                                case 0xE:
                                    uv4ListNGS.Add(streamReader.ReadBE2Shorts());
                                    break;
                                case 0x12:
                                    uv4List.Add(new Vector2(streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    break;
                                case 0x99: //For nova
                                    uv4List.Add(UshortsToVector2(streamReader.ReadBE2Ushorts()));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert uv4 struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)VertFlags.VertTangent:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x3:
                                    vertTangentList.Add(streamReader.Read<Vector3>());
                                    break;
                                case 0xF:
                                    vertTangentListNGS.Add(streamReader.ReadBE4Shorts());
                                    break;
                                case 0x13:
                                    vertTangentList.Add(new Vector3(streamReader.Read<Half>(), streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    var fillerHalf = streamReader.Read<ushort>(); //Technically another half float. Should always be 1.
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert tangent struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)VertFlags.VertBinormal:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x3:
                                    vertBinormalList.Add(streamReader.Read<Vector3>());
                                    break;
                                case 0xF:
                                    vertBinormalListNGS.Add(streamReader.ReadBE4Shorts());
                                    break;
                                case 0x13:
                                    vertBinormalList.Add(new Vector3(streamReader.Read<Half>(), streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    var fillerHalf = streamReader.Read<ushort>(); //Technically another half float. Should always be 1.
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert binormal struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)VertFlags.Vert0x22:
                            vert0x22.Add(streamReader.ReadBE2Shorts());
                            break;
                        case (int)VertFlags.Vert0x23:
                            vert0x23.Add(streamReader.ReadBE2Shorts());
                            break;
                        case (int)VertFlags.Vert0x24:
                            vert0x24.Add(streamReader.ReadBE2Shorts());
                            break;
                        case (int)VertFlags.Vert0x25:
                            vert0x25.Add(streamReader.ReadBE2Shorts());
                            break;
                        default:
                            Debug.WriteLine($"Unknown Vert type {vtxeSet.vertDataTypes[vtxeIndex].dataType.ToString("X")}! Please report!");
                            break;
                    }
                }

                //Ensure for 0xC33 variants that we seek to the end of each entry
                if (sizeToFill > 0)
                {
                    streamReader.Seek(startPosition + sizeToFill, System.IO.SeekOrigin.Begin);
                }
            }

            //Process 0xC33 variants for later use
            if (sizeToFill > 0)
            {
                convertToLegacyTypes();
            }
            createTrueVertWeights();
        }
        #endregion

        #region Writing
        public byte[] GetBytesVTBF(VTXE vtxe)
        {
            List<byte> outBytes2 = new List<byte>
            {
                0xBA,
                0x89
            };
            int vtxlSizeArea = outBytes2.Count;
            GetBytes(vtxe, outBytes2);

            //Calc and insert the vert data counts in post due to the way sega does it.
            int vertDataCount = ((outBytes2.Count - vtxlSizeArea) / 4) - 1;
            if (vertDataCount > byte.MaxValue)
            {
                if (vertDataCount - 1 > ushort.MaxValue)
                {
                    outBytes2.Insert(vtxlSizeArea, 0x18);
                    outBytes2.InsertRange(vtxlSizeArea + 0x1, BitConverter.GetBytes(vertDataCount));
                }
                else
                {
                    outBytes2.Insert(vtxlSizeArea, 0x10);
                    outBytes2.InsertRange(vtxlSizeArea + 0x1, BitConverter.GetBytes((short)(vertDataCount)));
                }
            }
            else
            {
                outBytes2.Insert(vtxlSizeArea, 0x8);
                outBytes2.Insert(vtxlSizeArea + 0x1, (byte)vertDataCount);
            }

            //Pointer count. Always 0 on VTXL
            //Subtag count
            VTBFMethods.WriteTagHeader(outBytes2, "VTXL", 0, 0x1);

            return outBytes2.ToArray();
        }

        public byte[] GetBytes(VTXE vtxe, List<byte> outBytes, int sizeToFill = -1)
        {
            outBytes.AddRange(GetBytes(vtxe, sizeToFill));

            return outBytes.ToArray();
        }

        public byte[] GetBytes(VTXE vtxe, int sizeToFill)
        {
            //For NGS models, we want to fill out each vertex to the size of the largest vertex in the model
            int padding;
            if (sizeToFill != -1)
            {
                padding = sizeToFill - vtxe.GetVTXESize();
            }
            else
            {
                padding = 0;
            }

            List<byte> outBytes = new List<byte>();
            for (int i = 0; i < vertPositions.Count; i++)
            {
                for (int j = 0; j < vtxe.vertDataTypes.Count; j++)
                {
                    switch (vtxe.vertDataTypes[j].dataType)
                    {
                        case (int)VertFlags.VertPosition:
                            outBytes.AddRange(DataHelpers.ConvertStruct(vertPositions[i]));
                            break;
                        case (int)VertFlags.VertWeight:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x4:
                                    outBytes.AddRange(DataHelpers.ConvertStruct(vertWeights[i]));
                                    break;
                                case 0x11:
                                    for (int id = 0; id < 4; id++)
                                    {
                                        outBytes.AddRange(BitConverter.GetBytes(vertWeightsNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)VertFlags.VertNormal:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x3:
                                    outBytes.AddRange(DataHelpers.ConvertStruct(vertNormals[i]));
                                    break;
                                case 0xF:
                                    for (int id = 0; id < 4; id++)
                                    {
                                        outBytes.AddRange(BitConverter.GetBytes(vertNormalsNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)VertFlags.VertColor:
                            for (int color = 0; color < 4; color++)
                            {
                                outBytes.Add(vertColors[i][color]);
                            }
                            break;
                        case (int)VertFlags.VertColor2:
                            for (int color = 0; color < 4; color++)
                            {
                                outBytes.Add(vertColor2s[i][color]);
                            }
                            break;
                        case (int)VertFlags.VertWeightIndex:
                            for (int weight = 0; weight < 4; weight++)
                            {
                                outBytes.Add((byte)vertWeightIndices[i][weight]);
                            }
                            break;
                        case (int)VertFlags.VertUV1:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x2:
                                    outBytes.AddRange(DataHelpers.ConvertStruct(uv1List[i]));
                                    break;
                                case 0xE:
                                    for (int id = 0; id < 2; id++)
                                    {
                                        outBytes.AddRange(BitConverter.GetBytes(uv1ListNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)VertFlags.VertUV2:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x2:
                                    outBytes.AddRange(DataHelpers.ConvertStruct(uv2List[i]));
                                    break;
                                case 0xE:
                                    for (int id = 0; id < 2; id++)
                                    {
                                        outBytes.AddRange(BitConverter.GetBytes(uv2ListNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)VertFlags.VertUV3:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x2:
                                    outBytes.AddRange(DataHelpers.ConvertStruct(uv3List[i]));
                                    break;
                                case 0xE:
                                    for (int id = 0; id < 2; id++)
                                    {
                                        outBytes.AddRange(BitConverter.GetBytes(uv3ListNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)VertFlags.VertUV4:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x2:
                                    outBytes.AddRange(DataHelpers.ConvertStruct(uv4List[i]));
                                    break;
                                case 0xE:
                                    for (int id = 0; id < 2; id++)
                                    {
                                        outBytes.AddRange(BitConverter.GetBytes(uv4ListNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)VertFlags.VertTangent:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x3:
                                    outBytes.AddRange(DataHelpers.ConvertStruct(vertTangentList[i]));
                                    break;
                                case 0xF:
                                    for (int id = 0; id < 4; id++)
                                    {
                                        outBytes.AddRange(BitConverter.GetBytes(vertTangentListNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)VertFlags.VertBinormal:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x3:
                                    outBytes.AddRange(DataHelpers.ConvertStruct(vertBinormalList[i]));
                                    break;
                                case 0xF:
                                    for (int id = 0; id < 4; id++)
                                    {
                                        outBytes.AddRange(BitConverter.GetBytes(vertBinormalListNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)VertFlags.Vert0x22:
                            for (int id = 0; id < 2; id++)
                            {
                                outBytes.AddRange(BitConverter.GetBytes(vert0x22[i][id]));
                            }
                            break;
                        case (int)VertFlags.Vert0x23:
                            for (int id = 0; id < 2; id++)
                            {
                                outBytes.AddRange(BitConverter.GetBytes(vert0x23[i][id]));
                            }
                            break;
                        case (int)VertFlags.Vert0x24:
                            for (int id = 0; id < 2; id++)
                            {
                                outBytes.AddRange(BitConverter.GetBytes(vert0x24[i][id]));
                            }
                            break;
                        case (int)VertFlags.Vert0x25:
                            for (int id = 0; id < 2; id++)
                            {
                                outBytes.AddRange(BitConverter.GetBytes(vert0x25[i][id]));
                            }
                            break;
                        default:
                            Debug.WriteLine($"Unknown Vert type {vtxe.vertDataTypes[j].dataType}! Please report!");
                            throw new Exception("Not implemented!");
                    }
                }
                outBytes.AddRange(new byte[padding]);
            }

            return outBytes.ToArray();
        }
        #endregion

        #region Classic<->NGS Conversion
        public void convertFromLegacyTypes()
        {
            //Weights
            vertWeightsNGS.Clear();
            for (int i = 0; i < vertWeights.Count; i++)
            {
                vertWeightsNGS.Add(new ushort[4]);
                vertWeightsNGS[i][0] = (ushort)(vertWeights[i].X * ushort.MaxValue);
                vertWeightsNGS[i][1] = (ushort)(vertWeights[i].Y * ushort.MaxValue);
                vertWeightsNGS[i][2] = (ushort)(vertWeights[i].Z * ushort.MaxValue);
                vertWeightsNGS[i][3] = (ushort)(vertWeights[i].W * ushort.MaxValue);
            }

            //Normals
            vertNormalsNGS.Clear();
            for (int i = 0; i < vertNormals.Count; i++)
            {
                vertNormalsNGS.Add(new short[4]);
                vertNormalsNGS[i][0] = (short)(vertNormals[i].X * short.MaxValue); if (vertNormalsNGS[i][0] == -short.MaxValue) { vertNormalsNGS[i][0]--; }
                vertNormalsNGS[i][1] = (short)(vertNormals[i].Y * short.MaxValue); if (vertNormalsNGS[i][1] == -short.MaxValue) { vertNormalsNGS[i][1]--; }
                vertNormalsNGS[i][2] = (short)(vertNormals[i].Z * short.MaxValue); if (vertNormalsNGS[i][2] == -short.MaxValue) { vertNormalsNGS[i][2]--; }
                vertNormalsNGS[i][3] = 0;
            }

            //UV1List
            uv1ListNGS.Clear();
            for (int i = 0; i < uv1List.Count; i++)
            {
                uv1ListNGS.Add(new short[2]);
                uv1ListNGS[i][0] = (short)(uv1List[i].X * short.MaxValue); if (uv1ListNGS[i][0] == -short.MaxValue) { uv1ListNGS[i][0]--; }
                uv1ListNGS[i][1] = (short)(uv1List[i].Y * short.MaxValue); if (uv1ListNGS[i][1] == -short.MaxValue) { uv1ListNGS[i][1]--; }
            }

            uv2ListNGS.Clear();
            for (int i = 0; i < uv2List.Count; i++)
            {
                uv2ListNGS.Add(new short[2]);
                uv2ListNGS[i][0] = (short)(uv2List[i].X * short.MaxValue); if (uv2ListNGS[i][0] == -short.MaxValue) { uv2ListNGS[i][0]--; }
                uv2ListNGS[i][1] = (short)(uv2List[i].Y * short.MaxValue); if (uv2ListNGS[i][1] == -short.MaxValue) { uv2ListNGS[i][1]--; }
            }

            uv3ListNGS.Clear();
            for (int i = 0; i < uv3List.Count; i++)
            {
                uv3ListNGS.Add(new short[2]);
                uv3ListNGS[i][0] = (short)(uv3List[i].X * short.MaxValue); if (uv3ListNGS[i][0] == -short.MaxValue) { uv3ListNGS[i][0]--; }
                uv3ListNGS[i][1] = (short)(uv3List[i].Y * short.MaxValue); if (uv3ListNGS[i][1] == -short.MaxValue) { uv3ListNGS[i][1]--; }
            }

            uv4ListNGS.Clear();
            for (int i = 0; i < uv4List.Count; i++)
            {
                uv4ListNGS.Add(new short[2]);
                uv4ListNGS[i][0] = (short)(uv4List[i].X * short.MaxValue); if (uv4ListNGS[i][0] == -short.MaxValue) { uv4ListNGS[i][0]--; }
                uv4ListNGS[i][1] = (short)(uv4List[i].Y * short.MaxValue); if (uv4ListNGS[i][1] == -short.MaxValue) { uv4ListNGS[i][1]--; }
            }

            //Tangents
            vertTangentListNGS.Clear();
            for (int i = 0; i < vertTangentList.Count; i++)
            {
                vertTangentListNGS.Add(new short[4]);
                vertTangentListNGS[i][0] = (short)(vertTangentList[i].X * short.MaxValue); if (vertTangentListNGS[i][0] == -short.MaxValue) { vertTangentListNGS[i][0]--; }
                vertTangentListNGS[i][1] = (short)(vertTangentList[i].Y * short.MaxValue); if (vertTangentListNGS[i][1] == -short.MaxValue) { vertTangentListNGS[i][1]--; }
                vertTangentListNGS[i][2] = (short)(vertTangentList[i].Z * short.MaxValue); if (vertTangentListNGS[i][2] == -short.MaxValue) { vertTangentListNGS[i][2]--; }
                vertTangentListNGS[i][3] = 0;
            }

            //Binormals
            vertBinormalListNGS.Clear();
            for (int i = 0; i < vertBinormalList.Count; i++)
            {
                vertBinormalListNGS.Add(new short[4]);
                vertBinormalListNGS[i][0] = (short)(vertBinormalList[i].X * short.MaxValue); if (vertBinormalListNGS[i][0] == -short.MaxValue) { vertBinormalListNGS[i][0]--; }
                vertBinormalListNGS[i][0] = (short)(vertBinormalList[i].Y * short.MaxValue); if (vertBinormalListNGS[i][1] == -short.MaxValue) { vertBinormalListNGS[i][1]--; }
                vertBinormalListNGS[i][0] = (short)(vertBinormalList[i].Z * short.MaxValue); if (vertBinormalListNGS[i][2] == -short.MaxValue) { vertBinormalListNGS[i][2]--; }
                vertBinormalListNGS[i][0] = 0;
            }
        }

        public void convertToLegacyTypes(bool force = false)
        {
            if (force || vertWeights.Count == 0)
            {
                //Weights
                vertWeights.Clear();
                for (int i = 0; i < vertWeightsNGS.Count; i++)
                {
                    var weight = new Vector4();
                    weight.X = (float)vertWeightsNGS[i][0] / ushort.MaxValue;
                    weight.Y = (float)vertWeightsNGS[i][1] / ushort.MaxValue;
                    weight.Z = (float)vertWeightsNGS[i][2] / ushort.MaxValue;
                    weight.W = (float)vertWeightsNGS[i][3] / ushort.MaxValue;
                    weight = Vector4.Normalize(weight);
                    vertWeights.Add(weight);
                }
            }

            if (force || vertNormals.Count == 0)
            {
                //Normals
                vertNormals.Clear();
                for (int i = 0; i < vertNormalsNGS.Count; i++)
                {
                    var normal = new Vector3();
                    normal.X = (float)vertNormalsNGS[i][0] / short.MaxValue;
                    normal.Y = (float)vertNormalsNGS[i][1] / short.MaxValue;
                    normal.Z = (float)vertNormalsNGS[i][2] / short.MaxValue;
                    normal = Vector3.Normalize(normal);
                    vertNormals.Add(normal);
                }
            }

            if (force || uv1List.Count == 0)
            {
                //UV1List
                uv1List.Clear();
                for (int i = 0; i < uv1ListNGS.Count; i++)
                {
                    var uv = new Vector2();
                    uv.X = (float)uv1ListNGS[i][0] / short.MaxValue;
                    uv.Y = (float)uv1ListNGS[i][1] / short.MaxValue;
                    uv1List.Add(uv);
                }
            }

            if (force || uv2List.Count == 0)
            {
                //UV2List
                uv2List.Clear();
                for (int i = 0; i < uv2ListNGS.Count; i++)
                {
                    var uv = new Vector2();
                    uv.X = (float)uv2ListNGS[i][0] / short.MaxValue;
                    uv.Y = (float)uv2ListNGS[i][1] / short.MaxValue;
                    uv2List.Add(uv);
                }
            }

            if (force || uv3List.Count == 0)
            {
                //UV2List
                uv3List.Clear();
                for (int i = 0; i < uv3ListNGS.Count; i++)
                {
                    var uv = new Vector2();
                    uv.X = (float)uv3ListNGS[i][0] / short.MaxValue;
                    uv.Y = (float)uv3ListNGS[i][1] / short.MaxValue;
                    uv2List.Add(uv);
                }
            }

            if (force || uv4List.Count == 0)
            {
                //UV2List
                uv4List.Clear();
                for (int i = 0; i < uv4ListNGS.Count; i++)
                {
                    var uv = new Vector2();
                    uv.X = (float)uv4ListNGS[i][0] / short.MaxValue;
                    uv.Y = (float)uv4ListNGS[i][1] / short.MaxValue;
                    uv2List.Add(uv);
                }
            }

            if (force || vertTangentList.Count == 0)
            {
                //Tangents
                vertTangentList.Clear();
                for (int i = 0; i < vertTangentListNGS.Count; i++)
                {
                    var tangent = new Vector3();
                    tangent.X = (float)vertTangentListNGS[i][0] / short.MaxValue;
                    tangent.Y = (float)vertTangentListNGS[i][1] / short.MaxValue;
                    tangent.Z = (float)vertTangentListNGS[i][2] / short.MaxValue;
                    tangent = Vector3.Normalize(tangent);
                    vertTangentList.Add(tangent);
                }
            }

            //Binormals
            if (force || vertBinormalList.Count == 0)
            {
                vertBinormalList.Clear();
                for (int i = 0; i < vertBinormalListNGS.Count; i++)
                {
                    var binormal = new Vector3();
                    binormal.X = (float)vertBinormalListNGS[i][0] / short.MaxValue;
                    binormal.Y = (float)vertBinormalListNGS[i][1] / short.MaxValue;
                    binormal.Z = (float)vertBinormalListNGS[i][2] / short.MaxValue;
                    binormal = Vector3.Normalize(binormal);
                    vertBinormalList.Add(binormal);
                }
            }
        }
        #endregion

        #region VertWeightManipulation
        //If last weight id is bone 0, move that to slot 0
        public void setLastId0sFirst()
        {
            if (vertWeights.Count > 0 && vertWeightIndices.Count > 0)
            {
                //Account for bone palette 0 being ordered weird
                for (int i = 0; i < vertWeights.Count; i++)
                {
                    if (vertWeightIndices[i][1] == 0 && vertWeights[i].Y != 0 && vertWeightIndices[i][2] == 0)
                    {
                        vertWeights[i] = new Vector4(vertWeights[i].Y, vertWeights[i].X, vertWeights[i].Z, vertWeights[i].W);
                        vertWeightIndices[i] = new int[] { vertWeightIndices[i][1], vertWeightIndices[i][0], vertWeightIndices[i][2], vertWeightIndices[i][3] };
                    }
                    if (vertWeightIndices[i][2] == 0 && vertWeights[i].Z != 0 && vertWeightIndices[i][3] == 0)
                    {
                        vertWeights[i] = new Vector4(vertWeights[i].Z, vertWeights[i].X, vertWeights[i].Y, vertWeights[i].W);
                        vertWeightIndices[i] = new int[] { vertWeightIndices[i][2], vertWeightIndices[i][0], vertWeightIndices[i][1], vertWeightIndices[i][3] };
                    }
                    if (vertWeightIndices[i][3] == 0 && vertWeights[i].W != 0)
                    {
                        vertWeights[i] = new Vector4(vertWeights[i].W, vertWeights[i].X, vertWeights[i].Y, vertWeights[i].Z);
                        vertWeightIndices[i] = new int[] { vertWeightIndices[i][3], vertWeightIndices[i][0], vertWeightIndices[i][1], vertWeightIndices[i][2] };
                    }
                }
            }
        }

        //If id is 0 and not in slot 0, it goes to slot 0
        public void setId0sFirst()
        {
            if (vertWeights.Count > 0 && vertWeightIndices.Count > 0)
            {
                //Account for bone palette 0 being ordered weird
                for (int i = 0; i < vertWeights.Count; i++)
                {
                    if (vertWeightIndices[i][1] == 0 && vertWeights[i].Y != 0)
                    {
                        vertWeights[i] = new Vector4(vertWeights[i].Y, vertWeights[i].X, vertWeights[i].Z, vertWeights[i].W);
                        vertWeightIndices[i] = new int[] { vertWeightIndices[i][1], vertWeightIndices[i][0], vertWeightIndices[i][2], vertWeightIndices[i][3] };
                    }
                    if (vertWeightIndices[i][2] == 0 && vertWeights[i].Z != 0)
                    {
                        vertWeights[i] = new Vector4(vertWeights[i].Z, vertWeights[i].X, vertWeights[i].Y, vertWeights[i].W);
                        vertWeightIndices[i] = new int[] { vertWeightIndices[i][2], vertWeightIndices[i][0], vertWeightIndices[i][1], vertWeightIndices[i][3] };
                    }
                    if (vertWeightIndices[i][3] == 0 && vertWeights[i].W != 0)
                    {
                        vertWeights[i] = new Vector4(vertWeights[i].W, vertWeights[i].X, vertWeights[i].Y, vertWeights[i].Z);
                        vertWeightIndices[i] = new int[] { vertWeightIndices[i][3], vertWeightIndices[i][0], vertWeightIndices[i][1], vertWeightIndices[i][2] };
                    }
                }
            }
        }

        public void AssureSumOfOneOnWeights()
        {
            if (vertWeights.Count > 0 && vertWeightIndices.Count > 0)
            {
                for (int i = 0; i < vertWeights.Count; i++)
                {
                    vertWeights[i] = SumWeightsTo1(vertWeights[i]);
                }
            }
        }

        //PSO2 doesn't differentiate in the file how many weights a particular vert has. 
        //This allows one to condense the weight data
        public void createTrueVertWeights()
        {
            trueVertWeightIndices.Clear();
            trueVertWeights.Clear();
            if (vertWeights.Count > 0 && vertWeightIndices.Count > 0)
            {
                //Account for bone palette 0 being ordered weird
                for (int i = 0; i < vertWeights.Count; i++)
                {
                    Vector4 trueWeight = new Vector4();
                    List<int> trueIds = new List<int>();
                    if (vertWeightIndices[i][0] != 0 || vertWeights[i].X != 0)
                    {
                        trueWeight = Set(trueWeight, trueIds.Count, vertWeights[i].X);
                        trueIds.Add(vertWeightIndices[i][0]);
                    }
                    if (vertWeightIndices[i][1] != 0 || vertWeights[i].Y != 0)
                    {
                        if (trueIds.Contains(vertWeightIndices[i][1]))
                        {
                            trueWeight = AddToId(trueWeight, vertWeights[i].Y, trueIds.IndexOf(vertWeightIndices[i][1]));
                        }
                        else
                        {
                            trueWeight = Set(trueWeight, trueIds.Count, vertWeights[i].Y);
                            trueIds.Add(vertWeightIndices[i][1]);
                        }
                    }
                    if (vertWeightIndices[i][2] != 0 || vertWeights[i].Z != 0)
                    {
                        if (trueIds.Contains(vertWeightIndices[i][2]))
                        {
                            trueWeight = AddToId(trueWeight, vertWeights[i].Z, trueIds.IndexOf(vertWeightIndices[i][2]));
                        }
                        else
                        {
                            trueWeight = Set(trueWeight, trueIds.Count, vertWeights[i].Z);
                            trueIds.Add(vertWeightIndices[i][2]);
                        }
                    }
                    if (vertWeightIndices[i][3] != 0 || vertWeights[i].W != 0)
                    {
                        if (trueIds.Contains(vertWeightIndices[i][3]))
                        {
                            trueWeight = AddToId(trueWeight, vertWeights[i].W, trueIds.IndexOf(vertWeightIndices[i][3]));
                        }
                        else
                        {
                            trueWeight = Set(trueWeight, trueIds.Count, vertWeights[i].W);
                            trueIds.Add(vertWeightIndices[i][3]);
                        }
                    }
                    //Ensure sum is as close as possible to 1.0.
                    trueWeight = SumWeightsTo1(trueWeight);

                    trueVertWeights.Add(trueWeight);
                    trueVertWeightIndices.Add(trueIds.ToArray());
                }
            }

        }

        //Fixes invalid bone assignments and weights them bone 0 instead.
        public void fixWeightsFromBoneCount(int maxBone = int.MaxValue)
        {
            if (vertWeights.Count > 0 && vertWeightIndices.Count > 0)
            {
                for (int i = 0; i < vertWeights.Count; i++)
                {
                    List<int> idList = new List<int>(); //List of ids to combine to bone0Id's value
                    int bone0Id = 0;
                    bool bone0Set = false;
                    for (int id = 0; id < vertWeightIndices[i].Length; id++)
                    {
                        int loopId = vertWeightIndices[i][id];
                        if (loopId == 0)
                        {
                            if (bone0Set == false)
                            {
                                bone0Id = id;
                                bone0Set = true;
                                continue;
                            }
                            else
                            {
                                idList.Add(id);
                            }
                        }

                        if (loopId > maxBone)
                        {
                            vertWeightIndices[i][id] = 0;
                            if (bone0Set == false)
                            {
                                bone0Id = id;

                            }
                            else
                            {
                                idList.Add(id);
                            }
                        }
                    }

                    foreach (var id in idList)
                    {
                        vertWeights[i] = Set(vertWeights[i], bone0Id, vertWeights[i].Get(id) + vertWeights[i].Get(bone0Id));
                        vertWeights[i] = Set(vertWeights[i], id, 0);
                    }
                }
            }
            if (rawVertWeights.Count > 0 && rawVertWeightIds.Count > 0)
            {
                for (int i = 0; i < rawVertWeights.Count; i++)
                {
                    List<int> idList = new List<int>(); //List of ids to combine to bone0Id's value
                    int bone0Id = 0;
                    bool bone0Set = false;
                    for (int id = 0; id < rawVertWeightIds[i].Count; id++)
                    {
                        int loopId = rawVertWeightIds[i][id];
                        if (loopId == 0)
                        {
                            if (bone0Set == false)
                            {
                                bone0Id = id;
                                bone0Set = true;
                                continue;
                            }
                            else
                            {
                                idList.Add(id);
                            }
                        }

                        if (loopId > maxBone)
                        {
                            rawVertWeightIds[i][id] = 0;
                            if (bone0Set == false)
                            {
                                bone0Id = id;

                            }
                            else
                            {
                                idList.Add(id);
                            }
                        }
                    }

                    foreach (var id in idList)
                    {
                        rawVertWeights[i][bone0Id] = rawVertWeights[i][id] + rawVertWeights[i][bone0Id];
                        rawVertWeights[i][id] = 0;
                    }
                    if (idList.Count > 0)
                    {
                        for (int id = idList.Count - 1; id >= 0; id--)
                        {
                            rawVertWeightIds.RemoveAt(idList[id]);
                            rawVertWeights.RemoveAt(idList[id]);
                        }
                    }
                }
            }
            if (trueVertWeights.Count > 0 && trueVertWeightIndices.Count > 0)
            {
                for (int i = 0; i < trueVertWeights.Count; i++)
                {
                    List<int> idList = new List<int>(); //List of ids to combine to bone0Id's value
                    int bone0Id = 0;
                    bool bone0Set = false;
                    for (int id = 0; id < trueVertWeightIndices[i].Length; id++)
                    {
                        int loopId = trueVertWeightIndices[i][id];
                        if (loopId == 0)
                        {
                            if (bone0Set == false)
                            {
                                bone0Id = id;
                                bone0Set = true;
                                continue;
                            }
                            else
                            {
                                idList.Add(id);
                            }
                        }

                        if (loopId > maxBone)
                        {
                            trueVertWeightIndices[i][id] = 0;
                            if (bone0Set == false)
                            {
                                bone0Id = id;

                            }
                            else
                            {
                                idList.Add(id);
                            }
                        }
                    }

                    foreach (var id in idList)
                    {
                        trueVertWeights[i] = Set(trueVertWeights[i], bone0Id, trueVertWeights[i].Get(id) + trueVertWeights[i].Get(bone0Id));
                        trueVertWeights[i] = Set(trueVertWeights[i], id, 0);
                    }
                    if (idList.Count > 0)
                    {
                        for (int id = idList.Count - 1; id >= 0; id--)
                        {
                            trueVertWeightIndices.RemoveAt(idList[id]);
                        }
                    }
                }
            }

        }

        public static Vector4 SumWeightsTo1(Vector4 trueWeight)
        {
            double sum = trueWeight.X + trueWeight.Y + trueWeight.Z + trueWeight.W;
            trueWeight.X = (float)(trueWeight.X / sum);
            trueWeight.Y = (float)(trueWeight.Y / sum);
            trueWeight.Z = (float)(trueWeight.Z / sum);
            trueWeight.W = (float)(trueWeight.W / sum);
            return trueWeight;
        }

        public void ProcessToPSO2Weights(bool useNGSWeights = false)
        {
            //Should be the same count for both lists, go through and populate as needed to cull weight counts that are too large
            for (int wt = 0; wt < rawVertWeights.Count; wt++)
            {
                int[] vertIds = new int[4];
                Vector4 vertWts = new Vector4();

                //Descending sort to get 
                for (int i = 0; i < rawVertWeights[wt].Count; i++)
                {
                    for (int j = 0; j < rawVertWeights[wt].Count; j++)
                        if (rawVertWeights[wt][j] < rawVertWeights[wt][i])
                        {
                            var tmp0 = rawVertWeights[wt][i];
                            var tmp1 = rawVertWeightIds[wt][i];

                            rawVertWeights[wt][i] = rawVertWeights[wt][j];
                            rawVertWeightIds[wt][i] = rawVertWeightIds[wt][j];
                            rawVertWeights[wt][j] = tmp0;
                            rawVertWeightIds[wt][j] = tmp1;
                        }
                }
                switch (rawVertWeights[wt].Count)
                {
                    //Case 0 really shouldn't happen
                    case 0:
                        vertWts.X = 1;
                        vertIds[0] = 0;
                        break;
                    case 1:
                        vertWts.X = rawVertWeights[wt][0];
                        vertIds[0] = rawVertWeightIds[wt][0];
                        break;
                    case 2:
                        vertWts.X = rawVertWeights[wt][0];
                        vertWts.Y = rawVertWeights[wt][1];
                        vertIds[0] = rawVertWeightIds[wt][0];
                        vertIds[1] = rawVertWeightIds[wt][1];
                        break;
                    case 3:
                        vertWts.X = rawVertWeights[wt][0];
                        vertWts.Y = rawVertWeights[wt][1];
                        vertWts.Z = rawVertWeights[wt][2];
                        vertIds[0] = rawVertWeightIds[wt][0];
                        vertIds[1] = rawVertWeightIds[wt][1];
                        vertIds[2] = rawVertWeightIds[wt][2];
                        break;
                    default:
                        vertWts.X = rawVertWeights[wt][0];
                        vertWts.Y = rawVertWeights[wt][1];
                        vertWts.Z = rawVertWeights[wt][2];
                        vertWts.W = rawVertWeights[wt][3];
                        vertIds[0] = rawVertWeightIds[wt][0];
                        vertIds[1] = rawVertWeightIds[wt][1];
                        vertIds[2] = rawVertWeightIds[wt][2];
                        vertIds[3] = rawVertWeightIds[wt][3];
                        break;
                }
                vertWts = SumWeightsTo1(vertWts);

                vertWeightIndices.Add(vertIds);
                vertWeights.Add(vertWts);
                if (useNGSWeights)
                {
                    ushort[] shortWeights = new ushort[] { (ushort)(vertWts.X * ushort.MaxValue), (ushort)(vertWts.Y * ushort.MaxValue),
                            (ushort)(vertWts.Z * ushort.MaxValue), (ushort)(vertWts.W * ushort.MaxValue), };
                    vertWeightsNGS.Add(shortWeights);
                }
            }
        }


        //Obviously in a lot of cases the bonepalettes should cover this, but can be helpful for NGS models
        public List<uint> GetUsedBonesTrueWeightIndices()
        {
            List<uint> weightIndices = new List<uint>();
            for (int i = 0; i < trueVertWeightIndices.Count; i++)
            {
                foreach (uint id in trueVertWeightIndices[i])
                {
                    if (!weightIndices.Contains(id))
                    {
                        weightIndices.Add(id);
                    }
                }
            }

            weightIndices.Sort();

            return weightIndices;
        }

        public void SortBoneIndexWeightOrderByWeight()
        {
            if (vertWeights.Count > 0)
            {
                for (int i = 0; i < vertWeightIndices.Count; i++)
                {
                    var indices = vertWeightIndices[i].ToArray();
                    var weights = vertWeights[i];
                    var weightsArr = new float[4];
                    for (int j = 0; j < indices.Length; j++)
                    {
                        switch (j)
                        {
                            case 0:
                                weightsArr[0] = weights.X;
                                break;
                            case 1:
                                weightsArr[1] = weights.Y;
                                break;
                            case 2:
                                weightsArr[2] = weights.Z;
                                break;
                            case 3:
                                weightsArr[3] = weights.W;
                                break;
                        }
                    }
                    Array.Sort(weightsArr, indices);
                    vertWeightIndices[i] = indices;
                    var newWeightVec4 = new Vector4(weightsArr[0], weightsArr[1], weightsArr[2], weightsArr[3]);
                    vertWeights[i] = newWeightVec4;
                }
            }
            if (trueVertWeights.Count > 0)
            {
                for (int i = 0; i < trueVertWeightIndices.Count; i++)
                {
                    var indices = trueVertWeightIndices[i].ToArray();
                    var weights = trueVertWeights[i];
                    var weightsArr = new float[4];
                    for (int j = 0; j < indices.Length; j++)
                    {
                        switch (j)
                        {
                            case 0:
                                weightsArr[0] = weights.X;
                                break;
                            case 1:
                                weightsArr[1] = weights.Y;
                                break;
                            case 2:
                                weightsArr[2] = weights.Z;
                                break;
                            case 3:
                                weightsArr[3] = weights.W;
                                break;
                        }
                    }
                    Array.Sort(weightsArr, indices);
                    trueVertWeightIndices[i] = indices;
                    var newWeightVec4 = new Vector4(weightsArr[0], weightsArr[1], weightsArr[2], weightsArr[3]);
                    trueVertWeights[i] = newWeightVec4;
                }
            }
            if (rawVertWeights.Count > 0)
            {
                for (int i = 0; i < rawVertWeightIds.Count; i++)
                {
                    var indices = rawVertWeightIds[i].ToArray();
                    var weights = rawVertWeights[i].ToArray();

                    Array.Sort(weights, indices);
                    rawVertWeightIds[i] = indices.ToList();
                    rawVertWeights[i] = weights.ToList();
                }
            }
        }
        #endregion

        #region FullVertexListOperations

        public static void AddVertices(VTXL sourceVtxl, Dictionary<int, int> vertIdDict, VTXL destinationVtxl, Vector3 tri, out int x, out int y, out int z)
        {
            if (vertIdDict.TryGetValue((int)tri.X, out var value))
            {
                x = value;
            }
            else
            {
                vertIdDict.Add((int)tri.X, destinationVtxl.vertPositions.Count);
                x = destinationVtxl.vertPositions.Count;
                VTXL.AppendVertex(sourceVtxl, destinationVtxl, (int)tri.X);
            }
            if (vertIdDict.TryGetValue((int)tri.Y, out var value2))
            {
                y = value2;
            }
            else
            {
                vertIdDict.Add((int)tri.Y, destinationVtxl.vertPositions.Count);
                y = destinationVtxl.vertPositions.Count;
                VTXL.AppendVertex(sourceVtxl, destinationVtxl, (int)tri.Y);
            }
            if (vertIdDict.TryGetValue((int)tri.Z, out var value3))
            {
                z = value3;
            }
            else
            {
                vertIdDict.Add((int)tri.Z, destinationVtxl.vertPositions.Count);
                z = destinationVtxl.vertPositions.Count;
                VTXL.AppendVertex(sourceVtxl, destinationVtxl, (int)tri.Z);
            }
        }

        public void InsertBlankVerts(int vertCount, VTXL modelVtxl)
        {
            if (modelVtxl.vertPositions.Count > 0)
            {
                vertPositions.AddRange(new Vector3[vertCount]); //Any vert should honestly have this if it's a proper vertex.
            }
            if (modelVtxl.vertNormals.Count > 0)
            {
                vertNormals.AddRange(new Vector3[vertCount]);
            }
            if (modelVtxl.vertNormalsNGS.Count > 0)
            {
                vertNormalsNGS.AddRange(new short[vertCount][]);
            }
            if (modelVtxl.vertBinormalList.Count > 0)
            {
                vertBinormalList.AddRange(new Vector3[vertCount]);
            }
            if (modelVtxl.vertBinormalListNGS.Count > 0)
            {
                vertBinormalListNGS.AddRange(new short[vertCount][]);
            }
            if (modelVtxl.vertTangentList.Count > 0)
            {
                vertTangentList.AddRange(new Vector3[vertCount]);
            }
            if (modelVtxl.vertTangentListNGS.Count > 0)
            {
                vertTangentListNGS.AddRange(new short[vertCount][]);
            }
            if (modelVtxl.vertColors.Count > 0)
            {
                vertColors.AddRange(new byte[vertCount][]);
            }
            if (modelVtxl.vertColor2s.Count > 0)
            {
                vertColor2s.AddRange(new byte[vertCount][]);
            }
            if (modelVtxl.uv1List.Count > 0)
            {
                uv1List.AddRange(new Vector2[vertCount]);
            }
            if (modelVtxl.uv1ListNGS.Count > 0)
            {
                uv1ListNGS.AddRange(new short[vertCount][]);
            }
            if (modelVtxl.uv2ListNGS.Count > 0)
            {
                uv2ListNGS.AddRange(new short[vertCount][]);
            }
            if (modelVtxl.uv3ListNGS.Count > 0)
            {
                uv3ListNGS.AddRange(new short[vertCount][]);
            }
            if (modelVtxl.uv4ListNGS.Count > 0)
            {
                uv4ListNGS.AddRange(new short[vertCount][]);
            }
            if (modelVtxl.uv2List.Count > 0)
            {
                uv2List.AddRange(new Vector2[vertCount]);
            }
            if (modelVtxl.uv3List.Count > 0)
            {
                uv3List.AddRange(new Vector2[vertCount]);
            }
            if (modelVtxl.uv4List.Count > 0)
            {
                uv4List.AddRange(new Vector2[vertCount]);
            }
            if (modelVtxl.vert0x22.Count > 0)
            {
                vert0x22.AddRange(new short[vertCount][]);
            }
            if (modelVtxl.vert0x23.Count > 0)
            {
                vert0x23.AddRange(new short[vertCount][]);
            }
            if (modelVtxl.vert0x24.Count > 0)
            {
                vert0x24.AddRange(new short[vertCount][]);
            }
            if (modelVtxl.vert0x25.Count > 0)
            {
                vert0x25.AddRange(new short[vertCount][]);
            }
            //These can... potentially be mutually exclusive, but the use cases for that are kind of limited and I don't and am not interested in handling them.
            if (modelVtxl.rawVertWeights.Count > 0)
            {
                for (int i = rawVertWeights.Count; i < vertCount; i++)
                {
                    rawVertWeights.Add(new List<float>());
                    rawVertWeightIds.Add(new List<int>());
                }
            }
            else if (modelVtxl.vertWeights.Count > 0)
            {
                int start = vertWeights.Count;
                vertWeights.AddRange(new Vector4[vertCount]);
                vertWeightIndices.AddRange(new int[vertCount][]);
                for (int i = start; i < start + vertCount; i++)
                {
                    vertWeightIndices[i] = new int[4];
                }
            }
            if (modelVtxl.vertWeightsNGS.Count > 0)
            {
                int start = vertWeightsNGS.Count;
                vertWeightsNGS.AddRange(new ushort[vertCount][]);
                for (int i = start; i < start + vertCount; i++)
                {
                    vertWeightsNGS[i] = new ushort[4];
                }
            }
        }

        public static void CopyVertex(VTXL sourceVTXL, VTXL destinationVTXL, int sourceIndex, int destinationIndex)
        {
            if (sourceVTXL.vertPositions.Count > sourceIndex)
            {
                destinationVTXL.vertPositions[destinationIndex] = sourceVTXL.vertPositions[sourceIndex];
            }
            if (sourceVTXL.vertWeightsNGS.Count > sourceIndex)
            {
                destinationVTXL.vertWeightsNGS[destinationIndex] = (ushort[])sourceVTXL.vertWeightsNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.vertWeights.Count > sourceIndex)
            {
                destinationVTXL.vertWeights[destinationIndex] = sourceVTXL.vertWeights[sourceIndex];
            }
            if (sourceVTXL.vertNormalsNGS.Count > sourceIndex)
            {
                destinationVTXL.vertNormalsNGS[destinationIndex] = (short[])sourceVTXL.vertNormalsNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.vertNormals.Count > sourceIndex)
            {
                destinationVTXL.vertNormals[destinationIndex] = sourceVTXL.vertNormals[sourceIndex];
            }
            if (sourceVTXL.vertColors.Count > sourceIndex)
            {
                destinationVTXL.vertColors[destinationIndex] = (byte[])sourceVTXL.vertColors[sourceIndex].Clone();
            }
            if (sourceVTXL.vertColor2s.Count > sourceIndex)
            {
                destinationVTXL.vertColor2s[destinationIndex] = (byte[])sourceVTXL.vertColor2s[sourceIndex].Clone();
            }
            if (sourceVTXL.vertWeightIndices.Count > sourceIndex)
            {
                destinationVTXL.vertWeightIndices[destinationIndex] = (int[])sourceVTXL.vertWeightIndices[sourceIndex].Clone();
            }
            if (sourceVTXL.uv1ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv1ListNGS[destinationIndex] = (short[])sourceVTXL.uv1ListNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.uv2ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv2ListNGS[destinationIndex] = (short[])sourceVTXL.uv2ListNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.uv3ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv3ListNGS[destinationIndex] = (short[])sourceVTXL.uv3ListNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.uv4ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv4ListNGS[destinationIndex] = (short[])sourceVTXL.uv4ListNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.uv1List.Count > sourceIndex)
            {
                destinationVTXL.uv1List[destinationIndex] = sourceVTXL.uv1List[sourceIndex];
            }
            if (sourceVTXL.uv2List.Count > sourceIndex)
            {
                destinationVTXL.uv2List[destinationIndex] = sourceVTXL.uv2List[sourceIndex];
            }
            if (sourceVTXL.uv3List.Count > sourceIndex)
            {
                destinationVTXL.uv3List[destinationIndex] = sourceVTXL.uv3List[sourceIndex];
            }
            if (sourceVTXL.uv4List.Count > sourceIndex)
            {
                destinationVTXL.uv4List[destinationIndex] = sourceVTXL.uv4List[sourceIndex];
            }
            if (sourceVTXL.vert0x22.Count > sourceIndex)
            {
                destinationVTXL.vert0x22[destinationIndex] = (short[])sourceVTXL.vert0x22[sourceIndex].Clone();
            }
            if (sourceVTXL.vert0x23.Count > sourceIndex)
            {
                destinationVTXL.vert0x23[destinationIndex] = (short[])sourceVTXL.vert0x23[sourceIndex].Clone();
            }
            if (sourceVTXL.vert0x24.Count > sourceIndex)
            {
                destinationVTXL.vert0x24[destinationIndex] = (short[])sourceVTXL.vert0x24[sourceIndex].Clone();
            }
            if (sourceVTXL.vert0x25.Count > sourceIndex)
            {
                destinationVTXL.vert0x25[destinationIndex] = (short[])sourceVTXL.vert0x25[sourceIndex].Clone();
            }
            if (sourceVTXL.vertTangentListNGS.Count > sourceIndex)
            {
                destinationVTXL.vertTangentListNGS[destinationIndex] = (short[])sourceVTXL.vertTangentListNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.vertTangentList.Count > sourceIndex)
            {
                destinationVTXL.vertTangentList[destinationIndex] = sourceVTXL.vertTangentList[sourceIndex];
            }
            if (sourceVTXL.vertBinormalListNGS.Count > sourceIndex)
            {
                destinationVTXL.vertBinormalListNGS[destinationIndex] = (short[])sourceVTXL.vertBinormalListNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.vertBinormalList.Count > sourceIndex)
            {
                destinationVTXL.vertBinormalList[destinationIndex] = sourceVTXL.vertBinormalList[sourceIndex];
            }
            if (sourceVTXL.rawVertWeights.Count > sourceIndex)
            {
                destinationVTXL.rawVertWeights[destinationIndex] = new List<float>(sourceVTXL.rawVertWeights[sourceIndex]);
            }
            if (sourceVTXL.rawVertWeightIds.Count > sourceIndex)
            {
                destinationVTXL.rawVertWeightIds[destinationIndex] = new List<int>(sourceVTXL.rawVertWeightIds[sourceIndex]);
            }
            if (sourceVTXL.trueVertWeights.Count > sourceIndex)
            {
                destinationVTXL.trueVertWeights[destinationIndex] = sourceVTXL.trueVertWeights[sourceIndex];
            }
            if (sourceVTXL.trueVertWeightIndices.Count > sourceIndex)
            {
                destinationVTXL.trueVertWeightIndices[destinationIndex] = (int[])sourceVTXL.trueVertWeightIndices[sourceIndex].Clone();
            }
        }

        public static void AppendVertex(VTXL sourceVTXL, VTXL destinationVTXL, int sourceIndex)
        {
            if (sourceVTXL.vertPositions.Count > sourceIndex)
            {
                destinationVTXL.vertPositions.Add(sourceVTXL.vertPositions[sourceIndex]);
            }
            if (sourceVTXL.vertWeightsNGS.Count > sourceIndex)
            {
                destinationVTXL.vertWeightsNGS.Add((ushort[])sourceVTXL.vertWeightsNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.vertWeights.Count > sourceIndex)
            {
                destinationVTXL.vertWeights.Add(sourceVTXL.vertWeights[sourceIndex]);
            }
            if (sourceVTXL.vertNormalsNGS.Count > sourceIndex)
            {
                destinationVTXL.vertNormalsNGS.Add((short[])sourceVTXL.vertNormalsNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.vertNormals.Count > sourceIndex)
            {
                destinationVTXL.vertNormals.Add(sourceVTXL.vertNormals[sourceIndex]);
            }
            if (sourceVTXL.vertColors.Count > sourceIndex)
            {
                destinationVTXL.vertColors.Add((byte[])sourceVTXL.vertColors[sourceIndex].Clone());
            }
            if (sourceVTXL.vertColor2s.Count > sourceIndex)
            {
                destinationVTXL.vertColor2s.Add((byte[])sourceVTXL.vertColor2s[sourceIndex].Clone());
            }
            if (sourceVTXL.vertWeightIndices.Count > sourceIndex)
            {
                destinationVTXL.vertWeightIndices.Add((int[])sourceVTXL.vertWeightIndices[sourceIndex].Clone());
            }
            if (sourceVTXL.uv1ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv1ListNGS.Add((short[])sourceVTXL.uv1ListNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.uv2ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv2ListNGS.Add((short[])sourceVTXL.uv2ListNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.uv3ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv3ListNGS.Add((short[])sourceVTXL.uv3ListNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.uv4ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv4ListNGS.Add((short[])sourceVTXL.uv4ListNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.uv1List.Count > sourceIndex)
            {
                destinationVTXL.uv1List.Add(sourceVTXL.uv1List[sourceIndex]);
            }
            if (sourceVTXL.uv2List.Count > sourceIndex)
            {
                destinationVTXL.uv2List.Add(sourceVTXL.uv2List[sourceIndex]);
            }
            if (sourceVTXL.uv3List.Count > sourceIndex)
            {
                destinationVTXL.uv3List.Add(sourceVTXL.uv3List[sourceIndex]);
            }
            if (sourceVTXL.uv4List.Count > sourceIndex)
            {
                destinationVTXL.uv4List.Add(sourceVTXL.uv4List[sourceIndex]);
            }
            if (sourceVTXL.vert0x22.Count > sourceIndex)
            {
                destinationVTXL.vert0x22.Add((short[])sourceVTXL.vert0x22[sourceIndex].Clone());
            }
            if (sourceVTXL.vert0x23.Count > sourceIndex)
            {
                destinationVTXL.vert0x23.Add((short[])sourceVTXL.vert0x23[sourceIndex].Clone());
            }
            if (sourceVTXL.vert0x24.Count > sourceIndex)
            {
                destinationVTXL.vert0x24.Add((short[])sourceVTXL.vert0x24[sourceIndex].Clone());
            }
            if (sourceVTXL.vert0x25.Count > sourceIndex)
            {
                destinationVTXL.vert0x25.Add((short[])sourceVTXL.vert0x25[sourceIndex].Clone());
            }
            if (sourceVTXL.vertTangentListNGS.Count > sourceIndex)
            {
                destinationVTXL.vertTangentListNGS.Add((short[])sourceVTXL.vertTangentListNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.vertTangentList.Count > sourceIndex)
            {
                destinationVTXL.vertTangentList.Add(sourceVTXL.vertTangentList[sourceIndex]);
            }
            if (sourceVTXL.vertBinormalListNGS.Count > sourceIndex)
            {
                destinationVTXL.vertBinormalListNGS.Add((short[])sourceVTXL.vertBinormalListNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.vertBinormalList.Count > sourceIndex)
            {
                destinationVTXL.vertBinormalList.Add(sourceVTXL.vertBinormalList[sourceIndex]);
            }
            if (sourceVTXL.rawVertWeights.Count > sourceIndex)
            {
                destinationVTXL.rawVertWeights.Add(new List<float>(sourceVTXL.rawVertWeights[sourceIndex]));
            }
            if (sourceVTXL.rawVertWeightIds.Count > sourceIndex)
            {
                destinationVTXL.rawVertWeightIds.Add(new List<int>(sourceVTXL.rawVertWeightIds[sourceIndex]));
            }
            if (sourceVTXL.trueVertWeights.Count > sourceIndex)
            {
                destinationVTXL.trueVertWeights.Add(sourceVTXL.trueVertWeights[sourceIndex]);
            }
            if (sourceVTXL.trueVertWeightIndices.Count > sourceIndex)
            {
                destinationVTXL.trueVertWeightIndices.Add((int[])sourceVTXL.trueVertWeightIndices[sourceIndex].Clone());
            }
        }

        public static void AppendAllVertices(VTXL sourceVTXL, VTXL destinationVTXL)
        {
            if(sourceVTXL == null || destinationVTXL == null)
            {
                return;
            }
            destinationVTXL.vertPositions.AddRange(sourceVTXL.vertPositions);
            destinationVTXL.vertWeightsNGS.AddRange(sourceVTXL.vertWeightsNGS.ConvertAll(wt => (ushort[])wt.Clone()));
            destinationVTXL.vertWeights.AddRange(sourceVTXL.vertWeights);
            destinationVTXL.vertNormalsNGS.AddRange(sourceVTXL.vertNormalsNGS.ConvertAll(nrm => (short[])nrm.Clone()));
            destinationVTXL.vertNormals.AddRange(sourceVTXL.vertNormals);
            destinationVTXL.vertColors.AddRange(sourceVTXL.vertColors.ConvertAll(clr => (byte[])clr.Clone()));
            destinationVTXL.vertColor2s.AddRange(sourceVTXL.vertColor2s.ConvertAll(clr => (byte[])clr.Clone()));
            destinationVTXL.vertWeightIndices.AddRange(sourceVTXL.vertWeightIndices.ConvertAll(wt => (int[])wt.Clone()));
            destinationVTXL.uv1ListNGS.AddRange(sourceVTXL.uv1ListNGS.ConvertAll(uv => (short[])uv.Clone()));
            destinationVTXL.uv2ListNGS.AddRange(sourceVTXL.uv2ListNGS.ConvertAll(uv => (short[])uv.Clone()));
            destinationVTXL.uv3ListNGS.AddRange(sourceVTXL.uv3ListNGS.ConvertAll(uv => (short[])uv.Clone()));
            destinationVTXL.uv4ListNGS.AddRange(sourceVTXL.uv4ListNGS.ConvertAll(uv => (short[])uv.Clone()));
            destinationVTXL.uv1List.AddRange(sourceVTXL.uv1List);
            destinationVTXL.uv2List.AddRange(sourceVTXL.uv2List);
            destinationVTXL.uv3List.AddRange(sourceVTXL.uv3List);
            destinationVTXL.uv4List.AddRange(sourceVTXL.uv4List);
            destinationVTXL.vert0x22.AddRange(sourceVTXL.vert0x22.ConvertAll(uv => (short[])uv.Clone()));
            destinationVTXL.vert0x23.AddRange(sourceVTXL.vert0x23.ConvertAll(uv => (short[])uv.Clone()));
            destinationVTXL.vert0x24.AddRange(sourceVTXL.vert0x24.ConvertAll(uv => (short[])uv.Clone()));
            destinationVTXL.vert0x25.AddRange(sourceVTXL.vert0x25.ConvertAll(uv => (short[])uv.Clone()));
            destinationVTXL.vertTangentListNGS.AddRange(sourceVTXL.vertTangentListNGS.ConvertAll(nrm => (short[])nrm.Clone()));
            destinationVTXL.vertTangentList.AddRange(sourceVTXL.vertTangentList);
            destinationVTXL.vertBinormalListNGS.AddRange(sourceVTXL.vertBinormalListNGS.ConvertAll(nrm => (short[])nrm.Clone()));
            destinationVTXL.vertBinormalList.AddRange(sourceVTXL.vertBinormalList);
            destinationVTXL.rawVertWeights.AddRange(sourceVTXL.rawVertWeights.ConvertAll(wt => new List<float>((float[])wt.ToArray().Clone())));
            destinationVTXL.rawVertWeightIds.AddRange(sourceVTXL.rawVertWeightIds.ConvertAll(wt => new List<int>((int[])wt.ToArray().Clone())));
            destinationVTXL.trueVertWeights.AddRange(sourceVTXL.trueVertWeights);
            destinationVTXL.trueVertWeightIndices.AddRange(sourceVTXL.trueVertWeightIndices.ConvertAll(wt => (int[])wt.Clone()));
            destinationVTXL.edgeVerts.AddRange(sourceVTXL.edgeVerts.ConvertAll(ev => (ushort)(destinationVTXL.vertPositions.Count - 1 + ev)));
        }

        public static bool IsSameVertex(VTXL vtxl, int vertIndex, VTXL vtxl2, int vertIndex2)
        {
            if (vtxl.vertPositions.Count > 0 && !vtxl.vertPositions[vertIndex].Equals(vtxl2.vertPositions[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vertNormals.Count > 0 && !vtxl.vertNormals[vertIndex].Equals(vtxl2.vertNormals[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vertNormalsNGS.Count > 0 && !vtxl.vertNormalsNGS[vertIndex].Equals(vtxl2.vertNormalsNGS[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vertColors.Count > 0 && !IsEqualByteArray(vtxl.vertColors[vertIndex], vtxl2.vertColors[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vertColor2s.Count > 0 && !IsEqualByteArray(vtxl.vertColor2s[vertIndex], vtxl2.vertColor2s[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv1List.Count > 0 && !vtxl.uv1List[vertIndex].Equals(vtxl2.uv1List[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv1ListNGS.Count > 0 && !IsEqualShortArray(vtxl.uv1ListNGS[vertIndex], vtxl2.uv1ListNGS[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv2ListNGS.Count > 0 && !IsEqualShortArray(vtxl.uv2ListNGS[vertIndex], vtxl2.uv2ListNGS[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv3ListNGS.Count > 0 && !IsEqualShortArray(vtxl.uv3ListNGS[vertIndex], vtxl2.uv3ListNGS[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv4ListNGS.Count > 0 && !IsEqualShortArray(vtxl.uv4ListNGS[vertIndex], vtxl2.uv4ListNGS[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv2List.Count > 0 && !vtxl.uv2List[vertIndex].Equals(vtxl2.uv2List[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv3List.Count > 0 && !vtxl.uv3List[vertIndex].Equals(vtxl2.uv3List[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv4List.Count > 0 && !vtxl.uv4List[vertIndex].Equals(vtxl2.uv4List[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vert0x22.Count > 0 && !IsEqualShortArray(vtxl.vert0x22[vertIndex], vtxl2.vert0x22[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vert0x23.Count > 0 && !IsEqualShortArray(vtxl.vert0x23[vertIndex], vtxl2.vert0x23[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vert0x24.Count > 0 && !IsEqualShortArray(vtxl.vert0x24[vertIndex], vtxl2.vert0x24[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vert0x25.Count > 0 && !IsEqualShortArray(vtxl.vert0x25[vertIndex], vtxl2.vert0x25[vertIndex2]))
            {
                return false;
            }

            return true;
        }

        public VTXL Clone()
        {
            VTXL newVTXL = new VTXL();

            newVTXL.vertPositions = new List<Vector3>(vertPositions);
            newVTXL.vertWeightsNGS = vertWeightsNGS.ConvertAll(wt => (ushort[])wt.Clone()).ToList();
            newVTXL.vertNormalsNGS = vertNormalsNGS.ConvertAll(nrm => (short[])nrm.Clone()).ToList();
            newVTXL.vertColors = vertColors.ConvertAll(clr => (byte[])clr.Clone()).ToList();
            newVTXL.vertColor2s = vertColor2s.ConvertAll(clr => (byte[])clr.Clone()).ToList();
            newVTXL.vertWeightIndices = vertWeightIndices.ConvertAll(wt => (int[])wt.Clone()).ToList();
            newVTXL.uv1ListNGS = uv1ListNGS.ConvertAll(uv => (short[])uv.Clone()).ToList();
            newVTXL.uv2ListNGS = uv2ListNGS.ConvertAll(uv => (short[])uv.Clone()).ToList();
            newVTXL.uv3ListNGS = uv3ListNGS.ConvertAll(uv => (short[])uv.Clone()).ToList();
            newVTXL.uv4ListNGS = uv4ListNGS.ConvertAll(uv => (short[])uv.Clone()).ToList();
            newVTXL.uv1List = new List<Vector2>(uv1List);
            newVTXL.uv2List = new List<Vector2>(uv2List);
            newVTXL.uv3List = new List<Vector2>(uv3List);
            newVTXL.uv4List = new List<Vector2>(uv4List);
            newVTXL.vert0x22 = vert0x22.ConvertAll(uv => (short[])uv.Clone()).ToList();
            newVTXL.vert0x23 = vert0x23.ConvertAll(uv => (short[])uv.Clone()).ToList();
            newVTXL.vert0x24 = vert0x24.ConvertAll(uv => (short[])uv.Clone()).ToList();
            newVTXL.vert0x25 = vert0x25.ConvertAll(uv => (short[])uv.Clone()).ToList();
            newVTXL.vertTangentListNGS = vertTangentListNGS.ConvertAll(nrm => (short[])nrm.Clone()).ToList();
            newVTXL.vertBinormalListNGS = vertBinormalListNGS.ConvertAll(nrm => (short[])nrm.Clone()).ToList();
            newVTXL.vertWeights = new List<Vector4>(vertWeights);
            newVTXL.vertNormals = new List<Vector3>(vertNormals);
            newVTXL.vertTangentList = new List<Vector3>(vertTangentList);
            newVTXL.vertBinormalList = new List<Vector3>(vertBinormalList);
            newVTXL.bonePalette = new List<ushort>(bonePalette);
            newVTXL.edgeVerts = new List<ushort>(edgeVerts);
            newVTXL.rawVertWeights = rawVertWeights.ConvertAll(wt => new List<float>((float[])wt.ToArray().Clone())).ToList();
            newVTXL.rawVertWeightIds = rawVertWeightIds.ConvertAll(wt => new List<int>((int[])wt.ToArray().Clone())).ToList();
            newVTXL.rawVertId = new List<int>(rawVertId);
            newVTXL.rawFaceId = new List<int>(rawFaceId);
            newVTXL.trueVertWeights = new List<Vector4>(trueVertWeights);
            newVTXL.trueVertWeightIndices = trueVertWeightIndices.ConvertAll(wt => (int[])wt.Clone()).ToList();

            return newVTXL;
        }
        #endregion
    }
}
