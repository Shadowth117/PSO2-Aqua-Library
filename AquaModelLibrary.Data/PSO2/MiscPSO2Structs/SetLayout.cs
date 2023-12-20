using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary.Data.PSO2.MiscPSO2Structs
{
    public class SetLayout
    {
        public string fileName;
        public SetHeader header;
        public List<EntityString> entityStrings = new List<EntityString>();
        public List<SetEntity> setEntities = new List<SetEntity>();

        public struct SetHeader
        {
            public int magic;
            public int entityCount;
            public int entityStringCount;
            public int reserve0;
        }

        public class EntityString
        {
            public int size; //Typically 0x20
            //Strings are comma delimited and the string set as a whole is either null terminated or limited by the size.
            public List<string> subStrings = new List<string>();
        }

        //SetEntities are basically defined on the fly as they go, somewhat like VTBF files
        //Subobjects typically have a length and/or type that determine how to read them as well as a utf8 name preceded by said name's length
        //While they're fairly straightforward, there's a lot of variations.
        public class SetEntity
        {
            public int size;
            public PSO2String entity_variant_string0; //PSO2String
            public int int_str1Sum; //Sum of next string length + its size variable
            public string entity_variant_string1; //utf8 with size
            public string entity_variant_stringJP; //utf16 with size
            public int subObjectCount; //Number of objects within this entity. Note objects can contain other objects, namely "edit".

            //Variables in .set are super arbitrary. While entities tend to be defined pretty rigidly, nothing says they have to be.
            public Dictionary<string, object> variables = new Dictionary<string, object>();

            /*
            public float position_x; //Type 1, float
            public float position_y; //Type 1, float
            public float position_z; //Type 1, float
            public float rotation_x; //Type 1, float
            public float rotation_y; //Type 1, float
            public float rotation_z; //Type 1, float
            public int instance_id; //Type 0, int
            public string instance_comment; //Type 0x3, utf16 with size

            public int edit;
            public int editCount; //int directly after "edit" plain text that gives number of elements for that section
            public string group_id; //Type 2, utf8 with size
            public int group_tag; //Type 2, utf8 with size
            public string object_name; //Type 2, utf8 with size
            public int start_scene; //Type 0, int
            public float appearance_rate; //Type 1, float 
            public string setter_flag; //Type 4, utf8 with null termination. Seemingly comma delimited like EntityStrings
            public string skit_name; //Type 2, utf8 with size
            public int connect_id; //Type 0, int
            public int visible; //Type 0, int
            public float height; //Type 1, float
            public float extend_down; //Type 1, float
            public int unique_id; //Type 0, int
            public int force_move; //Type 0, int
            public int padding1; //Type 0, int
            */
        }
    }
}
