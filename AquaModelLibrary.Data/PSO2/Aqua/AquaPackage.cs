using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Data.PSO2.MiscPSO2Structs;
using AquaModelLibrary.Extensions.Readers;
using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Ice;
using System.IO;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    /// <summary>
    /// Contains either models and possibly texture patterns or motions. Consecutive files are seemingly only used for LOD outside of TPN. 
    /// TPN is extremely rare and likely got phased out early on.
    /// </summary>
    public class AquaPackage
    {
        /// <summary>
        /// We need the extension in case this isn't an AFP archive, in which case the extension is the most reasonable way to determine the file type.
        /// </summary>
        public string ext = null;
        public AFPMain afp = new AFPMain();
        public List<AFPBase> afpEnvelopes = new List<AFPBase>();
        public List<AquaObject> models = new List<AquaObject>();
        public List<TPNTexturePattern> tpns = new List<TPNTexturePattern>();

        public List<AquaMotion> anims = new List<AquaMotion>();

        public static readonly string[] fileExtensions = new string[]
        {
            "aqp\0",
            "trp\0",
            "aqm\0",
            "aqv\0",
            "aqw\0",
            "aqc\0",
            "trw\0",
            "trv\0",
            "trm\0",
        };

        public AquaPackage() { }

        public AquaPackage(byte[] file, string _ext)
        {
            Read(file, _ext);
        }

        public AquaPackage(BufferedStreamReaderBE<MemoryStream> sr, string _ext)
        {
            Read(sr, _ext);
        }

        public void Read(byte[] file, string _ext)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(sr, _ext);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr, string _ext)
        {
            ext = _ext;
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(sr.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header

            //Skip ice header junk if it's there
            IceMethods.SkipIceEnvelope(sr, fileExtensions, ref type, ref offset);

            //Deal with afp header or aqo. prefixing as needed
            if (type.Equals("afp\0"))
            {
                afp = sr.Read<AFPMain>();
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(sr.Peek<int>()));
                offset += 0x40;
            }
            //For deicer extracted aqo or tro. For w/e reason, they extract like this specially from that program. Nonstandard.
            else if (type.Equals("aqo\0") || type.Equals("tro\0"))
            {
                sr.Seek(0x4, SeekOrigin.Current);
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(sr.Peek<int>()));
                offset += 0x4;
            }

            //Workaround for older mods that made this in a ghetto way. The game still reads these so we still consider them valid
            //Used as well for if this is an afp-less file
            if (afp.fileCount == 0)
            {
                afp.fileCount = 1;
            }

            for(int i = 0; i < afp.fileCount; i++)
            {
                string currentExt;
                if (i > 0)
                {
                    sr.Seek(0x10, SeekOrigin.Current);
                }

                //If afp magic isn't 0, we have an AFPBase to read
                int endOffset;
                if (afp.magic != 0)
                {
                    afpEnvelopes.Add(sr.Read<AFPBase>());
                    offset = (int)sr.Position + 0x20;
                    currentExt = Path.GetExtension(afpEnvelopes[i].fileName.GetString());
                    endOffset = (int)(sr.Position + afpEnvelopes[i].afpBaseSize);
                } else //Not an afp archive, so we use the original extension
                {
                    currentExt = ext;
                    endOffset = (int)sr.BaseStream.Length;
                }
               
                //Handle based on known
                switch(currentExt)
                {
                    case ".aqp":
                    case ".trp":
                    case ".aqo":
                    case ".tro":
                        models.Add(new AquaObject(sr, offset, endOffset));
                        break;
                    case ".aqm":
                    case ".aqv":
                    case ".aqw":
                    case ".aqc":
                    case ".trm":
                    case ".trv":
                    case ".trw":
                        switch (type)
                        {
                            case "NIFL":
                                anims.Add();
                                break;
                            case "VTBF":
                                anims.Add();
                                break;
                        }
                        break;
                    case ".tpn":
                        tpns.Add(new TPNTexturePattern(sr));
                        break;
                }
            }
        }

        public void WriteVTBFModel(string FileName)
        {
            bool package = FileName.Contains(".aqp") || FileName.Contains(".trp"); //If we're doing .aqo/.tro instead, we write only the first model and no aqp header
            int modelCount = models.Count;
            List<byte> finalOutBytes = new List<byte>();
            if (package)
            {
                finalOutBytes.AddRange(new byte[] { 0x61, 0x66, 0x70, 0 });
                finalOutBytes.AddRange(BitConverter.GetBytes(models.Count + tpns.Count));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)0));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)1));
            }
            else
            {
                modelCount = 1;
            }

            for (int i = 0; i < modelCount; i++)
            {
                int bonusBytes = 0;
                if (i == 0)
                {
                    bonusBytes = 0x10;
                }

                List<byte> outBytes = new List<byte>();
                outBytes.AddRange(models[i].GetBytesVTBF());
                //Header info
                int size = outBytes.Count;
                WriteAFPBase(Path.ChangeExtension(FileName.Replace(".", $"_l{i + 1}."), ReturnModelTypeString(FileName)), package, bonusBytes, outBytes, size);

                finalOutBytes.AddRange(outBytes);
                finalOutBytes.AlignFileEndWriter(0x10);
            }
            WriteTPN(package, finalOutBytes, Path.ChangeExtension(FileName, ".tpn"));

            File.WriteAllBytes(FileName, finalOutBytes.ToArray());
        }

        private void WriteAFPBase(string fileName, bool package, int bonusBytes, List<byte> outBytes, int size)
        {
            if (package)
            {
                int difference;
                if (size % 0x10 == 0)
                {
                    difference = 0x10;
                }
                else
                {
                    difference = 0x10 - (size % 0x10);
                }

                //Handle filename text
                AFPBase afpBase = new AFPBase();
                afpBase.fileName = new SetLengthStrings.PSO2String(fileName);
                afpBase.paddingOffset = size;
                afpBase.afpBaseSize = 0x30;
                afpBase.totalSize = size + difference + 0x30 + bonusBytes;
                afpBase.fileTypeCString = ReturnModelType(fileName);

                outBytes.InsertRange(0, afpBase.GetBytes());
            }
        }

        private void WriteTPN(bool package, List<byte> finalOutBytes, string fileName)
        {

            //Write texture patterns
            if (package)
            {
                for (int i = 0; i < tpns.Count; i++)
                {
                    List<byte> outBytes = new List<byte>();
                    outBytes.AddRange(tpns[i].GetBytes());
                    WriteAFPBase(fileName, true, 0, outBytes, outBytes.Count);
                    ByteListExtension.AlignFileEndWriter(outBytes, 0x10);
                    finalOutBytes.AddRange(outBytes);
                }
            }
        }

        private int ReturnModelType(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            if (ext.Equals(".aqp") || ext.Equals(".aqo"))
            {
                return 0x6F7161;
            }
            else if (ext.Equals(".trp") || ext.Equals(".tro"))
            {
                return 0x6F7274;
            }
            else
            {
                return BitConverter.ToInt32(Encoding.ASCII.GetBytes(ext), 0);
            }
        }

        private string ReturnModelTypeString(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            if (ext.Equals(".aqp") || ext.Equals(".aqo"))
            {
                return ".aqo";
            }
            else if (ext.Equals(".trp") || ext.Equals(".tro"))
            {
                return ".aqo";
            }
            else
            {
                return ext;
            }
        }
    }
}
