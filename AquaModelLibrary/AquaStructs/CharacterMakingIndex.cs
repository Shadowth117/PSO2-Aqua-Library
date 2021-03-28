using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary
{
    public class CharacterMakingIndex
    {
        public NIFL nifl;
        public REL0 rel0;

        public NOF0 nof0;
        public NEND nend;

        public struct CMXTable
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
            public int rebootFaceAddress; //NGS Faces?
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

            public int castBodyIdLinkAddress; //BCLN Cast body ids for recolors
            public int castLegIdLinkAddress; //BCLN Cast leg ids for recolors
            public int castArmIdLinkAddress; //BCLN Cast arm ids for recolors
            public int baseWearIdLinkAddress; //BCLN basewear ids for recolors

            public int innerWearIdLinkAddress; //BCLN innerwear ids for recolors

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
            public int rebootFaceCount;
            public int facTexturesCount;

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

            public int castBodyIdLinkCount;
            public int castLegIdLinkCount;
            public int castArmIdlinkCount;
            public int baseWearIdLinkCount;

            public int innerWearIdLinkCount;
        }
    }
}
