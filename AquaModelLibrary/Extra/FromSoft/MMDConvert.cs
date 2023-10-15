using SoulsFormats.Formats;
using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary.Extra.FromSoft
{
    internal class MMDConvert
    {/*
        public static List<NGSAquaObject> MMDToAqua(MMD mmd)
        {
            int matCounter = 0;
            List<NGSAquaObject> aqpList = new List<NGSAquaObject>();

            for (int m = 0; m < mdl.models.Count; m++)
            {
                NGSAquaObject aqp = new NGSAquaObject();
                var model = mdl.models[m];
                for (int i = 0; i <= model.highestBone; i++)
                {
                    aqp.bonePalette.Add((uint)i);
                }
                aqp.objc.bonePaletteOffset = 1;

                for (int vertSet = 0; vertSet < model.meshes.Count; vertSet++)
                {
                    var eObjMesh = model.meshes[vertSet];

                    AquaObject.VTXL vtxl = new AquaObject.VTXL();

                    vtxl.vertPositions.AddRange(eObjMesh.vertPositions);
                    vtxl.vertNormals.AddRange(eObjMesh.vertNormals);
                    vtxl.uv1List.AddRange(eObjMesh.vertUvs);
                    vtxl.uv2List.AddRange(eObjMesh.vertUv2s);

                    if (eObjMesh.vertRigidWeightIndices.Count > 0)
                    {
                        foreach (var index in eObjMesh.vertRigidWeightIndices)
                        {
                            vtxl.vertWeightIndices.Add(new int[] { (int)index, 0, 0, 0 });
                            vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                        }
                    }
                    else
                    {
                        vtxl.vertWeightIndices.AddRange(eObjMesh.vertWeightIndices);
                        vtxl.vertWeights.AddRange(eObjMesh.vertWeights);
                    }

                    int bpCounter = 0;
                    foreach (var faceSet in eObjMesh.faceLists)
                    {
                        //Material
                        var mat = new AquaObject.GenericMaterial();
                        mat.matName = $"Material_{matCounter++}";
                        mat.texNames = new List<string>() { "test_d.dds" };
                        aqp.tempMats.Add(mat);

                        Dictionary<int, int> vertIdDict = new Dictionary<int, int>();
                        AquaObject.VTXL matVtxl = new AquaObject.VTXL();
                        AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();
                        List<Vector3> triList = new List<Vector3>();
                        for (int vertIndex = 0; vertIndex < faceSet.Count - 2; vertIndex++)
                        {
                            Vector3 tri;
                            //When index is odd, flip
                            if ((vertIndex & 1) > 0)
                            {
                                tri = new Vector3(faceSet[vertIndex + 2], faceSet[vertIndex + 1], faceSet[vertIndex]);
                            }
                            else
                            {
                                tri = new Vector3(faceSet[vertIndex], faceSet[vertIndex + 1], faceSet[vertIndex + 2]);
                            }

                            AddVertices(vtxl, vertIdDict, matVtxl, tri, out int x, out int y, out int z);

                            //Avoid degen tris
                            if (x == y || x == z || y == z)
                            {
                                continue;
                            }
                            triList.Add(new Vector3(x, y, z));
                        }
                        genMesh.triList = triList;
                        //matVtxl.bonePalette.AddRange(eObjMesh.bonePalettes[bpCounter++]);

                        //Extra
                        genMesh.vertCount = matVtxl.vertPositions.Count;
                        genMesh.matIdList = new List<int>(new int[genMesh.triList.Count]);
                        for (int j = 0; j < genMesh.matIdList.Count; j++)
                        {
                            genMesh.matIdList[j] = aqp.tempMats.Count - 1;
                        }

                        if (genMesh.vertCount > 0)
                        {
                            aqp.tempTris.Add(genMesh);
                            aqp.vtxlList.Add(matVtxl);
                            aqp.vtxeList.Add(AquaObjectMethods.ConstructClassicVTXE(matVtxl, out int vc));
                        }
                    }
                }

                aqpList.Add(aqp);
            }

            return aqpList;
        }*/
    }
}
