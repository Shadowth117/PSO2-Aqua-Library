using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    public class CSKL
    {
        public CSKLHeader header = null;
        public List<CSKLTransform> transforms = new List<CSKLTransform>();
        public List<Matrix4x4> invTransforms = new List<Matrix4x4>();
        public CSKLMetaData metadata = null;
        public CSKLNames names = null;
        public CFooter footerData;

        public CSKL()
        {

        }

        public CSKL(byte[] file)
        {
            file = CompressionHandler.CheckCompression(file);
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr.Seek(sr.BaseStream.Length - 0xC, SeekOrigin.Begin);
            footerData = sr.Read<CFooter>();

            sr.Seek(0, SeekOrigin.Begin);
            header = new CSKLHeader(sr, footerData.version);

            switch (footerData.version)
            {
                case 0x9:
                    metadata = new CSKLMetaData();
                    Dictionary<int, CSKLFamilyIds> familyDict = new Dictionary<int, CSKLFamilyIds>();
                    while (sr.Position < header.transformListOffset)
                    {
                        var newFamilyIds = new CSKLFamilyIds();
                        var nodeId = sr.ReadBE<short>();
                        newFamilyIds.parentId = sr.ReadBE<short>();
                        if(newFamilyIds.parentId == 32767)
                        {
                            newFamilyIds.parentId = -1;
                        }
                        if(!familyDict.ContainsKey(nodeId))
                        {
                            familyDict.Add(nodeId, newFamilyIds);
                        }

                        if(nodeId == header.boneCount - 1)
                        {
                            break;
                        }
                    }
                    for(int i = 0; i < header.boneCount; i++)
                    {
                        metadata.familyIds.Add(familyDict[i]);
                    }

                    //Process bone metadata
                    for(int i = 0; i < metadata.familyIds.Count; i++)
                    {
                        var parId = metadata.familyIds[i].parentId;
                        if ( parId != -1)
                        {
                            var parentBone = metadata.familyIds[parId];
                            if (parentBone.firstChildId == -1)
                            {
                                parentBone.firstChildId = i;
                            }
                            else
                            {
                                var prevSibling = metadata.familyIds[parentBone.firstChildId];
                                while(prevSibling.nextSiblingId != -1)
                                {
                                    prevSibling = metadata.familyIds[prevSibling.nextSiblingId];
                                }
                                prevSibling.nextSiblingId = i;
                            }
                        }
                    }

                    sr.Seek(header.transformListOffset, System.IO.SeekOrigin.Begin);
                    for (int i = 0; i < header.boneCount; i++)
                    {
                        transforms.Add(new CSKLTransform(sr, footerData.version));
                    }

                    sr.Seek(header.boneNamesOffset, System.IO.SeekOrigin.Begin);
                    names = new CSKLNames(sr, footerData.version, header.boneCount);
                    break;
                case 0x19:
                    sr.Seek(header.transformListOffset, System.IO.SeekOrigin.Begin);
                    for (int i = 0; i < header.boneCount; i++)
                    {
                        transforms.Add(new CSKLTransform(sr, footerData.version));
                    }
                    sr.Seek(header.inverseBoneMatricesOffset, System.IO.SeekOrigin.Begin);
                    for (int i = 0; i < header.boneCount; i++)
                    {
                        invTransforms.Add(sr.Read<Matrix4x4>());
                    }
                    sr.Seek(header.boneMetadataOffset, System.IO.SeekOrigin.Begin);
                    metadata = new CSKLMetaData(sr, header.boneCount);
                    names = new CSKLNames(sr, footerData.version);
                    break;
            }
        }
    }
}
