using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary
{
    public class PSO2Text : AquaCommon
    {
        public List<string> categoryNames = new List<string>();
        public List<List<List<PSO2Text.textPair>>> text = new List<List<List<PSO2Text.textPair>>>(); //Category, subCategory, id

        public struct textPair
        {
            public string name;
            public string str;
        }
    }
}
