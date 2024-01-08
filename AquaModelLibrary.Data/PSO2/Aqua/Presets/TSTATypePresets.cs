using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.Presets
{
    //All of these should be considered a last resort in the case that the shader ones can't be found
    public class TSTATypePresets
    {
        //A last resort TSTA if nothing else can be found 
        public static TSTA defaultPreset = new TSTA() { tag = 150, texUsageOrder = 0, modelUVSet = 0, unkVector0 = new Vector3(0f, 0f, 1f), unkFloat2 = 0f, unkFloat3 = 0f, unkFloat4 = 0f, unkInt3 = 1, unkInt4 = 1, unkInt5 = 1, unkFloat0 = 0f, unkFloat1 = 0f, };

        public static Dictionary<string, TSTA> tstaTypeDict = new Dictionary<string, TSTA>() //A Dictionary of TSTAs with default paramters for texture types.
        {
            {"d", new TSTA() {tag = 150, texUsageOrder = 0, modelUVSet = 0, unkVector0 = new Vector3(0f, 0f, 1f), unkFloat2 = 0f, unkFloat3 = 0f, unkFloat4 = 0f,unkInt3 = 1, unkInt4 = 1, unkInt5 = 1, unkFloat0 = 0f, unkFloat1 = 0f,} },
            {"s", new TSTA() {tag = 19, texUsageOrder = 3, modelUVSet = 0, unkVector0 = new Vector3(0f, 0f, 1f), unkFloat2 = 0f, unkFloat3 = 0f, unkFloat4 = 0f,unkInt3 = 1, unkInt4 = 1, unkInt5 = 1, unkFloat0 = 0f, unkFloat1 = 0f,} },
            {"n", new TSTA() {tag = 23, texUsageOrder = 8, modelUVSet = 0, unkVector0 = new Vector3(0f, 0f, 1f), unkFloat2 = 0f, unkFloat3 = 0f, unkFloat4 = 0f,unkInt3 = 1, unkInt4 = 1, unkInt5 = 1, unkFloat0 = 0f, unkFloat1 = 0f,} },
            {"m", new TSTA() {tag = 19, texUsageOrder = 3, modelUVSet = 0, unkVector0 = new Vector3(0f, -7f, 1f), unkFloat2 = 0f, unkFloat3 = 0f, unkFloat4 = 0f,unkInt3 = 1, unkInt4 = 1, unkInt5 = 1, unkFloat0 = 0f, unkFloat1 = 0f,} }
        };
    }
}
