using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher
{
    public class MC2Convert
    {
        public static bool vColorDeath = false;
        public static AquaObject ConvertMC2(byte[] file, out AquaNode aqn)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                return MC2ToAqua(new MC2(streamReader), out aqn);
            }
        }

        public static AquaObject MC2ToAqua(MC2 mc2, out AquaNode aqn)
        {
            AquaObject aqp = new AquaObject();
            aqn = AquaNode.GenerateBasicAQN();

            aqn.ndtr.boneCount = aqn.nodeList.Count;
            aqp.objc.bonePaletteOffset = 1;

            double heightMax = -1000000000;
            double heightMin = 1000000000;
            if(vColorDeath)
            {
                foreach(var vert in mc2.vertPositions)
                {
                    heightMax = Math.Max(vert.Y, heightMax);
                    heightMin = Math.Min(vert.Y, heightMin);
                }
            }
            double difference = heightMax - heightMin;
            //Separate out to meshes by flag combos
            int i = 0;
            Dictionary<string, List<MC2.MC2FaceData>> meshDict = new Dictionary<string, List<MC2.MC2FaceData>>();
            for (int triId = 0; triId < mc2.faceData.Count; triId++)
            {
                var tri = mc2.faceData[triId];
                string name = $"mat";

                if (vColorDeath && (tri.flagSet1 & MC2.FlagSet1.Unk1_0x20) != 0 && tri.flagSet0 == 0 && tri.flagSet1 == (MC2.FlagSet1.Unk1_0x20) && tri.flagSet2 == 0)
                {
                    continue;
                }
                //Flags
                foreach (var flag in Enum.GetValues(typeof(MC2.FlagSet0)))
                {
                    if ((tri.flagSet0 & (MC2.FlagSet0)flag) > 0)
                    {
                        name += $"#{flag}";
                    }
                }
                foreach (var flag in Enum.GetValues(typeof(MC2.FlagSet1)))
                {
                    if ((tri.flagSet1 & (MC2.FlagSet1)flag) > 0)
                    {
                        name += $"#{flag}";
                    }
                }
                foreach (var flag in Enum.GetValues(typeof(MC2.FlagSet2)))
                {
                    if ((tri.flagSet2 & (MC2.FlagSet2)flag) > 0)
                    {
                        name += $"#{flag}";
                    }
                }
                if(vColorDeath)
                {
                    name += $"#enablelighting";
                }

                if (!meshDict.ContainsKey(name))
                {
                    meshDict.Add(name, new List<MC2.MC2FaceData>());
                }
                meshDict[name].Add(tri);

                i++;
            }

            //Assemble Meshes
            int m = 0;
            foreach (var pair in meshDict)
            {
                Dictionary<int, int> vertIndexRemap = new Dictionary<int, int>();
                GenericTriangles genMesh = new GenericTriangles();
                var genMat = new GenericMaterial();
                genMesh.triList = new List<Vector3>();
                genMat.matName = pair.Key;
                genMat.diffuseRGBA = new Vector4(1, 1, 1, 1);
                int f = 0;

                foreach (var tri in pair.Value)
                {
                    if (vColorDeath && (tri.flagSet1 & MC2.FlagSet1.Unk1_0x20) != 0 && tri.flagSet0 == 0 && tri.flagSet1 == (MC2.FlagSet1.Unk1_0x20) && tri.flagSet2 == 0)
                    {
                        continue;
                    }
                    VTXL faceVtxl = new VTXL();
                    faceVtxl.rawFaceId.Add(f);
                    faceVtxl.rawFaceId.Add(f);
                    faceVtxl.rawFaceId.Add(f++);

                    if (!vertIndexRemap.ContainsKey(tri.vert0))
                    {
                        vertIndexRemap.Add(tri.vert0, genMesh.vertCount++);
                    }
                    if (!vertIndexRemap.ContainsKey(tri.vert1))
                    {
                        vertIndexRemap.Add(tri.vert1, genMesh.vertCount++);
                    }
                    if (!vertIndexRemap.ContainsKey(tri.vert2))
                    {
                        vertIndexRemap.Add(tri.vert2, genMesh.vertCount++);
                    }
                    faceVtxl.vertPositions.Add(mc2.vertPositions[tri.vert0]);
                    faceVtxl.vertPositions.Add(mc2.vertPositions[tri.vert1]);
                    faceVtxl.vertPositions.Add(mc2.vertPositions[tri.vert2]);
                    if(vColorDeath)
                    {
                        var normal = tri.faceNormal;
                        var horiz = Math.Abs(normal.X) + Math.Abs(normal.Z);
                        horiz /= 2;
                        var hypotenuse = Math.Sqrt(horiz * horiz + normal.Y * normal.Y);
                        var angleRange = (Math.Asin(normal.Y / hypotenuse) + (Math.PI / 2)) / Math.PI;

                        byte[] vertColor = [0xC4, 0xC4, 0xC4, 0xFF];
                        if((tri.flagSet0 & MC2.FlagSet0.Death) != 0)
                        {
                            vertColor = [0x00, 0xFF, 0x00, 0xFF];
                        } else if((tri.flagSet0 & MC2.FlagSet0.Lava) != 0)
                        {
                            vertColor = [0, 0xFF, 0x0, 0xFF];
                        }
                        else if ((tri.flagSet1 & MC2.FlagSet1.Quicksand) != 0)
                        {
                            vertColor = [0, 0xFF, 0x0, 0xFF];
                        }
                        else if ((tri.flagSet1 & MC2.FlagSet1.Drown) != 0)
                        {
                            vertColor = [0x0, 0xFF, 0, 0xFF];
                        }
                        for(int j = 0; j < 2; j++)
                        {
                            Vector3 vert = faceVtxl.vertPositions[j];
                            vertColor[j] = (byte)((angleRange * ((vert.Y - heightMin) / difference) * vertColor[j] ));
                        }

                        faceVtxl.vertColors.Add(vertColor);
                        faceVtxl.vertColors.Add(vertColor);
                        faceVtxl.vertColors.Add(vertColor);
                        faceVtxl.vertColor2s.Add(vertColor);
                        faceVtxl.vertColor2s.Add(vertColor);
                        faceVtxl.vertColor2s.Add(vertColor);
                        faceVtxl.uv1List.Add(new Vector2());
                        faceVtxl.uv1List.Add(new Vector2());
                        faceVtxl.uv1List.Add(new Vector2());
                    }
                    genMesh.matIdList.Add(aqp.tempMats.Count);
                    genMesh.triList.Add(new Vector3(vertIndexRemap[tri.vert0], vertIndexRemap[tri.vert1], vertIndexRemap[tri.vert2]));
                    faceVtxl.rawVertId.Add(vertIndexRemap[tri.vert0]);
                    faceVtxl.rawVertId.Add(vertIndexRemap[tri.vert1]);
                    faceVtxl.rawVertId.Add(vertIndexRemap[tri.vert2]);

                    genMesh.faceVerts.Add(faceVtxl);
                }

                aqp.tempMats.Add(genMat);
                aqp.tempTris.Add(genMesh);

                m++;
            }

            return aqp;
        }

    }
}
