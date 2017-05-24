using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RT2DARRAY
{
    public class RenderTexture2DArray : MonoBehaviour
    {
		//The render texture for this camera, will be generated in Awake.
        public Texture2DArray renderTexture;

		//this component must be attached on a Camera
        private Camera mainCam = null;

        //Shader Variables used for single-pass stereo rendering
        private Vector4[] unity_StereoScaleOffset = new Vector4[2];
        private Matrix4x4[] unity_StereoMatrixP = new Matrix4x4[2];
        private Matrix4x4[] unity_StereoWorldToCamera = new Matrix4x4[2];
        private Matrix4x4[] unity_StereoCameraToWorld = new Matrix4x4[2];
        private Matrix4x4[] unity_StereoMatrixV = new Matrix4x4[2];
        private Matrix4x4[] unity_StereoMatrixInvV = new Matrix4x4[2];
        private Matrix4x4[] unity_StereoMatrixVP = new Matrix4x4[2];

		//the eye distance, will use the value from the Camera.stereoSeparation
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

			//Enable these keywords to let the unity shaders works for single pass stereo rendering
            Shader.EnableKeyword("STEREO_MULTIVIEW_ON");
            Shader.EnableKeyword("UNITY_SINGLE_PASS_STEREO");

			//Texture2DArray could only be generated in runtime
            renderTexture = new Texture2DArray(1024, 1024, 2, TextureFormat.ARGB32, false);
            System.IntPtr tid = renderTexture.GetNativeTexturePtr();
            Debug.Log("Texture ID " + (int)tid);
			//Tell native plugin the texture id
            NativePlugin.SetTextureID((int)tid);
        }


        // Use this for initialization
        void Start()
        {
			//we need a dummy RenderTexture for camera to let it do off-screen rendering.
            RenderTexture dummyRT = new RenderTexture(1024,
                                        1024,
                                        24,
                                        RenderTextureFormat.ARGB32);
            dummyRT.antiAliasing = 1;
            dummyRT.useMipMap = false;
            dummyRT.autoGenerateMips = false;
			//Even though we could set the dimension here, it will not work properly anyway.
            dummyRT.dimension = TextureDimension.Tex2DArray;
            dummyRT.volumeDepth = 2;
            dummyRT.Create();
			
            mainCam.targetTexture = dummyRT;
            mainCam.forceIntoRenderTexture = true;

			//Very few documentations about this variable, not sure if I set this right.
            Vector4[] unity_StereoScaleOffset = new Vector4[2];
            unity_StereoScaleOffset[0] = new Vector4(1.0f, 1.0f, 0f, 0f);
            unity_StereoScaleOffset[1] = new Vector4(1.0f, 1.0f, 0.5f, 0f);
            Shader.SetGlobalVectorArray("unity_StereoScaleOffset", unity_StereoScaleOffset);

            //Add callback to hack frambuffer
			//In unity forward rendering path, the BeforeForwardOpaque event will occur first
			//so, here is the point we change the framebuffer's binding in native plugin.
            NativePlugin.AddCameraCallbacks(mainCam, CameraEvent.BeforeForwardOpaque);

            //Clear framebuffer.
			//In many cases Unity will always clear it for us, this is like a double-check
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
			//Unity will not handle these Stereo shader variables for us, so we have to set it all by ourselves
			
			//If the camera's fov, ratio and clip plane didn't change, the projection matrix will stay the same.
			//I just copy the normal shader variables because I just want two same eyes.
			//Of course you can calculate your own Projection Matrix for each eye and only set it once if you 
			//don't need to change it anymore.
            MultipleMatrix("unity_CameraProjection", "unity_StereoCameraProjection");
            MultipleMatrix("unity_CameraInvProjection", "unity_StereoCameraInvProjection");
            unity_StereoMatrixP = MultipleMatrix("glstate_matrix_projection", "unity_StereoMatrixP");

			//Since eyes are moving, so below variables need to re-calculate every frame.
            Matrix4x4 world2Camera = mainCam.worldToCameraMatrix;
            Matrix4x4 camera2World = mainCam.cameraToWorldMatrix;
            Matrix4x4 c2w_L, c2w_R, w2c_L, w2c_R;

			//The camera is the center point of two eyes
            Matrix4x4 eyeOffsetMatrix = Matrix4x4.TRS(eyeOffsetVector, Quaternion.identity, Vector3.one);
            w2c_L = world2Camera * eyeOffsetMatrix;
            w2c_R = world2Camera * eyeOffsetMatrix.inverse;

            unity_StereoWorldToCamera[0] = w2c_L;
            unity_StereoWorldToCamera[1] = w2c_R;
            Shader.SetGlobalMatrixArray("unity_StereoWorldToCamera", unity_StereoWorldToCamera);

            c2w_L = camera2World * eyeOffsetMatrix;
            c2w_R = camera2World * eyeOffsetMatrix.inverse;
            unity_StereoCameraToWorld[0] = c2w_L;
            unity_StereoCameraToWorld[1] = c2w_R;
            Shader.SetGlobalMatrixArray("unity_StereoCameraToWorld", unity_StereoCameraToWorld);

			//So the camera positons
            Vector4[] result = new Vector4[2];
            result[1] = mainCam.transform.position + eyeOffsetVector;
            result[0] = mainCam.transform.position - eyeOffsetVector;
            Shader.SetGlobalVectorArray("unity_StereoWorldSpaceCameraPos", result);

			//camera.worldToCameraMatrix is the view matrix
            Shader.SetGlobalMatrixArray("unity_StereoMatrixV", unity_StereoWorldToCamera);
            unity_StereoMatrixInvV[0] = w2c_L.inverse;
            unity_StereoMatrixInvV[1] = w2c_R.inverse;
            Shader.SetGlobalMatrixArray("unity_StereoMatrixInvV", unity_StereoMatrixInvV);

			//MatrixVP is the value UNITY_MATRIX_VP used in shader
            unity_StereoMatrixVP[0] = unity_StereoMatrixP[0] * w2c_L;
            unity_StereoMatrixVP[1] = unity_StereoMatrixP[1] * w2c_R;
            Shader.SetGlobalMatrixArray("unity_StereoMatrixVP", unity_StereoMatrixVP);
			
			//The workaround for Skybox rendering
			//Since Unity5, skybox rendering after forward opaque
			//As skybox need a particular MatrixVP, two CommandBuffer used to handle this.
			//The MatrixVP must be changed back after skybox rendering.
            CommandBuffer afterSkyCB = new CommandBuffer();
            mainCam.RemoveCommandBuffers(CameraEvent.AfterSkybox);
            afterSkyCB.SetGlobalMatrixArray("unity_StereoMatrixVP", unity_StereoMatrixVP);
            mainCam.AddCommandBuffer(CameraEvent.AfterSkybox, afterSkyCB);

			//Skybox View Matrix should be at world zero point.
			//As in OpenGL, camera's forward is the negative Z axis
            Matrix4x4 viewMatrix1 = Matrix4x4.LookAt(Vector3.zero, mainCam.transform.forward, mainCam.transform.up) * Matrix4x4.Scale(new Vector3(1, 1, -1));
			//Change it from column major to row major.
            viewMatrix1 = viewMatrix1.transpose;
            Matrix4x4 proj = unity_StereoMatrixP[0];
			//Trick here. I supporse skybox doesn't need clip in Projection Matrix
			//And m22 and m23 is calculated by clip near/far, -1 is the default value of m22.
            proj.m22 = -1.0f;
            Matrix4x4[] skybox_MatrixVP = new Matrix4x4[2];
            skybox_MatrixVP[0] = proj * viewMatrix1;
            skybox_MatrixVP[1] = proj * viewMatrix1;

			//The MatrixVP should be set before skybox rendering.
            CommandBuffer beforeSkyCB = new CommandBuffer();
            mainCam.RemoveCommandBuffers(CameraEvent.BeforeSkybox);
            beforeSkyCB.SetGlobalMatrixArray("unity_StereoMatrixVP", skybox_MatrixVP);
            mainCam.AddCommandBuffer(CameraEvent.BeforeSkybox, beforeSkyCB);
        }
    }

}
