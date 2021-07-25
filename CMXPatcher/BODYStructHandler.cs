using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.CharacterMakingIndex;
namespace CMXPatcher
{
    public static class BODYStructHandler
    {
        //Converts a body struct's accessible contents to text
        //Currently strings and such are left alone, though that could be changed in the future
        //Some things like the main id really shouldn't be changed and so are intentionally excluded
        public static StringBuilder ConvertToString(BODYObject body, string type)
        {
            StringBuilder outText = new StringBuilder();

            outText.AppendLine(type + " : " + body.body.id);

            outText.AppendLine("int_20 = " + body.body.int_20);
            outText.AppendLine("int_24_0x9_0x9 = " + body.body.int_24_0x9_0x9);
            outText.AppendLine("int_28 = " + body.body.int_28);
            outText.AppendLine("int_2C = " + body.body.int_2C);

            outText.AppendLine("costumeSoundId = " + body.body.costumeSoundId);
            outText.AppendLine("reference_id = " + body.body.reference_id);
            outText.AppendLine("int_38 = " + body.body.int_38);
            outText.AppendLine("int_3C = " + body.body.int_3C);

            outText.AppendLine("int_40 = " + body.body.int_40);
            outText.AppendLine("int_44 = " + body.body.int_44);
            outText.AppendLine("legLength = " + body.body.legLength);
            outText.AppendLine("float_4C_0xB = " + body.body.float_4C_0xB);

            outText.AppendLine("float_50 = " + body.body.float_50);
            outText.AppendLine("float_54 = " + body.body.float_54);
            outText.AppendLine("float_58 = " + body.body.float_58);
            outText.AppendLine("float_5C = " + body.body.float_5C);

            outText.AppendLine("float_60 = " + body.body.float_60);
            outText.AppendLine("int_64 = " + body.body.int_64);

            return outText;
        }
        
        //Takes an existing bodyObject class and applies the data from the user text into it
        //Expects data to be pruned a little beforehand
        public static void PatchBody(BODYObject body, List<string> bodyData)
        {
            foreach(var line in bodyData)
            {
                var contents = line.Replace(" = ", "=").Split("=");

                switch(contents[0])
                {
                    case "int_20":
                        body.body.int_20 = Int32.Parse(contents[1]);
                        break;
                    case "int_24_0x9_0x9":
                        body.body.int_24_0x9_0x9 = Int32.Parse(contents[1]);
                        break;
                    case "int_28":
                        body.body.int_28 = Int32.Parse(contents[1]);
                        break;
                    case "int_2C":
                        body.body.int_2C = Int32.Parse(contents[1]);
                        break;

                    case "costumeSoundId":
                        body.body.costumeSoundId = Int32.Parse(contents[1]);
                        break;
                    case "reference_id":
                        body.body.reference_id = Int32.Parse(contents[1]);
                        break;
                    case "int_38":
                        body.body.int_38 = Int32.Parse(contents[1]);
                        break;
                    case "int_3C":
                        body.body.int_3C = Int32.Parse(contents[1]);
                        break;

                    case "int_40":
                        body.body.int_40 = Int32.Parse(contents[1]);
                        break;
                    case "int_44":
                        body.body.int_44 = Int32.Parse(contents[1]);
                        break;
                    case "legLength":
                        body.body.legLength = Single.Parse(contents[1]);
                        break;
                    case "float_4C_0xB":
                        body.body.float_4C_0xB = Single.Parse(contents[1]);
                        break;

                    case "float_50":
                        body.body.float_50 = Single.Parse(contents[1]);
                        break;
                    case "float_54":
                        body.body.float_54 = Single.Parse(contents[1]);
                        break;
                    case "float_58":
                        body.body.float_58 = Single.Parse(contents[1]);
                        break;
                    case "float_5C":
                        body.body.float_5C = Single.Parse(contents[1]);
                        break;

                    case "float_60":
                        body.body.float_60 = Single.Parse(contents[1]);
                        break;
                    case "int_64":
                        body.body.int_64 = Int32.Parse(contents[1]);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
