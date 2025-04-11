using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using System.Numerics;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;

namespace AquaModelLibrary.Data.Capcom.MonsterHunter
{
    public class MHConvert
    {
        public static void AMOAHIConvert(MHTagFile amo, MHTagFile ahi, List<string> texNames, out AquaObject aqo, out AquaNode aqn)
        {
            var modelRootSections = ((AMOModelRoot)amo.rootSection).sections;
            aqo = new AquaObject();
            aqn = new AquaNode();
            var backupBonePalette = ahi.GenerateBonePaletteFromAHI();

            if(ahi == null)
            {
                aqn = AquaNode.GenerateBasicAQN();
            } else
            {
                aqn = new AquaNode();
                var ahiList = (AHINodeList)ahi.rootSection;
                for(int i = 0; i < ahiList.sections.Count; i++)
                {
                    var section = ahiList.sections[i];
                    if(section is MHNode)
                    {
                        var node = ((MHNode)section).node;
                        Vector3 parentPos = new Vector3();
                        if(node.parentId != -1)
                        {
                            var parNd = aqn.nodeList[node.parentId];
                            parentPos.X = parNd.m4.X;
                            parentPos.Y = parNd.m4.Y;
                            parentPos.Z = parNd.m4.Z;
                        }

                        NODE nd = new NODE();
                        nd.animatedFlag = 1;
                        nd.boneName.SetString($"Node_{aqn.nodeList.Count}");
                        nd.m1 = new Vector4(1, 0, 0, 0);
                        nd.m2 = new Vector4(0, 1, 0, 0);
                        nd.m3 = new Vector4(0, 0, 1, 0);

                        var finalPos = -node.position + parentPos;
                        nd.m4 = new Vector4(finalPos.X, finalPos.Y, finalPos.Z, 1);
                        nd.parentId = node.parentId;
                        nd.nextSibling = node.nextSiblingId;
                        nd.firstChild = node.firstChildId;
                        nd.unkNode = -1;

                        aqn.nodeList.Add(nd);
                    }
                }
            }
            AMOMaterialList matList = MHTagFile.GetFirstSection<AMOMaterialList>(modelRootSections);
            AMOTextureInfo texInfoList = MHTagFile.GetFirstSection<AMOTextureInfo>(modelRootSections);
            
            List<AMOMesh> meshList = new List<AMOMesh>();
            //In theory we only ever have one AMOModel structure, but just in case
            foreach (var model in MHTagFile.GetSections<AMOModel>(modelRootSections))
            {
                foreach(var mesh in MHTagFile.GetSections<AMOMesh>(model.sections))
                {
                    meshList.Add(mesh);
                }
            }

            //Fallback in case we absolutely cannot get the names since this just defaults to ids
            if(texNames == null)
            {
                //Oh boy, we are kinda screwed if this isn't filled
                if(texInfoList != null)
                {
                    int maxTexId = 0;
                    foreach(var item in texInfoList.texInfoList)
                    {
                        maxTexId = Math.Max(item.textureId, maxTexId);
                    }

                    texNames = new List<string>();
                    for(int i = 0; i < maxTexId + 1; i++)
                    {
                        texNames.Add($"{i}.dds");
                    }
                }
            }

            for(int m = 0; m < matList.materials.Count; m++)
            {
                var mhMat = matList.materials[m];

                //Setting this to a new one means if we don't have one it'll default to 0 later
                AMOTextureInfo.TexInfo texInfo = new AMOTextureInfo.TexInfo();
                if (mhMat.texInfoIds.Count > 0)
                {
                    texInfo = texInfoList.texInfoList[mhMat.texInfoIds[0]];
                }
                GenericMaterial mat = new GenericMaterial();
                mat.matName = $"Mat_{m}";
                mat.diffuseRGBA = new Vector4(mhMat.DiffuseColor, 0);
                if(texNames.Count > texInfo.textureId)
                {
                    mat.texNames = new List<string> { texNames[texInfo.textureId] };
                } else if (texNames.Count > 0)
                {
                    mat.texNames = new List<string> { texNames[0] };
                } else
                {
                    mat.texNames = new List<string>() { "dummy.dds"};
                }
                aqo.tempMats.Add(mat);
            }

            for(int m = 0; m < meshList.Count; m++)
            {
                var mesh = meshList[m];
                var stripList = MHTagFile.GetFirstSection<AMOStripSetList>(mesh.sections);
                var bonePalette = MHTagFile.GetFirstSection<MHBonePalette>(mesh.sections);
                var matPalette = MHTagFile.GetFirstSection<MHMaterialIdPalette>(mesh.sections);
                var matStripMapping = MHTagFile.GetFirstSection<MHMaterialStripMapping>(mesh.sections);
                var posList = MHTagFile.GetFirstSection<MHVertPositions>(mesh.sections);
                var nrmList = MHTagFile.GetFirstSection<MHVertNormals>(mesh.sections);
                var uvList = MHTagFile.GetFirstSection<MHVertUVs>(mesh.sections);
                var colorList = MHTagFile.GetFirstSection<MHVertColors>(mesh.sections);
                var weightList = MHTagFile.GetFirstSection<MHVertWeights>(mesh.sections);

                if(bonePalette == null)
                {
                    bonePalette = backupBonePalette;
                }

                int stripCounter = 0; //We use this to orient the strip we're working with within the mesh's full list for material mapping 
                for(int s = 0; s < stripList.sections.Count; s++)
                {
                    MHStripSet stripSet = (MHStripSet)stripList.sections[s];
                    VTXL vtxl = new VTXL();
                    Dictionary<int, int> vertexMapping = new Dictionary<int, int>();
                    List<Vector3> indices = new List<Vector3>();
                    List<int> matIds = new List<int>();

                    if(bonePalette != null)
                    {
                        for (int i = 0; i < bonePalette.bonePalette.Count; i++)
                        {
                            vtxl.bonePalette.Add((ushort)bonePalette.bonePalette[i]);
                        }
                    }
                    void AddVert(int vertId)
                    {
                        if(!vertexMapping.ContainsKey(vertId))
                        {
                            vertexMapping[vertId] = vtxl.vertPositions.Count;
                            vtxl.vertPositions.Add(posList.positions[vertId]); //Could be null technically, but if it is the model is already screwed
                            if(nrmList != null)
                            {
                                vtxl.vertNormals.Add(nrmList.normals[vertId]);
                            }
                            if(uvList != null)
                            {
                                vtxl.uv1List.Add(uvList.uvs[vertId]);
                            }
                            if(colorList != null)
                            {
                                var color = colorList.colors[vertId];
                                byte[] byteColor = new byte[] { (byte)color.Z, (byte)color.Y, (byte)color.X, (byte)color.W };
                                vtxl.vertColors.Add(byteColor);
                            }
                            if (weightList != null)
                            {
                                var pairs = weightList.weightsPairs[vertId];
                                List<int> boneIndices = new List<int>();
                                List<float> boneWeights = new List<float>();
                                for(int i = 0; i < pairs.Count; i++)
                                {
                                    if(i == 0)
                                    {
                                        boneIndices = new List<int> { pairs[i].boneIndex };
                                        boneWeights = new List<float> { pairs[i].boneWeight };
                                    } else
                                    {
                                        boneIndices.Add(pairs[i].boneIndex);
                                        boneWeights.Add(pairs[i].boneWeight);
                                    }
                                }
                                vtxl.rawVertWeightIds.Add(boneIndices);
                                vtxl.rawVertWeights.Add(boneWeights);
                            }
                        }
                    }

                    for(int su = 0; su < stripSet.strips.Count; su++)
                    {
                        bool flip = false;
                        var strip = stripSet.strips[su];
                        for(int si = 0; si < strip.Count - 2; si++)
                        {
                            int vert0;
                            int vert1;
                            int vert2;
                            if (flip)
                            {
                                vert0 = strip[si + 2];
                                vert1 = strip[si + 1];
                                vert2 = strip[si];
                            } else
                            {
                                vert0 = strip[si];
                                vert1 = strip[si + 1];
                                vert2 = strip[si + 2];
                            }
                            AddVert(vert0);
                            AddVert(vert1);
                            AddVert(vert2);
                            matIds.Add(matPalette.materialids[matStripMapping.stripMapping[stripCounter + su]]);

                            indices.Add(new Vector3(vertexMapping[vert0], vertexMapping[vert1], vertexMapping[vert2]));
                            flip = !flip;
                        }
                    }
                    var tris = new GenericTriangles(indices, matIds);
                    aqo.vtxlList.Add(vtxl);
                    aqo.tempTris.Add(tris);

                    stripCounter += stripSet.strips.Count;
                }
            }

            aqo.ConvertToPSO2Model(rebootModel: true, useUnrms: true, baHack: false, useBiTangent: true, zeroBounds: false, useRigid: false, splitVerts: true, useHighCountFaces: true, condenseMaterials: false, forceClassicBonePalette: true);
        }
    }
}
