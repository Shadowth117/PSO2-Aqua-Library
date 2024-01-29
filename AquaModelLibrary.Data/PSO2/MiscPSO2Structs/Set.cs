using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.MiscPSO2Structs
{
    /// <summary>
    /// .set object layout
    /// </summary>
    public class Set
    {
        public string fileName = null;
        public SetHeader header;
        public List<EntityString> entityStrings = new List<EntityString>();
        public List<SetEntity> setEntities = new List<SetEntity>();

        public Set() { }

        public Set(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public Set(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            string fileType = Encoding.UTF8.GetString(BitConverter.GetBytes(sr.Peek<int>()));

            if (fileType == "set\0")
            {
                sr.Seek(0xC, SeekOrigin.Current);
                var envelopeSize = sr.Read<int>() - 0x10;
                sr.Seek(envelopeSize, SeekOrigin.Current);
            }

            var set = new Set();
            set.fileName = Path.GetFileNameWithoutExtension(fileName);
            set.header = sr.Read<SetHeader>();

            //Read strings
            for (int i = 0; i < set.header.entityStringCount; i++)
            {
                var entityStr = new Set.EntityString();
                entityStr.size = sr.Read<int>();
                var rawStr = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, entityStr.size - 4));
                rawStr = rawStr.Remove(rawStr.IndexOf(char.MinValue));

                //Entity strings are comma delimited
                var rawArray = rawStr.Split(',');
                for (int sub = 0; sub < rawArray.Length; sub++)
                {
                    entityStr.subStrings.Add(rawArray[sub]);
                }

                set.entityStrings.Add(entityStr);
                sr.Seek(entityStr.size - 4, SeekOrigin.Current);
            }

            //Read entities
            for (int i = 0; i < set.header.entityCount; i++)
            {
                var entityStart = sr.Position;

                var entity = new SetEntity();
                entity.size = sr.Read<int>();
                entity.entity_variant_string0 = sr.Read<PSO2String>();
                entity.int_str1Sum = sr.Read<int>();

                var strCount = sr.Read<int>();
                entity.entity_variant_string1 = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, strCount));
                sr.Seek(strCount, SeekOrigin.Current);

                strCount = sr.Read<int>();
                entity.entity_variant_stringJP = Encoding.Unicode.GetString(sr.ReadBytes(sr.Position, strCount));
                sr.Seek(strCount, SeekOrigin.Current);

                entity.subObjectCount = sr.Read<int>();
                //Debug.WriteLine($"Position {(streamReader.Position() - offset).ToString("X")}");
                int trueCount = entity.subObjectCount;
                //Gather variables
                for (int obj = 0; obj < trueCount; obj++)
                {
                    var type = sr.Read<int>();
                    int length; //Used for some types
                    object data;
                    switch (type)
                    {
                        case 0: //Int
                            data = sr.Read<int>();
                            break;
                        case 1: //Float
                            data = sr.Read<float>();
                            break;
                        case 2: //Utf8 String with size
                            length = sr.Read<int>();
                            data = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, length));
                            sr.Seek(length, SeekOrigin.Current);
                            break;
                        case 3: //Unicode-16 String with size
                            length = sr.Read<int>();
                            data = Encoding.Unicode.GetString(sr.ReadBytes(sr.Position, length));
                            sr.Seek(length, SeekOrigin.Current);
                            break;
                        case 4: //Null terminated, comma delimited string list
                            string str = sr.ReadCString(sr.BaseStream.Length); //Yeah idk if this has a limit. I tried.
                            length = str.Length;
                            data = str.Remove(length);
                            sr.Seek(length + 1, SeekOrigin.Current);
                            break;
                        default:
                            Console.WriteLine($"Unknown set type: {type} at position {sr.Position.ToString("X")}");
                            throw new Exception();
                    }

                    //Name is always a utf8 string right after with a predefined length
                    int nameLength = sr.Read<int>();
                    string name = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, nameLength));
                    sr.Seek(nameLength, SeekOrigin.Current);

                    //Some things can denote further objects
                    if (name == "edit")
                    {
                        trueCount += sr.Read<int>();
                    }

                    //I don't know if it's possible for there to be a dupe within these, but if it is, we'll check for it and note it
                    if (entity.variables.ContainsKey(name))
                    {
                        Console.WriteLine($"Duplicate key: {name} at position {sr.Position.ToString("X")}");
                        entity.variables.Add(name + $"({obj})", data);
                    }
                    else
                    {
                        entity.variables.Add(name, data);
                    }

                }
                set.setEntities.Add(entity);
                //Make sure we move to the end properly
                if (sr.Position != entityStart + entity.size)
                {
                    sr.Seek(entityStart + entity.size, SeekOrigin.Begin);
                }
            }
        }

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
            public string entity_variant_string1 = null; //utf8 with size
            public string entity_variant_stringJP = null; //utf16 with size
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
