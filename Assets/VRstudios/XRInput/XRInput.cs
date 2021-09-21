using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using VRstudios.API;

namespace VRstudios
{
    public enum XRInputAPIType
    {
        AutoDetect,
        UnityEngine_XR,
        OpenVR,
        OpenVR_Legacy
    }

    [DefaultExecutionOrder(-32000)]
    public sealed class XRInput : MonoBehaviour
    {
        public static XRInput singleton { get; private set; }

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

        private const int controllerStateLength = 4;
        private int lastControllerCount;
        public const string defaultStateGroupName = "Default";
        private XRControllerStateGroup activeStateGroup, switchNextFrame_StateGroup;
        private bool switchNextFrame_DisableInputForNonActive;
        private Dictionary<string, XRControllerStateGroup> stateGroups = new Dictionary<string, XRControllerStateGroup>();
        private Guid[] stateGuids = new Guid[controllerStateLength];
        private Guid state_controllerMerged_ID = Guid.NewGuid();

        private class XRControllerStateGroup
        {
            public readonly string name;
            public bool updateInput;
            public XRControllerState[] state_controllers = new XRControllerState[controllerStateLength], state_controllers_last = new XRControllerState[controllerStateLength];
            public XRControllerState state_controllerLeft, state_controllerRight;
            public XRControllerState state_controllerFirst, state_controllerMerged;

            public XRControllerStateGroup(string name, bool updateInput, Guid[] stateGuids)
            {
                this.name = name;
                this.updateInput = updateInput;
                for (int i = 0; i != stateGuids.Length; ++i)
                {
                    state_controllers[i].id = singleton.stateGuids[i];
				}
			}
        }

        public static void AddStateGroup(string name, bool updateInput, bool setActive)
        {
            var group = new XRControllerStateGroup(name, updateInput, singleton.stateGuids);
            singleton.stateGroups.Add(name, group);
            if (setActive) singleton.activeStateGroup = group;
		}

        public static void RemoveStateGroup(string name)
        {
            if (name == defaultStateGroupName) throw new Exception("Cannot remove default group");
            singleton.stateGroups.Remove(name);
            singleton.activeStateGroup = singleton.stateGroups[defaultStateGroupName];
        }

        public static void SwitchStateGroup_NextFrame(string name, bool updateInput, bool disableInputForNonActive, bool copyCurrentState)
        {
            singleton.switchNextFrame_StateGroup = singleton.stateGroups[name];
            singleton.switchNextFrame_StateGroup.updateInput = updateInput;
            singleton.switchNextFrame_DisableInputForNonActive = disableInputForNonActive;
            if (copyCurrentState)
            {
                Array.Copy(singleton.activeStateGroup.state_controllers, singleton.switchNextFrame_StateGroup.state_controllers, singleton.activeStateGroup.state_controllers.Length);
                Array.Copy(singleton.activeStateGroup.state_controllers_last, singleton.switchNextFrame_StateGroup.state_controllers_last, singleton.activeStateGroup.state_controllers_last.Length);
                singleton.switchNextFrame_StateGroup.state_controllerLeft = singleton.activeStateGroup.state_controllerLeft;
                singleton.switchNextFrame_StateGroup.state_controllerRight = singleton.activeStateGroup.state_controllerRight;
                singleton.switchNextFrame_StateGroup.state_controllerFirst = singleton.activeStateGroup.state_controllerFirst;
                singleton.switchNextFrame_StateGroup.state_controllerMerged = singleton.activeStateGroup.state_controllerMerged;
            }
		}

        public static void SwitchStateGroup_Instant(string name)
        {
            singleton.activeStateGroup = singleton.stateGroups[name];
        }

        public static string GetActiveState()
        {
            return singleton.activeStateGroup.name;
		}

        public static void SetStateGroupInputMode(string name, bool updateInput)
        {
            singleton.stateGroups[name].updateInput = updateInput;
        }

        public static void SetAllStateGroupsInputMode(bool updateInput)
        {
            foreach (var stateGroupKeyValue in singleton.stateGroups)
            {
                stateGroupKeyValue.Value.updateInput = updateInput;
            }
        }

        public static void SetActiveStateGroupInputMode(bool updateInput)
        {
            singleton.activeStateGroup.updateInput = updateInput;
        }

        public static void SetUnactiveStateGroupsInputMode(bool updateInput)
        {
            foreach (var stateGroupKeyValue in singleton.stateGroups)
            {
                if (stateGroupKeyValue.Value != singleton.activeStateGroup) stateGroupKeyValue.Value.updateInput = updateInput;
            }
        }

        private static void UpdateStateGroupToNoInput(XRControllerStateGroup stateGroup)
        {
            static void UpdateButton(ref XRControllerButton button)
            {
                button.Update(false);
			}

            static void UpdateAnalog(ref XRControllerAnalog analog)
            {
                analog.Update(0);
            }

            static void UpdateJoystick(ref XRControllerJoystick joystick)
            {
                joystick.Update(Vector2.zero);
            }

            static void UpdateControllerState(ref XRControllerState state)
            {
                // main buttons
                UpdateButton(ref state.button1);
                UpdateButton(ref state.button2);
                UpdateButton(ref state.button3);
                UpdateButton(ref state.button4);

                UpdateButton(ref state.touch1);
                UpdateButton(ref state.touch2);
                UpdateButton(ref state.touch3);
                UpdateButton(ref state.touch4);

                // other buttons
                UpdateButton(ref state.buttonTrigger);
                UpdateButton(ref state.buttonJoystick);
                UpdateButton(ref state.buttonJoystick2);
                UpdateButton(ref state.buttonGrip);
                UpdateButton(ref state.buttonMenu);
                UpdateButton(ref state.buttonBumper);

                UpdateButton(ref state.touchTrigger);
                UpdateButton(ref state.touchJoystick);
                UpdateButton(ref state.touchJoystick2);
                UpdateButton(ref state.touchGrip);
                UpdateButton(ref state.touchMenu);
                UpdateButton(ref state.touchBumper);

                // analogs
                UpdateAnalog(ref state.trigger);
                UpdateAnalog(ref state.grip);

                // joysticks
                UpdateJoystick(ref state.joystick);
                UpdateJoystick(ref state.joystick2);
            }

            UpdateControllerState(ref stateGroup.state_controllerLeft);
            UpdateControllerState(ref stateGroup.state_controllerRight);
            UpdateControllerState(ref stateGroup.state_controllerFirst);
            UpdateControllerState(ref stateGroup.state_controllerMerged);
            for (int i = 0; i != controllerStateLength; ++i)
            {
                UpdateControllerState(ref stateGroup.state_controllers[i]);
                Array.Copy(stateGroup.state_controllers, stateGroup.state_controllers_last, stateGroup.state_controllers.Length);
            }
		}

        private static void UpdateStateGroupToExistingInput(XRControllerStateGroup stateGroupSrc, XRControllerStateGroup stateGroupDst)
        {
            static void UpdateButton(ref XRControllerButton buttonSrc, ref XRControllerButton buttonDst)
            {
                buttonDst.Update(buttonSrc.on);
            }

            static void UpdateAnalog(ref XRControllerAnalog analogSrc, ref XRControllerAnalog analogDst)
            {
                analogDst.Update(analogSrc.value);
            }

            static void UpdateJoystick(ref XRControllerJoystick joystickSrc, ref XRControllerJoystick joystickDst)
            {
                joystickDst.Update(joystickSrc.value);
            }

            static void UpdateControllerState(ref XRControllerState stateSrc, ref XRControllerState stateDst)
            {
                // main buttons
                UpdateButton(ref stateSrc.button1, ref stateDst.button1);
                UpdateButton(ref stateSrc.button2, ref stateDst.button2);
                UpdateButton(ref stateSrc.button3, ref stateDst.button3);
                UpdateButton(ref stateSrc.button4, ref stateDst.button4);

                UpdateButton(ref stateSrc.touch1, ref stateDst.touch1);
                UpdateButton(ref stateSrc.touch2, ref stateDst.touch2);
                UpdateButton(ref stateSrc.touch3, ref stateDst.touch3);
                UpdateButton(ref stateSrc.touch4, ref stateDst.touch4);

                // other buttons
                UpdateButton(ref stateSrc.buttonTrigger, ref stateDst.buttonTrigger);
                UpdateButton(ref stateSrc.buttonJoystick, ref stateDst.buttonJoystick);
                UpdateButton(ref stateSrc.buttonJoystick2, ref stateDst.buttonJoystick2);
                UpdateButton(ref stateSrc.buttonGrip, ref stateDst.buttonGrip);
                UpdateButton(ref stateSrc.buttonMenu, ref stateDst.buttonMenu);
                UpdateButton(ref stateSrc.buttonBumper, ref stateDst.buttonBumper);

                UpdateButton(ref stateSrc.touchTrigger, ref stateDst.touchTrigger);
                UpdateButton(ref stateSrc.touchJoystick, ref stateDst.touchJoystick);
                UpdateButton(ref stateSrc.touchJoystick2, ref stateDst.touchJoystick2);
                UpdateButton(ref stateSrc.touchGrip, ref stateDst.touchGrip);
                UpdateButton(ref stateSrc.touchMenu, ref stateDst.touchMenu);
                UpdateButton(ref stateSrc.touchBumper, ref stateDst.touchBumper);

                // analogs
                UpdateAnalog(ref stateSrc.trigger, ref stateDst.trigger);
                UpdateAnalog(ref stateSrc.grip, ref stateDst.grip);

                // joysticks
                UpdateJoystick(ref stateSrc.joystick, ref stateDst.joystick);
                UpdateJoystick(ref stateSrc.joystick2, ref stateDst.joystick2);
            }

            UpdateControllerState(ref stateGroupSrc.state_controllerLeft, ref stateGroupDst.state_controllerLeft);
            UpdateControllerState(ref stateGroupSrc.state_controllerRight, ref stateGroupDst.state_controllerRight);
            UpdateControllerState(ref stateGroupSrc.state_controllerFirst, ref stateGroupDst.state_controllerFirst);
            UpdateControllerState(ref stateGroupSrc.state_controllerMerged, ref stateGroupDst.state_controllerMerged);
            for (int i = 0; i != controllerStateLength; ++i)
            {
                UpdateControllerState(ref stateGroupSrc.state_controllers[i], ref stateGroupDst.state_controllers[i]);
                Array.Copy(stateGroupDst.state_controllers, stateGroupDst.state_controllers_last, stateGroupDst.state_controllers.Length);
            }
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
            Debug.Log("XRInput version: 1.1.0");

            // wait for XR loader
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
                // init controller instance IDs
                for (int i = 0; i != stateGuids.Length; ++i)
                {
                    stateGuids[i] = Guid.NewGuid();
				}

                // add default state group
                activeStateGroup = new XRControllerStateGroup(defaultStateGroupName, true, stateGuids);
                stateGroups.Add(defaultStateGroupName, activeStateGroup);

                // get loader type
                var loaderType = loader.GetType();
                string loaderTypeName = loaderType.Name;
                Debug.Log($"XR-Loader: '{loader.name}' TYPE:{loaderType}");

                // auto set rumble channel
                if (autoSetRumbleChannel)
                {
                    rumbleChannel = 0;
                }

                // auto detect
                if (apiType == XRInputAPIType.AutoDetect)
                {
                    #if UNITY_STANDALONE
                    if (loaderTypeName == "OpenVRLoader") apiType = XRInputAPIType.OpenVR;
                    else apiType = XRInputAPIType.UnityEngine_XR;
                    #else
                    apiType = XRInputAPIType.UnityEngine_XR;
                    #endif
                }

                // init api
                disposeAPI = true;
                switch (apiType)
                {
                    case XRInputAPIType.UnityEngine_XR: api = new UnityEngine_XR(); break;
                    case XRInputAPIType.OpenVR: api = new OpenVR_New(); break;
                    case XRInputAPIType.OpenVR_Legacy: api = new OpenVR_Legacy(); break;
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

            apiInit = false;
            if (disposeAPI && api != null)
            {
                api.Dispose();
                api = null;
                if (DisposedCallback != null) DisposedCallback();
            }
        }

		private void OnApplicationQuit()
		{
			if (stopLoaderOnAppExit && loader != null) loader.Stop();
		}

		private void Update()
        {
            // gather controller states from current API
            if (!apiInit) return;

            // switch state group if needed
            if (switchNextFrame_StateGroup != null)
            {
                activeStateGroup = switchNextFrame_StateGroup;
                switchNextFrame_StateGroup = null;
                if (switchNextFrame_DisableInputForNonActive)
                {
                    switchNextFrame_DisableInputForNonActive = false;
                    foreach (var stateGroupKeyValue in singleton.stateGroups)
                    {
                        var stateGroup = stateGroupKeyValue.Value;
                        if (stateGroup != activeStateGroup) stateGroup.updateInput = false;
                    }
                }
            }

            // gather input from api
            int controllerCount;
            bool leftSet, rightSet;
            int leftSetIndex, rightSetIndex;
            SideToSet sideToSet;
            if (!api.GatherInput(activeStateGroup.state_controllers, out controllerCount, out leftSet, out leftSetIndex, out rightSet, out rightSetIndex, out sideToSet)) return;

            // update non-active state groups
            foreach (var stateGroupKeyValue in singleton.stateGroups)
            {
                var stateGroup = stateGroupKeyValue.Value;
                if (stateGroup != activeStateGroup)
                {
                    if (stateGroup.updateInput) UpdateStateGroupToExistingInput(activeStateGroup, stateGroup);
				}
            }

            // track controller connection/disconnections
            if (lastControllerCount != controllerCount)
            {
                foreach (var stateGroupKeyValue in stateGroups)
                {
                    var stateGroup = stateGroupKeyValue.Value;
                    for (int i = 0; i != stateGroup.state_controllers.Length; ++i)
                    {
                        var currentController = stateGroup.state_controllers[i];
                        var lastController = stateGroup.state_controllers_last[i];
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
                }
                lastControllerCount = controllerCount;
            }

            // update all state groups
            foreach (var stateGroupKeyValue in stateGroups)
            {
                var stateGroup = stateGroupKeyValue.Value;

                // if input disabled update as no input
                if (!stateGroup.updateInput)
                {
                    UpdateStateGroupToNoInput(stateGroup);
                    continue;
				}

                // store last controller state
                Array.Copy(stateGroup.state_controllers, stateGroup.state_controllers_last, stateGroup.state_controllers.Length);

                // if left or right not known use controller index as side
                if (sideToSet == SideToSet.Left || sideToSet == SideToSet.Both) stateGroup.state_controllerLeft = stateGroup.state_controllers[leftSetIndex];
                if (sideToSet == SideToSet.Right || sideToSet == SideToSet.Both) stateGroup.state_controllerRight = stateGroup.state_controllers[rightSetIndex];

                // null memory if no state
                if (!leftSet) stateGroup.state_controllerLeft = new XRControllerState();
                if (!rightSet) stateGroup.state_controllerRight = new XRControllerState();

                // buffer special controller states
                if (controllerCount != 0) stateGroup.state_controllerFirst = stateGroup.state_controllers[0];
                else stateGroup.state_controllerFirst = new XRControllerState();

                stateGroup.state_controllerMerged = new XRControllerState()
                {
                    id = state_controllerMerged_ID// keep a constant ID
                };
                for (uint i = 0; i != controllerCount; ++i)
                {
                    var controllerState = stateGroup.state_controllers[i];
                    if (controllerState.connected) stateGroup.state_controllerMerged.connected = true;

                    controllerState.buttonTrigger.Merge(ref stateGroup.state_controllerMerged.buttonTrigger);
                    controllerState.buttonJoystick.Merge(ref stateGroup.state_controllerMerged.buttonJoystick);
                    controllerState.buttonJoystick2.Merge(ref stateGroup.state_controllerMerged.buttonJoystick2);
                    controllerState.buttonGrip.Merge(ref stateGroup.state_controllerMerged.buttonGrip);
                    controllerState.buttonMenu.Merge(ref stateGroup.state_controllerMerged.buttonMenu);

                    controllerState.button1.Merge(ref stateGroup.state_controllerMerged.button1);
                    controllerState.button2.Merge(ref stateGroup.state_controllerMerged.button2);
                    controllerState.button3.Merge(ref stateGroup.state_controllerMerged.button3);
                    controllerState.button4.Merge(ref stateGroup.state_controllerMerged.button4);

                    controllerState.touchTrigger.Merge(ref stateGroup.state_controllerMerged.touchTrigger);
                    controllerState.touchJoystick.Merge(ref stateGroup.state_controllerMerged.touchJoystick);
                    controllerState.touchJoystick2.Merge(ref stateGroup.state_controllerMerged.touchJoystick2);
                    controllerState.touchGrip.Merge(ref stateGroup.state_controllerMerged.touchGrip);
                    controllerState.touchMenu.Merge(ref stateGroup.state_controllerMerged.touchMenu);

                    controllerState.touch1.Merge(ref stateGroup.state_controllerMerged.touch1);
                    controllerState.touch2.Merge(ref stateGroup.state_controllerMerged.touch2);
                    controllerState.touch3.Merge(ref stateGroup.state_controllerMerged.touch3);
                    controllerState.touch4.Merge(ref stateGroup.state_controllerMerged.touch4);

                    controllerState.trigger.Merge(ref stateGroup.state_controllerMerged.trigger);
                    controllerState.grip.Merge(ref stateGroup.state_controllerMerged.grip);
                    controllerState.joystick.Merge(ref stateGroup.state_controllerMerged.joystick);
                    controllerState.joystick2.Merge(ref stateGroup.state_controllerMerged.joystick2);
                }

                // fire events
                string name = stateGroup.name;

                // <<< buttons
                TestButtonEvent(ButtonTriggerOnEvent, ButtonTriggerDownEvent, ButtonTriggerUpEvent, ref stateGroup.state_controllerRight.buttonTrigger, XRControllerSide.Right, name);
                TestButtonEvent(ButtonTriggerOnEvent, ButtonTriggerDownEvent, ButtonTriggerUpEvent, ref stateGroup.state_controllerLeft.buttonTrigger, XRControllerSide.Left, name);

                TestButtonEvent(ButtonJoystickOnEvent, ButtonJoystickDownEvent, ButtonJoystickUpEvent, ref stateGroup.state_controllerRight.buttonJoystick, XRControllerSide.Right, name);
                TestButtonEvent(ButtonJoystickOnEvent, ButtonJoystickDownEvent, ButtonJoystickUpEvent, ref stateGroup.state_controllerLeft.buttonJoystick, XRControllerSide.Left, name);

                TestButtonEvent(ButtonJoystick2OnEvent, ButtonJoystick2DownEvent, ButtonJoystick2UpEvent, ref stateGroup.state_controllerRight.buttonJoystick2, XRControllerSide.Right, name);
                TestButtonEvent(ButtonJoystick2OnEvent, ButtonJoystick2DownEvent, ButtonJoystick2UpEvent, ref stateGroup.state_controllerLeft.buttonJoystick2, XRControllerSide.Left, name);

                TestButtonEvent(ButtonGripOnEvent, ButtonGripDownEvent, ButtonGripUpEvent, ref stateGroup.state_controllerRight.buttonGrip, XRControllerSide.Right, name);
                TestButtonEvent(ButtonGripOnEvent, ButtonGripDownEvent, ButtonGripUpEvent, ref stateGroup.state_controllerLeft.buttonGrip, XRControllerSide.Left, name);

                TestButtonEvent(ButtonMenuOnEvent, ButtonMenuDownEvent, ButtonMenuUpEvent, ref stateGroup.state_controllerRight.buttonMenu, XRControllerSide.Right, name);
                TestButtonEvent(ButtonMenuOnEvent, ButtonMenuDownEvent, ButtonMenuUpEvent, ref stateGroup.state_controllerLeft.buttonMenu, XRControllerSide.Left, name);

                TestButtonEvent(Button1OnEvent, Button1DownEvent, Button1UpEvent, ref stateGroup.state_controllerRight.button1, XRControllerSide.Right, name);
                TestButtonEvent(Button1OnEvent, Button1DownEvent, Button1UpEvent, ref stateGroup.state_controllerLeft.button1, XRControllerSide.Left, name);

                TestButtonEvent(Button2OnEvent, Button2DownEvent, Button2UpEvent, ref stateGroup.state_controllerRight.button2, XRControllerSide.Right, name);
                TestButtonEvent(Button2OnEvent, Button2DownEvent, Button2UpEvent, ref stateGroup.state_controllerLeft.button2, XRControllerSide.Left, name);

                TestButtonEvent(Button3OnEvent, Button3DownEvent, Button3UpEvent, ref stateGroup.state_controllerRight.button3, XRControllerSide.Right, name);
                TestButtonEvent(Button3OnEvent, Button3DownEvent, Button3UpEvent, ref stateGroup.state_controllerLeft.button3, XRControllerSide.Left, name);

                TestButtonEvent(Button4OnEvent, Button4DownEvent, Button4UpEvent, ref stateGroup.state_controllerRight.button4, XRControllerSide.Right, name);
                TestButtonEvent(Button4OnEvent, Button4DownEvent, Button4UpEvent, ref stateGroup.state_controllerLeft.button4, XRControllerSide.Left, name);

                // <<< touch
                TestButtonEvent(TouchTriggerOnEvent, TouchTriggerDownEvent, TouchTriggerUpEvent, ref stateGroup.state_controllerRight.touchTrigger, XRControllerSide.Right, name);
                TestButtonEvent(TouchTriggerOnEvent, TouchTriggerDownEvent, TouchTriggerUpEvent, ref stateGroup.state_controllerLeft.touchTrigger, XRControllerSide.Left, name);

                TestButtonEvent(TouchJoystickOnEvent, TouchJoystickDownEvent, TouchJoystickUpEvent, ref stateGroup.state_controllerRight.touchJoystick, XRControllerSide.Right, name);
                TestButtonEvent(TouchJoystickOnEvent, TouchJoystickDownEvent, TouchJoystickUpEvent, ref stateGroup.state_controllerLeft.touchJoystick, XRControllerSide.Left, name);

                TestButtonEvent(TouchJoystick2OnEvent, TouchJoystick2DownEvent, TouchJoystick2UpEvent, ref stateGroup.state_controllerRight.touchJoystick2, XRControllerSide.Right, name);
                TestButtonEvent(TouchJoystick2OnEvent, TouchJoystick2DownEvent, TouchJoystick2UpEvent, ref stateGroup.state_controllerLeft.touchJoystick2, XRControllerSide.Left, name);

                TestButtonEvent(TouchGripOnEvent, TouchGripDownEvent, TouchGripUpEvent, ref stateGroup.state_controllerRight.touchGrip, XRControllerSide.Right, name);
                TestButtonEvent(TouchGripOnEvent, TouchGripDownEvent, TouchGripUpEvent, ref stateGroup.state_controllerLeft.touchGrip, XRControllerSide.Left, name);

                TestButtonEvent(TouchMenuOnEvent, TouchMenuDownEvent, TouchMenuUpEvent, ref stateGroup.state_controllerRight.touchMenu, XRControllerSide.Right, name);
                TestButtonEvent(TouchMenuOnEvent, TouchMenuDownEvent, TouchMenuUpEvent, ref stateGroup.state_controllerLeft.touchMenu, XRControllerSide.Left, name);

                TestButtonEvent(Touch1OnEvent, Touch1DownEvent, Touch1UpEvent, ref stateGroup.state_controllerRight.touch1, XRControllerSide.Right, name);
                TestButtonEvent(Touch1OnEvent, Touch1DownEvent, Touch1UpEvent, ref stateGroup.state_controllerLeft.touch1, XRControllerSide.Left, name);

                TestButtonEvent(Touch2OnEvent, Touch2DownEvent, Touch2UpEvent, ref stateGroup.state_controllerRight.touch2, XRControllerSide.Right, name);
                TestButtonEvent(Touch2OnEvent, Touch2DownEvent, Touch2UpEvent, ref stateGroup.state_controllerLeft.touch2, XRControllerSide.Left, name);

                TestButtonEvent(Touch3OnEvent, Touch3DownEvent, Touch3UpEvent, ref stateGroup.state_controllerRight.touch3, XRControllerSide.Right, name);
                TestButtonEvent(Touch3OnEvent, Touch3DownEvent, Touch3UpEvent, ref stateGroup.state_controllerLeft.touch3, XRControllerSide.Left, name);

                TestButtonEvent(Touch4OnEvent, Touch4DownEvent, Touch4UpEvent, ref stateGroup.state_controllerRight.touch4, XRControllerSide.Right, name);
                TestButtonEvent(Touch4OnEvent, Touch4DownEvent, Touch4UpEvent, ref stateGroup.state_controllerLeft.touch4, XRControllerSide.Left, name);

                // <<< analogs
                TestAnalogEvent(TriggerActiveEvent, ref stateGroup.state_controllerRight.trigger, XRControllerSide.Right, name);
                TestAnalogEvent(TriggerActiveEvent, ref stateGroup.state_controllerLeft.trigger, XRControllerSide.Left, name);

                TestAnalogEvent(GripActiveEvent, ref stateGroup.state_controllerRight.grip, XRControllerSide.Right, name);
                TestAnalogEvent(GripActiveEvent, ref stateGroup.state_controllerLeft.grip, XRControllerSide.Left, name);

                // <<< joysticks
                TestJoystickEvent(JoystickActiveEvent, ref stateGroup.state_controllerRight.joystick, XRControllerSide.Right, name);
                TestJoystickEvent(JoystickActiveEvent, ref stateGroup.state_controllerLeft.joystick, XRControllerSide.Left, name);

                TestJoystickEvent(Joystick2ActiveEvent, ref stateGroup.state_controllerRight.joystick2, XRControllerSide.Right, name);
                TestJoystickEvent(Joystick2ActiveEvent, ref stateGroup.state_controllerLeft.joystick2, XRControllerSide.Left, name);
            }
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
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton ButtonTrigger(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.buttonTrigger;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.buttonTrigger;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.buttonTrigger;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.buttonTrigger;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton ButtonJoystick(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.buttonJoystick;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.buttonJoystick;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.buttonJoystick;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.buttonJoystick;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton ButtonJoystick2(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.buttonJoystick2;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.buttonJoystick2;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.buttonJoystick2;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.buttonJoystick2;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton ButtonGrip(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.buttonGrip;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.buttonGrip;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.buttonGrip;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.buttonGrip;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton ButtonMenu(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.buttonMenu;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.buttonMenu;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.buttonMenu;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.buttonMenu;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Button1(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.button1;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.button1;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.button1;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.button1;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Button2(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.button2;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.button2;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.button2;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.button2;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Button3(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.button3;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.button3;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.button3;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.button3;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Button4(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.button4;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.button4;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.button4;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.button4;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton TouchTrigger(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.touchTrigger;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.touchTrigger;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.touchTrigger;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.touchTrigger;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton TouchJoystick(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.touchJoystick;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.touchJoystick;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.touchJoystick;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.touchJoystick;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton TouchJoystick2(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.touchJoystick2;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.touchJoystick2;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.touchJoystick2;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.touchJoystick2;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton TouchGrip(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.touchGrip;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.touchGrip;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.touchGrip;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.touchGrip;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton TouchMenu(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.touchMenu;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.touchMenu;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.touchMenu;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.touchMenu;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Touch1(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.touch1;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.touch1;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.touch1;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.touch1;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Touch2(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.touch2;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.touch2;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.touch2;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.touch2;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Touch3(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.touch3;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.touch3;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.touch3;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.touch3;
            }
            throw new NotImplementedException();
        }

        public static XRControllerButton Touch4(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerButton();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.touch4;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.touch4;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.touch4;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.touch4;
            }
            throw new NotImplementedException();
        }

        public static XRControllerAnalog Trigger(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerAnalog();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.trigger;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.trigger;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.trigger;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.trigger;
            }
            throw new NotImplementedException();
        }

        public static XRControllerAnalog Grip(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerAnalog();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.grip;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.grip;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.grip;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.grip;
            }
            throw new NotImplementedException();
        }

        public static XRControllerJoystick Joystick(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerJoystick();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.joystick;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.joystick;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.joystick;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.joystick;
            }
            throw new NotImplementedException();
        }

        public static XRControllerJoystick Joystick2(XRController controller)
        {
            if (singleton == null || !singleton.apiInit) return new XRControllerJoystick();
            switch (controller)
            {
                case XRController.First: return singleton.activeStateGroup.state_controllerFirst.joystick2;
                case XRController.Left: return singleton.activeStateGroup.state_controllerLeft.joystick2;
                case XRController.Right: return singleton.activeStateGroup.state_controllerRight.joystick2;
                case XRController.Merged: return singleton.activeStateGroup.state_controllerMerged.joystick2;
            }
            throw new NotImplementedException();
        }

        public static bool SetRumble(XRControllerRumbleSide controller, float strength, float duration = .1f)
        {
            if (singleton == null || !singleton.apiInit) return false;
            return singleton.api.SetRumble(controller, strength, duration);
        }

        public static XRInputControllerType GetControllerType(XRInputControllerTypeSide controller)
        {
            if (singleton == null || !singleton.apiInit) return XRInputControllerType.Unknown;
            switch (controller)
            {
                case XRInputControllerTypeSide.First: return singleton.activeStateGroup.state_controllerFirst.type;
                case XRInputControllerTypeSide.Left: return singleton.activeStateGroup.state_controllerLeft.type;
                case XRInputControllerTypeSide.Right: return singleton.activeStateGroup.state_controllerRight.type;
            }
            throw new NotImplementedException();
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
        HTCVive,
        HTCViveCosmos,
        HTCViveWave,
        ValveIndex,
        WMR,
        WMR_G2
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
        public static float virtualButtonThreshold = .85f;

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