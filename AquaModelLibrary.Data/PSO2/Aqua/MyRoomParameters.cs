using AquaModelLibrary.Helpers.Readers;
using System.IO;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class MyRoomParameters : AquaCommon
    {
        public int mrpType;
        public List<RoomGoodsObject> roomGoodsList = new List<RoomGoodsObject>();
        public List<ChipObject> chipsList = new List<ChipObject>();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "mrp\0"
            };
        }

        public MyRoomParameters() { }

        public MyRoomParameters(byte[] file, int _mrpType)
        {
            mrpType = _mrpType;
            Read(file);
        }
        public MyRoomParameters(BufferedStreamReaderBE<MemoryStream> sr, int _mrpType)
        {
            mrpType = _mrpType;
            Read(sr);
        }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            var count = sr.Read<int>();
            var mainOffset = sr.Read<int>();
            long bookmark;

            sr.Seek(mainOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                switch (mrpType)
                {
                    case 0:
                        RoomGoodsObject rgobj = new RoomGoodsObject();
                        rgobj.goods = sr.Read<RoomGoods>();
                        bookmark = sr.Position;

                        sr.Seek(rgobj.goods.categoryPtr + offset, SeekOrigin.Begin);
                        rgobj.categoryString = sr.ReadCString();
                        sr.Seek(rgobj.goods.allPtr + offset, SeekOrigin.Begin);
                        rgobj.allString = sr.ReadCString();
                        sr.Seek(rgobj.goods.categoryPtr2 + offset, SeekOrigin.Begin);
                        rgobj.categoryString2 = sr.ReadCString();
                        sr.Seek(rgobj.goods.functionStrPtr + offset, SeekOrigin.Begin);
                        rgobj.functionString = sr.ReadCString();
                        sr.Seek(rgobj.goods.motionPtr + offset, SeekOrigin.Begin);
                        rgobj.motionType = sr.ReadCString();
                        sr.Seek(rgobj.goods.unknownPtr + offset, SeekOrigin.Begin);
                        rgobj.unknownString = sr.ReadCString();

                        sr.Seek(bookmark, SeekOrigin.Begin);
                        roomGoodsList.Add(rgobj);
                        break;
                    case 1:
                        ChipObject chipobj = new ChipObject();
                        chipobj.chip = sr.Read<Chip>();
                        bookmark = sr.Position;

                        sr.Seek(chipobj.chip.objectPtr + offset, SeekOrigin.Begin);
                        chipobj.objectString = sr.ReadCString();
                        sr.Seek(chipobj.chip.collisionTypePtr + offset, SeekOrigin.Begin);
                        chipobj.collisionTypeString = sr.ReadCString();
                        sr.Seek(chipobj.chip.unkStringPtr + offset, SeekOrigin.Begin);
                        chipobj.unknownString = sr.ReadCString();

                        sr.Seek(bookmark, SeekOrigin.Begin);
                        chipsList.Add(chipobj);
                        break;
                }
            }
        }

        public class RoomGoodsObject
        {
            public RoomGoods goods;
            public string categoryString = null;
            public string allString = null;
            public string categoryString2 = null;
            public string functionString = null;
            public string motionType = null;
            public string unknownString = null;
        }

        public struct RoomGoods
        {
            /// <summary>
            /// Id might be 1 less than actual object id
            /// </summary>
            public int id;
            public int categoryPtr;
            public int int_08;
            public byte bt_0C;
            public byte bt_0D;
            public byte bt_0E;
            public byte bt_0F;

            public int int_10;
            public int allPtr;
            public int categoryPtr2;
            public int functionStrPtr;

            public int motionPtr;
            public int int_24;
            public int unknownPtr;
        }

        public class ChipObject
        {
            public Chip chip;
            public string objectString = null;
            public string unknownString = null;
            public string collisionTypeString = null;
        }

        public struct Chip
        {
            public int objectPtr;
            public int int_04;
            public int unkStringPtr;
            public int collisionTypePtr;

            //Collision shape params?
            public float flt_10;
            public float flt_14;
            public float flt_18;
        }
    }
}
