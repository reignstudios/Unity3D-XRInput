// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID
#define ANDROID_DEVICE
#endif

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Pvr_UnitySDKAPI
{
    #region Properties
    public class PvrControllerKey
    {
        public bool State;
        public bool PressedDown;
        public bool PressedUp;
        public bool LongPressed;
        public bool Click;
        public PvrControllerKey()
        {
            State = false;
            PressedDown = false;
            PressedUp = false;
            LongPressed = false;
            Click = false;
        }
    }

    public class ControllerHand
    {
        public PvrControllerKey App;
        public PvrControllerKey Touch;
        public PvrControllerKey Home;
        public PvrControllerKey VolumeDown;
        public PvrControllerKey VolumeUp;
        public PvrControllerKey Trigger;
        public PvrControllerKey X;
        public PvrControllerKey Y;
        public PvrControllerKey A;
        public PvrControllerKey B;
        public PvrControllerKey Left;
        public PvrControllerKey Right;
        public Vector2 TouchPadPosition;
        public int TriggerNum;
        public Quaternion Rotation;
        public Vector3 Position;
        public int Battery;
        public ControllerState ConnectState;
        public SwipeDirection SwipeDirection;
        public TouchPadClick TouchPadClick;

        public ControllerHand()
        {
            App = new PvrControllerKey();
            Touch = new PvrControllerKey();
            Home = new PvrControllerKey();
            VolumeDown = new PvrControllerKey();
            VolumeUp = new PvrControllerKey();
            Trigger = new PvrControllerKey();
            A = new PvrControllerKey();
            B = new PvrControllerKey();
            X = new PvrControllerKey();
            Y = new PvrControllerKey();
            Left = new PvrControllerKey();
            Right = new PvrControllerKey();
            TouchPadPosition = new Vector2();
            Rotation = new Quaternion();
            Position = new Vector3();
            Battery = 0;
            TriggerNum = 0;
            ConnectState = ControllerState.Error;
            SwipeDirection = SwipeDirection.No;
            TouchPadClick = TouchPadClick.No;
        }
    }

    public enum ControllerState
    {
        Error = -1,
        DisConnected = 0,
        Connected = 1
    }

    /// <summary>
    /// controller key value
    /// </summary>
    public enum Pvr_KeyCode
    {
        APP = 1,
        TOUCHPAD = 2,
        HOME = 3,
        VOLUMEUP = 4,
        VOLUMEDOWN = 5,
        TRIGGER = 6,
        A = 7,
        B = 8,
        X = 9,
        Y = 10,
        Left = 11,
        Right = 12
    }

    /// <summary>
    /// The controller Touchpad slides in the direction.
    /// </summary>
    public enum SwipeDirection
    {
        No = 0,
        SwipeUp = 1,
        SwipeDown = 2,
        SwipeLeft = 3,
        SwipeRight = 4
    }

    /// <summary>
    /// The controller Touchpad click the direction.
    /// </summary>
    public enum TouchPadClick
    {
        No = 0,
        ClickUp = 1,
        ClickDown = 2,
        ClickLeft = 3,
        ClickRight = 4
    }

    #endregion
    public struct Controller
    {
        /**************************** Public Static Funcations *******************************************/
        #region Public Static Funcation  

        /// <summary>
        /// Get the touch pad position data.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static Vector2 UPvr_GetTouchPadPosition(int hand)
        {
            switch (hand)
            {
                case 0:
                    {
                        var postion = Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition;
                        return postion;
                    }
                case 1:
                    {
                        var postion = Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition;
                        return postion;
                    }
            }
            return new Vector2(0, 0);
        }

        /// <summary>
        /// convert coordinate system
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <returns>horizontal :X -1~1 vertical: Y -1~1</returns>
        public static Vector2 UPvr_GetAxis2D(int hand)
        {
            switch (hand)
            {
                case 0:
                    {
                        if (Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition != Vector2.zero)
                        {
                            var postion = new Vector2(Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.y / 128.0f - 1,
                                Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.x / 128.0f - 1);
                            return postion;
                        }

                        return Vector2.zero;

                    }
                case 1:
                    {
                        if (Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition != Vector2.zero)
                        {
                            var postion = new Vector2(Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.y / 128.0f - 1,
                                Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.x / 128.0f - 1);
                            return postion;
                        }

                        return Vector2.zero;
                    }
            }
            return Vector2.zero;
        }

        public static ControllerState UPvr_GetControllerState(int hand)
        {
            switch (hand)
            {
                case 0:
                    Pvr_ControllerManager.controllerlink.Controller0.ConnectState = Pvr_ControllerManager.GetControllerConnectionState(0) == 1 ? ControllerState.Connected : ControllerState.DisConnected;
                    return Pvr_ControllerManager.controllerlink.Controller0.ConnectState;
                case 1:
                    if (Pvr_ControllerManager.controllerlink.neoserviceStarted)
                    {
                        Pvr_ControllerManager.controllerlink.Controller1.ConnectState = Pvr_ControllerManager.GetControllerConnectionState(1) == 1 ? ControllerState.Connected : ControllerState.DisConnected;
                    }
                    return Pvr_ControllerManager.controllerlink.Controller1.ConnectState;

            }
            return ControllerState.Error;
        }

        /// <summary>
        /// Get the controller rotation data.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static Quaternion UPvr_GetControllerQUA(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.Rotation;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.Rotation;
            }
            return new Quaternion(0, 0, 0, 1);
        }

        /// <summary>
        /// Get the controller position data.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static Vector3 UPvr_GetControllerPOS(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.Position;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.Position;
            }
            return new Vector3(0, 0, 0);
        }

        /// <summary>
        /// Get the value of the trigger key 
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <returns>Neo/Neo2:0-255,G2:0/1</returns>
        public static int UPvr_GetControllerTriggerValue(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.TriggerNum;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.TriggerNum;
            }
            return 0;
        }

        /// <summary>
        /// Get the power of the controller, neo power is 1-10, goblin/goblin2 power is 1-4.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static int UPvr_GetControllerPower(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.Battery;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.Battery;
            }
            return 0;
        }

        /// <summary>
        /// Get the sliding direction of the touchpad.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static SwipeDirection UPvr_GetSwipeDirection(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.SwipeDirection;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.SwipeDirection;
            }
            return SwipeDirection.No;
        }


        /// <summary>
        /// Get the click direction of the touchpad.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static TouchPadClick UPvr_GetTouchPadClick(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.TouchPadClick;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.TouchPadClick;
            }
            return TouchPadClick.No;
        }

        /// <summary>
        /// Get the key state
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_GetKey(int hand, Pvr_KeyCode key)
        {
            if (hand == 0)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller0.App.State;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller0.Home.State;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller0.Touch.State;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeUp.State;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeDown.State;
                    case Pvr_KeyCode.TRIGGER:
                        return Pvr_ControllerManager.controllerlink.Controller0.Trigger.State;
                    case Pvr_KeyCode.X:
                        return Pvr_ControllerManager.controllerlink.Controller0.X.State;
                    case Pvr_KeyCode.Y:
                        return Pvr_ControllerManager.controllerlink.Controller0.Y.State;
                    case Pvr_KeyCode.Left:
                        return Pvr_ControllerManager.controllerlink.Controller0.Left.State;
                    default:
                        return false;
                }
            }
            if (hand == 1)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller1.App.State;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller1.Home.State;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller1.Touch.State;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeUp.State;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeDown.State;
                    case Pvr_KeyCode.TRIGGER:
                        return Pvr_ControllerManager.controllerlink.Controller1.Trigger.State;
                    case Pvr_KeyCode.A:
                        return Pvr_ControllerManager.controllerlink.Controller1.A.State;
                    case Pvr_KeyCode.B:
                        return Pvr_ControllerManager.controllerlink.Controller1.B.State;
                    case Pvr_KeyCode.Right:
                        return Pvr_ControllerManager.controllerlink.Controller1.Right.State;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the pressed state of the key
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_GetKeyDown(int hand, Pvr_KeyCode key)
        {
            if (hand == 0)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller0.App.PressedDown;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller0.Home.PressedDown;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller0.Touch.PressedDown;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeUp.PressedDown;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeDown.PressedDown;
                    case Pvr_KeyCode.TRIGGER:
                        return Pvr_ControllerManager.controllerlink.Controller0.Trigger.PressedDown;
                    case Pvr_KeyCode.X:
                        return Pvr_ControllerManager.controllerlink.Controller0.X.PressedDown;
                    case Pvr_KeyCode.Y:
                        return Pvr_ControllerManager.controllerlink.Controller0.Y.PressedDown;
                    case Pvr_KeyCode.Left:
                        return Pvr_ControllerManager.controllerlink.Controller0.Left.PressedDown;
                    default:
                        return false;
                }
            }
            if (hand == 1)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller1.App.PressedDown;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller1.Home.PressedDown;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller1.Touch.PressedDown;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeUp.PressedDown;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeDown.PressedDown;
                    case Pvr_KeyCode.TRIGGER:
                        return Pvr_ControllerManager.controllerlink.Controller1.Trigger.PressedDown;
                    case Pvr_KeyCode.A:
                        return Pvr_ControllerManager.controllerlink.Controller1.A.PressedDown;
                    case Pvr_KeyCode.B:
                        return Pvr_ControllerManager.controllerlink.Controller1.B.PressedDown;
                    case Pvr_KeyCode.Right:
                        return Pvr_ControllerManager.controllerlink.Controller1.Right.PressedDown;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the lift state of the key.
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_GetKeyUp(int hand, Pvr_KeyCode key)
        {
            if (hand == 0)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller0.App.PressedUp;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller0.Home.PressedUp;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller0.Touch.PressedUp;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeUp.PressedUp;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeDown.PressedUp;
                    case Pvr_KeyCode.TRIGGER:
                        return Pvr_ControllerManager.controllerlink.Controller0.Trigger.PressedUp;
                    case Pvr_KeyCode.X:
                        return Pvr_ControllerManager.controllerlink.Controller0.X.PressedUp;
                    case Pvr_KeyCode.Y:
                        return Pvr_ControllerManager.controllerlink.Controller0.Y.PressedUp;
                    case Pvr_KeyCode.Left:
                        return Pvr_ControllerManager.controllerlink.Controller0.Left.PressedUp;
                    default:
                        return false;
                }
            }
            if (hand == 1)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller1.App.PressedUp;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller1.Home.PressedUp;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller1.Touch.PressedUp;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeUp.PressedUp;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeDown.PressedUp;
                    case Pvr_KeyCode.TRIGGER:
                        return Pvr_ControllerManager.controllerlink.Controller1.Trigger.PressedUp;
                    case Pvr_KeyCode.A:
                        return Pvr_ControllerManager.controllerlink.Controller1.A.PressedUp;
                    case Pvr_KeyCode.B:
                        return Pvr_ControllerManager.controllerlink.Controller1.B.PressedUp;
                    case Pvr_KeyCode.Right:
                        return Pvr_ControllerManager.controllerlink.Controller1.Right.PressedUp;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the click state of the Key.
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_GetKeyClick(int hand, Pvr_KeyCode key)
        {
            if (hand == 0)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller0.App.Click;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller0.Home.Click;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller0.Touch.Click;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeUp.Click;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeDown.Click;
                    case Pvr_KeyCode.TRIGGER:
                        return Pvr_ControllerManager.controllerlink.Controller0.Trigger.Click;
                    case Pvr_KeyCode.X:
                        return Pvr_ControllerManager.controllerlink.Controller0.X.Click;
                    case Pvr_KeyCode.Y:
                        return Pvr_ControllerManager.controllerlink.Controller0.Y.Click;
                    case Pvr_KeyCode.Left:
                        return Pvr_ControllerManager.controllerlink.Controller0.Left.Click;
                    default:
                        return false;
                }
            }
            if (hand == 1)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller1.App.Click;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller1.Home.Click;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller1.Touch.Click;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeUp.Click;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeDown.Click;
                    case Pvr_KeyCode.TRIGGER:
                        return Pvr_ControllerManager.controllerlink.Controller1.Trigger.Click;
                    case Pvr_KeyCode.A:
                        return Pvr_ControllerManager.controllerlink.Controller1.A.Click;
                    case Pvr_KeyCode.B:
                        return Pvr_ControllerManager.controllerlink.Controller1.B.Click;
                    case Pvr_KeyCode.Right:
                        return Pvr_ControllerManager.controllerlink.Controller1.Right.Click;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the long press state of the Key.
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_GetKeyLongPressed(int hand, Pvr_KeyCode key)
        {
            if (hand == 0)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller0.App.LongPressed;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller0.Home.LongPressed;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller0.Touch.LongPressed;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeUp.LongPressed;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeDown.LongPressed;
                    case Pvr_KeyCode.TRIGGER:
                        return Pvr_ControllerManager.controllerlink.Controller0.Trigger.LongPressed;
                    case Pvr_KeyCode.X:
                        return Pvr_ControllerManager.controllerlink.Controller0.X.LongPressed;
                    case Pvr_KeyCode.Y:
                        return Pvr_ControllerManager.controllerlink.Controller0.Y.LongPressed;
                    case Pvr_KeyCode.Left:
                        return Pvr_ControllerManager.controllerlink.Controller0.Left.LongPressed;
                    default:
                        return false;
                }
            }
            if (hand == 1)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller1.App.LongPressed;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller1.Home.LongPressed;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller1.Touch.LongPressed;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeUp.LongPressed;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeDown.LongPressed;
                    case Pvr_KeyCode.TRIGGER:
                        return Pvr_ControllerManager.controllerlink.Controller1.Trigger.LongPressed;
                    case Pvr_KeyCode.A:
                        return Pvr_ControllerManager.controllerlink.Controller1.A.LongPressed;
                    case Pvr_KeyCode.B:
                        return Pvr_ControllerManager.controllerlink.Controller1.B.LongPressed;
                    case Pvr_KeyCode.Right:
                        return Pvr_ControllerManager.controllerlink.Controller1.Right.LongPressed;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Determine if you touched the touchpad.
        /// </summary>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_IsTouching(int hand)
        {
            const float tolerance = 0;
            switch (hand)
            {
                case 0:
                    {
                        return Math.Abs(Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.x) > tolerance ||
                               Math.Abs(Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.y) > tolerance;
                    }
                case 1:
                    {
                        return Math.Abs(Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.x) > tolerance ||
                               Math.Abs(Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.y) > tolerance;
                    }
            }
            return false;
        }

        /// <summary>
        /// Set the handness.
        /// </summary>
        /// <param name="hand">UserHandNess</param>
        public static void UPvr_SetHandNess(Pvr_Controller.UserHandNess hand)
        {
            if (Pvr_ControllerManager.controllerlink.getHandness() != (int)hand)
            {
                Pvr_ControllerManager.controllerlink.setHandness((int)hand);
            }
        }

        /// <summary>
        /// Get the handness.
        /// </summary>
        public static Pvr_Controller.UserHandNess UPvr_GetHandNess()
        {
            return (Pvr_Controller.UserHandNess)Pvr_ControllerManager.controllerlink.getHandness();
        }

        /// <summary>
        /// The service type that currently needs bind.
        /// </summary>
        /// <returns>1：Goblin service 2:Neo service </returns>
        public static int UPvr_GetPreferenceDevice()
        {
            var trackingmode = Pvr_ControllerManager.controllerlink.trackingmode;
            var systemproc = Pvr_ControllerManager.controllerlink.systemProp;
            if (trackingmode == 0 || trackingmode == 1 || (trackingmode == 3 || trackingmode == 5) && (systemproc == 1 || systemproc == 3))
            {
                return 1;
            }
            return 2;
        }

        /// <summary>
        /// Whether the current controller has trigger key
        /// </summary>
        public static bool UPvr_IsEnbleTrigger()
        {
            return Pvr_ControllerManager.controllerlink.IsEnbleTrigger();
        }

        /// <summary>
        ///Gets the controller type of the current connection.
        /// </summary>
        /// <returns>0: no connection 1：goblin1 2:Neo 3:goblin2 4:Neo2</returns>
        public static int UPvr_GetDeviceType()
        {
            return Pvr_ControllerManager.controllerlink.GetDeviceType();
        }

        /// <summary>
        /// Gets the current master hand for which 0/1.
        /// </summary>
        /// <returns></returns>
        public static int UPvr_GetMainHandNess()
        {
            return Pvr_ControllerManager.controllerlink.GetMainControllerIndex();
        }

        /// <summary>
        /// Set the current controller as the master controller.
        /// </summary>
        public static void UPvr_SetMainHandNess(int hand)
        {
            Pvr_ControllerManager.controllerlink.SetMainController(hand);
        }

        /// <summary>
        /// Ability to obtain the current controller (3dof/6dof)
        /// </summary>
        /// <param name="hand">0/1</param>
        /// <returns>-1:error 0：6dof  1：3dof 2:6dof </returns>
        public static int UPvr_GetControllerAbility(int hand)
        {
            return Pvr_ControllerManager.controllerlink.GetControllerAbility(hand);
        }

        /// <summary>
        /// Vibrate Neo2 controller 
        /// </summary>
        /// <param name="strength">0-1</param>
        /// <param name="time">ms,0-65535</param>
        /// <param name="hand">0,1</param>
        public static void UPvr_VibrateNeo2Controller(float strength, int time, int hand)
        {
            Pvr_ControllerManager.controllerlink.VibrateNeo2Controller(strength, time, hand);
        }

        /// <summary>
        /// get controller binding state
        /// </summary>
        /// <param name="id">0,1</param>
        /// <returns>-1:error 0:Unbound 1:bind</returns>
        public static int UPvr_GetControllerBindingState(int id)
        {
            return Pvr_ControllerManager.controllerlink.GetControllerBindingState(id);
        }

        /// <summary>
        /// Get the controller Velocity, Obtain the controller's pose data.
        /// unit:mm/s
        /// </summary>
        public static Vector3 UPvr_GetVelocity(int hand)
        {
            return Pvr_ControllerManager.controllerlink.GetVelocity(hand);
        }

        /// <summary>
        /// Get the controller AngularVelocity, Obtain the controller's gyroscope data.
        /// unit:rad/s
        /// </summary>
        public static Vector3 UPvr_GetAngularVelocity(int num)
        {
            Vector3 Aglr = new Vector3(0.0f, 0.0f, 0.0f);
#if ANDROID_DEVICE
            Aglr = Pvr_ControllerManager.Instance.GetAngularVelocity(num);
#endif
            return Aglr;
        }

        /// <summary>
        /// Get the controller Acceleration.
        /// mm/s^2
        /// </summary>
        public static Vector3 UPvr_GetAcceleration(int num)
        {
            Vector3 Acc = new Vector3(0.0f, 0.0f, 0.0f);
#if ANDROID_DEVICE
            Acc = Pvr_ControllerManager.Instance.GetAcceleration(num);
#endif
            return Acc;
        }

        /// <summary>
        /// scan Goblin,G2 controller
        /// </summary>
        public static void UPvr_ScanController()
        {
            Pvr_ControllerManager.controllerlink.StartScan();
        }

        /// <summary>
        /// Connect controller by mac address.
        /// only fit goblin,g2
        /// </summary>
        /// <param name="mac">mac address of controller</param>
        public static void UPvr_ConnectController(string mac)
        {
            if (mac != "")
            {
                Pvr_ControllerManager.controllerlink.hummingBirdMac = mac;
            }
            Pvr_ControllerManager.controllerlink.ConnectBLE();
        }

        /// <summary>
        /// get controller version
        /// </summary>
        public static string UPvr_GetControllerVersion()
        {
            return Pvr_ControllerManager.controllerlink.GetControllerVersion();
        }

        /// <summary>
        /// Get version number deviceType
        /// </summary>
        /// <param name="deviceType">0-station 1-controller0  2-controller1</param>
        public static void UPvr_GetDeviceVersion(int deviceType)
        {
            Pvr_ControllerManager.controllerlink.GetDeviceVersion(deviceType);
        }

        /// <summary>
        /// Get the controller Sn number controllerSerialNum
        /// </summary>
        /// <param name="controllerSerialNum">0-controller0  1-controller1</param>
        public static void UPvr_GetControllerSnCode(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.GetControllerSnCode(controllerSerialNum);
        }

        /// <summary>
        /// neo:Unbind the controller: 0- controller 0 1- controller 1
        /// neo2:Unbind the controller: 0- all controller 1- left controller 2- right controller
        /// </summary>
        public static void UPvr_SetControllerUnbind(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.SetControllerUnbind(controllerSerialNum);
        }

        /// <summary>
        /// Restart the station
        /// </summary>
        public static void UPvr_SetStationRestart()
        {
            Pvr_ControllerManager.controllerlink.SetStationRestart();
        }

        /// <summary>
        /// Launch station OTA upgrade.
        /// </summary>
        public static void UPvr_StartStationOtaUpdate()
        {
            Pvr_ControllerManager.controllerlink.StartStationOtaUpdate();
        }

        /// <summary>
        /// Launch controller ota upgrade mode: 1-rf upgrade communication module 2- upgrade STM32 module;ControllerSerialNum: 0- controller 0 1- controller 1.
        /// </summary>
        public static void UPvr_StartControllerOtaUpdate(int mode, int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.StartControllerOtaUpdate(mode, controllerSerialNum);
        }

        /// <summary>
        /// Enter the pairing mode controllerSerialNum: 0- controller 0 1- controller 1.
        /// </summary>
        public static void UPvr_EnterPairMode(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.EnterPairMode(controllerSerialNum);
        }

        /// <summary>
        /// controller shutdown controllerSerialNum: 0- controller 0 1- controller 1.
        /// </summary>
        public static void UPvr_SetControllerShutdown(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.SetControllerShutdown(controllerSerialNum);
        }

        /// <summary>
        /// Retrieves the pairing status of the current station with 0- unpaired state 1- pairing.
        /// </summary>
        public static int UPvr_GetStationPairState()
        {
            return Pvr_ControllerManager.controllerlink.GetStationPairState();
        }

        /// <summary>
        /// Get the upgrade of station ota
        /// </summary>
        public static int UPvr_GetStationOtaUpdateProgress()
        {
            return Pvr_ControllerManager.controllerlink.GetStationOtaUpdateProgress();
        }

        /// <summary>
        /// Get the Controller ota upgrade progress
        /// Normal 0-100
        /// Exception 101: failed to receive a successful upgrade of id 102: the controller did not enter the upgrade status 103: upgrade interrupt exception
        /// </summary>
        public static int UPvr_GetControllerOtaUpdateProgress()
        {
            return Pvr_ControllerManager.controllerlink.GetControllerOtaUpdateProgress();
        }

        /// <summary>
        /// Also get the controller version number and SN number controllerSerialNum: 0- controller 0 1- controller 1
        /// </summary>
        public static void UPvr_GetControllerVersionAndSN(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.GetControllerVersionAndSN(controllerSerialNum);
        }

        /// <summary>
        /// Gets the unique identifier of the controller
        /// </summary>
        public static void UPvr_GetControllerUniqueID()
        {
            Pvr_ControllerManager.controllerlink.GetControllerUniqueID();
        }

        /// <summary>
        /// Disconnect the station from the current pairing mode
        /// </summary>
        public void UPvr_InterruptStationPairMode()
        {
            Pvr_ControllerManager.controllerlink.InterruptStationPairMode();
        }

        /// <summary>
        /// deviceType: 0：scan both controller；1：scan left controller；2：scan right controller
        /// </summary>
        public void UPvr_StartCV2PairingMode(int deviceType)
        {
            Pvr_ControllerManager.controllerlink.StartCV2PairingMode(deviceType);
        }

        /// <summary>
        /// deviceType: 0：stop scan both controller；1：stop scan left controller；2：stop scan right controller
        /// </summary>
        public void UPvr_StopCV2PairingMode(int deviceType)
        {
            Pvr_ControllerManager.controllerlink.StopCV2PairingMode(deviceType);
        }

        public static void UPvr_SetArmModelParameters(int hand, int gazeType, float elbowHeight, float elbowDepth, float pointerTiltAngle)
        {
#if ANDROID_DEVICE
            Pvr_SetArmModelParameters( hand,  gazeType,  elbowHeight,  elbowDepth,  pointerTiltAngle);
#endif
        }

        public static void UPvr_CalcArmModelParameters(float[] headOrientation, float[] controllerOrientation, float[] controllerPrimary)
        {
#if ANDROID_DEVICE
            Pvr_CalcArmModelParameters( headOrientation,  controllerOrientation, controllerPrimary);
#endif
        }

        public static void UPvr_GetPointerPose(float[] rotation, float[] position)
        {
#if ANDROID_DEVICE
            Pvr_GetPointerPose(  rotation,  position);
#endif
        }

        public static void UPvr_GetElbowPose(float[] rotation, float[] position)
        {
#if ANDROID_DEVICE
            Pvr_GetElbowPose(  rotation,   position);
#endif
        }

        public static void UPvr_GetWristPose(float[] rotation, float[] position)
        {
#if ANDROID_DEVICE
            Pvr_GetWristPose(  rotation,  position);
#endif
        }

        public static void UPvr_GetShoulderPose(float[] rotation, float[] position)
        {
#if ANDROID_DEVICE
            Pvr_GetShoulderPose(  rotation,   position);
#endif
        }
        //Whether key injection
        //true:open injection
        //false:close injection,Unity can get the key value
        public static void UPvr_IsEnbleHomeKey(bool state)
        {
            Pvr_ControllerManager.controllerlink.setIsEnbleHomeKey(state);
        }
        //whether use default home key
        //true:Use the default home key function,Developers cannot operate on the home key
        //false:Developers can operate on the home key
        public static void UPvr_SwitchHomeKey(bool state)
        {
            Pvr_ControllerManager.controllerlink.SwitchHomeKey(state);
        }

        /// <summary>
        /// Determine whether the current controller data is valid
        /// </summary>
        /// <returns>1:valid 0: unvalid -1:fail</returns>
        public static int UPvr_GetControllerSensorStatus(int id)
        {
            return Pvr_ControllerManager.controllerlink.getControllerSensorStatus(id);
        }
        /**************************** Private Static Funcations *******************************************/
        #region Private Static Funcation
#if ANDROID_DEVICE
        public const string LibFileName = "Pvr_UnitySDK";
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_CalcArmModelParameters(float[] headOrientation,float[] controllerOrientation,float[] gyro);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_GetPointerPose( float[] rotation,  float[] position);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_GetElbowPose( float[] rotation,  float[] position);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_GetWristPose( float[] rotation,  float[] position);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_GetShoulderPose( float[] rotation,  float[] position);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_SetArmModelParameters(int hand, int gazeType, float elbowHeight, float elbowDepth, float pointerTiltAngle);
#endif
        #endregion
        #endregion
    }

}
