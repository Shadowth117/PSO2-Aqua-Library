using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    public class StageDef
    {
        public NinjaHeader njHeader;
        public ushort defCount;
        public ushort unkSht;
        public int defOffset;
        public List<StageDefinition> defs = new List<StageDefinition>();
        public StageCommonData definition = null;
        /// <summary>
        /// Due to the nature of these, it's more efficient to store with the offset as the key 
        /// </summary>
        public Dictionary<int, StageCommonData> commonDataDict = new Dictionary<int, StageCommonData>();

        public StageDef() { }

        public StageDef(string filePath)
        {
            using (Stream stream = new MemoryStream(File.ReadAllBytes(filePath)))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                Read(streamReader);
            }
        }

        public StageDef(byte[] file)
        {
            using (Stream stream = new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                Read(streamReader);
            }
        }

        public StageDef(BufferedStreamReader sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReader sr)
        {
            var encoding = Encoding.GetEncoding("shift-jis");
            BigEndianHelper._active = true;
            njHeader = new NinjaHeader();
            njHeader.magic = sr.Read<int>();
            njHeader.fileSize = sr.Read<int>();

            defCount = sr.ReadBE<ushort>();
            unkSht = sr.ReadBE<ushort>();
            defOffset = sr.ReadBE<int>();

            sr.Seek(8 + defOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < defCount; i++)
            {
                defs.Add(new StageDefinition()
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
,                    },
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
                });
                
            }
            definition = ReadCommonData(sr, encoding);

            foreach (var def in defs)
            {
                if (def.missionNameOffset > 0)
                {
                    sr.Seek(8 + def.missionNameOffset, System.IO.SeekOrigin.Begin);
                    def.missionName = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }
                
                if(def.commonDataOffset > 0 && !commonDataDict.ContainsKey(def.commonDataOffset))
                {
                    sr.Seek(8 + def.commonDataOffset, System.IO.SeekOrigin.Begin);
                    var commonData = ReadCommonData(sr, encoding);
                    def.commonData = commonData;
                    commonDataDict.Add(def.commonDataOffset, commonData);
                }

                if (def.worldNameOffset > 0)
                {
                    sr.Seek(8 + def.worldNameOffset, System.IO.SeekOrigin.Begin);
                    def.worldName = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }
                if (def.missionTypeOffset > 0)
                {
                    sr.Seek(8 + def.missionTypeOffset, System.IO.SeekOrigin.Begin);
                    def.missionType = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }

                if (def.lndFilenameOffset > 0)
                {
                    sr.Seek(8 + def.lndFilenameOffset, System.IO.SeekOrigin.Begin);
                    def.lndFilename = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }
                if (def.bspFilenameOffset > 0)
                {
                    sr.Seek(8 + def.bspFilenameOffset, System.IO.SeekOrigin.Begin);
                    def.bspFilename = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }
                if (def.mc2FilenameOffset > 0)
                {
                    sr.Seek(8 + def.mc2FilenameOffset, System.IO.SeekOrigin.Begin);
                    def.mc2Filename = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }
                if (def.setObjFilenameOffset > 0)
                {
                    sr.Seek(8 + def.setObjFilenameOffset, System.IO.SeekOrigin.Begin);
                    def.setObjFilename = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }

                if (def.setEnemyFilenameOffset > 0)
                {
                    sr.Seek(8 + def.setEnemyFilenameOffset, System.IO.SeekOrigin.Begin);
                    def.setEnemyFilename = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }
                if (def.setCameraFilenameOffset > 0)
                {
                    sr.Seek(8 + def.setCameraFilenameOffset, System.IO.SeekOrigin.Begin);
                    def.setCameraFilename = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }
                if (def.setDesignFilenameOffset > 0)
                {
                    sr.Seek(8 + def.setDesignFilenameOffset, System.IO.SeekOrigin.Begin);
                    def.setDesignFilename = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }
                if (def.pathFilenameOffset > 0)
                {
                    sr.Seek(8 + def.pathFilenameOffset, System.IO.SeekOrigin.Begin);
                    def.pathFilename = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }

                if (def.eventCameraFilenameOffset > 0)
                {
                    sr.Seek(8 + def.eventCameraFilenameOffset, System.IO.SeekOrigin.Begin);
                    def.eventCameraFilename = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }
                if (def.messageFilenameOffset > 0)
                {
                    sr.Seek(8 + def.messageFilenameOffset, System.IO.SeekOrigin.Begin);
                    def.messageFilename = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }
                if (def.eventFilenameOffset > 0)
                {
                    sr.Seek(8 + def.eventFilenameOffset, System.IO.SeekOrigin.Begin);
                    def.eventFilename = AquaMethods.AquaGeneralMethods.ReadCString(sr);
                }
            }

        }

        private static StageCommonData ReadCommonData(BufferedStreamReader sr, Encoding encoding)
        {
            var commonData = new StageCommonData();
            commonData.SEBank4Offset = sr.ReadBE<int>();
            commonData.SEBank6Offset = sr.ReadBE<int>();
            commonData.SEBank7Offset = sr.ReadBE<int>();
            commonData.particleOffset = sr.ReadBE<int>();
            commonData.effectOffset = sr.ReadBE<int>();
            commonData.objectDefinitionOffset = sr.ReadBE<int>();
            commonData.objectDataOffset = sr.ReadBE<int>();

            var bookmark = sr.Position();

            if (commonData.SEBank4Offset > 0)
            {
                sr.Seek(8 + commonData.SEBank4Offset, System.IO.SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position(), 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.SEBank4 = str.Remove(minVal);
            }
            if (commonData.SEBank6Offset > 0)
            {
                sr.Seek(8 + commonData.SEBank6Offset, System.IO.SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position(), 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.SEBank6 = str.Remove(minVal);
            }
            if (commonData.SEBank7Offset > 0)
            {
                sr.Seek(8 + commonData.SEBank7Offset, System.IO.SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position(), 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.SEBank7 = str.Remove(minVal);
            }
            if (commonData.particleOffset > 0)
            {
                sr.Seek(8 + commonData.particleOffset, System.IO.SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position(), 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.particle = str.Remove(minVal);
            }
            if (commonData.effectOffset > 0)
            {
                sr.Seek(8 + commonData.effectOffset, System.IO.SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position(), 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.effect = str.Remove(minVal);
            }
            if (commonData.objectDefinitionOffset > 0)
            {
                sr.Seek(8 + commonData.objectDefinitionOffset, System.IO.SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position(), 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.objectDefinition = str.Remove(minVal);
            }
            if (commonData.objectDataOffset > 0)
            {
                sr.Seek(8 + commonData.objectDataOffset, System.IO.SeekOrigin.Begin);
                var str = encoding.GetString(sr.ReadBytes(sr.Position(), 0x40));
                var minVal = str.IndexOf(char.MinValue);
                commonData.objectData = str.Remove(minVal);
            }

            sr.Seek(bookmark, System.IO.SeekOrigin.Begin);
            return commonData;
        }

        public byte[] GetBytes()
        {
            var outbytes = new List<byte>();
            for(int i = 0; i < defs.Count; i++)
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
