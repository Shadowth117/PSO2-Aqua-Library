#include "FbxExporterCore.h"
#include "Utf8String.h"

using namespace Collections::Generic;
using namespace IO;
using namespace Numerics;
using namespace Runtime::InteropServices;
using namespace AquaModelLibrary;

namespace AquaModelLibrary::Objects::Processing::Fbx
{

    FbxAMatrix CreateFbxAMatrixFromNumerics( Matrix4x4 matrix )
    {
        FbxAMatrix lMatrix;

        lMatrix.mData[ 0 ][ 0 ] = matrix.M11; lMatrix.mData[ 0 ][ 1 ] = matrix.M12; lMatrix.mData[ 0 ][ 2 ] = matrix.M13; lMatrix.mData[ 0 ][ 3 ] = matrix.M14;
        lMatrix.mData[ 1 ][ 0 ] = matrix.M21; lMatrix.mData[ 1 ][ 1 ] = matrix.M22; lMatrix.mData[ 1 ][ 2 ] = matrix.M23; lMatrix.mData[ 1 ][ 3 ] = matrix.M24;
        lMatrix.mData[ 2 ][ 0 ] = matrix.M31; lMatrix.mData[ 2 ][ 1 ] = matrix.M32; lMatrix.mData[ 2 ][ 2 ] = matrix.M33; lMatrix.mData[ 2 ][ 3 ] = matrix.M34;
        lMatrix.mData[ 3 ][ 0 ] = matrix.M41; lMatrix.mData[ 3 ][ 1 ] = matrix.M42; lMatrix.mData[ 3 ][ 2 ] = matrix.M43; lMatrix.mData[ 3 ][ 3 ] = matrix.M44;

        return lMatrix;
    }

    FbxLayer* GetFbxLayer( FbxMesh* lMesh, int pIndex )
    {
        FbxLayer* lLayer = lMesh->GetLayer( pIndex );

        if ( !lLayer )
        {
            while ( lMesh->CreateLayer() != pIndex );
            lLayer = lMesh->GetLayer( pIndex );
        }

        return lLayer;
    }
    
    FbxNode* CreateFbxNodeFromMesh( AquaObject^ aqo, int meshId, FbxScene* lScene, List<IntPtr>^ materials, List<int>^ meshMatMappings, List<IntPtr>^ convertedBones, AquaNode^ aqn, bool includeMetadata)
    {
        List<uint>^ bonePalette;
        AquaObject::MESH msh = aqo->meshList[meshId];
        AquaObject::VTXL^ vtxl = aqo->vtxlList[msh.vsetIndex];
        String^ meshName;
        if (includeMetadata)
        {
            meshName = String::Format("mesh[{4}]_{0}_{1}_{2}_{3}#{5}#{6}", msh.mateIndex, msh.rendIndex, msh.shadIndex, msh.tsetIndex, meshId, msh.baseMeshNodeId, msh.baseMeshDummyId);
        }
        else {
            meshName = String::Format("mesh[{0}]", meshId);
        }
        FbxNode* lNode = FbxNode::Create( lScene, Utf8String(meshName).ToCStr() );
        FbxMesh* lMesh = FbxMesh::Create( lScene, Utf8String(meshName + "_mesh" ).ToCStr() );
        bool hasVertexWeights = vtxl->vertWeightIndices->Count > 0;

        lMesh->InitControlPoints( vtxl->vertPositions->Count );

        if (aqo->objc.bonePaletteOffset > 0)
        {
            bonePalette = aqo->bonePalette;
        }
        else {
            bonePalette = gcnew List<uint>();
            for (int bn = 0; bn < vtxl->bonePalette->Count; bn++)
            {
                bonePalette->Add(vtxl->bonePalette[bn]);
            }
        }

        for ( int i = 0; i < vtxl->vertPositions->Count; i++ )
        {
            Vector3 position = vtxl->vertPositions[ i ];
            lMesh->SetControlPointAt( FbxVector4( position.X, position.Y, position.Z ), i );
        }

        // Elements need to be created in this exact order for 3DS Max to read it properly.

        if (vtxl->vertNormals->Count > 0 )
        {
            FbxGeometryElementNormal* lElementNormal = ( FbxGeometryElementNormal* ) GetFbxLayer( lMesh, 0 )->CreateLayerElementOfType( FbxLayerElement::eNormal );
            lElementNormal->SetMappingMode( FbxLayerElement::eByControlPoint );
            lElementNormal->SetReferenceMode( FbxLayerElement::eDirect );

            for each (Vector3 normal in vtxl->vertNormals)
            {
                lElementNormal->GetDirectArray().Add(FbxVector4(normal.X, normal.Y, normal.Z));
            }
        }

        FbxGeometryElementVertexColor* lElementVertexColor = nullptr;
        FbxGeometryElementVertexColor* lElementVertexColor2 = nullptr;

        if (vtxl->vertColors->Count > 0)
        {
            lElementVertexColor = (FbxGeometryElementVertexColor*)GetFbxLayer(lMesh, 0)->CreateLayerElementOfType(FbxLayerElement::eVertexColor);

            // Vertex color elements need to use these modes for 3DS Max to read them properly. Anything else is not going to work.
            //lElementVertexColor2->SetName("VCChannel_0"); //Remove this if the name breaks it
            lElementVertexColor->SetMappingMode(FbxLayerElement::eByPolygonVertex);
            lElementVertexColor->SetReferenceMode(FbxLayerElement::eIndexToDirect);

            for each (array<unsigned char>^ color in vtxl->vertColors)
                lElementVertexColor->GetDirectArray().Add(FbxColor(((float)color[2]) / 255, ((float)color[1]) / 255, ((float)color[0]) / 255, ((float)color[3]) / 255));
        }
        
        if (vtxl->vertColor2s->Count > 0)
        {
            lElementVertexColor2 = (FbxGeometryElementVertexColor*)GetFbxLayer(lMesh, 1)->CreateLayerElementOfType(FbxLayerElement::eVertexColor);

            // Vertex color elements need to use these modes for 3DS Max to read them properly. Anything else is not going to work.
            //lElementVertexColor2->SetName("VCChannel_1");
            lElementVertexColor2->SetMappingMode(FbxLayerElement::eByPolygonVertex);
            lElementVertexColor2->SetReferenceMode(FbxLayerElement::eIndexToDirect);

            for each (array<unsigned char> ^ color in vtxl->vertColor2s)
                lElementVertexColor->GetDirectArray().Add(FbxColor(((float)color[2]) / 255, ((float)color[1]) / 255, ((float)color[0]) / 255, ((float)color[3]) / 255));
        }
        
        if (vtxl->uv1List->Count > 0)
        {
            FbxGeometryElementUV* lElementUV = (FbxGeometryElementUV*)GetFbxLayer(lMesh, 0)->CreateLayerElementOfType(FbxLayerElement::eUV);
            lElementUV->SetName(Utf8String(String::Format("UVChannel_1")).ToCStr());
            lElementUV->SetMappingMode(FbxLayerElement::eByControlPoint);
            lElementUV->SetReferenceMode(FbxLayerElement::eDirect);

            for each (Vector2 texCoord in vtxl->uv1List)
                lElementUV->GetDirectArray().Add(FbxVector2(texCoord.X, 1 - texCoord.Y));
        }

        if (vtxl->uv2List->Count > 0)
        {
            FbxGeometryElementUV* lElementUV = (FbxGeometryElementUV*)GetFbxLayer(lMesh, 1)->CreateLayerElementOfType(FbxLayerElement::eUV);
            lElementUV->SetName(Utf8String(String::Format("UVChannel_2")).ToCStr());
            lElementUV->SetMappingMode(FbxLayerElement::eByControlPoint);
            lElementUV->SetReferenceMode(FbxLayerElement::eDirect);

            for each (Vector2 texCoord in vtxl->uv2List)
                lElementUV->GetDirectArray().Add(FbxVector2(texCoord.X, 1 - texCoord.Y));
        }

        if (vtxl->uv3List->Count > 0)
        {
            FbxGeometryElementUV* lElementUV = (FbxGeometryElementUV*)GetFbxLayer(lMesh, 2)->CreateLayerElementOfType(FbxLayerElement::eUV);
            lElementUV->SetName(Utf8String(String::Format("UVChannel_3")).ToCStr());
            lElementUV->SetMappingMode(FbxLayerElement::eByControlPoint);
            lElementUV->SetReferenceMode(FbxLayerElement::eDirect);

            for each (Vector2 texCoord in vtxl->uv3List)
                lElementUV->GetDirectArray().Add(FbxVector2(texCoord.X, 1 - texCoord.Y));
        }

        if (vtxl->uv4List->Count > 0)
        {
            FbxGeometryElementUV* lElementUV = (FbxGeometryElementUV*)GetFbxLayer(lMesh, 3)->CreateLayerElementOfType(FbxLayerElement::eUV);
            lElementUV->SetName(Utf8String(String::Format("UVChannel_4")).ToCStr());
            lElementUV->SetMappingMode(FbxLayerElement::eByControlPoint);
            lElementUV->SetReferenceMode(FbxLayerElement::eDirect);

            for each (Vector2 texCoord in vtxl->uv4List)
                lElementUV->GetDirectArray().Add(FbxVector2(texCoord.X, 1 - texCoord.Y));
        }

        if (vtxl->vert0x22->Count > 0)
        {
            FbxGeometryElementUV* lElementUV = (FbxGeometryElementUV*)GetFbxLayer(lMesh, 4)->CreateLayerElementOfType(FbxLayerElement::eUV);
            lElementUV->SetName(Utf8String(String::Format("UVChannel_5")).ToCStr());
            lElementUV->SetMappingMode(FbxLayerElement::eByControlPoint);
            lElementUV->SetReferenceMode(FbxLayerElement::eDirect);

            for each (array<short>^ texCoord in vtxl->vert0x22)
                lElementUV->GetDirectArray().Add(FbxVector2((float)texCoord[0] / 32767,texCoord[1] / 32767));
        }

        if (vtxl->vert0x23->Count > 0)
        {
            FbxGeometryElementUV* lElementUV = (FbxGeometryElementUV*)GetFbxLayer(lMesh, 5)->CreateLayerElementOfType(FbxLayerElement::eUV);
            lElementUV->SetName(Utf8String(String::Format("UVChannel_6")).ToCStr());
            lElementUV->SetMappingMode(FbxLayerElement::eByControlPoint);
            lElementUV->SetReferenceMode(FbxLayerElement::eDirect);

            for each (array<short> ^ texCoord in vtxl->vert0x23)
                lElementUV->GetDirectArray().Add(FbxVector2((float)texCoord[0] / 32767, texCoord[1] / 32767));
        }

        if (vtxl->vert0x24->Count > 0)
        {
            FbxGeometryElementUV* lElementUV = (FbxGeometryElementUV*)GetFbxLayer(lMesh, 6)->CreateLayerElementOfType(FbxLayerElement::eUV);
            lElementUV->SetName(Utf8String(String::Format("UVChannel_7")).ToCStr());
            lElementUV->SetMappingMode(FbxLayerElement::eByControlPoint);
            lElementUV->SetReferenceMode(FbxLayerElement::eDirect);

            for each (array<short> ^ texCoord in vtxl->vert0x24)
                lElementUV->GetDirectArray().Add(FbxVector2((float)texCoord[0] / 32767, texCoord[1] / 32767));
        }

        if (vtxl->vert0x25->Count > 0)
        {
            FbxGeometryElementUV* lElementUV = (FbxGeometryElementUV*)GetFbxLayer(lMesh, 7)->CreateLayerElementOfType(FbxLayerElement::eUV);
            lElementUV->SetName(Utf8String(String::Format("UVChannel_8")).ToCStr());
            lElementUV->SetMappingMode(FbxLayerElement::eByControlPoint);
            lElementUV->SetReferenceMode(FbxLayerElement::eDirect);

            for each (array<short> ^ texCoord in vtxl->vert0x25)
                lElementUV->GetDirectArray().Add(FbxVector2((float)texCoord[0] / 32767, texCoord[1] / 32767));
        }

        FbxGeometryElementMaterial* lElementMaterial = ( FbxGeometryElementMaterial* ) GetFbxLayer( lMesh, 0 )->CreateLayerElementOfType( FbxLayerElement::eMaterial );
        lElementMaterial->SetMappingMode( FbxLayerElement::eByPolygon );
        lElementMaterial->SetReferenceMode( FbxLayerElement::eIndexToDirect );

        HashSet<unsigned int>^ vertexIndices = gcnew HashSet<unsigned int>( vtxl->vertPositions->Count );

        FbxSkin* lSkin = nullptr;
        Dictionary<int, IntPtr>^ clusterMap = nullptr;

        if ( vtxl->vertWeights->Count > 0 )
        {
            lSkin = FbxSkin::Create( lScene, Utf8String(meshName + "_skin" ).ToCStr() );
            clusterMap = gcnew Dictionary<int, IntPtr>( bonePalette->Count );
        }

        List<Vector3>^ triangles = aqo->strips[msh.psetIndex]->GetTriangles(true);

        int materialIndex = lNode->AddMaterial( ( FbxSurfacePhong* ) materials[meshMatMappings[meshId]].ToPointer() );

        for each ( Vector3 triangle in triangles )
        {
            vertexIndices->Add( triangle.X );
            vertexIndices->Add( triangle.Y );
            vertexIndices->Add( triangle.Z );

            lMesh->BeginPolygon( materialIndex, -1, -1, false );
            lMesh->AddPolygon( triangle.X );
            lMesh->AddPolygon( triangle.Y );
            lMesh->AddPolygon( triangle.Z );
            lMesh->EndPolygon();

            if ( lElementVertexColor )
            {
                lElementVertexColor->GetIndexArray().Add( triangle.X ); 
                lElementVertexColor->GetIndexArray().Add( triangle.Y ); 
                lElementVertexColor->GetIndexArray().Add( triangle.Z ); 
            }
            if (lElementVertexColor2)
            {
                lElementVertexColor->GetIndexArray().Add(triangle.X);
                lElementVertexColor->GetIndexArray().Add(triangle.Y);
                lElementVertexColor->GetIndexArray().Add(triangle.Z);
            }
        }

        if ( lSkin != nullptr )
        {
            for ( int j = 0; j < bonePalette->Count; j++ )
            {
                ushort boneIndex = bonePalette[ j ];
                AquaNode::NODE node = aqn->nodeList[ boneIndex ];
                FbxNode* lBoneNode = ( FbxNode* ) convertedBones[ boneIndex ].ToPointer();

                IntPtr clusterPtr;
                FbxCluster* lCluster;

                if ( !clusterMap->TryGetValue( boneIndex, clusterPtr ) )
                {
                    lCluster = FbxCluster::Create( lScene, Utf8String( String::Format( "{0}_{1}_{2}_cluster", meshName, 0, String::Format("(" + boneIndex + ")" + node.boneName.GetString()) ) ).ToCStr() );
                    lCluster->SetLink( lBoneNode );
                    lCluster->SetLinkMode( FbxCluster::eTotalOne );

                    Matrix4x4 worldTransformation;
                    Matrix4x4::Invert( node.GetInverseBindPoseMatrix(), worldTransformation );

                    lCluster->SetTransformLinkMatrix( CreateFbxAMatrixFromNumerics( worldTransformation ) );

                    clusterMap->Add( boneIndex, IntPtr( lCluster ) );
                }
                else
                {
                    lCluster = ( FbxCluster* ) clusterPtr.ToPointer();
                }

                for each ( unsigned int index in vertexIndices )
                {
                    array<unsigned char>^ weightIndices = vtxl->trueVertWeightIndices[index];
                    Vector4 weights = vtxl->trueVertWeights[index];

                    for (int wt = 0; wt < weightIndices->Length; wt++)
                    {
                        switch (wt)
                        {
                            case 0:
                                if (weightIndices[0] == j)
                                    lCluster->AddControlPointIndex(index, weights.X);
                                break;
                            case 1:
                                if (weightIndices[1] == j)
                                    lCluster->AddControlPointIndex(index, weights.Y);
                                break;
                            case 2:
                                if (weightIndices[2] == j)
                                    lCluster->AddControlPointIndex(index, weights.Z);
                                break;
                            case 3:
                                if (weightIndices[3] == j)
                                    lCluster->AddControlPointIndex(index, weights.W);
                                break;
                        }
                    }
                }

                lSkin->AddCluster( lCluster );
            }
        }

        vertexIndices->Clear();
        

        if ( lSkin != nullptr )
        {
            if ( lSkin->GetClusterCount() > 0 )
                lMesh->AddDeformer( lSkin );

            else
                lSkin->Destroy();
        }

        lNode->SetNodeAttribute( lMesh );
        lNode->SetShadingMode( FbxNode::eTextureShading );

        return lNode;
    }

    FbxSurfacePhong* CreateFbxSurfacePhongFromMaterial( AquaObject::GenericMaterial^ aqMat, String^ texturesDirectoryPath, FbxScene* lScene, bool includeMetadata)
    {
        const char* name;
        if (includeMetadata)
        {
            name = Utf8String("(" + aqMat->shaderNames[0] + "," + aqMat->shaderNames[1] + ")" + "{" + aqMat->blendType + "}" + aqMat->matName + "@" + aqMat->twoSided.ToString()).ToCStr();
        }
        else {
            name = Utf8String(aqMat->matName).ToCStr();
        }
        FbxSurfacePhong* lSurfacePhong = FbxSurfacePhong::Create( lScene, name);

        lSurfacePhong->ShadingModel.Set( "Phong" );

        lSurfacePhong->Diffuse.Set( FbxDouble3( aqMat->diffuseRGBA.X, aqMat->diffuseRGBA.Y, aqMat->diffuseRGBA.Z) );
        lSurfacePhong->TransparencyFactor.Set((double)aqMat->diffuseRGBA.W);

        lSurfacePhong->Ambient.Set( FbxDouble3(aqMat->unkRGBA0.X, aqMat->unkRGBA0.Y, aqMat->unkRGBA0.Z) );

        if ( aqMat->texNames->Count == 0 )
            return lSurfacePhong;

        //For now, just do diffuse since sets are so arbitrary
        for(int i = 0; i < 1; i++)
        {
            FbxFileTexture* lFileTexture = FbxFileTexture::Create( lScene,
                Utf8String( String::Format( "{0}", aqMat->texNames[i] ) ).ToCStr() );

            lFileTexture->SetFileName( Utf8String( Path::Combine( texturesDirectoryPath, aqMat->texNames[i]) ).ToCStr() );

            if (aqMat->texUVSets[i] == -1)
            {
                lFileTexture->SetMappingType(FbxTexture::eEnvironment);
                lFileTexture->UVSet.Set("UVChannel_1");
                lSurfacePhong->Reflection.ConnectSrcObject(lFileTexture);
            }
            else
            {
                lFileTexture->SetMappingType(FbxTexture::eUV);
                lFileTexture->UVSet.Set(Utf8String(String::Format("UVChannel_{0}", aqMat->texUVSets[i] + 1)).ToCStr());
                lSurfacePhong->Diffuse.ConnectSrcObject(lFileTexture);
            }
        }

        return lSurfacePhong;
    }

    FbxNode* CreateFbxNodeFromAquaObject( AquaObject^ aqo, String^ aqoName, String^ texturesDirectoryPath, FbxScene* lScene, FbxPose* lBindPose, List<IntPtr>^ convertedBones, AquaNode^ aqn, bool includeMetadata)
    {
        FbxNode* lNode = FbxNode::Create( lScene, Utf8String(aqoName + "_model").ToCStr() );
        List<int>^ meshMatMappings;
        List<AquaObject::GenericMaterial^>^ aqMats = aqo->GetUniqueMaterials(meshMatMappings);
        List<IntPtr>^ materials = gcnew List<IntPtr>( aqMats->Count );

        for each (AquaObject::GenericMaterial^ aqMat in aqMats)
        {
            materials->Add(IntPtr(CreateFbxSurfacePhongFromMaterial(aqMat, texturesDirectoryPath, lScene, includeMetadata)));
        }
        
        for (int i = 0; i < aqo->meshList->Count; i++ )
        {
            FbxNode* lMeshNode = CreateFbxNodeFromMesh( aqo, i, lScene, materials, meshMatMappings, convertedBones, aqn, includeMetadata);
            lNode->AddChild( lMeshNode );
            lBindPose->Add( lMeshNode, FbxAMatrix() );
        }

        lBindPose->Add( lNode, FbxAMatrix() );

        return lNode;
    }
    
    FbxNode* CreateFbxNodeFromAqnNode(AquaNode::NODE node, const Matrix4x4& inverseParentTransformation, FbxScene* lScene, FbxPose* lBindPose, int boneIndex, bool includeMetadata)
    {
        const char* name;
        if (includeMetadata)
        {
            name = Utf8String(String::Format("(" + boneIndex + ")" + node.boneName.GetString() + "#" + node.boneShort1.ToString("X") + "#" + node.boneShort2.ToString("X"))).ToCStr();
        }
        else {
            name = Utf8String(node.boneName.GetString()).ToCStr();
        }
        FbxNode* lNode = FbxNode::Create(lScene, name);

        Matrix4x4 worldTransformation;
        Matrix4x4::Invert(node.GetInverseBindPoseMatrix(), worldTransformation);

        Matrix4x4 localTransformation = Matrix4x4::Multiply(worldTransformation, inverseParentTransformation);

        Vector3 scale, translation;
        Quaternion rotation;

        Matrix4x4::Decompose(localTransformation, scale, rotation, translation);

        FbxVector4 eulerAngles;
        eulerAngles.SetXYZ(FbxQuaternion(rotation.X, rotation.Y, rotation.Z, rotation.W));

        lNode->LclTranslation.Set(FbxVector4(translation.X, translation.Y, translation.Z));
        lNode->LclRotation.Set(eulerAngles);
        lNode->LclScaling.Set(FbxVector4(scale.X, scale.Y, scale.Z));

        FbxSkeleton* lSkeleton = FbxSkeleton::Create(lScene, name);
        lSkeleton->Color.Set(FbxDouble3(0, 0.769, 0.769));
        lSkeleton->SetSkeletonType(FbxSkeleton::eLimbNode);
        lSkeleton->LimbLength = 0.1;

        lNode->SetNodeAttribute(lSkeleton);
        lBindPose->Add(lNode, CreateFbxAMatrixFromNumerics(worldTransformation));

        return lNode;
    }

    FbxNode* CreateFbxNodeFromAqnNodo(AquaNode::NODO nodo, const Matrix4x4& inverseParentTransformation, FbxScene* lScene, FbxPose* lBindPose, bool includeMetadata)
    {
        const char* name;
        if (includeMetadata)
        {
            name = Utf8String(nodo.boneName.GetString() + "#" + nodo.boneShort1.ToString("X") + "#" + nodo.boneShort2.ToString("X")).ToCStr();
        }
        else {
            name = Utf8String(nodo.boneName.GetString()).ToCStr();
        }
        FbxNode* lNode = FbxNode::Create(lScene, name);

        Matrix4x4 parentTransformation;
        Matrix4x4::Invert(inverseParentTransformation, parentTransformation);
        Matrix4x4 localTransformation = nodo.GetLocalTransformMatrix();
       
        Matrix4x4 worldTransformation;
        Matrix4x4::Invert(Matrix4x4::Multiply(localTransformation, parentTransformation), worldTransformation);

        Vector3 scale, translation;
        Quaternion rotation;

        Matrix4x4::Decompose(localTransformation, scale, rotation, translation);

        FbxVector4 eulerAngles;
        eulerAngles.SetXYZ(FbxQuaternion(rotation.X, rotation.Y, rotation.Z, rotation.W));

        lNode->LclTranslation.Set(FbxVector4(translation.X, translation.Y, translation.Z));
        lNode->LclRotation.Set(eulerAngles);
        lNode->LclScaling.Set(FbxVector4(scale.X, scale.Y, scale.Z));

        FbxSkeleton* lSkeleton = FbxSkeleton::Create(lScene, name);
        lSkeleton->Color.Set(FbxDouble3(0, 0.769, 0));

        lSkeleton->SetSkeletonType(FbxSkeleton::eLimbNode);
        lSkeleton->LimbLength = 0.1;

        lNode->SetNodeAttribute(lSkeleton);
        lBindPose->Add(lNode, CreateFbxAMatrixFromNumerics(worldTransformation));

        return lNode;
    }


    FbxExporterCore::FbxExporterCore()
    {
        lManager = FbxManager::Create();
    }

    FbxExporterCore::~FbxExporterCore()
    {
        lManager->Destroy();
    }
    
    //Expects pre VSET split model
    void FbxExporterCore::ExportToFile( AquaObject^ aqo, AquaNode^ aqn, String^ destinationFilePath, bool includeMetadata)
    {
        String^ texturesDirectoryPath = Path::GetDirectoryName( destinationFilePath );
        String^ aqoName = Path::GetFileNameWithoutExtension ( destinationFilePath );

        int lFileFormat = lManager->GetIOPluginRegistry()->GetNativeWriterFormat();

        ::FbxExporter* lExporter = ::FbxExporter::Create( lManager, "" );
        lExporter->SetFileExportVersion( FBX_2014_00_COMPATIBLE );

        bool lExportStatus = lExporter->Initialize( Utf8String( destinationFilePath ).ToCStr(), lFileFormat, lManager->GetIOSettings() );

        if ( !lExportStatus )
            throw gcnew Exception( String::Format( "Failed to export FBX file ({0})", destinationFilePath ) );

        FbxScene* lScene = FbxScene::Create( lManager, "" );

        FbxPose* lBindPose = FbxPose::Create( lScene, "BindPoses" );
        lBindPose->SetIsBindPose( true );

        FbxNode* lRootNode = lScene->GetRootNode();

        List<IntPtr>^ convertedBones = gcnew List<IntPtr>();
        AquaNode::NODE node0 = aqn->nodeList[0];
        const char* name;
        if (includeMetadata)
        {
            name = Utf8String(String::Format("(0)" + node0.boneName.GetString() + "#" + node0.boneShort1.ToString("X") + "#" + node0.boneShort2.ToString("X"))).ToCStr();
        }
        else {
            name = Utf8String(node0.boneName.GetString()).ToCStr();
        }

        FbxNode* lSkeletonNode = FbxNode::Create( lScene, name);
        convertedBones->Add(IntPtr(lSkeletonNode));

        FbxSkeleton* lSkeleton = FbxSkeleton::Create( lScene, name);
        lSkeleton->SetSkeletonType( FbxSkeleton::eRoot );

        lSkeletonNode->SetNodeAttribute( lSkeleton );

        //Go through standard nodes
        for(int i = 0; i < aqn->nodeList->Count; i++)
        {
            if (i == 0)
            {
                continue;
            }
            AquaNode::NODE node = aqn->nodeList[i];

            Matrix4x4 parentInvTfm;
            FbxNode* parentFbxNode = nullptr;
            if (node.parentId != -1)
            {
                parentFbxNode = (FbxNode*)convertedBones[node.parentId].ToPointer();
                parentInvTfm = aqn->nodeList[node.parentId].GetInverseBindPoseMatrix();
            }
            else {
                parentInvTfm = Matrix4x4::Identity;
            }
            FbxNode* fbxNode = CreateFbxNodeFromAqnNode(node, parentInvTfm, lScene, lBindPose, i, includeMetadata);
            if (parentFbxNode != nullptr)
            {
                parentFbxNode->AddChild(fbxNode);
            }

            convertedBones->Add(IntPtr(fbxNode));
        }

        //Go through 'effect nodes'. These can't have children or be tied to skinning
        for (int i = 0; i < aqn->nodoList->Count; i++) 
        {
            AquaNode::NODO nodo = aqn->nodoList[i];
            //All effect nodes need a parent. 
            FbxNode* parentNode = (FbxNode*)convertedBones[nodo.parentId].ToPointer();
            Matrix4x4 parentInvTfm = aqn->nodeList[nodo.parentId].GetInverseBindPoseMatrix();
            
            FbxNode* fbxNode = CreateFbxNodeFromAqnNodo(nodo, parentInvTfm, lScene, lBindPose, includeMetadata);
            parentNode->AddChild(fbxNode);
        }

        //PSO2 models should ALWAYS have at least one bone
        lSkeletonNode->AddChild((FbxNode*)convertedBones[0].ToPointer());

        lBindPose->Add(lSkeletonNode, FbxMatrix());
        lRootNode->AddChild(lSkeletonNode);
        
        FbxNode* lObjectNode = CreateFbxNodeFromAquaObject( aqo, aqoName, texturesDirectoryPath, lScene, lBindPose, convertedBones, aqn, includeMetadata);
        lRootNode->AddChild( lObjectNode );
        

        lScene->AddPose( lBindPose );

        lScene->GetGlobalSettings().SetAxisSystem( FbxAxisSystem::OpenGL );
        lScene->GetGlobalSettings().SetSystemUnit( FbxSystemUnit::m );

        lExporter->Export( lScene );
        lExporter->Destroy();

        lScene->Destroy( true );
    }
}
