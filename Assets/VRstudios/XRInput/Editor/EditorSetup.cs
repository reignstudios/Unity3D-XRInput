using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.XR.Management;

namespace VRstudios
{
    static class EditorSetup
    {
        /*[MenuItem("Assets/VRstudios/XRInput/Enable for Steam")]
        public static void EnableForOpenVR()
        {
            EnsureSingleExists(new string[] { "VRSTUDIOS_XRINPUT_OPENVR" }, null);
        }

        [MenuItem("Assets/VRstudios/XRInput/Enable for Generic-Unity-Input")]
        public static void EnableForOculus()
        {
            EnsureSingleExists(null, new string[] { "VRSTUDIOS_XRINPUT_OPENVR" });
        }*/

        private static void EnsureSingleExists(string[] definesToAdd, string[] definesToRemove)
        {
            // get defaults
            bool changes = false;
            PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, out string[] defines);
            var definesList = new List<string>(defines);

            // check if we need to add defines
            if (definesToAdd != null)
            {
                foreach (string define in definesToAdd)
                {
                    if (!definesList.Contains(define))
                    {
                        definesList.Add(define);
                        changes = true;
                    }
                }
            }

            // check if we need to remove defines
            if (definesToRemove != null)
            {
                foreach (string define in definesToRemove)
                {
                    if (definesList.Contains(define)) definesList.Remove(define);
                    changes = true;
                }
			}

            // apply changes if needed
            if (changes) PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, definesList.ToArray());
        }
    }
}