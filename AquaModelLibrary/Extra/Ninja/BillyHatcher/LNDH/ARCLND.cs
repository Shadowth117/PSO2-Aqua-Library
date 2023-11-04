using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher.LNDH
{
    //ARCLND
    public struct ARCLNDHeader
    {
        public int mainDataOffset;
        public int extraFileCount;
        public int extraFileOffsetsOffset;
        public int motionFileOffset;    //Often 0

        public int texRefTableOffset;
        public int GVMOffset;
    }

    public struct ARCLNDRefTableHead
    {
        public int entryOffset;
        public int entryCount;
    }

    public struct ARCLNDRefEntry
    {
        public int textOffset;
        public int unkInt0;
        public int unkInt1;
    }

    public struct ARCLNDMainDataHeader
    {
        public int mainOffsetTableOffset;
        public int unkOffset0;
        public int unkCount;
        public int unkOffset1;

        public int unkInt_10;
        public int unkInt_14;
        public int unkInt_18;
        public int unkInt_1C;
    }

    //Similar to NN's main branching point struct
    public struct ARCLNDMainOffsetTable
    {
        public int landEntryCount;
        public int landEntryOffset;
        public int vertDataCount;
        public int vertDataOffset;

        public int faceSetsCount;
        public int faceSetsOffset;
        public int nodeBoundingCount;
        public int nodeBoundingOffset;

        public int unkCount3;
        public int unkCount4;
        public int unkOffset3;
    }

    public struct ARCLNDLandEntryRef
    {
        public int unkInt;
        public int offset;
    }

    public struct ARCLNDLandEntry
    {
        public int unkInt0;
        public int unkInt1;
        public int unkInt2;
        public int unkInt3;

        public int unkInt4;
        public int unkInt5;
        public int unkInt6;
        public int unkInt7;

        public int ushort0;
        public int ushort1;
        public int unkInt8;
    }

    public struct ARCLNDVertDataRef
    {
        public int unkInt;
        public int offset;
    }

    public struct ARCLNDVertDataInfo
    {
        public ARCLNDVertData Position;
        public ARCLNDVertData Normal;
        public ARCLNDVertData VertColor;
        public ARCLNDVertData VertColor2;
        public ARCLNDVertData UV1; //??
        public ARCLNDVertData UV2; //??
    }

    public struct ARCLNDVertData
    {
        public ushort type;
        public ushort count;
        public int offset;
    }

    public struct ARCLNDFaceDataRef
    {
        public int unkInt;
        public int offset;
    }

    public struct ARCLNDFaceDataHead
    {
        public int unkInt0;
        public int unkInt1;
        public int unkInt2;
        public int faceDataOffset;
        public int bufferSize;
    }

    public struct ARCLNDNodeBounding
    {
        public float unkFlt_00;
        public ushort usht_04;
        public ushort usht_06;
        public ushort usht_08;
        public ushort usht_0A;
        public int int_0C;

        public int int_10;
        public int int_14;
        public int int_18;
        public int int_1C;

        public int int_20;
        public int int_24;
        public int int_28;
        public int int_2C;

        public Vector2 minBounding;
        public Vector2 maxBounding;
    }

    public struct unkData
    {
        public int int_00;
        public int int_04;
        public int int_08;
        public int int_0C;
        public int int_10;
    }
}
