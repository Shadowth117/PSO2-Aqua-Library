using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// MorphemeBundle type
    /// </summary>
    public enum eBundleType : int
    {
        Bundle_Invalid = 0,
        Bundle_SkeletonMap = 1,
        Bundle_MessageIndices = 2,
        Bundle_DiscreteEventTrack = 3,
        Bundle_DurationEventTrack = 4,
        Bundle_CharacterControllerDef = 7,
        Bundle_Network = 10,
        Bundle_FileHeader = 12,
        Bundle_FileNameLookupTable = 13
    };

    /// <summary>
    /// The type of a particular NMB node.
    /// </summary>
    public enum NodeType : int
    {
        NodeType_NetworkInstance = 9,
        NodeType_StateMachine = 10,
        NodeType_ControlParameterFloat = 20,
        NodeType_ControlParameterVector3 = 21,
        NodeType_ControlParameterBool = 23,
        NodeType_ControlParameterInt = 24,
        NodeType_NodeAnimSyncEvents = 104,
        Nodetype_ShareChildren_105 = 105,
        NodeType_Blend2SyncEvents = 107,
        NodeType_Blend2Additive = 108,
        NodeType_Share1Child1InputCP_109,
        NodeType_ShareCreateFloatOutputAttribute_110 = 110,
        NodeType_ShareCreateFloatOutputAttribute_112 = 112,
        NodeType_Blend2Additive_2 = 114,
        NodeType_TwoBoneIK = 120,
        NodeType_LockFoot = 121,
        NodeType_ShareChildren1CompulsoryManyOptionalInputCPs_120 = 122,
        NodeType_Share1Child1InputCP = 125,
        NodeType_Freeze = 126,
        NodeType_ShareChildrenOptionalInputCPs = 129,
        NodeType_Switch = 131,
        NodeType_ShareChildren = 134,
        NodeType_ShareChildren_2 = 135,
        NodeType_ShareUpdateConnections1Child2OptionalInputCP = 136,
        NodeType_PredictiveUnevenTerrain = 138,
        NodeType_OperatorSmoothDamp = 142,
        NodeType_ShareCreateVector3OutputAttribute = 144,
        NodeType_OperatorRandomFloat = 146,
        NodeType_ShareChildren1CompulsoryManyOptionalInputCPs_150 = 150,
        NodeType_ShareChild1InputCP_151 = 151,
        NodeType_ShareChildren_153 = 153,
        NodeType_SubtractiveBlend = 170,
        NodeType_TransitSyncEvents = 400,
        NodeType_Transit = 402,
        NodeType_Share1Child1OptionalInputCP = 500,
        Unk550 = 550,
    };
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
