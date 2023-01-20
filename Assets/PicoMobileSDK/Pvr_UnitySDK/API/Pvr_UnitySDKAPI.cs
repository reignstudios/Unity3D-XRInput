// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID 
#define ANDROID_DEVICE
#endif

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Pvr_UnitySDKAPI
{
    public enum GlobalIntConfigs
    {
        EYE_TEXTURE_RESOLUTION0,
        EYE_TEXTURE_RESOLUTION1,
        SEENSOR_COUNT,
        ABILITY6DOF,
        PLATFORM_TYPE,
        TRACKING_MODE, 
        LOG_LEVEL,
        ENBLE_HAND6DOF_BY_HEAD,
        ENBLE_6DOF_GLOBAL_TRACKING,
        TARGET_FRAME_RATE,
        iShowFPS,
        SensorMode,
        LOGICFLOW,// 0 ,1 Viewer	
        EYE_TEXTURE_RES_HIGH,
        EYE_TEXTURE_RES_NORMAL,
        iCtrlModelLoadingPri,
        iPhoneHMDModeEnabled,
        isEnableBoundary,
        Enable_Activity_Rotation,

    };

    public enum GlobalFloatConfigs
    {
        IPD,
        VFOV,
        HFOV,
        NECK_MODEL_X,
        NECK_MODEL_Y,
        NECK_MODEL_Z,
        DISPLAY_REFRESH_RATE
    };

    public enum RenderTextureAntiAliasing
    {
        X_1 = 1,
        X_2 = 2,
        X_4 = 4,
        X_8 = 8,
    }

    public enum PlatForm
    {
        Android = 1,
        IOS = 2,
        Win = 3,
        Notsupport = 4,
    }

    public enum RenderTextureDepth
    {
        BD_0 = 0,
        BD_16 = 16,
        BD_24 = 24,
    }

    public enum RenderTextureLevel
    {
        Normal,
        High
    }

    public enum Sensorindex
    {
        Default = 0,
        FirstSensor = 1,
        SecondSensor = 2,
    }

    public enum Eye
    {
        LeftEye = 0,
        RightEye,
        BothEye
    }

    public enum ResUtilsType
    {
        TYPE_TEXTSIZE = 0,
        TYPE_COLOR = 1,
        TYPE_TEXT = 2,
        TYPE_FONT = 3,
        TYPE_VALUE = 4,
        TYPE_DRAWABLE = 5,
        TYPE_OBJECT = 6,
        TYPR_OBJECTARRAY = 7,
    }

    /// <summary>
    /// Device Tracking Origin
    ///     Represents how the SDK is reporting pose data
    /// 
    /// EyeLevel:
    ///     Represents the tracking origin whereby (0,0,0) is on the "eye"(virtual camera).
    ///     This means that pose data returned in this mode will not include the user height.
    /// 
    /// FloorLevel:
    ///     Represents the tracking origin whereby (0,0,0) is on the "floor" or other surface detected by Device.
    ///     This means that pose data returned in this mode will include the height that user defined.
    /// </summary>
    public enum TrackingOrigin
    {
        EyeLevel,
        FloorLevel
    }

    public enum EFoveationLevel
    {
        None = -1,
        Low = 0,
        Med = 1,
        High = 2
    }

    public enum StereoRenderingPathPico
    {
        MultiPass,
        SinglePass,
    }

    #region EyeTracking 
    public enum pvrEyePoseStatus
    {
        kGazePointValid = (1 << 0),
        kGazeVectorValid = (1 << 1),
        kEyeOpennessValid = (1 << 2),
        kEyePupilDilationValid = (1 << 3),
        kEyePositionGuideValid = (1 << 4)
    };

    public enum TrackingMode
    {
        PVR_TRACKING_MODE_ROTATION = 0x1,
        PVR_TRACKING_MODE_POSITION = 0x2,
        PVR_TRACKING_MODE_EYE = 0x4
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct EyeTrackingData
    {
        public int leftEyePoseStatus;          //!< Bit field (pvrEyePoseStatus) indicating left eye pose status
        public int rightEyePoseStatus;         //!< Bit field (pvrEyePoseStatus) indicating right eye pose status
        public int combinedEyePoseStatus;      //!< Bit field (pvrEyePoseStatus) indicating combined eye pose status

        public Vector3 leftEyeGazePoint;        //!< Left Eye Gaze Point
        public Vector3 rightEyeGazePoint;       //!< Right Eye Gaze Point
        public Vector3 combinedEyeGazePoint;    //!< Combined Eye Gaze Point (HMD center-eye point)

        public Vector3 leftEyeGazeVector;       //!< Left Eye Gaze Point
        public Vector3 rightEyeGazeVector;      //!< Right Eye Gaze Point
        public Vector3 combinedEyeGazeVector;   //!< Comnbined Eye Gaze Vector (HMD center-eye point)

        public float leftEyeOpenness;            //!< Left eye value between 0.0 and 1.0 where 1.0 means fully open and 0.0 closed.
        public float rightEyeOpenness;           //!< Right eye value between 0.0 and 1.0 where 1.0 means fully open and 0.0 closed.

        public float leftEyePupilDilation;       //!< Left eye value in millimeters indicating the pupil dilation
        public float rightEyePupilDilation;      //!< Right eye value in millimeters indicating the pupil dilation

        public Vector3 leftEyePositionGuide;    //!< Position of the inner corner of the left eye in meters from the HMD center-eye coordinate system's origin.
        public Vector3 rightEyePositionGuide;   //!< Position of the inner corner of the right eye in meters from the HMD center-eye coordinate system's origin.
        public Vector3 foveatedGazeDirection;   //!< Position of the gaze direction in meters from the HMD center-eye coordinate system's origin.
        public int foveatedGazeTrackingState; //!< The current state of the foveatedGazeDirection signal.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] reserved;               //!< reserved
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EyeTrackingGazeRay
    {
        /// <summary>
        /// Vector in world space with the gaze direction.
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// IsValid is true when there is available gaze data.
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// The middle of the eyes in world space.
        /// </summary>
        public Vector3 Origin;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EyeDeviceInfo
    {
        public ViewFrustum targetFrustumLeft;
        public ViewFrustum targetFrustumRight;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ViewFrustum
    {
        public float left;           //!< Left Plane of Frustum
        public float right;          //!< Right Plane of Frustum
        public float top;            //!< Top Plane of Frustum
        public float bottom;         //!< Bottom Plane of Frustum

        public float near;           //!< Near Plane of Frustum
        public float far;            //!< Far Plane of Frustum (Arbitrary)
    }
    #endregion 

    [StructLayout(LayoutKind.Sequential)]
    public struct EyeSetting
    {
        public Transform eyelocalPosition;
        public Rect eyeRect;
        public float eyeFov;
        public float eyeAspect;
        public Matrix4x4 eyeProjectionMatrix;
        public Shader eyeShader;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sensor
    {
#if ANDROID_DEVICE
        public const string LibFileName = "Pvr_UnitySDK";
#else
        public const string LibFileName = "Pvr_UnitySDK";
#endif


#if ANDROID_DEVICE
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_Enable6DofModule(bool enable);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_OptionalResetSensor(int index, int resetRot, int resetPos);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_Init(int index);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_StartSensor(int index);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_StopSensor(int index);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_ResetSensor(int index);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_ResetSensorAll(int index);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetSensorState(int index, ref float x, ref float y, ref float z, ref float w, ref float px, ref float py, ref float pz);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetMainSensorState(ref float x, ref float y, ref float z, ref float w, ref float px, ref float py, ref float pz, ref float vfov,ref float hfov, ref int viewNumber);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetPsensorState();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetHmdPSensorStatus();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetSensorAcceleration(int index, ref float x, ref float y, ref float z);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetSensorGyroscope(int index, ref float x, ref float y, ref float z);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetSensorMagnet(int index, ref float x, ref float y, ref float z);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_Get6DofSensorQualityStatus();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_Get6DofSafePanelFlag();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_SetReinPosition(float x, float y, float z,float w, float px,float py,float pz, int hand, bool valid, int key);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_SetTrackingOriginType(TrackingOrigin trackingOriginType);
#endif


        #region Public Funcation
        public static bool UPvr_Pvr_Get6DofSafePanelFlag()
        {
#if ANDROID_DEVICE
            return Pvr_Get6DofSafePanelFlag();
#endif
            return false;

        }
        public static int UPvr_Init(int index)
        {
#if ANDROID_DEVICE
            return Pvr_Init(index);
#endif
            return 0;
        }
        public static void UPvr_InitPsensor()
        {
            Pvr_InitPsensor();
        }
        public static int UPvr_GetPsensorState()
        {
            int platformType = -1;
#if ANDROID_DEVICE
            int enumindex = (int)GlobalIntConfigs.PLATFORM_TYPE;
            Render.UPvr_GetIntConfig(enumindex, ref platformType);
#endif
            if (platformType == 1)
            {
#if ANDROID_DEVICE
                return Pvr_GetPsensorState();
#else
                return 0;
#endif
            }
            else
            {
                int state = Pvr_GetAndroidPsensorState();
                if (state != 0 && state != -1)
                {
                    state = 1;
                }
                return state;
            }

        }

        public static int UPvr_GetPSensorStatus()
        {
#if ANDROID_DEVICE
             return Pvr_GetHmdPSensorStatus();
#endif
            return 0;

        }
        public static void UPvr_UnregisterPsensor()
        {
            Pvr_UnregisterPsensor();
        }
        public static int UPvr_StartSensor(int index)
        {
#if ANDROID_DEVICE
            return Pvr_StartSensor(index);
#else
            return 0;
#endif
        }
        public static int UPvr_StopSensor(int index)
        {
#if ANDROID_DEVICE
            return Pvr_StopSensor(index);
#else
            return 0;
#endif
        }
        public static int UPvr_ResetSensor(int index)
        {
            Pvr_UnitySDKManager.SDK.resetBasePos = new Vector3();
#if ANDROID_DEVICE
            return Pvr_ResetSensor(index);
#else
            return 0;
#endif
        }
        public static int UPvr_OptionalResetSensor(int index, int resetRot, int resetPos)
        {
#if ANDROID_DEVICE
            return Pvr_OptionalResetSensor(index, resetRot, resetPos);
#else
            return 0;
#endif
        }
        public static int UPvr_GetSensorState(int index, ref float x, ref float y, ref float z, ref float w, ref float px, ref float py, ref float pz)
        {
#if ANDROID_DEVICE
            return Pvr_GetSensorState(index, ref x, ref y, ref z, ref w, ref px, ref py, ref pz);
#else
            return 0;
#endif
        }
        public static int UPvr_GetMainSensorState(ref float x, ref float y, ref float z, ref float w, ref float px, ref float py, ref float pz, ref float vfov, ref float hfov, ref int viewNumber)
        {
#if ANDROID_DEVICE
            return Pvr_GetMainSensorState(ref x, ref y, ref z, ref w, ref px, ref py, ref pz, ref vfov,ref hfov, ref viewNumber);
#else
            return 0;
#endif
        }

        public static int UPvr_GetSensorAcceleration(int index, ref float x, ref float y, ref float z)
        {
#if ANDROID_DEVICE
            return Pvr_GetSensorAcceleration(index, ref x, ref y, ref z);
#else
            return 0;
#endif
        }

        public static int UPvr_GetSensorGyroscope(int index, ref float x, ref float y, ref float z)
        {
#if ANDROID_DEVICE
            return Pvr_GetSensorGyroscope(index, ref x, ref y, ref z);;
#else
            return 0;
#endif
        }

        public static int UPvr_GetSensorMagnet(int index, ref float x, ref float y, ref float z)
        {
#if ANDROID_DEVICE
            return Pvr_GetSensorMagnet(index, ref x, ref y, ref z);
#else
            return 0;
#endif
        }

        public static int UPvr_Get6DofSensorQualityStatus()
        {
#if ANDROID_DEVICE
            return Pvr_Get6DofSensorQualityStatus();
#else
            return 0;
#endif
        }

        public static int UPvr_Enable6DofModule(bool enable)
        {
#if ANDROID_DEVICE
            return    Pvr_Enable6DofModule(enable);
#endif
            return 0;
        }
        public static void Pvr_InitPsensor()
        {
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(Pvr_UnitySDKRender.javaSysActivityClass, "initPsensor",Pvr_UnitySDKManager.pvr_UnitySDKRender.activity);
            }
            catch (Exception e)
            {
                PLOG.E("Error :" + e.ToString());
            }
#endif
        }

        public static bool Pvr_IsHead6dofReset()
        {
            int value = 0;
            Render.UPvr_GetIntConfig((int)GlobalIntConfigs.isEnableBoundary, ref value);
            if (value > 0)
            {
                return false;
            }
                
            bool state = false;
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref state,Pvr_UnitySDKRender.javaSysActivityClass, "isHead6dofReset", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity);
            }
            catch (Exception e)
            {
                PLOG.E("Error :" + e.ToString());
            }
#endif
            return state;
        }
        public static int Pvr_GetAndroidPsensorState()
        {
            int psensor = -1;
#if ANDROID_DEVICE
    
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>( ref psensor,Pvr_UnitySDKRender.javaSysActivityClass, "getPsensorState");
            }
            catch (Exception e)
            {
                PLOG.E("Error :" + e.ToString());
            }
#endif
            return psensor;
        }
        public static void Pvr_UnregisterPsensor()
        {
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(Pvr_UnitySDKRender.javaSysActivityClass, "unregisterListener");
            }
            catch (Exception e)
            {
                PLOG.E("Error :" + e.ToString());
            }
#endif
        }
        public static int UPvr_ResetSensorAll(int index)
        {
#if ANDROID_DEVICE
                return Pvr_ResetSensorAll(index);   
#endif
            return 0;
        }

        public static void UPvr_SetReinPosition( float x, float y, float z,float w,float px, float py,float pz, int hand, bool valid, int key)
        {
            if (PLOG.logLevel > 2)
            {
                PLOG.D("PvrLog UPvr_SetReinPosition" + x + y + z + w + px + py + pz + hand + valid + key);
            }
#if ANDROID_DEVICE
            Pvr_SetReinPosition(x,y,z,w,px,py,pz,hand,valid,key);
#endif
        }

        public static bool UPvr_SetTrackingOriginType(TrackingOrigin trackingOriginType)
        {
            bool ret = true;
#if ANDROID_DEVICE
            ret = Pvr_SetTrackingOriginType(trackingOriginType);
#endif
            return ret;
        }
        #endregion

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Render
    {
#if ANDROID_DEVICE
        public const string LibFileName = "Pvr_UnitySDK";
#endif

#if ANDROID_DEVICE
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void Pvr_ChangeScreenParameters(string model, int width, int height, double xppi, double yppi, double densityDpi );

		[DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
		private static extern int Pvr_SetRatio(float midH, float midV);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_SetPupillaryPoint(bool enable);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Pvr_GetSupportHMDTypes();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_SetCurrentHMDType([MarshalAs(UnmanagedType.LPStr)]string type);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetIntConfig(int configsenum, ref int res);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetFloatConfig(int configsenum, ref float res);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_SetupLayerData(int layerIndex, int sideMask, int textureId, int textureType, int layerFlags, float[] colorScaleAndOffset);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_SetupLayerCoords(int layerIndex, int sideMask, float[] lowerLeft, float[] lowerRight, float[] upperLeft, float[] upperRight);

        // 2D Overlay
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_SetOverlayModelViewMatrix(int overlayType, int overlayShape, int texId, int eyeSide, int layerIndex, bool isHeadLocked, int layerFlags,
                                                                 float[] mvMatrix, float[] modelS, float[] modelR, float[] modelT, float[] cameraR, float[] cameraT, float[] colorScaleAndOffset);

        // Foveation
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_SetFoveationParameters(int textureId, int previousId,
                                                              float focalPointX, float focalPointY,
                                                              float foveationGainX, float foveationGainY,
                                                              float foveationArea, float foveationMinimum);

        // ColorSpace
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_SetColorspaceType(int colorspaceType);

        // External Surface
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Pvr_CreateLayerAndroidSurface(int layerType, int layerIndex);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Pvr_GetLayerAndroidSurface(int layerType, int layerIndex);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_SetMonoMode(bool openMono);
#endif


        #region Public Funcation

        public static void UPvr_ChangeScreenParameters(string model, int width, int height, double xppi, double yppi, double densityDpi)
		{
#if ANDROID_DEVICE
			Pvr_ChangeScreenParameters(model,  width,  height,  xppi,  yppi, densityDpi );
#endif
        }

        public static int UPvr_SetRatio(float midH, float midV)
        {
#if ANDROID_DEVICE
            return Pvr_SetRatio(midH, midV);
#endif
            return 0;
        }

        public static EFoveationLevel GetFoveatedRenderingLevel()
        {
#if ANDROID_DEVICE
            return Pvr_UnitySDKEyeManager.Instance.FoveationLevel;
#endif
            return EFoveationLevel.None;
        }

        public static void SetFoveatedRenderingLevel(EFoveationLevel level)
        {
#if ANDROID_DEVICE
            Pvr_UnitySDKEyeManager.Instance.FoveationLevel = level;
#endif
        }

        public static void GetFoveatedRenderingParameters(ref Vector2 ffrGainValue, ref float ffrAreaValue, ref float ffrMinimumValue)
        {
#if ANDROID_DEVICE
            Pvr_UnitySDKEye.GetFoveatedRenderingParameters(ref ffrGainValue, ref ffrAreaValue, ref ffrMinimumValue);
#endif
        }

        public static void SetFoveatedRenderingParameters(Vector2 ffrGainValue, float ffrAreaValue, float ffrMinimumValue)
        {
#if ANDROID_DEVICE
            Pvr_UnitySDKEye.SetFoveatedRenderingParameters(ffrGainValue, ffrAreaValue, ffrMinimumValue);
#endif
        }

        // Foveation
        public static void UPvr_SetFoveationParameters(int textureId, int previousId, float focalPointX, float focalPointY, float foveationGainX, float foveationGainY, float foveationArea, float foveationMinimum)
        {
#if ANDROID_DEVICE
            Pvr_SetFoveationParameters(textureId, previousId, focalPointX, focalPointY, foveationGainX, foveationGainY, foveationArea, foveationMinimum);
#endif
        }

        public static int UPvr_GetIntConfig(int configsenum, ref int res)
        {
#if ANDROID_DEVICE
            return Pvr_GetIntConfig(configsenum, ref res);
#else
            return 0;
#endif
        }

        public static int UPvr_GetFloatConfig(int configsenum, ref float res)
        {
#if ANDROID_DEVICE
            return Pvr_GetFloatConfig(configsenum, ref res);
#else
            return 0;
#endif
        }
        public static string UPvr_GetSupportHMDTypes()
        {
#if ANDROID_DEVICE
            IntPtr ptr = Pvr_GetSupportHMDTypes();
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
#endif
            return null;

        }
        public static void UPvr_SetCurrentHMDType(string type)
        {
#if ANDROID_DEVICE
            Pvr_SetCurrentHMDType(type);
#endif
        }

        // StandTexture Overlay
        public static void UPvr_SetupLayerData(int layerIndex, int sideMask, int textureId, int textureType, int layerFlags, Vector4 colorScale, Vector4 colorOffset)
        {
#if ANDROID_DEVICE
            float[] colorScaleAndOffset = new float[8];
            colorScaleAndOffset[0] = colorScale.x;
            colorScaleAndOffset[1] = colorScale.y;
            colorScaleAndOffset[2] = colorScale.z;
            colorScaleAndOffset[3] = colorScale.w;

            colorScaleAndOffset[4] = colorOffset.x;
            colorScaleAndOffset[5] = colorOffset.y;
            colorScaleAndOffset[6] = colorOffset.z;
            colorScaleAndOffset[7] = colorOffset.w;

            Pvr_SetupLayerData(layerIndex, sideMask, textureId, textureType, layerFlags, colorScaleAndOffset);
#endif
        }

        public static void UPvr_SetupLayerCoords(int layerIndex, int sideMask, float[] lowerLeft, float[] lowerRight, float[] upperLeft, float[] upperRight)
        {
#if ANDROID_DEVICE
             Pvr_SetupLayerCoords(layerIndex, sideMask, lowerLeft, lowerRight, upperLeft, upperRight);
#endif
        }

        public static void UPvr_SetOverlayModelViewMatrix(int overlayType, int overlayShape, int texId, int eyeSide, int layerIndex, bool isHeadLocked, int layerFlags, Matrix4x4 mvMatrix, Vector3 modelS, Quaternion modelR, Vector3 modelT, Quaternion cameraR, Vector3 cameraT, Vector4 colorScale, Vector4 colorOffset)
        {
#if ANDROID_DEVICE
            float[] mvMat = new float[16];
            mvMat[0] = mvMatrix.m00; mvMat[1] = mvMatrix.m01; mvMat[2] = mvMatrix.m02; mvMat[3] = mvMatrix.m03;
            mvMat[4] = mvMatrix.m10; mvMat[5] = mvMatrix.m11; mvMat[6] = mvMatrix.m12; mvMat[7] = mvMatrix.m13;
            mvMat[8] = mvMatrix.m20; mvMat[9] = mvMatrix.m21; mvMat[10] = mvMatrix.m22; mvMat[11] = mvMatrix.m23;
            mvMat[12] = mvMatrix.m30; mvMat[13] = mvMatrix.m31; mvMat[14] = mvMatrix.m32; mvMat[15] = mvMatrix.m33;
			float[] scaleM = new float[3];
			scaleM[0] = modelS.x; scaleM[1] = modelS.y; scaleM[2] = modelS.z;

			float[] rotationM = new float[4];
			rotationM[0] = modelR.x; rotationM[1] = modelR.y; rotationM[2] = modelR.z; rotationM[3] = modelR.w;

			float[] translationM = new float[3];
			translationM[0] = modelT.x; translationM[1] = modelT.y; translationM[2] = modelT.z;

			float[] rotationC = new float[4];
			rotationC[0] = cameraR.x; rotationC[1] = cameraR.y; rotationC[2] = cameraR.z; rotationC[3] = cameraR.w;

			float[] translationC = new float[3];
			translationC[0] = cameraT.x; translationC[1] = cameraT.y; translationC[2] = cameraT.z;

            float[] colorScaleAndOffset = new float[8];
            colorScaleAndOffset[0] = colorScale.x;
            colorScaleAndOffset[1] = colorScale.y;
            colorScaleAndOffset[2] = colorScale.z;
            colorScaleAndOffset[3] = colorScale.w;

            colorScaleAndOffset[4] = colorOffset.x;
            colorScaleAndOffset[5] = colorOffset.y;
            colorScaleAndOffset[6] = colorOffset.z;
            colorScaleAndOffset[7] = colorOffset.w;


            Pvr_SetOverlayModelViewMatrix(overlayType, overlayShape, texId, eyeSide, layerIndex, isHeadLocked, layerFlags,  mvMat, scaleM, rotationM, translationM, rotationC, translationC, colorScaleAndOffset);
#endif
        }

        public static void UPvr_SetColorspaceType(int colorspaceType)
        {
#if ANDROID_DEVICE
            Pvr_SetColorspaceType(colorspaceType);
#endif
        }

        public static IntPtr UPvr_CreateLayerAndroidSurface(int layerType, int layerIndex)
        {
#if ANDROID_DEVICE
            return Pvr_CreateLayerAndroidSurface(layerType, layerIndex);
#else
            return IntPtr.Zero;
#endif
        }

        public static IntPtr UPvr_GetLayerAndroidSurface(int layerType, int layerIndex)
        {
#if ANDROID_DEVICE
            return Pvr_GetLayerAndroidSurface(layerType, layerIndex);
#else
            return IntPtr.Zero;
#endif
        }

        public static void UPvr_SetMonoMode(bool openMono)
        {
#if ANDROID_DEVICE
            Pvr_SetMonoMode(openMono);
#endif
        }
#endregion

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct System
    {
#if ANDROID_DEVICE
        public const string LibFileName = "Pvr_UnitySDK";
#else
        public const string LibFileName = "Pvr_UnitySDK";
#endif

        public const string UnitySDKVersion = "2.8.5.6";

#if ANDROID_DEVICE
		[DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Pvr_SetInitActivity(IntPtr activity, IntPtr vrActivityClass);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Pvr_GetSDKVersion();   
		[DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pvr_GetHmdHardwareVersion();
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Pvr_GetHmdFirmwareVersion();
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Pvr_GetHmdSerialNumber();
		
		[DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PVR_GetHmdBatteryLevel();
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PVR_GetHmdBatteryStatus();
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float PVR_GetHmdBatteryTemperature();
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PVR_SetHmdAudioStatus(bool enable);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Pvr_GetEyeTrackingData(ref int leftEyePoseStatus, ref int rightEyePoseStatus, ref int combinedEyePoseStatus,
                                                         ref float leftEyeGazePointX, ref float leftEyeGazePointY, ref float leftEyeGazePointZ,
                                                         ref float rightEyeGazePointX, ref float rightEyeGazePointY, ref float rightEyeGazePointZ,
                                                         ref float combinedEyeGazePointX, ref float combinedEyeGazePointY, ref float combinedEyeGazePointZ,
                                                         ref float leftEyeGazeVectorX, ref float leftEyeGazeVectorY, ref float leftEyeGazeVectorZ,
                                                         ref float rightEyeGazeVectorX, ref float rightEyeGazeVectorY, ref float rightEyeGazeVectorZ,
                                                         ref float combinedEyeGazeVectorX, ref float combinedEyeGazeVectorY, ref float combinedEyeGazeVectorZ,
                                                         ref float leftEyeOpenness, ref float rightEyeOpenness,
                                                         ref float leftEyePupilDilation, ref float rightEyePupilDilation,
                                                         ref float leftEyePositionGuideX, ref float leftEyePositionGuideY, ref float leftEyePositionGuideZ,
                                                         ref float rightEyePositionGuideX, ref float rightEyePositionGuideY, ref float rightEyePositionGuideZ,
                                                         ref float foveatedGazeDirectionX, ref float foveatedGazeDirectionY, ref float foveatedGazeDirectionZ,
                                                         ref int foveatedGazeTrackingState);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Pvr_SetTrackingMode(int trackingMode);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pvr_GetTrackingMode();
		[DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetRenderEventFunc();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]       
        public static extern void UnityEventData(long data);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Pvr_EnableSinglePass(bool enable);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Pvr_SetAntiAliasing(int antiAliasing);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Pvr_SinglePassBeforeForwardOpaque();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Pvr_SetCurrentRenderTexture(uint textureId);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Pvr_SetSinglePassDepthBufferWidthHeight(int width, int height);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PVR_setPerformanceLevels(int cpuLevel, int gpuLevel);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Pvr_SetIPD(float distance);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float Pvr_GetIPD();
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Pvr_SetTrackingIPDEnabled(bool enable);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Pvr_GetTrackingIPDEnabled();
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Pvr_GetEyeTrackingAutoIPD(ref float autoIPD);
#endif


        #region Public Funcation
        public static bool UPvr_CallStaticMethod<T>(ref T result, UnityEngine.AndroidJavaClass jclass, string name, params object[] args)
        {
            try
            {
                result = jclass.CallStatic<T>(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                PLOG.E("Exception calling static method " + name + ": " + e);
                return false;
            }
        }

        public static bool UPvr_CallStaticMethod(UnityEngine.AndroidJavaObject jobj, string name, params object[] args)
        {
            try
            {
                jobj.CallStatic(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                PLOG.E("CallStaticMethod  Exception calling activity method " + name + ": " + e);
                return false;
            }
        }

        public static bool UPvr_CallMethod<T>(ref T result, UnityEngine.AndroidJavaObject jobj, string name, params object[] args)
        {
            try
            {
                result = jobj.Call<T>(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                PLOG.E("Exception calling activity method " + name + ": " + e);
                return false;
            }
        }

        public static bool UPvr_CallMethod(UnityEngine.AndroidJavaObject jobj, string name, params object[] args)
        {
            try
            {
                jobj.Call(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                PLOG.E(" Exception calling activity method " + name + ": " + e);
                return false;
            }
        }

        public static string UPvr_GetSDKVersion()
        {
#if ANDROID_DEVICE
            IntPtr ptr = Pvr_GetSDKVersion();
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
#endif
            return "";
        }

        public static string UPvr_GetUnitySDKVersion()
        {
            return UnitySDKVersion;

        }
        public static string UPvr_GetDeviceMode()
        {
            string devicemode = "";
#if ANDROID_DEVICE
            devicemode = SystemInfo.deviceModel;
#endif
            return devicemode;
        }

        public static string UPvr_GetDeviceModel()
        {
            return SystemInfo.deviceModel;
        }
        public static string UPvr_GetDeviceSN()
        {
            string serialNum = "UNKONWN";
#if ANDROID_DEVICE
            System.UPvr_CallStaticMethod<string>(ref serialNum, Pvr_UnitySDKRender.javaSysActivityClass, "getDeviceSN");
#endif
            return serialNum;
        }


        public static AndroidJavaObject UPvr_GetCurrentActivity()
        {
            AndroidJavaObject currentActivity = null;
#if ANDROID_DEVICE
            UnityEngine.AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");          
#endif
            return currentActivity;
        }


        public static void UPvr_ShutDown()
        {
#if ANDROID_DEVICE
            System.UPvr_CallStaticMethod(Pvr_UnitySDKRender.javaSysActivityClass, "Pvr_ShutDown");
#endif
        }
        public static bool UPvr_SetMonoPresentation()
        {
            bool value = false;
#if ANDROID_DEVICE
           value =  System.UPvr_CallMethod(UPvr_GetCurrentActivity(), "Pvr_setMonoPresentation");
#endif
            return value;
        }

        public static bool UPvr_IsPresentationExisted()
        {
            bool value = false;
            bool result = false;
#if ANDROID_DEVICE
           value = System.UPvr_CallMethod<bool>(ref result, UPvr_GetCurrentActivity(), "Pvr_isPresentationExisted");
#endif
            return value && result;
        }

        public static bool UPvr_GetMainActivityPauseStatus()
        {
            bool ret = false;
            bool isPause = false;
#if ANDROID_DEVICE
            ret = System.UPvr_CallMethod<bool>(ref isPause, UPvr_GetCurrentActivity(), "Pvr_getMainActivityPauseStatus");
#endif
            return ret && isPause;
        }

        public static void UPvr_Reboot()
        {
#if ANDROID_DEVICE
            System.UPvr_CallStaticMethod( Pvr_UnitySDKRender.javaSysActivityClass, "Pvr_Reboot",UPvr_GetCurrentActivity());
#endif
        }

        public static void UPvr_Sleep()
        {
#if ANDROID_DEVICE
            System.UPvr_CallStaticMethod( Pvr_UnitySDKRender.javaSysActivityClass, "Pvr_Sleep");
#endif
        }

        public static bool UPvr_StartHomeKeyReceiver(string startreceivre)
        {
#if ANDROID_DEVICE
            try
            {
                if (Pvr_UnitySDKManager.pvr_UnitySDKRender !=null)
                {
					Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(Pvr_UnitySDKRender.javaVrActivityLongReceiver, "Pvr_StartReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity, startreceivre);
                    PLOG.I("Start home key Receiver");
                    return true;
                }
              
            }
            catch (Exception e)
            {
                PLOG.E("Start home key  Receiver  Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }
        public static bool UPvr_StopHomeKeyReceiver()
        {
#if ANDROID_DEVICE
            try
            {
                if (Pvr_UnitySDKManager.pvr_UnitySDKRender !=null)
                {
					Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(Pvr_UnitySDKRender.javaVrActivityLongReceiver, "Pvr_StopReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity);
                    PLOG.I("Stop home key Receiver");
                    return true;
                }
              
            }
            catch (Exception e)
            {
                PLOG.E("Stop home key Receiver Error :" + e.ToString());
                return false;
            }
#endif
            return true;
        }
        public static void UPvr_StartVRModel()
        {
#if ANDROID_DEVICE
             Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(Pvr_UnitySDKRender.javaVrActivityClass, "startVRModel");
#endif
        }
        public static void UPvr_RemovePlatformLogo()
        {
#if ANDROID_DEVICE
			Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(Pvr_UnitySDKRender.javaVrActivityClass, "removePlatformLogo");
#endif
        }
        public static void UPvr_ShowPlatformLogo()
        {
#if ANDROID_DEVICE
			Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(Pvr_UnitySDKRender.javaVrActivityClass, "showPlatformLogo");
#endif
        }

        public static bool UPvr_IsPicoActivity()
        {
            bool ret = false;
            bool isPause = false;
#if ANDROID_DEVICE
            ret = Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref isPause,Pvr_UnitySDKRender.javaVrActivityClass, "isPicoActivity", UPvr_GetCurrentActivity());
#endif
            return ret && isPause;
        }

        public static void UPvr_StopVRModel()
        {
#if ANDROID_DEVICE
             Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(Pvr_UnitySDKRender.javaVrActivityClass, "stopVRModel");
#endif
        }

        public static string UPvr_GetCountryCode()
        {
            string code = "";
#if ANDROID_DEVICE
             System.UPvr_CallStaticMethod<string>(ref code,Pvr_UnitySDKRender.javaVrActivityClass, "getCountryCode",UPvr_GetCurrentActivity());
#endif
            return code;
        }
#endregion

        public static bool UPvr_SetIPD(float distance)
        {
#if ANDROID_DEVICE
            for (int i = 0; i < Pvr_UnitySDKEyeManager.Instance.Eyes.Length; i++)
            {
                Pvr_UnitySDKEyeManager.Instance.Eyes[i].RefreshCameraPosition(distance);
            }

            return Pvr_SetIPD(distance);
#endif
            return false;
        }

        public static float UPvr_GetIPD()
        {
#if ANDROID_DEVICE
            float ipd = Pvr_GetIPD();
            Debug.Log("DISFT IPD:" + ipd);
            return ipd;
#endif
            return 0;
        }

        public static bool UPvr_SetTrackingIPDEnabled(bool enable)
        {
#if ANDROID_DEVICE
            return Pvr_SetTrackingIPDEnabled(enable);
#endif
            return false;
        }

        public static bool UPvr_GetTrackingIPDEnabled()
        {
#if ANDROID_DEVICE
            return Pvr_GetTrackingIPDEnabled();
#endif
            return false;
        }
        
        public static bool UPvr_GetEyeTrackingAutoIPD(ref float autoipd)
        {
#if ANDROID_DEVICE
            return Pvr_GetEyeTrackingAutoIPD(ref autoipd);
#endif
            return false;
        }

        public static void UPvr_UnityEventData(long data)
        {
#if ANDROID_DEVICE
             Pvr_UnitySDKAPI.System.UnityEventData(data);
#endif
        }

        public static long UPvr_GetEyeBufferData(int id)
        {
            return (long)Pvr_UnitySDKManager.SDK.RenderviewNumber << 32 | (long)id;
        }

        public static bool UPvr_checkDevice(string packagename)
        {
            bool value = false;
#if ANDROID_DEVICE
             Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref value,Pvr_UnitySDKRender.javaVrActivityClass, "checkDevice", packagename,UPvr_GetCurrentActivity());
#endif
            return value;
        }

        public static int UPvr_GetHmdHardwareVersion()
        {
#if ANDROID_DEVICE
            return Pvr_GetHmdHardwareVersion();
#endif
            return 0;
        }
        public static string UPvr_GetHmdFirmwareVersion()
        {

#if ANDROID_DEVICE
            IntPtr ptr = Pvr_GetHmdFirmwareVersion();
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
#endif
            return "";
        }
        public static string UPvr_GetHmdSerialNumber()
        {
#if ANDROID_DEVICE
            IntPtr ptr = Pvr_GetHmdSerialNumber();
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
#endif
            return "";
        }
        public static int UPvr_GetHmdBatteryLevel()
        {
#if ANDROID_DEVICE
            return PVR_GetHmdBatteryLevel();
#endif
            return 0;
        }
        public static int UPvr_GetHmdBatteryStatus()
        {
#if ANDROID_DEVICE
            return PVR_GetHmdBatteryStatus();
#endif
            return 0;
        }

        public static float UPvr_GetHmdBatteryTemperature()
        {
#if ANDROID_DEVICE
            return PVR_GetHmdBatteryTemperature();
#endif
            return 0.0f;
        }
        public static int UPvr_SetHmdAudioStatus(bool enable)
        {
#if ANDROID_DEVICE
            return PVR_SetHmdAudioStatus(enable);
#endif
            return 0;
        }

        public static int UPvr_GetTrackingMode()
        {
            int result = 0;
#if ANDROID_DEVICE
            result = Pvr_GetTrackingMode();
#endif
            return result;
        }

        public static bool UPvr_setTrackingMode(int trackingMode)
        {
#if ANDROID_DEVICE
            return Pvr_SetTrackingMode(trackingMode);
#endif
            return false;
        }

        public static bool UPvr_getEyeTrackingData(ref EyeTrackingData trackingData)
        {
#if ANDROID_DEVICE
            bool result = Pvr_GetEyeTrackingData(
                ref trackingData.leftEyePoseStatus, ref trackingData.rightEyePoseStatus, ref trackingData.combinedEyePoseStatus,
                ref trackingData.leftEyeGazePoint.x, ref trackingData.leftEyeGazePoint.y, ref trackingData.leftEyeGazePoint.z,
                ref trackingData.rightEyeGazePoint.x, ref trackingData.rightEyeGazePoint.y, ref trackingData.rightEyeGazePoint.z,
                ref trackingData.combinedEyeGazePoint.x, ref trackingData.combinedEyeGazePoint.y, ref trackingData.combinedEyeGazePoint.z,
                ref trackingData.leftEyeGazeVector.x, ref trackingData.leftEyeGazeVector.y, ref trackingData.leftEyeGazeVector.z,
                ref trackingData.rightEyeGazeVector.x, ref trackingData.rightEyeGazeVector.y, ref trackingData.rightEyeGazeVector.z,
                ref trackingData.combinedEyeGazeVector.x, ref trackingData.combinedEyeGazeVector.y, ref trackingData.combinedEyeGazeVector.z,
                ref trackingData.leftEyeOpenness, ref trackingData.rightEyeOpenness,
                ref trackingData.leftEyePupilDilation, ref trackingData.rightEyePupilDilation,
                ref trackingData.leftEyePositionGuide.x, ref trackingData.leftEyePositionGuide.y, ref trackingData.leftEyePositionGuide.z,
                ref trackingData.rightEyePositionGuide.x, ref trackingData.rightEyePositionGuide.y, ref trackingData.rightEyePositionGuide.z,
                ref trackingData.foveatedGazeDirection.x, ref trackingData.foveatedGazeDirection.y, ref trackingData.foveatedGazeDirection.z,
                ref trackingData.foveatedGazeTrackingState
                );
            trackingData.leftEyeGazeVector.z = -trackingData.leftEyeGazeVector.z;
            trackingData.rightEyeGazeVector.z = -trackingData.rightEyeGazeVector.z;
            trackingData.combinedEyeGazeVector.z = -trackingData.combinedEyeGazeVector.z;

            trackingData.leftEyeGazePoint.z = -trackingData.leftEyeGazePoint.z;
            trackingData.rightEyeGazePoint.z = -trackingData.rightEyeGazePoint.z;
            trackingData.combinedEyeGazePoint.z = -trackingData.combinedEyeGazePoint.z;
            trackingData.foveatedGazeDirection.z = -trackingData.foveatedGazeDirection.z;
            return result;            
#endif
            return false;
        }

        public static bool UPvr_getEyeTrackingGazeRay(ref EyeTrackingGazeRay gazeRay)
        {
#if ANDROID_DEVICE
            EyeTrackingData eyeTrackingData = new EyeTrackingData();
            bool result = Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingData(ref eyeTrackingData);

            gazeRay.IsValid = (eyeTrackingData.combinedEyePoseStatus & (int)Pvr_UnitySDKAPI.pvrEyePoseStatus.kGazePointValid) != 0 && (eyeTrackingData.combinedEyePoseStatus & (int)Pvr_UnitySDKAPI.pvrEyePoseStatus.kGazeVectorValid) != 0;
            if(gazeRay.IsValid)
            {
                gazeRay.Direction = eyeTrackingData.combinedEyeGazeVector;
                gazeRay.Origin = eyeTrackingData.combinedEyeGazePoint;
                gazeRay.Origin = Pvr_UnitySDKManager.SDK.HeadPose.Matrix.MultiplyPoint(gazeRay.Origin);
                gazeRay.Direction = Pvr_UnitySDKManager.SDK.HeadPose.Matrix.MultiplyVector(gazeRay.Direction);
                return true;
            }

            return false;
#endif
            gazeRay.Origin = Vector3.zero;
            gazeRay.Direction = Vector3.forward;
            gazeRay.Origin = Pvr_UnitySDKManager.SDK.HeadPose.Matrix.MultiplyPoint(gazeRay.Origin);
            gazeRay.Direction = Pvr_UnitySDKManager.SDK.HeadPose.Matrix.MultiplyVector(gazeRay.Direction);
            return true;
        }

        public static bool UPvr_getEyeTrackingGazeRayWorld(ref EyeTrackingGazeRay gazeRay)
        {
            Transform target = Pvr_UnitySDKEyeManager.Instance.transform;
            Matrix4x4 mat = Matrix4x4.TRS(target.position, target.rotation, Vector3.one);
#if ANDROID_DEVICE
            EyeTrackingData eyeTrackingData = new EyeTrackingData();
            bool result = Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingData(ref eyeTrackingData);

            gazeRay.IsValid = (eyeTrackingData.combinedEyePoseStatus & (int)Pvr_UnitySDKAPI.pvrEyePoseStatus.kGazePointValid) != 0 && (eyeTrackingData.combinedEyePoseStatus & (int)Pvr_UnitySDKAPI.pvrEyePoseStatus.kGazeVectorValid) != 0;
            if(gazeRay.IsValid)
            {
                gazeRay.Direction = eyeTrackingData.combinedEyeGazeVector;
                gazeRay.Origin = eyeTrackingData.combinedEyeGazePoint;
                gazeRay.Origin = mat.MultiplyPoint(gazeRay.Origin);
                gazeRay.Direction = mat.MultiplyVector(gazeRay.Direction);
                return true;
            }

            return false;
#endif
            gazeRay.IsValid = true;
            gazeRay.Origin = Vector3.zero;
            gazeRay.Direction = Vector3.forward;
            gazeRay.Origin = mat.MultiplyPoint(gazeRay.Origin);
            gazeRay.Direction = mat.MultiplyVector(gazeRay.Direction);
            return true;
        }

        public static Vector3 UPvr_getEyeTrackingPos()
        {
#if ANDROID_DEVICE
            return Pvr_UnitySDKEyeManager.Instance.GetEyeTrackingPos();
#endif
            return Vector3.zero;
        }

        public static int UPvr_GetPhoneScreenBrightness()
        {
            int level = 0;
#if ANDROID_DEVICE
             Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref level,Pvr_UnitySDKRender.javaVrActivityClientClass, "Pvr_GetScreen_Brightness", UPvr_GetCurrentActivity());
#endif
            return level;
        }

        public static void UPvr_SetPhoneScreenBrightness(int level)
        {
#if ANDROID_DEVICE
             Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(Pvr_UnitySDKRender.javaVrActivityClientClass, "Pvr_setAPPScreen_Brightness", UPvr_GetCurrentActivity(),level);
#endif
        }

        public static bool UPvr_IsPicoDefaultActivity()
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    string currentActivityClassName = jo.Call<string>("getLocalClassName");
                    if (currentActivityClassName == "com.unity3d.player.UnityPlayerNativeActivityPico")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool UPvr_EnableSinglePass(bool enable)
        {
#if ANDROID_DEVICE
            return Pvr_UnitySDKAPI.System.Pvr_EnableSinglePass(enable);
#endif
            return false;
        }

        public static void UPvr_SetAntiAliasing(int antiAliasing)
        {
#if ANDROID_DEVICE
            Pvr_UnitySDKAPI.System.Pvr_SetAntiAliasing(antiAliasing);
#endif
        }

        public static void UPvr_SinglePassBeforeForwardOpaque()
        {
#if ANDROID_DEVICE
            Pvr_UnitySDKAPI.System.Pvr_SinglePassBeforeForwardOpaque();
#endif
        }

        public static void UPvr_SetCurrentRenderTexture(uint textureId)
        {
#if ANDROID_DEVICE
            Pvr_UnitySDKAPI.System.Pvr_SetCurrentRenderTexture(textureId);
#endif
        }

        public static bool UPvr_SetSinglePassDepthBufferWidthHeight(int width, int height)
        {
            bool ret = false;
#if ANDROID_DEVICE
            ret = Pvr_UnitySDKAPI.System.Pvr_SetSinglePassDepthBufferWidthHeight(width, height);
#endif
            return ret;
        }

        public static int UPVR_setPerformanceLevels(int cpuLevel, int gpuLevel)
        {
            int result = -1;
#if ANDROID_DEVICE
            result = Pvr_UnitySDKAPI.System.PVR_setPerformanceLevels(cpuLevel, gpuLevel);
#endif
            return result;
        }

        public static int UPvr_GetColorRes(string name)
        {
            int value = -1;
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref value, Pvr_UnitySDKRender.javaVrActivityClass, "getColorRes", name);
            }
            catch (Exception e)
            {
                PLOG.E("GetColorResError :" + e.ToString());
            }
#endif
            return value;
        }

        public static int UPvr_GetConfigInt(string name)
        {
            int value = -1;
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref value, Pvr_UnitySDKRender.javaVrActivityClass, "getConfigInt", name);
            }
            catch (Exception e)
            {
                PLOG.E("GetConfigIntError :" + e.ToString());
            }
#endif
            return value;
        }

        public static string UPvr_GetConfigString(string name)
        {
            string value = "";
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref value, Pvr_UnitySDKRender.javaVrActivityClass, "getConfigString", name);
            }
            catch (Exception e)
            {
                PLOG.E("GetConfigStringError :" + e.ToString());
            }
#endif
            return value;
        }

        public static string UPvr_GetDrawableLocation(string name)
        {
            string value = "";
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref value, Pvr_UnitySDKRender.javaVrActivityClass, "getDrawableLocation", name);
            }
            catch (Exception e)
            {
                PLOG.E("GetDrawableLocationError :" + e.ToString());
            }
#endif
            return value;
        }

        public static int UPvr_GetTextSize(string name)
        {
            int value = -1;
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref value, Pvr_UnitySDKRender.javaVrActivityClass, "getTextSize", name);
            }
            catch (Exception e)
            {
                PLOG.E("GetTextSizeError :" + e.ToString());
            }
#endif
            return value;
        }

        public static string UPvr_GetLangString(string name)
        {
            string value = "";
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref value, Pvr_UnitySDKRender.javaVrActivityClass, "getLangString", name);
            }
            catch (Exception e)
            {
                PLOG.E("GetLangStringError :" + e.ToString());
            }
#endif
            return value;
        }

        public static string UPvr_GetStringValue(string id,int type)
        {
            string value = "";
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref value, Pvr_UnitySDKRender.javaVrActivityClass, "getStringValue", id, type);
            }
            catch (Exception e)
            {
                PLOG.E("GetStringValueError :" + e.ToString());
            }
#endif
            return value;
        }

        public static int UPvr_GetIntValue(string id,int type)
        {
            int value = -1;
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref value, Pvr_UnitySDKRender.javaVrActivityClass, "getIntValue", id, type);
            }
            catch (Exception e)
            {
                PLOG.E("GetIntValueError :" + e.ToString());
            }
#endif
            return value;
        }

        public static float UPvr_GetFloatValue(string id)
        {
            float value = -1;
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<float>(ref value, Pvr_UnitySDKRender.javaVrActivityClass, "getFloatValue", id);
            }
            catch (Exception e)
            {
                PLOG.E("GetFloatValueError :" + e.ToString());
            }
#endif
            return value;
        }

        public static string UPvr_GetObjectOrArray(string id, int type)
        {
            string value = "";
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref value, Pvr_UnitySDKRender.javaVrActivityClass, "getObjectOrArray", id, type);
            }
            catch (Exception e)
            {
                PLOG.E("GetObjectOrArrayError :" + e.ToString());
            }
#endif
            return value;
        }

        public static int UPvr_GetCharSpace(string id)
        {
            int value = -1;
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref value, Pvr_UnitySDKRender.javaVrActivityClass, "getCharSpace", id);
            }
            catch (Exception e)
            {
                PLOG.E("GetCharSpaceError :" + e.ToString());
            }
#endif
            return value;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BoundarySystem
    {
#if ANDROID_DEVICE
        public const string LibFileName = "Pvr_UnitySDK";
#else
        public const string LibFileName = "Pvr_UnitySDK";
#endif

#if ANDROID_DEVICE
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern float Pvr_GetFloorHeight();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetSeeThroughState();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_GetFrameRateLimit();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_IsBoundaryEnable();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_BoundaryGetConfigured();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_BoundaryTestNode(int node, bool isPlayArea, ref bool isTriggering, ref float closestDistance, ref float px, ref float py, ref float pz, ref float nx, ref float ny, ref float nz);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_BoundaryTestPoint(float x, float y, float z, bool isPlayArea, ref bool isTriggering, ref float closestDistance, ref float px, ref float py, ref float pz, ref float nx, ref float ny, ref float nz);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_BoundaryGetGeometry(out IntPtr handle, bool isPlayArea);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_BoundaryGetEnabled();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_BoundarySetVisible(bool value);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_EnableLWRP(bool enable);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_SetViewportSize(int w, int h);

#endif

        /// <summary>
        /// Boundary Type
        /// </summary>
        public enum BoundaryType
        {
            OuterBoundary,
            PlayArea
        }

        /// <summary>
        /// Result of boundary test API
        /// </summary>
        public struct BoundaryTestResult
        {
            public bool IsTriggering;
            public float ClosestDistance;
            public Vector3 ClosestPoint;
            public Vector3 ClosestPointNormal;
        }

        /// <summary>
        /// Boundary Test Node Type
        /// </summary>
        public enum BoundaryTrackingNode
        {
            HandLeft    = 0,
            HandRight   = 1,
            Head        = 2
        }

        public static float UPvr_GetFloorHeight()
        {
            float floorHeight = 0;
#if ANDROID_DEVICE
            floorHeight = Pvr_GetFloorHeight();
#endif
            return floorHeight;
        }

        /// <summary>
        /// 0 - no seethrough
        /// 1 - gradient seethrough
        /// 2 - total seethrough
        /// </summary>
        /// <returns></returns>
        public static int UPvr_GetSeeThroughState()
        {
            int state = 0;
#if ANDROID_DEVICE
            state = Pvr_GetSeeThroughState();
#endif
            return state;
        }

        public static bool UPvr_GetFrameRateLimit()
        {
            bool ret = false;
#if ANDROID_DEVICE
            ret = Pvr_GetFrameRateLimit();
#endif
            return ret;
        }

        public static bool UPvr_IsBoundaryEnable()
        {
            bool state = false;
#if ANDROID_DEVICE
            state = Pvr_IsBoundaryEnable();
#endif
            return state;
        }

        /// <summary>
        /// Returns true if the boundary system is currently configured with valid boundary data.
        /// </summary>
        /// <returns></returns>
        public static bool UPvr_BoundaryGetConfigured()
        {
            bool ret = false;
#if ANDROID_DEVICE
            ret = Pvr_BoundaryGetConfigured();
#endif
            return ret;
        }

        /// <summary>
        ///  Returns the result of testing a tracked node against the specified boundary type
        /// </summary>
        /// <param name="node"></param>
        /// <param name="boundaryType"></param>
        /// <returns></returns>
        public static BoundaryTestResult UPvr_BoundaryTestNode(BoundaryTrackingNode node, BoundaryType boundaryType)
        {
            BoundaryTestResult testResult = new BoundaryTestResult();
#if ANDROID_DEVICE
            bool ret = Pvr_BoundaryTestNode((int)node, boundaryType == BoundaryType.PlayArea, ref testResult.IsTriggering, ref testResult.ClosestDistance,
                ref testResult.ClosestPoint.x, ref testResult.ClosestPoint.y, ref testResult.ClosestPoint.z,
                ref testResult.ClosestPointNormal.x, ref testResult.ClosestPointNormal.y, ref testResult.ClosestPointNormal.z);

            testResult.ClosestPoint.z = -testResult.ClosestPoint.z;
            testResult.ClosestPointNormal.z = -testResult.ClosestPointNormal.z;
            if (!ret)
            {
                Debug.LogError(string.Format("UPvr_BoundaryTestNode({0}, {1}) API call failed!", node, boundaryType));
            }
#endif
            return testResult;
        }

        /// <summary>
        /// Returns the result of testing a 3d point against the specified boundary type.
        /// </summary>
        /// <param name="point">the coordinate of the point</param>
        /// <param name="boundaryType">OuterBoundary or PlayArea</param>
        /// <returns></returns>
        public static BoundaryTestResult UPvr_BoundaryTestPoint(Vector3 point, BoundaryType boundaryType)
        {
            BoundaryTestResult testResult = new BoundaryTestResult();
#if ANDROID_DEVICE
            bool ret = Pvr_BoundaryTestPoint(point.x, point.y, -point.z, boundaryType == BoundaryType.PlayArea, ref testResult.IsTriggering, ref testResult.ClosestDistance, 
                ref testResult.ClosestPoint.x, ref testResult.ClosestPoint.y, ref testResult.ClosestPoint.z, 
                ref testResult.ClosestPointNormal.x, ref testResult.ClosestPointNormal.y, ref testResult.ClosestPointNormal.z);

            if (!ret)
            {
                Debug.LogError(string.Format("UPvr_BoundaryTestPoint({0}, {1}) API call failed!", point, boundaryType));
            }
#endif
            return testResult;
        }

        /// <summary>
        /// Return the boundary geometry points
        /// </summary>
        /// <param name="boundaryType">OuterBoundary or PlayArea</param>
        /// <returns>Boundary geometry point-collection num</returns>
        public static Vector3[] UPvr_BoundaryGetGeometry(BoundaryType boundaryType)
        {
#if ANDROID_DEVICE
            IntPtr pointHandle = IntPtr.Zero;           
            int pointsCount = Pvr_BoundaryGetGeometry(out pointHandle, boundaryType == BoundaryType.PlayArea);
            if (pointsCount <= 0)
            {
                Debug.LogError("Boundary geometry point count is " + pointsCount);
                return null;
            }
            // managed buffer
            int pointBufferSize = pointsCount * 3;
            float[] pointsBuffer = new float[pointBufferSize];
            Marshal.Copy(pointHandle, pointsBuffer, 0, pointBufferSize);

            Vector3[] points = new Vector3[pointsCount];
            for (int i = 0; i < pointsCount; i++)
            {
                points[i] = new Vector3()
                {
                    x = pointsBuffer[3 * i + 0],
                    y = pointsBuffer[3 * i + 1],
                    z = -pointsBuffer[3 * i + 2],
                };
            }

            return points;
#endif
            return null;
        }

        /// <summary>
        /// Returns true if the boundary system is currently enabled.
        /// </summary>
        /// <returns></returns>
        public static bool UPvr_BoundaryGetEnabled()
        {
            bool ret = false;
#if ANDROID_DEVICE
            ret = Pvr_BoundaryGetEnabled();
#endif
            return ret;
        }

        /// <summary>
        /// Set boundary system visibility to be the specified value.
        /// Note:
        ///     The actual visibility of boundary can be overridden by the system(e.g. proximity trigger) or user setting(e.g. disable boundary system).
        /// </summary>
        /// <param name="value"></param>
        public static void UPvr_BoundarySetVisible(bool value)
        {
#if ANDROID_DEVICE
            Pvr_BoundarySetVisible(value);
#endif
        }




        public static bool UPvr_EnableLWRP(bool enable)
        {
#if ANDROID_DEVICE
            return Pvr_EnableLWRP(enable);
#endif
            return false;
        }

        public static bool UPvr_SetViewportSize(int width, int height)
        {
#if ANDROID_DEVICE
            return Pvr_SetViewportSize(width, height);
#endif
            return false;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PlatformSettings
    {
        public static Pvr_UnitySDKPlatformSetting.simulationType UPvr_IsCurrentDeviceValid()
        {
            if (Pvr_UnitySDKPlatformSetting.Entitlementchecksimulation)
            {
                if (Pvr_UnitySDKPlatformSetting.Instance.deviceSN.Count <= 0)
                {
                    return Pvr_UnitySDKPlatformSetting.simulationType.Null;
                }
                else
                {
                    foreach (var t in Pvr_UnitySDKPlatformSetting.Instance.deviceSN)
                    {
                        if (System.UPvr_GetDeviceSN() == t)
                        {
                            return Pvr_UnitySDKPlatformSetting.simulationType.Valid;
                        }
                    }

                    return Pvr_UnitySDKPlatformSetting.simulationType.Invalid;
                }
            }
            else
            {
                return Pvr_UnitySDKPlatformSetting.simulationType.Invalid;
            }
        }

        public static bool UPvr_AppEntitlementCheck(string appid)
        {
            bool state = false;
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref state, Pvr_UnitySDKRender.javaVrActivityClass, "verifyAPP", System.UPvr_GetCurrentActivity(), appid,"");
            }
            catch (Exception e)
            {
                PLOG.E("Error :" + e.ToString());
            }
#endif
            Debug.Log("PvrLog UPvr_AppEntitlementCheck" + state);
            return state;
        }

        public static bool UPvr_KeyEntitlementCheck(string publicKey)
        {
            bool state = false;
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref state, Pvr_UnitySDKRender.javaVrActivityClass,"verifyAPP", System.UPvr_GetCurrentActivity(),"",publicKey);
            }
            catch (Exception e)
            {
                PLOG.E("Error :" + e.ToString());
            }
#endif
            Debug.Log("PvrLog UPvr_KeyEntitlementCheck" + state);
            return state;
        }

        //0:success -1:invalid params -2:service not exist -3:time out
        public static int UPvr_AppEntitlementCheckExtra(string appid)
        {
            int state = -1;
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref state, Pvr_UnitySDKRender.javaVrActivityClass, "verifyAPPExt", System.UPvr_GetCurrentActivity(), appid,"");
            }
            catch (Exception e)
            {
                PLOG.E("Error :" + e.ToString());
            }
#endif
            Debug.Log("PvrLog UPvr_AppEntitlementCheck" + state);
            return state;
        }

        //0:success -1:invalid params -2:service not exist -3:time out
        public static int UPvr_KeyEntitlementCheckExtra(string publicKey)
        {
            int state = -1;
#if ANDROID_DEVICE
            try
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref state, Pvr_UnitySDKRender.javaVrActivityClass,"verifyAPPExt", System.UPvr_GetCurrentActivity(),"",publicKey);
            }
            catch (Exception e)
            {
                PLOG.E("Error :" + e.ToString());
            }
#endif
            Debug.Log("PvrLog UPvr_KeyEntitlementCheck" + state);
            return state;
        }
    }
}