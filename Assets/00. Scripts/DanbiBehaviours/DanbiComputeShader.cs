﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

using ComputeBuffersDict = System.Collections.Generic.Dictionary<string, UnityEngine.ComputeBuffer>;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiComputeShader : MonoBehaviour
    {
        [SerializeField, Readonly, Space(5)]
        int m_maxNumOfBounce = 2;

        [SerializeField, Readonly]
        int m_samplingThreshold = 10;

        [SerializeField]
        ComputeShader m_danbiShader;
        public ComputeShader danbiShader => m_danbiShader;

        [SerializeField, Readonly]
        int m_isPanoramaTex;

        [SerializeField, Readonly]
        Vector4 m_centerOfPanoramaMesh;

        Material m_addMaterial_ScreenSampling;

        int m_samplingCounter;

        [SerializeField]
        RenderTexture m_resultRT_LowRes;
        public RenderTexture resultRT_LowRes => m_resultRT_LowRes;

        [SerializeField]
        RenderTexture m_convergedRT_HiRes;
        public RenderTexture convergedResultRT_HiRes => m_convergedRT_HiRes;

        public DanbiComputeShaderBufferDictionary bufferDict = new DanbiComputeShaderBufferDictionary();

        readonly System.DateTime seedDateTime = new System.DateTime();

        public delegate void OnSampleFinished(RenderTexture sampledRenderTex);
        public static event OnSampleFinished onSampleFinished;

        #region dbg
        // ComputeBuffer dbg_centerOfPanoBuf;
        // Vector4 dbg_centerOfPanoArr = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        // ComputeBuffer dbg_rayLengthBuf;
        // Vector3 dbg_rayLengthArr = new Vector3();

        // ComputeBuffer dbg_cameraInternalDataBuf;
        // DanbiCameraInternalData_struct dbg_cameraInternalData = new DanbiCameraInternalData_struct();

        ComputeBuffer dbg_hitInfoBuf;
        Vector4 dbg_hitInfoArr = new Vector4();

        // ComputeBuffer dbg_cameraToWorldMatBuf;
        // float4x4 dbg_cameraToWorldMatArr = new float4x4();

        // ComputeBuffer dbg_cameraInverseProjectionBuf;
        // float4x4 dbg_cameraInverseProjectionArr = new float4x4();

        ComputeBuffer dbg_usedHeightBuf;
        float dbg_usedHeightArr = 0.0f;
        #endregion dbg

        void Awake()
        {
            // query the hardward it supports the compute shader.
            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogError("This machine doesn't support Compute Shader!", this);
            }

            // Initialize the Screen Sampling shader.
            m_addMaterial_ScreenSampling = new Material(Shader.Find("Hidden/AddShader"));

            DanbiPanoramaScreenChanger.onCenterPosOfMeshUpdate_Panorama +=
                (Vector3 newCenterOfPanoramaMesh) =>
                {
                    m_centerOfPanoramaMesh = new Vector4(newCenterOfPanoramaMesh.x,
                                                         newCenterOfPanoramaMesh.y,
                                                         newCenterOfPanoramaMesh.z,
                                                         0.0f);
                };

            DanbiUIImageGeneratorTexturePanel.onTextureTypeChange +=
                (EDanbiTextureType type) => m_isPanoramaTex = (int)type;

            DanbiUIVideoGeneratorVideoPanel.onVideoTypeChange +=
                (int isPanoramaTex) => m_isPanoramaTex = isPanoramaTex;

            // Populate kernels index.
            PopulateKernels();

            m_maxNumOfBounce = 2;
            m_samplingThreshold = 10;

            #region dbg
            // DanbiDbg.PrepareDbgBuffers();
            // SetData is performed automatically when the buffer is created.
            dbg_usedHeightBuf = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(dbg_usedHeightArr, 4);
            // dbg_centerOfPanoBuf = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(dbg_centerOfPanoArr, 16);
            // dbg_rayLengthBuf = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(dbg_rayLengthArr, 12);
            dbg_hitInfoBuf = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(dbg_hitInfoArr, 16);
            // dbg_cameraInternalDataBuf = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(dbg_cameraInternalData, 40);
            // dbg_cameraToWorldMatBuf = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(dbg_cameraInverseProjectionArr, 64);
            // dbg_cameraInverseProjectionBuf = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(dbg_cameraInverseProjectionArr, 64);
            #endregion dbg
        }

        void Update()
        {
            SetShaderParams();

            #region dbg
            // dbg_centerOfPanoBuf.GetData(arr);
            // foreach (var i in arr)
            // {
            //     Debug.Log($"{i.x}, {i.y}, {i.z}");
            // }
            // if (Input.GetKeyDown(KeyCode.D))
            // {

            //     // var arr = new Vector3[1];
            //     // dbg_rayLengthBuf.GetData(arr);

            if (Input.GetKeyDown(KeyCode.D))
            {
                var arr = new Vector4[1];
                dbg_hitInfoBuf.GetData(arr);
                foreach (var i in arr)
                {
                    Debug.Log($"{i.x}, {i.y}, {i.z}");
                }
            }

            //     // var arr = new DanbiCameraInternalData_struct[1];
            //     // dbg_cameraInternalDataBuf.GetData(arr);
            //     // foreach (var i in arr)
            //     // {
            //     //     Debug.Log($"radX : {i.radialCoefficientX}, radY : {i.radialCoefficientY}, radZ : {i.radialCoefficientZ}");
            //     //     Debug.Log($"tanX : {i.tangentialCoefficientX}, tanY : {i.tangentialCoefficientY}");
            //     //     Debug.Log($"prinX : {i.principalPointX}, prinY : {i.principalPointY}");
            //     //     Debug.Log($"FocalLenX : {i.focalLengthX}, FocalLenY : {i.focalLengthY}");
            //     // }

            //     // var arr1 = new float4x4[1];
            //     // dbg_cameraToWorldMatBuf.GetData(arr1);
            //     // foreach (var i in arr1)
            //     // {
            //     //     Debug.Log($"Camera To World");
            //     //     Debug.Log($"c0 : {i.c0.x}, {i.c0.y}, {i.c0.z}, {i.c0.w}");
            //     //     Debug.Log($"c1 : {i.c1.x}, {i.c1.y}, {i.c1.z}, {i.c1.w}");
            //     //     Debug.Log($"c2 : {i.c2.x}, {i.c2.y}, {i.c2.z}, {i.c2.w}");
            //     //     Debug.Log($"c3 : {i.c3.x}, {i.c3.y}, {i.c3.z}, {i.c3.w}");
            //     // }

            //     // var arr2 = new float4x4[1];
            //     // dbg_cameraInverseProjectionBuf.GetData(arr2);
            //     // foreach (var i in arr2)
            //     // {
            //     //     Debug.Log($"Camera Inverse Projection");
            //     //     Debug.Log($"c0 : {i.c0.x}, {i.c0.y}, {i.c0.z}, {i.c0.w}");
            //     //     Debug.Log($"c1 : {i.c1.x}, {i.c1.y}, {i.c1.z}, {i.c1.w}");
            //     //     Debug.Log($"c2 : {i.c2.x}, {i.c2.y}, {i.c2.z}, {i.c2.w}");
            //     //     Debug.Log($"c3 : {i.c3.x}, {i.c3.y}, {i.c3.z}, {i.c3.w}");
            //     // }
            // }

            if (Input.GetKeyDown(KeyCode.A))
            {
                var arr = new float[1];
                dbg_usedHeightBuf.GetData(arr);
                foreach (var i in arr)
                {
                    Debug.Log($"Used Height");
                    Debug.Log($"{i}");
                }
            }
            #endregion dbg
        }

        void PopulateKernels()
        {
            DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Dome_Reflector_Cube_Panorama,
                danbiShader.FindKernel("Dome_Reflector_Cube_Panorama"));
            // DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Dome_Reflector_Cylinder_Panorama,
            //     rayTracingShader.FindKernel("Dome_Reflector_Cylinder_Panorama"));            
        }

        void SetShaderParams()
        {
            UnityEngine.Random.InitState(seedDateTime.Millisecond);
            danbiShader.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
            danbiShader.SetInt("_isPanoramaTex", m_isPanoramaTex);
            danbiShader.SetInt("_MaxBounce", m_maxNumOfBounce);
            danbiShader.SetVector("_centerOfPanoramaMesh", m_centerOfPanoramaMesh);
        }

        /// <summary>
        /// Called on GenerateImage()
        /// </summary>
        /// <param name="usedTexList"></param>
        /// <param name="x"></param>
        /// <param name="screenResolutions"></param>
        public void SetBuffersAndRenderTextures(List<Texture2D> usedTexList, (int x, int y) screenResolutions)
        {
            // DanbiComputeShaderHelper.ClearRenderTexture(resultRT_LowRes);
            // DanbiComputeShaderHelper.ClearRenderTexture(convergedResultRT_HiRes);

            m_samplingCounter = 0;
            // 01. Prepare RenderTextures.
            DanbiComputeShaderHelper.PrepareRenderTextures(screenResolutions,
                                                           out m_samplingCounter,
                                                           ref m_resultRT_LowRes,
                                                           ref m_convergedRT_HiRes);

            // 02. Prepare the current kernel for connecting Compute Shader.                    
            int currentKernel = DanbiKernelHelper.CurrentKernelIndex;

            #region dbg
            danbiShader.SetBuffer(currentKernel, "dbg_usedHeight", dbg_usedHeightBuf);
            // danbiShader.SetBuffer(currentKernel, "dbg_centerOfPano", dbg_centerOfPanoBuf);
            // danbiShader.SetBuffer(currentKernel, "dbg_rayLengthBuf", dbg_rayLengthBuf);
            danbiShader.SetBuffer(currentKernel, "dbg_hitInfoBuf", dbg_hitInfoBuf);
            // danbiShader.SetBuffer(currentKernel, "dbg_CameraInternalData", dbg_cameraInternalDataBuf);
            // danbiShader.SetBuffer(currentKernel, "dbg_cameraToWorldMat", dbg_cameraToWorldMatBuf);
            // danbiShader.SetBuffer(currentKernel, "dbg_cameraInverseProjection", dbg_cameraInverseProjectionBuf);
            // danbiShader.SetBuffer(currentKernel, "dbg_centerOfPanoBuf", dbg_centerOfPanoBuf);
            // rayTracingShader.SetBuffer(currentKernel, "_Dbg_direct", DanbiDbg.Dbg   Buf_direct);
            #endregion dbg


            // 03. Set the other parameters as buffer into the ray tracing compute shader.
            danbiShader.SetBuffer(currentKernel, "_DomeData", bufferDict.GetBuffer("_DomeData"));
            danbiShader.SetBuffer(currentKernel, "_PanoramaData", bufferDict.GetBuffer("_PanoramaData"));
            danbiShader.SetBuffer(currentKernel, "_Vertices", bufferDict.GetBuffer("_Vertices"));
            danbiShader.SetBuffer(currentKernel, "_Indices", bufferDict.GetBuffer("_Indices"));
            danbiShader.SetBuffer(currentKernel, "_Texcoords", bufferDict.GetBuffer("_Texcoords"));

            // 04. Textures.            
            danbiShader.SetTexture(currentKernel, "_DistortedImage", resultRT_LowRes);

            // Set the camera parameters to the compute shader.
            DanbiManager.instance.cameraControl.SetCameraParametersToComputeShader(this);   // this == DanbiComputeShaderControl            

            // Set the textures to the compute shader
            if (usedTexList.Count == 1)
            {
                danbiShader.SetTexture(currentKernel, "_Tex0", usedTexList[0]);
                danbiShader.SetTexture(currentKernel, "_Tex1", usedTexList[0]);
                danbiShader.SetTexture(currentKernel, "_Tex2", usedTexList[0]);
                danbiShader.SetTexture(currentKernel, "_Tex3", usedTexList[0]);
                danbiShader.SetInt("_NumOfTex", 1);
            }
            else if (usedTexList.Count == 2)
            {
                danbiShader.SetTexture(currentKernel, "_Tex0", usedTexList[0]);
                danbiShader.SetTexture(currentKernel, "_Tex1", usedTexList[1]);
                danbiShader.SetTexture(currentKernel, "_Tex2", usedTexList[1]);
                danbiShader.SetTexture(currentKernel, "_Tex3", usedTexList[1]);
                danbiShader.SetInt("_NumOfTex", 2);
            }
            else if (usedTexList.Count == 3)
            {
                danbiShader.SetTexture(currentKernel, "_Tex0", usedTexList[0]);
                danbiShader.SetTexture(currentKernel, "_Tex1", usedTexList[1]);
                danbiShader.SetTexture(currentKernel, "_Tex2", usedTexList[2]);
                danbiShader.SetTexture(currentKernel, "_Tex3", usedTexList[2]);
                danbiShader.SetInt("_NumOfTex", 3);
            }
            else if (usedTexList.Count == 4)
            {
                danbiShader.SetTexture(currentKernel, "_Tex0", usedTexList[0]);
                danbiShader.SetTexture(currentKernel, "_Tex1", usedTexList[1]);
                danbiShader.SetTexture(currentKernel, "_Tex2", usedTexList[2]);
                danbiShader.SetTexture(currentKernel, "_Tex3", usedTexList[3]);
                danbiShader.SetInt("_NumOfTex", 4);
            }
        } // SetBuffersAndRenderTextures()

        public void Dispatch((int x, int y) threadGroups, RenderTexture dest)
        {
            // Dispatch with the current kernel.
            danbiShader.Dispatch(DanbiKernelHelper.CurrentKernelIndex, threadGroups.x, threadGroups.y, 1);

            // Check Screen Sampler and apply it.      
            m_addMaterial_ScreenSampling.SetFloat("_SampleCount", m_samplingCounter);

            // Sample the result into the ConvergedResultRT to improve the aliasing quality.
            Graphics.Blit(resultRT_LowRes, convergedResultRT_HiRes, m_addMaterial_ScreenSampling);
            Graphics.Blit(convergedResultRT_HiRes, dest);

            // Upscale float precisions to improve the resolution of the result RenderTextue and blit to dest rendertexture.

            #region unused
            // TODO: sRGB of the rendertexture Test
            // GL.sRGBWrite = true;

            // RenderTexture prevRT = RenderTexture.active;
            // RenderTexture.active = convergedResultRT_HiRes;

            // RenderTexture.active = prevRT;
            #endregion unused

            // Update the sample counts.
            if (m_samplingCounter++ > m_samplingThreshold)
            {
                m_samplingCounter = 0;

                onSampleFinished?.Invoke(convergedResultRT_HiRes);
                DanbiManager.instance.m_distortedImageRenderFinished = true;

                // You should set 
                // The above onSampleFinished delegate will call  the following: It simply sets the global member variable
                // m_distoredRT to   convergedResultRT_HiRes;
            }
        } // Dispatch(): This method is called every frame in OnRenderImage()
    };
};
