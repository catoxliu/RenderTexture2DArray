using UnityEngine;

namespace RT2DARRAY
{
    public sealed class GL
    {
        public static void LoadOrtho(Material mat)
        {
            Matrix4x4 proj = Matrix4x4.Ortho(0, 1, 0, 1, 0.01f, 100);
            proj = UnityEngine.GL.GetGPUProjectionMatrix(proj, false);
            UnityEngine.GL.LoadIdentity();
            UnityEngine.GL.LoadProjectionMatrix(proj);

            MultipleMatrix("unity_CameraProjection", "unity_StereoCameraProjection", mat);
            MultipleMatrix("unity_CameraInvProjection", "unity_StereoCameraInvProjection", mat);
            MultipleMatrix("glstate_matrix_projection", "unity_StereoMatrixP", mat);
            MultipleMatrix("unity_MatrixVP", "unity_StereoMatrixVP", mat);
            MultipleMatrix("unity_MatrixInvV", "unity_StereoMatrixInvV", mat);
            MultipleMatrix("unity_MatrixV", "unity_StereoMatrixV", mat);
            MultipleMatrix("unity_WorldToCamera", "unity_StereoWorldToCamera", mat);
            MultipleMatrix("unity_CameraToWorld", "unity_StereoCameraToWorld", mat);
            MultipleMatrix("_WorldSpaceCameraPos", "unity_StereoWorldSpaceCameraPos", mat);
        }

        static void MultipleMatrix(string shader_name, string target_name, Material mat)
        {
            Matrix4x4 tmp = Shader.GetGlobalMatrix(shader_name);
            if (tmp == null)
            {
                Debug.LogError("No shader name " + shader_name);
                return;
            }
            //Debug.Log(shader_name + tmp);
            Matrix4x4[] result = new Matrix4x4[2];
            result[1] = tmp;
            result[0] = tmp;
            mat.SetMatrixArray(target_name, result);
        }
    }
}