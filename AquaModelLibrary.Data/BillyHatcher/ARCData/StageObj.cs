using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// File containing info on scenery/breakable objects
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

            if(magic != "GEOS") //GEOS is only used in unused files from the PC version
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
                objEntry.cameraCullingDistance = sr.ReadBE<float>();
                objEntry.model2Id0 = sr.ReadBE<ushort>();
                objEntry.model2Id1 = sr.ReadBE<ushort>();
                objEntry.collision2ModelId0 = sr.ReadBE<ushort>();
                objEntry.collision2ModelId1 = sr.ReadBE<ushort>();
                objEntry.collisionSoundId = sr.ReadBE<int>();
                objEntry.objectFlags = sr.ReadBE<int>();
                objEntry.int_14 = sr.ReadBE<int>();
                objEntry.collisionInfoPtr = sr.ReadBE<int>();
                objEntry.breakInfoPtr = sr.ReadBE<int>();
                objEntry.ptclInfoPtr = sr.ReadBE<int>();
                objEntry.objNameStrPtr = sr.ReadBE<int>();
                objEntry.paInfoAPtr = sr.ReadBE<int>();
                objEntry.paInfoBPtr = sr.ReadBE<int>();
                objEntry.damageInfoPtr = sr.ReadBE<int>();
                objEntry.cBreakInfoFloatPtr = sr.ReadBE<int>();

                var bookmark = sr.Position;
                if (objEntry.paInfoAPtr > 0)
                {
                    throw new NotImplementedException();
                }
                if (objEntry.paInfoBPtr > 0)
                {
                    throw new NotImplementedException();
                }

                if (objEntry.collisionInfoPtr > 0)
                {
                    sr.Seek(0x20 + objEntry.collisionInfoPtr, SeekOrigin.Begin);
                    var ptr_18 = sr.ReadBE<int>();
                    if(ptr_18 > 0)
                    {
                        sr.Seek(0x20 + ptr_18, SeekOrigin.Begin);
                        objEntry.collisionInfo = new CollisionInfo()
                        {
                            collisionShapeType = sr.ReadBE<int>(),
                            flt_04 = sr.ReadBE<float>(),
                            flt_08 = sr.ReadBE<float>(),
                            flt_0C = sr.ReadBE<float>(),

                            flt_10 = sr.ReadBE<float>(),
                            flt_14 = sr.ReadBE<float>(),
                            flt_18 = sr.ReadBE<float>()
                        };
                    }
                }

                if (objEntry.breakInfoPtr > 0)
                {
                    sr.Seek(0x20 + objEntry.breakInfoPtr, SeekOrigin.Begin);
                    objEntry.breakInfo = new BreakInfo()
                    {
                        useStageSpecificPool = sr.ReadBE<int>(),
                        startingPiece = sr.ReadBE<int>(),
                        piecePoolCount = sr.ReadBE<int>(),
                        pieceCount = sr.ReadBE<int>(),
                        pieceScale = sr.ReadBE<float>(),
                        pieceLaunchForce = sr.ReadBE<float>()
                    };
                }

                if (objEntry.ptclInfoPtr > 0)
                {
                    sr.Seek(0x20 + objEntry.ptclInfoPtr, SeekOrigin.Begin);
                    objEntry.ptclInfo = new ParticleInfo()
                    {
                        int_00 = sr.ReadBE<int>(),
                        int_04 = sr.ReadBE<int>()
                    };
                }

                if (objEntry.damageInfoPtr > 0)
                {
                    sr.Seek(0x20 + objEntry.damageInfoPtr, SeekOrigin.Begin);
                    objEntry.damageInfo = new DamageInfo()
                    {
                        damage = sr.ReadBE<int>(),
                        int_04 = sr.ReadBE<int>()
                    };
                }

                if (objEntry.cBreakInfoFloatPtr > 0)
                {
                    sr.Seek(0x20 + objEntry.cBreakInfoFloatPtr, SeekOrigin.Begin);
                    objEntry.cBreakInfoFloat = sr.ReadBE<float>();
                }

                if (objEntry.objNameStrPtr > 0)
                {
                    sr.Seek(0x20 + objEntry.objNameStrPtr, SeekOrigin.Begin);
                    objEntry.objName = sr.ReadCString();
                }

                //The final object doesn't align to 0x10
                sr.Seek(bookmark, SeekOrigin.Begin);
                if(i + 1 != offsetsCount)
                {
                    sr.Seek(0x8, SeekOrigin.Current);
                }

                objEntries.Add(objEntry);
            }
        }

        public byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<int> pofSets = new List<int>();

            Dictionary<string, int> group2StructureOffsets = new Dictionary<string, int>();
            List<byte> outBytes = new List<byte>();

            pofSets.Add(outBytes.Count);
            outBytes.AddValue((int)8);
            outBytes.AddValue((int)objEntries.Count);

            for(int i = 0; i < objEntries.Count; i++)
            {
                var obj = objEntries[i];

                //Write obj address data
                pofSets.Add(outBytes.Count);
                outBytes.ReserveInt($"objEntry{i}");
            }
            outBytes.AlignWriter(0x10);

            //Get null info list
            int stageObjOffset = outBytes.Count;
            for (int i = 0; i < objEntries.Count; i++)
            {
                var baseOffset = stageObjOffset + 0x40 * i;
                var obj = objEntries[i];

                if (obj.breakInfo == null)
                {
                    group2StructureOffsets.Add($"brkinfo{i}", baseOffset + 0x1C);
                }
                if (obj.ptclInfo == null)
                {
                    group2StructureOffsets.Add($"ptclinfo{i}", baseOffset + 0x20);
                }
                if (obj.paInfoAPtr == -1)
                {
                    group2StructureOffsets.Add($"painfo_a{i}", baseOffset + 0x28);
                }
                if (obj.paInfoBPtr == -1)
                {
                    group2StructureOffsets.Add($"painfo_b{i}", baseOffset + 0x2C);
                }
                if (obj.damageInfo == null)
                {
                    group2StructureOffsets.Add($"damageinfo{i}", baseOffset + 0x30);
                }
                if (obj.cBreakInfoFloat == null)
                {
                    group2StructureOffsets.Add($"cbrkinfo{i}", baseOffset + 0x34);
                }
            }

            //Write obj entries
            for (int i = 0; i < objEntries.Count; i++)
            {
                var obj = objEntries[i];

                if(obj.collisionInfo != null)
                {
                    pofSets.Add(outBytes.Count + 0x18);
                }
                if(obj.breakInfo != null)
                {
                    pofSets.Add(outBytes.Count + 0x1C);
                }
                if (obj.ptclInfo != null)
                {
                    pofSets.Add(outBytes.Count + 0x20);
                }
                if (obj.objName != null)
                {
                    pofSets.Add(outBytes.Count + 0x24);
                }
                if (obj.damageInfo != null)
                {
                    pofSets.Add(outBytes.Count + 0x30);
                }
                if (obj.cBreakInfoFloat != null)
                {
                    pofSets.Add(outBytes.Count + 0x34);
                }

                //Write obj address data
                outBytes.FillInt($"objEntry{i}", outBytes.Count);
                outBytes.AddValue(obj.cameraCullingDistance);
                outBytes.AddValue(obj.model2Id0);
                outBytes.AddValue(obj.model2Id1);
                outBytes.AddValue(obj.collision2ModelId0);
                outBytes.AddValue(obj.collision2ModelId1);
                outBytes.AddValue(obj.collisionSoundId);

                outBytes.AddValue(obj.objectFlags);
                outBytes.AddValue(obj.int_14);
                outBytes.ReserveInt($"data18{i}");
                outBytes.ReserveInt($"breakinfo{i}");

                outBytes.ReserveInt($"ptclinfo{i}");
                outBytes.ReserveInt($"objName{i}");
                outBytes.AddValue((int)-1);
                outBytes.AddValue((int)-1);

                outBytes.ReserveInt($"damageinfo{i}");
                outBytes.ReserveInt($"cbreakinfo{i}");

                //Pad to 0x10 except for the final entry
                if (i + 1 < objEntries.Count)
                {
                    outBytes.AddValue((int)0);
                    outBytes.AddValue((int)0);
                }
            }

            //Write FloatField _18 pointers
            List<int> floatFieldDupeMapping = new List<int>();
            List<int> floatFieldDupePtrs = new List<int>();

            //For whatever reason, if the previous objEntry's int_14 is not 0, recycling it will cause behavior issues.
            int prev_14 = 0;
            for(int i = 0; i < objEntries.Count; i++)
            {
                if (objEntries[i] != null && objEntries[i].collisionInfo != null)
                {
                    var obj = objEntries[i].collisionInfo.GetValueOrDefault();

                    int ffMapping = i;
                    if(prev_14 == 0 && i != 0)
                    {
                        int prevIndex = i - 1;
                        if (objEntries[prevIndex] != null && objEntries[prevIndex].collisionInfo != null)
                        {
                            var refObj = objEntries[prevIndex].collisionInfo.GetValueOrDefault();
                            //Check for duplicate
                            if (obj.collisionShapeType == refObj.collisionShapeType && obj.flt_04 == refObj.flt_04 && obj.flt_08 == refObj.flt_08 && obj.flt_0C == refObj.flt_0C
                                && obj.flt_10 == refObj.flt_10 && obj.flt_14 == refObj.flt_14 && obj.flt_18 == refObj.flt_18)
                            {
                                ffMapping = floatFieldDupeMapping[prevIndex];
                            }
                        }
                    }
                    floatFieldDupeMapping.Add(ffMapping);
                    floatFieldDupePtrs.Add(outBytes.Count);

                    if(ffMapping < i)
                    {
                        outBytes.FillInt($"data18{i}", floatFieldDupePtrs[ffMapping]);
                    } else
                    {
                        outBytes.FillInt($"data18{i}", outBytes.Count);
                        pofSets.Add(outBytes.Count);
                        outBytes.ReserveInt($"data18ptr{i}");
                    }

                    prev_14 = objEntries[i].int_14;
                }
            }

            //Write breakinfo data
            for (int i = 0; i < objEntries.Count; i++)
            {
                if (objEntries[i] != null && objEntries[i].breakInfo != null)
                {
                    outBytes.FillInt($"breakinfo{i}", outBytes.Count);
                    var breakInfo = objEntries[i].breakInfo.GetValueOrDefault();
                    outBytes.AddValue(breakInfo.useStageSpecificPool);
                    outBytes.AddValue(breakInfo.startingPiece);
                    outBytes.AddValue(breakInfo.piecePoolCount);
                    outBytes.AddValue(breakInfo.pieceCount);

                    outBytes.AddValue(breakInfo.pieceScale);
                    outBytes.AddValue(breakInfo.pieceLaunchForce);
                }
                else
                {
                    outBytes.FillInt($"breakinfo{i}", -1);
                }
            }

            //Write FloatField _18 data, unlike the rest of these, all objEntries must have this
            for (int i = 0; i < objEntries.Count; i++)
            {
                if (objEntries[i] != null && objEntries[i].collisionInfo != null)
                {
                    if (floatFieldDupeMapping[i] == i)
                    {
                        outBytes.FillInt($"data18ptr{i}", outBytes.Count);
                        var floatField = objEntries[i].collisionInfo.GetValueOrDefault();
                        outBytes.AddValue(floatField.collisionShapeType);
                        outBytes.AddValue(floatField.flt_04);
                        outBytes.AddValue(floatField.flt_08);
                        outBytes.AddValue(floatField.flt_0C);

                        outBytes.AddValue(floatField.flt_10);
                        outBytes.AddValue(floatField.flt_14);
                        outBytes.AddValue(floatField.flt_18);
                    }
                } else
                {
                    outBytes.FillInt($"data18ptr{i}", -1);
                }
            }

            //Write ptclinfo data
            for (int i = 0; i < objEntries.Count; i++)
            {
                if (objEntries[i] != null && objEntries[i].ptclInfo != null)
                {
                    outBytes.FillInt($"ptclinfo{i}", outBytes.Count);
                    var ptclInfo = objEntries[i].ptclInfo.GetValueOrDefault();
                    outBytes.AddValue(ptclInfo.int_00);
                    outBytes.AddValue(ptclInfo.int_04);
                } else
                {
                    outBytes.FillInt($"ptclinfo{i}", -1);
                }
            }

            //Write damageinfo data
            for (int i = 0; i < objEntries.Count; i++)
            {
                if (objEntries[i] != null && objEntries[i].damageInfo != null)
                {
                    outBytes.FillInt($"damageinfo{i}", outBytes.Count);
                    var damageInfo = objEntries[i].damageInfo.GetValueOrDefault();
                    outBytes.AddValue(damageInfo.damage);
                    outBytes.AddValue(damageInfo.int_04);
                }
                else
                {
                    outBytes.FillInt($"damageinfo{i}", -1);
                }
            }

            //Write cbreakinfo data
            for (int i = 0; i < objEntries.Count; i++)
            {
                if (objEntries[i] != null && objEntries[i].cBreakInfoFloat != null)
                {
                    outBytes.FillInt($"cbreakinfo{i}", outBytes.Count);
                    var flt = objEntries[i].cBreakInfoFloat.GetValueOrDefault();
                    outBytes.AddValue(flt);
                }
                else
                {
                    outBytes.FillInt($"cbreakinfo{i}", -1);
                }
            }

            //Write object name data
            for (int i = 0; i < objEntries.Count; i++)
            {
                if (objEntries[i] != null && objEntries[i].objName != null)
                {
                    outBytes.FillInt($"objName{i}", outBytes.Count);
                    var str = objEntries[i].objName;
                    outBytes.AddValue(Encoding.ASCII.GetBytes(str));
                    outBytes.Add(0);
                }
                else
                {
                    outBytes.FillInt($"objName{i}", -1);
                }
            }

            //Add POF0, insert header
            outBytes.AlignWriter(0x4);
            var pof0Offset = outBytes.Count;
            pofSets.Sort();
            var pof0 = POF0.GenerateRawPOF0(pofSets, true);
            outBytes.AddRange(pof0);

            //Structure references
            var keysInitial = group2StructureOffsets.Keys.ToList();
            keysInitial.Sort();
            foreach (var key in keysInitial)
            {
                outBytes.AddValue((int)group2StructureOffsets[key]);
                outBytes.ReserveInt(key);
            }

            //Strings
            var keys = group2StructureOffsets.Keys.ToList(); //For some reason these aren't sorted alphabetically, but the previous list references them alphabetically
            var stringStart = outBytes.Count;
            foreach (var key in keys)
            {
                outBytes.FillInt(key, outBytes.Count - stringStart);
                outBytes.AddRange(Encoding.ASCII.GetBytes(key));
                outBytes.Add(0);
            }

            var arcBytes = new List<byte>();
            arcBytes.AddValue(outBytes.Count + 0x20);
            arcBytes.AddValue(pof0Offset);
            arcBytes.AddValue(pof0.Length);
            arcBytes.AddValue(0); //No group 1 structures here

            arcBytes.AddValue(group2StructureOffsets.Count);
            arcBytes.Add(0x30);
            arcBytes.Add(0x31);
            arcBytes.Add(0x30);
            arcBytes.Add(0x30);
            arcBytes.AddValue(0);
            arcBytes.AddValue(0);

            outBytes.InsertRange(0, arcBytes);

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }

        public class StageObjEntry
        {
            public string objName;
            public CollisionInfo? collisionInfo = null;
            public BreakInfo? breakInfo = null;
            public ParticleInfo? ptclInfo = null;
            public DamageInfo? damageInfo = null;

            public float? cBreakInfoFloat = null;

            public float cameraCullingDistance;
            public ushort model2Id0;
            public ushort model2Id1;
            /// <summary>
            /// Optional explicit collision model setting.
            /// Usually only Id1 is used, but some things use Id0.
            /// </summary>
            public ushort collision2ModelId0;
            /// <summary>
            /// Optional explicit collision model setting.
            /// Usually only Id1 is used, but some things use Id0.
            /// </summary>
            public ushort collision2ModelId1;
            /// <summary>
            /// Sound id of the sound that plays when the object is touched.
            /// 0-2 are the normal breaking sounds. 3 is the bomb sound, 4 is silent, 5 is a grass rustle. 6+ is a crash.
            /// </summary>
            public int collisionSoundId;

            public int objectFlags;
            /// <summary>
            /// Unknown value, but if it's greater than 0, other objects may get errors sharing its same collisionInfo struct
            /// </summary>
            public int int_14;
            /// <summary>
            /// Collision info, position 0x18. Name is a guess since there's no reference for this one.
            /// </summary>
            public int collisionInfoPtr = -1;
            /// <summary>
            /// Break Info, position 0x1C. Internal name based on file references
            /// </summary>
            public int breakInfoPtr = -1;

            /// <summary>
            /// Particle Info, position 0x20. Internal name based on file references
            /// </summary>
            public int ptclInfoPtr = -1;
            /// <summary>
            /// The object's name, position 0x24.
            /// </summary>
            public int objNameStrPtr = -1;
            /// <summary>
            /// PolyAnim data? Position 0x28. Always -1 in final so seemingly unused. Internal name based on file references
            /// </summary>
            public int paInfoAPtr = -1;
            /// <summary>
            /// PolyAnim data? Position 0x2C. Always -1 in final so seemingly unused. Internal name based on file references
            /// </summary>
            public int paInfoBPtr = -1;

            /// <summary>
            /// Damage Info. Position 0x30. Internal name based on file references
            /// </summary>
            public int damageInfoPtr = -1;
            /// <summary>
            /// C Break Info? Position 0x34. Internal name based on file references
            /// </summary>
            public int cBreakInfoFloatPtr = -1;

            //From here, the struct is padded to 0x10 except for the final structure.
        }

        /// <summary>
        /// Float field 0x1C long?
        /// </summary>
        public struct CollisionInfo
        {
            /// <summary>
            /// 2 - Cylinder
            /// </summary>
            public int collisionShapeType;
            /// <summary>
            /// 2 - X Position Offset
            /// </summary>
            public float flt_04;
            /// <summary>
            /// 2 - Height 1
            /// </summary>
            public float flt_08;
            /// <summary>
            /// 2 - Z (Horizontal) Position Offset
            /// </summary>
            public float flt_0C;

            /// <summary>
            /// 2 - Radius
            /// </summary>
            public float flt_10;
            /// <summary>
            /// 2 - Height 2
            /// </summary>
            public float flt_14;
            /// <summary>
            /// 2 - Unused?
            /// </summary>
            public float flt_18;
        }

        public struct BreakInfo
        {
            /// <summary>
            /// Flag for if the pieces pull from the common models or the stage specific models. 
            /// If anything other than 0, pulls from stage specific models 
            /// Setting this to 1 in the common stageobj will ALSO pull from the stage specific models!
            /// </summary>
            public int useStageSpecificPool;
            /// <summary>
            /// The first breakable piece selectable
            /// </summary>
            public int startingPiece;
            /// <summary>
            /// The amount of models past the starting piece to include in the random breakable piece pool for this object
            /// </summary>
            public int piecePoolCount;
            /// <summary>
            /// The amount of pieces that will break from the object
            /// </summary>
            public int pieceCount;
            /// <summary>
            /// The size of the pieces
            /// </summary>
            public float pieceScale;
            /// <summary>
            /// How much force is applied to pieces when they explode from the object
            /// </summary>
            public float pieceLaunchForce;
        }

        public struct ParticleInfo
        {
            public int int_00;
            public int int_04;
        }

        public struct DamageInfo
        {
            /// <summary>
            /// Damage done to the player and eggs
            /// </summary>
            public int damage;
            public int int_04;
        }
    }
}
