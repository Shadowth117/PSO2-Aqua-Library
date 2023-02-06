using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra.FromSoft
{
    public static class SoulsMapMetadataGenerator
    {
        private class RoomInfo
        {
            public int roomId = -1; //Id of the NVM itself
            public string areaId = "";
            public Vector3 BoundingBoxMax;
            public Vector3 BoundingBoxMin;
            public List<Gate> gates = new List<Gate>();
        }

        private class Gate
        {
            public List<NVM.Triangle> tris = new List<NVM.Triangle>();
            public List<int> uniqueVertIds = new List<int>();
        }

        public class MCCombo
        {
            public MCP mcp;
            public MCG mcg;
        }

        public static void Generate(List<string> directories, out MCCombo mcCombo)
        {
            mcCombo = new MCCombo();
            mcCombo.mcp = new MCP();
            mcCombo.mcg = new MCG();

            //Gather NVM files and filter by MSB 
            Dictionary<string, List<NVM>> nvmDict = new Dictionary<string, List<NVM>>(); 
            foreach(var dir in directories)
            {
                //Get the MSB to see what navmeshes are being used
                var name = Path.GetFileName(dir);
                var msbPath = Path.GetDirectoryName(dir) + $"\\mapstudio\\{name}.msb";
                MSBD msb = null;
                List<string> msbNavmeshNames = new List<string>();
                if (File.Exists(msbPath))
                {
                    msb = SoulsFile<MSBD>.Read(msbPath);
                    foreach(var navMesh in msb.Parts.Navmeshes)
                    {
                        msbNavmeshNames.Add(navMesh.Name.ToLower());
                    }
                }

                //Get the nvms from the nvmBnd, fall back to loose .nvms if non-existant
                List<NVM> nvmList = new List<NVM>();
                var nvmBnd = Directory.EnumerateFiles(dir, "*.nvmbnd").ToArray();
                if(nvmBnd.Length > 0)
                {
                    var nvmBndFile = SoulsFile<BND3>.Read(nvmBnd[0]);
                    foreach(var nvmFile in nvmBndFile.Files)
                    {
                        var fname = Path.GetFileNameWithoutExtension(nvmFile.Name).ToLower();
                        if (Path.GetExtension(nvmFile.Name) == ".nvm" && msbNavmeshNames.Contains(fname))
                        {
                            var nvm = SoulsFile<NVM>.Read(nvmFile.Bytes);
                            nvm.fileName = fname;
                            nvmList.Add(nvm);
                        }
                    }
                } else
                {
                    var files = Directory.EnumerateFiles(dir, "*.nvm");
                    foreach (var file in files)
                    {
                        var fname = Path.GetFileNameWithoutExtension(file);
                        if (msbNavmeshNames.Contains(fname))
                        {
                            var nvm = SoulsFile<NVM>.Read(file);
                            nvm.fileName = fname;
                            nvmList.Add(nvm);
                        }
                    }
                }
                nvmDict.Add(dir, nvmList);
            }

            //Process NVM data into what we need for MCG and MCP stuff
            //We need this calculated before we devise the result, hence the separate loop
            Dictionary<string, List<RoomInfo>> triDicts = new Dictionary<string, List<RoomInfo>>();
            foreach(var dir in directories)
            {
                var nvmList = nvmDict[dir];
                List<RoomInfo> nvmTriList = new List<RoomInfo>();
                foreach (var nvm in nvmList)
                {
                    RoomInfo roomInfo = new RoomInfo();
                    
                    //Set bounding from nvm
                    roomInfo.BoundingBoxMax = nvm.RootBox.MaxValueCorner;
                    roomInfo.BoundingBoxMin = nvm.RootBox.MinValueCorner;

                    //Add and subtract 1 from Y bounding for the corners. Demon's Souls does this for w/e reason so I'm doing it
                    roomInfo.BoundingBoxMax.Y += 1;
                    roomInfo.BoundingBoxMin.Y -= 1;

                    roomInfo.roomId = nvmTriList.Count;
                    roomInfo.areaId = Path.GetFileName(dir);

                    List<int> usedIndices = new List<int>();
                    //Gather Gate triangles
                    for(int i = 0; i < nvm.Triangles.Count; i++)
                    {
                        var triSet = new List<NVM.Triangle>();
                        CompileGateTriSet(triSet, usedIndices, nvm, i);
                        if(triSet.Count > 0)
                        {
                            Gate gate = new Gate();
                            gate.tris = triSet;
                            gate.uniqueVertIds = GetUniqueVertIds(triSet);
                            roomInfo.gates.Add(gate);
                        }
                    }
                    nvmTriList.Add(roomInfo);
                }
                triDicts.Add(dir, nvmTriList);
            }

            //Create MCP and MCG, presumably Map Container Portals and Map Container Gates, or something along those lines
            //Bounds in MCP should be the same as the bounds in each individual .nvm

        }

        private static List<int> GetUniqueVertIds(List<NVM.Triangle> triSet)
        {
            List<int> uniqueIds = new List<int>();

            foreach(var tri in triSet)
            {
                if(!uniqueIds.Contains(tri.VertexIndex1))
                {
                    uniqueIds.Add(tri.VertexIndex1);
                }
                if (!uniqueIds.Contains(tri.VertexIndex2))
                {
                    uniqueIds.Add(tri.VertexIndex2);
                }
                if (!uniqueIds.Contains(tri.VertexIndex3))
                {
                    uniqueIds.Add(tri.VertexIndex3);
                }
            }

            return uniqueIds;
        }

        private static void CompileGateTriSet(List<NVM.Triangle> triSet, List<int> usedIndices, NVM nvm, int id)
        {
            List<NVM.Triangle> tris = new List<NVM.Triangle>();
            var tri = nvm.Triangles[id];
            if (!usedIndices.Contains(id) && (tri.Flags & NVM.TriangleFlags.GATE) > 0)
            {
                //If we haven't taken this triangle yet, recursively check its adjacent triangles and add ones which are also gates.
                //Adjacent tris which are gates are considered part of the same gate entity
                usedIndices.Add(id);
                triSet.Add(tri);

                if (tri.EdgeIndex1 >= 0)
                {
                    CompileGateTriSet(triSet, usedIndices, nvm, tri.EdgeIndex1);
                }
                if (tri.EdgeIndex2 >= 0)
                {
                    CompileGateTriSet(triSet, usedIndices, nvm, tri.EdgeIndex2);
                }
                if (tri.EdgeIndex3 >= 0)
                {
                    CompileGateTriSet(triSet, usedIndices, nvm, tri.EdgeIndex3);
                }
            }
        }
    }
}
