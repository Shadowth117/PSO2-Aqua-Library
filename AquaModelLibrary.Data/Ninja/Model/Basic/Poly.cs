using AquaModelLibrary.Helpers.Writers;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.Ninja.Model.Basic
{
    public enum BasicPolyType : byte
    {
        Triangles,
        Quads,
        NPoly, //We can functionally treat this as a strip
        Strips
    }
    [Serializable]
    public sealed class Triangle : Poly
    {
        public Triangle()
        {
            Indexes = new ushort[3];
        }

        public Triangle(ushort a, ushort b, ushort c)
        {
            Indexes = new ushort[3];
            Indexes[0] = a;
            Indexes[1] = b;
            Indexes[2] = c;
        }

        public Triangle(BufferedStreamReaderBE<MemoryStream> sr)
            : this()
        {
            Indexes[0] = sr.ReadBE<ushort>();
            Indexes[1] = sr.ReadBE<ushort>();
            Indexes[2] = sr.ReadBE<ushort>();
        }

        public override BasicPolyType PolyType
        {
            get { return BasicPolyType.Triangles; }
        }
    }

    [Serializable]
    public sealed class Quad : Poly
    {
        public Quad()
        {
            Indexes = new ushort[4];
        }

        public Quad(BufferedStreamReaderBE<MemoryStream> sr)
            : this()
        {
            Indexes[0] = sr.ReadBE<ushort>();
            Indexes[1] = sr.ReadBE<ushort>();
            Indexes[2] = sr.ReadBE<ushort>();
            Indexes[3] = sr.ReadBE<ushort>();
        }

        public override BasicPolyType PolyType
        {
            get { return BasicPolyType.Quads; }
        }
    }

    [Serializable]
    public sealed class Strip : Poly
    {
        public bool Reversed { get; private set; }

        public Strip(int NumVerts, bool Reverse)
        {
            Indexes = new ushort[NumVerts];
            Reversed = Reverse;
        }

        public Strip(ushort[] Verts, bool Reverse)
        {
            Indexes = Verts;
            Reversed = Reverse;
        }

        public Strip(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var temp = sr.ReadBE<ushort>();
            Indexes = new ushort[temp & 0x7FFF];
            Reversed = (temp & 0x8000) == 0x8000;

            for (int i = 0; i < Indexes.Length; i++)
            {
                Indexes[i] = sr.ReadBE<ushort>();
            }
        }

        public override int Size
        {
            get { return base.Size + 2; }
        }

        public override BasicPolyType PolyType
        {
            get { return BasicPolyType.Strips; }
        }

        public override void Write(ByteListWriter outBytes)
        {
            outBytes.AddValue((ushort)(Indexes.Length | (Reversed ? 0x8000 : 0)));
            base.Write(outBytes);
        }
    }

    [Serializable]
    public abstract class Poly : ICloneable
    {
        public ushort[] Indexes { get; protected set; }

        internal Poly()
        {
        }

        public virtual int Size
        {
            get { return Indexes.Length * 2; }
        }

        public abstract BasicPolyType PolyType { get; }

        public virtual void Write(ByteListWriter outBytes)
        {
            foreach (ushort item in Indexes)
                outBytes.AddValue(item);
        }

        public static Poly CreatePoly(BasicPolyType type)
        {
            switch (type)
            {
                case BasicPolyType.Triangles:
                    return new Triangle();
                case BasicPolyType.Quads:
                    return new Quad();
                case BasicPolyType.NPoly:
                case BasicPolyType.Strips:
                    throw new ArgumentException(
                        "Cannot create strip-type poly without additional information.\nUse Strip.Strip(int NumVerts, bool Reverse) instead.",
                        "type");
            }
            throw new ArgumentException("Unknown poly type!", "type");
        }

        public static Poly CreatePoly(BasicPolyType type, BufferedStreamReaderBE<MemoryStream> sr)
        {
            switch (type)
            {
                case BasicPolyType.Triangles:
                    return new Triangle(sr);
                case BasicPolyType.Quads:
                    return new Quad(sr);
                case BasicPolyType.NPoly:
                case BasicPolyType.Strips:
                    return new Strip(sr);
            }
            throw new ArgumentException("Unknown poly type!", "type");
        }

        object ICloneable.Clone() => Clone();

        public Poly Clone()
        {
            Poly result = (Poly)MemberwiseClone();
            Indexes = (ushort[])Indexes.Clone();
            return result;
        }
    }
}
