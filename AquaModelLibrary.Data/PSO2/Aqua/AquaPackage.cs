using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Data.PSO2.MiscPSO2Structs;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Ice;
using AquaModelLibrary.Helpers.Readers;
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
        public List<TPN> tpns = new List<TPN>();

        public List<AquaMotion> motions = new List<AquaMotion>();

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

        public string[] GetEnvelopeTypes() => fileExtensions;

        public AquaPackage() { }
        public AquaPackage(AquaObject aqp) { models.Add(aqp); }
        public AquaPackage(List<AquaObject> aqps) { models.AddRange(aqps); }
        public AquaPackage(AquaMotion aqm) { motions.Add(aqm); }
        public AquaPackage(List<AquaMotion> aqms) { motions.AddRange(aqms); }

        public AquaPackage(byte[] file)
        {
            Read(file);
        }

        public AquaPackage(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(sr);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
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

            long nextFileOffset = -1;
            for (int i = 0; i < afp.fileCount; i++)
            {
                string currentExt;

                //Seek based on AFP size for consecutive files so we don't get misaligned for VTBF
                if (i > 0)
                {
                    sr.Seek(nextFileOffset, SeekOrigin.Begin);
                }

                //If afp magic isn't 0, we have an AFPBase to read
                if (afp.magic != 0)
                {
                    afpEnvelopes.Add(sr.Read<AFPBase>());
                    offset = (int)sr.Position + 0x20;
                    currentExt = Path.GetExtension(afpEnvelopes[i].fileName.GetString());
                    nextFileOffset = sr.Position - 0x30 + afpEnvelopes[i].totalSize;
                }
                else //Not an afp archive, so we use the original extension
                {
                    currentExt = ext;
                }

                //Handle based on known
                switch (currentExt)
                {
                    case ".aqp":
                    case ".trp":
                    case ".aqo":
                    case ".tro":
                        models.Add(new AquaObject(sr));
                        break;
                    case ".aqm":
                    case ".aqv":
                    case ".aqw":
                    case ".aqc":
                    case ".trm":
                    case ".trv":
                    case ".trw":
                        motions.Add(new AquaMotion(sr));
                        break;
                    case ".tpn":
                        tpns.Add(new TPN(sr));
                        break;
                }
            }
        }

        public void WritePackage(string FileName, bool writeVTBF = false)
        {
            File.WriteAllBytes(FileName, GetPackageBytes(FileName, writeVTBF));
        }

        public byte[] GetPackageBytes(string FileName, bool writeVTBF = false)
        {
            FileName = Path.GetFileName(FileName);
            bool package = FileName.Contains(".aqp") || FileName.Contains(".trp"); //If we're doing .aqo/.tro instead, we write only the first model and no aqp header

            var files = new List<AquaCommon>();
            files.AddRange(models);
            files.AddRange(motions);
            int fileCount = files.Count;
            List<byte> finalOutBytes = new List<byte>();
            if (package)
            {
                finalOutBytes.AddRange(new byte[] { 0x61, 0x66, 0x70, 0 });
                finalOutBytes.AddRange(BitConverter.GetBytes(fileCount + tpns.Count));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)0));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)1));
            }
            else
            {
                fileCount = 1;
            }

            if(fileCount == 1 && models.Count == 0)
            {
                List<byte> outBytes = new List<byte>();
                if (writeVTBF)
                {
                    outBytes.AddRange(files[0].GetBytesVTBF());
                }
                else
                {
                    outBytes.AddRange(files[0].GetBytesNIFL());
                }
                return outBytes.ToArray();
            }
            else
            {
                for (int i = 0; i < fileCount; i++)
                {
                    int bonusPadding = 0;
                    if (writeVTBF && i == 0)
                    {
                        bonusPadding = 0x10;
                    }

                    List<byte> outBytes = new List<byte>();
                    if (writeVTBF)
                    {
                        outBytes.AddRange(files[i].GetBytesVTBF());
                    }
                    else
                    {
                        outBytes.AddRange(files[i].GetBytesNIFL());
                    }
                    //Header info
                    int size = outBytes.Count;

                    if (FileName.Length > 0x20)
                    {
                        FileName = FileName.Substring(0, 0x19) + Path.GetExtension(FileName);
                    }
                    WriteAFPBase(Path.ChangeExtension(FileName.Replace(".", $"_l{i + 1}."), ReturnModelTypeString(FileName)), package, bonusPadding, outBytes, size);

                    finalOutBytes.AddRange(outBytes);
                    finalOutBytes.AlignFileEndWriter(0x10);
                }
                WriteTPN(package, finalOutBytes, Path.ChangeExtension(FileName, ".tpn"));
                return finalOutBytes.ToArray();
            }
        }

        private void WriteAFPBase(string fileName, bool package, int bonusPadding, List<byte> outBytes, int size)
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
                afpBase.fileName = new PSO2String(fileName);
                afpBase.paddingOffset = size;
                afpBase.afpBaseSize = 0x30;
                afpBase.totalSize = size + difference + 0x30 + bonusPadding;
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
