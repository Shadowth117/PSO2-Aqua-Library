﻿using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.ComponentModel;
using System.Globalization;

//From SA Tools
namespace AquaModelLibrary.Data.Ninja.Motion
{
    [TypeConverter(typeof(RotationConverter))]
    public class Rotation : ICloneable, IEquatable<Rotation>
    {
        [Browsable(false)]
        public int X { get; set; }
        [Browsable(false)]
        public int Y { get; set; }
        [Browsable(false)]
        public int Z { get; set; }

        [DisplayName("X")]
        public string XStr
        {
            get { return X.ToString("X"); }
            set { X = int.Parse(value, NumberStyles.HexNumber); }
        }
        [DisplayName("Y")]
        public string YStr
        {
            get { return Y.ToString("X"); }
            set { Y = int.Parse(value, NumberStyles.HexNumber); }
        }
        [DisplayName("Z")]
        public string ZStr
        {
            get { return Z.ToString("X"); }
            set { Z = int.Parse(value, NumberStyles.HexNumber); }
        }

        [Browsable(false)]
        public float XDeg
        {
            get { return BAMSToDeg(X); }
            set { X = DegToBAMS(value); }
        }
        [Browsable(false)]
        public float YDeg
        {
            get { return BAMSToDeg(Y); }
            set { Y = DegToBAMS(value); }
        }
        [Browsable(false)]
        public float ZDeg
        {
            get { return BAMSToDeg(Z); }
            set { Z = DegToBAMS(value); }
        }
        [Browsable(false)]
        public System.Numerics.Vector3 Deg
        {
            get { return new System.Numerics.Vector3(XDeg, YDeg, ZDeg); }
            set { XDeg = value.X; YDeg = value.Y; ZDeg = value.Z; }
        }

        [Browsable(false)]
        public static int Size
        {
            get { return 12; }
        }

        public Rotation()
        {
        }

        public Rotation(BufferedStreamReaderBE<MemoryStream> sr)
        {
            X = sr.ReadBE<int>();
            Y = sr.ReadBE<int>();
            Z = sr.ReadBE<int>();
        }

        public Rotation(string data)
        {
            string[] a = data.Split(',');
            XStr = a[0];
            YStr = a[1];
            ZStr = a[2];
        }

        public Rotation(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public byte[] GetBytes(bool bigEndian)
        {
            List<byte> result = new List<byte>();
            ByteListExtension.AddAsBigEndian = bigEndian;
            result.AddValue(X);
            result.AddValue(Y);
            result.AddValue(Z);
            return result.ToArray();
        }

        public override string ToString()
        {
            return XStr + ", " + YStr + ", " + ZStr;
        }

        public int[] ToArray()
        {
            int[] result = new int[3];
            result[0] = X;
            result[1] = Y;
            result[2] = Z;
            return result;
        }

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        return;
                    case 1:
                        Y = value;
                        return;
                    case 2:
                        Z = value;
                        return;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public static float BAMSToDeg(int BAMS)
        {
            return (float)(BAMS / (65536 / 360.0));
        }

        public static int DegToBAMS(float deg)
        {
            return (int)(deg * (65536 / 360.0));
        }

        public static float BAMSToRad(float rad)
        {
            return (float)(rad / (65536 / (2 * Math.PI)));
        }

        public static int RadToBAMS(float rad)
        {
            return (int)(rad * (65536 / (2 * Math.PI)));
        }

        [Browsable(false)]
        public bool IsEmpty
        {
            get { return X == 0 && Y == 0 && Z == 0; }
        }

        public override bool Equals(object obj)
        {
            if (obj is Rotation)
                return Equals((Rotation)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public bool Equals(Rotation other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        object ICloneable.Clone() => Clone();

        public Rotation Clone() => new Rotation(X, Y, Z);
    }

    public class RotationConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Rotation))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Rotation)
                return ((Rotation)value).ToString();
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
                return new Rotation((string)value);
            return base.ConvertFrom(context, culture, value);
        }
    }
}
