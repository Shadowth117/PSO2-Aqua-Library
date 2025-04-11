using AquaModelLibrary.Data.BluePoint.CGPR;
using AquaModelLibrary.Helpers.Readers;
using UnluacNET;

namespace AquaModelLibrary.Data.BluePoint.CMDL
{
    public class CMDL
    {
        public CMDLMagic magic;
        public CGPRObject mainObject = null;
        public CGPRObject secondObject = null;
        public CFooter footer;

        public CMDL()
        {

        }

        public CMDL(byte[] file)
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
            magic = sr.Peek<CMDLMagic>();
            mainObject = CGPRObject.ReadObject(sr, BPEra.None);
            secondObject = CGPRObject.ReadObject(sr, mainObject.era);
            footer = sr.Read<CFooter>();
        }
        public List<string> GetCMeshReferences()
        {
            List<string> cmeshReferences = new List<string>();
            switch (magic)
            {
                case CMDLMagic.SOTC:
                case CMDLMagic.DeSR:
                    foreach (var obj in ((CMDLContainer_Object)mainObject).subObjects)
                    {
                        if (obj.magic == CGPRSubMagic.x8259284A || obj.magic == CGPRSubMagic.x0AF1EECC)
                        {
                            foreach (var set in ((CGPRMultiSubObjectArray_SubObject)obj).subObjectsList)
                            {
                                foreach(var setpiece in set.subObjects)
                                {
                                    switch (setpiece.magic)
                                    {
                                        case CGPRSubMagic.x68955D41:
                                        case CGPRSubMagic.xDC55E007:
                                            cmeshReferences.Add(((CGPRString_SubObject)setpiece).dataString.str);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception("Unrecognized model type!");
            }

            return cmeshReferences;
        }
        public Dictionary<string, string> GetCMATMaterialMap()
        {
            Dictionary<string, string> cmatMap = new Dictionary<string, string>();
            switch (magic)
            {
                case CMDLMagic.SOTC:
                case CMDLMagic.DeSR:
                    foreach(var obj in ((CMDLContainer_Object)mainObject).subObjects)
                    {
                        if(obj.magic == CGPRSubMagic.xD4E77FA8 || obj.magic == CGPRSubMagic.x256BD189)
                        {
                            foreach(var subObj in ((CGPRSubObjectArray_SubObject)obj).subObjects)
                            {
                                if (subObj.magic == CGPRSubMagic.x89DBCCD3 || subObj.magic == CGPRSubMagic.x1694E619)
                                {
                                    foreach (var set in ((CGPRMultiSubObjectArray_SubObject)subObj).subObjectsList)
                                    {
                                        string matName = null;
                                        string matFile = null;
                                        foreach (var finalObj in set.subObjects)
                                        {
                                            switch(finalObj.magic)
                                            {
                                                case CGPRSubMagic.xC317C885:
                                                case CGPRSubMagic.x804FDBCB:
                                                    matName = ((CGPRString_SubObject)finalObj).dataString.str;
                                                    break;
                                                case CGPRSubMagic.x587568C3:
                                                case CGPRSubMagic.x3B129F82:
                                                    matFile = ((CGPRString_SubObject)finalObj).dataString.str;
                                                    break;
                                            }
                                        }
                                        cmatMap.Add(matName, matFile);
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception("Unrecognized model type!");
            }

            return cmatMap;
        }

        /// <summary>
        /// Only relevant for SOTC and most cmdls don't have this
        /// </summary>
        public string GetCPIDPath()
        {
            string cpidPath = null;
            foreach(var obj in ((CMDLContainer_Object)mainObject).subObjects)
            {
                if(obj.magic == CGPRSubMagic.x56C23AA0)
                {
                    cpidPath = ((CGPRString_SubObject)obj).dataString.str;
                }
            }

            return cpidPath;
        }

        /// <summary>
        /// Only relevant for SOTC and many cclms don't have one. Fewer still have both
        /// </summary>
        public void GetCCLMs(out string cclmPath, out string hqCclmPath)
        {
            cclmPath = null;
            hqCclmPath = null;
            foreach (var obj in ((CMDLContainer_Object)mainObject).subObjects)
            {
                switch(obj.magic)
                {
                    case CGPRSubMagic.xF23E68B7:
                        cclmPath = ((CGPRString_SubObject)obj).dataString.str;
                        break;
                    case CGPRSubMagic.x733AC6C8:
                        hqCclmPath = ((CGPRString_SubObject)obj).dataString.str;
                        break;
                }
            }
        }
    }
}
