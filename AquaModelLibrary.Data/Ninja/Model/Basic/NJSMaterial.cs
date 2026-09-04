using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Drawing;
using System.Net;

namespace AquaModelLibrary.Data.Ninja.Model.Basic
{
    public enum FilterMode
    {
        PointSampled,
        Bilinear,
        Trilinear,
        Reserved
    }
    /// <summary>
    /// Handling based on SA Tools implementation https://github.com/X-Hax/sa_tools
    /// </summary>
    public class NJSMaterial
    {
        public Color AmbientColor { get; set; }
        public Color DiffuseColor { get; set; }
        public Color SpecularColor { get; set; }
        public float Exponent { get; set; }
        public int TextureID { get; set; }
        /// <summary>
        /// Ideally use sthe flag accessor fields to manipulate this instead
        /// </summary>
        public uint Flags { get; set; }

        #region Flag Accessors (user use)

        public byte UserFlags
        {
            get { return (byte)(Flags & 0x7F); }
            set { Flags = (uint)((Flags & ~0x7F) | (value & 0x7Fu)); }
        }

        public bool PickStatus
        {
            get { return (Flags & 0x80) == 0x80; }
            set { Flags = (uint)((Flags & ~0x80) | (value ? 0x80u : 0)); }
        }

        public float MipmapDAdjust
        {
            get { return ((Flags & 0xF00) >> 8) * 0.25f; }
            set
            {
                Flags = (uint)(Flags & ~0xF00) | ((uint)Math.Max(0, Math.Min(0xF, Math.Round(value / 0.25, MidpointRounding.AwayFromZero))) << 8);
            }
        }

        public bool SuperSample
        {
            get { return (Flags & 0x1000) == 0x1000; }
            set { Flags = (uint)((Flags & ~0x1000) | (value ? 0x1000u : 0)); }
        }

        public FilterMode FilterMode
        {
            get { return (FilterMode)((Flags >> 13) & 3); }
            set { Flags = (uint)((Flags & ~0x6000) | ((uint)value << 13)); }
        }

        public bool ClampV
        {
            get { return (Flags & 0x8000) == 0x8000; }
            set { Flags = (uint)((Flags & ~0x8000) | (value ? 0x8000u : 0)); }
        }

        public bool ClampU
        {
            get { return (Flags & 0x10000) == 0x10000; }
            set { Flags = (uint)((Flags & ~0x10000) | (value ? 0x10000u : 0)); }
        }

        public bool FlipV
        {
            get { return (Flags & 0x20000) == 0x20000; }
            set { Flags = (uint)((Flags & ~0x20000) | (value ? 0x20000u : 0)); }
        }

        public bool FlipU
        {
            get { return (Flags & 0x40000) == 0x40000; }
            set { Flags = (uint)((Flags & ~0x40000) | (value ? 0x40000u : 0)); }
        }

        public bool IgnoreSpecular
        {
            get { return (Flags & 0x80000) == 0x80000; }
            set { Flags = (uint)((Flags & ~0x80000) | (value ? 0x80000u : 0)); }
        }

        public bool UseAlpha
        {
            get { return (Flags & 0x100000) == 0x100000; }
            set { Flags = (uint)((Flags & ~0x100000) | (value ? 0x100000u : 0)); }
        }

        public bool UseTexture
        {
            get { return (Flags & 0x200000) == 0x200000; }
            set { Flags = (uint)((Flags & ~0x200000) | (value ? 0x200000u : 0)); }
        }

        public bool EnvironmentMap
        {
            get { return (Flags & 0x400000) == 0x400000; }
            set { Flags = (uint)((Flags & ~0x400000) | (value ? 0x400000u : 0)); }
        }

        public bool DoubleSided
        {
            get { return (Flags & 0x800000) == 0x800000; }
            set { Flags = (uint)((Flags & ~0x800000) | (value ? 0x800000u : 0)); }
        }

        public bool FlatShading
        {
            get { return (Flags & 0x1000000) == 0x1000000; }
            set { Flags = (uint)((Flags & ~0x1000000) | (value ? 0x1000000u : 0)); }
        }

        public bool IgnoreLighting
        {
            get { return (Flags & 0x2000000) == 0x2000000; }
            set { Flags = (uint)((Flags & ~0x2000000) | (value ? 0x2000000u : 0)); }
        }

        public AlphaInstruction DestinationAlpha
        {
            get { return (AlphaInstruction)((Flags >> 26) & 7); }
            set { Flags = (uint)((Flags & ~0x1C000000) | ((uint)value << 26)); }
        }

        public AlphaInstruction SourceAlpha
        {
            get { return (AlphaInstruction)((Flags >> 29) & 7); }
            set { Flags = (Flags & ~0xE0000000) | ((uint)value << 29); }
        }
        #endregion

        /// <summary>
        /// Create a new material.
        /// </summary>
        public NJSMaterial()
        {
            AmbientColor = Color.White;
            DiffuseColor = Color.White;
            SpecularColor = Color.Transparent;
            UseAlpha = true;
            UseTexture = true;
            DoubleSided = false;
            FlatShading = false;
            IgnoreLighting = false;
            IgnoreSpecular = false;
            ClampU = false;
            ClampV = false;
            FlipU = false;
            FlipV = false;
            EnvironmentMap = false;
            DestinationAlpha = AlphaInstruction.InverseSourceAlpha;
            SourceAlpha = AlphaInstruction.SourceAlpha;
        }

        public NJSMaterial(BufferedStreamReaderBE<MemoryStream> sr, bool bigEndian = false, int offset = 0, bool DX = false)
        {
            Read(sr, bigEndian, offset, DX);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr, bool bigEndian = false, int offset = 0, bool DX = false)
        {
            var temp = sr._BEReadActive;
            sr._BEReadActive = bigEndian;
            bool SADXColorReverse = sr.streamChecks.ContainsKey("SADXColorReverse") ? sr.streamChecks["SADXColorReverse"] : false;

            var colorBytes = sr.ReadBytesSeek(8);
            ReadMatColors(bigEndian, SADXColorReverse, colorBytes);
            Exponent = sr.ReadBE<float>();
            TextureID = sr.ReadBE<int>();
            Flags = sr.ReadBE<uint>();

            sr._BEReadActive = temp;
        }

        private void ReadMatColors(bool bigEndian, bool SADXColorReverse, byte[] colorBytes)
        {
            switch (bigEndian)
            {
                case true:
                    switch (SADXColorReverse)
                    {
                        case true:
                            DiffuseColor = Color.FromArgb(colorBytes[3], colorBytes[0], colorBytes[1], colorBytes[2]);
                            SpecularColor = Color.FromArgb(colorBytes[7], colorBytes[4], colorBytes[5], colorBytes[6]);
                            return;
                        case false:
                            DiffuseColor = Color.FromArgb(colorBytes[0], colorBytes[1], colorBytes[2], colorBytes[3]);
                            SpecularColor = Color.FromArgb(colorBytes[4], colorBytes[5], colorBytes[6], colorBytes[7]);
                            return;
                    }
                case false:
                    switch (SADXColorReverse)
                    {
                        case true:
                            DiffuseColor = Color.FromArgb(colorBytes[0], colorBytes[3], colorBytes[2], colorBytes[1]);
                            SpecularColor = Color.FromArgb(colorBytes[4], colorBytes[7], colorBytes[6], colorBytes[5]);
                            return;
                        case false:
                            DiffuseColor = Color.FromArgb(colorBytes[3], colorBytes[2], colorBytes[1], colorBytes[0]);
                            SpecularColor = Color.FromArgb(colorBytes[7], colorBytes[6], colorBytes[5], colorBytes[4]);
                            return;
                    }
            }
        }

        public byte[] GetBytes()
        {
            List<byte> result = new List<byte>();
            result.AddValue(DiffuseColor.ToArgb());
            result.AddValue(SpecularColor.ToArgb());
            result.AddValue(Exponent);
            result.AddValue(TextureID);
            result.AddValue(Flags);
            return result.ToArray();
        }
    }
}