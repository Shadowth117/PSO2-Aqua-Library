using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class CMXTable
    {
        public int bodyAddress; //BODY Costumes
        public int carmAddress; //CARM Cast Arms
        public int clegAddress; //CLEG Cast Legs
        public int bodyOuterAddress; //BODY Outer Wear

        public int baseWearAddress; //BCLN Base Wear
        public int innerWearAddress; //BBLY Inner Wear
        public int bodyPaintAddress; //BDP1 Body Paint 
        public int stickerAddress; //BDP2 Stickers

        public int faceAddress; //FACE All heads
        public int faceMotionAddress; //Face motions
        public int faceTextureAddress; //NGS Faces?
        public int faceTexturesAddress; //Face textures and face paint

        public int accessoryAddress; //ACCE Accessories
        public int eyeTextureAddress; //EYE eye textures
        public int earAddress; //reboot ears
        public int teethAddress; //reboot mouths

        public int hornAddress; //reboot horns
        public int skinAddress; //reboot and maybe classic skin?
        public int eyebrowAddress; //EYEB eyebrows
        public int eyelashAddress; //EYEL eyelashes

        public int hairAddress; //HAIR 
        public int colAddress; //COL, for color chart textures
        public int unkAddress; //Unknown arrays
        public int costumeIdLinkAddress; //BCLN Costume ids for recolors

        public int castArmIdLinkAddress; //BCLN Cast arm ids for recolors
        public int castLegIdLinkAddress; //BCLN Cast leg ids for recolors
        public int outerIdLinkAddress; //BCLN Outer ids for recolors                  Outer, basewear, and inner links are shifted down after oct 21 2021. This becomes CastHeadLinks
        public int baseWearIdLinkAddress; //BCLN basewear ids for recolors

        public int innerWearIdLinkAddress; //BCLN innerwear ids for recolors
        public int oct21UnkAddress; //Only in October 12, 2021 builds and forward
        public int jun7_22Address; //Only in June 7, 2022 builds and forward
        public int feb8_22UnkAddress; //Only in feb 8, 2022 builds and forwared

        public int bodyCount;
        public int carmCount;
        public int clegCount;
        public int bodyOuterCount;

        public int baseWearCount;
        public int innerWearCount;
        public int bodyPaintCount;
        public int stickerCount;

        public int faceCount;
        public int faceMotionCount;
        public int faceTextureCount;
        public int faceTexturesCount;

        public int accessoryCount;
        public int eyeTextureCount;
        public int earCount;
        public int teethCount;

        public int hornCount;
        public int skinCount;
        public int eyebrowCount;
        public int eyelashCount;

        public int hairCount;
        public int colCount;
        public int unkCount;
        public int costumeIdLinkCount;

        public int castArmIdLinkCount;
        public int castLegIdLinkCount;
        public int outerIdLinkCount;
        public int baseWearIdLinkCount;

        public int innerWearIdLinkCount;
        public int oct21UnkCount; //Only in October 12, 2021 builds and forward
        public int jun7_22Count; //Only in June 7, 2022 builds and forward
        public int feb8_22UnkCount; //Only in feb 8, 2022 builds and forward

        public CMXTable(BufferedStreamReaderBE<MemoryStream> streamReader, int headerOffset)
        {
            bodyAddress = streamReader.Read<int>(); //BODY Costumes
            carmAddress = streamReader.Read<int>(); //CARM Cast Arms
            clegAddress = streamReader.Read<int>(); //CLEG Cast Legs
            bodyOuterAddress = streamReader.Read<int>(); //BODY Outer Wear
            //0x10
            baseWearAddress = streamReader.Read<int>(); //BCLN Base Wear
            innerWearAddress = streamReader.Read<int>(); //BBLY Inner Wear
            bodyPaintAddress = streamReader.Read<int>(); //BDP1 Body Paint 
            stickerAddress = streamReader.Read<int>(); //BDP2 Stickers
            //0x20
            faceAddress = streamReader.Read<int>(); //FACE All heads
            faceMotionAddress = streamReader.Read<int>(); //Face motions
            faceTextureAddress = streamReader.Read<int>(); //NGS Faces?
            faceTexturesAddress = streamReader.Read<int>(); //Face textures and face paint
            //0x30
            accessoryAddress = streamReader.Read<int>(); //ACCE Accessories
            eyeTextureAddress = streamReader.Read<int>(); //EYE eye textures
            earAddress = streamReader.Read<int>(); //reboot ears
            teethAddress = streamReader.Read<int>(); //reboot mouths
            //0x40
            hornAddress = streamReader.Read<int>(); //reboot horns
            skinAddress = streamReader.Read<int>(); //reboot and maybe classic skin?
            eyebrowAddress = streamReader.Read<int>(); //EYEB eyebrows
            eyelashAddress = streamReader.Read<int>(); //EYEL eyelashes
            //0x50
            hairAddress = streamReader.Read<int>(); //HAIR 
            colAddress = streamReader.Read<int>(); //COL, for color chart textures
            unkAddress = streamReader.Read<int>(); //Unknown arrays
            costumeIdLinkAddress = streamReader.Read<int>(); //BCLN Costume ids for recolors
            //0x60
            castArmIdLinkAddress = streamReader.Read<int>(); //BCLN Cast arm ids for recolors
            castLegIdLinkAddress = streamReader.Read<int>(); //BCLN Cast leg ids for recolors
            outerIdLinkAddress = streamReader.Read<int>(); //BCLN Outer ids for recolors
            baseWearIdLinkAddress = streamReader.Read<int>(); //BCLN basewear ids for recolors
            //0x70
            innerWearIdLinkAddress = streamReader.Read<int>(); //BCLN innerwear ids for recolors

            if (headerOffset >= CMXConstants.oct21TableAddressInt)
            {
                oct21UnkAddress = streamReader.Read<int>(); //Only in October 12, 2021 builds and forward
            }
            if (headerOffset >= CMXConstants.jun7_22TableAddressInt)
            {
                jun7_22Address = streamReader.Read<int>(); //Only in feb 8, 2022 builds and forwared
            }
            if (headerOffset >= CMXConstants.feb8_22TableAddressInt)
            {
                feb8_22UnkAddress = streamReader.Read<int>(); //Only in feb 8, 2022 builds and forwared
            }

            bodyCount = streamReader.Read<int>();
            carmCount = streamReader.Read<int>();
            clegCount = streamReader.Read<int>();
            bodyOuterCount = streamReader.Read<int>();

            baseWearCount = streamReader.Read<int>();
            innerWearCount = streamReader.Read<int>();
            bodyPaintCount = streamReader.Read<int>();
            stickerCount = streamReader.Read<int>();

            faceCount = streamReader.Read<int>();
            faceMotionCount = streamReader.Read<int>();
            faceTextureCount = streamReader.Read<int>();
            faceTexturesCount = streamReader.Read<int>();

            accessoryCount = streamReader.Read<int>();
            eyeTextureCount = streamReader.Read<int>();
            earCount = streamReader.Read<int>();
            teethCount = streamReader.Read<int>();

            hornCount = streamReader.Read<int>();
            skinCount = streamReader.Read<int>();
            eyebrowCount = streamReader.Read<int>();
            eyelashCount = streamReader.Read<int>();

            hairCount = streamReader.Read<int>();
            colCount = streamReader.Read<int>();
            unkCount = streamReader.Read<int>();
            costumeIdLinkCount = streamReader.Read<int>();

            castArmIdLinkCount = streamReader.Read<int>();
            castLegIdLinkCount = streamReader.Read<int>();
            outerIdLinkCount = streamReader.Read<int>();
            baseWearIdLinkCount = streamReader.Read<int>();

            innerWearIdLinkCount = streamReader.Read<int>();
            if (headerOffset >= CMXConstants.oct21TableAddressInt)
            {
                oct21UnkCount = streamReader.Read<int>(); //Only in October 12, 2021 builds and forward
            }
            if (headerOffset >= CMXConstants.jun7_22TableAddressInt)
            {
                jun7_22Count = streamReader.Read<int>(); //Only in feb 8, 2022 builds and forwared
            }
            if (headerOffset >= CMXConstants.feb8_22TableAddressInt)
            {
                feb8_22UnkCount = streamReader.Read<int>(); //Only in feb 8, 2022 builds and forwared
            }
        }
    }
}
