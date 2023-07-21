using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace AquaModelLibrary.Noesis
{
    public unsafe class AMLNoesis
    {
        public static int NOESIS_PLUGIN_VERSION = 3;
        public static string g_pPluginName = "PSO2";
        public static string g_pPluginDesc = "Phantasy Star Online 2: New Genesis - aqp, aqo, trp, tro, prm, prx, tcb| Phantasy Star Nova - axs, aif| Phantasy Star Universe .xnj, .xnr (model)| Demon's Souls (2020) .cmsh| plugin by Shadowth117";
        //=========================================
        //Main Noesis interface
        //=========================================

        //called by Noesis to init the plugin
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "NPAPI_Init")]
        public static bool NPAPI_Init(mathImpFn_s* mathfn, noePluginFn_s* noepfn)
        {
            var api = new NoesisFunctions(mathfn, noepfn);

            //var desc = Marshal.StringToBSTR("PSO2 model (.aqp, .aqo, .trp, .tro)");
            //var types = Marshal.StringToBSTR(".aqp;.aqo;.trp;.tro"); 
            
            //pso2 and ngs models
            //var fh = api.npAPI_Register((byte*)desc.ToPointer(), (byte*)types.ToPointer());
            
            //set the data handlers for this format
            //api.NPAPI_SetTypeHandler_TypeCheck(fh, Model_FF9_Check);
            //api.NPAPI_SetTypeHandler_LoadModel(fh, Model_FF9_Load);
            
            return true;
        }

        //called by Noesis before the plugin is freed
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "NPAPI_Shutdown")]
        public static void NPAPI_Shutdown()
        {
            return;
        }

        //returns current version
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "NPAPI_GetPluginVer")]
        public static int NPAPI_GetPluginVer()
        {
            return NOESIS_PLUGIN_VERSION;
        }

        //copies off plugin info strings
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "NPAPI_GetPluginInfo")]
        public static bool NPAPI_GetPluginInfo(noePluginInfo_s* infOut)
        {
            StringCopy(g_pPluginName, 64, infOut->pluginName);
            StringCopy(g_pPluginDesc, 512, infOut->pluginDesc);
            return true;
        }
        /*
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) }, EntryPoint = "NPAPI_GetPluginInfo")]
        public static bool AquaModelCheck(byte* fileBuffer, nint bufferLen, noeRAPI_t* rapi)
        {
            RAPIObj rapiObj = new RAPIObj(rapi);
            var fname = rapiObj.Noesis_GetInputNameW();
            using (Stream stream = new UnmanagedMemoryStream(fileBuffer, bufferLen))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                var result = AquaGeneralMethods.ReadAquaHeader(streamReader, Path.GetExtension(fname));

                if(result == "NIFL" || result == "VTBF")
                {
                    return true;
                } else
                {
                    return false;
                }
            }
        }
        */
        public static void StringCopy(string str, int count, byte* strLocation)
        {
            byte[] strArr = Encoding.UTF8.GetBytes(str);
            for (int i = 0; i < count; i++)
            {
                if (i < strArr.Length)
                {
                    strLocation[i] = strArr[i];
                }
                else
                {
                    strLocation[i] = 0;
                }
            }
        }

    }
}