# RenderTexture2DArray
Using Unity5.6(above) single-pass stereo rendering on Android (supported Multi-View) to render to a Texture2DArray (off-screen).

# What's this Used for?
* Since Unity5.6 give us a new feature to support [single-pass stereo rendering](https://docs.unity3d.com/Manual/SinglePassStereoRendering.html) on Android platform through [Multi-View](https://www.khronos.org/registry/OpenGL/extensions/OVR/OVR_multiview.txt) (seems particularly for GearVR with Oculus SDK selected). [Here](http://malideveloper.arm.com/downloads/deved/tutorial/SDK/android/2.1/multiview.html) is a simple example show you how Multi-View works and [ScriptableRenderLoop](https://github.com/Unity-Technologies/ScriptableRenderLoop) really give me an insight about unity rendering loop. So I am trying to use this feature on our standalone VR headset, and this is the basic solution.
* Simply, this is a plugin you can used to let Unity Camera render to a Texture2DArray by 2 eyes. Then you can use this Texture2DArray to do whatever you want, for example, render them to a screen. It is very useful for VR, because you could [save drawcalls (half of two cameras) and CPU overhead](https://developer.oculus.com/documentation/mobilesdk/latest/concepts/mobile-multiview).

# Usage
* This could only be used on an Android device with OpenGL 3 and GL_OVR_multiview extension supported.
* Unity5.6 and above, ensure the Virtual Reality Supported checkbox is ticked, then tick the Single-Pass Stereo Rendering checkbox underneath it.
* Using "Split Stereo Display (non head-mounted)" for sdks.
* Attach "RenderTexture2DArray" to the Camera you want to use and configure "Stereo Separation" to control two eyes distance.
* You can get the Texture2DArray through RenderTexture2DArray.renderTexture after its Awake.
* There is a simple demo I made in this repo, I used another Camera to render two Quad to screen to show the Texture2DArray's content.

# Known issues and bugs
* For someone may want to modify this plugin to integrate with their own project or just do some development with it, I would like to share these with you.
* Firstly, this Unity feature is a preview one, which means it's buggy and not stable.
* Secondly, this solution is a tricky way, I use native plugin to change Unity framebuffer to bind with my Texture2DArray and enable Unity's shader keywords "STEREO_MULTIVIEW_ON" to let it do the magic. So you may get wrong or nothing if you are not using the proper shader.
* Last, I calculate matrix for two eyes and set them for shaders in **OnPreRender** step, so if you want do magic about view projection matrix, you should probably do it here.
* **The biggest problem now is that I CANNOT render the SKYBOX properly!** I am still working on this and if anyone has any thoughts about it, will be glad to hear that.
