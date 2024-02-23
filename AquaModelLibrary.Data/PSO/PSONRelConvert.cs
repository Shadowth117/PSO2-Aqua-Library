using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;
using System.Numerics;
using static AquaModelLibrary.Data.Ninja.NinjaConstants;
using static AquaModelLibrary.Data.PSO.PSOXVMConvert;

namespace AquaModelLibrary.Data.PSO
{
    //Written based on: https://gitlab.com/dashgl/ikaruga/-/snippets/2054101
    //And based on the struct notes provided in the Schthack map viewer readme.
    //Intended to read *n.rel files (n rel files are the geometry)
    public class PSONRelConvert
    {
        public AquaNode aqn = AquaNode.GenerateBasicAQN();
        public AquaObject aqObj = new AquaObject();
        public List<string> texNames = new List<string>();
        private GenericTriangles currentMesh;
        private BufferedStreamReaderBE<MemoryStream> streamReader;
        private bool be; //Stores if we're reading big endian or not. Gamecube variants appear to be different as well.
        private float rootScale = 0.1f;
        private long fileSize;

        public struct relHeader
        {
            public uint fmt2;
            public uint uint_04;
            public uint drawCount;
            public float hd; //Float 800? Or maybe just letters.

            public uint drawOffset;
            public uint nameInfoOffset;
        }
        public static relHeader ReadRelHeader(BufferedStreamReaderBE<MemoryStream> streamReader, bool active)
        {
            var header = new relHeader();

            header.fmt2 = streamReader.ReadBE<uint>(active);
            header.uint_04 = streamReader.ReadBE<uint>(active);
            header.drawCount = streamReader.ReadBE<uint>(active);
            header.hd = streamReader.ReadBE<float>(active);

            header.drawOffset = streamReader.ReadBE<uint>(active);
            header.nameInfoOffset = streamReader.ReadBE<uint>(active);

            return header;
        }

        public struct dSection
        {
            public int id;
            public Vector3 pos;
            public Vector3 rot;

            public float radius;
            public uint staticOffset;
            public uint animatedOffset;
            public uint staticCount;

            public uint animatedCount;
            public uint end;
        }

        public struct staticMeshOffset
        {
            public uint offset;
            public uint uint_04;
            public uint uint_08;
            public uint flags;
        }
        public static staticMeshOffset ReadStaticMeshOffset(BufferedStreamReaderBE<MemoryStream> streamReader, bool active)
        {
            var offset = new staticMeshOffset();

            offset.offset = streamReader.ReadBE<uint>(active);
            offset.uint_04 = streamReader.ReadBE<uint>(active);
            offset.uint_08 = streamReader.ReadBE<uint>(active);
            offset.flags = streamReader.ReadBE<uint>(active);

            return offset;
        }

        public struct animMeshOffset
        {
            public uint offset;
            public uint uint_04;
            public uint uint_08;
            public uint uint_0C;

            public uint uint_10;
            public uint uint_14;
            public uint uint_18;
            public uint flags;
        }
        public static animMeshOffset ReadAnimMeshOffset(BufferedStreamReaderBE<MemoryStream> streamReader, bool active)
        {
            var offset = new animMeshOffset();

            offset.offset = streamReader.ReadBE<uint>(active);
            offset.uint_04 = streamReader.ReadBE<uint>(active);
            offset.uint_08 = streamReader.ReadBE<uint>(active);
            offset.uint_0C = streamReader.ReadBE<uint>(active);

            offset.uint_10 = streamReader.ReadBE<uint>(active);
            offset.uint_14 = streamReader.ReadBE<uint>(active);
            offset.uint_18 = streamReader.ReadBE<uint>(active);
            offset.flags = streamReader.ReadBE<uint>(active);

            return offset;
        }

        public struct NJS_OBJECT
        {
            public uint flags;
            public uint attachOffset;

            public Vector3 pos;
            public Vector3 rot;
            public Vector3 scl;

            public uint childOffset;
            public uint siblingOffset;
        }

        public struct njMesh
        {
            public uint flags;
            public uint vertexInfoListOffset;
            public uint vertexInfoCount;
            public uint triangleStripListAOffset;

            public uint triangleStripListACount;
            public uint triangleStripListBOffset;
            public uint triangleStripListBCount;

            public Vector3 center;
            public float radius;
        }

        public struct njMeshGC
        {
            public uint vertexInfoListOffset;
            public uint int_04;
            public uint triangleStripListAOffset;
            public uint triangleStripListBOffset;

            public ushort triangleStripListACount;
            public ushort triangleStripListBCount;

            public Vector3 center;
            public float radius;
        }

        public static njMesh ReadNjMesh(BufferedStreamReaderBE<MemoryStream> streamReader, bool active)
        {
            var mesh = new njMesh();

            mesh.flags = streamReader.ReadBE<uint>(active);
            mesh.vertexInfoListOffset = streamReader.ReadBE<uint>(active);
            mesh.vertexInfoCount = streamReader.ReadBE<uint>(active);
            mesh.triangleStripListAOffset = streamReader.ReadBE<uint>(active);

            mesh.triangleStripListACount = streamReader.ReadBE<uint>(active);
            mesh.triangleStripListBOffset = streamReader.ReadBE<uint>(active);
            mesh.triangleStripListBCount = streamReader.ReadBE<uint>(active);

            mesh.center = streamReader.ReadBEV3(active);
            mesh.radius = streamReader.ReadBE<float>(active);

            return mesh;
        }

        public static njMeshGC ReadNjMeshGC(BufferedStreamReaderBE<MemoryStream> streamReader, bool active)
        {
            var mesh = new njMeshGC();

            mesh.vertexInfoListOffset = streamReader.ReadBE<uint>(active);
            mesh.int_04 = streamReader.ReadBE<uint>(active);
            mesh.triangleStripListAOffset = streamReader.ReadBE<uint>(active);
            mesh.triangleStripListBOffset = streamReader.ReadBE<uint>(active);

            mesh.triangleStripListACount = streamReader.ReadBE<ushort>(active);
            mesh.triangleStripListBCount = streamReader.ReadBE<ushort>(active);

            mesh.center = streamReader.ReadBEV3(active);
            mesh.radius = streamReader.ReadBE<float>(active);

            return mesh;
        }

        public struct njVTXE
        {
            public ushort vertexType;
            public ushort ushort_02;
            public uint vertexListOffset;
            public uint vertexSize;

            public uint vertexCount;
        }
        public static njVTXE ReadNjVtxe(BufferedStreamReaderBE<MemoryStream> streamReader, bool active)
        {
            var vtxe = new njVTXE();

            vtxe.vertexType = streamReader.ReadBE<ushort>(active);
            vtxe.ushort_02 = streamReader.ReadBE<ushort>(active);
            vtxe.vertexListOffset = streamReader.ReadBE<uint>(active);
            vtxe.vertexSize = streamReader.ReadBE<uint>(active);

            vtxe.vertexCount = streamReader.ReadBE<uint>(active);

            return vtxe;
        }

        public struct stripInfo
        {
            public uint materialPropertyListOffset;
            public uint materialPropertyListSize;
            public uint indexListOffset;
            public uint indexCount;

            public uint uint_10;
        }
        public static stripInfo ReadStripInfo(BufferedStreamReaderBE<MemoryStream> streamReader, bool active)
        {
            var sInfo = new stripInfo();

            sInfo.materialPropertyListOffset = streamReader.ReadBE<uint>(active);
            sInfo.materialPropertyListSize = streamReader.ReadBE<uint>(active);
            sInfo.indexListOffset = streamReader.ReadBE<uint>(active);
            sInfo.indexCount = streamReader.ReadBE<uint>(active);

            sInfo.uint_10 = streamReader.ReadBE<uint>(active);

            return sInfo;
        }

        //Takes in bytes of a *n.rel file from PSO
        //To convert to PSO2's units, we set the scale to 1/10th scale
        public PSONRelConvert(byte[] file, string fileName = null, float scale = 0.1f, string outFolder = null)
        {
            fileSize = file.Length;
            rootScale = scale;
            List<dSection> dSections = new List<dSection>();
            streamReader = new BufferedStreamReaderBE<MemoryStream>(new MemoryStream(file));

            //Get header offset
            streamReader.Seek(file.Length - 0x10, SeekOrigin.Begin);

            //Check Endianness. No offset should ever come close to half of the int max value.
            be = streamReader.PeekBigEndianUInt32() < streamReader.Peek<uint>();
            if (be)
            {
                Debug.WriteLine("Sorry, Gamecube n.rel files are not supported at this time.");
            }
            uint tableOfs = streamReader.ReadBE<uint>(be);

            //Read header
            streamReader.Seek(tableOfs, SeekOrigin.Begin);
            var header = ReadRelHeader(streamReader, be);

            //Read draw Sections
            streamReader.Seek(header.drawOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.drawCount; i++)
            {
                dSection section = new dSection();
                section.id = streamReader.ReadBE<int>(be);
                section.pos = streamReader.ReadBEV3(be);
                var rotX = streamReader.ReadBE<int>(be);
                var rotY = streamReader.ReadBE<int>(be);
                var rotZ = streamReader.ReadBE<int>(be);
                section.rot = new Vector3((float)(rotX * FromBAMSvalueToRadians), (float)(rotY * FromBAMSvalueToRadians), (float)(rotZ * FromBAMSvalueToRadians));
                section.radius = streamReader.ReadBE<float>(be);
                section.staticOffset = streamReader.ReadBE<uint>(be);
                section.animatedOffset = streamReader.ReadBE<uint>(be);
                section.staticCount = streamReader.ReadBE<uint>(be);
                section.animatedCount = streamReader.ReadBE<uint>(be);
                section.end = streamReader.ReadBE<uint>(be);

                dSections.Add(section);
            }

            //Get texture names
            streamReader.Seek(header.nameInfoOffset, SeekOrigin.Begin);
            var nameOffset = streamReader.ReadBE<uint>(be);
            var nameCount = streamReader.ReadBE<uint>(be);
            streamReader.Seek(nameOffset, SeekOrigin.Begin);
            List<uint> nameOffsets = new List<uint>();
            for (int i = 0; i < nameCount; i++)
            {
                nameOffsets.Add(streamReader.ReadBE<uint>(be));
                var unk0 = streamReader.ReadBE<uint>(be);
                var unk1 = streamReader.ReadBE<uint>(be);

                if (unk0 != 0)
                {
                    Console.WriteLine($"Iteration {i} unk0 == {unk0}");
                }
                if (unk1 != 0)
                {
                    Console.WriteLine($"Iteration {i} unk1 == {unk1}");
                }
            }
            foreach (uint offset in nameOffsets)
            {
                streamReader.Seek(offset, SeekOrigin.Begin);
                texNames.Add(streamReader.ReadCString());
            }

            //If there's an .xvm, dump that too with texture names from the .rel
            if (fileName != null)
            {
                //Naming patterns for *n.rel files are *_12n.rel for example or *n.rel  vs *.xvm. We can determine which we have, edit, and proceed
                var basename = fileName.Substring(0, fileName.Length - 5);
                string xvmName = null;

                if (basename.ElementAt(basename.Length - 3) == '_')
                {
                    xvmName = basename.Substring(0, basename.Length - 3) + ".xvm";
                }
                else
                {
                    xvmName = basename + ".xvm";
                }

                ExtractXVM(xvmName, texNames, outFolder);
            }

            //Loop through nodes and parse geometry
            for (int i = 0; i < dSections.Count; i++)
            {
                var matrix = Matrix4x4.Identity;

                matrix *= Matrix4x4.CreateScale(1, 1, 1);

                var rotation = Matrix4x4.CreateRotationX(dSections[i].rot.X) *
                    Matrix4x4.CreateRotationY(dSections[i].rot.Y) *
                    Matrix4x4.CreateRotationZ(dSections[i].rot.Z);

                matrix *= rotation;

                matrix *= Matrix4x4.CreateTranslation(dSections[i].pos * rootScale);

                //Read static meshes
                List<staticMeshOffset> staticMeshOffsets = new List<staticMeshOffset>();
                streamReader.Seek(dSections[i].staticOffset, SeekOrigin.Begin);
                for (int st = 0; st < dSections[i].staticCount; st++)
                {
                    staticMeshOffsets.Add(ReadStaticMeshOffset(streamReader, be));
                }
                for (int ofs = 0; ofs < staticMeshOffsets.Count; ofs++)
                {
                    streamReader.Seek(staticMeshOffsets[ofs].offset, SeekOrigin.Begin);
                    readNode(matrix, 0);
                }


                //Read animated meshes
                List<animMeshOffset> animatedMeshOffsets = new List<animMeshOffset>();
                streamReader.Seek(dSections[i].animatedOffset, SeekOrigin.Begin);
                for (int st = 0; st < dSections[i].animatedCount; st++)
                {
                    animatedMeshOffsets.Add(ReadAnimMeshOffset(streamReader, be));
                }
                for (int ofs = 0; ofs < animatedMeshOffsets.Count; ofs++)
                {
                    streamReader.Seek(animatedMeshOffsets[ofs].offset, SeekOrigin.Begin);
                    readNode(matrix, 0);
                }

            }

            //Set material names
            for (int i = 0; i < aqObj.tempMats.Count; i++)
            {
                aqObj.tempMats[i].matName = $"PSOMat {i}";
            }
        }

        //Read NJS_OBJECT
        public void readNode(Matrix4x4 parentMatrix, int parentId)
        {
            NJS_OBJECT node = new NJS_OBJECT();
            node.flags = streamReader.ReadBE<uint>(be);
            node.attachOffset = streamReader.ReadBE<uint>(be);
            node.pos = streamReader.ReadBEV3(be);
            var rotX = streamReader.ReadBE<int>(be);
            var rotY = streamReader.ReadBE<int>(be);
            var rotZ = streamReader.ReadBE<int>(be);
            node.rot = new Vector3((float)(rotX * FromBAMSvalueToRadians), (float)(rotY * FromBAMSvalueToRadians), (float)(rotZ * FromBAMSvalueToRadians));
            node.scl = streamReader.ReadBEV3(be);
            node.childOffset = streamReader.ReadBE<uint>(be);
            node.siblingOffset = streamReader.ReadBE<uint>(be);

            Matrix4x4 mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(node.scl);
            var rotation = Matrix4x4.CreateRotationX(node.rot.X) *
                Matrix4x4.CreateRotationY(node.rot.Y) *
                Matrix4x4.CreateRotationZ(node.rot.Z);
            mat *= rotation;
            mat *= Matrix4x4.CreateTranslation(node.pos * rootScale);
            mat = mat * parentMatrix;

            //Create AQN node
            NODE aqNode = new NODE();
            aqNode.animatedFlag = 1;
            aqNode.parentId = parentId;
            aqNode.unkNode = -1;
            aqNode.pos = node.pos;
            aqNode.eulRot = new Vector3((float)(rotX * FromBAMSvalueToRadians * 180 / Math.PI), (float)(rotY * FromBAMSvalueToRadians * 180 / Math.PI), (float)(rotZ * FromBAMSvalueToRadians * 180 / Math.PI));

            if (Math.Abs(aqNode.eulRot.Y) > 120)
            {
                aqNode.scale = new Vector3(-1, -1, -1);
            }
            else
            {
                aqNode.scale = new Vector3(1, 1, 1);
            }

            Matrix4x4.Invert(mat, out var invMat);
            aqNode.m1 = new Vector4(invMat.M11, invMat.M12, invMat.M13, invMat.M14);
            aqNode.m2 = new Vector4(invMat.M21, invMat.M22, invMat.M23, invMat.M24);
            aqNode.m3 = new Vector4(invMat.M31, invMat.M32, invMat.M33, invMat.M34);
            aqNode.m4 = new Vector4(invMat.M41, invMat.M42, invMat.M43, invMat.M44);
            aqNode.boneName.SetString("Node " + aqn.nodeList.Count);
            aqn.nodeList.Add(aqNode);

            //Not sure what it means when these happen, but sometimes they do. Maybe hardcoded logic?
            if (node.attachOffset > fileSize || node.siblingOffset > fileSize || node.childOffset > fileSize)
            {
                return;
            }

            //Read the attached Mesh
            if (node.attachOffset != 0)
            {
                streamReader.Seek(node.attachOffset, SeekOrigin.Begin);
                readMesh(mat);
            }

            //Read the child
            if (node.childOffset != 0)
            {
                streamReader.Seek(node.childOffset, SeekOrigin.Begin);
                readNode(mat, aqn.nodeList.Count - 1);
            }

            //Read the sibling
            if (node.siblingOffset != 0)
            {
                streamReader.Seek(node.siblingOffset, SeekOrigin.Begin);
                readNode(parentMatrix, parentId);
            }
        }

        public void readMesh(Matrix4x4 mat)
        {
            currentMesh = new GenericTriangles();
            currentMesh.baseMeshNodeId = aqn.nodeList.Count - 1;
            uint vertOffset;
            uint triOffsetA;
            uint triOffsetB;
            uint triCountA;
            uint triCountB;

            if (be)
            {
                var meshGC = ReadNjMeshGC(streamReader, be);

                vertOffset = meshGC.vertexInfoListOffset;
                triOffsetA = meshGC.triangleStripListAOffset;
                triOffsetB = meshGC.triangleStripListBOffset;
                triCountA = meshGC.triangleStripListACount;
                triCountB = meshGC.triangleStripListBCount;
            }
            else
            {
                var mesh = ReadNjMesh(streamReader, be);
                vertOffset = mesh.vertexInfoListOffset;
                triOffsetA = mesh.triangleStripListAOffset;
                triOffsetB = mesh.triangleStripListBOffset;
                triCountA = mesh.triangleStripListACount;
                triCountB = mesh.triangleStripListBCount;
            }

            streamReader.Seek(vertOffset, SeekOrigin.Begin);
            readVertexList(mat);

            if (triCountA > 0)
            {
                streamReader.Seek(triOffsetA, SeekOrigin.Begin);
                readStripList(triCountA, false);
            }

            if (triCountB > 0)
            {
                streamReader.Seek(triOffsetB, SeekOrigin.Begin);
                readStripList(triCountB, true);
            }

            aqObj.tempTris.Add(currentMesh);
        }

        public void readVertexList(Matrix4x4 mat)
        {
            var vtxe = ReadNjVtxe(streamReader, be);
            streamReader.Seek(vtxe.vertexListOffset, SeekOrigin.Begin);
            var vtxl = new VTXL();

            bool readUv = false;
            bool readNormal = false;
            bool readColor = false;

            int size = (int)vtxe.vertexSize;

            //pos
            size -= 0xC;

            //Process bitflags
            if ((vtxe.vertexType & 0x01) != 0)
            {
                readUv = true;
                size -= 0x8;
            }
            if ((vtxe.vertexType & 0x02) != 0)
            {
                readNormal = true;
                size -= 0xC;
            }
            if ((vtxe.vertexType & 0x04) != 0)
            {
                readColor = true;
                size -= 0x4;
            }

            if (size != 0)
            {
                Console.WriteLine($"Vert size is not 0 at vtxe {(streamReader.Position - 0x10).ToString("X")}");
            }
            //Read vertices
            for (int i = 0; i < vtxe.vertexCount; i++)
            {
                var pos = streamReader.ReadBEV3(be) * rootScale;
                var newPos = Vector3.Transform(pos, mat);
                vtxl.vertPositions.Add(newPos);

                if (readNormal)
                {
                    var nrm = streamReader.ReadBEV3(be);
                    var newNrm = Vector3.TransformNormal(nrm, mat);

                    vtxl.vertNormals.Add(newNrm);
                }
                if (readColor)
                {
                    byte[] color = new byte[4];
                    color[0] = streamReader.Read<byte>();
                    color[1] = streamReader.Read<byte>();
                    color[2] = streamReader.Read<byte>();
                    color[3] = streamReader.Read<byte>();

                    vtxl.vertColors.Add(color);
                }
                if (readUv)
                {
                    vtxl.uv1List.Add(streamReader.ReadBEV2(be));
                }

                //skip unexpected sections
                if (size > 0)
                {
                    streamReader.Seek(size, SeekOrigin.Current);
                }
            }

            aqObj.vtxlList.Add(vtxl);
        }

        public void readStripList(uint count, bool useAlpha = false)
        {
            List<stripInfo> info = new List<stripInfo>();
            for (int i = 0; i < count; i++)
            {
                info.Add(ReadStripInfo(streamReader, be));
            }
            int materialId = 0;

            foreach (var strip in info)
            {
                //Read material
                if (strip.materialPropertyListOffset > 0)
                {
                    streamReader.Seek(strip.materialPropertyListOffset, SeekOrigin.Begin);
                    materialId = readMaterial(strip.materialPropertyListSize, useAlpha);
                }

                //Read strips
                streamReader.Seek(strip.indexListOffset, SeekOrigin.Begin);
                var triStrip = new StripData();
                for (int i = 0; i < strip.indexCount; i++)
                {
                    var id = streamReader.ReadBE<ushort>(be);
                    triStrip.triStrips.Add(id);
                }

                //Set triangles
                var triSet = triStrip.GetTriangles(true);
                for (int i = 0; i < triSet.Count; i++)
                {
                    currentMesh.matIdList.Add(materialId);
                }
                currentMesh.triList.AddRange(triSet);
                triStrip = null;
            }
        }

        public int readMaterial(uint propertyCount, bool useAlpha)
        {
            GenericMaterial genMat = new GenericMaterial();
            if (useAlpha == true)
            {
                genMat.blendType = "blendalpha";
            }
            else
            {
                genMat.blendType = "opaque";
            }
            genMat.shaderNames = new List<string>
            {
                "0002p",
                "0002"
            };
            genMat.texNames = new List<string>();

            int def0;
            int def1;
            int def2;
            for (int i = 0; i < propertyCount; i++)
            {
                uint type = streamReader.ReadBE<uint>(be);
                switch (type)
                {
                    case 2:
                        var dstAlpha = streamReader.Read<uint>();
                        var srcAlpha = streamReader.Read<uint>();
                        var unk = streamReader.Read<uint>();
                        break;
                    case 3:
                        //Texture id. Use this to read from the list of names.
                        var id = streamReader.ReadBE<uint>(be);
                        var idUnk = streamReader.ReadBEV2(be);

                        genMat.texNames.Add(texNames[(int)id] + ".dds");
                        break;
                    case 4:
                        genMat.twoSided = 1;
                        var twoSided = streamReader.ReadBEV3(be);
                        break;
                    case 5:
                        byte[] color = new byte[4];
                        color[0] = streamReader.Read<byte>();
                        color[1] = streamReader.Read<byte>();
                        color[2] = streamReader.Read<byte>();
                        color[3] = streamReader.Read<byte>();
                        var colorUnk = streamReader.ReadBEV2(be);

                        genMat.diffuseRGBA = new Vector4((float)color[0] / 255, (float)color[1] / 255, (float)color[2] / 255, (float)color[3] / 255);
                        break;
                    case 6:
                        //Console.WriteLine($"ambient?");
                        var amb = streamReader.ReadBEV3(be);
                        break;
                    case 7:
                        //Console.WriteLine($"Specular?");
                        var spec = streamReader.ReadBEV3(be);
                        break;
                    default:
                        Console.WriteLine($"Unexpected mat type {type}");
                        def0 = streamReader.Read<int>();
                        def1 = streamReader.Read<int>();
                        def2 = streamReader.Read<int>();
                        break;
                }
            }
            //Compare against existing materials and only add if different
            for (int i = 0; i < aqObj.tempMats.Count; i++)
            {
                if (aqObj.tempMats[i] == genMat)
                {
                    return i;
                }
            }
            aqObj.tempMats.Add(genMat);

            if (genMat.texNames.Count == 0)
            {
                Console.WriteLine("No texture name on mat " + (aqObj.tempMats.Count - 1) + " at: " + streamReader.Position);
            }

            return aqObj.tempMats.Count - 1;
        }
    }
}
