using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SystemHalf;
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
            public List<EOBJMaterialObject> materials = new List<EOBJMaterialObject>();
            public int highestBone = -1;
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
            public List<EOBJMeshSubStruct> subStructs = new List<EOBJMeshSubStruct>();
            
            public List<Vector3> vertPositions = new List<Vector3>();
            public List<Vector3> vertNormals = new List<Vector3>();
            public List<Vector2> vertUvs = new List<Vector2>();
            public List<Vector2> vertUv2s = new List<Vector2>();
            public List<Vector4> vertWeights = new List<Vector4>();
            public List<int[]> vertWeightIndices = new List<int[]>();
            public List<Half> vertRigidWeightIndices = new List<Half>();

            public List<List<ushort>> bonePalettes = new List<List<ushort>>();
            public List<List<ushort>> faceLists = new List<List<ushort>>();
        }

        public struct EOBJMeshHeader
        {
            public int int_00;
            public float flt_04;
            public Vector3 vec3_08;

            public int eobjMeshSubStructCount;
            public int eobjMeshSubStructOffset;
            public int int_1C;

            public int flags;
            public int int_24;
            public int vertCount;
            public int int_2C;

            public int vertPositionsOffset; //Floats
            public int int_34;
            public int vertNormalsOffset; //Halfs
            public int int_3C;

            public int vertUV2sOffset; //Floats; Swapped with 2nd one since second typically has main UV map stuff for this game.
            public int int_44;
            public int int_48;
            public int int_4C;

            public int vertUVsOffset; //Floats
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

            public int vertWeightsOffset; //Vector4
            public int int_84;
            public int vertBoneIndicesOffset; //4 byte array
            public int int_8C;

            public int int_90;
            public int int_94;
            public int vertRigidWeightIndicesOffset;
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

            //Read models
            streamReader.Seek(header.meshPointerArrayOffset, System.IO.SeekOrigin.Begin);
            for(int i = 0; i < header.modelCount; i++)
            {
                modelOffsets.Add(streamReader.Read<int>());
                streamReader.Seek(4, System.IO.SeekOrigin.Current);
            }

            foreach(var modelOffs in modelOffsets)
            {
                streamReader.Seek(modelOffs, System.IO.SeekOrigin.Begin);
                EOBJModelObject model = new EOBJModelObject();
                model.header = streamReader.Read<EOBJModelHeader>();

                //Read meshes
                for (int i = 0; i < model.header.meshCount; i++)
                {
                    EOBJMeshObject meshObj = new EOBJMeshObject();
                    meshObj.header = streamReader.Read<EOBJMeshHeader>();
                    model.meshes.Add(meshObj);
                }
                foreach(var mesh in model.meshes)
                {
                    //Vert positions
                    if (mesh.header.vertPositionsOffset > 0)
                    {
                        streamReader.Seek(modelOffs + mesh.header.vertPositionsOffset, System.IO.SeekOrigin.Begin);
                        for(int v = 0; v < mesh.header.vertCount; v++)
                        {
                            mesh.vertPositions.Add(streamReader.Read<Vector3>());
                        }
                    }

                    //Vert normals
                    if (mesh.header.vertNormalsOffset > 0)
                    {
                        streamReader.Seek(modelOffs + mesh.header.vertNormalsOffset, System.IO.SeekOrigin.Begin);
                        for (int v = 0; v < mesh.header.vertCount; v++)
                        {
                            mesh.vertNormals.Add(new Vector3(streamReader.Read<Half>(), streamReader.Read<Half>(), streamReader.Read<Half>()));
                        }
                    }

                    //UV Coordinates
                    if (mesh.header.vertUVsOffset > 0)
                    {
                        streamReader.Seek(modelOffs + mesh.header.vertUVsOffset, System.IO.SeekOrigin.Begin);
                        for (int v = 0; v < mesh.header.vertCount; v++)
                        {
                            mesh.vertUvs.Add(streamReader.Read<Vector2>());
                        }
                    }

                    //UV Coordinates 2
                    if (mesh.header.vertUV2sOffset > 0)
                    {
                        streamReader.Seek(modelOffs + mesh.header.vertUV2sOffset, System.IO.SeekOrigin.Begin);
                        for (int v = 0; v < mesh.header.vertCount; v++)
                        {
                            mesh.vertUv2s.Add(streamReader.Read<Vector2>());
                        }
                    }

                    //Vert weights
                    if (mesh.header.vertWeightsOffset > 0)
                    {
                        streamReader.Seek(modelOffs + mesh.header.vertWeightsOffset, System.IO.SeekOrigin.Begin);
                        for (int v = 0; v < mesh.header.vertCount; v++)
                        {
                            mesh.vertWeights.Add(streamReader.Read<Vector4>());
                        }
                    }

                    //Vert weight indices
                    if (mesh.header.vertBoneIndicesOffset > 0)
                    {
                        streamReader.Seek(modelOffs + mesh.header.vertBoneIndicesOffset, System.IO.SeekOrigin.Begin);
                        for (int v = 0; v < mesh.header.vertCount; v++)
                        {
                            mesh.vertWeightIndices.Add(AquaGeneralMethods.Read4BytesToIntArray(streamReader));
                            for (int i = 0; i < mesh.vertWeightIndices[v].Length; i++)
                            {
                                if (mesh.vertWeightIndices[v][i] > model.highestBone)
                                {
                                    model.highestBone = (int)mesh.vertWeightIndices[v][i];
                                }
                            }
                            streamReader.Seek(4, System.IO.SeekOrigin.Current);
                        }
                    }

                    //Vert rigid weights
                    if (mesh.header.vertRigidWeightIndicesOffset > 0)
                    {
                        streamReader.Seek(modelOffs + mesh.header.vertRigidWeightIndicesOffset, System.IO.SeekOrigin.Begin);
                        for (int v = 0; v < mesh.header.vertCount; v++)
                        {
                            mesh.vertRigidWeightIndices.Add(streamReader.Read<Half>());
                            if (mesh.vertRigidWeightIndices[v] > model.highestBone)
                            {
                                model.highestBone = (int)mesh.vertRigidWeightIndices[v];
                            }
                        }
                    }

                    //Faces
                    if (mesh.header.eobjMeshSubStructCount > 0 && mesh.header.eobjMeshSubStructOffset > 0)
                    {
                        streamReader.Seek(modelOffs + mesh.header.eobjMeshSubStructOffset, System.IO.SeekOrigin.Begin);
                        for (int ms = 0; ms < mesh.header.eobjMeshSubStructCount; ms++)
                        {
                            mesh.subStructs.Add(streamReader.Read<EOBJMeshSubStruct>());
                        }
                        foreach(var ms in mesh.subStructs)
                        {
                            var faceList = new List<ushort>();
                            var bonePalette = new List<ushort>();
                            streamReader.Seek(modelOffs + ms.faceIndicesOffset, System.IO.SeekOrigin.Begin);
                            for (int f = 0; f < ms.faceIndicesCount; f++)
                            {
                                faceList.Add(streamReader.Read<ushort>());
                            }

                            streamReader.Seek(modelOffs + ms.bonePaletteOffset, System.IO.SeekOrigin.Begin);
                            for (int f = 0; f < ms.bonePaletteCount; f++)
                            {
                                bonePalette.Add(streamReader.Read<ushort>());
                            }

                            mesh.bonePalettes.Add(bonePalette);
                            mesh.faceLists.Add(faceList);
                        }
                    }
                }

                //Read materials
                streamReader.Seek(modelOffs + model.header.materialOffset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < model.header.materialCount; i++)
                {
                    EOBJMaterialObject matObj = new EOBJMaterialObject();
                    matObj.matStruct = streamReader.Read<EOBJMaterial>();
                    model.materials.Add(matObj);
                }

                models.Add(model);
            }
        }
    }
}
