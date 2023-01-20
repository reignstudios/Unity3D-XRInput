/*******************************************************************************
Copyright © 2015-2022 PICO Technology Co., Ltd.All rights reserved.

NOTICE：All information contained herein is, and remains the property of
PICO Technology Co., Ltd. The intellectual and technical concepts
contained herein are proprietary to PICO Technology Co., Ltd. and may be
covered by patents, patents in process, and are protected by trade secret or
copyright law. Dissemination of this information or reproduction of this
material is strictly forbidden unless prior written permission is obtained from
PICO Technology Co., Ltd.
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Pico.Platform
{
    public partial class CLIB
    {
        public static ulong ppf_Achievements_GetProgressByName(string[] names)
        {
            var namesHandle = new PtrArray(names);
            var result = ppf_Achievements_GetProgressByName(namesHandle.a, names.Length);
            namesHandle.Free();
            return result;
        }

        public static ulong ppf_Achievements_GetDefinitionsByName(string[] names)
        {
            var namesHandle = new PtrArray(names);
            var result = ppf_Achievements_GetDefinitionsByName(namesHandle.a, names.Length);
            namesHandle.Free();
            return result;
        }

        public static ulong ppf_IAP_GetProductsBySKU(string[] names)
        {
            var namesHandle = new PtrArray(names);
            var result = ppf_IAP_GetProductsBySKU(namesHandle.a, names.Length);
            namesHandle.Free();
            return result;
        }

        public static ulong ppf_Leaderboard_GetEntriesByIds(string leaderboardName, int pageSize, int pageIdx, LeaderboardStartAt startAt, string[] userIDs)
        {
            var userIds = new PtrArray(userIDs);
            var result = ppf_Leaderboard_GetEntriesByIds(leaderboardName, pageSize, pageIdx, startAt, userIds.a, (uint) userIDs.Length);
            userIds.Free();
            return result;
        }

        public static ulong ppf_Challenges_GetEntriesByIds(ulong challengeID, global::Pico.Platform.LeaderboardStartAt startAt, string[] userIDs, int pageIdx, int pageSize)
        {
            var userIds = new PtrArray(userIDs);
            var result = ppf_Challenges_GetEntriesByIds(challengeID, startAt, userIds.a, (uint) userIDs.Length, pageIdx, pageSize);
            userIds.Free();
            return result;
        }

        public static ulong ppf_Challenges_Invites(ulong challengeID, string[] userIDs)
        {
            var userIds = new PtrArray(userIDs);
            var result = ppf_Challenges_Invites(challengeID, userIds.a, (uint) userIDs.Length);
            userIds.Free();
            return result;
        }

        public static ulong ppf_User_RequestUserPermissions(string[] permissions)
        {
            var ptrs = new PtrArray(permissions);
            var result = ppf_User_RequestUserPermissions(ptrs.a, permissions.Length);
            ptrs.Free();
            return result;
        }

        public static ulong ppf_Presence_SendInvites(string[] userIDs)
        {
            var ptrs = new PtrArray(userIDs);
            var result = ppf_Presence_SendInvites(ptrs.a, (uint) userIDs.Length);
            ptrs.Free();
            return result;
        }

        public static Dictionary<string, string> DataStoreFromNative(IntPtr ppfDataStore)
        {
            var map = new Dictionary<string, string>();
            var size = (int) ppf_DataStore_GetNumKeys(ppfDataStore);
            for (var i = 0; i < size; i++)
            {
                string key = ppf_DataStore_GetKey(ppfDataStore, i);
                map[key] = ppf_DataStore_GetValue(ppfDataStore, key);
            }

            return map;
        }

        [DllImport("pxrplatformloader", EntryPoint = "ppf_PcInitWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern global::Pico.Platform.PlatformInitializeResult ppf_PcInitWrapper([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Pico.Platform.UTF8Marshaller))] string appId, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Pico.Platform.UTF8Marshaller))] string configJson, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Pico.Platform.UTF8Marshaller))] string logDirectory);

        [DllImport("pxrplatformloader", EntryPoint = "ppf_PcInitAsynchronousWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong ppf_PcInitAsynchronousWrapper([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Pico.Platform.UTF8Marshaller))] string appId, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Pico.Platform.UTF8Marshaller))] string configJson, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Pico.Platform.UTF8Marshaller))] string logDirectory);


        [DllImport("pxrplatformloader", EntryPoint = "ppf_GetLoaderVersion", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ppf_GetLoaderVersion();
    }
}