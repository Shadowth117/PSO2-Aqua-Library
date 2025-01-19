using AquaModelLibrary.Data.Ninja.Model;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Data.BillyHatcher.Collision;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    /// <summary>
    /// ar_ene_*.arc - These contain data for enemy models, motions, and a texture list. The GVM is stored externally with the same name, but no preceding ar_ and the .gvm extension
    /// ene_am04 is unused in final, but present on PC
    /// </summary>
    public class ArEnemy : ARC
    {
        public static string[] archiveFilenames = new string[] { 
            "ar_ene_am02.arc",
            "ar_ene_am04.arc",
            "ar_ene_ants_queen.arc",
            "ar_ene_armadillo.arc",
            "ar_ene_bat.arc",
            "ar_ene_blue_boss.arc",
            "ar_ene_common.arc",
            "ar_ene_frog.arc",
            "ar_ene_green_boss.arc",
            "ar_ene_green_demo.arc",
            "ar_ene_last_boss.arc",
            "ar_ene_last_dark.arc",
            "ar_ene_last_ex_boss.arc",
            "ar_ene_lizard.arc",
            "ar_ene_orange_boss.arc",
            "ar_ene_orange_boss_map.arc",
            "ar_ene_penguin.arc",
            "ar_ene_purple_boss.arc",
            "ar_ene_red_boss.arc",
            "ar_ene_snake.arc",
            "ar_ene_yellow_boss.arc",
            "ar_ene_yellow_boss_green.arc",
            "ar_ene_zako_ant.arc",
            "ar_ene_zako_bat.arc",
            "ar_ene_zako_cat.arc",
            "ar_ene_zako_fugu.arc",
            "ar_ene_zako_mtn.arc",
            "ar_ene_zako_snake.arc",
        };

        public static string[] textureFilenames = new string[] {
            "ene_am02.GVM",
            "ene_am04.GVM",
            "ene_ants_queen.GVM",
            "ene_armadillo.GVM",
            "ene_bat.GVM",
            "ene_blue_boss.GVM",
            "ene_blue_boss_suimen.GVM",
            "ene_common.GVM",
            "ene_emotion.GVM",
            "ene_eye.GVM",
            "ene_frog.GVM",
            "ene_green_boss.GVM",
            "ene_green_demo.GVM",
            "ene_last_boss.GVM",
            "ene_last_dark.GVM",
            "ene_last_ex_boss.GVM",
            "ene_lizard.GVM",
            "ene_orange_boss.GVM",
            "ene_orange_boss_map.GVM",
            "ene_penguin.GVM",
            "ene_purple_boss.GVM",
            "ene_red_boss.GVM",
            "ene_snake.GVM",
            "ene_yellow_boss.GVM",
            "ene_yellow_boss_green.GVM",
            "ene_zako_ant.GVM",
            "ene_zako_bat.GVM",
            "ene_zako_cat.GVM",
            "ene_zako_fugu.GVM",
            "ene_zako_mtn.GVM",
            "ene_zako_snake.GVM",
        };

        public List<NJSObject> models = new List<NJSObject>();
        public List<List<NJSMotion>> anims = new List<List<NJSMotion>>();
        public List<NJTextureList> texList = new List<NJTextureList>();
        public List<BoundsXYZ> boundsXYZList = new List<BoundsXYZ>();

        public ArEnemy() { }

        public ArEnemy(byte[] file)
        {
            Read(file);
        }

        public ArEnemy(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }
        public void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            int nodeCount = 0;
            sr._BEReadActive = true;
            base.Read(sr);
            sr.Seek(0x20, SeekOrigin.Begin);

            var modelOffset = sr.ReadBE<int>();
            var animOffset = sr.ReadBE<int>();
            var texListOffset = sr.ReadBE<int>();
            var boundingOffset = sr.ReadBE<int>();
            var firstOffset = sr.PeekBigEndianInt32();
            List<int> offsetQueue = new List<int>() { animOffset, texListOffset, boundingOffset, firstOffset };

            int modelCount = HelperFunctions.GetCount(modelOffset, offsetQueue);
            offsetQueue.RemoveAt(0);
            int animCount = HelperFunctions.GetCount(animOffset, offsetQueue);
            offsetQueue.RemoveAt(0);
            int texListCount = HelperFunctions.GetCount(texListOffset, offsetQueue);
            offsetQueue.RemoveAt(0);
            int boundingCount = HelperFunctions.GetCount(boundingOffset, offsetQueue);

            //Read Models
            sr.Seek(0x20 + modelOffset, SeekOrigin.Begin);
            for (int i = 0; i < modelCount; i++)
            {
                var offset = sr.ReadBE<int>();
                if (offset == 0)
                {
                    break;
                }
                var bookmark = sr.Position;
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                models.Add(new NJSObject(sr, NinjaVariant.Ginja, true, 0x20));

                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Motions
            //This offset leads to a list of offset groups so it ends up being more work to parse out those counts
            if(animOffset > 0)
            {
                sr.Seek(0x20 + animOffset, SeekOrigin.Begin);
                int animSetCount = 0;
                List<int> animSetOffsets = new List<int>();
                for (int i = 0; i < animCount; i++)
                {
                    var offset = sr.ReadBE<int>();
                    if (offset == 0)
                    {
                        break;
                    }
                    if (offset > firstOffset)
                    {
                        animSetCount = i;
                        break;
                    }
                    else
                    {
                        animSetOffsets.Add(offset);
                    }
                }

                var animSetOffsetsArr = animSetOffsets.ToArray();
                animSetOffsets.RemoveAt(0);
                if (texListOffset != 0)
                {
                    animSetOffsets.Add((int)texListOffset);
                }
                else if (boundingOffset != 0)
                {
                    animSetOffsets.Add((int)boundingOffset);
                }
                else
                {
                    animSetOffsets.Add(firstOffset);
                }
                for (int i = 0; i < animSetOffsetsArr.Length; i++)
                {
                    sr.Seek(0x20 + animSetOffsetsArr[i], SeekOrigin.Begin);
                    var setCount = HelperFunctions.GetCount(animSetOffsetsArr[i], animSetOffsets);
                    animSetOffsets.RemoveAt(0);

                    List<NJSMotion> motions = new List<NJSMotion>();
                    for (int j = 0; j < setCount; j++)
                    {
                        var offset = sr.ReadBE<int>();
                        if (offset == 0)
                        {
                            break;
                        }
                        var bookmark = sr.Position;
                        sr.Seek(offset + 0x20, SeekOrigin.Begin);
                        motions.Add(new NJSMotion(sr, true, 0x20, true, nodeCount));

                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                    anims.Add(motions);
                }
            }

            //Read Texlists
            sr.Seek(0x20 + texListOffset, SeekOrigin.Begin);
            for (int i = 0; i < texListCount; i++)
            {
                var offset = sr.ReadBE<int>();
                if(offset == 0)
                {
                    break;
                }
                var bookmark = sr.Position;
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                texList.Add(new NJTextureList(sr, 0x20));

                sr.Seek(bookmark, SeekOrigin.Begin);
            }

            //Read Bounds
            sr.Seek(0x20 + boundingOffset, SeekOrigin.Begin);
            for (int i = 0; i < boundingCount; i++)
            {
                var offset = sr.ReadBE<int>();
                if (offset == 0)
                {
                    break;
                }
                var bookmark = sr.Position;
                sr.Seek(offset + 0x20, SeekOrigin.Begin);
                boundsXYZList.Add(new BoundsXYZ() { Min = sr.ReadBEV3(), Max = sr.ReadBEV3()});

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }
    }
}
