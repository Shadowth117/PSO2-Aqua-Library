using AquaModelLibrary.Data.BillyHatcher.ARCData;
using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher
{
    /// <summary>
    /// ARC is similar to something like PSO2 NIFL where it's more of a wrapper for other formats. .lnd also uses a variant of this at times.
    /// While some ARC files have named file groups to denote usage, some do not and simply expect to be handled based on their filename pattern.
    /// Here is a rough list of what .arc file does what:
    /// 
    /// ae_  : 2d data. Seems to contain data to assemble and animate the images they contain. At a glance, these vary based on if they contain palette data or not.
    /// ani_ : Egg animal data. These have file groups. _model and _param subcategories are what they sound like. _birth_cam is seemingly the camera data, including pathing, for when egg animals hatch.
    ///             race_red is unclear at this time, though probably a mission race's pathing.
    /// ar_blue_ : Pirate Elder data? Has file groups.
    /// ar_ene_ : Enemy data. 
    /// ar_event_ : In-game cutscene data. Has file groups.
    /// ar_icon3d : Unknown icon data. Has file groups.
    /// ar_ma_ : Item data. Used for when an animated item is activated? Has file groups.
    /// ar_nowloading : Stub file related to the loading screen. Unclear if it does something at a glance.
    /// ar_obj : Object data for various stages. 
    /// ar_title : Title screen data 
    /// balloon : Unknown data : Has file groups.
    /// battlemodel_ : Battle mode model data
    /// egg_gold : Elder golden egg data : Has file groups.
    /// egg_suit : Legendary chicken suit : Has file groups.
    /// event_ : Level scripting? 
    /// gallery_egg : Data for eggs in the gallery
    /// ge_egg : Egg data. Has file groups.
    /// ge_player : Player character data. Has file groups.
    /// geobj_ : Level geometry objects : Has partial file groups. Some data is listed at the start with counts as to how much of a particular data type is there, followed by pointers to it. 
    ///                                   Following all of this data starts the file group data.
    ///                                   geobj_ that aren't common or a specific stage geobj_ are a different format variation entirely. These object ones ARE file group based with redundant references leading off.
    /// item_ani : Used for item data.
    /// item_cap : Used for cap item data.
    /// item_chickenbomb : What it sounds like.
    /// item_comb : Chicken comb item data.
    /// item_ : All others items work similarly to the above.
    /// lib_ : Gallery data. Same as item_ data in layout and many entries here are identical to that area.
    /// main_menu : Menu data.
    /// menu : Menu data.
    /// obj_ : Various objects and their data. Has file groups.
    /// ptcl2_ : Particle data.
    /// stgobj_ : Stage object data. Has file groups.
    /// storySeq : Story sequence data. Has file groups.
    /// superBilly : Super Billy data. Has file groups.
    /// 
    /// arc type files that aren't .arc:
    /// 
    /// egglevel.bin : Tables for configuring the various eggs. See EggLevel.cs for details.
    /// .lnd : Stage geometry, comes in an ARC and non ARC variant. ARC variant has file groups and is used in the final game. The other variant is seemingly unused in final.
    /// .pad : PArtical Data. Has file groups. ani_ files and some others contain this type of data too.
    /// </summary>
    public class ARC
    {
        public ARCHeader arcHeader;
        public List<uint> pof0Offsets = new List<uint>();
        public List<ARCFileRef> group1FileReferences = new List<ARCFileRef>();
        public List<ARCFileRef> group2FileReferences = new List<ARCFileRef>();
        public List<string> group1FileNames = new List<string>();
        public List<string> group2FileNames = new List<string>();
        public List<byte[]> files = new List<byte[]>();
        public ARC() { }

        public ARC(byte[] file)
        {
            Read(file);
        }

        public ARC(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public virtual void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public virtual void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            //Generic ARC header
            arcHeader = ReadArcHeader(sr);

            //Get model references
            sr.Seek(0x20 + arcHeader.pof0Offset, SeekOrigin.Begin);
            pof0Offsets = POF0.GetRawPOF0Offsets(sr.ReadBytes(sr.Position, arcHeader.pof0OffsetsSize));
            sr.Seek(arcHeader.pof0OffsetsSize, SeekOrigin.Current);

            for (int i = 0; i < arcHeader.group1FileCount; i++)
            {
                ARCFileRef modelRef = new ARCFileRef();
                modelRef.fileOffset = sr.ReadBE<int>();
                modelRef.relativeNameOffset = sr.ReadBE<int>();
                group1FileReferences.Add(modelRef);
            }
            for (int i = 0; i < arcHeader.group2FileCount; i++)
            {
                ARCFileRef fileRef = new ARCFileRef();
                fileRef.fileOffset = sr.ReadBE<int>();
                fileRef.relativeNameOffset = sr.ReadBE<int>();
                group2FileReferences.Add(fileRef);
            }

            //Get file names
            var nameStart = sr.Position;
            foreach (var modelRef in group1FileReferences)
            {
                sr.Seek(nameStart + modelRef.relativeNameOffset, SeekOrigin.Begin);
                group1FileNames.Add(sr.ReadCString());
            }
            foreach (var modelRef in group2FileReferences)
            {
                sr.Seek(nameStart + modelRef.relativeNameOffset, SeekOrigin.Begin);
                group2FileNames.Add(sr.ReadCString());
            }
        }

        public static ARCHeader ReadArcHeader(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var arcHeader = new ARCHeader();
            arcHeader.fileSize = sr.ReadBE<int>();
            arcHeader.pof0Offset = sr.ReadBE<int>();
            arcHeader.pof0OffsetsSize = sr.ReadBE<int>();
            arcHeader.group1FileCount = sr.ReadBE<int>();

            arcHeader.group2FileCount = sr.ReadBE<int>();
            arcHeader.magic = sr.ReadBE<int>();
            arcHeader.unkInt0 = sr.ReadBE<int>();
            arcHeader.unkInt1 = sr.ReadBE<int>();

            return arcHeader;
        }
    }
}
