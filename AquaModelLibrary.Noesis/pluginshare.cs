using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Noesis
{
    public unsafe class NoesisFunctions
    {
        public delegate nint NPAPI_Register(byte* typeDesc, byte* extList);
        public delegate void Math_CalcPlaneEq(float* x, float* y, float* z, float* planeEq);
        public NPAPI_Register npAPI_Register;
        public Math_CalcPlaneEq math_CalcPlaneEq;

        public NoesisFunctions()
        {

        }

        public NoesisFunctions(mathImpFn_s* mathStr, noePluginFn_s* noeStr)
        {
            npAPI_Register = (NPAPI_Register?)Marshal.GetDelegateForFunctionPointer(noeStr->NPAPI_Register, typeof(nint));
            //math_CalcPlaneEq = (Math_CalcPlaneEq?)Marshal.GetDelegateForFunctionPointer(mathStr->Math_CalcPlaneEq, typeof(void));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct mathImpFn_s
    {
        //public delegate* unmanaged<void> Math_CalcPlaneEq(float* x, float* y, float* z, float* planeEq);
        //public IntPtr Math_CalcPlaneEq;
        public IntPtr Math_Max2;
	    public IntPtr Math_Max3;
		public IntPtr Math_Min2;
		public IntPtr Math_Min3;
		public IntPtr Math_TransformPointByMatrix;
		public IntPtr Math_TransformPointByMatrixNoTrans;
		public IntPtr Math_MatrixInverse;
		public IntPtr Math_TransformPointByMatrix4x4;
		public IntPtr Math_MatrixInverse4x4;
		public IntPtr Math_MatRotDist;
		public IntPtr Math_VecRotDist;
		public IntPtr Math_MatToAngles;
        public IntPtr Math_AnglesToMat;
		public IntPtr Math_AngleVectors;
		public IntPtr Math_MatrixIsSkewed;
		public IntPtr Math_OrthogonalizeMatrix;
		public IntPtr Math_LerpMatricesQ;
		public IntPtr Math_LerpMatrices;
		public IntPtr Math_RotationMatrix;
		public IntPtr Math_MatrixMultiply;
        public IntPtr Math_4x4ToModelMat;
		public IntPtr Math_ModelMatToGL;
		public IntPtr Math_ModelMatFromGL;
		public IntPtr Math_TransposeMat;
		public IntPtr Math_TranslateMatrix;
		public IntPtr Math_RotateMatrix;
		public IntPtr Math_MatrixMultiply4x4;
        public IntPtr Math_RotationMatrix4x4;
		public IntPtr Math_TranslateMatrix4x4;
		public IntPtr Math_RotateMatrix4x4;
		public IntPtr Math_AxisForNormal;
		public IntPtr Math_ExpandBounds;
		public IntPtr Math_MaxExtent;
		public IntPtr Math_PointInOnBox;
		public IntPtr Math_BoxesOverlap;
		public IntPtr Math_PlaneFromPoints;
		public IntPtr Math_ConfinePointToBox;
		public IntPtr Math_BuildBoxPlanes2D;
		public IntPtr Math_VecCopy;
		public IntPtr Math_VecSub;
		public IntPtr Math_VecSub2;
		public IntPtr Math_VecAdd;
		public IntPtr Math_VecScale;
		public IntPtr Math_VecScaleVec;
		public IntPtr Math_VecNorm;
		public IntPtr Math_DotProduct;
        public IntPtr Math_CrossProduct;
        public IntPtr Math_VecLen;
		public IntPtr Math_VecLenSq;
		public IntPtr Math_VecLen2;
		public IntPtr Math_VecMA;
		public IntPtr Math_VecToAngles;
        public IntPtr Math_ProjectOntoPlane;
		public IntPtr Math_PointInTriPlanes;
		public IntPtr Math_ConfinePointToTri;
        public IntPtr Math_GetTriEdgeFracs;
		public IntPtr Math_ConstantLerp;
		public IntPtr Math_LinearLerp;
		public IntPtr Math_BilinearLerp;
		public IntPtr Math_TriLerp;
		public IntPtr Math_CubicLerp;
		public IntPtr Math_PlaneDist;
        public IntPtr Math_GetFloat16;
		public IntPtr Math_QuatToMat;
		public IntPtr Math_MatToQuat;
		public IntPtr Math_QuatSlerp;
		public IntPtr Math_NextPow2;
		public IntPtr Math_CheckPointInTri;
		public IntPtr Math_ExpandTriangle;
		public IntPtr Math_GetMappedValue;
		public IntPtr Math_RandSetSeed;
        public IntPtr Math_RandInt;
		public IntPtr Math_RandFloat;
		public IntPtr Math_RandIntOnSeed;
		public IntPtr Math_RandFloatOnSeed;
		public IntPtr Math_RandIntUnseeded;
		public IntPtr Math_RandFloatUnseeded;

		//noesis 2.6 and later
		public IntPtr Math_AngleMod;
		public IntPtr Math_BlendAngleLinear;
        public IntPtr Math_RotateMatrixTP;

		//noesis 3.46 and later
		public IntPtr Math_EncodeFloat16;

		//noesis 3.56 and later
		public IntPtr Math_AnglesToMatAxis;

        //noesis 3.66 and later
        public IntPtr Math_TransformPointByMatrixD;
		public IntPtr Math_TransformPointByMatrixNoTransD;
		public IntPtr Math_MatrixInverseD;
		public IntPtr Math_MatrixMultiplyD;
        public IntPtr Math_MatrixDToMatrix;
		public IntPtr Math_MatrixToMatrixD;

		//noesis 3.862 and later
		public IntPtr Math_Morton2D;

		//noesis 3.996 and later
		public IntPtr Math_WorldToScreenSpace;
		public IntPtr Math_ScreenToWorldSpace;
		public IntPtr Math_PointRelativeToPlane;
		public IntPtr Math_LineIntersectTri;

		//noesis 4.0799 and later
		public IntPtr Math_CatmullRomLerp;
		public IntPtr Math_HermiteLerp;
		public IntPtr Math_Bezier3D;
		public IntPtr Math_ClampInt;
		public IntPtr Math_WrapInt;

		//noesis 4.081 and later
		public IntPtr Math_CubicBezier3D;
		public IntPtr Math_BezierTangent3D;

		//noesis 4.0824 and later
		public IntPtr Math_CatmullRomTangent;
		public IntPtr Math_CatmullRomLerp3D;
		public IntPtr Math_CatmullRomTangent3D;
		public IntPtr Math_CreateProjectionMatrix;

		//noesis 4.143 and later
		//encode/decode functions assume all 0 exponent bits equal 0, and don't handle any other IEEE standards cases.
		//exponent bias for source and destination formats is assumed to be 2^(exponentBits-1)-1.
		public IntPtr Math_EncodeFloatBitsFromBits;
		public IntPtr Math_DecodeFloatFromBits;

		//noesis 4.144 and later

		//get base-e log for v
		public IntPtr Math_Log;

        //get base-2 log for v
        public IntPtr Math_Log2;

        //get base-e log for v (double)
        public IntPtr Math_LogD;

        //get base-2 log for v (double)
        public IntPtr Math_Log2D;

        //convert from linear color space to gamma space
        public IntPtr Math_LinearToGamma;

        //convert from gamma space to linear space
        public IntPtr Math_GammaToLinear;

        //convert from linear color space to gamma space (double)
        public IntPtr Math_LinearToGammaD;

        //convert from gamma space to linear space (double)
        public IntPtr Math_GammaToLinearD;

        //calculates fraction bits with the IEEE-based assumption of a leading 1.
        //because the leading 1 is assumed, fractionValues starting under 1 will return a best-case of 0.
        public IntPtr Math_CalculateFractionBits;

		//as above, assuming 32-bit float.
		public IntPtr Math_CalculateFractionBits32;

        //calculates exponent bits, explicitly clamping between values of 0 and (2^exponentBitCount)-1 under the assumption
        //that those values are reserved as per IEEE 754.
        public IntPtr Math_CalculateExponentBits;

		//as above, assuming 32-bit float.
		public IntPtr Math_CalculateExponentBits32;

        //extracts the fraction and exponent from a 32-bit float.
        public IntPtr Math_ExtractFractionAndExponent32;

        //extracts the fraction and exponent from a 64-bit float.
        public IntPtr Math_ExtractFractionAndExponent64;

		public IntPtr Math_TransformQST;

        public IntPtr Math_GetFirstLastBitSet64;

		public IntPtr Math_SHProjectCubemap;

        //unless otherwise specified, inputs and outputs assume 4-channel rgba
        public IntPtr Math_CreateIrradianceCubemap;
        public IntPtr Math_CreateIrradianceCubemapLambert;
        public IntPtr Math_PrefilterCubemapGGX;
        //Math_SampleSphericalProjectionIntoHDRCubemap flags - 1: flip theta, 2: flip phi
        public IntPtr Math_SampleSphericalProjectionIntoHDRCubemap;

        //calculate approximate derivative of pFn(x)
        public IntPtr Math_CalculateDerivative;
        //calculate definite integral of pFn(x) from xMin to xMax, stopping when delta is < errorTolerance
        public IntPtr Math_CalculateIntegral;

        //DFT & DCT functions are not at all optimized and were done from original formulas as a reference implementation. makes it easier to see data transforms under the hood when doing things
        //properly and not using bit twiddling/look tables/etc. if you have a serious use for these, you should probably be using something like FFTW instead.
        //note that transformed dft input/output (RichComplex) is in the form of complex double*2 numbers. you can cast this memory as std::complex<double> to operate on it.
        public IntPtr Math_DiscreteFourierTransform;
        public IntPtr Math_InverseDiscreteFourierTransform;
        public IntPtr Math_DiscreteFourierTransform2D;
        public IntPtr Math_InverseDiscreteFourierTransform2D;

        public IntPtr Math_DiscreteCosineTransform;
        public IntPtr Math_InverseDiscreteCosineTransform;
        public IntPtr Math_DiscreteCosineTransform2D;
        public IntPtr Math_InverseDiscreteCosineTransform2D;
        public IntPtr Math_DiscreteCosineTransform3D;
        public IntPtr Math_InverseDiscreteCosineTransform3D;

        public IntPtr Math_QuatMultiply;

        //reserved, do not call.
        public IntPtr resvA;
		public IntPtr resvB;
		public IntPtr resvC;
		public IntPtr resvD;
		public IntPtr resvE;
		public IntPtr resvF;
		public IntPtr resvG;
		public IntPtr resvH;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct noePluginFn_s
    {
        //=========================
        //engine-provided functions
        //=========================

        //note that rapi handles can be passed from multiple different and concurrent noesis core instances. you should always deal with the local rapi
        //function set.

        //returns a handle to a new type for this module.
        public IntPtr NPAPI_Register;
		//sets the callback for "is this data of this type?"
		public IntPtr NPAPI_SetTypeHandler_TypeCheck;
		//sets the callback to load model data
		public IntPtr NPAPI_SetTypeHandler_LoadModel;
        //sets the callback to write model data
        public IntPtr NPAPI_SetTypeHandler_WriteModel;
		//sets the callback to load pixel data
		public IntPtr NPAPI_SetTypeHandler_LoadRGBA;
        //sets the callback to write pixel data
        public IntPtr NPAPI_SetTypeHandler_WriteRGBA;
		//sets the callback to write animation data
		public IntPtr NPAPI_SetTypeHandler_WriteAnim;
		//sets the callback to extract an archive
		public IntPtr NPAPI_SetTypeHandler_ExtractArc;

		//gets the handle for the main window. make sure to check if this is non-0, as it can be called before the main Noesis window is created.
		public IntPtr NPAPI_GetMainWnd;

		//gets the noesis api version
		public IntPtr NPAPI_GetAPIVersion;

		//sets the callback to extract an archive in direct-stream mode (new in Noesis 2.5)
		public IntPtr NPAPI_SetTypeHandler_ExtractArcStream;

		//allows you to add advanced/commandline option hooks for your program (new in Noesis 3.22)
		//if successful, returns a pointer to the variable store for the option.
		public IntPtr NPAPI_AddTypeOption;

		//New in Noesis 3.46
		//dst is assumed to be at least MAX_NOESIS_PATH wchars
		public IntPtr NPAPI_GetExecutablePath;
		//returns number of files found, calls fileCallback (if non-null) for each file found. example:
		//NPAPI_EnumerateFiles(noesisPathDir, L"plugins", L"*.dll", myPathHandler);
		public IntPtr NPAPI_EnumerateFiles;

		public IntPtr fileCallback;
		//plugins are allowed a single critical section to avoid processing-previewing conflicts. resv MUST BE 0 for these calls.
		public IntPtr NPAPI_EnterCritical;
		public IntPtr NPAPI_LeaveCritical;
		//allows a plugin to invoke the debug log. resv must be 0.
		public IntPtr NPAPI_PopupDebugLog;
		//logs a debug string
		public IntPtr NPAPI_DebugLogStr;

		//New in Noesis 3.84
		//this handler intercepts *all* image exports for the active export, and feeds them to the batch write handler. This can be useful for compiling animations from an array of textures.
		public IntPtr NPAPI_SetTypeHandler_WriteRGBABatch;

		//New in Noesis 3.89
		//creates and displays a user prompt, returns true if the callback handler approved the input and the user hit ok, otherwise false
		//if the return value is true, the contents of the input are stored in the param structure's valBuf, to be casted to the appropriate type based on the value of valType.
		public IntPtr NPAPI_UserPrompt;
		//displays a message in a standard dialog prompt
		public IntPtr NPAPI_MessagePrompt;
		//returns a handle to a new tool for this module.
		public IntPtr NPAPI_RegisterTool;
		//returns the user data for a tool by index
		public IntPtr NPAPI_GetToolUserData;

		//New in Noesis 3.9
		//dst is assumed to be at least MAX_NOESIS_PATH wchars
		public IntPtr NPAPI_GetScenesPath;
		//tells noesis to open a new file in the main preview view
		public IntPtr NPAPI_OpenFile;

		//New in Noesis 3.93
		//it is recommended that you try to use unique names (for example, prefix all setting names with your plugin name) when reading/writing settings to avoid conflicts with other plugins.
		//writes a chunk of binary for a user plugin
		public IntPtr NPAPI_UserSettingWrite;
		//reads a chunk of binary for a user plugin
		public IntPtr NPAPI_UserSettingRead;
		//performs a task on a job thread. jobHandle may be NULL if you do not need to query for the completion of the job.
		public IntPtr NPAPI_Threads_DoJob;
		//checks if a job is done yet
		public IntPtr NPAPI_Threads_JobDone;
		//runs the windows message pump. should only be used by tools and when you know what you're doing.
		public IntPtr NPAPI_DoPump;
		//returns internal afx app handle. again should only be used when you know what you're doing.
		public IntPtr NPAPI_GetAfxWinApp;

		//New in Noesis 3.97
		//will return null if memory maps are disabled in preferences
		public IntPtr NPAPI_AllocMappedFile;
		public IntPtr NPAPI_DestroyMappedFile;
		//returns engine-allocated rgba buffer
		public IntPtr NPAPI_LoadImageRGBA;
		//returns true if write was successful
		public IntPtr NPAPI_SaveImageRGBA;
		//engine-allocated pointers must be freed with NPAPI_EngineFree
		public IntPtr NPAPI_EngineAlloc;
		public IntPtr NPAPI_EngineFree;
		//module instantiation can be performed from tools. always remember to free the module when you're finished with it, or you will bog Noesis down.
		public IntPtr NPAPI_InstantiateModule;
		public IntPtr NPAPI_FreeModule;
		public IntPtr NPAPI_GetModuleRAPI;
		//fills in a buffer (expected to be MAX_NOESIS_PATH wchars) with the currently selected file in the viewer. "" is no selection is made.
		public IntPtr NPAPI_GetSelectedFile;

		//New in Noesis 3.994
		//sets implicit export options for the format
		public IntPtr NPAPI_SetTypeExportOptions;

		//New in Noesis 3.996
		//sets menu help text for a tool entry
		public IntPtr NPAPI_SetToolHelpText;
		//gets a pointer to the rapi interface for the active preview model. NULL if nothing is loaded.
		public IntPtr NPAPI_GetPreviewRAPI;
		//register a visualizer
		public IntPtr NPAPI_RegisterVisualizer;
		//set visualizer pre model render callback
		public IntPtr NPAPI_Visualizer_SetPreRender;
		//set visualizer post model render callback
		public IntPtr NPAPI_Visualizer_SetPostRender;
		//set visualizer callback for after preview model is loaded (called after successful load)
		public IntPtr NPAPI_Visualizer_SetPreviewLoaded;
		//set visualizer callback for when preview is closed (called before close)
		public IntPtr NPAPI_Visualizer_SetPreviewClose;
		//set visualizer callback for when preview is reset (f12/middlemouse)
		public IntPtr NPAPI_Visualizer_SetPreviewReset;
		//set visualizer callback for new input
		public IntPtr NPAPI_Visualizer_SetInput;
		//checks or unchecks a tool's menu item
		public IntPtr NPAPI_CheckToolMenuItem;

		//New in Noesis 4.02
		//gets a program setting. returns NULL if setting doesn't exist.
		public IntPtr NPAPI_GetProgramSetting;
		//returns true if debug log is open
		public IntPtr NPAPI_DebugLogIsOpen;
		//reloads all plugins
		public IntPtr NPAPI_ReloadPlugins;

		//New in Noesis 4.04
		//dst is assumed to be at least MAX_NOESIS_PATH wchars
		public IntPtr NPAPI_GetPluginsPath;
		//returns true if a forced reload is occurring
		public IntPtr NPAPI_IsTriggeredPluginReload;
		//dst is assumed to be at least MAX_NOESIS_PATH wchars, or dst may be NULL
		public IntPtr NPAPI_GetOpenPreviewFile;

		//New in Noesis 4.061
		//fills in a buffer (expected to be MAX_NOESIS_PATH wchars) with the currently selected directory in the viewer. "" is no selection is made.
		public IntPtr NPAPI_GetSelectedDirectory;

		//New in Noesis 4.066
		public IntPtr Array_Alloc;
		public IntPtr Array_Free;
		public IntPtr Array_SetGrowth;
		public IntPtr Array_QSort;
        public IntPtr Array_GetElement;
		public IntPtr Array_GetElementGrow;
		public IntPtr Array_Append;
        public IntPtr Array_RemoveLast;
		
        public IntPtr Array_Remove;
		public IntPtr Array_GetCount;
		public IntPtr Array_Reset;
		public IntPtr Array_Tighten;

		public IntPtr Stream_Alloc;
		public IntPtr Stream_AllocFixed;
		public IntPtr Stream_Free;
		public IntPtr Stream_WriteBits;
        public IntPtr Stream_WriteBytes;
        public IntPtr Stream_ReadBits;
		public IntPtr Stream_ReadBytes;
		public IntPtr Stream_WriteBool;
		public IntPtr Stream_WriteInt;
		public IntPtr Stream_WriteFloat;
		public IntPtr Stream_WriteString;
        public IntPtr Stream_WriteWString;
        public IntPtr Stream_ReadBool;
		public IntPtr Stream_ReadInt;
		public IntPtr Stream_ReadFloat;
		public IntPtr Stream_ReadString;
		public IntPtr Stream_Buffer;
		public IntPtr Stream_Size;
		public IntPtr Stream_SetOffset;
		public IntPtr Stream_GetOffset;
		public IntPtr Stream_SetFlags;
		public IntPtr Stream_GetFlags;
		public IntPtr Steam_WriteToFile;

		//New in Noesis 4.081
		public IntPtr Noesis_GetCharSplineSet;

        //New in Noesis 4.084
        //should be some combination of NTOOLFLAG values
        public IntPtr NPAPI_SetToolFlags;
		public IntPtr NPAPI_GetToolFlags;
		//sets visibility callback. if it's a context item and a file is selected, focusFileName will be the full path to the selected file.
		//otherwise, the value will be NULL.
		//the callback should return 1 if the menu is visible, otherwise 0.
		public IntPtr NPAPI_SetToolVisibleCallback;
        //returns some combination of NFORMATFLAG values.
        //ext should be the file extension including the dot, e.g. ".png"
        //if the extension is used by multiple format handlers, expect to get flags from all applicable formats.
        //(numHandlers will also be set appropriately if you pass a non-NULL value)
        public IntPtr NPAPI_GetFormatExtensionFlags;
		//tells noesis to open a new file in the main preview view, then delete it
		public IntPtr NPAPI_OpenAndRemoveTempFile;

		//selects a mesh in the data viewer, may be used by tool plugins.
		public IntPtr NPAPI_SelectDataViewerMesh;

		//selects a mesh's material in the data viewer, may be used by tool plugins.
		public IntPtr NPAPI_SelectDataViewerMeshMaterial;

		//selects a bone in the data viewer, may be used by tool plugins.
		public IntPtr NPAPI_SelectDataViewerBone;

		public IntPtr NPAPI_Visualizer_SetRawKeyDownHook;
		public IntPtr NPAPI_Visualizer_SetRawKeyUpHook;
		public IntPtr NPAPI_Visualizer_SetOverrideRenders;

		public IntPtr NPAPI_FileIsLoadable;

		public IntPtr NPAPI_DisableFormatByDescription;

		public IntPtr NPAPI_HighPrecisionTime; //in seconds

		public IntPtr NPAPI_AddUserExtProc;
		public IntPtr NPAPI_GetUserExtProc;

        public IntPtr Stream_SetBitOffset;
		public IntPtr Stream_GetBitOffset;

		public IntPtr NPAPI_PumpModalStatus;
        public IntPtr NPAPI_ClearModalStatus;

		public IntPtr NPAPI_GetResourceHandle;

        public IntPtr NPAPI_GetDirectoryList;

        //returns 1 for file, 2 for directory, otherwise
        public IntPtr NPAPI_PathExists;
        public IntPtr NPAPI_PathNormalize;

		public IntPtr Stream_ReadRevBits;

		public IntPtr NPAPI_SetGlobalMemForRAPI;

		public IntPtr NPAPI_SetToolSubMenuName;

        public IntPtr NPAPI_RegisterVRMenuItem;
		public IntPtr NPAPI_EnterCustomVRMenuState;
        public IntPtr NPAPI_SetCustomVRMenuItem;

        public IntPtr NPAPI_IsSupportedFileExtension;

        public IntPtr NPAPI_OpenDataViewer;
		public IntPtr NPAPI_CloseDataViewer;
		public IntPtr NPAPI_GetDataViewerSetting;
        public IntPtr NPAPI_SetDataViewerSetting;

        //returns true if write was successful
        public IntPtr NPAPI_SaveImageFromTexture;

		//reserved, do not call.
		public IntPtr resvA;
		public IntPtr resvB;
		public IntPtr resvC;
		public IntPtr resvD;
		public IntPtr resvE;
		public IntPtr resvF;
		public IntPtr resvG;
	}

	public unsafe struct noePluginInfo_s
    {
        public fixed byte pluginName[64];
        public fixed byte pluginDesc[512];

        public fixed byte resv[512];
    }
}
