using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static AquaModelLibrary.AquaObject;

namespace AquaModelLibrary.AquaMethods
{
    public unsafe class AquaObjectMethods
    {
        public static BoundingVolume GenerateBounding (List<VTXL> vertData)
        {
            BoundingVolume bounds = new BoundingVolume();
            Vector3 maxPoint = new Vector3();
            Vector3 minPoint = new Vector3();
            Vector3 difference = new Vector3();
            Vector3 center = new Vector3();
            float radius = 0;

            for (int vset = 0; vset < vertData.Count; vset++)
            {
                for(int vert = 0; vert < vertData[vset].vertPositions.Count; vert++)
                {
                    //Compare to max
                    if (maxPoint.X < vertData[vset].vertPositions[vert].X) 
                    {
                        maxPoint.X = vertData[vset].vertPositions[vert].X;
                    }
                    if (maxPoint.Y < vertData[vset].vertPositions[vert].Y)
                    {
                        maxPoint.Y = vertData[vset].vertPositions[vert].Y;
                    }
                    if (maxPoint.Z < vertData[vset].vertPositions[vert].Z)
                    {
                        maxPoint.Z = vertData[vset].vertPositions[vert].Z;
                    }

                    //Compare to min
                    if (minPoint.X > vertData[vset].vertPositions[vert].X)
                    {
                        minPoint.X = vertData[vset].vertPositions[vert].X;
                    }
                    if (minPoint.Y > vertData[vset].vertPositions[vert].Y)
                    {
                        minPoint.Y = vertData[vset].vertPositions[vert].Y;
                    }
                    if (minPoint.Z > vertData[vset].vertPositions[vert].Z)
                    {
                        minPoint.Z = vertData[vset].vertPositions[vert].Z;
                    }
                }
            }

            difference.X = Math.Abs(maxPoint.X - minPoint.X / 2);
            difference.Y = Math.Abs(maxPoint.Y - minPoint.Y / 2);
            difference.Z = Math.Abs(maxPoint.Z - minPoint.Z / 2);
            center.X = maxPoint.X - difference.X;
            center.Y = maxPoint.Y - difference.Y;
            center.Z = maxPoint.Z - difference.Z;

            //Get max radius from center
            for (int vset = 0; vset < vertData.Count; vset++)
            {
                for (int vert = 0; vert < vertData[vset].vertPositions.Count; vert++)
                {
                    float distance = Distance(center, vertData[vset].vertPositions[vert]);
                    if(distance > radius)
                    {
                        radius = distance;
                    }
                }
            }

            bounds.modelCenter = center;
            bounds.modelCenter2 = center;
            bounds.maxMinXYZDifference = difference;
            bounds.boundingRadius = radius;

            return bounds;
        }

        public static float Distance(Vector3 point1, Vector3 point2)
        {
            return (float)Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2) + Math.Pow(point2.Z - point1.Z, 2));
        }

        //Adapted from this: https://forums.cgsociety.org/t/finding-bi-normals-tangents/975005/8 
        //Binormals and tangents for each face are calculated and each face's values for a particular vertex are summed and averaged for the result before being normalized
        //Though vertex position is used, due to the nature of the normalization applied during the process, resizing is unneeded.
        public static void computeTangentSpace(AquaObject model)
        {
            for(int mesh = 0; mesh < model.vsetList.Count; mesh++)
            {

            }
        }

        public static string GetPSO2String(byte* str)
        {
            string finalText;

            //Lazily determine string end
            int end = 0;
            for (int j = 0; j < 0x20; j++)
            {
                if (str[j] == 0)
                {
                    end = j;
                    break;
                }
            }

            byte[] text = new byte[end];
            Marshal.Copy(new IntPtr(str), text, 0, end);
            finalText = System.Text.Encoding.UTF8.GetString(text);

            return finalText;
        }

        public static string GetMatName(MATE mate) => GetPSO2String(mate.matName);

        public static string GetMatOpacity(MATE mate) => GetPSO2String(mate.alphaType);

        public static List<string> GetTexListNames(AquaObject model, int tsetIndex)
        {
            List<string> textureList = new List<string>();

            TSET tset = model.tsetList[tsetIndex];

            for (int index = 0; index < 4; index++) 
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = model.tstaList[texIndex];

                    textureList.Add(GetPSO2String(tsta.texName));
                }
            }

            return textureList;
        }
    }
}
