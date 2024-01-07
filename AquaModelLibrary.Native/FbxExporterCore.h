#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Numerics;
using namespace AquaModelLibrary::Data::PSO2::Aqua;

namespace AquaModelLibrary::Objects::Processing::Fbx
{
    public ref class FbxExporterCore
    {
        FbxManager* lManager;

    public:
        FbxExporterCore();
        ~FbxExporterCore();

        virtual void ExportToFile(AquaObject^ aqo, AquaNode^ aqn, List<AquaMotion^>^ aqmList, String^ destinationFilePath, List<String^>^ aqmNameList, List<Matrix4x4>^ instanceTransforms, bool includeMetadata);
        virtual void ExportToFileSets(List<AquaObject^>^ aqoList, List<AquaNode^>^ aqnList, List<String^>^ modelNames, String^ destinationFilePath, List<List<Matrix4x4>^>^ instanceTransformsList, bool includeMetadata);
    };
    
    void SetUVChannel(fbxsdk::FbxMesh* lMesh, System::Collections::Generic::List<System::Numerics::Vector2>^ uvList, int count, int uvNum);
    void SetUVChannelShorts(fbxsdk::FbxMesh* lMesh, System::Collections::Generic::List<array<short>^ >^ uvList, int count, int uvNum);
}