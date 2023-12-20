using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary
{
    public class PSO2Text : AquaCommon
    {
        public List<string> categoryNames = new List<string>();
        public List<List<List<textPair>>> text = new List<List<List<textPair>>>(); //Category, subCategory, id

        public struct textPair
        {
            public string name;
            public string str;
        }
        
        //For helping with writing
        public struct textPairLocation
        {
            public int address;
            public textPair text;
            public int category;
        }

        public struct textLocation
        {
            public int address;
            public string str;
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < text.Count; i++)
            {
                output.AppendLine(categoryNames[i]);

                for (int j = 0; j < text[i].Count; j++)
                {
                    output.AppendLine($"Group {j}");

                    for (int k = 0; k < text[i][j].Count; k++)
                    {
                        var pair = text[i][j][k];
                        output.AppendLine($"{pair.name} - {pair.str}");
                    }
                    output.AppendLine();
                }
                output.AppendLine();
            }

            return output.ToString();
        }
    }
}
