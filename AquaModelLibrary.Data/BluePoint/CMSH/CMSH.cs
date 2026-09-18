using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    public class CMSH
    {
        public bool wasCompressed = false;
        public CMSHHeader header = null;
        public CMSHVertexData vertData = null;
        public CMSHFaceData faceData = null;
        public CMSHUnkData0 unkdata0 = null;
        public CMSHUnkData1 unkdata1 = null;
        public CMSHUnkData2 unkdata2 = null;
        public CMSHBoneData boneData = null;
        public CFooter footerData;

        public CMSH()
        {

        }

        public CMSH(byte[] file)
        {
            file = CompressionHandler.CheckCompression(file, out wasCompressed);
            var test = BitConverter.ToUInt16(file, 0);
            if(test == 0x1500 || test == 0x500 || test == 0xD01 || test == 0x4100 || test == 0x1100 || test == 0x4901 || test == 0x5100)
            {
                //Maybe one day we'll support these, but they're kinda dumb and all LODs
                return;
            }
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        /// <summary>
        /// This should always be called after generating a custom weighted cmsh before export
        /// </summary>
        public void GenerateBoneBoundingSpheres(List<Matrix4x4> inverseWorldTransforms)
        {
            //Loop through all vert weights to sort by bone
            var boneRefs = new List<HashSet<int>>(vertData.positionList.Count);
            for (int i = 0; i < vertData.positionList.Count; i++)
            {
                HashSet<int> vertWeightedBones = new HashSet<int>();
                for(int j = 0; j < vertData.vertWeightIndices.Count; j++)
                {
                    float weight = 0;
                    switch (j)
                    {
                        case 0:
                            weight = vertData.vertWeights[j].X;
                            break;
                        case 1:
                            weight = vertData.vertWeights[j].Y;
                            break;
                        case 2:
                            weight = vertData.vertWeights[j].Z;
                            break;
                        case 3:
                            weight = vertData.vertWeights[j].W;
                            break;
                    }
                    if(weight > 0)
                    {
                        vertWeightedBones.Add(vertData.vertWeightIndices[i][j]);
                    }
                }
                boneRefs.Add(vertWeightedBones);
            }

            //Average per instance per face index
            var avgArr = new Vector3[boneData.boneNames.Count];
            var countArr = new int[boneData.boneNames.Count];
            foreach (var face in faceData.faceList)
            {
                for(int i = 0; i < 3; i++)
                {
                    var faceVertId = face[i];
                    foreach(int boneId in boneRefs[faceVertId])
                    {
                        avgArr[boneId] += vertData.positionList[faceVertId];
                        countArr[boneId]++;
                    }
                }
            }
            for(int i = 0; i < avgArr.Length; i++)
            {
                avgArr[i] /= countArr[i];
            }

            //Find radius by looking through vert weight sorted bones and measuring against averages
            var radiusArr = new double[boneData.boneNames.Count];
            for(int i = 0; i < avgArr.Length; i++)
            {
                float radius = 0;
                for (int v = 0; v < vertData.positionList.Count; v++)
                {
                    radius = Math.Max(Vector3.Distance(avgArr[v], vertData.positionList[v]), radius);
                }
                radiusArr[i] = radius;
            }

            //Create new bounding sphere list based on results, transforming by inverse world transforms to remove bone influence
            boneData.boneBoundingSpheres.Clear();
            for(int i = 0; i < avgArr.Length; i++)
            {
                boneData.boneBoundingSpheres.Add(new Vector4(Vector3.Transform(avgArr[i], inverseWorldTransforms[i]), (float)radiusArr[i]));
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr.Seek(sr.BaseStream.Length - 0xC, SeekOrigin.Begin);
            footerData = sr.Read<CFooter>();

            sr.Seek(0, SeekOrigin.Begin);
            header = new CMSHHeader(sr, footerData.version);
            if (header.variantFlag2 != 0x41)
            {
                vertData = new CMSHVertexData(sr, header);
                faceData = new CMSHFaceData(sr, header, vertData.positionList.Count);

                if ((header.variantFlag2 & 0x20) > 0)
                {
                    unkdata0 = new CMSHUnkData0(sr);
                    unkdata1 = new CMSHUnkData1(sr);
                }
                if ((header.variantFlag & 0x1) > 0)
                {
                    byte[] test = sr.ReadBytes(sr.Position + 1, 1);
                    if (test[0] != '$' && !(header.variantFlag == 0x1 && header.variantFlag2 == 0xA))
                    {
                        unkdata2 = new CMSHUnkData2(sr);
                    }
                    boneData = new CMSHBoneData(sr, header.era);
                }
            }
        }

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange(header.GetBytes());
            switch(header.variantFlag2)
            {
                case 0x41:
                    break;
                default:
                    outBytes.AddRange(vertData.GetBytes(boneData == null ? 0 : boneData.boneNames.Count));
                    outBytes.AddRange(faceData.GetBytes(vertData.positionList.Count));
                    if((header.variantFlag2 & 0x20) > 0)
                    {
                        outBytes.AddRange(unkdata0.GetBytes());
                        outBytes.AddRange(unkdata1.GetBytes());
                    }
                    if((header.variantFlag & 0x1) > 0)
                    {
                        if(unkdata2 != null)
                        {
                            outBytes.AddRange(unkdata2.GetBytes());
                        }
                        outBytes.AddRange(boneData.GetBytes(header.era));
                    }
                    break;
            }

            //Footer
            var fileSize = outBytes.Count;
            outBytes.AddRange([0x48, 0x53, 0x45, 0x4D]);
            switch (header.era)
            {
                case BPEra.DemonsSouls:
                    outBytes.AddRange([0x5B, 0, 0, 0]);
                    outBytes.AddValue(fileSize);
                    break;
                default:
                    throw new NotImplementedException($"Unexpected cmsh era {header.era}");
            }
            return outBytes.ToArray();
        }
    }
}
