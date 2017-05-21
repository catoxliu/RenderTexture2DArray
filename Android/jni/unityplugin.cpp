/*
 * unityplugin.cpp
 *
 *      Author: liushijian
 */

#include "unityplugin.h"

static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;
static UnityGfxRenderer s_RendererType = kUnityGfxRendererNull;
static GLuint frameBufferTextureId = 0;
static GLuint frameBufferDepthTextureId = 0;

// Unity plugin load event
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
    UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_UnityInterfaces = unityInterfaces;
    s_Graphics = unityInterfaces->Get<IUnityGraphics>();

    s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);

    // Run OnGraphicsDeviceEvent(initialize) manually on plugin load
    // to not miss the event in case the graphics device is already initialized
    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

// Unity plugin unload event
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
    UnityPluginUnload()
{
    s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

static void UNITY_INTERFACE_API
    OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    switch (eventType)
    {
        case kUnityGfxDeviceEventInitialize:
        {
            s_RendererType = s_Graphics->GetRenderer();
            glFramebufferTextureMultiviewOVR = (PFNGLFRAMEBUFFERTEXTUREMULTIVIEWOVR)eglGetProcAddress ("glFramebufferTextureMultiviewOVR");
            if (!glFramebufferTextureMultiviewOVR)
            {
            	ANDROID_LOG("Can not get proc address for glFramebufferTextureMultiviewOVR.\n");
            	//exit(EXIT_FAILURE);
            }

            //Use Unity to init Texture2DArray, easy to use in Unity.
//            GL_CHECK(glGenTextures(1, &frameBufferTextureId));
//            GL_CHECK(glBindTexture(GL_TEXTURE_2D_ARRAY, frameBufferTextureId));
//            GL_CHECK(glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MIN_FILTER, GL_LINEAR));
//            GL_CHECK(glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MAG_FILTER, GL_LINEAR));
//            GL_CHECK(glTexStorage3D(GL_TEXTURE_2D_ARRAY, 1, GL_RGBA32F, 1024, 1024, 2));

            GL_CHECK(glGenTextures(1, &frameBufferDepthTextureId));
            GL_CHECK(glBindTexture(GL_TEXTURE_2D_ARRAY, frameBufferDepthTextureId));
            GL_CHECK(glTexStorage3D(GL_TEXTURE_2D_ARRAY, 1, GL_DEPTH_COMPONENT24, 1024, 1024, 2));
            break;
        }
        case kUnityGfxDeviceEventShutdown:
        {
            s_RendererType = kUnityGfxRendererNull;
            //TODO: Destroy FBO
            break;
        }
        case kUnityGfxDeviceEventBeforeReset:
        {
            //No use now
            break;
        }
        case kUnityGfxDeviceEventAfterReset:
        {
            //No use now
            break;
        }
    };
}

//Use for Debug
static bool CheckFramebufferStatus()
{
	GLenum result = GL_CHECK(glCheckFramebufferStatus(GL_DRAW_FRAMEBUFFER));
	if (result != GL_FRAMEBUFFER_COMPLETE)
	{
		ANDROID_LOG("Frambeuffer incomplete [%08x] with %d %d", result, frameBufferTextureId, frameBufferDepthTextureId);
		//GL_CHECK(glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0));
		return false;
	} else
	{
		ANDROID_LOG("Frambeuffer COMPLETE with %d %d", frameBufferTextureId, frameBufferDepthTextureId);
		return true;
	}
}

static void HackFrambuffer()
{
//	CheckFramebufferStatus();

	GL_CHECK(glFramebufferTexture2D(GL_DRAW_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, 0, 0));
//	CheckFramebufferStatus();

	GL_CHECK(glFramebufferRenderbuffer(GL_DRAW_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, 0));
//	CheckFramebufferStatus();

	GL_CHECK(glFramebufferRenderbuffer(GL_DRAW_FRAMEBUFFER, GL_STENCIL_ATTACHMENT, GL_RENDERBUFFER, 0));
//	CheckFramebufferStatus();

//	GL_CHECK(glBindTexture(GL_TEXTURE_2D_ARRAY, frameBufferTextureId));
	GL_CHECK(glFramebufferTextureMultiviewOVR(GL_DRAW_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, frameBufferTextureId, 0, 0, 2));
//	CheckFramebufferStatus();

//	GL_CHECK(glBindTexture(GL_TEXTURE_2D_ARRAY, frameBufferDepthTextureId));
	GL_CHECK(glFramebufferTextureMultiviewOVR(GL_DRAW_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, frameBufferDepthTextureId, 0, 0, 2));
	CheckFramebufferStatus();

	//Use Unity commandbuffer to clear
	//GL_CHECK(glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT));
}

// Plugin function to handle a specific rendering event
static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	switch ((UnityCameraEvent)eventID)
	{
		case BeforeDepthTexture:
		{
			ANDROID_LOG("BeforeDepthTexture");
			break;
		}
		case AfterDepthTexture:
		{
			ANDROID_LOG("AfterDepthTexture");
			break;
		}
		case BeforeDepthNormalsTexture:
		{
			ANDROID_LOG("BeforeDepthNormalsTexture");
			break;
		}
		case AfterDepthNormalsTexture:
		{
			ANDROID_LOG("AfterDepthNormalsTexture");
			break;
		}
		case BeforeGBuffer:
		{
			ANDROID_LOG("BeforeGBuffer");
			break;
		}
		case AfterGBuffer:
		{
			ANDROID_LOG("AfterGBuffer");
			break;
		}
		case BeforeLighting:
		{
			ANDROID_LOG("BeforeLighting");
			break;
		}
		case AfterLighting:
		{
			ANDROID_LOG("AfterLighting");
			break;
		}
		case BeforeFinalPass:
		{
			ANDROID_LOG("BeforeFinalPass");
			break;
		}
		case AfterFinalPass:
		{
			ANDROID_LOG("AfterFinalPass");
			break;
		}
		case BeforeForwardOpaque:
		{
			ANDROID_LOG("BeforeForwardOpaque");
			if (frameBufferTextureId > 0) HackFrambuffer();
			break;
		}
		case AfterForwardOpaque:
		{
			ANDROID_LOG("AfterForwardOpaque");
			break;
		}
		case BeforeImageEffectsOpaque:
		{
			ANDROID_LOG("BeforeImageEffectsOpaque");
			break;
		}
		case AfterImageEffectsOpaque:
		{
			ANDROID_LOG("AfterImageEffectsOpaque");
			break;
		}
		case BeforeSkybox:
		{
			ANDROID_LOG("BeforeSkybox");
			break;
		}
		case AfterSkybox:
		{
			ANDROID_LOG("AfterSkybox");
			break;
		}
		case BeforeForwardAlpha:
		{
			ANDROID_LOG("BeforeForwardAlpha");
			break;
		}
		case AfterForwardAlpha:
		{
			ANDROID_LOG("AfterForwardAlpha");
			break;
		}
		case BeforeImageEffects:
		{
			ANDROID_LOG("BeforeImageEffects");
			break;
		}
		case AfterImageEffects:
		{
			ANDROID_LOG("AfterImageEffects");
			break;
		}
		case AfterEverything:
		{
			ANDROID_LOG("AfterEverything");
			break;
		}
		case BeforeReflections:
		{
			ANDROID_LOG("BeforeReflections");
			break;
		}
		case AfterReflections:
		{
			ANDROID_LOG("AfterReflections");
			break;
		}
	}
//	PrintCurrentFrameBufferID();
}

//Used for debug
static void PrintCurrentFrameBufferID()
{
	GLint drawFboId = 0, type = 0, name = 0, width = 0, height = 0;
	GL_CHECK(glGetIntegerv(GL_DRAW_FRAMEBUFFER_BINDING, &drawFboId));
	GL_CHECK(glGetFramebufferAttachmentParameteriv(GL_DRAW_FRAMEBUFFER,
			GL_COLOR_ATTACHMENT0, GL_FRAMEBUFFER_ATTACHMENT_OBJECT_TYPE, &type));
	GL_CHECK(glGetFramebufferAttachmentParameteriv(GL_DRAW_FRAMEBUFFER,
				GL_COLOR_ATTACHMENT0, GL_FRAMEBUFFER_ATTACHMENT_OBJECT_NAME, &name));
//	GL_CHECK(glBindTexture(GL_TEXTURE_2D, name));
//	GL_CHECK(glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_WIDTH, &width));
//	GL_CHECK(glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_HEIGHT, &height));
//	GL_CHECK(glBindTexture(GL_TEXTURE_2D, 0));
	ANDROID_LOG("Current FBO : %d name %d type %08x width %d height %d", drawFboId, name, type, width, height);
}

// Freely defined function to pass a callback to plugin-specific scripts
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
    GetRenderEventFunc()
{
    return OnRenderEvent;
}

static void UNITY_INTERFACE_API OnTextureID(int textureID)
{
	frameBufferTextureId = textureID;
	ANDROID_LOG("frameBufferTextureId [%d]", frameBufferTextureId);
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
    GetTextureIDFunc()
{
    return OnTextureID;
}
