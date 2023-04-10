#pragma warning( disable : 4244 4965)
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
        
        if (aqo->meshNames->Count - 1 >= meshId)
        {
            meshName = aqo->meshNames[meshId];
        } else if (includeMetadata)
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
        FbxGeometryElementUV* lElementVertexColor2_1 = nullptr;
        FbxGeometryElementUV* lElementVertexColor2_2 = nullptr;

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

        //Having a second vertex color channel breaks things because Autodesk doesn't even support their own schema :)
        /*
        if (vtxl->vertColor2s->Count > 0)
        {
            lElementVertexColor2 = (FbxGeometryElementVertexColor*)GetFbxLayer(lMesh, 1)->CreateLayerElementOfType(FbxLayerElement::eVertexColor);

            // Vertex color elements need to use these modes for 3DS Max to read them properly. Anything else is not going to work.
            //lElementVertexColor2->SetName("VCChannel_1");
            lElementVertexColor2->SetMappingMode(FbxLayerElement::eByPolygonVertex);
            lElementVertexColor2->SetReferenceMode(FbxLayerElement::eIndexToDirect);

            for each (array<unsigned char> ^ color in vtxl->vertColor2s)
                lElementVertexColor->GetDirectArray().Add(FbxColor(((float)color[2]) / 255, ((float)color[1]) / 255, ((float)color[0]) / 255, ((float)color[3]) / 255));
        }*/

        SetUVChannel(lMesh, vtxl->uv1List, vtxl->vertPositions->Count, 1);
        SetUVChannel(lMesh, vtxl->uv2List, vtxl->vertPositions->Count, 2);
        SetUVChannel(lMesh, vtxl->uv3List, vtxl->vertPositions->Count, 3);
        SetUVChannel(lMesh, vtxl->uv4List, vtxl->vertPositions->Count, 4);
        SetUVChannelShorts(lMesh, vtxl->vert0x22, vtxl->vertPositions->Count, 5);
        SetUVChannelShorts(lMesh, vtxl->vert0x23, vtxl->vertPositions->Count, 6);
        SetUVChannelShorts(lMesh, vtxl->vert0x24, vtxl->vertPositions->Count, 7);
        SetUVChannelShorts(lMesh, vtxl->vert0x25, vtxl->vertPositions->Count, 8);

        if (vtxl->vertColor2s->Count > 0)
        {
            lElementVertexColor2_1 = (FbxGeometryElementUV*)GetFbxLayer(lMesh, 9)->CreateLayerElementOfType(FbxLayerElement::eUV);

            // Vertex color elements need to use these modes for 3DS Max to read them properly. Anything else is not going to work.
            lElementVertexColor2_1->SetName(Utf8String(String::Format("UVChannel_9")).ToCStr());
            lElementVertexColor2_1->SetMappingMode(FbxLayerElement::eByControlPoint);
            lElementVertexColor2_1->SetReferenceMode(FbxLayerElement::eDirect);

            for each (array<unsigned char> ^ color in vtxl->vertColor2s)
                lElementVertexColor2_1->GetDirectArray().Add(FbxVector2(((float)color[2]) / 255, ((float)color[1]) / 255));

            lElementVertexColor2_2 = (FbxGeometryElementUV*)GetFbxLayer(lMesh, 10)->CreateLayerElementOfType(FbxLayerElement::eUV);

            // Vertex color elements need to use these modes for 3DS Max to read them properly. Anything else is not going to work.
            lElementVertexColor2_2->SetName(Utf8String(String::Format("UVChannel_10")).ToCStr());
            lElementVertexColor2_2->SetMappingMode(FbxLayerElement::eByControlPoint);
            lElementVertexColor2_2->SetReferenceMode(FbxLayerElement::eDirect);

            for each (array<unsigned char> ^ color in vtxl->vertColor2s)
                lElementVertexColor2_2->GetDirectArray().Add(FbxVector2(((float)color[0]) / 255, ((float)color[3]) / 255));
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
        }

        if ( lSkin != nullptr )
        {
            for ( int j = 0; j < bonePalette->Count; j++ )
            {
                ushort boneIndex = bonePalette[ j ];
                AquaNode::NODE node;
                if (aqn->nodeList->Count > boneIndex)
                {
                    node = aqn->nodeList[boneIndex];
                }
                else {
                    node = aqn->nodeList[0];
                    boneIndex = 0;
                }
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
                    array<int>^ weightIndices = vtxl->trueVertWeightIndices[index];
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

    void SetUVChannel(fbxsdk::FbxMesh* lMesh, System::Collections::Generic::List<System::Numerics::Vector2>^ uvList, int count, int uvNum)
    {
        FbxGeometryElementUV* lElementUV = (FbxGeometryElementUV*)GetFbxLayer(lMesh, uvNum - 1)->CreateLayerElementOfType(FbxLayerElement::eUV);
        lElementUV->SetName(Utf8String(String::Format("UVChannel_" + uvNum.ToString())).ToCStr());
        lElementUV->SetMappingMode(FbxLayerElement::eByControlPoint);
        lElementUV->SetReferenceMode(FbxLayerElement::eDirect);
        if (uvList->Count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                System::Numerics::Vector2 texCoord = uvList[i];
                lElementUV->GetDirectArray().Add(FbxVector2(texCoord.X, 1 - texCoord.Y));
            }
        }
        else {
            for (int i = 0; i < count; i++)
            {
                lElementUV->GetDirectArray().Add(FbxVector2(0.0, 0.0));
            }
        }
    }

    void SetUVChannelShorts(fbxsdk::FbxMesh* lMesh, System::Collections::Generic::List<array<short>^ >^ uvList, int count, int uvNum)
    {
        FbxGeometryElementUV* lElementUV = (FbxGeometryElementUV*)GetFbxLayer(lMesh, uvNum - 1)->CreateLayerElementOfType(FbxLayerElement::eUV);
        lElementUV->SetName(Utf8String(String::Format("UVChannel_" + uvNum.ToString())).ToCStr());
        lElementUV->SetMappingMode(FbxLayerElement::eByControlPoint);
        lElementUV->SetReferenceMode(FbxLayerElement::eDirect);
        if (uvList->Count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                array<short>^ texCoord = uvList[i];
                lElementUV->GetDirectArray().Add(FbxVector2((float)texCoord[0] / 32767, (float)texCoord[1] / 32767));
            }
        }
        else {
            for (int i = 0; i < count; i++)
            {
                lElementUV->GetDirectArray().Add(FbxVector2(0.0f, 0.0f));
            }
        }
    }

    FbxSurfacePhong* CreateFbxSurfacePhongFromMaterial( AquaObject::GenericMaterial^ aqMat, String^ texturesDirectoryPath, FbxScene* lScene, bool includeMetadata)
    {
        const char* name;
        System::String^ specialType = aqMat->specialType->Length > 0 ? "[" + aqMat->specialType + "]": "";
        if (includeMetadata)
        {
            name = Utf8String("(" + aqMat->shaderNames[0] + "," + aqMat->shaderNames[1] + ")" + "{" + aqMat->blendType + "}" + specialType + aqMat->matName + "@" + aqMat->twoSided.ToString() + "@" + aqMat->alphaCutoff.ToString()).ToCStr();
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

    void CreateFbxNodeFromAquaObject( AquaObject^ aqo, String^ aqoName, String^ texturesDirectoryPath, FbxScene* lScene, FbxPose* lBindPose, List<IntPtr>^ convertedBones, AquaNode^ aqn, List<Matrix4x4>^ instanceTransforms, bool includeMetadata)
    {
        FbxNode* lNode = FbxNode::Create( lScene, Utf8String(aqoName + "_model").ToCStr() );
        FbxNode* lNodeInstances = instanceTransforms->Count > 0 ? FbxNode::Create(lScene, Utf8String(aqoName + "_instances").ToCStr()) : NULL;
        List<int>^ meshMatMappings;
        List<int>^ nodesToRemove = gcnew List<int>();
        List<AquaObject::GenericMaterial^>^ aqMats = aqo->GetUniqueMaterials(meshMatMappings);
        List<IntPtr>^ materials = gcnew List<IntPtr>( aqMats->Count );

        FbxNode* lRootNode = lScene->GetRootNode();

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

        //If we have instance transforms, we want to acutalize those
        for (int i = 0; i < instanceTransforms->Count; i++)
        {
            FbxNode* instanceNode = FbxNode::Create(lScene, Utf8String(aqoName + "_instance_" + i).ToCStr());
            Matrix4x4 tfm = instanceTransforms[i];        
            Vector3 scale, translation;
            Quaternion rotation;

            Matrix4x4::Decompose(tfm, scale, rotation, translation);
            FbxVector4 eulerAngles;
            eulerAngles.SetXYZ(FbxQuaternion(rotation.X, rotation.Y, rotation.Z, rotation.W));

            instanceNode->LclTranslation.Set(FbxVector4(translation.X, translation.Y, translation.Z));
            instanceNode->LclRotation.Set(eulerAngles);
            instanceNode->LclScaling.Set(FbxVector4(scale.X, scale.Y, scale.Z));

            for (int j = 0; j < lNode->GetChildCount(); j++)
            {
                FbxNode* instanceNodeMesh = FbxNode::Create(lScene, Utf8String(aqoName + "_i_" + i + "_mesh_" + j).ToCStr());
                instanceNodeMesh->SetNodeAttribute(lNode->GetChild(j)->GetMesh());
                instanceNode->AddChild(instanceNodeMesh);
                lBindPose->Add(instanceNodeMesh, FbxAMatrix());
            }
            lNodeInstances->AddChild(instanceNode);
            lBindPose->Add(instanceNode, FbxAMatrix());
        }

        if (instanceTransforms->Count > 0)
        {
            //Remove from the main node if we're using instances
            for (int i = 0; i < lNode->GetChildCount(); i++)
            {
                FbxNode* meshNode = lNode->GetChild(i);
                lNode->RemoveChild(meshNode);
            }
            lNode->Destroy(true);
            lRootNode->AddChild(lNodeInstances);
            lBindPose->Add(lNodeInstances, FbxAMatrix());
        }
        else {
            lRootNode->AddChild(lNode);
            lBindPose->Add(lNode, FbxAMatrix());
        }
        aqMats->Clear();

        return;
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
        lNode->LclScaling.Set(FbxVector4(1, 1, 1));

        FbxSkeleton* lSkeleton = FbxSkeleton::Create(lScene, name);
        lSkeleton->Color.Set(FbxDouble3(0, 0.769, 0));

        lSkeleton->SetSkeletonType(FbxSkeleton::eLimbNode);
        lSkeleton->LimbLength = 0.1;

        lNode->SetNodeAttribute(lSkeleton);
        lBindPose->Add(lNode, CreateFbxAMatrixFromNumerics(worldTransformation));

        return lNode;
    }

    void CreateAnimationTakeFromAquaMotion(FbxScene* lScene, List<IntPtr>^ convertedBones, AquaNode^ aqn, AquaMotion^ aqm, String^ name)
    {
        AquaMotion::MOHeader header = aqm->moHeader;
        const char* cStrName = Utf8String(name).ToCStr();
        FbxAnimStack* animStack = FbxAnimStack::Create(lScene, cStrName);
        FbxAnimLayer* animBaseLayer = FbxAnimLayer::Create(lScene, cStrName);
        animStack->AddMember(animBaseLayer);

        FbxTime lStart;
        lStart.SetFrame(0);
        FbxTime lEnd;
        lEnd.SetFrame(header.endFrame);
        animStack->SetLocalTimeSpan(FbxTimeSpan(lStart, lEnd));

        int boneCount = System::Math::Min(aqm->motionKeys->Count, convertedBones->Count);
        for (int i = 0; i < boneCount; i++)
        {
            ushort bs1 = aqn->nodeList[i].boneShort1;
            ushort bs2 = aqn->nodeList[i].boneShort2;
            FbxNode* bone = ((FbxNode*)convertedBones[i].ToPointer());
            FbxAnimCurve* curveTX = bone->LclTranslation.GetCurve(animBaseLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
            FbxAnimCurve* curveTY = bone->LclTranslation.GetCurve(animBaseLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
            FbxAnimCurve* curveTZ = bone->LclTranslation.GetCurve(animBaseLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
            FbxAnimCurve* curveRX = bone->LclRotation.GetCurve(animBaseLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
            FbxAnimCurve* curveRY = bone->LclRotation.GetCurve(animBaseLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
            FbxAnimCurve* curveRZ = bone->LclRotation.GetCurve(animBaseLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
            FbxAnimCurve* curveSX = bone->LclScaling.GetCurve(animBaseLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
            FbxAnimCurve* curveSY = bone->LclScaling.GetCurve(animBaseLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
            FbxAnimCurve* curveSZ = bone->LclScaling.GetCurve(animBaseLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);

            for (int keySetId = 0; keySetId < aqm->motionKeys[i]->keyData->Count; keySetId++)
            {
                AquaMotion::MKEY^ keySet = aqm->motionKeys[i]->keyData[keySetId];

                //Mainly to skip NodeTreeFlags for player animations. We don't really understand these well, but they don't appear to do much either so we'll skip for now
                //Other types haven't been observed either, but we probably want to skip those too
                if (keySet->keyType < 1 || keySet->keyType > 3)
                {
                    continue;
                }

                //Open keys for modification
                switch (keySet->keyType)
                {
                    case 1:
                        curveTX->KeyModifyBegin();
                        curveTY->KeyModifyBegin();
                        curveTZ->KeyModifyBegin();
                        break;
                    case 2:
                        curveRX->KeyModifyBegin();
                        curveRY->KeyModifyBegin();
                        curveRZ->KeyModifyBegin();
                        break;
                    case 3:
                        curveSX->KeyModifyBegin();
                        curveSY->KeyModifyBegin();
                        curveSZ->KeyModifyBegin();
                        break;
                }

                //Set keys
                for (int t = 0; t < keySet->vector4Keys->Count; t++)
                {
                    // Set animation time
                    FbxTime lTime;
                    if (keySet->frameTimings->Count > 0)
                    {
                        lTime.SetFrame(keySet->frameTimings[t] / keySet->GetTimeMultiplier());
                    }
                    else {
                        lTime.SetFrame(0);
                    }
                    System::Numerics::Vector4 netVec4 = keySet->vector4Keys[t]; //We can expect all transform types to use this type of keydata

                    FbxVector4 lcl_rotation;
                    FbxQuaternion lcl_quat;
                    int lKeyIndex;

                    switch (keySet->keyType)
                    {
                        case 1:
                            lKeyIndex = curveTX->KeyAdd(lTime);
                            curveTX->KeySetValue(lKeyIndex, netVec4.X);
                            curveTX->KeySetInterpolation(lKeyIndex, FbxAnimCurveDef::eInterpolationLinear);

                            lKeyIndex = curveTY->KeyAdd(lTime);
                            curveTY->KeySetValue(lKeyIndex, netVec4.Y);
                            curveTY->KeySetInterpolation(lKeyIndex, FbxAnimCurveDef::eInterpolationLinear);

                            lKeyIndex = curveTZ->KeyAdd(lTime);
                            curveTZ->KeySetValue(lKeyIndex, netVec4.Z);
                            curveTZ->KeySetInterpolation(lKeyIndex, FbxAnimCurveDef::eInterpolationLinear);
                            break;
                        case 2:
                            lcl_quat = FbxQuaternion(netVec4.X, netVec4.Y, netVec4.Z, netVec4.W);
                            lcl_rotation.SetXYZ(lcl_quat);

                            lKeyIndex = curveRX->KeyAdd(lTime);
                            curveRX->KeySetValue(lKeyIndex, lcl_rotation[0]);
                            curveRX->KeySetInterpolation(lKeyIndex, FbxAnimCurveDef::eInterpolationLinear);

                            lKeyIndex = curveRY->KeyAdd(lTime);
                            curveRY->KeySetValue(lKeyIndex, lcl_rotation[1]);
                            curveRY->KeySetInterpolation(lKeyIndex, FbxAnimCurveDef::eInterpolationLinear);

                            lKeyIndex = curveRZ->KeyAdd(lTime);
                            curveRZ->KeySetValue(lKeyIndex, lcl_rotation[2]);
                            curveRZ->KeySetInterpolation(lKeyIndex, FbxAnimCurveDef::eInterpolationLinear);
                            break;
                        case 3:
                            lKeyIndex = curveSX->KeyAdd(lTime);
                            curveSX->KeySetValue(lKeyIndex, netVec4.X);
                            curveSX->KeySetInterpolation(lKeyIndex, FbxAnimCurveDef::eInterpolationLinear);

                            lKeyIndex = curveSY->KeyAdd(lTime);
                            curveSY->KeySetValue(lKeyIndex, netVec4.Y);
                            curveSY->KeySetInterpolation(lKeyIndex, FbxAnimCurveDef::eInterpolationLinear);

                            lKeyIndex = curveSZ->KeyAdd(lTime);
                            curveSZ->KeySetValue(lKeyIndex, netVec4.Z);
                            curveSZ->KeySetInterpolation(lKeyIndex, FbxAnimCurveDef::eInterpolationLinear);
                            break;
                    }
                }

                //End Modification
                switch (keySet->keyType)
                {
                    case 1:
                        curveTX->KeyModifyEnd();
                        curveTY->KeyModifyEnd();
                        curveTZ->KeyModifyEnd();
                        break;
                    case 2:
                        curveRX->KeyModifyEnd();
                        curveRY->KeyModifyEnd();
                        curveRZ->KeyModifyEnd();
                        break;
                    case 3:
                        curveSX->KeyModifyEnd();
                        curveSY->KeyModifyEnd();
                        curveSZ->KeyModifyEnd();
                        break;
                }
            }
        }

        // Apply unroll filter to fix flickering when interpolating
        FbxAnimCurveFilterUnroll lFilter;

        for (size_t i = 0; i < convertedBones->Count; i++)
        {
            FbxAnimCurve* lCurve[3];
            FbxNode* bone = ((FbxNode*)convertedBones[i].ToPointer());

            lCurve[0] = bone->LclRotation.GetCurve(animBaseLayer, FBXSDK_CURVENODE_COMPONENT_X);
            lCurve[1] = bone->LclRotation.GetCurve(animBaseLayer, FBXSDK_CURVENODE_COMPONENT_Y);
            lCurve[2] = bone->LclRotation.GetCurve(animBaseLayer, FBXSDK_CURVENODE_COMPONENT_Z);

            if (lCurve[0] && lCurve[1] && lCurve[2] && lFilter.NeedApply(lCurve, 3))
                lFilter.Apply(lCurve, 3);
        }
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
    void FbxExporterCore::ExportToFile( AquaObject^ aqo, AquaNode^ aqn, List<AquaMotion^>^ aqmList, String^ destinationFilePath, List<String^>^ aqmNameList, List<Matrix4x4>^ instanceTransforms, bool includeMetadata)
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

        FbxNode* lSkeletonNode = CreateFbxNodeFromAqnNode(node0, Matrix4x4::Identity, lScene, lBindPose, 0, includeMetadata);
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
            FbxNode* parentNode;
            Matrix4x4 parentInvTfm;
            //Almost all effect nodes need a parent. 
            if (nodo.parentId != -1)
            {
                parentNode = (FbxNode*)convertedBones[nodo.parentId].ToPointer();
                parentInvTfm = aqn->nodeList[nodo.parentId].GetInverseBindPoseMatrix();
            }
            else {
                parentNode = lRootNode;
                parentInvTfm = Matrix4x4::Identity;
            }
            
            FbxNode* fbxNode = CreateFbxNodeFromAqnNodo(nodo, parentInvTfm, lScene, lBindPose, includeMetadata);
            parentNode->AddChild(fbxNode);
        }

        //PSO2 models should ALWAYS have at least one bone
        lSkeletonNode->AddChild((FbxNode*)convertedBones[0].ToPointer());

        lBindPose->Add(lSkeletonNode, FbxMatrix());
        lRootNode->AddChild(lSkeletonNode);
        
        CreateFbxNodeFromAquaObject( aqo, aqoName, texturesDirectoryPath, lScene, lBindPose, convertedBones, aqn, instanceTransforms, includeMetadata);
        

        lScene->AddPose( lBindPose );

        //Animations
        if (aqmList->Count > 0)
        {
            lScene->GetGlobalSettings().SetTimeMode(FbxTime::EMode::eFrames30);
            for (int i = 0; i < aqmList->Count; i++)
            {
                aqmList[i]->PrepareScalingForExport(aqn);
                CreateAnimationTakeFromAquaMotion(lScene, convertedBones, aqn, aqmList[i], aqmNameList[i]);
            }
        }

        lScene->GetGlobalSettings().SetAxisSystem( FbxAxisSystem::OpenGL );
        lScene->GetGlobalSettings().SetSystemUnit( FbxSystemUnit::m );
        lExporter->Export( lScene );
        lExporter->Destroy();

        lScene->Destroy( true );
    }
}
