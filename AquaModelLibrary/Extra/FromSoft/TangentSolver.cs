using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra.FromSoft
{
    //Adaption from FBX2FLVER's TangentSolver by MeowMaritus
    public class TangentSolver
    {
        public static Vector3 RotatePoint(Vector3 p, float pitch, float roll, float yaw)
        {

            Vector3 ans = new Vector3(0, 0, 0);


            var cosa = Math.Cos(yaw);
            var sina = Math.Sin(yaw);

            var cosb = Math.Cos(pitch);
            var sinb = Math.Sin(pitch);

            var cosc = Math.Cos(roll);
            var sinc = Math.Sin(roll);

            var Axx = cosa * cosb;
            var Axy = cosa * sinb * sinc - sina * cosc;
            var Axz = cosa * sinb * cosc + sina * sinc;

            var Ayx = sina * cosb;
            var Ayy = sina * sinb * sinc + cosa * cosc;
            var Ayz = sina * sinb * cosc - cosa * sinc;

            var Azx = -sinb;
            var Azy = cosb * sinc;
            var Azz = cosb * cosc;

            var px = p.X;
            var py = p.Y;
            var pz = p.Z;

            ans.X = (float)(Axx * px + Axy * py + Axz * pz);
            ans.Y = (float)(Ayx * px + Ayy * py + Ayz * pz);
            ans.Z = (float)(Azx * px + Azy * py + Azz * pz);


            return ans;
        }

        public static Vector3 Slerp(Vector3 start, Vector3 end, float percent)
        {
            // Dot product - the cosine of the angle between 2 vectors.
            float dot = Vector3.Dot(start, end);

            // Clamp it to be in the range of Acos()
            // This may be unnecessary, but floating point
            // precision can be a fickle mistress.
            if(dot > 1)
            {
                dot = 1;
            } else if(dot < -1)
            {
                dot = -1;
            }

            // Acos(dot) returns the angle between start and end,
            // And multiplying that by percent returns the angle between
            // start and the final result.
            float theta = (float)(Math.Acos(dot) * percent);
            Vector3 RelativeVec = end - start * dot;
            RelativeVec = Vector3.Normalize(RelativeVec);

            // Orthonormal basis
            // The final result.
            return ((start * (float)Math.Cos(theta)) + (RelativeVec * (float)Math.Sin(theta)));
        }

        public static void SolveTangentsDemonsSouls(SoulsFormats.FLVER0.Mesh mesh, int version)
        {
            if (mesh.Vertices?[0].Tangents.Count > 0)
            {
                var vertexIndices = mesh.Triangulate(version);

                int vertexCount = mesh.Vertices.Count;

                Vector3[] tan1 = new Vector3[vertexCount];
                Vector3[] tan2 = new Vector3[vertexCount];

                for (int a = 0; a < vertexIndices.Count; a += 3)
                {
                    int i1 = vertexIndices[a];
                    int i2 = vertexIndices[a + 1];
                    int i3 = vertexIndices[a + 2];

                    if (i1 != i2 || i2 != i3)
                    {
                        Vector3 v1 = mesh.Vertices[i1].Position;
                        Vector3 v2 = mesh.Vertices[i2].Position;
                        Vector3 v3 = mesh.Vertices[i3].Position;

                        Vector2 w1 = new Vector2(mesh.Vertices[i1].UVs[0].X, mesh.Vertices[i1].UVs[0].Y);
                        Vector2 w2 = new Vector2(mesh.Vertices[i2].UVs[0].X, mesh.Vertices[i2].UVs[0].Y);
                        Vector2 w3 = new Vector2(mesh.Vertices[i3].UVs[0].X, mesh.Vertices[i3].UVs[0].Y);

                        float x1 = v2.X - v1.X;
                        float x2 = v3.X - v1.X;
                        float y1 = v2.Y - v1.Y;
                        float y2 = v3.Y - v1.Y;
                        float z1 = v2.Z - v1.Z;
                        float z2 = v3.Z - v1.Z;

                        float s1 = w2.X - w1.X;
                        float s2 = w3.X - w1.X;
                        float t1 = w2.Y - w1.Y;
                        float t2 = w3.Y - w1.Y;

                        float r = 1.0f / (s1 * t2 - s2 * t1);

                        Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                        Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                        tan1[i1] += sdir;
                        tan1[i2] += sdir;
                        tan1[i3] += sdir;

                        tan2[i1] += tdir;
                        tan2[i2] += tdir;
                        tan2[i3] += tdir;
                    }
                }
                for (int i = 0; i < vertexCount; i++)
                {
                    Vector3 n = mesh.Vertices[i].Normal;
                    Vector3 t = tan1[i];
                    Vector3 t2 = tan2[i];
                    Vector3 t1n = Vector3.Normalize(tan1[i]);
                    Vector3 t2n = Vector3.Normalize(tan2[i]);

                    float w = ((!(Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0f)) ? 1 : (-1));
                    var tangent0 = (Vector3.Normalize(t - n * Vector3.Dot(n, t)));
                    Vector3 finalTangent0 = Vector3.Normalize(Vector3.Cross(mesh.Vertices[i].Normal,
                               new Vector3(tangent0.X,
                               tangent0.Y,
                               tangent0.Z) * w));

                    mesh.Vertices[i].Tangents[0] = new System.Numerics.Vector4(finalTangent0.X, finalTangent0.Y, finalTangent0.Z, -w);

                    var ghettoBit = Vector3.Normalize(Vector3.Lerp(tangent0, finalTangent0, 0.46f)); //This is between these two and for w/e reason, .46 or so seems the closest approximation
                    if (mesh.Vertices[i].Tangents.Count >= 2)
                    {
                        var ghettoTan = RotatePoint(new Vector3(mesh.Vertices[i].Normal.X, mesh.Vertices[i].Normal.Y, mesh.Vertices[i].Normal.Z), 0, (float)Math.PI / 2f, 0);
                        mesh.Vertices[i].Tangents[1] = new System.Numerics.Vector4(ghettoTan.X, ghettoTan.Y, ghettoTan.Z, 0);
                    }
                    if (mesh.Vertices[i].Bitangent != Vector4.Zero)
                    {
                        mesh.Vertices[i].Bitangent = new Vector4(ghettoBit, -w);
                    }

                }
            }

            return;
        }
    }
}
