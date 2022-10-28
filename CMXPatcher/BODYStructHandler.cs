using AquaModelLibrary;
using AquaModelLibrary.AquaMethods;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            outText.AppendLine(type + " : " + body.body.id.ToString(new CultureInfo("en-US")));

            outText.AppendLine("int_20 = " + body.body.int_20.ToString(new CultureInfo("en-US")));
            outText.AppendLine("int_24_0x9_0x9 = " + body.body2.int_24_0x9_0x9.ToString(new CultureInfo("en-US")));
            outText.AppendLine("int_28 = " + body.body2.int_28.ToString(new CultureInfo("en-US")));
            outText.AppendLine("int_2C = " + body.body2.int_2C.ToString(new CultureInfo("en-US")));

            outText.AppendLine("costumeSoundId = " + body.body2.costumeSoundId.ToString(new CultureInfo("en-US")));
            outText.AppendLine("reference_id = " + body.body2.reference_id.ToString(new CultureInfo("en-US")));
            outText.AppendLine("int_38 = " + body.body2.int_38.ToString(new CultureInfo("en-US")));
            outText.AppendLine("int_3C = " + body.body2.int_3C.ToString(new CultureInfo("en-US")));

            outText.AppendLine("linkedInnerId = " + body.body2.linkedInnerId.ToString(new CultureInfo("en-US")));
            outText.AppendLine("int_44 = " + body.body2.int_44.ToString(new CultureInfo("en-US")));
            outText.AppendLine("legLength = " + body.body2.legLength.ToString(new CultureInfo("en-US")));
            outText.AppendLine("float_4C_0xB = " + body.body2.float_4C_0xB.ToString(new CultureInfo("en-US")));

            outText.AppendLine("float_50 = " + body.body2.float_50.ToString(new CultureInfo("en-US")));
            outText.AppendLine("float_54 = " + body.body2.float_54.ToString(new CultureInfo("en-US")));
            outText.AppendLine("float_58 = " + body.body2.float_58.ToString(new CultureInfo("en-US")));
            outText.AppendLine("float_5C = " + body.body2.float_5C.ToString(new CultureInfo("en-US")));

            outText.AppendLine("float_60 = " + body.body2.float_60.ToString(new CultureInfo("en-US")));
            outText.AppendLine("int_64 = " + body.body2.int_64.ToString(new CultureInfo("en-US")));

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
                        body.body.int_20 = Int32.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "int_24_0x9_0x9":
                        body.body2.int_24_0x9_0x9 = Int32.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "int_28":
                        body.body2.int_28 = Int32.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "int_2C":
                        body.body2.int_2C = Int32.Parse(contents[1], new CultureInfo("en-US"));
                        break;

                    case "costumeSoundId":
                        body.body2.costumeSoundId = Int32.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "reference_id":
                        body.body2.reference_id = Int32.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "int_38":
                        body.body2.int_38 = Int32.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "int_3C":
                        body.body2.int_3C = Int32.Parse(contents[1], new CultureInfo("en-US"));
                        break;

                    case "int_40":
                    case "linkedInnerId":
                        body.body2.linkedInnerId = Int32.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "int_44":
                        body.body2.int_44 = Int32.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "legLength":
                        body.body2.legLength = Single.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "float_4C_0xB":
                        body.body2.float_4C_0xB = Single.Parse(contents[1], new CultureInfo("en-US"));
                        break;

                    case "float_50":
                        body.body2.float_50 = Single.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "float_54":
                        body.body2.float_54 = Single.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "float_58":
                        body.body2.float_58 = Single.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "float_5C":
                        body.body2.float_5C = Single.Parse(contents[1], new CultureInfo("en-US"));
                        break;

                    case "float_60":
                        body.body2.float_60 = Single.Parse(contents[1], new CultureInfo("en-US"));
                        break;
                    case "int_64":
                        body.body2.int_64 = Int32.Parse(contents[1], new CultureInfo("en-US"));
                        break;

                    default:
                        break;
                }
            }
        }

        public static byte[] GetBODYAsBytes(BODYObject body, bool postRetem = true)
        {
            List<byte> bodyBytes = new List<byte>();
            bodyBytes.AddRange(AquaGeneralMethods.ConvertStruct(body.body));
            if(postRetem)
            {
                bodyBytes.AddRange(AquaGeneralMethods.ConvertStruct(body.bodyRitem));
            }
            bodyBytes.AddRange(AquaGeneralMethods.ConvertStruct(body.body2));

            return bodyBytes.ToArray();
        }
    }
}
