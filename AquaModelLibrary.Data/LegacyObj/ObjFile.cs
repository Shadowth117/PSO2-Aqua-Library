//Borrowed from Aqp2obj (Name is a misnomer, program did partial conversions both ways via use of intermediary files)

using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.LegacyObj
{
    public class ObjFile
    {
        public List<ObjMesh> Meshes { get; private set; }
        public string FileName { get; private set; }
        public string CurrentName { get; private set; }
        public ObjMesh CurrentMesh { get; private set; }
        public ObjSubMesh CurrentSubMesh { get; private set; }
        public MtlFile MtlData { get; private set; }
        public List<Vector3> Positions { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<Vector3> TexCoords { get; set; }

        public ObjFile()
        {
            Meshes = new List<ObjMesh>();
            Positions = new List<Vector3>();
            Normals = new List<Vector3>();
            TexCoords = new List<Vector3>();
        }

        public static ObjFile FromFile(string file)
        {
            using (var r = new StreamReader(file, Encoding.Default))
                return ObjFile.FromReader(r);
        }

        public static ObjFile FromReader(StreamReader r)
        {
            var obj = new ObjFile();
            obj.Load(r);
            return obj;
        }

        private void Load(StreamReader r)
        {
            if (r.BaseStream is FileStream)
                FileName = ((FileStream)r.BaseStream).Name;

            var delim = " \t".ToArray();

            for (; ; )
            {
                var line = r.ReadLine();

                if (null == line)
                    break;

                line = line.Trim();

                if (line.Length == 0)
                    continue;

                if (line[0] == '#')
                    continue;

                LineReaded(line.Split(delim, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        private void LineReaded(string[] t)
        {
            switch (t[0])
            {
                case "mtllib": LoadMtlLib(t[1]); break;
                case "v": AddVector3(Positions, t); break;
                case "vn": AddVector3(Normals, t); break;
                case "vt": AddVector3(TexCoords, t); break;

                case "o": CurrentName = t[1]; break;
                case "g": CurrentName = t[1]; break;

                case "usemtl": NewMesh("").Material = MtlData == null ? null : MtlData.Get(t[1]); break;  // usemtl mat0

                case "s": NewSubMesh(0).Name = CurrentName; break;  // s 1
                case "f": GetCurrentSubMesh().AddFace(t); break;  // f 1/1/1 2/2/2 3/3/3 

                //Account for blender line output
                case "l": break;

                default: throw new FormatException();
            }
        }

        private void LoadMtlLib(string file)
        {
            var mtlfile = Path.IsPathRooted(file) ? file
                : FileName == null ? file : Path.Combine(Path.GetDirectoryName(FileName), file);

            if (File.Exists(mtlfile))
                MtlData = MtlFile.FromFile(mtlfile);
        }

        public void AddVector3(List<Vector3> v, string[] t)
        {
            v.Add(new Vector3(float.Parse(t[1]), float.Parse(t[2]), t.Length <= 3 ? 0 : float.Parse(t[3])));
        }

        public void AddVector2(List<Vector2> v, string[] t)
        {
            v.Add(new Vector2(float.Parse(t[1]), float.Parse(t[2])));
        }

        private ObjMesh GetCurrentMesh()
        {
            return null == CurrentMesh ? NewMesh("noname") : CurrentMesh;
        }

        private ObjSubMesh GetCurrentSubMesh()
        {
            return null == CurrentSubMesh ? NewSubMesh(0) : CurrentSubMesh;
        }

        private ObjMesh NewMesh(string name)
        {
            Meshes.Add(CurrentMesh = new ObjMesh(this) { Name = name });
            CurrentSubMesh = null;
            return CurrentMesh;
        }

        private ObjSubMesh NewSubMesh(int smooth)
        {
            var mesh = GetCurrentMesh();
            mesh.SubMeshes.Add(CurrentSubMesh = new ObjSubMesh(mesh) { SmoothGroup = smooth });
            return CurrentSubMesh;
        }
    }

    public class ObjMesh
    {
        public ObjFile Owner { get; private set; }
        public List<ObjSubMesh> SubMeshes { get; private set; }
        public string Name { get; set; }
        public MtlMaterial Material { get; set; }

        public ObjMesh(ObjFile owner)
        {
            Owner = owner;
            SubMeshes = new List<ObjSubMesh>();
        }

        public void MergeSubMeshes()
        {
            var submesh = new ObjSubMesh(this);

            submesh.PositionFaces = SubMeshes.SelectMany(i => i.PositionFaces).ToList();
            submesh.NormalFaces = SubMeshes.SelectMany(i => i.NormalFaces).ToList();
            submesh.TexCoordFaces = SubMeshes.SelectMany(i => i.TexCoordFaces).ToList();

            SubMeshes.Clear();
            SubMeshes.Add(submesh);
        }
    }

    public class ObjSubMesh
    {
        public ObjMesh Owner { get; private set; }
        public string Name { get; set; }
        public List<Face> PositionFaces { get; set; }
        public List<Face> NormalFaces { get; set; }
        public List<Face> TexCoordFaces { get; set; }
        public int SmoothGroup { get; set; }
        public int FaceCount { get { return PositionFaces.Count; } }

        public ObjSubMesh(ObjMesh owner)
        {
            Owner = owner;
            PositionFaces = new List<Face>();
            NormalFaces = new List<Face>();
            TexCoordFaces = new List<Face>();
        }

        public void AddFace(string[] t)
        {
            var a = t[1].Split('/');
            var b = t[2].Split('/');
            var c = t[3].Split('/');

            if (a.Length >= 1 && a[0].Length > 0 && b[0].Length > 0 && c[0].Length > 0)
                PositionFaces.Add(ParseFace(a[0], b[0], c[0]));
            else PositionFaces.Add(new Face(0, 0, 0));

            if (a.Length >= 2 && a[1].Length > 0 && b[1].Length > 0 && c[1].Length > 0)
                TexCoordFaces.Add(ParseFace(a[1], b[1], c[1]));
            else TexCoordFaces.Add(new Face(0, 0, 0));

            if (a.Length >= 3 && a[2].Length > 0 && b[2].Length > 0 && c[2].Length > 0)
                NormalFaces.Add(ParseFace(a[2], b[2], c[2]));
            else NormalFaces.Add(new Face(0, 0, 0));
        }

        public static Face ParseFace(string a, string b, string c)
        {
            return new Face(int.Parse(a) - 1, int.Parse(b) - 1, int.Parse(c) - 1);
        }
    }

    public class Face
    {
        public int A, B, C;

        public Face()
        {
        }

        public Face(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
}
