using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Helpers.Readers;
using ArchiveLib;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// Used for obj_snowman.arc
    /// </summary>
    public class ObjSnowman : ARC
    {
        public List<NJSObject> models = new();
        public List<NJSMotion> cameras = new();
        public List<NJTextureList> texLists = new();
        public PuyoFile gvm = null;

        public ObjSnowman() { }

        public ObjSnowman(byte[] file)
        {
            Read(file);
        }

        public ObjSnowman(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }
        public override void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public override void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            base.Read(sr);
            sr.Seek(0x20, SeekOrigin.Begin);

            var modelOffset = sr.ReadBE<int>();
            var model1Offset = sr.ReadBE<int>();
            var texList0Offset = sr.ReadBE<int>(); //Only one texlist despite it being 'texlists' in the arc contents
            var texturesOffset = sr.ReadBE<int>();

            var cameraOffset0 = sr.ReadBE<int>();
            var cameraOffset1 = sr.ReadBE<int>();

            //Read Models
            if (modelOffset != 0)
            {
                sr.Seek(modelOffset + 0x20, SeekOrigin.Begin);
                models.Add(new NJSObject(sr, NinjaVariant.Ginja, true, 0x20));
            }
            if (model1Offset != 0)
            {
                sr.Seek(model1Offset + 0x20, SeekOrigin.Begin);
                models.Add(new NJSObject(sr, NinjaVariant.Ginja, true, 0x20));
            }

            //Read Texlist
            if (texList0Offset != 0)
            {
                sr.Seek(texList0Offset + 0x20, SeekOrigin.Begin);
                texLists.Add(new NJTextureList(sr, 0x20));
            }

            //Read Textures
            if(texturesOffset != 0)
            {
                sr.Seek(texturesOffset + 0x20, SeekOrigin.Begin);
                gvm = new PuyoFile(GVMUtil.ReadGVMBytes(sr));
            }

            //Read Cameras
            if (cameraOffset0 != 0)
            {
                sr.Seek(cameraOffset0 + 0x20, SeekOrigin.Begin);
                cameras.Add(new NJSMotion(sr, true, 0x20));
            }
            if (cameraOffset1 != 0)
            {
                sr.Seek(cameraOffset1 + 0x20, SeekOrigin.Begin);
                cameras.Add(new NJSMotion(sr, true, 0x20));
            }
        }
    }
}
