using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary
{
    public unsafe class PRMModel
    {
        //Seems to somehow link the PRM entries together when the count isn't 0
        public PRMHeader header = new PRMHeader();
        public List<Vector3> faces = new List<Vector3>(); //These are stored as ushorts, but we'll read them in to Vector3s;
        public List<PRMVert> vertices = new List<PRMVert>();

        public struct PRMHeader
        {
            public int magic;
            public int entryCount;
            public int groupIndexCount;
            public int entryVersion;
        }

        public struct PRMType01Vert
        {
            public Vector3 pos;
            public float oftenOne;
            public fixed byte color[4];
            public Vector2 uv1;
            public Vector2 uv2;
            public Vector3 unkVector; //Padding? Never observed non zero.
        }

        public struct PRMType02Vert
        {

        }

        public struct PRMType03Vert
        {
            public Vector3 pos;
            public fixed byte color[4];
            public Vector2 uv1;
            public Vector2 uv2;
        }

        public struct PRMType04Vert
        {
            public Vector3 pos;
            public Vector3 normal;
            public fixed byte color[4];
            public Vector2 uv1;
            public Vector2 uv2;
        }

        public class PRMVert
        {
            public Vector3 pos;
            public Vector3 normal; 
            public byte[] color;
            public Vector2 uv1;
            public Vector2 uv2;

            public PRMVert()
            {
            }

            public PRMVert(PRMType01Vert vert)
            {
                pos = vert.pos;
                color = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    color[i] = vert.color[i];
                }
                uv1 = vert.uv1;
                uv2 = vert.uv2;
            }

            public PRMVert(PRMType03Vert vert)
            {
                pos = vert.pos;
                color = new byte[4];
                for(int i = 0; i < 4; i++)
                {
                    color[i] = vert.color[i];
                }
                uv1 = vert.uv1;
                uv2 = vert.uv2;
            }

            public PRMVert(PRMType04Vert vert)
            {
                pos = vert.pos;
                normal = vert.normal;
                color = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    color[i] = vert.color[i];
                }
                uv1 = vert.uv1;
                uv2 = vert.uv2;
            }

            public PRMType01Vert GetType01Vert()
            {
                PRMType01Vert vert = new PRMType01Vert();
                vert.pos = pos;
                if (color != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vert.color[i] = color[i];
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vert.color[i] = 0xFF;
                    }
                }
                vert.oftenOne = 0x1;
                vert.uv1 = uv1;
                vert.uv2 = uv2;

                return vert;
            }

            public PRMType03Vert GetType03Vert()
            {
                PRMType03Vert vert = new PRMType03Vert();
                vert.pos = pos;
                if (color != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vert.color[i] = color[i];
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vert.color[i] = 0xFF;
                    }
                }
                vert.uv1 = uv1;
                vert.uv2 = uv2;

                return vert;
            }

            public PRMType04Vert GetType04Vert()
            {
                PRMType04Vert vert = new PRMType04Vert();
                vert.pos = pos;
                vert.normal = normal;
                if(color != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vert.color[i] = color[i];
                    }
                } else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vert.color[i] = 0xFF;
                    }
                }
                vert.uv1 = uv1;
                vert.uv2 = uv2;

                return vert;
            }
        }

    }
}
