using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Reign.XR.API;

namespace Reign.XR
{
    public enum XRInputLoaderType
    {
        Unknown,
        OpenXR,
        OculusXR,
        OpenVR,
        PicoXR,
        PSVR2
    }

    public enum XRInputAPIType
    {
        AutoDetect,
        UnityEngine_XR,
        OculusXR,
        OpenVR,
        OpenVR_Legacy,
        Pico2VR
    }

    [Serializable]
    public struct XRInputRigSet
    {
        public XRInputLoaderType loaderType;
        public XRInputAPIType apiType;
        public GameObject[] enableObjects, disableObjects;
    }

    [DefaultExecutionOrder(-32000)]
    public sealed class XRInput : MonoBehaviour
    {
        public static XRInput singleton { get; private set; }
        public static XRInputLoaderType loaderType { get; private set; }
        public static string loaderTypeName { get; private set; }

        public delegate void InitializedMethod(bool success);
        public static event InitializedMethod InitializedCallback;

        public delegate void DisposedMethod();
        public static event DisposedMethod DisposedCallback;

        public delegate void ControllerConstructionMethod(Guid id, XRControllerSide side, XRInputControllerType type);
        public static event ControllerConstructionMethod ControllerConnectedCallback, ControllerDisconnectedMethod;

        public bool autoSetRumbleChannel = true;
        public uint rumbleChannel;
        public bool steamSDK_InUse;

        public XRLoader loaderOverride;
        public XRInputAPIType apiType;
        private XRInputAPI api;
        private bool disposeAPI, apiInit;

        public bool stopLoaderOnAppExit = true;
        private XRLoader loader;
        public bool enableLogging = true;

        private const int controllerStateLength = 4;
        private int lastControllerCount;
        private XRControllerState[] state_controllers = new XRControllerState[controllerStateLength], state_controllers_last = new XRControllerState[controllerStateLength];
        private XRControllerState state_controllerLeft, state_controllerRight;
        private XRControllerState state_controllerFirst, state_controllerMerged;
        private Guid state_controllerMerged_ID = Guid.NewGuid();

        public XRInputRigSet[] rigSets;

        internal static void Log(string message)
		{
            if (singleton != null && singleton.enableLogging) Debug.Log(message);
		}

        private IEnumerator Start()
        {
            // only one can exist in scene at a time
            if (singleton != null)
            {
                disposeAPI = false;// don't dispose apis if we're not the owner of possible native instances
                Destroy(gameObject);
                yield break;
		    }
            DontDestroyOnLoad(gameObject);
            singleton = this;

            // print version
            XRInput.Log("XRInput version: 1.5.0");

            // check if loaders are used
            bool useLoader = apiType != XRInputAPIType.Pico2VR;
            Log("XRInput Using Loader: " + useLoader.ToString());

            // wait for XR loader
            if (useLoader)
            {
                while (loader == null || !XRSettings.enabled)
                {
                    try
                    {
                        if (loaderOverride != null) loader = loaderOverride;
                        else loader = XRGeneralSettings.Instance.Manager.activeLoader;
                    }
                    catch { }
                    yield return new WaitForSeconds(1);
                }
            }

            // give XR some time to start
            var wait = new WaitForEndOfFrame();
            yield return wait;
            yield return wait;
            yield return wait;
            yield return wait;
            yield return wait;
            yield return new WaitForSeconds(1);

            try
            {
                if (useLoader)
                {
                    // get loader type
                    var loaderRuntimeType = loader.GetType();
                    loaderTypeName = loaderRuntimeType.Name;
                    XRInput.Log($"XR-Loader: '{loader.name}' TYPE:{loaderRuntimeType}");

                    // auto set rumble channel
                    if (autoSetRumbleChannel)
                    {
                        rumbleChannel = 0;
                    }

                    // loader type
                    switch (loaderTypeName)
                    {
                        case "OpenXRLoader": loaderType = XRInputLoaderType.OpenXR; break;
                        case "OculusLoader": loaderType = XRInputLoaderType.OculusXR; break;
                        case "OpenVRLoader": loaderType = XRInputLoaderType.OpenVR; break;
                        case "PXR Loader": loaderType = XRInputLoaderType.PicoXR; break;
                        case "PSVR2Loader": loaderType = XRInputLoaderType.PSVR2; break;
                        default: loaderType = XRInputLoaderType.Unknown; break;
                    }

                    // auto detect
                    if (apiType == XRInputAPIType.AutoDetect)
                    {
                        if (loaderType == XRInputLoaderType.OpenXR)
                        {
                            #if !XRINPUT_DISABLE_OPENXR
                            string platformName = UnityEngine.XR.OpenXR.OpenXRRuntime.name;
                            Debug.Log($"OpenXR platform name '{platformName}'");
                            if (platformName == "Oculus") apiType = XRInputAPIType.OculusXR;
                            else apiType = XRInputAPIType.UnityEngine_XR;
                            #else
                            throw new NotSupportedException("You have XRINPUT_DISABLE_OPENXR enabled");
                            #endif
                        }
                        else if (loaderType == XRInputLoaderType.OculusXR)
                        {
                            apiType = XRInputAPIType.OculusXR;
                        }
                        else
                        {
                            #if UNITY_STANDALONE
                            if (loaderType == XRInputLoaderType.OpenVR) apiType = XRInputAPIType.OpenVR;
                            else apiType = XRInputAPIType.UnityEngine_XR;
                            #else
                            apiType = XRInputAPIType.UnityEngine_XR;
                            #endif
                        }
                    }
                }
                else
                {
                    loaderType = XRInputLoaderType.Unknown;
                    loaderTypeName = "None";
                }

                // init api
                Debug.Log("XRInput selected api: " + apiType.ToString());
                disposeAPI = true;
                switch (apiType)
                {
                    case XRInputAPIType.UnityEngine_XR: api = new UnityEngine_XR(); break;
                    case XRInputAPIType.OculusXR: api = new OculusXR(); break;
                    case XRInputAPIType.OpenVR: api = new OpenVR_New(); break;
                    case XRInputAPIType.OpenVR_Legacy: api = new OpenVR_Legacy(); break;
                    case XRInputAPIType.Pico2VR: api = new Pico2VR(); break;
                    default: throw new NotImplementedException();
                }

                api.Init();

                // ensure we dispose before stopping to avoid editor race-condition bugs
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
                #endif

                apiInit = true;
            }
            catch (Exception e)
            {
                if (InitializedCallback != null) InitializedCallback(false);
                throw e;
			}

            // enable rig
            if (rigSets != null)
            {
                foreach (var rig in rigSets)
                {
                    if (rig.loaderType == loaderType && rig.apiType == apiType)
                    {
                        if (rig.enableObjects != null)
                        {
                            foreach (var obj in rig.enableObjects)
                            {
                                if (obj != null) obj.SetActive(true);
                            }
                        }

                        if (rig.disableObjects != null)
                        {
                            foreach (var obj in rig.disableObjects)
                            {
                                if (obj != null) obj.SetActive(false);
                            }
                        }
                        break;
                    }
                }
            }

            // finish
            if (InitializedCallback != null) InitializedCallback(true);
        }

        #if UNITY_EDITOR
		private void EditorApplication_playModeStateChanged(UnityEditor.PlayModeStateChange change)
		{
			if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode) OnDestroy();
		}
        #endif

	    private void OnDestroy()
	    {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
            #endif

            if (disposeAPI && api != null)
            {
                api.Dispose();
                api = null;
                if (DisposedCallback != null) DisposedCallback();
            }

            apiInit = false;
        }

		private void OnApplicationQuit()
		{
			if (stopLoaderOnAppExit && loaderType == XRInputLoaderType.OpenVR && loader != null)
            {
                loader.Stop();
            }
		}

        private void FixedUpdate()
        {
            // only proccess if init
            if (!apiInit) return;

            // optional fixed-update for APIs
            api.FixedUpdate();
        }

        private void LateUpdate()
        {
            // only proccess if init
            if (!apiInit) return;

            // optional fixed-update for APIs
            api.LateUpdate();
        }

        private void Update()
        {
            // only proccess if init
            if (!apiInit) return;

            // optional update for APIs
            api.Update();

            // gather input from api
            int controllerCount;
            bool leftSet, rightSet;
            int leftSetIndex, rightSetIndex;
            SideToSet sideToSet;
            if (!api.GatherInput(state_controllers, out controllerCount, out leftSet, out leftSetIndex, out rightSet, out rightSetIndex, out sideToSet)) return;

            // track controller connection/disconnections
            if (lastControllerCount != controllerCount)
            {
                for (int i = 0; i != state_controllers.Length; ++i)
                {
                    var currentController = state_controllers[i];
                    var lastController = state_controllers_last[i];
                    if (currentController.connected != lastController.connected)
                    {
                        if (currentController.connected)
                        {
                            if (ControllerConnectedCallback != null) ControllerConnectedCallback(currentController.id, currentController.side, currentController.type);
                        }
                        else
                        {
                            if (ControllerDisconnectedMethod != null) ControllerDisconnectedMethod(currentController.id, currentController.side, currentController.type);
                        }
					}
                }
                lastControllerCount = controllerCount;
            }

            // store last controller state
            Array.Copy(state_controllers, state_controllers_last, state_controllers.Length);

            // if left or right not known use controller index as side
            if (sideToSet == SideToSet.Left || sideToSet == SideToSet.Both) state_controllerLeft = state_controllers[leftSetIndex];
            if (sideToSet == SideToSet.Right || sideToSet == SideToSet.Both) state_controllerRight = state_controllers[rightSetIndex];

            // null memory if no state
            if (!leftSet) state_controllerLeft = new XRControllerState();
            if (!rightSet) state_controllerRight = new XRControllerState();

            // buffer special controller states
            if (controllerCount != 0) state_controllerFirst = state_controllers[0];
            else state_controllerFirst = new XRControllerState();

            state_controllerMerged = new XRControllerState()
            {
                id = state_controllerMerged_ID// keep a constant ID
            };
            for (uint i = 0; i != controllerCount; ++i)
            {
                var controllerState = state_controllers[i];
                if (controllerState.connected) state_controllerMerged.connected = true;

                controllerState.buttonTrigger.Merge(ref state_controllerMerged.buttonTrigger);
                controllerState.buttonJoystick.Merge(ref state_controllerMerged.buttonJoystick);
                controllerState.buttonJoystick2.Merge(ref state_controllerMerged.buttonJoystick2);
                controllerState.buttonGrip.Merge(ref state_controllerMerged.buttonGrip);
                controllerState.buttonMenu.Merge(ref state_controllerMerged.buttonMenu);

                controllerState.button1.Merge(ref state_controllerMerged.button1);
                controllerState.button2.Merge(ref state_controllerMerged.button2);
                controllerState.button3.Merge(ref state_controllerMerged.button3);
                controllerState.button4.Merge(ref state_controllerMerged.button4);

                controllerState.touchTrigger.Merge(ref state_controllerMerged.touchTrigger);
                controllerState.touchJoystick.Merge(ref state_controllerMerged.touchJoystick);
                controllerState.touchJoystick2.Merge(ref state_controllerMerged.touchJoystick2);
                controllerState.touchGrip.Merge(ref state_controllerMerged.touchGrip);
                controllerState.touchMenu.Merge(ref state_controllerMerged.touchMenu);

                controllerState.touch1.Merge(ref state_controllerMerged.touch1);
                controllerState.touch2.Merge(ref state_controllerMerged.touch2);
                controllerState.touch3.Merge(ref state_controllerMerged.touch3);
                controllerState.touch4.Merge(ref state_controllerMerged.touch4);

                controllerState.trigger.Merge(ref state_controllerMerged.trigger);
                controllerState.grip.Merge(ref state_controllerMerged.grip);
                controllerState.joystick.Merge(ref state_controllerMerged.joystick);
                controllerState.joystick2.Merge(ref state_controllerMerged.joystick2);

                state_controllerMerged.linearVelocity += controllerState.linearVelocity;
                state_controllerMerged.angularVelocity += controllerState.angularVelocity;
                state_controllerMerged.linearVelocityValid |= controllerState.linearVelocityValid;
                state_controllerMerged.angularVelocityValid |= controllerState.angularVelocityValid;
            }

            // fire events
            //string name = stateGroup.name;

            // <<< buttons
            TestButtonEvent(ButtonTriggerOnEvent, ButtonTriggerDownEvent, ButtonTriggerUpEvent, ref state_controllerRight.buttonTrigger, XRControllerSide.Right, name);
            TestButtonEvent(ButtonTriggerOnEvent, ButtonTriggerDownEvent, ButtonTriggerUpEvent, ref state_controllerLeft.buttonTrigger, XRControllerSide.Left, name);

            TestButtonEvent(ButtonJoystickOnEvent, ButtonJoystickDownEvent, ButtonJoystickUpEvent, ref state_controllerRight.buttonJoystick, XRControllerSide.Right, name);
            TestButtonEvent(ButtonJoystickOnEvent, ButtonJoystickDownEvent, ButtonJoystickUpEvent, ref state_controllerLeft.buttonJoystick, XRControllerSide.Left, name);

            TestButtonEvent(ButtonJoystick2OnEvent, ButtonJoystick2DownEvent, ButtonJoystick2UpEvent, ref state_controllerRight.buttonJoystick2, XRControllerSide.Right, name);
            TestButtonEvent(ButtonJoystick2OnEvent, ButtonJoystick2DownEvent, ButtonJoystick2UpEvent, ref state_controllerLeft.buttonJoystick2, XRControllerSide.Left, name);

            TestButtonEvent(ButtonGripOnEvent, ButtonGripDownEvent, ButtonGripUpEvent, ref state_controllerRight.buttonGrip, XRControllerSide.Right, name);
            TestButtonEvent(ButtonGripOnEvent, ButtonGripDownEvent, ButtonGripUpEvent, ref state_controllerLeft.buttonGrip, XRControllerSide.Left, name);

            TestButtonEvent(ButtonMenuOnEvent, ButtonMenuDownEvent, ButtonMenuUpEvent, ref state_controllerRight.buttonMenu, XRControllerSide.Right, name);
            TestButtonEvent(ButtonMenuOnEvent, ButtonMenuDownEvent, ButtonMenuUpEvent, ref state_controllerLeft.buttonMenu, XRControllerSide.Left, name);

            TestButtonEvent(Button1OnEvent, Button1DownEvent, Button1UpEvent, ref state_controllerRight.button1, XRControllerSide.Right, name);
            TestButtonEvent(Button1OnEvent, Button1DownEvent, Button1UpEvent, ref state_controllerLeft.button1, XRControllerSide.Left, name);

            TestButtonEvent(Button2OnEvent, Button2DownEvent, Button2UpEvent, ref state_controllerRight.button2, XRControllerSide.Right, name);
            TestButtonEvent(Button2OnEvent, Button2DownEvent, Button2UpEvent, ref state_controllerLeft.button2, XRControllerSide.Left, name);

            TestButtonEvent(Button3OnEvent, Button3DownEvent, Button3UpEvent, ref state_controllerRight.button3, XRControllerSide.Right, name);
            TestButtonEvent(Button3OnEvent, Button3DownEvent, Button3UpEvent, ref state_controllerLeft.button3, XRControllerSide.Left, name);

            TestButtonEvent(Button4OnEvent, Button4DownEvent, Button4UpEvent, ref state_controllerRight.button4, XRControllerSide.Right, name);
            TestButtonEvent(Button4OnEvent, Button4DownEvent, Button4UpEvent, ref state_controllerLeft.button4, XRControllerSide.Left, name);

            // <<< touch
            TestButtonEvent(TouchTriggerOnEvent, TouchTriggerDownEvent, TouchTriggerUpEvent, ref state_controllerRight.touchTrigger, XRControllerSide.Right, name);
            TestButtonEvent(TouchTriggerOnEvent, TouchTriggerDownEvent, TouchTriggerUpEvent, ref state_controllerLeft.touchTrigger, XRControllerSide.Left, name);

            TestButtonEvent(TouchJoystickOnEvent, TouchJoystickDownEvent, TouchJoystickUpEvent, ref state_controllerRight.touchJoystick, XRControllerSide.Right, name);
            TestButtonEvent(TouchJoystickOnEvent, TouchJoystickDownEvent, TouchJoystickUpEvent, ref state_controllerLeft.touchJoystick, XRControllerSide.Left, name);

            TestButtonEvent(TouchJoystick2OnEvent, TouchJoystick2DownEvent, TouchJoystick2UpEvent, ref state_controllerRight.touchJoystick2, XRControllerSide.Right, name);
            TestButtonEvent(TouchJoystick2OnEvent, TouchJoystick2DownEvent, TouchJoystick2UpEvent, ref state_controllerLeft.touchJoystick2, XRControllerSide.Left, name);

            TestButtonEvent(TouchGripOnEvent, TouchGripDownEvent, TouchGripUpEvent, ref state_controllerRight.touchGrip, XRControllerSide.Right, name);
            TestButtonEvent(TouchGripOnEvent, TouchGripDownEvent, TouchGripUpEvent, ref state_controllerLeft.touchGrip, XRControllerSide.Left, name);

            TestButtonEvent(TouchMenuOnEvent, TouchMenuDownEvent, TouchMenuUpEvent, ref state_controllerRight.touchMenu, XRControllerSide.Right, name);
            TestButtonEvent(TouchMenuOnEvent, TouchMenuDownEvent, TouchMenuUpEvent, ref state_controllerLeft.touchMenu, XRControllerSide.Left, name);

            TestButtonEvent(Touch1OnEvent, Touch1DownEvent, Touch1UpEvent, ref state_controllerRight.touch1, XRControllerSide.Right, name);
            TestButtonEvent(Touch1OnEvent, Touch1DownEvent, Touch1UpEvent, ref state_controllerLeft.touch1, XRControllerSide.Left, name);

            TestButtonEvent(Touch2OnEvent, Touch2DownEvent, Touch2UpEvent, ref state_controllerRight.touch2, XRControllerSide.Right, name);
            TestButtonEvent(Touch2OnEvent, Touch2DownEvent, Touch2UpEvent, ref state_controllerLeft.touch2, XRControllerSide.Left, name);

            TestButtonEvent(Touch3OnEvent, Touch3DownEvent, Touch3UpEvent, ref state_controllerRight.touch3, XRControllerSide.Right, name);
            TestButtonEvent(Touch3OnEvent, Touch3DownEvent, Touch3UpEvent, ref state_controllerLeft.touch3, XRControllerSide.Left, name);

            TestButtonEvent(Touch4OnEvent, Touch4DownEvent, Touch4UpEvent, ref state_controllerRight.touch4, XRControllerSide.Right, name);
            TestButtonEvent(Touch4OnEvent, Touch4DownEvent, Touch4UpEvent, ref state_controllerLeft.touch4, XRControllerSide.Left, name);

            // <<< analogs
            TestAnalogEvent(TriggerActiveEvent, ref state_controllerRight.trigger, XRControllerSide.Right, name);
            TestAnalogEvent(TriggerActiveEvent, ref state_controllerLeft.trigger, XRControllerSide.Left, name);

            TestAnalogEvent(GripActiveEvent, ref state_controllerRight.grip, XRControllerSide.Right, name);
            TestAnalogEvent(GripActiveEvent, ref state_controllerLeft.grip, XRControllerSide.Left, name);

            // <<< joysticks
            TestJoystickEvent(JoystickActiveEvent, ref state_controllerRight.joystick, XRControllerSide.Right, name);
            TestJoystickEvent(JoystickActiveEvent, ref state_controllerLeft.joystick, XRControllerSide.Left, name);

            TestJoystickEvent(Joystick2ActiveEvent, ref state_controllerRight.joystick2, XRControllerSide.Right, name);
            TestJoystickEvent(Joystick2ActiveEvent, ref state_controllerLeft.joystick2, XRControllerSide.Left, name);
        }

        #region Public static interface
        public delegate void ButtonEvent(XRControllerSide side, string stateGroup);
        public delegate void AnalogEvent(XRControllerSide side, float value, string stateGroup);
        public delegate void JoystickEvent(XRControllerSide side, Vector2 value, string stateGroup);

        public static event ButtonEvent ButtonTriggerOnEvent, ButtonTriggerDownEvent, ButtonTriggerUpEvent;
        public static event ButtonEvent ButtonJoystickOnEvent, ButtonJoystickDownEvent, ButtonJoystickUpEvent;
        public static event ButtonEvent ButtonJoystick2OnEvent, ButtonJoystick2DownEvent, ButtonJoystick2UpEvent;
        public static event ButtonEvent ButtonGripOnEvent, ButtonGripDownEvent, ButtonGripUpEvent;
        public static event ButtonEvent ButtonMenuOnEvent, ButtonMenuDownEvent, ButtonMenuUpEvent;
        public static event ButtonEvent Button1OnEvent, Button1DownEvent, Button1UpEvent;
        public static event ButtonEvent Button2OnEvent, Button2DownEvent, Button2UpEvent;
        public static event ButtonEvent Button3OnEvent, Button3DownEvent, Button3UpEvent;
        public static event ButtonEvent Button4OnEvent, Button4DownEvent, Button4UpEvent;

        public static event ButtonEvent TouchTriggerOnEvent, TouchTriggerDownEvent, TouchTriggerUpEvent;
        public static event ButtonEvent TouchJoystickOnEvent, TouchJoystickDownEvent, TouchJoystickUpEvent;
        public static event ButtonEvent TouchJoystick2OnEvent, TouchJoystick2DownEvent, TouchJoystick2UpEvent;
        public static event ButtonEvent TouchGripOnEvent, TouchGripDownEvent, TouchGripUpEvent;
        public static event ButtonEvent TouchMenuOnEvent, TouchMenuDownEvent, TouchMenuUpEvent;
        public static event ButtonEvent Touch1OnEvent, Touch1DownEvent, Touch1UpEvent;
        public static event ButtonEvent Touch2OnEvent, Touch2DownEvent, Touch2UpEvent;
        public static event ButtonEvent Touch3OnEvent, Touch3DownEvent, Touch3UpEvent;
        public static event ButtonEvent Touch4OnEvent, Touch4DownEvent, Touch4UpEvent;

        public static event AnalogEvent TriggerActiveEvent, GripActiveEvent;
        public static event JoystickEvent JoystickActiveEvent, Joystick2ActiveEvent;

        private static void TestButtonEvent(ButtonEvent onEvent, ButtonEvent downEvent, ButtonEvent upEvent, ref XRControllerButton button, XRControllerSide side, string stateGroup)
        {
            if (onEvent != null && button.on) onEvent(side, stateGroup);
            if (downEvent != null && button.down) downEvent(side, stateGroup);
            if (upEvent != null && button.up) upEvent(side, stateGroup);
        }

        private static void TestAnalogEvent(AnalogEvent e, ref XRControllerAnalog analog, XRControllerSide side, string stateGroup)
        {
            if (e != null && analog.value != 0) e(side, analog.value, stateGroup);
        }

        private static void TestJoystickEvent(JoystickEvent e, ref XRControllerJoystick joystick, XRControllerSide side, string stateGroup)
        {
            if (e != null && joystick.value.magnitude != 0) e(side, joystick.value, stateGroup);
        }

        /// <summary>
        /// Gets full controller state
        /// </summary>
        public static XRControllerState ControllerState(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerState();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst;
                case XRController.Left: return singleton.state_controllerLeft;
                case XRController.Right: return singleton.state_controllerRight;
                case XRController.Merged: return singleton.state_controllerMerged;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton ButtonTrigger(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonTrigger;
                case XRController.Left: return singleton.state_controllerLeft.buttonTrigger;
                case XRController.Right: return singleton.state_controllerRight.buttonTrigger;
                case XRController.Merged: return singleton.state_controllerMerged.buttonTrigger;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton ButtonJoystick(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonJoystick;
                case XRController.Left: return singleton.state_controllerLeft.buttonJoystick;
                case XRController.Right: return singleton.state_controllerRight.buttonJoystick;
                case XRController.Merged: return singleton.state_controllerMerged.buttonJoystick;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton ButtonJoystick2(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonJoystick2;
                case XRController.Left: return singleton.state_controllerLeft.buttonJoystick2;
                case XRController.Right: return singleton.state_controllerRight.buttonJoystick2;
                case XRController.Merged: return singleton.state_controllerMerged.buttonJoystick2;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton ButtonGrip(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonGrip;
                case XRController.Left: return singleton.state_controllerLeft.buttonGrip;
                case XRController.Right: return singleton.state_controllerRight.buttonGrip;
                case XRController.Merged: return singleton.state_controllerMerged.buttonGrip;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton ButtonMenu(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.buttonMenu;
                case XRController.Left: return singleton.state_controllerLeft.buttonMenu;
                case XRController.Right: return singleton.state_controllerRight.buttonMenu;
                case XRController.Merged: return singleton.state_controllerMerged.buttonMenu;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Button1(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.button1;
                case XRController.Left: return singleton.state_controllerLeft.button1;
                case XRController.Right: return singleton.state_controllerRight.button1;
                case XRController.Merged: return singleton.state_controllerMerged.button1;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Button2(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.button2;
                case XRController.Left: return singleton.state_controllerLeft.button2;
                case XRController.Right: return singleton.state_controllerRight.button2;
                case XRController.Merged: return singleton.state_controllerMerged.button2;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Button3(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.button3;
                case XRController.Left: return singleton.state_controllerLeft.button3;
                case XRController.Right: return singleton.state_controllerRight.button3;
                case XRController.Merged: return singleton.state_controllerMerged.button3;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Button4(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.button4;
                case XRController.Left: return singleton.state_controllerLeft.button4;
                case XRController.Right: return singleton.state_controllerRight.button4;
                case XRController.Merged: return singleton.state_controllerMerged.button4;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton TouchTrigger(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchTrigger;
                case XRController.Left: return singleton.state_controllerLeft.touchTrigger;
                case XRController.Right: return singleton.state_controllerRight.touchTrigger;
                case XRController.Merged: return singleton.state_controllerMerged.touchTrigger;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton TouchJoystick(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchJoystick;
                case XRController.Left: return singleton.state_controllerLeft.touchJoystick;
                case XRController.Right: return singleton.state_controllerRight.touchJoystick;
                case XRController.Merged: return singleton.state_controllerMerged.touchJoystick;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton TouchJoystick2(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchJoystick2;
                case XRController.Left: return singleton.state_controllerLeft.touchJoystick2;
                case XRController.Right: return singleton.state_controllerRight.touchJoystick2;
                case XRController.Merged: return singleton.state_controllerMerged.touchJoystick2;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton TouchGrip(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchGrip;
                case XRController.Left: return singleton.state_controllerLeft.touchGrip;
                case XRController.Right: return singleton.state_controllerRight.touchGrip;
                case XRController.Merged: return singleton.state_controllerMerged.touchGrip;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton TouchMenu(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touchMenu;
                case XRController.Left: return singleton.state_controllerLeft.touchMenu;
                case XRController.Right: return singleton.state_controllerRight.touchMenu;
                case XRController.Merged: return singleton.state_controllerMerged.touchMenu;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Touch1(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touch1;
                case XRController.Left: return singleton.state_controllerLeft.touch1;
                case XRController.Right: return singleton.state_controllerRight.touch1;
                case XRController.Merged: return singleton.state_controllerMerged.touch1;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Touch2(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touch2;
                case XRController.Left: return singleton.state_controllerLeft.touch2;
                case XRController.Right: return singleton.state_controllerRight.touch2;
                case XRController.Merged: return singleton.state_controllerMerged.touch2;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Touch3(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touch3;
                case XRController.Left: return singleton.state_controllerLeft.touch3;
                case XRController.Right: return singleton.state_controllerRight.touch3;
                case XRController.Merged: return singleton.state_controllerMerged.touch3;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Touch4(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.touch4;
                case XRController.Left: return singleton.state_controllerLeft.touch4;
                case XRController.Right: return singleton.state_controllerRight.touch4;
                case XRController.Merged: return singleton.state_controllerMerged.touch4;
            }
            throw new NotImplementedException();
        }

        public static XRControllerAnalog Trigger(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerAnalog();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.trigger;
                case XRController.Left: return singleton.state_controllerLeft.trigger;
                case XRController.Right: return singleton.state_controllerRight.trigger;
                case XRController.Merged: return singleton.state_controllerMerged.trigger;
            }
            throw new NotImplementedException();
        }

        public static XRControllerAnalog Grip(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerAnalog();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.grip;
                case XRController.Left: return singleton.state_controllerLeft.grip;
                case XRController.Right: return singleton.state_controllerRight.grip;
                case XRController.Merged: return singleton.state_controllerMerged.grip;
            }
            throw new NotImplementedException();
        }

        public static XRControllerJoystick Joystick(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerJoystick();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.joystick;
                case XRController.Left: return singleton.state_controllerLeft.joystick;
                case XRController.Right: return singleton.state_controllerRight.joystick;
                case XRController.Merged: return singleton.state_controllerMerged.joystick;
            }
            throw new NotImplementedException();
        }

        public static XRControllerJoystick Joystick2(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerJoystick();
            switch (controller)
            {
                case XRController.First: return singleton.state_controllerFirst.joystick2;
                case XRController.Left: return singleton.state_controllerLeft.joystick2;
                case XRController.Right: return singleton.state_controllerRight.joystick2;
                case XRController.Merged: return singleton.state_controllerMerged.joystick2;
            }
            throw new NotImplementedException();
        }

        public static void Velocity(XRController controller, out Vector3 linearVelocity, out Vector3 angularVelocity, out bool linearVelocityValid, out bool angularVelocityValid)
        {
            if (singleton == null || !singleton.apiInit)
			{
                linearVelocity = Vector3.zero;
                angularVelocity = Vector3.zero;
                linearVelocityValid = false;
                angularVelocityValid = false;
                return;
			}

            switch (controller)
            {
                case XRController.First:
                    linearVelocity = singleton.state_controllerFirst.linearVelocity;
                    angularVelocity = singleton.state_controllerFirst.angularVelocity;
                    linearVelocityValid = singleton.state_controllerFirst.linearVelocityValid;
                    angularVelocityValid = singleton.state_controllerFirst.angularVelocityValid;
                    return;

                case XRController.Left:
                    linearVelocity = singleton.state_controllerLeft.linearVelocity;
                    angularVelocity = singleton.state_controllerLeft.angularVelocity;
                    linearVelocityValid = singleton.state_controllerLeft.linearVelocityValid;
                    angularVelocityValid = singleton.state_controllerLeft.angularVelocityValid;
                    return;

                case XRController.Right:
                    linearVelocity = singleton.state_controllerRight.linearVelocity;
                    angularVelocity = singleton.state_controllerRight.angularVelocity;
                    linearVelocityValid = singleton.state_controllerRight.linearVelocityValid;
                    angularVelocityValid = singleton.state_controllerRight.angularVelocityValid;
                    return;

                case XRController.Merged:
                    linearVelocity = singleton.state_controllerMerged.linearVelocity;
                    angularVelocity = singleton.state_controllerMerged.angularVelocity;
                    linearVelocityValid = singleton.state_controllerMerged.linearVelocityValid;
                    angularVelocityValid = singleton.state_controllerMerged.angularVelocityValid;
                    return;
            }
            throw new NotImplementedException();
        }

        public static void VelocityHMD(out Vector3 linearVelocity, out Vector3 angularVelocity, out bool linearVelocityValid, out bool angularVelocityValid)
        {
            if (singleton == null || !singleton.apiInit)
            {
                linearVelocity = Vector3.zero;
                angularVelocity = Vector3.zero;
                linearVelocityValid = false;
                angularVelocityValid = false;
                return;
            }

            linearVelocityValid = singleton.api.hmdLinearVelocityValid;
            linearVelocity = singleton.api.hmdLinearVelocity;
            angularVelocityValid = singleton.api.hmdAngularVelocityValid;
            angularVelocity = singleton.api.hmdAngularVelocity;
        }

        public static bool SetRumble(XRControllerRumbleSide controller, float strength, float duration = .1f)
        {
            if (singleton == null || !singleton.apiInit) return false;
            return singleton.api.SetRumble(controller, strength, duration);
        }

        public static bool SetHMDRumble(float strength, float duration = .1f)
        {
            if (singleton == null || !singleton.apiInit) return false;
            return singleton.api.SetHMDRumble(strength, duration);
        }

        public static XRInputControllerType GetControllerType(XRInputControllerTypeSide controller)
        {
            if (singleton == null || !singleton.apiInit) return XRInputControllerType.Unknown;
            switch (controller)
            {
                case XRInputControllerTypeSide.First: return singleton.state_controllerFirst.type;
                case XRInputControllerTypeSide.Left: return singleton.state_controllerLeft.type;
                case XRInputControllerTypeSide.Right: return singleton.state_controllerRight.type;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates total velocity of linear + angular converted to linear
        /// </summary>
        /// <param name="handLinearVelocity">Linear Velocity of Hand</param>
        /// <param name="handAngularVelocity">Angular Velocity of Hand</param>
        /// <param name="handPosition">Hand position in World-Space</param>
        /// <param name="grabPosition">Point of object around hand in World-Space</param>
        /// <returns>Total velocity</returns>
        public static Vector3 GetVelocityAtOffset(Vector3 handLinearVelocity, Vector3 handAngularVelocity, Vector3 handPosition, Vector3 grabPosition)
        {
            return Vector3.Cross(handAngularVelocity, grabPosition - handPosition) + handLinearVelocity;
        }

        /// <summary>
        /// Calculates total velocity by subtracting last calculated rotational position to its current
        /// </summary>
        /// <param name="handLinearVelocity">Linear Velocity of Hand</param>
        /// <param name="handAngularVelocity">Angular Velocity of Hand</param>
        /// <param name="handPosition">Hand position in World-Space</param>
        /// <param name="grabPosition">Point of object around hand in World-Space</param>
        /// <returns>Total velocity</returns>
        public static Vector3 GetVelocityAtOffset2(Vector3 handLinearVelocity, Vector3 handAngularVelocity, Vector3 handPosition, Vector3 grabPosition)
        {
            const float meterPerSec = 0.01f;
            var grabPosLocal = grabPosition - handPosition;
            var grabPosLast = (Quaternion.AngleAxis(-(handAngularVelocity.magnitude * meterPerSec) * Mathf.Rad2Deg, handAngularVelocity.normalized) * grabPosLocal) + handPosition;
            grabPosLast -= handLinearVelocity * meterPerSec;
            return (grabPosition - grabPosLast) * 100;
        }
        #endregion
    }

    public enum XRController
    {
        /// <summary>
        /// First controller connected
        /// </summary>
        First,

        /// <summary>
        /// All controller states merged
        /// </summary>
        Merged,

        /// <summary>
        /// Left controller states only
        /// </summary>
        Left,

        /// <summary>
        /// Right controller states only
        /// </summary>
        Right
    }

    public enum XRControllerSide
    {
        Unknown,
        Left,
        Right
	}

    public enum XRControllerRumbleSide
    {
        Both,
        Left,
        Right
    }

    public enum XRInputControllerTypeSide
    {
        First,
        Left,
        Right
    }

    public enum XRInputControllerType
    {
        Unknown,
        Oculus,
        PICO,
        HTCVive,
        HTCViveCosmos,
        HTCViveWave,
        ValveIndex,
        WMR,
        WMR_G2,
        PSVR2
    }

    public struct XRControllerState
    {
        /// <summary>
        /// Unique ID of this controller instance
        /// </summary>
        public Guid id;

        public bool connected;
        public XRInputControllerType type;
        public XRControllerSide side;
        public XRControllerButton touchBumper;
        public XRControllerButton touchTrigger, touchJoystick, touchJoystick2, touchGrip, touchMenu;
        public XRControllerButton touch1, touch2, touch3, touch4;
        public XRControllerButton buttonBumper;
        public XRControllerButton buttonTrigger, buttonJoystick, buttonJoystick2, buttonGrip, buttonMenu;
        public XRControllerButton button1, button2, button3, button4;
        public XRControllerAnalog trigger, grip;
        public XRControllerJoystick joystick, joystick2;

        public bool linearVelocityValid, angularVelocityValid;
        public Vector3 linearVelocity, angularVelocity;
    }

    public struct XRControllerButton
    {
        /// <summary>
        /// True when button is held on
        /// </summary>
        public bool on;
        
        /// <summary>
        /// True when button-pressed / state-changes
        /// </summary>
        public bool down;

        /// <summary>
        /// True when button-released / state-changes
        /// </summary>
        public bool up;

        internal void Update(bool on)
        {
            down = false;
            up = false;
            if (this.on != on)
            {
                if (on) down = true;
                else if (!on) up = true;
            }
            this.on = on;
	    }

        internal void Merge(ref XRControllerButton button)
        {
            if (on) button.on = true;
            if (down) button.down = true;
            if (up) button.up = true;
		}
    }

    public struct XRControllerAnalog
    {
        /// <summary>
        /// Analog 0-1 value
        /// </summary>
        public float value;

        /// <summary>
        /// If the value drops below this, it will clamp to 0
        /// </summary>
        public static float tolerance = 0.2f;

        /// <summary>
        /// Adjust analog value before virtual buttons trigger
        /// </summary>
        public static float virtualButtonThreshold = .9f;

        internal void Update(float value)
        {
            if (value < tolerance) value = 0.0f;
            this.value = value;
	    }

        internal void Merge(ref XRControllerAnalog analog)
        {
            if (value >= tolerance) analog.value = value;
        }
    }

    public struct XRControllerJoystick
    {
        /// <summary>
        /// Joystick 0-1 value in length
        /// </summary>
        public Vector2 value;

        /// <summary>
        /// If the value drops below this, it will clamp to (0,0)
        /// </summary>
        public static float tolerance = 0.2f;

        internal void Update(Vector2 value)
        {
            if (value.magnitude < tolerance) value = Vector2.zero;
            this.value = value;
        }

        internal void Merge(ref XRControllerJoystick joystick)
        {
            if (value.magnitude >= tolerance) joystick.value = value;
        }
    }
}