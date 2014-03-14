LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := lodepng
LOCAL_SRC_FILES := lodepng.c
include $(BUILD_SHARED_LIBRARY)
