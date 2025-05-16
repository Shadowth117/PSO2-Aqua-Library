using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    //Billy Hatcher ani_model_*.arc files
    //hari - Richie. Anim 0 is hatch, anim 1 is idle, anim 2 is run, anim 3 is attack, anim 4 is jump (over obstacles), anim 5 is befriending
    public class AniModel : ARC
    {
        public List<NJSObject> models = new List<NJSObject>();
        public List<NJSMotion> motions = new List<NJSMotion>();
        public List<float> unkFloatList = new List<float>();
        public List<CameraAnchor> cameraAnchorList = new List<CameraAnchor>();
        public List<NJSMotion> motion2s = new List<NJSMotion>();
        public NJTextureList texList = new NJTextureList();
        public PuyoFile gvm = null;
        public PolyAnim polyAnim = null;

        public AniModel() { }

        public AniModel(byte[] file)
        {
            Read(file);
        }

        public AniModel(BufferedStreamReaderBE<MemoryStream> sr)
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
            base.Read(sr);
            int nodeCount = 0;
            for (int i = 0; i < group1FileNames.Count; i++)
            {
                var fileName = group1FileNames[i];
                sr.Seek(0x20 + group1FileReferences[i].fileOffset, SeekOrigin.Begin);
                switch (fileName)
                {
                    case "AniModel":
                        var modelAddress = sr.ReadBE<int>();
                        var animAddress = sr.ReadBE<int>();
                        var boundingAddress = sr.ReadBE<int>();
                        var anim2Address = sr.ReadBE<int>();

                        var unkFloatAddress = sr.ReadBE<int>();
                        var texListAddress = sr.ReadBE<int>();
                        var gvmAddress = sr.ReadBE<int>();

                        //Read models
                        var modelCount = (animAddress - modelAddress) / 4;
                        List<int> modelAddresses = new List<int>();
                        sr.Seek(0x20 + modelAddress, SeekOrigin.Begin);
                        for(int j = 0; j < modelCount; j++)
                        {
                            modelAddresses.Add(sr.ReadBE<int>());
                        }
                        foreach(var modelAddr in modelAddresses)
                        {
                            sr.Seek(0x20 + modelAddr, SeekOrigin.Begin);
                            models.Add(new NJSObject(sr, NinjaVariant.Ginja, sr._BEReadActive, 0x20));
                        }

                        //Read Animations
                        var animCount = (boundingAddress - animAddress) / 4;
                        List<int> animAddresses = new List<int>();
                        sr.Seek(0x20 + animAddress, SeekOrigin.Begin);
                        for (int j = 0; j < animCount; j++)
                        {
                            animAddresses.Add(sr.ReadBE<int>());
                        }
                        foreach (var animAddr in animAddresses)
                        {
                            if(animAddr != -1)
                            {
                                sr.Seek(0x20 + animAddr, SeekOrigin.Begin);
                                motions.Add(new NJSMotion(sr, true, 0x20));
                            } else
                            {
                                motions.Add(null);
                            }
                        }

                        //Read Bounding
                        var boundCount = (anim2Address - boundingAddress) / 4;
                        List<int> boundAddresses = new List<int>();
                        sr.Seek(0x20 + boundingAddress, SeekOrigin.Begin);
                        for (int j = 0; j < boundCount; j++)
                        {
                            boundAddresses.Add(sr.ReadBE<int>());
                        }
                        foreach (var boundAddr in boundAddresses)
                        {
                            sr.Seek(0x20 + boundAddr, SeekOrigin.Begin);
                            cameraAnchorList.Add(new CameraAnchor() { Min = sr.ReadBEV3(), Max = sr.ReadBEV3(), int_18 = sr.ReadBE<int>(), int_1C = sr.ReadBE<int>()});
                        }

                        //Read Animation 2s
                        var nextAddress = unkFloatAddress > 0 ? unkFloatAddress : texListAddress;
                        var anim2Count = (nextAddress - anim2Address) / 4;
                        List<int> anim2Addresses = new List<int>();
                        sr.Seek(0x20 + anim2Address, SeekOrigin.Begin);
                        for (int j = 0; j < anim2Count; j++)
                        {
                            anim2Addresses.Add(sr.ReadBE<int>());
                        }
                        foreach (var animAddr in anim2Addresses)
                        {
                            sr.Seek(0x20 + animAddr, SeekOrigin.Begin);
                            motion2s.Add(new NJSMotion(sr, true, 0x20));
                        }

                        //Read Weird Floats
                        if(unkFloatAddress != 0)
                        {
                            var floatCount = (texListAddress - unkFloatAddress) / 4;
                            List<int> floatAddresses = new List<int>();
                            sr.Seek(0x20 + unkFloatAddress, SeekOrigin.Begin);
                            for (int j = 0; j < floatCount; j++)
                            {
                                floatAddresses.Add(sr.ReadBE<int>());
                            }
                            foreach (var floatAddr in floatAddresses)
                            {
                                sr.Seek(0x20 + floatAddr, SeekOrigin.Begin);
                                unkFloatList.Add(sr.ReadBE<float>());
                            }
                        }

                        //Read Texture data
                        sr.Seek(0x20 + texListAddress, SeekOrigin.Begin);
                        sr.Seek(0x20 + sr.ReadBE<int>(), SeekOrigin.Begin);
                        texList = new NJTextureList(sr, 0x20);

                        sr.Seek(0x20 + gvmAddress, SeekOrigin.Begin);
                        gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
                        break;
                    case "polyanim":
                        polyAnim = new PolyAnim(sr, 0x20);
                        break;
                    default:
                        Debug.WriteLine($"Ignoring {fileName}");
                        break;
                }
            }
        }

        public byte[] GetBytes(bool crabSharkOrder = false)
        {
            ByteListExtension.AddAsBigEndian = true;
            //AniModel, polyanim. Polyanim can be null and if so should not be inserted at all
            Dictionary<string, int> group1StructureOffsets = new Dictionary<string, int>();
            group1StructureOffsets.Add("AniModel", 0);
            //If any motions are null, they should be inserted as motion{id} etc with the offset linking to the -1 null offset value
            Dictionary<string, int> group2StructureOffsets = new Dictionary<string, int>();
            List<byte> outBytes = new List<byte>();
            List<int> pofSets = new List<int>();

            //AniModel
            if (models.Count > 0)
            {
                pofSets.Add(0x0);
            }
            if (motions.Count > 0)
            {
                pofSets.Add(0x4);
            }
            if (cameraAnchorList.Count > 0)
            {
                pofSets.Add(0x8);
            }
            if (motion2s.Count > 0)
            {
                pofSets.Add(0xC);
            }

            if (unkFloatList.Count > 0)
            {
                pofSets.Add(0x10);
            }
            if (texList != null)
            {
                pofSets.Add(0x14);
            }
            if (gvm != null)
            {
                pofSets.Add(0x18);
            }

            outBytes.ReserveInt("ModelListOffset");
            outBytes.ReserveInt("AnimListOffset");
            outBytes.ReserveInt("BoundingListOffset");
            outBytes.ReserveInt("Anim2ListOffset");

            outBytes.ReserveInt("UnkFloatListOffset");
            outBytes.ReserveInt("TexListOffset");
            outBytes.ReserveInt("TexOffset");

            outBytes.FillInt("ModelListOffset", outBytes.Count);
            for(int i = 0; i < models.Count; i++)
            {
                pofSets.Add(outBytes.Count);
                outBytes.ReserveInt($"Model{i}");
            }
            outBytes.FillInt("AnimListOffset", outBytes.Count);
            for (int i = 0; i < motions.Count; i++)
            {
                if (motions[i] != null)
                {
                    pofSets.Add(outBytes.Count);
                    outBytes.ReserveInt($"Anim{i}");
                }
                else
                {
                    group2StructureOffsets[$"motion{i}"] = outBytes.Count;
                    outBytes.AddValue((int)-1);
                }
            }
            if(cameraAnchorList.Count > 0)
            {
                outBytes.FillInt("BoundingListOffset", outBytes.Count);
                for (int i = 0; i < cameraAnchorList.Count; i++)
                {
                    pofSets.Add(outBytes.Count);
                    outBytes.ReserveInt($"Bounding{i}");
                }
            }
            outBytes.FillInt("Anim2ListOffset", outBytes.Count);
            for (int i = 0; i < motion2s.Count; i++)
            {
                pofSets.Add(outBytes.Count);
                outBytes.ReserveInt($"Anim2_{i}");
            }

            if(unkFloatList.Count > 0)
            {
                outBytes.FillInt("UnkFloatListOffset", outBytes.Count);
                for (int i = 0; i < unkFloatList.Count; i++)
                {
                    pofSets.Add(outBytes.Count);
                    outBytes.ReserveInt($"UnkFloat{i}");
                }
            }
            outBytes.FillInt("TexListOffset", outBytes.Count);
            pofSets.Add(outBytes.Count);
            outBytes.ReserveInt($"TexList");
            outBytes.AlignWriter(0x20);
            
            if(!crabSharkOrder)
            {
                outBytes.AlignWriter(0x20);
                outBytes.FillInt("TexOffset", outBytes.Count);
                outBytes.AddRange(gvm.GetBytes());

                for (int i = 0; i < models.Count; i++)
                {
                    outBytes.FillInt($"Model{i}", outBytes.Count);
                    models[i].Write(outBytes, pofSets, true);
                    outBytes.AlignWriter(0x20);
                }
                for (int i = 0; i < motions.Count; i++)
                {
                    outBytes.FillInt($"Anim{i}", outBytes.Count);
                    motions[i].Write(outBytes, pofSets, NJSMotion.MotionWriteMode.BillyMode);
                }
                for (int i = 0; i < cameraAnchorList.Count; i++)
                {
                    outBytes.FillInt($"Bounding{i}", outBytes.Count);
                    outBytes.AddValue(cameraAnchorList[i].Min);
                    outBytes.AddValue(cameraAnchorList[i].Max);
                    outBytes.AddValue(cameraAnchorList[i].int_18);
                    outBytes.AddValue(cameraAnchorList[i].int_1C);
                }
                for (int i = 0; i < motion2s.Count; i++)
                {
                    outBytes.FillInt($"Anim2_{i}", outBytes.Count);
                    motion2s[i].Write(outBytes, pofSets, NJSMotion.MotionWriteMode.BillyMode);
                }

                for (int i = 0; i < unkFloatList.Count; i++)
                {
                    outBytes.FillInt($"UnkFloat{i}", outBytes.Count);
                    outBytes.AddValue(unkFloatList[i]);
                }
                outBytes.FillInt($"TexList", outBytes.Count);
                texList.Write(outBytes, pofSets);
            } else if (crabSharkOrder)
            {
                for (int i = 0; i < models.Count; i++)
                {
                    outBytes.FillInt($"Model{i}", outBytes.Count);
                    models[i].Write(outBytes, pofSets, true);
                }
                for (int i = 0; i < motions.Count; i++)
                {
                    if (motions[i] != null)
                    {
                        outBytes.FillInt($"Anim{i}", outBytes.Count);
                        motions[i].Write(outBytes, pofSets, NJSMotion.MotionWriteMode.BillyMode);
                    }
                }

                for (int i = 0; i < unkFloatList.Count; i++)
                {
                    outBytes.FillInt($"UnkFloat{i}", outBytes.Count);
                    outBytes.AddValue(unkFloatList[i]);
                }
                outBytes.FillInt($"TexList", outBytes.Count);
                texList.Write(outBytes, pofSets);
                outBytes.AlignWriter(0x20);

                outBytes.FillInt("TexOffset", outBytes.Count);
                outBytes.AddRange(gvm.GetBytes());
                for (int i = 0; i < cameraAnchorList.Count; i++)
                {
                    outBytes.FillInt($"Bounding{i}", outBytes.Count);
                    outBytes.AddValue(cameraAnchorList[i].Min);
                    outBytes.AddValue(cameraAnchorList[i].Max);
                    outBytes.AddValue(cameraAnchorList[i].int_18);
                    outBytes.AddValue(cameraAnchorList[i].int_1C);
                }
                for (int i = 0; i < motion2s.Count; i++)
                {
                    outBytes.FillInt($"Anim2_{i}", outBytes.Count);
                    motion2s[i].Write(outBytes, pofSets, NJSMotion.MotionWriteMode.BillyMode);
                }
                outBytes.AlignWriter(0x20);
            }

            //PolyAnim
            if (polyAnim != null)
            {
                group1StructureOffsets.Add("polyanim", outBytes.Count);
                polyAnim.Write(outBytes, pofSets);
            }

            //Add POF0, insert header
            outBytes.AlignWriter(0x4);
            var pof0Offset = outBytes.Count;
            pofSets.Sort();
            var pof0 = POF0.GenerateRawPOF0(pofSets, true);
            outBytes.AddRange(pof0);

            //File references
            outBytes.AddValue((int)group1StructureOffsets["AniModel"]);
            outBytes.ReserveInt("AniModel");
            if(group1StructureOffsets.ContainsKey("polyanim"))
            {
                outBytes.AddValue((int)group1StructureOffsets["polyanim"]);
                outBytes.ReserveInt("polyanim");
            }
            var keys = group2StructureOffsets.Keys.ToList();
            keys.Sort();
            foreach(var key in keys)
            {
                outBytes.AddValue((int)group2StructureOffsets[key]);
                outBytes.ReserveInt(key);
            }

            //Strings
            keys.AddRange(group1StructureOffsets.Keys.ToArray());
            keys.Sort();
            var stringStart = outBytes.Count;
            foreach(var key in keys)
            {
                outBytes.FillInt(key, outBytes.Count - stringStart);
                outBytes.AddRange(Encoding.ASCII.GetBytes(key));
                outBytes.Add(0);
            }

            var arcBytes = new List<byte>();
            arcBytes.AddValue(outBytes.Count + 0x20);
            arcBytes.AddValue(pof0Offset);
            arcBytes.AddValue(pof0.Length);
            arcBytes.AddValue(group1StructureOffsets.Count);

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
    }
}
