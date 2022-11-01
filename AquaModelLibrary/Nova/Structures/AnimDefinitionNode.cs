using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary.Nova.Structures
{
    public class AnimDefinitionNode
    {
        public ushort header0;
        public ushort dataCount;
        public int len;
        public PSO2String nameData;
        public string name;
        public List<DataClump> data = new List<DataClump>();
    }

    public class DataClump
    {
        public string dcString = null;
        public DataClumpStart dcStart;
        public object dc;
        /*
        public DataClump14 d14;
        public DataClump24 d24;
        public DataClump30 d30;
        public DataClump34 d34;
        public DataClump44 d44;*/
    }

    public struct DataClumpStart
    {
        public ushort dc0;
        public ushort dcType;
    }

    public struct DataClump14
    {
        public byte bt_4;
        public byte bt_5;
        public byte bt_6;
        public byte bt_7;
        public int int_08;
        public byte bt_0C;
        public byte bt_0D;
        public byte bt_0E;
        public byte bt_0F;

        public byte bt_10;
        public byte bt_11;
        public byte bt_12;
        public byte bt_13;
    }

    public struct DataClump24
    {
        public byte bt_4;
        public byte bt_5;
        public byte bt_6;
        public byte bt_7;
        public int int_08;
        public byte bt_0C;
        public byte bt_0D;
        public byte bt_0E;
        public byte bt_0F;

        public byte bt_10;
        public byte bt_11;
        public byte bt_12;
        public byte bt_13;

        public int int_00;
        public int int_04;
        public float endFrameTIme;
        public int int_0C;
    }

    public struct DataClump30
    {
        public ushort sht_04;
        public ushort sht_06;
        public int int_08;
        public ushort sht_0C;
        public ushort sht_0E;

        public byte bt_10;
        public byte bt_11;
        public byte bt_12;
        public byte bt_13;
        public float flt_14;
        public float flt_18;
        public float flt_1C;

        public float flt_20;
        public int int_24;
        public byte bt_25;
        public byte bt_26;
        public byte bt_27;
        public byte bt_28;
        public ushort sht_2C;
        public ushort sht_2E;
    }

    public struct DataClump34
    {
        public byte bt_04;
        public byte bt_05;
        public byte bt_06;
        public byte bt_07;
        public int int_08;

        public PSO2String clumpName;

        public ushort sht_2C;
        public ushort sht_2E;
        public byte bt_30;
        public byte bt_31;
        public byte bt_32;
        public byte bt_33;
    }

    public struct DataClump44
    {
        public byte bt_04;
        public byte bt_05;
        public byte bt_06;
        public byte bt_07;
        public int int_08;

        public PSO2String clumpName;

        public ushort sht_2C;
        public ushort sht_2E;
        public byte bt_30;
        public byte bt_31;
        public byte bt_32;
        public byte bt_33;
        public int int_34;
        public int int_38;
        public int int_3C;
        public int int_40;
    }
}
