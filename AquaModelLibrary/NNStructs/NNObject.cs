using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary
{
    //Naming based primarily on variable namings in Sonic 06 via an exported text file leftover from development
    public class NNObject
    {
        public struct NNS_MESHSET
        {
            public Vector3 MSST_CENTER; 
            public float MSST_RADIUS;
            public int MSST_NODE; //Used for assigning rigid weights in absence of vertex weights.
                                  //Otherwise, takes the value of the dummy bone auto generated per mesh exported. 
            public int MSST_MATRIX; //? Always observed -1
            public int MSST_MATERIAL;    //ID in material list
            public int MSST_VTXLIST;     //ID in vertex list list
            public int MSST_PRIMLIST;    //ID in tristrip list list
            public int MSST_SHADERLIST;  //ID in shader list list
        }

        //NNS Xbox vertex data. Also used for PC and PS3? In text data, type name is appended by vertex data type abbreviations of those that are used.
        /*
         * P - Position
         * W - Vertex Weights
         * 4I - 4 Vertex Weight Indices
         * N - Vertex Normal
         * C - Vertex Color
         * T - Texture Coordinates (UVs)
         * A - Vertex Tangent**
         * B - Vertex Binormal**
        
         **Binormals and tangents for each face are calculated and each face's values for a particular vertex are summed and averaged for the result before being normalized
         Though vertex position is used, due to the nature of the normalization applied during the process, resizing is unneeded.
         */
        public class NNS_VTXTYPE_XB
        {
            //While vertex data is technically stored in sequence (Position->Weights->Indices->Normals etc.), we'll store them here in separate lists.
            public List<Vector3> POS_F32_LIST = new List<Vector3>();
            public List<Vector3> WEIGHT3_F32_LIST = new List<Vector3>();
            public List<Vector4> MTXIDX4_I8_LIST = new List<Vector4>();
            public List<Vector3> NRM_F32_LIST = new List<Vector3>();
            public List<byte[]> RGBA8888_LIST = new List<byte[]>();
            public List<Vector2> ST0_F32_UV_LIST = new List<Vector2>(); //UV Coords
            public List<Vector3> TAN_F32_LIST = new List<Vector3>();
            public List<Vector3> BNRM_F32_LIST = new List<Vector3>();
        }
    }
}
