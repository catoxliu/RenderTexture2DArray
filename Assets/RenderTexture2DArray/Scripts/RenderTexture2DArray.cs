using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RT2DARRAY
{
    public class RenderTexture2DArray : MonoBehaviour
    {
        public Texture2DArray renderTexture;

        private Camera mainCam = null;

        //Shader Variables
        private Vector4[] unity_StereoScaleOffset = new Vector4[2];
        private Matrix4x4[] unity_StereoMatrixP = new Matrix4x4[2];
        private Matrix4x4[] unity_StereoWorldToCamera = new Matrix4x4[2];
        private Matrix4x4[] unity_StereoCameraToWorld = new Matrix4x4[2];
        private Matrix4x4[] unity_StereoMatrixV = new Matrix4x4[2];
        private Matrix4x4[] unity_StereoMatrixInvV = new Matrix4x4[2];
        private Matrix4x4[] unity_StereoMatrixVP = new Matrix4x4[2];

        private Vector3 eyeOffsetVector = Vector3.zero;

        void Awake()
        {
            mainCam = GetComponent<Camera>();
            if (mainCam == null)
            {
                Debug.LogError("Can't find Camera!");
                DestroyImmediate(this);
                return;
            }

            Shader.EnableKeyword("STEREO_MULTIVIEW_ON");
            Shader.EnableKeyword("UNITY_SINGLE_PASS_STEREO");

            renderTexture = new Texture2DArray(1024, 1024, 2, TextureFormat.ARGB32, false);
            System.IntPtr tid = renderTexture.GetNativeTexturePtr();
            Debug.Log("Texture ID " + (int)tid);
            NativePlugin.SetTextureID((int)tid);
        }


        // Use this for initialization
        void Start()
        {
            RenderTexture dummyRT = new RenderTexture(1024,
                                        1024,
                                        24,
                                        RenderTextureFormat.ARGB32);
            dummyRT.antiAliasing = 1;
            dummyRT.useMipMap = false;
            dummyRT.autoGenerateMips = false;
            dummyRT.dimension = TextureDimension.Tex2DArray;
            dummyRT.volumeDepth = 2;
            dummyRT.Create();
            mainCam.targetTexture = dummyRT;
            mainCam.forceIntoRenderTexture = true;

            Vector4[] unity_StereoScaleOffset = new Vector4[2];
            unity_StereoScaleOffset[0] = new Vector4(1.0f, 1.0f, 0f, 0f);
            unity_StereoScaleOffset[1] = new Vector4(1.0f, 1.0f, 0.5f, 0f);
            Shader.SetGlobalVectorArray("unity_StereoScaleOffset", unity_StereoScaleOffset);

            //Add callback to hack frambuffer
            NativePlugin.AddCameraCallbacks(mainCam, CameraEvent.BeforeForwardOpaque);

            //Clear framebuffer
            CommandBuffer cb = new CommandBuffer();
            cb.ClearRenderTarget(true, true, mainCam.backgroundColor);
            mainCam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cb);

            float eyeOffset = mainCam.stereoSeparation / 2.0f;
            eyeOffsetVector = new Vector3(eyeOffset, 0, 0);
        }

        Matrix4x4[] MultipleMatrix(string shader_name, string target_name)
        {
            Matrix4x4 tmp = Shader.GetGlobalMatrix(shader_name);
            if (tmp == null)
            {
                Debug.LogError("No shader name " + shader_name);
                return null;
            }
            Matrix4x4[] result = new Matrix4x4[2];
            result[1] = tmp;
            result[0] = tmp;
            Shader.SetGlobalMatrixArray(target_name, result);
            return result;
        }

        public void OnPreRender()
        {
            MultipleMatrix("unity_CameraProjection", "unity_StereoCameraProjection");
            MultipleMatrix("unity_CameraInvProjection", "unity_StereoCameraInvProjection");
            unity_StereoMatrixP = MultipleMatrix("glstate_matrix_projection", "unity_StereoMatrixP");

            Matrix4x4 world2Camera = mainCam.worldToCameraMatrix;
            Matrix4x4 camera2World = mainCam.cameraToWorldMatrix;
            Matrix4x4 c2w_L, c2w_R, w2c_L, w2c_R;

            Matrix4x4 eyeOffsetMat = Matrix4x4.TRS(eyeOffsetVector, Quaternion.identity, Vector3.one);
            w2c_L = world2Camera * eyeOffsetMat;
            w2c_R = world2Camera * eyeOffsetMat.inverse;

            unity_StereoWorldToCamera[0] = w2c_L;
            unity_StereoWorldToCamera[1] = w2c_R;
            Shader.SetGlobalMatrixArray("unity_StereoWorldToCamera", unity_StereoWorldToCamera);

            c2w_L = camera2World * eyeOffsetMat;
            c2w_R = camera2World * eyeOffsetMat.inverse;
            unity_StereoCameraToWorld[0] = c2w_L;
            unity_StereoCameraToWorld[1] = c2w_R;
            Shader.SetGlobalMatrixArray("unity_StereoCameraToWorld", unity_StereoCameraToWorld);

            Vector4[] result = new Vector4[2];
            result[1] = mainCam.transform.position + eyeOffsetVector;
            result[0] = mainCam.transform.position - eyeOffsetVector;
            Shader.SetGlobalVectorArray("unity_StereoWorldSpaceCameraPos", result);

            Shader.SetGlobalMatrixArray("unity_StereoMatrixV", unity_StereoWorldToCamera);
            unity_StereoMatrixInvV[0] = w2c_L.inverse;
            unity_StereoMatrixInvV[1] = w2c_R.inverse;
            Shader.SetGlobalMatrixArray("unity_StereoMatrixInvV", unity_StereoMatrixInvV);

            unity_StereoMatrixVP[0] = unity_StereoMatrixP[0] * w2c_L;
            unity_StereoMatrixVP[1] = unity_StereoMatrixP[1] * w2c_R;
            Shader.SetGlobalMatrixArray("unity_StereoMatrixVP", unity_StereoMatrixVP);
        }
    }

}
