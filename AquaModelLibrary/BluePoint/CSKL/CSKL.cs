using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CSKL
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

        public CSKL(BufferedStreamReader sr)
        {
            header = new CSKLHeader(sr);
            sr.Seek(header.transformListOffset, System.IO.SeekOrigin.Begin);
            for(int i = 0; i < header.boneCount; i++)
            {
                transforms.Add(new CSKLTransform(sr));
            }
            sr.Seek(header.inverseBoneMatricesOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.boneCount; i++)
            {
                invTransforms.Add(sr.Read<Matrix4x4>());
            }
            sr.Seek(header.boneMetadataOffset, System.IO.SeekOrigin.Begin);
            metadata = new CSKLMetaData(sr);
            names = new CSKLNames(sr);
            footerData = sr.Read<CFooter>();
        }
    }
}
