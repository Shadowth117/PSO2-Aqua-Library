using AquaModelLibrary.Helpers.Writers;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher.SetData
{
    public class SetCameraList
    {
        public List<SetCamera> setCameras = new List<SetCamera>();
        public SetCameraList() { }
        public SetCameraList(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public SetCameraList(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            while (sr.Position + 0x80 <= sr.BaseStream.Length)
            {
                SetCamera setCamera = new SetCamera();
                setCamera.camtype = sr.ReadBE<ushort>();
                setCamera.campriority = sr.ReadBE<ushort>();
                setCamera.interpIN = sr.ReadBE<float>();
                setCamera.interpOUT = sr.ReadBE<float>();

                setCamera.camTargX = sr.ReadBE<float>();
                setCamera.camPosX_camTargY_camSwivelTargX = sr.ReadBEV<float>();
                setCamera.camPosY_camTargZ_camSwivelTargYZ = sr.ReadBE<float>();
                setCamera.camPosZ_camPosBX_camOffsetY = sr.ReadBE<float>();
                setCamera.camPosBY_camOffsetZ = sr.ReadBE<float>();
                setCamera.camPosBZ = sr.ReadBE<float>();

                setCamera.flt_paddingA = sr.ReadBE<float>();
                setCamera.flt_paddingB = sr.ReadBE<float>();

                setCamera.c_stickpadding = sr.ReadBE<ushort>();
                setCamera.c_stickflag = sr.ReadBE<ushort>();

                setCamera.TriggerPosition = sr.ReadBEV3();
                setCamera.TriggerBAMS_RotX = sr.ReadBE<ushort>();
                setCamera.TriggerBAMS_RotY = sr.ReadBE<ushort>();

                setCamera.TriggerShapepadding = sr.ReadBE<ushort>();
                setCamera.TriggerShapeFlag = sr.ReadBE<ushort>();
                setCamera.TriggerRadiusXYZ = sr.ReadBEV3();

                setCamera.angslicepadding = sr.ReadBE<ushort>();
                setCamera.angsliceflag = sr.ReadBE<ushort>();
                setCamera.angsliceYrange = sr.ReadBE<float>();

                setCamera.unknownA = sr.ReadBE<float>();
                setCamera.unknownB = sr.ReadBE<float>();
                setCamera.unknownC = sr.ReadBE<float>();
                setCamera.int_64 = sr.ReadBE<int>();
                setCamera.int_68 = sr.ReadBE<int>();
                setCamera.int_6C = sr.ReadBE<int>();

                setCamera.int_70 = sr.ReadBE<int>();
                setCamera.int_74 = sr.ReadBE<int>();
                setCamera.int_78 = sr.ReadBE<int>();
                setCamera.int_7C = sr.ReadBE<int>();

                setCameras.Add(setCamera);
            }
        }

        public byte[] GetBytes()
        {
            var outBytes = new ByteListWriter() { AddAsBigEndian = true };
            foreach (var setCam in setCameras)
            {
                outBytes.AddValue(setCam.camtype);
                outBytes.AddValue(setCam.campriority);
                outBytes.AddValue(setCam.interpIN);
                outBytes.AddValue(setCam.interpOUT);
                outBytes.AddValue(setCam.camTargX);
                outBytes.AddValue(setCam.camPosX_camTargY_camSwivelTargX);
                outBytes.AddValue(setCam.camPosY_camTargZ_camSwivelTargYZ);
                outBytes.AddValue(setCam.camPosZ_camPosBX_camOffsetY);
                outBytes.AddValue(setCam.camPosBY_camOffsetZ);
                outBytes.AddValue(setCam.camPosBZ);
                outBytes.AddValue(setCam.flt_paddingA);
                outBytes.AddValue(setCam.flt_paddingB);
                outBytes.AddValue(setCam.c_stickpadding);
                outBytes.AddValue(setCam.c_stickflag);
                outBytes.AddValue(setCam.TriggerPosition);
                outBytes.AddValue(setCam.TriggerBAMS_RotX);
                outBytes.AddValue(setCam.TriggerBAMS_RotY);
                outBytes.AddValue(setCam.TriggerShapepadding);
                outBytes.AddValue(setCam.TriggerShapeFlag);
                outBytes.AddValue(setCam.TriggerRadiusXYZ);
                outBytes.AddValue(setCam.angslicepadding);
                outBytes.AddValue(setCam.angsliceflag);
                outBytes.AddValue(setCam.angsliceYrange);
                outBytes.AddValue(setCam.unknownA);
                outBytes.AddValue(setCam.unknownB);
                outBytes.AddValue(setCam.unknownC);
                outBytes.AddValue(setCam.int_64);
                outBytes.AddValue(setCam.int_68);
                outBytes.AddValue(setCam.int_6C);
                outBytes.AddValue(setCam.int_70);
                outBytes.AddValue(setCam.int_74);
                outBytes.AddValue(setCam.int_78);
                outBytes.AddValue(setCam.int_7C);
            }

            return outBytes.ToArray();
        }
    }

    public struct SetCamera
    {
        public ushort camtype;
        /// <summary>
        /// Type 0 nulls the volume and later entries. 1-5 are specialized types with unique properites, and 6 is a variant of default camera seen outside of cam volumes
        /// </summary>
        public ushort campriority;
        /// <summary>
        /// Usually 1, else it crashes. Controls priority if overlapping another Cam volume.
        /// </summary>
        public float interpIN;
        /// <summary>
        /// Controls how fast the camera updates to incoming volume position/rotation properties.
        /// </summary>
        public float interpOUT;
        /// <summary>
        /// Controls how fast the camera updates to default cam when exiting volumes. Overrided by other interpIN if entering another volume.
        /// </summary>

        public float camTargX;
        /// <summary>
        /// Controls camera lookat target world X position for cam type 5.
        /// </summary>
        public float CamPosX_CamTargY_CamSwivelTargX;
        /// <summary>
        /// Controls camera world X position for type 4, camera lookat target world Y position for type 5, and camera lookat target world X position for cam types 2 and 3. For cam type 1 it controls X angle, Billy used as pivot point
        /// </summary>
        public float CamPosY_CamTargZ_CamSwivelTargYZ;
        /// <summary>
        /// Controls camera world Y position for type 4, camera lookat target world Z position for type 5, camera lookat target world Y position for cam type 2, and camera lookat target world Z position for cam type 3. For cam type 1 it controls Z offset to Billy.
        /// </summary>
        public float CamPosZ_CamPosBX_CamOffsetY;
        /// <summary>
        /// Controls camera world Z position for type 4, camera world X position for cam type 5, and camera local Y offset for cam type 3. For type 1 it controls camera rotation strength for left/right movement.
        /// </summary>
        public float CamPosBY_CamOffsetZ;
        /// <summary>
        /// Controls camera world Y position for cam type 5, and camera local Z offset for cam types 1, 2, and 3.
        /// </summary>
        public float CamPosBZ;
        /// <summary>
        /// Controls camera world Z position for cam type 5.
        /// </summary>

        public float flt_paddingA;
        public float flt_paddingB;

        public ushort c_stickpadding;
        public ushort c_stickflag;
        /// <summary>
        /// Controls if you can use the C-Stick or L Trigger to override the camera, 0 has enables overridding, 1 prevents it. Oddly impacted by cam type where even numbered values prevent overriding for cam types 1+.
        /// </summary>

        public Vector3 TriggerPosition;
        public ushort TriggerBAMS_RotX;
        public ushort TriggerBAMS_RotY;
        /// <summary>
        /// To calculate angle, take the int16 value, divide by 65536, then multiply by 360.
        /// </summary>

        public ushort TriggerShapepadding;
        public ushort TriggerShapeFlag;
        /// <summary>
        /// Determines camera volume shape. 0 is Sphere, 1 is a cylinder, 2 is a box.
        /// </summary>
        public Vector3 TriggerRadiusXYZ;

        public ushort angslicepadding;
        public ushort angsliceflag;
        /// <summary>
        /// Can enable hemisphering a camera volume. 0 disables this, 1 hemispheres 180 degrees, and 2 can allow one to specify Y axis angle slice with the following float.
        /// </summary>
        public float angsliceYrange;


        public float unknownA;
        public float unknownB;
        public float unknownC;
        /// <summary>
        /// Typically set to 1.
        /// </summary>
        public int int_64;
        public int int_68;
        public int int_6C;

        public int int_70;
        public int int_74;
        public int int_78;
        public int int_7C;
    }
}
