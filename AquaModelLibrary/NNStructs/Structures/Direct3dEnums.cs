using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.NNStructs.Structures
{
    public enum Direct3DComparisonFunction
    {
        D3DCMP_NEVER,
        D3DCMP_LESS,
        D3DCMP_EQUAL,
        D3DCMP_LESSEQUAL,
        D3DCMP_GREATER,
        D3DCMP_NOTEQUAL,
        D3DCMP_GREATEREQUAL,
        D3DCMP_ALWAYS,
    }

    public enum Direct3DRenderStateBlendOperation
    {
        D3DBLENDOP_ADD,
        D3DBLENDOP_MIN,
        D3DBLENDOP_MAX,
        INVALID_8009,
        D3DBLENDOP_SUBTRACT,
        D3DBLENDOP_REVSUBTRACT,
        INVALID_F005,
        INVALID_F006,
    }

    public enum Direct3DRenderStateBlendMode
    {
        D3DBLEND_ZERO,
        D3DBLEND_ONE,
        D3DBLEND_SRCCOLOR,
        D3DBLEND_INVSRCCOLOR,
        D3DBLEND_SRCALPHA,
        D3DBLEND_INVSRCALPHA,
        D3DBLEND_DESTALPHA,
        D3DBLEND_INVDESTALPHA,
        D3DBLEND_DESTCOLOR,
        D3DBLEND_INVDESTCOLOR,
        D3DBLEND_SRCALPHASAT,
        D3DBLEND_ONE_8001,
        D3DBLEND_ONE_8002,
        D3DBLEND_ONE_8003,
        D3DBLEND_ONE_8004,
    }

    internal static class Direct3DEnums
    {
        public static Direct3DComparisonFunction PsuValueToCompareFunc(
          int psuCompareFunc)
        {
            switch (psuCompareFunc)
            {
                case 512:
                    return Direct3DComparisonFunction.D3DCMP_NEVER;
                case 513:
                    return Direct3DComparisonFunction.D3DCMP_LESS;
                case 514:
                    return Direct3DComparisonFunction.D3DCMP_EQUAL;
                case 515:
                    return Direct3DComparisonFunction.D3DCMP_LESSEQUAL;
                case 516:
                    return Direct3DComparisonFunction.D3DCMP_GREATER;
                case 517:
                    return Direct3DComparisonFunction.D3DCMP_NOTEQUAL;
                case 518:
                    return Direct3DComparisonFunction.D3DCMP_GREATEREQUAL;
                case 519:
                    return Direct3DComparisonFunction.D3DCMP_ALWAYS;
                default:
                    return Direct3DComparisonFunction.D3DCMP_NEVER;
            }
        }

        public static int ToPsuValue(this Direct3DComparisonFunction compareFunc)
        {
            switch (compareFunc)
            {
                case Direct3DComparisonFunction.D3DCMP_NEVER:
                    return 512;
                case Direct3DComparisonFunction.D3DCMP_LESS:
                    return 513;
                case Direct3DComparisonFunction.D3DCMP_EQUAL:
                    return 514;
                case Direct3DComparisonFunction.D3DCMP_LESSEQUAL:
                    return 515;
                case Direct3DComparisonFunction.D3DCMP_GREATER:
                    return 516;
                case Direct3DComparisonFunction.D3DCMP_NOTEQUAL:
                    return 517;
                case Direct3DComparisonFunction.D3DCMP_GREATEREQUAL:
                    return 518;
                case Direct3DComparisonFunction.D3DCMP_ALWAYS:
                    return 519;
                default:
                    return 512;
            }
        }

        public static Direct3DRenderStateBlendOperation PsuValueToBlendOperation(
          int psuBlendOp)
        {
            switch (psuBlendOp)
            {
                case 32774:
                    return Direct3DRenderStateBlendOperation.D3DBLENDOP_ADD;
                case 32775:
                    return Direct3DRenderStateBlendOperation.D3DBLENDOP_MIN;
                case 32776:
                    return Direct3DRenderStateBlendOperation.D3DBLENDOP_MAX;
                case 32777:
                    return Direct3DRenderStateBlendOperation.INVALID_8009;
                case 32778:
                    return Direct3DRenderStateBlendOperation.D3DBLENDOP_SUBTRACT;
                case 32779:
                    return Direct3DRenderStateBlendOperation.D3DBLENDOP_REVSUBTRACT;
                case 61445:
                    return Direct3DRenderStateBlendOperation.INVALID_F005;
                case 61446:
                    return Direct3DRenderStateBlendOperation.INVALID_F006;
                default:
                    return Direct3DRenderStateBlendOperation.D3DBLENDOP_ADD;
            }
        }

        public static int ToPsuValue(
          this Direct3DRenderStateBlendOperation blendOperation)
        {
            switch (blendOperation)
            {
                case Direct3DRenderStateBlendOperation.D3DBLENDOP_ADD:
                    return 32774;
                case Direct3DRenderStateBlendOperation.D3DBLENDOP_MIN:
                    return 32775;
                case Direct3DRenderStateBlendOperation.D3DBLENDOP_MAX:
                    return 32776;
                case Direct3DRenderStateBlendOperation.INVALID_8009:
                    return 32777;
                case Direct3DRenderStateBlendOperation.D3DBLENDOP_SUBTRACT:
                    return 32778;
                case Direct3DRenderStateBlendOperation.D3DBLENDOP_REVSUBTRACT:
                    return 32779;
                case Direct3DRenderStateBlendOperation.INVALID_F005:
                    return 61445;
                case Direct3DRenderStateBlendOperation.INVALID_F006:
                    return 61446;
                default:
                    return 0;
            }
        }

        public static Direct3DRenderStateBlendMode PsuValueToBlendMode(
          int psuBlendMode)
        {
            switch (psuBlendMode)
            {
                case 0:
                    return Direct3DRenderStateBlendMode.D3DBLEND_ZERO;
                case 1:
                    return Direct3DRenderStateBlendMode.D3DBLEND_ONE;
                case 768:
                    return Direct3DRenderStateBlendMode.D3DBLEND_SRCCOLOR;
                case 769:
                    return Direct3DRenderStateBlendMode.D3DBLEND_INVSRCCOLOR;
                case 770:
                    return Direct3DRenderStateBlendMode.D3DBLEND_SRCALPHA;
                case 771:
                    return Direct3DRenderStateBlendMode.D3DBLEND_INVSRCALPHA;
                case 772:
                    return Direct3DRenderStateBlendMode.D3DBLEND_DESTALPHA;
                case 773:
                    return Direct3DRenderStateBlendMode.D3DBLEND_INVDESTALPHA;
                case 774:
                    return Direct3DRenderStateBlendMode.D3DBLEND_DESTCOLOR;
                case 775:
                    return Direct3DRenderStateBlendMode.D3DBLEND_INVDESTCOLOR;
                case 776:
                    return Direct3DRenderStateBlendMode.D3DBLEND_SRCALPHASAT;
                case 32769:
                    return Direct3DRenderStateBlendMode.D3DBLEND_ONE_8001;
                case 32770:
                    return Direct3DRenderStateBlendMode.D3DBLEND_ONE_8002;
                case 32771:
                    return Direct3DRenderStateBlendMode.D3DBLEND_ONE_8003;
                case 32772:
                    return Direct3DRenderStateBlendMode.D3DBLEND_ONE_8004;
                default:
                    return Direct3DRenderStateBlendMode.D3DBLEND_ZERO;
            }
        }

        public static int ToPsuValue(this Direct3DRenderStateBlendMode blendMode)
        {
            switch (blendMode)
            {
                case Direct3DRenderStateBlendMode.D3DBLEND_ZERO:
                    return 0;
                case Direct3DRenderStateBlendMode.D3DBLEND_ONE:
                    return 1;
                case Direct3DRenderStateBlendMode.D3DBLEND_SRCCOLOR:
                    return 768;
                case Direct3DRenderStateBlendMode.D3DBLEND_INVSRCCOLOR:
                    return 769;
                case Direct3DRenderStateBlendMode.D3DBLEND_SRCALPHA:
                    return 770;
                case Direct3DRenderStateBlendMode.D3DBLEND_INVSRCALPHA:
                    return 771;
                case Direct3DRenderStateBlendMode.D3DBLEND_DESTALPHA:
                    return 772;
                case Direct3DRenderStateBlendMode.D3DBLEND_INVDESTALPHA:
                    return 773;
                case Direct3DRenderStateBlendMode.D3DBLEND_DESTCOLOR:
                    return 774;
                case Direct3DRenderStateBlendMode.D3DBLEND_INVDESTCOLOR:
                    return 775;
                case Direct3DRenderStateBlendMode.D3DBLEND_SRCALPHASAT:
                    return 776;
                case Direct3DRenderStateBlendMode.D3DBLEND_ONE_8001:
                    return 32769;
                case Direct3DRenderStateBlendMode.D3DBLEND_ONE_8002:
                    return 32770;
                case Direct3DRenderStateBlendMode.D3DBLEND_ONE_8003:
                    return 32771;
                case Direct3DRenderStateBlendMode.D3DBLEND_ONE_8004:
                    return 32772;
                default:
                    return 0;
            }
        }
    }
}
