using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.BillyHatcher
{
    public class StageDef
    {
        public NinjaHeader njHeader;
        public ushort defCount;
        public ushort unkSht;
        public int defOffset;
        public List<StageDefinition> defs = new List<StageDefinition>();
        public Dictionary<string, StageDefinition> defsDict = new Dictionary<string, StageDefinition>();
        public StageCommonData definition = null;
        /// <summary>
        /// Due to the nature of these, it's more efficient to store with the offset as the key 
        /// </summary>
        public Dictionary<int, StageCommonData> commonDataDict = new Dictionary<int, StageCommonData>();

        public StageDef() { }

        public StageDef(string filePath)
        {
            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(filePath)))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(streamReader);
            }
        }

        public StageDef(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(streamReader);
            }
        }

        public StageDef(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var encoding = Encoding.GetEncoding("shift-jis");
            sr._BEReadActive = true;
            njHeader = new NinjaHeader();
            njHeader.magic = sr.Read<int>();
            njHeader.fileSize = sr.Read<int>();

            defCount = sr.ReadBE<ushort>();
            unkSht = sr.ReadBE<ushort>();
            defOffset = sr.ReadBE<int>();

            sr.Seek(8 + defOffset, SeekOrigin.Begin);
            for (int i = 0; i < defCount; i++)
            {
                var def = new StageDefinition()
                {
                    missionNameOffset = sr.ReadBE<int>(),
                    commonDataOffset = sr.ReadBE<int>(),
                    worldNameOffset = sr.ReadBE<int>(),
                    missionTypeOffset = sr.ReadBE<int>(),

                    lndFilenameOffset = sr.ReadBE<int>(),
                    bspFilenameOffset = sr.ReadBE<int>(),
                    mc2FilenameOffset = sr.ReadBE<int>(),
                    setObjFilenameOffset = sr.ReadBE<int>(),

                    setEnemyFilenameOffset = sr.ReadBE<int>(),
                    setCameraFilenameOffset = sr.ReadBE<int>(),
                    setDesignFilenameOffset = sr.ReadBE<int>(),
                    pathFilenameOffset = sr.ReadBE<int>(),

                    eventCameraFilenameOffset = sr.ReadBE<int>(),
                    messageFilenameOffset = sr.ReadBE<int>(),
                    eventFilenameOffset = sr.ReadBE<int>(),
                    player1Start = new PlayerStart()
                    {
                        playerPosition = sr.ReadBEV3(),
                        rotation = sr.ReadBE<float>(),
                    },
                    player2Start = new PlayerStart()
                    {
                        playerPosition = sr.ReadBEV3(),
                        rotation = sr.ReadBE<float>(),
                    },
                    player3Start = new PlayerStart()
                    {
                        playerPosition = sr.ReadBEV3(),
                        rotation = sr.ReadBE<float>()
,
                    },
                    player4Start = new PlayerStart()
                    {
                        playerPosition = sr.ReadBEV3(),
                        rotation = sr.ReadBE<float>(),
                    },
                    rankTimeThreshold = sr.ReadBE<int>(),
                    scoreThreshold1 = sr.ReadBE<int>(),
                    scoreThreshold2 = sr.ReadBE<int>(),
                    scoreThreshold3 = sr.ReadBE<int>(),
                    scoreThreshold4 = sr.ReadBE<int>()
                };
                defs.Add(def);
            }
            definition = ReadCommonData(sr, encoding);

            foreach (var def in defs)
            {
                if (def.missionNameOffset > 0)
                {
                    sr.Seek(8 + def.missionNameOffset, SeekOrigin.Begin);
                    def.missionName = sr.ReadCString();
                }

                if (def.commonDataOffset > 0 && !commonDataDict.ContainsKey(def.commonDataOffset))
                {
                    sr.Seek(8 + def.commonDataOffset, SeekOrigin.Begin);
                    var commonData = ReadCommonData(sr, encoding);
                    def.commonData = commonData;
                    commonDataDict.Add(def.commonDataOffset, commonData);
                }

                if (def.worldNameOffset > 0)
                {
                    sr.Seek(8 + def.worldNameOffset, SeekOrigin.Begin);
                    def.worldName = sr.ReadCString();
                }
                if (def.missionTypeOffset > 0)
                {
                    sr.Seek(8 + def.missionTypeOffset, SeekOrigin.Begin);
                    def.missionType = sr.ReadCString();
                }

                if (def.lndFilenameOffset > 0)
                {
                    sr.Seek(8 + def.lndFilenameOffset, SeekOrigin.Begin);
                    def.lndFilename = sr.ReadCString();
                }
                if (def.bspFilenameOffset > 0)
                {
                    sr.Seek(8 + def.bspFilenameOffset, SeekOrigin.Begin);
                    def.bspFilename = sr.ReadCString();
                }
                if (def.mc2FilenameOffset > 0)
                {
                    sr.Seek(8 + def.mc2FilenameOffset, SeekOrigin.Begin);
                    def.mc2Filename = sr.ReadCString();
                }
                if (def.setObjFilenameOffset > 0)
                {
                    sr.Seek(8 + def.setObjFilenameOffset, SeekOrigin.Begin);
                    def.setObjFilename = sr.ReadCString();
                }

                if (def.setEnemyFilenameOffset > 0)
                {
                    sr.Seek(8 + def.setEnemyFilenameOffset, SeekOrigin.Begin);
                    def.setEnemyFilename = sr.ReadCString();
                }
                if (def.setCameraFilenameOffset > 0)
                {
                    sr.Seek(8 + def.setCameraFilenameOffset, SeekOrigin.Begin);
                    def.setCameraFilename = sr.ReadCString();
                }
                if (def.setDesignFilenameOffset > 0)
                {
                    sr.Seek(8 + def.setDesignFilenameOffset, SeekOrigin.Begin);
                    def.setDesignFilename = sr.ReadCString();
                }
                if (def.pathFilenameOffset > 0)
                {
                    sr.Seek(8 + def.pathFilenameOffset, SeekOrigin.Begin);
                    def.pathFilename = sr.ReadCString();
                }

                if (def.eventCameraFilenameOffset > 0)
                {
                    sr.Seek(8 + def.eventCameraFilenameOffset, SeekOrigin.Begin);
                    def.eventCameraFilename = sr.ReadCString();
                }
                if (def.messageFilenameOffset > 0)
                {
                    sr.Seek(8 + def.messageFilenameOffset, SeekOrigin.Begin);
                    def.messageFilename = sr.ReadCString();
                }
                if (def.eventFilenameOffset > 0)
                {
                    sr.Seek(8 + def.eventFilenameOffset, SeekOrigin.Begin);
                    def.eventFilename = sr.ReadCString();
                }

                defsDict.Add(def.missionName, def);
            }

        }

        private static StageCommonData ReadCommonData(BufferedStreamReaderBE<MemoryStream> sr, Encoding encoding)
        {
            var commonData = new StageCommonData();
            commonData.SEBank4Offset = sr.ReadBE<int>();
            commonData.SEBank6Offset = sr.ReadBE<int>();
            commonData.SEBank7Offset = sr.ReadBE<int>();
            commonData.particleOffset = sr.ReadBE<int>();
            commonData.effectOffset = sr.ReadBE<int>();
            commonData.objectDefinitionOffset = sr.ReadBE<int>();
            commonData.objectDataOffset = sr.ReadBE<int>();

            var bookmark = sr.Position;

            if (commonData.SEBank4Offset > 0)
            {
                sr.Seek(8 + commonData.SEBank4Offset, SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position, 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.SEBank4 = str.Remove(minVal);
            }
            if (commonData.SEBank6Offset > 0)
            {
                sr.Seek(8 + commonData.SEBank6Offset, SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position, 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.SEBank6 = str.Remove(minVal);
            }
            if (commonData.SEBank7Offset > 0)
            {
                sr.Seek(8 + commonData.SEBank7Offset, SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position, 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.SEBank7 = str.Remove(minVal);
            }
            if (commonData.particleOffset > 0)
            {
                sr.Seek(8 + commonData.particleOffset, SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position, 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.particle = str.Remove(minVal);
            }
            if (commonData.effectOffset > 0)
            {
                sr.Seek(8 + commonData.effectOffset, SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position, 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.effect = str.Remove(minVal);
            }
            if (commonData.objectDefinitionOffset > 0)
            {
                sr.Seek(8 + commonData.objectDefinitionOffset, SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position, 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.objectDefinition = str.Remove(minVal);
            }
            if (commonData.objectDataOffset > 0)
            {
                sr.Seek(8 + commonData.objectDataOffset, SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position, 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.objectData = str.Remove(minVal);
            }

            sr.Seek(bookmark, SeekOrigin.Begin);
            return commonData;
        }

        public byte[] GetBytes()
        {
            var outbytes = new List<byte>();
            for (int i = 0; i < defs.Count; i++)
            {

            }

            var njBytes = new List<byte>() { 0x47, 0x45, 0x53, 0x44 };
            njBytes.AddRange(BitConverter.GetBytes(outbytes.Count));
            outbytes.AddRange(njBytes);

            return outbytes.ToArray();
        }

        /// <summary>
        /// Each slot has a Japanese name stored as the first of these.
        /// </summary>
        public class StageCommonData
        {
            /// <summary>
            /// ＳＥバンク４
            /// </summary>
            public int SEBank4Offset;
            /// <summary>
            /// ＳＥバンク６
            /// </summary>
            public int SEBank6Offset;
            /// <summary>
            /// ＳＥバンク7
            /// </summary>
            public int SEBank7Offset;
            /// <summary>
            /// パーティクル
            /// </summary>
            public int particleOffset;
            /// <summary>
            /// エフェクト
            /// </summary>
            public int effectOffset;
            /// <summary>
            /// オブジェクト定義
            /// </summary>
            public int objectDefinitionOffset;
            /// <summary>
            /// オブジェクトデータ
            /// </summary>
            public int objectDataOffset;

            public string SEBank4 = null;
            public string SEBank6 = null;
            public string SEBank7 = null;
            public string particle = null;
            public string effect = null;
            public string objectDefinition = null;
            public string objectData = null;
        }

        public class StageDefinition
        {
            public int missionNameOffset;
            public int commonDataOffset;
            public int worldNameOffset;
            public int missionTypeOffset;

            public int lndFilenameOffset;
            public int bspFilenameOffset;
            public int mc2FilenameOffset;
            public int setObjFilenameOffset;

            public int setEnemyFilenameOffset;
            public int setCameraFilenameOffset;
            public int setDesignFilenameOffset;
            public int pathFilenameOffset;

            public int eventCameraFilenameOffset;
            public int messageFilenameOffset;
            public int eventFilenameOffset;
            public PlayerStart player1Start;
            public PlayerStart player2Start;
            public PlayerStart player3Start;
            public PlayerStart player4Start;

            public int rankTimeThreshold;

            public int scoreThreshold1;
            public int scoreThreshold2;
            public int scoreThreshold3;
            public int scoreThreshold4;

            //Offset strings
            public string missionName = null;
            public string worldName = null;
            public string missionType = null;

            public string lndFilename = null;
            public string bspFilename = null;
            public string mc2Filename = null;
            public string setObjFilename = null;

            public string setEnemyFilename = null;
            public string setCameraFilename = null;
            public string setDesignFilename = null;
            public string pathFilename = null;

            public string eventCameraFilename = null;
            public string messageFilename = null;
            public string eventFilename = null;

            public StageCommonData commonData;
        }

        public struct PlayerStart
        {
            public Vector3 playerPosition;
            /// <summary>
            /// 0-360 degrees
            /// </summary>
            public float rotation;
        }
    }
}
