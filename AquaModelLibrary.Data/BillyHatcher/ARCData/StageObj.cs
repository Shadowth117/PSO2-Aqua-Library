using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// File containing info on 
    /// </summary>
    public class StageObj : ARC
    {
        public List<StageObjEntry> objEntries = new List<StageObjEntry>();
        public StageObj() { }
        public StageObj(byte[] file)
        {
            Read(file);
        }

        public StageObj(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }
        public override void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public override void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var magic = sr.ReadUTF8String(0, 4);
            sr._BEReadActive = true;

            if(magic != "GEOS")
            {
                base.Read(sr);
                sr.Seek(0x20, SeekOrigin.Begin);
            } else
            {
                sr.Seek(0x8, SeekOrigin.Begin);
            }

            int offsetsOffset = sr.ReadBE<int>();
            int offsetsCount = sr.ReadBE<int>();
            sr.Seek(0x20 + offsetsOffset, SeekOrigin.Begin);
            List<int> offsets = new List<int>();
            for(int i = 0; i < offsetsCount; i++)
            {
                offsets.Add(sr.ReadBE<int>());
            }
            for(int i = 0; i < offsetsCount; i++)
            {
                sr.Seek(offsets[i] + 0x20, SeekOrigin.Begin);
                StageObjEntry objEntry = new StageObjEntry();
                objEntry.flt_00 = sr.ReadBE<float>();
                objEntry.model2Id0 = sr.ReadBE<ushort>();
                objEntry.model2Id1 = sr.ReadBE<ushort>();
                objEntry.usht_08 = sr.ReadBE<ushort>();
                objEntry.usht_0A = sr.ReadBE<ushort>();
                objEntry.int_0C = sr.ReadBE<int>();
                objEntry.int_10 = sr.ReadBE<int>();
                objEntry.int_14 = sr.ReadBE<int>();
                objEntry.dataPtr_18 = sr.ReadBE<int>();
                objEntry.dataPtr_1C = sr.ReadBE<int>();
                objEntry.dataPtr_20 = sr.ReadBE<int>();
                objEntry.objNameStrPtr = sr.ReadBE<int>();
                objEntry.int_28 = sr.ReadBE<int>();
                objEntry.int_2C = sr.ReadBE<int>();
                objEntry.dataPtr_30 = sr.ReadBE<int>();
                objEntry.fltPtr_34 = sr.ReadBE<int>();

                var bookmark = sr.Position;
                if (objEntry.int_28 != -1)
                {
                    throw new NotImplementedException();
                }
                if (objEntry.int_2C != -1)
                {
                    throw new NotImplementedException();
                }

                if (objEntry.dataPtr_18 != -1)
                {
                    sr.Seek(0x20 + objEntry.dataPtr_18, SeekOrigin.Begin);
                    var ptr_18 = sr.ReadBE<int>();
                    if(ptr_18 != -1)
                    {
                        sr.Seek(0x20 + ptr_18, SeekOrigin.Begin);
                        objEntry.floatField = new StageObjEntry_18()
                        {
                            flt_00 = sr.ReadBE<float>(),
                            flt_04 = sr.ReadBE<float>(),
                            flt_08 = sr.ReadBE<float>(),
                            flt_0C = sr.ReadBE<float>(),

                            flt_10 = sr.ReadBE<float>(),
                            flt_14 = sr.ReadBE<float>(),
                            flt_18 = sr.ReadBE<float>()
                        };
                    }
                }

                if (objEntry.dataPtr_1C != -1)
                {
                    objEntry.struct_1C = new StageObjEntry_1C()
                    {
                        int_00 = sr.ReadBE<int>(),
                        int_04 = sr.ReadBE<int>(),
                        flt_08 = sr.ReadBE<float>(),
                        int_0C = sr.ReadBE<int>(),
                        flt_10 = sr.ReadBE<float>(),
                        flt_14 = sr.ReadBE<float>()
                    };
                }

                if (objEntry.dataPtr_20 != -1)
                {
                    sr.Seek(0x20 + objEntry.dataPtr_20, SeekOrigin.Begin);
                    objEntry.struct_20 = new StageObjEntry_20()
                    {
                        int_00 = sr.ReadBE<int>(),
                        int_04 = sr.ReadBE<int>()
                    };
                }

                if (objEntry.dataPtr_30 != -1)
                {
                    sr.Seek(0x20 + objEntry.dataPtr_30, SeekOrigin.Begin);
                    objEntry.struct_30 = new StageObjEntry_30()
                    {
                        int_00 = sr.ReadBE<int>(),
                        int_04 = sr.ReadBE<int>()
                    };
                }

                if (objEntry.fltPtr_34 != -1)
                {
                    sr.Seek(0x20 + objEntry.fltPtr_34, SeekOrigin.Begin);
                    objEntry.pointedflt_34 = sr.ReadBE<float>();
                }

                if (objEntry.objNameStrPtr != -1)
                {
                    sr.Seek(0x20 + objEntry.objNameStrPtr, SeekOrigin.Begin);
                    objEntry.objName = sr.ReadCString();
                }

                sr.Seek(bookmark, SeekOrigin.Begin);
                if(i + 1 != offsetsCount)
                {
                    sr.Seek(0x8, SeekOrigin.Current);
                }

                objEntries.Add(objEntry);
            }
        }

        public class StageObjEntry
        {
            public string objName;
            public StageObjEntry_18? floatField = null;
            public StageObjEntry_1C? struct_1C = null;
            public StageObjEntry_20? struct_20 = null;
            public StageObjEntry_30? struct_30 = null;
            public float? pointedflt_34 = null;

            /// <summary>
            /// Always 500?
            /// </summary>
            public float flt_00;
            public ushort model2Id0;
            public ushort model2Id1;
            public ushort usht_08;
            public ushort usht_0A;
            public int int_0C;

            public int int_10;
            public int int_14;
            public int dataPtr_18;
            public int dataPtr_1C;

            public int dataPtr_20;
            public int objNameStrPtr;
            /// <summary>
            /// Always -1?
            /// </summary>
            public int int_28;
            /// <summary>
            /// Always -1?
            /// </summary>
            public int int_2C;

            public int dataPtr_30;
            /// <summary>
            /// Always -1?
            /// </summary>
            public int fltPtr_34;

            //From here, the struct is padded to 0x10 except for the final structure.
        }

        /// <summary>
        /// Float field 0x1C long?
        /// </summary>
        public struct StageObjEntry_18
        {
            public float flt_00;
            public float flt_04;
            public float flt_08;
            public float flt_0C;

            public float flt_10;
            public float flt_14;
            public float flt_18;
        }

        public struct StageObjEntry_1C
        {
            public int int_00;
            public int int_04;
            public float flt_08;
            public int int_0C;

            public float flt_10;
            public float flt_14;
        }

        public struct StageObjEntry_20
        {
            public int int_00;
            public int int_04;
        }

        public struct StageObjEntry_30
        {
            public int int_00;
            public int int_04;
        }
    }
}
