using AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.
    public unsafe class CharacterMakingIndex : AquaCommon
    {
        public Dictionary<int, BODYObject> costumeDict = new Dictionary<int, BODYObject>();
        public Dictionary<int, BODYObject> carmDict = new Dictionary<int, BODYObject>();
        public Dictionary<int, BODYObject> clegDict = new Dictionary<int, BODYObject>();
        public Dictionary<int, BODYObject> outerDict = new Dictionary<int, BODYObject>();

        public Dictionary<int, BODYObject> baseWearDict = new Dictionary<int, BODYObject>();
        public Dictionary<int, BBLYObject> innerWearDict = new Dictionary<int, BBLYObject>();
        public Dictionary<int, BBLYObject> bodyPaintDict = new Dictionary<int, BBLYObject>();
        public Dictionary<int, StickerObject> stickerDict = new Dictionary<int, StickerObject>();

        public Dictionary<int, FACEObject> faceDict = new Dictionary<int, FACEObject>();
        public Dictionary<int, FCMNObject> fcmnDict = new Dictionary<int, FCMNObject>();
        public Dictionary<int, FaceTextureObject> faceTextureDict = new Dictionary<int, FaceTextureObject>();
        public Dictionary<int, FCPObject> fcpDict = new Dictionary<int, FCPObject>();

        public Dictionary<int, ACCEObject> accessoryDict = new Dictionary<int, ACCEObject>();
        public Dictionary<int, EYEObject> eyeDict = new Dictionary<int, EYEObject>();
        public Dictionary<int, NGS_EarObject> ngsEarDict = new Dictionary<int, NGS_EarObject>();
        public Dictionary<int, NGS_TeethObject> ngsTeethDict = new Dictionary<int, NGS_TeethObject>();

        public Dictionary<int, NGS_HornObject> ngsHornDict = new Dictionary<int, NGS_HornObject>();
        public Dictionary<int, NGS_SKINObject> ngsSkinDict = new Dictionary<int, NGS_SKINObject>();
        public Dictionary<int, EYEBObject> eyebrowDict = new Dictionary<int, EYEBObject>();
        public Dictionary<int, EYEBObject> eyelashDict = new Dictionary<int, EYEBObject>();

        public Dictionary<int, HAIRObject> hairDict = new Dictionary<int, HAIRObject>();
        public Dictionary<int, NIFL_COLObject> colDict = new Dictionary<int, NIFL_COLObject>();
        public Dictionary<int, VTBF_COLObject> legacyColDict = new Dictionary<int, VTBF_COLObject>();

        public List<Unk_IntField> unkList = new List<Unk_IntField>();
        public Dictionary<int, BCLNObject> costumeIdLink = new Dictionary<int, BCLNObject>();

        public Dictionary<int, BCLNObject> castArmIdLink = new Dictionary<int, BCLNObject>();
        public Dictionary<int, BCLNObject> clegIdLink = new Dictionary<int, BCLNObject>();
        public Dictionary<int, BCLNObject> outerWearIdLink = new Dictionary<int, BCLNObject>();
        public Dictionary<int, BCLNObject> baseWearIdLink = new Dictionary<int, BCLNObject>();

        public Dictionary<int, BCLNObject> innerWearIdLink = new Dictionary<int, BCLNObject>();
        public Dictionary<int, BCLNObject> castHeadIdLink = new Dictionary<int, BCLNObject>();
        public Dictionary<int, BCLNObject> accessoryIdLink = new Dictionary<int, BCLNObject>();

        public Dictionary<int, Part6_7_22Obj> part6_7_22Dict = new Dictionary<int, Part6_7_22Obj>();

        public CMXTable cmxTable = null;
    }
}
