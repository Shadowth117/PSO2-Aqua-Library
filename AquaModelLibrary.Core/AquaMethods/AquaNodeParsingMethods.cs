using Reloaded.Memory.Streams;
using System;
using System.IO;
using System.Text;
using static AquaModelLibrary.VTBFMethods;

namespace AquaModelLibrary.AquaMethods
{
    public class AquaNodeParsingMethods
    {
        public static AquaNode ReadAquaBones(BufferedStreamReader streamReader)
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header

            //Deal with deicer's extra header nonsense
            if (type.Equals("aqn\0") || type.Equals("trn\0"))
            {
                streamReader.Seek(0xC, SeekOrigin.Begin);
                //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                int headJunkSize = streamReader.Read<int>();

                streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += headJunkSize;
            }

            //Proceed based on file variant
            if (type.Equals("NIFL"))
            {
                return ReadNIFLBones(streamReader);
            }
            else if (type.Equals("VTBF"))
            {
                return ReadVTBFBones(streamReader);
            }

            return null;
        }

        public static AquaNode ReadNIFLBones(BufferedStreamReader streamReader)
        {
            AquaNode bones = new AquaNode();

            bones.nifl = streamReader.Read<AquaCommon.NIFL>();
            bones.rel0 = streamReader.Read<AquaCommon.REL0>();
            bones.ndtr = streamReader.Read<AquaNode.NDTR>();
            for (int i = 0; i < bones.ndtr.boneCount; i++)
            {
                bones.nodeList.Add(streamReader.Read<AquaNode.NODE>());
            }
            for (int i = 0; i < bones.ndtr.effCount; i++)
            {
                bones.nodoList.Add(streamReader.Read<AquaNode.NODO>());
            }
            bones.nof0 = AquaCommon.readNOF0(streamReader);
            AquaGeneralMethods.AlignReader(streamReader, 0x10);
            bones.nend = streamReader.Read<AquaCommon.NEND>();

            return bones;
        }

        public static AquaNode ReadVTBFBones(BufferedStreamReader streamReader)
        {
            AquaNode bones = new AquaNode();

            int dataEnd = (int)streamReader.BaseStream().Length;

            //Seek past vtbf tag
            streamReader.Seek(0x10, SeekOrigin.Current);          //VTBF + AQGF tags

            while (streamReader.Position() < dataEnd)
            {
                var data = ReadVTBFTag(streamReader, out string tagType, out int ptrCount, out int entryCount);
                switch (tagType)
                {
                    case "ROOT":
                        //We don't do anything with this right now.
                        break;
                    case "NDTR":
                        bones.ndtr = parseNDTR(data);
                        break;
                    case "NODE":
                        bones.nodeList = parseNODE(data);
                        break;
                    case "NODO":
                        bones.nodoList = parseNODO(data);
                        break;
                    default:
                        //Data being null signfies that the last thing read wasn't a proper tag. This should mean the end of the VTBF stream if nothing else.
                        if (data == null)
                        {
                            return bones;
                        }
                        throw new System.Exception($"Unexpected tag at {streamReader.Position().ToString("X")}! {tagType} Please report!");
                }
            }

            return bones;
        }

    }
}
