using AquaModelLibrary.Data.BillyHatcher.LNDH;
using AquaModelLibrary.Data.Capcom.MonsterHunter;
using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;
using System.Text;
using static AquaModelLibrary.Data.BillyHatcher.LND;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// Default.arc from the Billy Hatcher demo/trial
    /// </summary>
    public class TrialDefaultArc : ARC
    {
        public LND protoLND = null;
        public TrialDefaultArc() { }
        public TrialDefaultArc(byte[] file)
        {
            Read(file);
        }

        public TrialDefaultArc(BufferedStreamReaderBE<MemoryStream> sr)
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
            sr._BEReadActive = true;
            base.Read(sr);
            sr.Seek(0x20, SeekOrigin.Begin);
            int mainLNDModelPtr = sr.ReadBE<int>();
            int altVertColorPtr = sr.ReadBE<int>();
            int animatedModelCount = sr.ReadBE<int>();
            int animatedModelOffset = sr.ReadBE<int>();
            int texListPtr = sr.ReadBE<int>();
            int gvmPtr = sr.ReadBE<int>();
            int pad0 = sr.ReadBE<int>();
            int pad1 = sr.ReadBE<int>();

            protoLND = new LND();
            protoLND.isArcLND = true;
            ARCLNDModel blockModel;
            protoLND.arcLndModels.Add(new LND.ARCLNDStaticMeshData() { name = "Block", model = protoLND.ReadArcLndModel(sr, true)});
            //Technically a static model so we set it as such after in case we wanted to write this to a regular ARCLND
            protoLND.arcLndModels[0].model.isAnimModel = false;
            blockModel = protoLND.arcLndModels[0].model;

            //Alt Vertex Colors
            if (altVertColorPtr != 0)
            {
                sr.Seek(0x20 + altVertColorPtr, SeekOrigin.Begin);
                blockModel.arcAltVertRef = new ARCLNDAltVertColorRef();
                blockModel.arcAltVertRef.count = sr.ReadBE<int>();
                blockModel.arcAltVertRef.offset = sr.ReadBE<int>();
                sr.Seek(0x20 + blockModel.arcAltVertRef.offset, SeekOrigin.Begin);
                for (int i = 0; i < blockModel.arcAltVertRef.count; i++)
                {
                    ARCLNDAltVertColorMainRef altVert = new ARCLNDAltVertColorMainRef();
                    altVert.id = sr.ReadBE<int>();
                    altVert.offset = sr.ReadBE<int>();
                    blockModel.arcAltVertRefs.Add(altVert);

                    var bookmark = sr.Position;
                    sr.Seek(0x20 + altVert.offset, SeekOrigin.Begin);
                    ARCLNDVertDataSet arcVertDataSet = new ARCLNDVertDataSet();
                    LND.ReadVertDataSet(sr, arcVertDataSet);
                    blockModel.arcAltVertColorList.Add(arcVertDataSet);

                    sr.Seek(bookmark, SeekOrigin.Begin);
                }
            }
            if (animatedModelOffset != 0)
            {
                sr.Seek(0x20 + animatedModelOffset, SeekOrigin.Begin);
                for (int i = 0; i < animatedModelCount; i++)
                {
                    ARCLNDAnimatedMeshRefSet set = new ARCLNDAnimatedMeshRefSet();
                    set.modelOffset = sr.ReadBE<int>();
                    set.motionOffset = sr.ReadBE<int>();
                    set.MPLAnimId = sr.ReadBE<int>();
                    protoLND.arcLndAnimatedModelRefs.Add(set);
                }
                foreach (var set in protoLND.arcLndAnimatedModelRefs)
                {
                    ARCLNDAnimatedMeshData meshData = new ARCLNDAnimatedMeshData();
                    meshData.MPLAnimId = set.MPLAnimId;
                    sr.Seek(0x20 + set.modelOffset, SeekOrigin.Begin);
                    meshData.model = protoLND.ReadArcLndModel(sr, true);
                    if (set.motionOffset != 0)
                    {
                        sr.Seek(0x20 + set.motionOffset, SeekOrigin.Begin);
                        meshData.motion = new Motion(sr, 0x20);
                    }
                    protoLND.arcLndAnimatedMeshDataList.Add(meshData);
                }
            }
            sr.Seek(0x20 + texListPtr, SeekOrigin.Begin);
            protoLND.texnames = new NJTextureList(sr, 0x20);
            sr.Seek(0x20 + gvmPtr, SeekOrigin.Begin);
            protoLND.gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr, true));
        }

        public byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            List<int> offsets = new List<int>()
            {
                outBytes.Count,
                outBytes.Count + 0x4,
                outBytes.Count + 0xC,
                outBytes.Count + 0x10,
                outBytes.Count + 0x14
            };

            outBytes.ReserveInt("mainLNDModelPtr");
            outBytes.ReserveInt("altVertColorPtr");
            outBytes.AddValue((int)protoLND.arcLndAnimatedMeshDataList.Count);
            outBytes.ReserveInt("animatedModelOffset");
            outBytes.ReserveInt("texListPtr");
            outBytes.ReserveInt("gvmPtr");
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);

            //Main model
            //This makes it skip the static header, which this version wouldn't have
            //Also remove the alt vert color list since we don't want to write that this way
            protoLND.arcLndModels[0].model.isAnimModel = true;
            var altColor = protoLND.arcLndModels[0].model.arcAltVertColorList[0];
            protoLND.arcLndModels[0].model.arcAltVertColorList.Clear();

            outBytes.FillInt("mainLNDModelPtr", outBytes.Count);
            outBytes.AddRange(protoLND.arcLndModels[0].model.GetBytes(outBytes.Count, new List<ARCLNDAnimatedMeshData>(), out var modelOffsets));
            offsets.AddRange(modelOffsets);
            //Reset it for future use
            protoLND.arcLndModels[0].model.isAnimModel = false;
            protoLND.arcLndModels[0].model.arcAltVertColorList.Add(altColor);

            //Alt Vert data
            if (protoLND.arcLndModels[0].model.arcAltVertColorList.Count > 0)
            {
                outBytes.FillInt($"altVertColorPtr", outBytes.Count + 0x20);
                outBytes.AddValue(protoLND.arcLndModels[0].model.arcAltVertColorList.Count);
                offsets.Add(outBytes.Count + 0x20);
                outBytes.ReserveInt("AltVertColorRefOffset");

                outBytes.FillInt("AltVertColorRefOffset", outBytes.Count + 0x20);
                for (int i = 0; i < protoLND.arcLndModels[0].model.arcAltVertColorList.Count; i++)
                {
                    outBytes.AddValue(i);
                    offsets.Add(outBytes.Count + 0x20);
                    outBytes.ReserveInt($"AltVerts{i}");
                }
                for (int i = 0; i < protoLND.arcLndModels[0].model.arcAltVertColorList.Count; i++)
                {
                    outBytes.FillInt($"AltVerts{i}", outBytes.Count + 0x20);
                    outBytes.AddRange(protoLND.arcLndModels[0].model.arcAltVertColorList[i].GetVertDataBytes(outBytes.Count + 0x20, out var verDataOffsets));
                    offsets.AddRange(verDataOffsets);
                }
            }

            //Animated Models
            if (protoLND.arcLndAnimatedMeshDataList.Count > 0)
            {
                outBytes.FillInt($"animatedModelOffset", outBytes.Count + 0x20);
                for (int i = 0; i < protoLND.arcLndAnimatedMeshDataList.Count; i++)
                {
                    offsets.Add(outBytes.Count + 0x20);
                    outBytes.ReserveInt($"AnimatedModel{i}");
                    offsets.Add(outBytes.Count + 0x20);
                    outBytes.ReserveInt($"AnimatedMotion{i}");
                    outBytes.AddValue(protoLND.arcLndAnimatedMeshDataList[i].MPLAnimId);
                }
                outBytes.AlignWriter(0x20);
                for (int i = 0; i < protoLND.arcLndAnimatedMeshDataList.Count; i++)
                {
                    outBytes.FillInt($"AnimatedModel{i}", outBytes.Count + 0x20);
                    outBytes.AddRange(protoLND.arcLndAnimatedMeshDataList[i].model.GetBytes(outBytes.Count + 0x20, new List<ARCLNDAnimatedMeshData>(), out var animModelOffsets));
                    offsets.AddRange(animModelOffsets);
                    outBytes.FillInt($"AnimatedMotion{i}", outBytes.Count + 0x20);
                    outBytes.AddRange(protoLND.arcLndAnimatedMeshDataList[i].motion.GetBytes(outBytes.Count + 0x20, out var animOffsets));
                    offsets.AddRange(animOffsets);
                    outBytes.AlignWriter(0x20);
                }
            }

            //Texlist
            if (protoLND.texnames.texNames.Count > 0)
            {
                outBytes.FillInt("texListPtr", outBytes.Count);
                offsets.Add(outBytes.Count + 0x20);
                outBytes.ReserveInt("TexListReferencesOffset");
                outBytes.AddValue(protoLND.texnames.texNames.Count);
                outBytes.FillInt("TexListReferencesOffset", outBytes.Count);
                for (int i = 0; i < protoLND.texnames.texNames.Count; i++)
                {
                    offsets.Add(outBytes.Count + 0x20);
                    outBytes.ReserveInt($"TexRef{i}");
                    outBytes.AddValue(0);
                    outBytes.AddValue(0);
                }
                for (int i = 0; i < protoLND.texnames.texNames.Count; i++)
                {
                    outBytes.FillInt($"TexRef{i}", outBytes.Count);
                    outBytes.AddRange(Encoding.UTF8.GetBytes(protoLND.texnames.texNames[i]));
                    outBytes.Add(0);
                }
            }

            //GVM
            outBytes.AlignWriter(0x20);
            outBytes.FillInt("gvmPtr", outBytes.Count);
            outBytes.AddRange(protoLND.gvm.GetBytes());
            outBytes.AlignWriter(0x20);

            //Write headerless POF0
            int pof0Offset = outBytes.Count;
            offsets.Sort();
            outBytes.AddRange(POF0.GenerateRawPOF0(offsets));
            int pof0End = outBytes.Count;
            int pof0Size = pof0End - pof0Offset;

            //ARC Header (insert at the end to make less messy)
            List<byte> arcHead = new List<byte>();
            arcHead.AddValue(outBytes.Count + 0x20);
            arcHead.AddValue(pof0Offset);
            arcHead.AddValue(pof0Size);
            arcHead.AddValue(0);

            arcHead.AddValue(0);
            arcHead.Add(0x30);
            arcHead.Add(0x31);
            arcHead.Add(0x30);
            arcHead.Add(0x30);
            arcHead.AddValue(0);
            arcHead.AddValue(0);
            outBytes.InsertRange(0, arcHead);

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }
    }
}
