using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using SoulsFormats.KF4;
using SoulsFormats.Other;
using SoulsFormats.Otogi2;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static AquaModelLibrary.AquaCommon;
using static SoulsFormats.DRB.Shape;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AquaModelLibrary.Noesis
{
    public unsafe class AMLNoesis
    {
        public static int NOESIS_PLUGIN_VERSION = 3;
        public static string g_pPluginName = "PSO2";
        public static string g_pPluginDesc = "Phantasy Star Online 2: New Genesis - aqp, aqo, trp, tro, prm, prx, tcb| Phantasy Star Nova - axs, aif| Phantasy Star Universe .xnj, .xnr (model)| Demon's Souls (2020) .cmsh| plugin by Shadowth117";
        public static GCHandle dataCheckHandle;
        public delegate bool AquaModelCheck(byte* fileBuffer, nint bufferLen, noeRAPI_s* rapi);
        public delegate IntPtr AquaModelLoad(byte* fileBuffer, nint bufferLen, ref int numMdl, noeRAPI_s* rapi);
        public static AquaModelCheck AquaModelCheckDel;
        public static AquaModelLoad AquaModelLoadDel;

        //=========================================
        //Main Noesis interface
        //=========================================

        //called by Noesis to init the plugin
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "NPAPI_Init")]
        public static bool NPAPI_Init(mathImpFn_s* mathfn, noePluginFn_s* noepfn)
        {
            var api = new NoesisFunctions(mathfn, noepfn);

            /* API Test
            var version = api.nPAPI_GetAPIVersion();
            File.WriteAllBytes("C:\\apiVer.bin", BitConverter.GetBytes(version));
            */

            //Descriptions
            var descPSO2 = Marshal.StringToBSTR("PSO2 model (.aqp, .aqo, .trp, .tro)");
            var descPSO2Simple = Marshal.StringToBSTR("PSO2 simple model (.prm, .prx)");

            //Types
            var typesPSO2 = Marshal.StringToBSTR(".aqp;.aqo;.trp;.tro");
            var typesPSO2Simple = Marshal.StringToBSTR(".prm;.prx");

            AquaModelCheckDel = AquaModelCheckFn;
            dataCheckHandle = GCHandle.Alloc(AquaModelCheckDel, GCHandleType.Pinned);

            AquaModelLoadDel = AquaModelLoadFn;
            dataCheckHandle = GCHandle.Alloc(AquaModelLoadDel, GCHandleType.Pinned);

            //pso2 and ngs models
            //File.WriteAllBytes("C:\\prenpapiregister.bin", new byte[0]);
            var fh = api.npAPI_Register((byte*)descPSO2.ToPointer(), (byte*)typesPSO2.ToPointer());

            if(fh == 0)
            {
                return false;
            }
            //set the data handlers for this format
            File.WriteAllBytes("C:\\preTypeCheck.bin", new byte[0]);
            api.nPAPI_SetTypeHandler_TypeCheck(fh, Marshal.GetFunctionPointerForDelegate(AquaModelCheckDel));
            File.WriteAllBytes("C:\\postTypeCheck.bin", BitConverter.GetBytes(fh) );

            api.nPAPI_SetTypeHandler_LoadModel(fh, Marshal.GetFunctionPointerForDelegate(AquaModelLoadDel));

            return true;
        }

        //called by Noesis before the plugin is freed
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "NPAPI_Shutdown")]
        public static void NPAPI_Shutdown()
        {
            return;
        }

        //returns current version
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "NPAPI_GetPluginVer")]
        public static int NPAPI_GetPluginVer()
        {
            return NOESIS_PLUGIN_VERSION;
        }

        //copies off plugin info strings
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "NPAPI_GetPluginInfo")]
        public static bool NPAPI_GetPluginInfo(noePluginInfo_s* infOut)
        {
            StringCopy(g_pPluginName, 64, infOut->pluginName);
            StringCopy(g_pPluginDesc, 512, infOut->pluginDesc);
            return true;
        }

        public static bool DummyMethod(byte* fileBuffer, nint bufferLen, noeRAPI_s* rapi)
        {
            return true;
        }

        public static bool AquaModelCheckFn(byte* fileBuffer, nint bufferLen, noeRAPI_s* rapi)
        {
            RAPIObj rapiObj = new RAPIObj(rapi);
            string filename = GetWideCharString(rapiObj.noesis_GetInputNameW());
            
            using (Stream stream = new UnmanagedMemoryStream(fileBuffer, bufferLen))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                var result = AquaGeneralMethods.ReadAquaHeader(streamReader, Path.GetExtension(filename), out int offset);

                if (result == "NIFL" || result == "VTBF")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static IntPtr AquaModelLoadFn(byte* fileBuffer, nint bufferLen, ref int numMdl, noeRAPI_s* rapi)
        {
            bool isPso2Model = true;
            RAPIObj rapiObj = new RAPIObj(rapi);
            IntPtr ctx = rapiObj.rpgCreateContext();

            AquaNode aqn = null;
            string filename = GetWideCharString(rapiObj.noesis_GetInputNameW());
            string aqnFilename = "";

            modelBone_s* bones = null;
            noesisTex_s*[] texList = null; //noesisTex_s
            noesisMaterial_s*[] matList = null; //noesisMaterial_s

            switch (Path.GetExtension(filename))
            {
                case ".aqp":
                case ".aqo":
                    aqnFilename = Path.ChangeExtension(filename, ".aqn");
                    break;
                case ".trp":
                case ".tro":
                    aqnFilename = Path.ChangeExtension(filename, ".trn");
                    break;
            }
            AquaUtil aquaUtil = new AquaUtil();

            byte[] fileArr = new byte[bufferLen];
            Marshal.Copy((nint)fileBuffer, fileArr, 0, (int)bufferLen);
            aquaUtil.BeginReadModel(fileArr);
            
            //Adjust model to be only 1 material per vertex set for NGS models and adjust Material type to match shading handling
            var model = aquaUtil.aquaModels[0].models[0];
            if (model.objc.type > 0xC32)
            {
                model.splitVSETPerMesh();
            }
            model.FixHollowMatNaming();

            //Handle bones if they exist
            bool setBoneData = false;
            if(File.Exists(aqnFilename))
            {
                setBoneData = true;
                aquaUtil.ReadBones(aqnFilename);
                aqn = aquaUtil.aquaBones[0];
                bones = rapiObj.noesis_AllocBones(aqn.nodeList.Count);

                //If names aren't unique (should only happen in a mod, but legal ingame afaik), note to add ids into name
                List<string> strings = new List<string>();
                bool addId = false;
                foreach (var b in aqn.nodeList)
                {
                    if(strings.IndexOf(b.boneName.GetString()) > -1)
                    {
                        addId = true;
                        break;
                    }
                }

                for (int i = 0; i < aqn.nodeList.Count; i++)
                {
                    var aqBone = aqn.nodeList[i];
                    var mat = aqBone.GetInverseBindPoseMatrixInverted();
                    modelBone_s bone = new modelBone_s();
                    bone.mat = new modelMatrix_s() { x1 = new Vector3(mat.M11, mat.M12, mat.M13), x2 = new Vector3(mat.M21, mat.M22, mat.M23), x3 = new Vector3(mat.M31, mat.M32, mat.M33), o = new Vector3(mat.M41, mat.M42, mat.M43)};
                    bone.eData.parent = aqBone.parentId == -1 ? 0 : (nint)bones + (sizeof(modelBone_s) * aqBone.parentId);

                    //Set up name
                    var name = aqBone.boneName.GetBytes();
                    var idStr = Encoding.UTF8.GetBytes($"({i})_");
                    if (addId)
                    {
                        for(int j = 0; j < idStr.Length; j++)
                        {
                            bone.name[j] = idStr[j];
                        }
                    }
                    for(int j = 0; j < name.Length; j++)
                    {
                        bone.name[j + idStr.Length] = name[j];
                    }
                        
                    bones[i] = bone;
                }
            }

            //Set up materials
            var materials = model.GetUniqueMaterials(out List<int> meshMatMapping);
            texList = new noesisTex_s*[materials.Count];
            matList = new noesisMaterial_s*[materials.Count];
            for (int i = 0; i < materials.Count; i++)
            {
                var aqMat = materials[i];
                
                noesisMaterial_s* mat = rapiObj.noesis_GetMaterialList(1, true);
                mat->name = rapiObj.noesis_PooledString(Encoding.UTF8.GetBytes(aqMat.matName));
                //Don't overload this if the ngs hair ones comes into play
                mat->diffuse = aqMat.diffuseRGBA.Length() > 2 ? new Vector4(1,1,1,1) : aqMat.diffuseRGBA;
                mat->alphaTest = (float)aqMat.alphaCutoff / 255;
                if(mat->alphaTest > 1)
                {
                    mat->alphaTest = 1;
                }
                mat->blendSrc = aqMat.srcAlpha;
                mat->blendDst = aqMat.destAlpha;

                //Handle player texture assignments
                if (aqMat.texNames.Count > 0 && aqMat.texNames[0].EndsWith("_diffuse.dds") && isPso2Model)
                {
                    var dir = Path.GetDirectoryName(filename);
                    var texFiles = Directory.GetFiles(dir, ".dds");
                    foreach(var texFile in texFiles)
                    {
                        if(texFile.EndsWith("_d.dds"))
                        {
                            aqMat.texNames[0] = Path.GetFileName(texFile);
                        }
                    }
                }

                var texBytes = File.ReadAllBytes(aqMat.texNames[0]);
                nint texBuffer = rapiObj.noesis_PooledAlloc(texBytes.Length);
                Marshal.Copy(texBytes, 0, texBuffer, texBytes.Length);
                noesisTex_s* tex = rapiObj.noesis_LoadTexByHandler((byte*)texBuffer, texBytes.Length, Encoding.UTF8.GetBytes(".dds"));
                var texFilenameBytes = Encoding.UTF8.GetBytes($"{aqMat.texNames[0]}");
                tex->filename = rapiObj.noesis_PooledString(texFilenameBytes);
                
                texList[i] = tex;
                matList[i] = mat;
            }

            if(materials.Count > 0)
            {
                noesisMatData_s* md = rapiObj.noesis_GetMatData(matList, matList.Length, texList, texList.Length);
                rapiObj.rpgSetExData_Materials(md);
            }

            //This has to be set down here or Noesis freaks out
            if(setBoneData)
            {
#pragma warning disable CS8602
                rapiObj.rpgSetExData_Bones(bones, aqn.nodeList.Count);
#pragma warning restore CS8602
            }

            //Handle mesh data
            for (int i = 0; i < model.meshList.Count; i++)
            {
                var mesh = model.meshList[i];
                var vtxl = model.vtxlList[mesh.vsetIndex];
                var faces = model.strips[mesh.psetIndex];
                rapiObj.rpgSetMaterialIndex(i);

                nint[] bonePalette;
                if (model.objc.bonePaletteOffset > 0)
                {
                    bonePalette = model.bonePalette.ToArray();
                }
                else
                {
                    var bonePaletteTemp = new List<uint>();
                    for (int bn = 0; bn < vtxl.bonePalette.Count; bn++)
                    {
                        bonePaletteTemp.Add(vtxl.bonePalette[bn]);
                    }
                    bonePalette = bonePaletteTemp.ToArray();
                }
                rapiObj.rpgSetBoneMap(bonePalette);

                if (vtxl.vertPositions.Count > 0)
                {
                    rapi->rpgBindPositionBuffer(vtxl.vertPositions.ToArray(), rpgeoDataType_e.RPGEODATA_FLOAT, 0xC);
                }
                if (vtxl.vertNormals.Count > 0)
                {
                    rapi->rpgBindNormalBuffer(vtxl.vertNormals.ToArray(), rpgeoDataType_e.RPGEODATA_FLOAT, 0xC);
                }
                if (vtxl.vertColors.Count > 0)
                {
                    byte[] vertColorsArr = new byte[vtxl.vertColors.Count * 4];
                    for(int j = 0; j < vtxl.vertColors.Count; j+=4) { vertColorsArr[j] = vtxl.vertColors[j][0]; vertColorsArr[j + 1] = vtxl.vertColors[j + 1][1]; vertColorsArr[j + 2] = vtxl.vertColors[j + 2][2]; vertColorsArr[j + 3] = vtxl.vertColors[j + 3][3]; }
                    rapi->rpgBindColorBuffer(vertColorsArr, rpgeoDataType_e.RPGEODATA_UBYTE, 4, 4);
                }
                if (vtxl.uv1List.Count > 0)
                {
                    rapi->rpgBindUV1Buffer(vtxl.uv1List.ToArray(), rpgeoDataType_e.RPGEODATA_FLOAT, 8);
                }
                if (vtxl.uv2List.Count > 0)
                {
                    rapi->rpgBindUV2Buffer(vtxl.uv2List.ToArray(), rpgeoDataType_e.RPGEODATA_FLOAT, 8);
                }
                if (vtxl.uv3List.Count > 0)
                {
                    rapi->rpgBindUVXBuffer(vtxl.uv3List.ToArray(), rpgeoDataType_e.RPGEODATA_FLOAT, 8, 2, 2);
                }
                if (vtxl.uv4List.Count > 0)
                {
                    rapi->rpgBindUVXBuffer(vtxl.uv4List.ToArray(), rpgeoDataType_e.RPGEODATA_FLOAT, 8, 3, 2);
                }
                if (vtxl.vert0x22.Count > 0)
                {
                    List<byte> vert0x22s = new List<byte>();
                    foreach (var x22s in vtxl.vert0x22) { vert0x22s.AddRange(BitConverter.GetBytes(x22s[0])); vert0x22s.AddRange(BitConverter.GetBytes(x22s[1])); }
                    rapi->rpgBindUVXBuffer(vert0x22s.ToArray(), rpgeoDataType_e.RPGEODATA_SHORT, 4, 4, 2);
                }
                if (vtxl.vert0x23.Count > 0)
                {
                    List<byte> vert0x23s = new List<byte>();
                    foreach (var x23s in vtxl.vert0x23) { vert0x23s.AddRange(BitConverter.GetBytes(x23s[0])); vert0x23s.AddRange(BitConverter.GetBytes(x23s[1])); }
                    rapi->rpgBindUVXBuffer(vert0x23s.ToArray(), rpgeoDataType_e.RPGEODATA_SHORT, 4, 5, 2);
                }
                if (vtxl.vert0x24.Count > 0)
                {
                    List<byte> vert0x24s = new List<byte>();
                    foreach (var x24s in vtxl.vert0x24) { vert0x24s.AddRange(BitConverter.GetBytes(x24s[0])); vert0x24s.AddRange(BitConverter.GetBytes(x24s[1])); }
                    rapi->rpgBindUVXBuffer(vert0x24s.ToArray(), rpgeoDataType_e.RPGEODATA_SHORT, 4, 6, 2);
                }
                if (vtxl.vert0x25.Count > 0)
                {
                    List<byte> vert0x25s = new List<byte>();
                    foreach (var x25s in vtxl.vert0x25) { vert0x25s.AddRange(BitConverter.GetBytes(x25s[0])); vert0x25s.AddRange(BitConverter.GetBytes(x25s[1])); }
                    rapi->rpgBindUVXBuffer(vert0x25s.ToArray(), rpgeoDataType_e.RPGEODATA_SHORT, 4, 7, 2);
                }
                if (vtxl.vertColor2s.Count > 0)
                {
                    byte[] vertColor2sArr = new byte[vtxl.vertColor2s.Count * 4];
                    for (int j = 0; j < vtxl.vertColor2s.Count; j += 4) { vertColor2sArr[j] = vtxl.vertColors[j][0]; vertColor2sArr[j + 1] = vtxl.vertColors[j + 1][1]; vertColor2sArr[j + 2] = vtxl.vertColors[j + 2][2]; vertColor2sArr[j + 3] = vtxl.vertColors[j + 3][3]; }
                    rapi->rpgBindUVXBuffer(vertColor2sArr.ToArray(), rpgeoDataType_e.RPGEODATA_UBYTE, 4, 8, 4);
                }
                if (vtxl.trueVertWeights.Count > 0)
                {
                    List<byte> vertWeights = new List<byte>();
                    foreach (var weightInds in vtxl.trueVertWeightIndices) { vertWeights.AddRange(BitConverter.GetBytes(weightInds[0])); vertWeights.AddRange(BitConverter.GetBytes(weightInds[1])); vertWeights.AddRange(BitConverter.GetBytes(weightInds[2])); vertWeights.AddRange(BitConverter.GetBytes(weightInds[3])); }
                    rapi->rpgBindBoneWeightBuffer(vertWeights.ToArray(), rpgeoDataType_e.RPGEODATA_FLOAT, 4);
                }
                if (vtxl.trueVertWeightIndices.Count > 0)
                {
                    List<byte> vertWeightIndices = new List<byte>();
                    foreach (var weightInds in vtxl.trueVertWeightIndices) { vertWeightIndices.AddRange(BitConverter.GetBytes(weightInds[0])); vertWeightIndices.AddRange(BitConverter.GetBytes(weightInds[1])); vertWeightIndices.AddRange(BitConverter.GetBytes(weightInds[2])); vertWeightIndices.AddRange(BitConverter.GetBytes(weightInds[3])); }
                    rapi->rpgBindBoneIndexBuffer(vertWeightIndices.ToArray(), rpgeoDataType_e.RPGEODATA_INT, 4);
                }

                var meshTris = faces.GetTriangles();
                List<byte> tris = new List<byte>();
                foreach (var tri in meshTris) { tris.AddRange(BitConverter.GetBytes((int)tri.X)); tris.AddRange(BitConverter.GetBytes((int)tri.Y)); tris.AddRange(BitConverter.GetBytes((int)tri.Z)); }
                rapiObj.rpgCommitTriangles(tris, rpgeoDataType_e.RPGEODATA_INT, meshTris.Count, rpgeoPrimType_e.RPGEO_TRIANGLE_STRIP, true);
                rapiObj.rpgClearBufferBinds();

                if(bonePalette.Length > 0)
                {
                    rapi->rpgSetBoneMap(null);
                }
            }

            IntPtr mdl = rapi->rpgConstructModel();
            numMdl = 1;
            rapiObj.rpgDestroyContext(ctx);

            return mdl;
        }

        private static string GetWideCharString(byte* rawFname)
        {
            return Marshal.PtrToStringUni((IntPtr)rawFname);
        }

        public static void StringCopy(string str, int count, byte* strLocation)
        {
            byte[] strArr = Encoding.UTF8.GetBytes(str);
            for (int i = 0; i < count; i++)
            {
                if (i < strArr.Length)
                {
                    strLocation[i] = strArr[i];
                }
                else
                {
                    strLocation[i] = 0;
                }
            }
        }

    }
}