#pragma once

using namespace System;
using namespace AquaModelLibrary::Native::Fbx::Interfaces;

namespace AquaModelLibrary::Objects::Processing::Fbx
{
    public ref class FbxExporterCore sealed : public IFbxExporter
    {
        FbxManager* lManager;

    public:
        FbxExporterCore();
        ~FbxExporterCore();

        virtual void ExportToFile( AquaObject^ aqo, AquaNode^ aqn, String^ destinationFilePath, bool includeMetadata);
    };
}