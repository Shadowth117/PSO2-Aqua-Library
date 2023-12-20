using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using static AquaModelLibrary.AquaStructs.MusicFileReboot;

namespace AquaModelLibrary.AquaStructs
{
    public class MusicFileReboot : AquaCommon
    {
        public int musHeaderOffset;
        public MusHeader header;
        public List<Composition> compositions = new List<Composition>();
        public List<TransitionData> transitionData = new List<TransitionData>();
        public List<Conditions> conditionsData = new List<Conditions>();
        public string musString1C = null;
        public string customVariables = null; //Custom variables are a comma separated list with a variable name, =, and an int value.

        public MusicFileReboot()
        {

        }

        public MusicFileReboot(BufferedStreamReader sr)
        {
            var variant = AquaGeneralMethods.ReadAquaHeader(sr, ".mus", out int offset);

            if (variant == "NIFL")
            {
                nifl = sr.Read<AquaCommon.NIFL>();
                rel0 = sr.Read<AquaCommon.REL0>();
                sr.Seek(offset + rel0.REL0DataStart, SeekOrigin.Begin);
                sr.Seek(offset + sr.Read<int>(), SeekOrigin.Begin);
                header = new MusHeader(sr);

                if (header.mus.string_1COffset != 0x10 && header.mus.string_1COffset != 0)
                {
                    sr.Seek(offset + header.mus.string_1COffset, SeekOrigin.Begin);
                    musString1C = AquaGeneralMethods.ReadCString(sr);
                }

                if (header.mus.customVariableStringOffset != 0x10 && header.mus.customVariableStringOffset != 0)
                {
                    sr.Seek(offset + header.mus.customVariableStringOffset, SeekOrigin.Begin);
                    customVariables = AquaGeneralMethods.ReadCString(sr);
                }

                //Read compositions
                sr.Seek(offset + header.mus.compositionOffset, SeekOrigin.Begin);
                for (int i = 0; i < header.mus.compositionCount; i++)
                {
                    compositions.Add(new Composition(sr, offset));
                }

                //Read transitions
                sr.Seek(offset + header.mus.transitionDataOffset, SeekOrigin.Begin);
                for (int i = 0; i < header.mus.transitionDataCount; i++)
                {
                    transitionData.Add(new TransitionData(sr, offset));
                }

                //Read conditions
                sr.Seek(offset + header.mus.conditionDataOffset, SeekOrigin.Begin);
                for (int i = 0; i < header.mus.conditionDataCount; i++)
                {
                    conditionsData.Add(new Conditions(sr, offset));
                }
            }
        }

        public void SetCustomVariables(Dictionary<string, int> customVars)
        {
            StringBuilder customVarBuilder = new StringBuilder();
            int i = 0;
            foreach(var customVar in customVars)
            {
                customVarBuilder.Append($"{customVar.Key}={customVar.Value}");
                if(i != customVars.Count - 1)
                {
                    customVarBuilder.Append(',');
                }
                i++;
            }

            customVariables = customVarBuilder.ToString();
        }

        public Dictionary<string, int> GetCustomVariables()
        {
            var customVars = new Dictionary<string, int>();
            if (customVariables == null)
            {
                return customVars;
            }

            var pairs = customVariables.Split(',');
            foreach (var pair in pairs)
            {
                var pairSplit = pair.Split('=');
                customVars.Add(pairSplit[0], Int32.Parse(pairSplit[1]));
            }

            return customVars;
        }

        public class MusHeader
        {
            public MusHeaderStruct mus;

            public MusHeader()
            {
            }

            public MusHeader(BufferedStreamReader sr)
            {
                mus = sr.Read<MusHeaderStruct>();
            }
        }

        public struct MusHeaderStruct
        {
            public int compositionOffset;
            public byte bt_4;
            public byte compositionCount;
            public byte bt_6;
            public byte bt_7;
            public int int_08;
            public int int_0C;

            public byte bt_10;
            public byte bt_11;
            public byte bt_12;
            public byte transitionDataCount;
            public int transitionDataOffset;
            public byte bt_18;
            public byte bt_19;
            public byte bt_1A;
            public byte bt_1B;
            public int string_1COffset;

            public byte conditionDataCount;
            public byte bt_21;
            public byte bt_22;
            public byte bt_23;
            public int conditionDataOffset;
            public int customVariableStringOffset;
        }

        //Normal, transition, battle, etc.
        public class Composition
        {
            public List<Part> parts = new List<Part>();
            public List<CompositionCondition> compositionConditions = new List<CompositionCondition>();
            public CompositionStruct compositionStruct;

            public Composition() { }

            public Composition(BufferedStreamReader sr, int offset)
            {
                compositionStruct = sr.Read<CompositionStruct>();

                var bookmark = sr.Position();

                sr.Seek(offset + compositionStruct.partOffset, SeekOrigin.Begin);
                for (int i = 0; i < compositionStruct.partCount; i++)
                {
                    parts.Add(new Part(sr, offset));
                }

                sr.Seek(offset + compositionStruct.compositionConditionOffset, SeekOrigin.Begin);
                for (int i = 0; i < compositionStruct.compositionConditionCount; i++)
                {
                    compositionConditions.Add(new CompositionCondition(sr, offset));
                }
                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public struct CompositionStruct
        {
            public int partOffset;
            public int compositionConditionOffset;
            public byte partCount;
            public byte bt_9;
            public byte compositionConditionCount;
            public byte bt_B;
        }

        public class CompositionCondition
        {
            public string conditionString;
            public CompositionConditionStruct compositionConditionStruct;

            public CompositionCondition()
            {

            }

            public CompositionCondition(BufferedStreamReader sr, int offset)
            {
                compositionConditionStruct = sr.Read<CompositionConditionStruct>();

                var bookmark = sr.Position();

                if (compositionConditionStruct.conditionStringOffset != 0x10 && compositionConditionStruct.conditionStringOffset != 0)
                {
                    sr.Seek(offset + compositionConditionStruct.conditionStringOffset, SeekOrigin.Begin);
                    conditionString = AquaGeneralMethods.ReadCString(sr);
                }
                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public struct CompositionConditionStruct
        {
            public int conditionStringOffset;
            public byte bt_4;
            public byte bt_5;
            public byte bt_6;
            public byte bt_7;
            public int int_08;
            public int int_0C;
            public int int_10;
        }

        //Switchable
        public class Part
        {
            public List<Movement> movements = new List<Movement>();
            public string conditionString;
            public PartStruct partStruct;

            public Part()
            {

            }

            public Part(BufferedStreamReader sr, int offset)
            {
                partStruct = sr.Read<PartStruct>();

                var bookmark = sr.Position();

                sr.Seek(offset + partStruct.movementOffset, SeekOrigin.Begin);
                for (int i = 0; i < partStruct.movementCount; i++)
                {
                    movements.Add(new Movement(sr, offset));
                }

                if (partStruct.conditionStringOffset != 0x10 && partStruct.conditionStringOffset != 0)
                {
                    sr.Seek(offset + partStruct.conditionStringOffset, SeekOrigin.Begin);
                    conditionString = AquaGeneralMethods.ReadCString(sr);
                }
                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public struct PartStruct
        {
            public int movementOffset;
            public byte movementCount;
            public byte bt_5;
            public byte bt_6;
            public byte bt_7;

            public int conditionStringOffset;
            public int int_0C;

            public int int_10;
            public int int_14;
            public int int_18;
            public int int_1C;

            public int int_20;
            public int int_24;
            public int int_28;

        }

        //Movement (Shuffle, skip operations etc.)
        public class Movement
        {
            public List<Phrase> phrases = new List<Phrase>();
            public MovementStruct movementStruct;

            public Movement()
            {

            }

            public Movement(BufferedStreamReader sr, int offset)
            {
                movementStruct = sr.Read<MovementStruct>();

                var bookmark = sr.Position();

                sr.Seek(offset + movementStruct.phraseOffset, SeekOrigin.Begin);
                for (int i = 0; i < movementStruct.phraseCount; i++)
                {
                    phrases.Add(new Phrase(sr, offset));
                }

                sr.Seek(bookmark, SeekOrigin.Begin);
            }

        }

        public struct MovementStruct
        {
            public int phraseOffset;
            public int phraseCount;
        }

        //Phrase
        public class Phrase
        {
            public List<Bar> bars = new List<Bar>();
            public PhraseStruct phraseStruct;

            public Phrase()
            {

            }

            public Phrase(BufferedStreamReader sr, int offset)
            {
                phraseStruct = sr.Read<PhraseStruct>();

                var bookmark = sr.Position();

                sr.Seek(offset + phraseStruct.barOffset, SeekOrigin.Begin);
                for(int i = 0; i < phraseStruct.barCount; i++)
                {
                    bars.Add(new Bar(sr, offset));
                }

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public struct PhraseStruct
        {
            public int barOffset;
            public int barCount;
        }

        //Bar
        public class Bar
        {
            public List<Clip> mainClips = new List<Clip>();
            public List<Clip> subClips = new List<Clip>();
            public BarStruct barStruct;

            public Bar()
            {

            }

            public Bar(BufferedStreamReader sr, int offset)
            {
                barStruct = sr.Read<BarStruct>();

                var bookmark = sr.Position();

                sr.Seek(offset + barStruct.mainClipOffset, SeekOrigin.Begin);
                for(int i = 0; i < barStruct.mainClipCount; i++)
                {
                    mainClips.Add(new Clip(sr, offset));
                }

                sr.Seek(offset + barStruct.subClipOffset, SeekOrigin.Begin);
                for (int i = 0; i < barStruct.subClipCount; i++)
                {
                    subClips.Add(new Clip(sr, offset));
                }

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public struct BarStruct
        {
            public int mainClipOffset;
            public int subClipOffset; //Usually nothing here
            public short beatsPerMinute;
            public short beat; //Sega's name for it
            public byte mainClipCount; //Flag for if main clip was used
            public byte subClipCount; //Flag for if sub clip was used
            public byte meVolume; //Sega's name for it
            public byte rhVolume; //Sega's name for it
        }

        //Clip
        public class Clip
        {
            public string clipFileName;
            public ClipStruct clipStruct;

            public Clip()
            {

            }

            public Clip(BufferedStreamReader sr, int offset)
            {
                clipStruct = sr.Read<ClipStruct>();
                
                var bookmark = sr.Position();
                if (clipStruct.clipFileNameOffset != 0x10 && clipStruct.clipFileNameOffset != 0)
                {
                    sr.Seek(offset + clipStruct.clipFileNameOffset, SeekOrigin.Begin);
                    clipFileName = AquaGeneralMethods.ReadCString(sr);
                }
                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public struct ClipStruct
        {
            public int clipFileNameOffset;
            public byte volume; //? Typically 100
            public byte bt_6;  //These are probably boolean flags
            public byte bt_7;
            public byte bt_8;
        }

        public class TransitionData
        {
            public TransitionDataStruct transStruct;
            public TransitionDataSubStruct transSubStruct;
            public string transitionClipName;
            public string subStructStr_04;

            public TransitionData()
            {

            }

            public TransitionData(BufferedStreamReader sr, int offset)
            {
                transStruct = sr.Read<TransitionDataStruct>();

                var bookmark = sr.Position();
                if (transStruct.transitionClipNameOffset != 0x10 && transStruct.transitionClipNameOffset != 0)
                {
                    sr.Seek(offset + transStruct.transitionClipNameOffset, SeekOrigin.Begin);
                    transitionClipName = AquaGeneralMethods.ReadCString(sr);
                }

                if(transStruct.transitionDataSubstructOffset != 0x10 && transStruct.transitionDataSubstructOffset > 0)
                {
                    sr.Seek(offset + transStruct.transitionDataSubstructOffset, SeekOrigin.Begin);
                    transSubStruct = sr.Read<TransitionDataSubStruct>();

                    if (transSubStruct.str_04 != 0x10 && transSubStruct.str_04 != 0)
                    {
                        sr.Seek(offset + transSubStruct.str_04, SeekOrigin.Begin);
                        subStructStr_04 = AquaGeneralMethods.ReadCString(sr);
                    }
                }

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public struct TransitionDataStruct
        {
            public int int_00;
            public int transitionDataSubstructOffset;
            public float flt_08;
            public int transitionClipNameOffset;
        }

        public struct TransitionDataSubStruct
        {
            public int int_00;
            public int str_04;
        }

        public class Conditions
        {
            public ConditionsStruct conditionstr;
            public string description;
            public string str_14;
            public string str_18;
            public string str_20;
            public string str_28;

            public Conditions()
            {

            }

            public Conditions(BufferedStreamReader sr, int offset)
            {
                conditionstr = sr.Read<ConditionsStruct>();

                var bookmark = sr.Position();
                if (conditionstr.descriptionStrOffset != 0x10 && conditionstr.descriptionStrOffset != 0)
                {
                    sr.Seek(offset + conditionstr.descriptionStrOffset, SeekOrigin.Begin);
                    description = AquaGeneralMethods.ReadCString(sr);
                }
                if (conditionstr.str_14 != 0x10 && conditionstr.str_14 != 0)
                {
                    sr.Seek(offset + conditionstr.str_14, SeekOrigin.Begin);
                    str_14 = AquaGeneralMethods.ReadCString(sr);
                }
                if (conditionstr.str_18 != 0x10 && conditionstr.str_18 != 0)
                {
                    sr.Seek(offset + conditionstr.str_18, SeekOrigin.Begin);
                    str_18 = AquaGeneralMethods.ReadCString(sr);
                }
                if (conditionstr.str_20 != 0x10 && conditionstr.str_20 != 0)
                {
                    sr.Seek(offset + conditionstr.str_20, SeekOrigin.Begin);
                    str_20 = AquaGeneralMethods.ReadCString(sr);
                }
                if (conditionstr.str_28 != 0x10 && conditionstr.str_28 != 0)
                {
                    sr.Seek(offset + conditionstr.str_28, SeekOrigin.Begin);
                    str_28 = AquaGeneralMethods.ReadCString(sr);
                }

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public struct ConditionsStruct
        {
            public int int_00;
            public int descriptionStrOffset;
            public int valueType;
            public float valueOne;

            public float valueTwo;
            public int str_14;
            public int str_18;
            public int int_1C;

            public int str_20;
            public byte bt_24;
            public byte bt_25;
            public byte bt_26;
            public byte bt_27;
            public int str_28;
            public int int_2C;
        }

    }
}
