LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_CFLAGS := -DNDEBUG -O2 -Wall
LOCAL_LDLIBS := -llog
LOCAL_MODULE    := RT2DArrayNativePlugin
LOCAL_SRC_FILES := unityplugin.cpp

LOCAL_LDLIBS	+= -lGLESv3			# OpenGL ES 3.0
LOCAL_LDLIBS	+= -lEGL			# GL platform interface
# LOCAL_LDLIBS	+= -llog			# logging
LOCAL_LDLIBS	+= -landroid		# native windows

include $(BUILD_SHARED_LIBRARY)
