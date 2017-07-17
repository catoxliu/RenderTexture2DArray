using System.Runtime.InteropServices;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RT2DARRAY
{
    public class NativePlugin
    {

        private const string libCameraCallbacks = "RT2DArrayNativePlugin";
        [DllImport(libCameraCallbacks)]
        private static extern IntPtr GetRenderEventFunc();
        [DllImport(libCameraCallbacks)]
        private static extern IntPtr GetTextureIDFunc();
        [DllImport(libCameraCallbacks)]
        private static extern void SetAntiAliasingLevel(int level);

        public static void AddCameraCallbacks(Camera cam, CameraEvent camEvent)
        {
#if !UNITY_EDITOR
        CommandBuffer cb = new CommandBuffer();
        cb.IssuePluginEvent(GetRenderEventFunc(), (int)camEvent);
        cam.AddCommandBuffer(camEvent, cb);
#endif
        }

        public static void SetTextureID(int id)
        {
#if !UNITY_EDITOR
        GL.IssuePluginEvent(GetTextureIDFunc(), id);
#endif
        }
        
        public static void SetAntiAliasing(int level)
        {
#if !UNITY_EDITOR
            SetAntiAliasingLevel(level);
#endif
        }

    }
}

