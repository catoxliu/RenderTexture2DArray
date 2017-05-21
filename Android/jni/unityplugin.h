/*
 * unityplugin.h
 *
 *      Author: liushijian
 */

#ifndef JNI_UNITYPLUGIN_H_
#define JNI_UNITYPLUGIN_H_

#include <stdio.h>
#include <stdlib.h>
#include <jni.h>
#include <EGL/egl.h>
#include <GLES3/gl3.h>
#include <GLES3/gl31.h>
#include <assert.h>
#include <android/log.h>

#include "IUnityInterface.h"
#include "IUnityGraphics.h"

typedef enum UnityCameraEvent
{
	BeforeDepthTexture = 0,
	AfterDepthTexture = 1,
	BeforeDepthNormalsTexture = 2,
	AfterDepthNormalsTexture = 3,
	BeforeGBuffer = 4,
	AfterGBuffer = 5,
	BeforeLighting = 6,
	AfterLighting = 7,
	BeforeFinalPass = 8,
	AfterFinalPass = 9,
	BeforeForwardOpaque = 10,
	AfterForwardOpaque = 11,
	BeforeImageEffectsOpaque = 12,
	AfterImageEffectsOpaque = 13,
	BeforeSkybox = 14,
	AfterSkybox = 15,
	BeforeForwardAlpha = 16,
	AfterForwardAlpha = 17,
	BeforeImageEffects = 18,
	AfterImageEffects = 19,
	AfterEverything = 20,
	BeforeReflections = 21,
	AfterReflections = 22,
} UnityCameraEvent;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
	UnityPluginLoad(IUnityInterfaces* unityInterfaces);
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
	UnityPluginUnload();
static void UNITY_INTERFACE_API
	OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);
static void PrintCurrentFrameBufferID();

template<typename... Args>
static void ANDROID_LOG(const char *fmt, Args... args)
{
	__android_log_print(ANDROID_LOG_INFO, "CC_JNI", fmt, args...);
}

void CheckOpenGLError(const char* stmt, const char* fname, int line)
{
	GLenum err = glGetError();
	if (err != GL_NO_ERROR)
	{
		ANDROID_LOG("OpenGL error %08x, at %s:%i - for %s\n", err, fname, line, stmt);
		abort();
	}
}

typedef void (*PFNGLFRAMEBUFFERTEXTUREMULTIVIEWOVR) (GLenum, GLenum, GLuint, GLint, GLint, GLsizei);
PFNGLFRAMEBUFFERTEXTUREMULTIVIEWOVR glFramebufferTextureMultiviewOVR;


#ifdef _DEBUG
#define GL_CHECK(stmt)
do { \
	stmt; \
	CheckOpenGLError(#stmt, __FILE__, __LINE__); \
} while (0)
#else
	#define GL_CHECK(stmt) stmt
#endif

#endif /* JNI_UNITYPLUGIN_H_ */
