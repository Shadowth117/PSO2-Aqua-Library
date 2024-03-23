using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.Ninja.Model
{
    public class NinjaModelConvert
    {
        public static AquaObject GinjaConvert(string fileName, out AquaNode aqn, List<string> texNames = null)
        {
            return ModelConvert(File.ReadAllBytes(fileName), NinjaVariant.Ginja, out aqn);
        }

        public static AquaObject ModelConvert(byte[] ninjaModel, NinjaVariant variant, out AquaNode aqn, int offset = 0, List<string> texNames = null)
        {
            using (var ms = new MemoryStream(ninjaModel))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                return ModelConvert(sr, variant, out aqn, offset);
            }
        }

        public static AquaObject ModelConvert(BufferedStreamReaderBE<MemoryStream> sr, NinjaVariant variant, out AquaNode aqn, int offset = 0, List<string> texNames = null)
        {
            var magic = sr.Peek<NJMagic>();
            switch(magic)
            {
                case NJMagic.NJBM:
                    variant = NinjaVariant.Basic;
                    offset += 8;
                    sr.Seek(8, SeekOrigin.Current);
                    break;
                case NJMagic.NJCM:
                    //Don't assign one here because for some unknown reason XJ and NJ Chunk have the same magic (thanks, sega)
                    offset += 8;
                    sr.Seek(8, SeekOrigin.Current);
                    break;
                case NJMagic.GJCM:
                    variant = NinjaVariant.Ginja;
                    offset += 8;
                    sr.Seek(8, SeekOrigin.Current);
                    break;
                default:
                    //Assume there's no 8 byte ninja header
                    break;
            }
            var leAddress = sr.Peek<int>();
            var beAddress = sr.PeekBigEndianInt32();

            var root = new NJSObject(sr, variant, leAddress > beAddress, offset);
            var model = NinjaToAqua(root, out aqn, texNames);
            return model;
        }

        public static AquaObject NinjaToAqua(NJSObject NinjaModelRoot, out AquaNode aqn, List<string> texNames = null)
        {
            VTXL fullVertList = null;
            AquaObject aqo = new AquaObject();
            aqn = new AquaNode();
            int nodeCounter = 0;
            if (NinjaModelRoot.HasWeights())
            {
                fullVertList = new VTXL();
                GatherFullVertexListRecursive(NinjaModelRoot, fullVertList, ref nodeCounter, Matrix4x4.Identity, -1);
                fullVertList.ProcessToPSO2Weights();
            }

            nodeCounter = 0;
            GatherModelDataRecursive(NinjaModelRoot, fullVertList, ref nodeCounter, aqo, aqn, Matrix4x4.Identity, -1);
            aqn.ndtr.boneCount = aqn.nodeList.Count;
            aqo.objc.bonePaletteOffset = 1;

            //Assign texture names, Ninja models don't contain these
            foreach(var tempMats in aqo.tempMats)
            {
                for(int i = 0; i < tempMats.texNames.Count; i++)
                {
                    var texId = Int32.Parse(tempMats.texNames[i]);
                    if (texNames?.Count > texId)
                    {
                        tempMats.texNames[i] = texNames[texId];
                    }
                }
            }
            return aqo;
        }

        /// <summary>
        /// For weighted models, at some point we have to gather all of the vertices before we can apply them. 
        /// With this, we can do a preprocessing loop for later usage.
        /// </summary>
        public static void GatherFullVertexListRecursive(NJSObject njObj, VTXL fullVertList, ref int nodeId, Matrix4x4 parentMatrix, int parentId)
        {
            Matrix4x4 mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(njObj.scale);
            var rotation = Matrix4x4.CreateRotationX(njObj.rot.X) *
                Matrix4x4.CreateRotationY(njObj.rot.Y) *
                Matrix4x4.CreateRotationZ(njObj.rot.Z);
            mat *= rotation;
            mat *= Matrix4x4.CreateTranslation(njObj.pos);
            mat = mat * parentMatrix;

            njObj.GetVertexData(nodeId, fullVertList, mat);

            if(njObj.childObject != null)
            {
                nodeId++;
                GatherFullVertexListRecursive(njObj.childObject, fullVertList, ref nodeId, mat, nodeId);
            }
            if (njObj.siblingObject != null)
            {
                nodeId++;
                GatherFullVertexListRecursive(njObj.siblingObject, fullVertList, ref nodeId, parentMatrix, parentId);
            }
        }

        public static void GatherModelDataRecursive(NJSObject njObj, VTXL fullVertList, ref int nodeId, AquaObject aqo, AquaNode aqn, Matrix4x4 parentMatrix, int parentId)
        {
            aqo.bonePalette.Add((uint)nodeId);
            int currentNodeId = nodeId;
            Matrix4x4 mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(njObj.scale);
            var rotation = Matrix4x4.CreateRotationX(njObj.rot.X) *
                Matrix4x4.CreateRotationY(njObj.rot.Y) *
                Matrix4x4.CreateRotationZ(njObj.rot.Z);
            mat *= rotation;
            mat *= Matrix4x4.CreateTranslation(njObj.pos);
            mat = mat * parentMatrix;

            //Create AQN node
            NODE aqNode = new NODE();
            aqNode.boneShort1 = 0x1C0;
            aqNode.animatedFlag = 1;
            aqNode.parentId = parentId;
            aqNode.nextSibling = -1;
            aqNode.firstChild = -1;
            aqNode.unkNode = -1;
            aqNode.pos = njObj.pos;
            aqNode.eulRot = new Vector3((float)(njObj.rot.X * 180 / Math.PI), 
                (float)(njObj.rot.Y * 180 / Math.PI), (float)(njObj.rot.Z * 180 / Math.PI));
            aqNode.scale = njObj.scale;
            Matrix4x4.Invert(mat, out var invMat);
            aqNode.m1 = new Vector4(invMat.M11, invMat.M12, invMat.M13, invMat.M14);
            aqNode.m2 = new Vector4(invMat.M21, invMat.M22, invMat.M23, invMat.M24);
            aqNode.m3 = new Vector4(invMat.M31, invMat.M32, invMat.M33, invMat.M34);
            aqNode.m4 = new Vector4(invMat.M41, invMat.M42, invMat.M43, invMat.M44);
            aqNode.boneName.SetString(aqn.nodeList.Count.ToString());
            aqn.nodeList.Add(aqNode);

            VTXL tempVTXL;
            if (fullVertList == null)
            {
                tempVTXL = new VTXL();
                njObj.GetVertexData(nodeId, tempVTXL, mat);
                tempVTXL.ProcessToPSO2Weights();
            } else
            {
                tempVTXL = fullVertList;
            }

            njObj.GetFaceData(nodeId, tempVTXL, aqo);

            if (njObj.childObject != null)
            {
                aqNode.firstChild = ++nodeId;
                GatherModelDataRecursive(njObj.childObject, fullVertList, ref nodeId, aqo, aqn, mat, currentNodeId);
            }
            if (njObj.siblingObject != null)
            {
                aqNode.nextSibling = ++nodeId;
                GatherModelDataRecursive(njObj.siblingObject, fullVertList, ref nodeId, aqo, aqn, parentMatrix, parentId);
            }
            aqn.nodeList[currentNodeId] = aqNode;
        }

        public static byte[] GetGjBytes(NJSObject njsObject)
        {
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            List<int> pofSets = new List<int>();
            njsObject.Write(outBytes, pofSets, true);

            List<byte> headerMagic = new List<byte>
            {
                0x47,
                0x4A,
                0x43,
                0x4D
            };
            //This should almost always be little endian, but can be be in rare cases such as skies of arcadia
            headerMagic.AddRange(BitConverter.GetBytes(outBytes.Count));

            outBytes.InsertRange(0, headerMagic);
            outBytes.AddRange(POF0.GeneratePOF0(pofSets));

            return outBytes.ToArray();
        }
    }
}
