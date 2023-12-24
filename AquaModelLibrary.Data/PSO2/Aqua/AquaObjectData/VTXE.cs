using AquaModelLibrary.Helpers.PSO2;
using System.Diagnostics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    //Definitions for data in VTXL array. Same as previous iteration in 0xC33, however relative addresses may be unused now, at least on the parsing side. 
    //VTXE is also separated from VTXL in 0xC33 as opposed to the pairing structure seen in previous variants.
    public class VTXE
    {
        public List<VTXEElement> vertDataTypes = new List<VTXEElement>();

        public VTXE() { }

        public VTXE(List<Dictionary<int, object>> vtxeRaw)
        {
            for (int i = 0; i < vtxeRaw.Count; i++)
            {
                VTXEElement vtxeEle = new VTXEElement();
                vtxeEle.dataType = (int)vtxeRaw[i][0xD0];
                vtxeEle.structVariation = (int)vtxeRaw[i][0xD1];
                vtxeEle.relativeAddress = (int)vtxeRaw[i][0xD2];
                vtxeEle.reserve0 = (int)vtxeRaw[i][0xD3];

                vertDataTypes.Add(vtxeEle);
            }
        }

        public static VTXE ConstructFromVTXL(VTXL vtxl, out int vertSize)
        {
            int curLength = 0;
            VTXE vtxe = new VTXE();

            if (vtxl.vertPositions.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x0, 0x3, curLength));
                curLength += 0xC;
            }
            if (vtxl.vertWeightsNGS.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x1, 0x11, curLength));
                curLength += 0x8;
            }
            else if (vtxl.vertWeights.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x1, 0x4, curLength));
                curLength += 0x10;
            }
            if (vtxl.vertNormals.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x2, 0x3, curLength));
                curLength += 0xC;
            }
            if (vtxl.vertColors.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x3, 0x5, curLength));
                curLength += 0x4;
            }
            if (vtxl.vertColor2s.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x4, 0x5, curLength));
                curLength += 0x4;
            }
            if (vtxl.vertWeightIndices.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0xb, 0x7, curLength));
                curLength += 0x4;
            }
            if (vtxl.uv1List.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x10, 0x2, curLength));
                curLength += 0x8;
            }
            if (vtxl.uv2List.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x11, 0x2, curLength));
                curLength += 0x8;
            }
            if (vtxl.uv3List.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x12, 0x2, curLength));
                curLength += 0x8;
            }
            if (vtxl.uv4List.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x13, 0x2, curLength));
                curLength += 0x8;
            }
            if (vtxl.vertBinormalList.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x21, 0x3, curLength));
                curLength += 0xC;
            }
            if (vtxl.vertTangentList.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x20, 0x3, curLength));
                curLength += 0xC;
            }
            if (vtxl.vert0x22.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x22, 0xC, curLength));
                curLength += 0x4;
            }
            if (vtxl.vert0x23.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x23, 0xC, curLength));
                curLength += 0x4;
            }
            if (vtxl.vert0x24.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x24, 0xC, curLength));
                curLength += 0x4;
            }
            if (vtxl.vert0x25.Count > 0)
            {
                vtxe.vertDataTypes.Add(new VTXEElement(0x25, 0xC, curLength));
                curLength += 0x4;
            }

            vertSize = curLength;
            return vtxe;
        }

        public int GetVTXESize()
        {
            int size = 0;
            for (int j = 0; j < vertDataTypes.Count; j++)
            {
                switch (vertDataTypes[j].dataType)
                {
                    case (int)VertFlags.VertPosition:
                        size += 0xC;
                        break;
                    case (int)VertFlags.VertWeight:
                        switch (vertDataTypes[j].structVariation)
                        {
                            case 0x4:
                                size += 0x10;
                                break;
                            case 0x11:
                                size += 0x8;
                                break;
                        }
                        break;
                    case (int)VertFlags.VertNormal:
                        switch (vertDataTypes[j].structVariation)
                        {
                            case 0x3:
                                size += 0xC;
                                break;
                            case 0xF:
                                size += 0x8;
                                break;
                        }
                        break;
                    case (int)VertFlags.VertColor:
                        size += 0x4;
                        break;
                    case (int)VertFlags.VertColor2:
                        size += 0x4;
                        break;
                    case (int)VertFlags.VertWeightIndex:
                        size += 0x4;
                        break;
                    case (int)VertFlags.VertUV1:
                        switch (vertDataTypes[j].structVariation)
                        {
                            case 0x2:
                                size += 0x8;
                                break;
                            case 0xE:
                                size += 0x4;
                                break;
                        }
                        break;
                    case (int)VertFlags.VertUV2:
                        switch (vertDataTypes[j].structVariation)
                        {
                            case 0x2:
                                size += 0x8;
                                break;
                            case 0xE:
                                size += 0x4;
                                break;
                        }
                        break;
                    case (int)VertFlags.VertUV3:
                        switch (vertDataTypes[j].structVariation)
                        {
                            case 0x2:
                                size += 0x8;
                                break;
                            case 0xE:
                                size += 0x4;
                                break;
                        }
                        break;
                    case (int)VertFlags.VertUV4:
                        switch (vertDataTypes[j].structVariation)
                        {
                            case 0x2:
                                size += 0x8;
                                break;
                            case 0xE:
                                size += 0x4;
                                break;
                        }
                        break;
                    case (int)VertFlags.VertTangent:
                        switch (vertDataTypes[j].structVariation)
                        {
                            case 0x3:
                                size += 0xC;
                                break;
                            case 0xF:
                                size += 0x8;
                                break;
                        }
                        break;
                    case (int)VertFlags.VertBinormal:
                        switch (vertDataTypes[j].structVariation)
                        {
                            case 0x3:
                                size += 0xC;
                                break;
                            case 0xF:
                                size += 0x8;
                                break;
                        }
                        break;
                    case (int)VertFlags.Vert0x22:
                        size += 0x4;
                        break;
                    case (int)VertFlags.Vert0x23:
                        size += 0x4;
                        break;
                    case (int)VertFlags.Vert0x24:
                        size += 0x4;
                        break;
                    case (int)VertFlags.Vert0x25:
                        size += 0x4;
                        break;
                    default:
                        Debug.WriteLine($"Unknown Vert type {vertDataTypes[j].dataType}! Please report!");
                        throw new Exception("Not implemented!");
                }
            }
            return size;
        }

        public byte[] GetBytesVTBF()
        {
            List<byte> outBytes = new List<byte>();

            //VTXE
            for (int i = 0; i < vertDataTypes.Count; i++)
            {
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                VTBFMethods.AddBytes(outBytes, 0xD0, 0x9, BitConverter.GetBytes(vertDataTypes[i].dataType));
                VTBFMethods.AddBytes(outBytes, 0xD1, 0x9, BitConverter.GetBytes(vertDataTypes[i].structVariation));
                VTBFMethods.AddBytes(outBytes, 0xD2, 0x9, BitConverter.GetBytes(vertDataTypes[i].relativeAddress));
                VTBFMethods.AddBytes(outBytes, 0xD3, 0x9, BitConverter.GetBytes(vertDataTypes[i].reserve0));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on VTXE
            //Subtag count
            VTBFMethods.WriteTagHeader(outBytes, "VTXE", 0, (ushort)(vertDataTypes.Count * 5 + 1));

            return outBytes.ToArray();
        }

        public VTXE Clone()
        {
            VTXE newVtxe = new VTXE();
            newVtxe.vertDataTypes = new List<VTXEElement>(vertDataTypes);

            return newVtxe;
        }
    }

    public struct VTXEElement
    {
        public int dataType;        //0xD0, type 0x9
        public int structVariation; //0xD1, type 0x9 //3 for Vector3, 4 for Vector4, 5 for 4 byte vert color, 2 for Vector2, 7 for 4 byte values
        public int relativeAddress; //0xD2, type 0x9
        public int reserve0;        //0xD3, type 0x9

        public VTXEElement(int _dataType, int _structType, int _relativeAddress)
        {
            dataType = _dataType;
            structVariation = _structType;
            relativeAddress = _relativeAddress;
            reserve0 = 0;
        }

        public void Set(int _dataType, int _structType, int _relativeAddress)
        {
            dataType = _dataType;
            structVariation = _structType;
            relativeAddress = _relativeAddress;
            reserve0 = 0;
        }
    }

}
