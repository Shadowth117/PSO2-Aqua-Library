using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary.Extra.AM2
{
    //E_OBJ is just a way to refer to the _obj.bin models within BORDER BREAK PS4
    //The are no ASCII tags for these files and their only real defining factor is their 0xE magic. Hence, E_OBJ.
    //Bad names aside, the files for these seemingly archive multiple models, similar to TXP3 archives.
    public unsafe class E_OBJ
    {
        public EOBJHeader header;
        public List<int> modelOffsets = new List<int>();
        public List<int> nameOffsets = new List<int>();
        public List<string> names = new List<string>();
        public List<EOBJModelObject> models = new List<EOBJModelObject>();

        public struct EOBJHeader
        {
            public int magic; //0xE always?
            public int modelCount;
            public int int_08;
            public int int_0C;

            public int meshPointerArrayOffset;
            public int int_14;
            public int namePointerArrayOffset;
            public int int_1C;
        }

        public class EOBJModelObject
        {
            public EOBJModelHeader header;
            public List<EOBJMeshObject> meshes = new List<EOBJMeshObject>();
        }

        public struct EOBJModelHeader
        {
            public ushort usht_00;
            public ushort usht_02;
            public int int_04;
            public float float_08;
            public Vector3 vec3_0C;

            public int meshCount;
            public int materialCount;

            public int int_20;
            public int vertexBufferOffset;
            public int structSize;
            public int int_2C;

            public int materialOffset;
            public int int_34;
            public int int_38;
            public int int_3C;

            public int int_40;
            public int int_44;
            public int int_48;
            public int int_4C;

            public int int_50;
            public int int_54;
            public int int_58;
            public int int_5C;

            public Vector3 vec3_60;
            public int int_6C;

            public int int_70;
            public int int_74;
            public int int_78;
            public int int_7C;

            public int int_80;
            public int int_84;
            public int int_88;
            public int int_8C;
        }

        public class EOBJMeshObject
        {
            public EOBJMeshHeader header;
            public List<Vector3> vertPositions = new List<Vector3>();
            public List<Vector3> vertNormals = new List<Vector3>();
            public List<Vector2> vertUvs = new List<Vector2>();
            public List<Vector2> vertUv2s = new List<Vector2>();
            public List<ushort> vertUShortData = new List<ushort>();
        }

        public struct EOBJMeshHeader
        {
            public int int_00;
            public Vector3 vec3_04;

            public int eobjMeshSubStructCount;
            public int eobjMeshSubStructOffset;
            public int int_1C;

            public int flags;
            public int int_24;
            public int vertCount;
            public int int_2C;

            public int vertPositionsOffset; //Floats
            public int int_34;
            public int vertNormalsOffset; //Half
            public int int_3C;

            public int vertUVsOffset; //Floats
            public int int_44;
            public int int_48;
            public int int_4C;

            public int vertUV2sOffset; //Floats
            public int int_54;
            public int int_58;
            public int int_5C;

            public int int_60;
            public int int_64;
            public int int_68;
            public int int_6C;

            public int int_70;
            public int int_74;
            public int int_78;
            public int int_7C;

            public int int_80;
            public int int_84;
            public int int_88;
            public int int_8C;

            public int int_90;
            public int int_94;
            public int vertShortListOffset;
            public int int_9C;

            public int int_A0;
            public int int_A4;
            public int int_A8;
            public int int_AC;

            public int int_B0;
            public int int_B4;
            public int int_B8;
            public int int_BC;

            public int int_C0;
            public int int_C4;
            public int int_C8;
            public int int_CC;

            public int int_E0;
            public int int_E4;
            public int int_E8;
            public int int_EC;

            public int int_F0;
            public int int_F4;
            public int int_F8;
            public int int_FC;

            public PSO2String meshName;
        }

        //When there's multiple of these, they should all pull from the same face index buffer, similar to ngs
        public struct EOBJMeshSubStruct
        {
            public int int_00;
            public float flt_04;
            public Vector3 vec3_08;

            public int int_14;
            public int int_18;
            public int int_1C;

            public int int_20;
            public int int_24;
            public int bonePaletteCount;
            public int int_2C;

            public int bonePaletteOffset;
            public int int_34;
            public int int_38;
            public int int_3C;

            public int int_40;
            public int faceIndicesCount;
            public int faceIndicesOffset;
            public int int_4C;

            public int int_50;
            public int int_54;
            public int int_58;
            public int int_5C;

            public int int_60;
            public int int_64;
            public int int_68;
            public int int_6C;
        }

        public class EOBJMaterialObject
        {
            public EOBJMaterial matStruct;
        }

        public struct EOBJMaterial
        {
            //TODO
            public fixed byte stringArray[0x5F8];
        }


        public E_OBJ() 
        {
            
        }

        public E_OBJ(BufferedStreamReader streamReader)
        {
            header = streamReader.Read<EOBJHeader>();

            //Read names
            streamReader.Seek(header.namePointerArrayOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.modelCount; i++)
            {
                nameOffsets.Add(streamReader.Read<int>());
                streamReader.Seek(4, System.IO.SeekOrigin.Current);
            }
            foreach(var nameOffset in nameOffsets)
            {
                streamReader.Seek(nameOffset, System.IO.SeekOrigin.Begin);
                names.Add(AquaGeneralMethods.ReadCString(streamReader));
            }

            //Read meshes
            streamReader.Seek(header.meshPointerArrayOffset, System.IO.SeekOrigin.Begin);
            for(int i = 0; i < header.modelCount; i++)
            {
                modelOffsets.Add(streamReader.Read<int>());
                streamReader.Seek(4, System.IO.SeekOrigin.Current);
            }

        }
    }
}
