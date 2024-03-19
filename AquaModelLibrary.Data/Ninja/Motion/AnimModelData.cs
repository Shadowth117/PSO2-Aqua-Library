using System.Numerics;

namespace AquaModelLibrary.Data.Ninja.Motion
{
    public class AnimModelData
    {
        public Dictionary<int, Vector3> Position = new Dictionary<int, Vector3>();
        public Dictionary<int, Rotation> RotationData = new Dictionary<int, Rotation>();
        public Dictionary<int, Vector3> Scale = new Dictionary<int, Vector3>();
        public Dictionary<int, Vector3> Vector = new Dictionary<int, Vector3>();
        public Dictionary<int, Vector3[]> Vertex = new Dictionary<int, Vector3[]>();
        public Dictionary<int, Vector3[]> Normal = new Dictionary<int, Vector3[]>();
        public Dictionary<int, Vector3> Target = new Dictionary<int, Vector3>();
        public Dictionary<int, int> Roll = new Dictionary<int, int>();
        public Dictionary<int, int> Angle = new Dictionary<int, int>();
        public Dictionary<int, uint> Color = new Dictionary<int, uint>();
        public Dictionary<int, float> Intensity = new Dictionary<int, float>();
        public Dictionary<int, Spotlight> Spot = new Dictionary<int, Spotlight>();
        public Dictionary<int, Vector2> Point = new Dictionary<int, Vector2>();
        public Dictionary<int, float[]> Quaternion = new Dictionary<int, float[]>();
        public int NbKeyframesCount;
        public AnimModelData()
        {
        }

        public AnimFlags GetAnimFlags(bool billyMode)
        {
            AnimFlags flags = new AnimFlags();
            if(Position.Count > 0)
            {
                flags |= AnimFlags.Position;
            }
            if (RotationData.Count > 0)
            {
                if(billyMode)
                {
                    flags |= AnimFlags.Normal;
                } else
                {
                    flags |= AnimFlags.Rotation;
                }
            }
            if (Scale.Count > 0)
            {
                flags |= AnimFlags.Scale;
            }
            if (Vector.Count > 0)
            {
                flags |= AnimFlags.Vector;
            }
            if (Vertex.Count > 0)
            {
                flags |= AnimFlags.Vertex;
            }
            if (Normal.Count > 0)
            {
                flags |= AnimFlags.Normal;
            }
            if (Target.Count > 0)
            {
                flags |= AnimFlags.Target;
            }
            if (Roll.Count > 0)
            {
                flags |= AnimFlags.Roll;
            }
            if (Angle.Count > 0)
            {
                flags |= AnimFlags.Angle;
            }
            if (Color.Count > 0)
            {
                flags |= AnimFlags.Color;
            }
            if (Intensity.Count > 0)
            {
                flags |= AnimFlags.Intensity;
            }
            if (Spot.Count > 0)
            {
                flags |= AnimFlags.Spot;
            }
            if (Point.Count > 0)
            {
                flags |= AnimFlags.Point;
            }
            if (Quaternion.Count > 0)
            {
                flags |= AnimFlags.Quaternion;
            }

            return flags;
        }

        public Vector3 GetPosition(float frame)
        {
            if (Math.Floor(frame) == frame && Position.ContainsKey((int)Math.Floor(frame)))
                return Position[(int)Math.Floor(frame)];
            int f1 = 0;
            int f2 = 0;
            List<int> keys = new List<int>();
            foreach (int k in Position.Keys)
                keys.Add(k);
            for (int i = 0; i < Position.Count; i++)
            {
                if (keys[i] < frame)
                    f1 = keys[i];
            }
            for (int i = Position.Count - 1; i >= 0; i--)
            {
                if (keys[i] > frame)
                    f2 = keys[i];
            }
            int diff = f2 != 0 ? (f2 - f1) : NbKeyframesCount - f1 + keys[0];
            int f2z = f2 != 0 ? f2 : keys[0];
            Vector3 val = new Vector3()
            {
                X = ((Position[f2z].X - Position[f1].X) / diff * (frame - f1)) + Position[f1].X,
                Y = ((Position[f2z].Y - Position[f1].Y) / diff * (frame - f1)) + Position[f1].Y,
                Z = ((Position[f2z].Z - Position[f1].Z) / diff * (frame - f1)) + Position[f1].Z
            };
            return val;
        }

        public Rotation GetRotation(float frame)
        {
            if (Math.Floor(frame) == frame && RotationData.ContainsKey((int)Math.Floor(frame)))
                return RotationData[(int)Math.Floor(frame)];
            int f1 = 0;
            int f2 = 0;
            List<int> keys = new List<int>();
            foreach (int k in RotationData.Keys)
                keys.Add(k);
            for (int i = 0; i < RotationData.Count; i++)
            {
                if (keys[i] < frame)
                    f1 = keys[i];
            }
            for (int i = RotationData.Count - 1; i >= 0; i--)
            {
                if (keys[i] > frame)
                    f2 = keys[i];
            }
            int diff = f2 != 0 ? (f2 - f1) : NbKeyframesCount - f1 + keys[0];
            int f2z = f2 != 0 ? f2 : keys[0];
            Rotation val = new Rotation()
            {
                X = (int)Math.Round(((RotationData[f2z].X - RotationData[f1].X) / (double)diff * (frame - f1)) + RotationData[f1].X, MidpointRounding.AwayFromZero),
                Y = (int)Math.Round(((RotationData[f2z].Y - RotationData[f1].Y) / (double)diff * (frame - f1)) + RotationData[f1].Y, MidpointRounding.AwayFromZero),
                Z = (int)Math.Round(((RotationData[f2z].Z - RotationData[f1].Z) / (double)diff * (frame - f1)) + RotationData[f1].Z, MidpointRounding.AwayFromZero)
            };
            return val;
        }

        public Vector3 GetScale(float frame)
        {
            if (Math.Floor(frame) == frame && Scale.ContainsKey((int)Math.Floor(frame)))
                return Scale[(int)Math.Floor(frame)];
            int f1 = 0;
            int f2 = 0;
            List<int> keys = new List<int>();
            foreach (int k in Scale.Keys)
                keys.Add(k);
            for (int i = 0; i < Scale.Count; i++)
            {
                if (keys[i] < frame)
                    f1 = keys[i];
            }
            for (int i = Scale.Count - 1; i >= 0; i--)
            {
                if (keys[i] > frame)
                    f2 = keys[i];
            }
            int diff = f2 != 0 ? (f2 - f1) : NbKeyframesCount - f1 + keys[0];
            int f2z = f2 != 0 ? f2 : keys[0];
            Vector3 val = new Vector3()
            {
                X = ((Scale[f2z].X - Scale[f1].X) / diff * (frame - f1)) + Scale[f1].X,
                Y = ((Scale[f2z].Y - Scale[f1].Y) / diff * (frame - f1)) + Scale[f1].Y,
                Z = ((Scale[f2z].Z - Scale[f1].Z) / diff * (frame - f1)) + Scale[f1].Z
            };
            return val;
        }

        public Vector3 GetVector(float frame)
        {
            if (Math.Floor(frame) == frame && Vector.ContainsKey((int)Math.Floor(frame)))
                return Vector[(int)Math.Floor(frame)];
            int f1 = 0;
            int f2 = 0;
            List<int> keys = new List<int>();
            foreach (int k in Vector.Keys)
                keys.Add(k);
            for (int i = 0; i < Vector.Count; i++)
            {
                if (keys[i] < frame)
                    f1 = keys[i];
            }
            for (int i = Vector.Count - 1; i >= 0; i--)
            {
                if (keys[i] > frame)
                    f2 = keys[i];
            }
            int diff = f2 != 0 ? (f2 - f1) : NbKeyframesCount - f1 + keys[0];
            int f2z = f2 != 0 ? f2 : keys[0];
            Vector3 val = new Vector3()
            {
                X = ((Vector[f2z].X - Vector[f1].X) / diff * (frame - f1)) + Vector[f1].X,
                Y = ((Vector[f2z].Y - Vector[f1].Y) / diff * (frame - f1)) + Vector[f1].Y,
                Z = ((Vector[f2z].Z - Vector[f1].Z) / diff * (frame - f1)) + Vector[f1].Z
            };
            return val;
        }

        public Vector3[] GetVertex(float frame)
        {
            if (Math.Floor(frame) == frame && Vertex.ContainsKey((int)Math.Floor(frame)))
                return Vertex[(int)Math.Floor(frame)];
            int f1 = 0;
            int f2 = 0;
            List<int> keys = new List<int>();
            foreach (int k in Vertex.Keys)
                keys.Add(k);
            for (int i = 0; i < Vertex.Count; i++)
            {
                if (keys[i] < frame)
                    f1 = keys[i];
            }
            for (int i = Vertex.Count - 1; i >= 0; i--)
            {
                if (keys[i] > frame)
                    f2 = keys[i];
            }
            int diff = f2 != 0 ? (f2 - f1) : NbKeyframesCount - f1 + keys[0];
            int f2z = f2 != 0 ? f2 : keys[0];
            Vector3[] result = new Vector3[Vertex[f1].Length];
            for (int i = 0; i < Vertex[f1].Length; i++)
                result[i] = new Vector3()
                {
                    X = ((Vertex[f2z][i].X - Vertex[f1][i].X) / diff * (frame - f1)) + Vertex[f1][i].X,
                    Y = ((Vertex[f2z][i].Y - Vertex[f1][i].Y) / diff * (frame - f1)) + Vertex[f1][i].Y,
                    Z = ((Vertex[f2z][i].Z - Vertex[f1][i].Z) / diff * (frame - f1)) + Vertex[f1][i].Z
                };
            return result;
        }

        public Vector3[] GetNormal(float frame)
        {
            if (Math.Floor(frame) == frame && Normal.ContainsKey((int)Math.Floor(frame)))
                return Normal[(int)Math.Floor(frame)];
            int f1 = 0;
            int f2 = 0;
            List<int> keys = new List<int>();
            foreach (int k in Normal.Keys)
                keys.Add(k);
            for (int i = 0; i < Normal.Count; i++)
            {
                if (keys[i] < frame)
                    f1 = keys[i];
            }
            for (int i = Normal.Count - 1; i >= 0; i--)
            {
                if (keys[i] > frame)
                    f2 = keys[i];
            }
            int diff = f2 != 0 ? (f2 - f1) : NbKeyframesCount - f1 + keys[0];
            int f2z = f2 != 0 ? f2 : keys[0];
            Vector3[] result = new Vector3[Normal[f1].Length];
            for (int i = 0; i < Normal[f1].Length; i++)
                result[i] = new Vector3()
                {
                    X = ((Normal[f2z][i].X - Normal[f1][i].X) / diff * (frame - f1)) + Normal[f1][i].X,
                    Y = ((Normal[f2z][i].Y - Normal[f1][i].Y) / diff * (frame - f1)) + Normal[f1][i].Y,
                    Z = ((Normal[f2z][i].Z - Normal[f1][i].Z) / diff * (frame - f1)) + Normal[f1][i].Z
                };
            return result;
        }

        public Vector3 GetTarget(float frame)
        {
            if (Math.Floor(frame) == frame && Target.ContainsKey((int)Math.Floor(frame)))
                return Target[(int)Math.Floor(frame)];
            int f1 = 0;
            int f2 = 0;
            List<int> keys = new List<int>();
            foreach (int k in Target.Keys)
                keys.Add(k);
            for (int i = 0; i < Target.Count; i++)
            {
                if (keys[i] < frame)
                    f1 = keys[i];
            }
            for (int i = Target.Count - 1; i >= 0; i--)
            {
                if (keys[i] > frame)
                    f2 = keys[i];
            }
            int diff = f2 != 0 ? (f2 - f1) : NbKeyframesCount - f1 + keys[0];
            int f2z = f2 != 0 ? f2 : keys[0];
            Vector3 val = new Vector3()
            {
                X = ((Target[f2z].X - Target[f1].X) / diff * (frame - f1)) + Target[f1].X,
                Y = ((Target[f2z].Y - Target[f1].Y) / diff * (frame - f1)) + Target[f1].Y,
                Z = ((Target[f2z].Z - Target[f1].Z) / diff * (frame - f1)) + Target[f1].Z
            };
            return val;
        }

        public int GetRoll(float frame)
        {
            if (Math.Floor(frame) == frame && Roll.ContainsKey((int)Math.Floor(frame)))
                return Roll[(int)Math.Floor(frame)];
            int f1 = 0;
            int f2 = 0;
            List<int> keys = new List<int>();
            foreach (int k in Roll.Keys)
                keys.Add(k);
            for (int i = 0; i < Roll.Count; i++)
            {
                if (keys[i] < frame)
                    f1 = keys[i];
            }
            for (int i = Roll.Count - 1; i >= 0; i--)
            {
                if (keys[i] > frame)
                    f2 = keys[i];
            }
            int diff = f2 != 0 ? (f2 - f1) : NbKeyframesCount - f1 + keys[0];
            int f2z = f2 != 0 ? f2 : keys[0];
            return (int)Math.Round((((Roll[f2z] - Roll[f1]) / (double)diff) * (frame - f1)) + Roll[f1], MidpointRounding.AwayFromZero);
        }

        public int GetAngle(float frame)
        {
            if (Math.Floor(frame) == frame && Angle.ContainsKey((int)Math.Floor(frame)))
                return Angle[(int)Math.Floor(frame)];
            int f1 = 0;
            int f2 = 0;
            List<int> keys = new List<int>();
            foreach (int k in Angle.Keys)
                keys.Add(k);
            for (int i = 0; i < Angle.Count; i++)
            {
                if (keys[i] < frame)
                    f1 = keys[i];
            }
            for (int i = Angle.Count - 1; i >= 0; i--)
            {
                if (keys[i] > frame)
                    f2 = keys[i];
            }
            int diff = f2 != 0 ? (f2 - f1) : NbKeyframesCount - f1 + keys[0];
            int f2z = f2 != 0 ? f2 : keys[0];
            return (int)Math.Round((((Angle[f2z] - Angle[f1]) / (double)diff) * (frame - f1)) + Angle[f1], MidpointRounding.AwayFromZero);
        }

        public Rotation GetQuaternion(float frame)
        {
            if (Math.Floor(frame) == frame && Quaternion.ContainsKey((int)Math.Floor(frame)))
            {
                return RotFromQuat(FloatsAsQuat(Quaternion[(int)Math.Floor(frame)]));
            }
            int f1 = 0;
            int f2 = 0;
            List<int> keys = new List<int>();
            foreach (int k in Quaternion.Keys)
                keys.Add(k);
            for (int i = 0; i < Quaternion.Count; i++)
            {
                if (keys[i] < frame)
                    f1 = keys[i];
            }
            for (int i = Quaternion.Count - 1; i >= 0; i--)
            {
                if (keys[i] > frame)
                    f2 = keys[i];
            }
            int diff = f2 != 0 ? (f2 - f1) : NbKeyframesCount - f1 + keys[0];
            int f2z = f2 != 0 ? f2 : keys[0];

            return RotFromQuat(System.Numerics.Quaternion.Slerp(FloatsAsQuat(Quaternion[f1]), FloatsAsQuat(Quaternion[f2z]), diff * (frame - f1)));
        }

        public System.Numerics.Quaternion FloatsAsQuat(float[] ninjaQuats)
        {
            return new System.Numerics.Quaternion(ninjaQuats[1], ninjaQuats[2], ninjaQuats[3], ninjaQuats[0]);
        }

        public Rotation RotFromQuat(System.Numerics.Quaternion quat)
        {
            float X;
            float Y;
            float Z;

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (quat.W * quat.X + quat.Y * quat.Z);
            double cosr_cosp = 1 - 2 * (quat.X * quat.X + quat.Y * quat.Y);
            X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (quat.W * quat.Y - quat.Z * quat.X);
            if (Math.Abs(sinp) >= 1)
                Y = (float)CopySign(Math.PI / 2, sinp); // use 90 degrees if out of range
            else
                Y = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (quat.W * quat.Z + quat.X * quat.Y);
            double cosy_cosp = 1 - 2 * (quat.Y * quat.Y + quat.Z * quat.Z);
            Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return new Rotation(Rotation.RadToBAMS(X), Rotation.RadToBAMS(Y), Rotation.RadToBAMS(Z));
        }
        public static double CopySign(double valMain, double valSign)
        {
            double final = Math.Abs(valMain);
            if (valSign >= 0)
            {
                return final;
            }
            else
            {
                return -final;
            }
        }
    }
}
