using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua

{    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.
    public unsafe class AquaObject : AquaCommon
    {
        /// <summary>
        /// Checks the objc for if the type is in NGS range. Default to NGS at this point.
        /// </summary>
        public bool IsNGS { get { return !(objc?.type < 0x32); } }

        public AFPBase afp;
        public OBJC objc = null;
        public List<VSET> vsetList = new List<VSET>();
        public List<VTXE> vtxeList = new List<VTXE>();
        public List<VTXL> vtxlList = new List<VTXL>();
        public List<PSET> psetList = new List<PSET>();
        public List<MESH> meshList = new List<MESH>();
        public List<MATE> mateList = new List<MATE>();
        public List<REND> rendList = new List<REND>();
        public List<SHAD> shadList = new List<SHAD>();
        public List<TSTA> tstaList = new List<TSTA>();
        public List<TSET> tsetList = new List<TSET>();
        public List<TEXF> texfList = new List<TEXF>();
        public UNRM unrms = null;
        public List<StripData> strips = new List<StripData>();

        //*** 0xC33 only
        public List<uint> bonePalette = new List<uint>();

        //Unclear the purpose of these, but when present they have a smaller count than initial mesh and psets. 
        public List<UnkStruct1> unkStruct1List = new List<UnkStruct1>();
        public List<MESH> mesh2List = new List<MESH>();
        public List<PSET> pset2List = new List<PSET>();
        public List<StripData> strips2 = new List<StripData>(); //Strip set 2 is from the same array as the first, just split differently, potentially.

        public List<int> strips3Lengths = new List<int>();
        public List<StripData> strips3 = new List<StripData>();
        public List<Vector3> unkPointArray1 = new List<Vector3>(); //Noooooooo idea what these are. Count matches the strips3Lengths count
        public List<Vector3> unkPointArray2 = new List<Vector3>();
        //***

        public bool applyNormalAveraging = false;

        //Custom model related data
        public List<GenericTriangles> tempTris = new List<GenericTriangles>();
        public List<GenericMaterial> tempMats = new List<GenericMaterial>();

        //Extra
        public List<string> meshNames = new List<string>();
        public List<string> matUnicodeNames = new List<string>();
        public List<string> texFUnicodeNames = new List<string>();

        public void CreateTrueVertWeights()
        {
            foreach (var vtxl in vtxlList)
            {
                vtxl.createTrueVertWeights();
            }
        }

        public void ChangeTexExtension(string ext)
        {
            for (int i = 0; i < texfList.Count; i++)
            {
                texFUnicodeNames[i] = Path.ChangeExtension(texFUnicodeNames[i], ext);
            }
            for (int i = 0; i < texfList.Count; i++)
            {
                texfList[i].texName.SetString(Path.ChangeExtension(texfList[i].texName.GetString(), ext));
            }
            for (int i = 0; i < tstaList.Count; i++)
            {
                tstaList[i].texName.SetString(Path.ChangeExtension(tstaList[i].texName.GetString(), ext));
            }
        }

        //0xC33 variations of the format can recycle vtxl lists for multiple meshes. Neat, but not helpful for conversion purposes.
        public int splitVSETPerMesh()
        {
            bool continueSplitting = false;
            Dictionary<int, List<int>> vsetTracker = new Dictionary<int, List<int>>(); //Key int is a VSET, value is a list of indices for each mesh that uses said VSET
            for (int meshId = 0; meshId < meshList.Count; meshId++)
            {
                if (!vsetTracker.ContainsKey(meshList[meshId].vsetIndex))
                {
                    vsetTracker.Add(meshList[meshId].vsetIndex, new List<int>() { meshId });
                }
                else
                {
                    continueSplitting = true;
                    vsetTracker[meshList[meshId].vsetIndex].Add(meshId);
                }
            }

            if (continueSplitting)
            {
                VSET[] newVsetArray = new VSET[meshList.Count];
                VTXL[] newVtxlArray = new VTXL[meshList.Count];

                //Handle instances in which there are multiple of the same VSET used.
                //VTXL and VSETs should be cloned and updated as necessary while strips should be updated to match new vertex ids (strips using the same VTXL continue from old Ids, typically)
                foreach (var key in vsetTracker.Keys)
                {
                    if (vsetTracker[key].Count > 1)
                    {
                        foreach (int meshId in vsetTracker[key])
                        {
                            if (meshList[meshId].vsetIndex >= 0 && meshList[meshId].psetIndex >= 0)
                            {
                                Dictionary<int, int> usedVerts = new Dictionary<int, int>();
                                VSET newVset = new VSET();
                                VTXL newVtxl = new VTXL();

                                int counter = 0;
                                for (int stripIndex = 0; stripIndex < strips[meshId].triStrips.Count; stripIndex++)
                                {
                                    ushort id = strips[meshId].triStrips[stripIndex];
                                    if (!usedVerts.ContainsKey(id))
                                    {
                                        VTXL.appendVertex(vtxlList[meshList[meshId].vsetIndex], newVtxl, id);
                                        usedVerts.Add(id, counter);
                                        counter++;
                                    }
                                    strips[meshId].triStrips[stripIndex] = (ushort)usedVerts[id];
                                }
                                var tempMesh = meshList[meshId];
                                tempMesh.vsetIndex = meshId;
                                meshList[meshId] = tempMesh;
                                newVsetArray[meshId] = newVset;
                                newVtxlArray[meshId] = newVtxl;
                            }
                        }
                    }
                    else
                    {
                        int meshId = vsetTracker[key][0];
                        newVsetArray[meshId] = vsetList[meshList[meshId].vsetIndex];
                        newVtxlArray[meshId] = vtxlList[meshList[meshId].vsetIndex];
                        var tempMesh = meshList[meshId];
                        tempMesh.vsetIndex = meshId;
                        meshList[meshId] = tempMesh;
                    }
                }
                vsetList = newVsetArray.ToList();
                vtxlList = newVtxlArray.ToList();
            }

            List<int> badIds = new List<int>();
            for (int i = 0; i < meshList.Count; i++)
            {
                if (meshList[i].vsetIndex < 0 || meshList[i].psetIndex < 0)
                {
                    badIds.Add(i);
                }
            }

            int badCounter = 0;
            foreach (var id in badIds)
            {
                meshList.RemoveAt(id - badCounter);
                badCounter++;
            }
            objc.meshCount = meshList.Count;
            objc.vsetCount = vsetList.Count;

            return vsetList.Count;
        }

        public int getStripIndexCount()
        {
            int indexCount = 0;
            for (int i = 0; i < strips.Count; i++)
            {
                indexCount += strips[i].triIdCount;
            }
            return indexCount;
        }

        public int getVertexCount()
        {
            int vertCount = 0;
            for (int i = 0; i < vtxlList.Count; i++)
            {
                vertCount += vtxlList[i].vertPositions.Count;
            }
            return vertCount;
        }

        /// <summary>
        /// Gets a list of GenericMaterials representing all possible material component combinations used by this model based on its MESH structs.
        /// The idea would be condensing a list of traditional materials for external usage.
        /// A list of integers which map mesh indices to GenericMaterial ids is also output.
        /// </summary>
        /// <returns>List<GenericMaterial>, out List<int></returns>
        public List<GenericMaterial> GetUniqueMaterials(out List<int> meshMatMapping)
        {
            List<GenericMaterial> mats = new List<GenericMaterial>();
            meshMatMapping = new List<int>();

            for (int i = 0; i < meshList.Count; i++)
            {
                var curMesh = meshList[i];
                for (int msh = 0; msh < meshMatMapping.Count; msh++)
                {
                    var checkMesh = meshList[msh];
                    //Mate, rend, shad, and tset define what would make up a traditional material in a 3d program
                    if (curMesh.mateIndex == checkMesh.mateIndex && curMesh.rendIndex == checkMesh.rendIndex && curMesh.shadIndex == checkMesh.shadIndex && curMesh.tsetIndex == checkMesh.tsetIndex)
                    {
                        meshMatMapping.Add(meshMatMapping[msh]);
                        break;
                    }
                }

                if (meshMatMapping.Count - 1 != i)
                {
                    var curMate = mateList[curMesh.mateIndex];
                    var curRend = rendList[curMesh.rendIndex];
                    var shadNames = GetShaderNames(curMesh.shadIndex);
                    var texNames = GetTexListNamesUnicode(curMesh.tsetIndex);
                    var texUvSets = GetTexListUVChannels(curMesh.tsetIndex);
                    GenericMaterial mat = new GenericMaterial();
                    mat.texNames = texNames;
                    mat.texUVSets = texUvSets;
                    mat.shaderNames = shadNames;
                    mat.blendType = curMate.alphaType.GetString();
                    mat.specialType = GetSpecialMatType(texNames);
                    if (matUnicodeNames.Count > curMesh.mateIndex)
                    {
                        mat.matName = matUnicodeNames[curMesh.mateIndex];
                    }
                    else
                    {
                        mat.matName = curMate.matName.GetString();
                    }
                    mat.twoSided = curRend.twosided;
                    mat.alphaCutoff = curRend.alphaCutoff;
                    mat.srcAlpha = curRend.sourceAlpha;
                    mat.destAlpha = curRend.destinationAlpha;

                    mat.diffuseRGBA = curMate.diffuseRGBA;
                    mat.unkRGBA0 = curMate.unkRGBA0;
                    mat._sRGBA = curMate._sRGBA;
                    mat.unkRGBA1 = curMate.unkRGBA1;

                    mat.reserve0 = curMate.reserve0;
                    mat.unkFloat0 = curMate.unkFloat0;
                    mat.unkFloat1 = curMate.unkFloat1;
                    mat.unkInt0 = curMate.unkInt0;
                    mat.unkInt1 = curMate.unkInt1;

                    mats.Add(mat);
                    meshMatMapping.Add(mats.Count - 1);
                }
            }

            return mats;
        }

        public void FixWeightsFromBoneCount(int maxBone = int.MaxValue)
        {
            for (int i = 0; i < vtxlList.Count; i++)
            {
                vtxlList[i].fixWeightsFromBoneCount(maxBone);
            }
        }
        public void ConvertToLegacyTypes()
        {
            for (int i = 0; i < vtxlList.Count; i++)
            {
                vtxlList[i].convertToLegacyTypes();
            }
        }

        public void AddUnfilledUVs()
        {
            for (int i = 0; i < vtxlList.Count; i++)
            {
                var vtxl = vtxlList[i];
                bool addUV1 = !(vtxl.uv1List.Count > 0);
                bool addUV2 = !(vtxl.uv2List.Count > 0);
                bool addUV3 = !(vtxl.uv3List.Count > 0);
                bool addUV4 = !(vtxl.uv4List.Count > 0);
                bool addUV5 = !(vtxl.vert0x22.Count > 0);
                bool addUV6 = !(vtxl.vert0x23.Count > 0);
                bool addUV7 = !(vtxl.vert0x24.Count > 0);
                bool addUV8 = !(vtxl.vert0x25.Count > 0);

                for (int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    if (addUV1)
                    {
                        vtxl.uv1List.Add(new Vector2());
                    }
                    if (addUV2)
                    {
                        vtxl.uv2List.Add(new Vector2());
                    }
                    if (addUV3)
                    {
                        vtxl.uv3List.Add(new Vector2());
                    }
                    if (addUV4)
                    {
                        vtxl.uv4List.Add(new Vector2());
                    }
                    if (addUV5)
                    {
                        vtxl.vert0x22.Add(new short[2]);
                    }
                    if (addUV6)
                    {
                        vtxl.vert0x23.Add(new short[2]);
                    }
                    if (addUV7)
                    {
                        vtxl.vert0x24.Add(new short[2]);
                    }
                    if (addUV8)
                    {
                        vtxl.vert0x25.Add(new short[2]);
                    }
                }
            }
        }

        /// <summary>
        /// PSO2 has a distinct rendering it does for hollow vs blendalpha materials, but sometimes the render data is changed in post to reflect something different.
        /// This method checks all instances of this and fixes them, making a new REND instance as needed.
        /// </summary>
        public void FixHollowMatNaming()
        {
            //Set wrongly assigned blendalphas to hollow for reexport purposes
            for (int i = 0; i < meshList.Count; i++)
            {
                var mesh = meshList[i];
                var mate = mateList[mesh.mateIndex];
                var rend = rendList[mesh.rendIndex];
                if ((mate.alphaType.GetString() == "opaque" || mate.alphaType.GetString() == "blendalpha") && rend.int_0C == 0 && rend.unk8 == 1 && rend.twosided == 2)
                {
                    //Account for recycled material, but separate rend
                    for (int j = 0; j < meshList.Count; j++)
                    {
                        var mesh2 = meshList[i];
                        if (mesh.mateIndex == mesh2.mateIndex && mesh.rendIndex != mesh2.rendIndex)
                        {
                            mesh.mateIndex = mateList.Count;
                            mateList.Add(mate);
                            mate = mateList[mateList.Count - 1];
                            objc.mateCount += 1;
                            meshList[i] = mesh;
                            break;
                        }
                    }

                    mate.alphaType.SetString("hollow");
                    mateList[mesh.mateIndex] = mate;
                }
            }
        }

        public static string GetSpecialMatType(List<string> names)
        {
            if (names.Count > 0)
            {
                var name = names[0];
                switch (name)
                {
                    case "pl_eye_diffuse.dds":
                        return "ey";
                    case "pl_hair_diffuse.dds":
                        switch (names[names.Count - 1])
                        {
                            case "pl_hair_noise.dds":
                                return "rhr";
                            default:
                                return "hr";
                        }
                    case "pl_face_diffuse.dds":
                        return "fc";
                    case "pl_body_diffuse.dds":
                        return "pl";
                    case "pl_body_base_diffuse.dds":
                        switch (names[names.Count - 1])
                        {
                            case "pl_body_decal.dds":
                                return "rbd_d";
                            default:
                                return "rbd";
                        }
                    case "pl_body_skin_diffuse.dds":
                        return "rbd_sk";
                    case "pl_body_outer_diffuse.dds":
                        switch (names[names.Count - 1])
                        {
                            case "pl_body_decal.dds":
                                return "rbd_ou_d";
                            default:
                                return "rbd_ou";
                        }
                    default:
                        return "";
                }

            }
            else
            {
                return "";
            }
        }

        public static List<TSTA> GetTexListTSTAs(AquaObject model, int tsetIndex)
        {
            List<TSTA> textureList = new List<TSTA>();

            //Don't try to read what's not there
            if (model.tstaList.Count == 0 || model.tstaList == null)
            {
                return textureList;
            }
            TSET tset = model.tsetList[tsetIndex];

            for (int index = 0; index < tset.tstaTexIDs.Count; index++)
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = model.tstaList[texIndex];

                    textureList.Add(tsta);
                }
            }

            return textureList;
        }

        public List<string> GetTexListNames(int tsetIndex)
        {
            List<string> textureList = new List<string>();

            //Don't try to read what's not there
            if (tstaList.Count == 0 || tstaList == null)
            {
                return textureList;
            }
            TSET tset = tsetList[tsetIndex];

            for (int index = 0; index < tset.tstaTexIDs.Count; index++)
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = tstaList[texIndex];

                    textureList.Add(tsta.texName.GetString());
                }
            }

            return textureList;
        }

        public List<string> GetTexListNamesUnicode(int tsetIndex)
        {
            List<string> textureList = new List<string>();

            //Don't try to read what's not there
            if (tstaList.Count == 0 || tstaList == null)
            {
                return textureList;
            }
            TSET tset = tsetList[tsetIndex];

            for (int index = 0; index < tset.tstaTexIDs.Count; index++)
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = tstaList[texIndex];
                    var name = tsta.texName.GetString();

                    bool skip = false;
                    foreach (var str in texFUnicodeNames)
                    {
                        if (str.StartsWith(name))
                        {
                            textureList.Add(str);
                            skip = true;
                            break;
                        }
                    }
                    if (skip == true)
                    {
                        continue;
                    }
                    textureList.Add(name);
                }
            }

            return textureList;
        }

        public List<int> GetTexListUVChannels(int tsetIndex)
        {
            List<int> uvList = new List<int>();

            //Don't try to read what's not there
            if (tstaList.Count == 0 || tstaList == null)
            {
                return uvList;
            }
            TSET tset = tsetList[tsetIndex];

            for (int index = 0; index < tset.tstaTexIDs.Count; index++)
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = tstaList[texIndex];

                    uvList.Add(tsta.modelUVSet);
                }
            }

            return uvList;
        }

        public List<string> GetShaderNames(int shadIndex)
        {
            List<string> shaderList = new List<string>();

            SHAD shad = shadList[shadIndex];

            shaderList.Add(shad.pixelShader.GetString());
            shaderList.Add(shad.vertexShader.GetString());

            return shaderList;
        }

        //To be honest I don't really know what these actually do, but this seems to generate the structure roughly the way the game's exporter does.
        //Essentially, vertices between different meshes are linked together 
        public void CalcUNRMs(AquaObject model, bool applyNormalAveraging, bool useUNRMs)
        {
            UNRM unrm = new UNRM();
            if (useUNRMs == false && applyNormalAveraging == false)
            {
                return;
            }

            //Set up a boolean array for tracking what we've gone through for this
            bool[][] meshCheckArr = new bool[model.vtxlList.Count][];
            for (int m = 0; m < model.vtxlList.Count; m++)
            {
                meshCheckArr[m] = new bool[model.vtxlList[m].vertPositions.Count];
            }

            for (int m = 0; m < model.vtxlList.Count; m++)
            {
                for (int v = 0; v < model.vtxlList[m].vertPositions.Count; v++)
                {
                    Vector3 normals = new Vector3();
                    if (model.vtxlList[m].vertNormals.Count > 0)
                    {
                        normals = model.vtxlList[m].vertNormals[v];
                    }

                    List<int> meshNum = new List<int>() { m };
                    List<int> vertId = new List<int>() { v };
                    //Loop through the other vertices to match them up
                    for (int n = 0; n < model.vtxlList.Count; n++)
                    {
                        for (int w = 0; w < model.vtxlList[n].vertPositions.Count; w++)
                        {
                            bool sameVert = (m == n && v == w);
                            if (!sameVert && model.vtxlList[n].vertPositions[w].Equals(model.vtxlList[m].vertPositions[v]) && !meshCheckArr[n][w])
                            {
                                meshCheckArr[n][w] = true;
                                meshNum.Add(n);
                                vertId.Add(w);
                                if (applyNormalAveraging && model.vtxlList[n].vertNormals.Count > 0)
                                {
                                    normals += model.vtxlList[n].vertNormals[w];
                                }
                            }
                        }
                    }
                    meshCheckArr[m][v] = true;

                    //UNRM groups are only valid if there's more than 1, ie more than one vertex linked by position.
                    if (meshNum.Count > 1)
                    {
                        unrm.vertGroupCountCount++;
                        unrm.vertCount += meshNum.Count;
                        unrm.unrmVertGroups.Add(meshNum.Count);
                        unrm.unrmMeshIds.Add(meshNum);
                        unrm.unrmVertIds.Add(vertId);
                        if (applyNormalAveraging)
                        {
                            normals = Vector3.Normalize(normals);
                            for (int i = 0; i < meshNum.Count; i++)
                            {
                                model.vtxlList[meshNum[i]].vertNormals[vertId[i]] = normals;
                            }
                        }
                    }
                }


            }

            //Only actually apply them if we choose to. This function may just be used for averaging normals.
            if (useUNRMs)
            {
                model.unrms = unrm;
            }
        }

        public AquaObject Clone()
        {
            AquaObject aqp = new AquaObject();
            aqp.afp = afp;
            aqp.objc = objc;
            aqp.vsetList = new List<VSET>(vsetList);
            aqp.vtxeList = vtxeList.ConvertAll(vtxe => vtxe.Clone()).ToList();
            aqp.vtxlList = vtxlList.ConvertAll(vtxl => vtxl.Clone()).ToList();
            aqp.psetList = new List<PSET>(psetList);
            aqp.meshList = new List<MESH>(meshList);
            aqp.mateList = new List<MATE>(mateList);
            aqp.rendList = new List<REND>(rendList);
            aqp.shadList = shadList.ConvertAll(shad => shad.Clone()).ToList();
            aqp.tstaList = new List<TSTA>(tstaList);
            aqp.tsetList = tsetList.ConvertAll(tset => tset.Clone()).ToList();
            aqp.texfList = new List<TEXF>(texfList);
            if (aqp.unrms != null)
            {
                aqp.unrms = unrms.Clone();
            }
            aqp.strips = strips.ConvertAll(stp => stp.Clone()).ToList();

            //*** 0xC33 only
            aqp.bonePalette = new List<uint>(bonePalette);

            //Unclear the purpose of these, but when present they have a smaller count than initial mesh and psets. 
            aqp.unkStruct1List = new List<UnkStruct1>(unkStruct1List);
            aqp.mesh2List = new List<MESH>(mesh2List);
            aqp.pset2List = new List<PSET>(pset2List);
            aqp.strips2 = strips2.ConvertAll(stp => stp.Clone()).ToList();

            aqp.strips3Lengths = new List<int>(strips3Lengths);
            aqp.strips3 = strips3.ConvertAll(stp => stp.Clone()).ToList();
            aqp.unkPointArray1 = new List<Vector3>(unkPointArray1); //Noooooooo idea what these are. Count matches the strips3Lengths count
            aqp.unkPointArray2 = new List<Vector3>(unkPointArray2);
            //***

            aqp.applyNormalAveraging = applyNormalAveraging;

            //Custom model related data
            aqp.tempTris = tempTris.ConvertAll(tri => tri.Clone()).ToList();
            aqp.tempMats = tempMats.ConvertAll(mat => mat.Clone()).ToList();
            aqp.texFUnicodeNames = texFUnicodeNames.ConvertAll(texf => $"{texf}").ToList();
            aqp.matUnicodeNames = texFUnicodeNames.ConvertAll(mat => $"{mat}").ToList();

            return aqp;
        }

    }
}
