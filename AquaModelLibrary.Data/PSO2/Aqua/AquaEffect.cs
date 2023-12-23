using AquaModelLibrary.Data.PSO2.Aqua.AquaEffectData;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //PSO2 Implementation of Glitter particle effect files. Shares striking similarity to Project Diva variations.
    //May be entirely different than the NIFL variation 
    //Seemingly, the file seems to be an efct header followed by an emit nodes, their animations and particle nodes with their animations.
    //There should be at least one EFCT, one EMIT, and one PTCL per file while they must all have ANIMs, null or not.
    public unsafe class AquaEffect : AquaCommon
    {
        public EFCTObject efct = null;
    }
}
