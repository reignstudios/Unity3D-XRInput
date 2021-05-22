//#if UNITY_EDITOR
//using System.Collections.Generic;
//using UnityEditor;

//namespace VRstudios
//{
//	static class EditorSetup
//    {
//        /*[DidReloadScripts]
//        private static void ValidateCompilerDefines()
//        {
//           //Debug.Log(typeof(Unity.XR.OpenVR.OpenVRLoader).AssemblyQualifiedName);
//           //Debug.Log(typeof(UnityEngine.XR.OpenXR.OpenXRLoader).AssemblyQualifiedName);

//            var target = EditorUserBuildSettings.activeBuildTarget;
//            var group = BuildPipeline.GetBuildTargetGroup(target);

//            // openvr loader
//            var typeInfo = Type.GetType("Unity.XR.OpenVR.OpenVRLoader, Unity.XR.OpenVR");
//            if (typeInfo != null) EnsureCompilerDefines(group, new string[] {"XRINPUT_OPENVR_LOADER"}, null);
//            else EnsureCompilerDefines(group, null, new string[] { "XRINPUT_OPENVR_LOADER" });

//            // openxr loader
//            typeInfo = Type.GetType("UnityEngine.XR.OpenXR.OpenXRLoader, Unity.XR.OpenXR");
//            if (typeInfo != null) EnsureCompilerDefines(group, new string[] { "XRINPUT_OPENXR_LOADER" }, null);
//            else EnsureCompilerDefines(group, null, new string[] { "XRINPUT_OPENXR_LOADER" });
//        }*/

//        /*private static void EnsureCompilerDefines(BuildTargetGroup group, string[] definesToAdd, string[] definesToRemove)
//        {
//            //Debug.Log("Adding defines to group: " + group.ToString());
//            //if (definesToAdd != null) foreach(string define in definesToAdd) Debug.Log("- DEFINE: " + define);
//            //Debug.Log("Removing defines from group: " + group.ToString());
//            //if (definesToRemove != null) foreach (string define in definesToRemove) Debug.Log("- DEFINE: " + define);

//            // get defaults
//            bool changes = false;
//            PlayerSettings.GetScriptingDefineSymbolsForGroup(group, out string[] defines);
//            var definesList = new List<string>(defines);

//            // check if we need to add defines
//            if (definesToAdd != null)
//            {
//                foreach (string define in definesToAdd)
//                {
//                    if (!definesList.Contains(define))
//                    {
//                        definesList.Add(define);
//                        changes = true;
//                    }
//                }
//            }

//            // check if we need to remove defines
//            if (definesToRemove != null)
//            {
//                foreach (string define in definesToRemove)
//                {
//                    if (definesList.Contains(define)) definesList.Remove(define);
//                    changes = true;
//                }
//			}

//            // apply changes if needed
//            if (changes) PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definesList.ToArray());
//        }*/
//    }
//}
//#endif