using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;

//Sourced from SA Tools
namespace AquaModelLibrary.Data.Ninja.Model.Ginja
{
    /// <summary>
    /// A single mesh, with its own parameter and primitive data <br/>
    /// </summary>
    [Serializable]
    public class GinjaMesh
    {
        /// <summary>
        /// The parameters that this mesh sets
        /// </summary>
        public List<GCParameter> parameters { get; private set; }

        /// <summary>
        /// The polygon data
        /// </summary>
        public List<GinjaPrimitive> primitives { get; private set; }

        /// <summary>
        /// The index attribute flags of this mesh. If it has no IndexAttribParam, it will return null
        /// </summary>
        public GCIndexAttributeFlags? IndexFlags
        {
            get
            {
                IndexAttributeParameter index_param = (IndexAttributeParameter)parameters.Find(x => x.type == ParameterType.IndexAttributeFlags);
                if (index_param == null) return null;
                else return index_param.IndexAttributes;
            }
        }

        /// <summary>
        /// The amount of bytes which have been written for the primitives
        /// </summary>
        private uint primitiveSize;


        /// <summary>
        /// Create an empty mesh
        /// </summary>
        public GinjaMesh()
        {
            parameters = new List<GCParameter>();
            primitives = new List<GinjaPrimitive>();
        }

        /// <summary>
        /// Create a new mesh from existing primitives and parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="primitives"></param>
        public GinjaMesh(List<GCParameter> parameters, List<GinjaPrimitive> primitives)
        {
            this.parameters = parameters;
            this.primitives = primitives;
        }

        /// <summary>
        /// Read a mesh from a file
        /// </summary>
        /// <param name="file">The files contents</param>
        /// <param name="address">The address at which the mesh is located</param>
        /// <param name="imageBase">The imagebase (used for when reading from an exe)</param>
        /// <param name="index">Indexattribute parameter of the previous mesh</param>
        public GinjaMesh(BufferedStreamReaderBE<MemoryStream> sr, GCIndexAttributeFlags indexFlags, bool be = true, int offset = 0)
        {
            sr._BEReadActive = be;

            // getting the addresses and sizes
            int parameters_offset = sr.ReadBE<int>();
            int parameters_count = sr.ReadBE<int>();

            int primitives_offset = sr.ReadBE<int>();
            uint primitives_size = sr.ReadBE<uint>();

            var bookmark = sr.Position;
            // reading the parameters
            parameters = new List<GCParameter>();
            sr.Seek(offset + parameters_offset, SeekOrigin.Begin);
            for (int i = 0; i < parameters_count; i++)
            {
                parameters.Add(GCParameter.Read(sr));
            }

            // getting the index attribute parameter
            GCIndexAttributeFlags? flags = IndexFlags;
            if (flags.HasValue)
                indexFlags = flags.Value;

            // reading the primitives
            primitives = new List<GinjaPrimitive>();

            int end_pos = offset + primitives_offset + (int)primitives_size;

            sr.Seek(offset + primitives_offset, SeekOrigin.Begin);
            while (sr.Position < end_pos)
            {
                // if the primitive isnt valid
                if (sr.Peek<byte>() == 0) break;
                primitives.Add(new GinjaPrimitive(sr, indexFlags));
            }
            primitiveSize = primitives_size;

            sr.Seek(bookmark, SeekOrigin.Begin);
        }

        /// <summary>
        /// Writes the parameters and primitives to a stream
        /// </summary>
        /// <param name="writer">The ouput stream</param>
        /// <param name="indexFlags">The index flags</param>

        public byte[] GetBytes(uint parameterAddress, uint primitiveAddress, GCIndexAttributeFlags indexFlags)
        {
            ByteListExtension.AddAsBigEndian = true;
            uint primsize = Convert.ToUInt32(Math.Ceiling((decimal)primitiveSize / 32) * 32);
            List<byte> result = new List<byte>();
            result.AddValue(parameterAddress);
            result.AddValue((uint)parameters.Count);
            result.AddValue(primitiveAddress);
            result.AddValue(primsize);
            return result.ToArray();
        }
    }
}
