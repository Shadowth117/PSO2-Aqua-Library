using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
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
        public Dictionary<string, StageDefinition> GetDefsDict() { return defs.ToDictionary(def => def.missionName); }

		/// <summary>
		/// A special StageCommonData that defines the fields for the others in the file.
		/// </summary>
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

				if (def.commonDataOffset > 0)
				{
					if(commonDataDict.ContainsKey(def.commonDataOffset))
					{
						def.commonData = commonDataDict[def.commonDataOffset];
					} else
                    {
                        sr.Seek(8 + def.commonDataOffset, SeekOrigin.Begin);
                        var commonData = ReadCommonData(sr, encoding);
                        def.commonData = commonData;
                        commonDataDict.Add(def.commonDataOffset, commonData);
                    }
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
			List<int> offsets = new List<int>();
			var outBytes = new List<byte>();
			ByteListExtension.AddAsBigEndian = true;
			outBytes.AddValue((ushort)defs.Count);
			outBytes.AddValue((ushort)0);
			offsets.Add(outBytes.Count);
			outBytes.AddValue((int)8);
			for (int i = 0; i < defs.Count; i++)
			{
				var def = defs[i];

				offsets.Add(outBytes.Count);
				offsets.Add(outBytes.Count + 0x4);
				offsets.Add(outBytes.Count + 0x8);
				offsets.Add(outBytes.Count + 0xC);

				offsets.Add(outBytes.Count + 0x10);
				offsets.Add(outBytes.Count + 0x14);
				offsets.Add(outBytes.Count + 0x18);
				offsets.Add(outBytes.Count + 0x1C);

				offsets.Add(outBytes.Count + 0x20);
				offsets.Add(outBytes.Count + 0x24);
				offsets.Add(outBytes.Count + 0x28);
				offsets.Add(outBytes.Count + 0x2C);

				offsets.Add(outBytes.Count + 0x30);
				offsets.Add(outBytes.Count + 0x34);
				offsets.Add(outBytes.Count + 0x38);

				outBytes.ReserveInt($"{i}MissionName");
				outBytes.ReserveInt($"{i}CommonData");
				outBytes.ReserveInt($"{i}WorldName");
				outBytes.ReserveInt($"{i}MissionType");

				outBytes.ReserveInt($"{i}LndName");
				outBytes.ReserveInt($"{i}BspName");
				outBytes.ReserveInt($"{i}Mc2Name");
				outBytes.ReserveInt($"{i}SetObjName");

				outBytes.ReserveInt($"{i}SetEnemyName");
				outBytes.ReserveInt($"{i}SetCameraName");
				outBytes.ReserveInt($"{i}SetDesignName");
				outBytes.ReserveInt($"{i}PathName");

				outBytes.ReserveInt($"{i}EventCameraName");
				outBytes.ReserveInt($"{i}MessageName");
				outBytes.ReserveInt($"{i}EventName");

				outBytes.AddValue(def.player1Start.playerPosition);
				outBytes.AddValue(def.player1Start.rotation);
				outBytes.AddValue(def.player2Start.playerPosition);
				outBytes.AddValue(def.player2Start.rotation);
				outBytes.AddValue(def.player3Start.playerPosition);
				outBytes.AddValue(def.player3Start.rotation);
				outBytes.AddValue(def.player4Start.playerPosition);
				outBytes.AddValue(def.player4Start.rotation);

				outBytes.AddValue(def.rankTimeThreshold);
				outBytes.AddValue(def.scoreThreshold1);
				outBytes.AddValue(def.scoreThreshold2);
				outBytes.AddValue(def.scoreThreshold3);
				outBytes.AddValue(def.scoreThreshold4);
			}
			if(definition != null)
			{
				offsets.Add(outBytes.Count);
				offsets.Add(outBytes.Count + 0x4);
				offsets.Add(outBytes.Count + 0x8);
				offsets.Add(outBytes.Count + 0xC);

				offsets.Add(outBytes.Count + 0x10);
				offsets.Add(outBytes.Count + 0x14);
				offsets.Add(outBytes.Count + 0x18);

				outBytes.ReserveInt($"DefBank4");
				outBytes.ReserveInt($"DefBank6");
				outBytes.ReserveInt($"DefBank7");
				outBytes.ReserveInt($"DefParticle");
				outBytes.ReserveInt($"DefEffect");
				outBytes.ReserveInt($"DefObjectDefinition");
				outBytes.ReserveInt($"DefObjectData");
			}

			//Here we go through and make sure we don't write redundant StageCommonData
			//We temporarily store the commonData id in the offset field since it's not used for anything else after being read.
			List<StageCommonData> commonDataList = new List<StageCommonData>();
			List<int> commonOffsets = new List<int>();
			for (int i = 0; i < defs.Count; i++)
			{
				var def = defs[i];
				if(def.commonData == null)
				{
					def.commonDataOffset = -1;
					continue;
				}
				var defCom = def.commonData;
				int match = -1;
				for(int j = 0; j < commonDataList.Count; j++)
				{
					var cmn = commonDataList[j];
					if(cmn.effect == defCom.effect && cmn.objectData == defCom.objectData && cmn.objectDefinition == defCom.objectDefinition 
						&& cmn.particle == defCom.particle && cmn.SEBank4 == defCom.SEBank4 && cmn.SEBank6 == defCom.SEBank6 && cmn.SEBank7 == defCom.SEBank7)
					{
						match = j;
						break;
					}
				}
				if (match == -1)
				{
					commonOffsets.Add(outBytes.Count);
					outBytes.FillInt($"{i}CommonData", outBytes.Count);
					offsets.Add(outBytes.Count);
					offsets.Add(outBytes.Count + 0x4);
					offsets.Add(outBytes.Count + 0x8);
					offsets.Add(outBytes.Count + 0xC);

					offsets.Add(outBytes.Count + 0x10);
					offsets.Add(outBytes.Count + 0x14);
					offsets.Add(outBytes.Count + 0x18);

					outBytes.ReserveInt($"{commonDataList.Count}Bank4");
					outBytes.ReserveInt($"{commonDataList.Count}Bank6");
					outBytes.ReserveInt($"{commonDataList.Count}Bank7");
					outBytes.ReserveInt($"{commonDataList.Count}Particle");
					outBytes.ReserveInt($"{commonDataList.Count}Effect");
					outBytes.ReserveInt($"{commonDataList.Count}ObjectDefinition");
					outBytes.ReserveInt($"{commonDataList.Count}ObjectData");
					commonDataList.Add(defCom);
				} else
				{
					outBytes.FillInt($"{i}CommonData", commonOffsets[match]);
				}
			}

			//Write out string data
			outBytes.AlignWriter(0x20);
			var encoding = Encoding.GetEncoding("shift-jis");
			Dictionary<string, int> stringTracker = new Dictionary<string, int>();
			for(int i = 0; i < defs.Count; i++)
			{
				var def = defs[i];
				outBytes.FillInt($"{i}MissionName", WriteUniqueString(outBytes, stringTracker, def.missionName, encoding));
				outBytes.FillInt($"{i}WorldName", WriteUniqueString(outBytes, stringTracker, def.worldName, encoding));
				outBytes.FillInt($"{i}MissionType", WriteUniqueString(outBytes, stringTracker, def.missionType, encoding));

				outBytes.FillInt($"{i}LndName", WriteUniqueString(outBytes, stringTracker, def.lndFilename, encoding));
				outBytes.FillInt($"{i}BspName", WriteUniqueString(outBytes, stringTracker, def.bspFilename, encoding));
				outBytes.FillInt($"{i}Mc2Name", WriteUniqueString(outBytes, stringTracker, def.mc2Filename, encoding));
				outBytes.FillInt($"{i}SetObjName", WriteUniqueString(outBytes, stringTracker, def.setObjFilename, encoding));

				outBytes.FillInt($"{i}SetEnemyName", WriteUniqueString(outBytes, stringTracker, def.setEnemyFilename, encoding));
				outBytes.FillInt($"{i}SetCameraName", WriteUniqueString(outBytes, stringTracker, def.setCameraFilename, encoding));
				outBytes.FillInt($"{i}SetDesignName", WriteUniqueString(outBytes, stringTracker, def.setDesignFilename, encoding));
				outBytes.FillInt($"{i}PathName", WriteUniqueString(outBytes, stringTracker, def.pathFilename, encoding));

                outBytes.FillInt($"{i}EventCameraName", WriteUniqueString(outBytes, stringTracker, def.eventCameraFilename, encoding));
                outBytes.FillInt($"{i}MessageName", WriteUniqueString(outBytes, stringTracker, def.messageFilename, encoding));
                outBytes.FillInt($"{i}EventName", WriteUniqueString(outBytes, stringTracker, def.eventFilename, encoding));
            }
			if(definition != null)
			{
				outBytes.FillInt($"DefBank4", WriteUniqueString(outBytes, stringTracker, definition.SEBank4, encoding));
				outBytes.FillInt($"DefBank6", WriteUniqueString(outBytes, stringTracker, definition.SEBank6, encoding));
				outBytes.FillInt($"DefBank7", WriteUniqueString(outBytes, stringTracker, definition.SEBank7, encoding));
				outBytes.FillInt($"DefParticle", WriteUniqueString(outBytes, stringTracker, definition.particle, encoding));

				outBytes.FillInt($"DefEffect", WriteUniqueString(outBytes, stringTracker, definition.effect, encoding));
				outBytes.FillInt($"DefObjectDefinition", WriteUniqueString(outBytes, stringTracker, definition.objectDefinition, encoding));
				outBytes.FillInt($"DefObjectData", WriteUniqueString(outBytes, stringTracker, definition.objectData, encoding));
			}
			for(int i = 0; i < commonDataList.Count; i++)
			{
				var cmn = commonDataList[i];
				outBytes.FillInt($"{i}Bank4", WriteUniqueString(outBytes, stringTracker, cmn.SEBank4, encoding));
				outBytes.FillInt($"{i}Bank6", WriteUniqueString(outBytes, stringTracker, cmn.SEBank6, encoding));
				outBytes.FillInt($"{i}Bank7", WriteUniqueString(outBytes, stringTracker, cmn.SEBank7, encoding));
				outBytes.FillInt($"{i}Particle", WriteUniqueString(outBytes, stringTracker, cmn.particle, encoding));
				outBytes.FillInt($"{i}Effect", WriteUniqueString(outBytes, stringTracker, cmn.effect, encoding));
				outBytes.FillInt($"{i}ObjectDefinition", WriteUniqueString(outBytes, stringTracker, cmn.objectDefinition, encoding));
				outBytes.FillInt($"{i}ObjectData", WriteUniqueString(outBytes, stringTracker, cmn.objectData, encoding));
			}
			outBytes.AlignWriter(0x4);

			var njBytes = new List<byte>() { 0x47, 0x45, 0x53, 0x44 };
			njBytes.AddRange(BitConverter.GetBytes(outBytes.Count));
			outBytes.InsertRange(0, njBytes);
			outBytes.AddRange(POF0.GeneratePOF0(offsets));

            ByteListExtension.Reset();
            return outBytes.ToArray();
		}

		private static int WriteUniqueString(List<byte> outBytes, Dictionary<string, int> stringTracker, string stringToWrite, Encoding encoding)
		{
			if (stringToWrite == null)
			{
				return 0;
			}
			if(stringTracker.ContainsKey(stringToWrite))
			{
				return stringTracker[stringToWrite];
			} else
			{
				var newOffset = outBytes.Count;
				outBytes.AddRange(encoding.GetBytes(stringToWrite));
				outBytes.Add(0);
				stringTracker.Add(stringToWrite, newOffset);
				return newOffset;
			}
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
