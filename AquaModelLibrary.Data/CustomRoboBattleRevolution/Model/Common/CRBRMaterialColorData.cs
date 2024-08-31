using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRMaterialColorData
    {
        //Colors are probably 4 byte BGRA order. Alpha is definitely the last byte
        //Very possible this is laid out like 3ds Max's legacy standard material info

        /// <summary>
        /// Ambient?
        /// </summary>
        public int ambientColor;
        /// <summary>
        /// Diffuse?
        /// </summary>
        public int diffuseColor;
        /// <summary>
        /// Specular?
        /// </summary>
        public int specularColor;
        /// <summary>
        /// Maybe opacity?
        /// </summary>
        public float unkFloat0;
        /// <summary>
        /// Maybe glossiness?
        /// </summary>
        public float unkFloat1;

        public CRBRMaterialColorData() { }

        public CRBRMaterialColorData(BufferedStreamReaderBE<MemoryStream> sr)
        {
            ambientColor = sr.ReadBE<int>();
            diffuseColor = sr.ReadBE<int>();
            specularColor = sr.ReadBE<int>();
            unkFloat0 = sr.ReadBE<float>();
            unkFloat1 = sr.ReadBE<float>();
        }
    }
}
