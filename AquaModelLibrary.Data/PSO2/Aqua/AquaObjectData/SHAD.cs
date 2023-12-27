using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.SHADData;
using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    //NGS SHADs are the same as the pso2 equivalent, but the 2 parts at the end are actually used for offsets to 2 new structs
    public class SHAD
    {
        public bool isNGS = false;
        public int unk0; //0x90, type 0x9 //Always 0?
        public PSO2String pixelShader; //0x91, type 0x2 //Pixel Shader string
        public PSO2String vertexShader; //0x92, type 0x2 //Vertex Shader string
        public int shadDetailOffset; //0x93, type 0x9 //Unused in classic. //Offset to struct containing details for the shadExtra area, including a count needed to read it.
        public int shadExtraOffset; //Unused in classic. Not read in some versions of NIFL Tool, causing misalignments. Doesn't exist in VTBF, so perhaps added later on.
                                    //Offset to struct containing extra shader info with areas for some int16s and floats.
        public SHADDetail shadDetail;
        public List<SHADExtraEntry> shadExtra = new List<SHADExtraEntry>();

        public SHAD() { }

        public SHAD(Dictionary<int, object> shadRaw)
        {
            unk0 = (int)shadRaw[0x90];
            pixelShader = new PSO2String((byte[])shadRaw[0x91]);
            vertexShader = new PSO2String((byte[])shadRaw[0x92]);
            shadDetailOffset = (int)shadRaw[0x93];
        }

        public SHAD(BufferedStreamReaderBE<MemoryStream> streamReader, int offset)
        {
            Read(streamReader, offset);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> streamReader, int offset)
        {
            unk0 = streamReader.Read<int>();
            pixelShader = streamReader.Read<PSO2String>();
            vertexShader = streamReader.Read<PSO2String>();
            shadDetailOffset = streamReader.Read<int>();
            shadExtraOffset = streamReader.Read<int>();
            isNGS = Int32.Parse(vertexShader.GetString()) >= 1000;

            long bookmark = streamReader.Position;
            //Some shaders, like some player ones apparently, do not use the extra structs...
            if (shadDetailOffset > 0)
            {
                streamReader.Seek(shadDetailOffset + offset, System.IO.SeekOrigin.Begin);
                shadDetail = streamReader.Read<SHADDetail>();

                streamReader.Seek(shadExtraOffset + offset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < shadDetail.shadExtraCount; i++)
                {
                    shadExtra.Add(streamReader.Read<SHADExtraEntry>());
                }
            }
            else if (shadExtraOffset > 0)
            {
                Debug.WriteLine("**Apparently shadExtraOffset is allowed to be used without shadDetailOffset???**");
            }
            streamReader.Seek(bookmark, System.IO.SeekOrigin.Begin);
        }

        public SHAD Clone()
        {
            SHAD newShad = new SHAD();
            newShad.isNGS = isNGS;
            newShad.unk0 = unk0;
            newShad.pixelShader = PSO2String.GeneratePSO2String(pixelShader.GetBytes());
            newShad.vertexShader = PSO2String.GeneratePSO2String(vertexShader.GetBytes());
            newShad.shadDetailOffset = shadDetailOffset;
            newShad.shadExtraOffset = shadExtraOffset;
            newShad.shadDetail = shadDetail;
            newShad.shadExtra = new List<SHADExtraEntry>(shadExtra);

            return newShad;
        }
    }
}
