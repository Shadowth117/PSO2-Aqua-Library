using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class StateMappingObject
    {
        public StateMapping stateMappingStruct;
        public string name = null;
        public List<CommandObject> commands = new List<CommandObject>();
        public List<EffectMapObject> effects = new List<EffectMapObject>();
        public List<AnimMapObject> anims = new List<AnimMapObject>();
        public StateMappingObject() { }

        public StateMappingObject(int offset, int address, BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr.Seek(offset + address, SeekOrigin.Begin);
            stateMappingStruct = sr.Read<StateMapping>();
            name = sr.ReadCStringValidOffset(stateMappingStruct.namePtr, offset);
            commands = new List<CommandObject>();
            if (stateMappingStruct.commandPtr > 0x10)
            {
                for (int i = 0; i < stateMappingStruct.commandCount; i++)
                {
                    long ptr = sr.Read<int>();
                    var bookmark = sr.Position;
                    if (ptr > 0x10)
                    {
                        commands.Add(new CommandObject(offset, ptr, sr));
                    }
                    sr.Seek(bookmark, SeekOrigin.Begin);
                }
            }
            effects = new List<EffectMapObject>();
            if (stateMappingStruct.effectMapPtr > 0x10)
            {
                for (int i = 0; i < stateMappingStruct.effectMapCount; i++)
                {
                    long ptr = sr.Read<int>();
                    var bookmark = sr.Position;
                    if (ptr > 0x10)
                    {
                        effects.Add(new EffectMapObject(offset, ptr, sr));
                        if (!effects[i].knownType)
                        {
                            Debug.WriteLine($"Undefined effect type '{effects[i].type}' found! Index {i}, address {address:X}");
                        }
                    }
                    sr.Seek(bookmark, SeekOrigin.Begin);
                }
            }
            anims = new List<AnimMapObject>();
            if (stateMappingStruct.animMapPtr > 0x10)
            {
                for (int i = 0; i < stateMappingStruct.animMapCount; i++)
                {
                    long ptr = sr.Read<int>();
                    var bookmark = sr.Position;
                    if (ptr > 0x10)
                    {
                        anims.Add(new AnimMapObject(offset, ptr, sr));
                    }
                    sr.Seek(bookmark, SeekOrigin.Begin);
                }
            }
        }
    }
}
