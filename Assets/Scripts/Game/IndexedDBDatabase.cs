#nullable enable
#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Unity.Logging;
using UnityEngine;

namespace Game
{
    public class IndexedDBDatabase : ISaveDatabase
    {
        [Serializable]
        private class SaveRowJson
        {
            public string text = "";
            public float turnWaitTime;
            public int bgm;
        }

        [Serializable]
        private class SettingEntryJson
        {
            public string name = "";
            public int value;
        }

        [Serializable]
        private class SettingEntriesJson
        {
            public SettingEntryJson[] entries = Array.Empty<SettingEntryJson>();
        }

        private bool _isReady;

        public IndexedDBDatabase()
        {
        }

        public async UniTask EnsureReadyAsync()
        {
            if (_isReady)
                return;

            IndexedDB_Init();
            while (IndexedDB_GetInitState() == 0)
                await UniTask.Yield();

            if (IndexedDB_GetInitState() < 0)
                throw new InvalidOperationException("IndexedDB initialization failed.");

            _isReady = true;
            Log.Debug("IndexedDB init done");
        }

        public void Save(int save_id, string saveData, float turnWaitTime, int bgm) =>
            SaveAsync(save_id, saveData, turnWaitTime, bgm).GetAwaiter().GetResult();

        public async UniTask SaveAsync(int save_id, string saveData, float turnWaitTime, int bgm)
        {
            await EnsureReadyAsync();
            IndexedDB_Save(save_id, saveData, turnWaitTime, bgm);
            await WaitOperationAsync();
        }

        public void SaveTurn(int save_id, int turn) =>
            SaveTurnAsync(save_id, turn).GetAwaiter().GetResult();

        public async UniTask SaveTurnAsync(int save_id, int turn)
        {
            await EnsureReadyAsync();
            IndexedDB_SaveTurn(save_id, turn);
            await WaitOperationAsync();
        }

        public void SaveMap(int save_id, string map_id, string mapData) =>
            SaveMapAsync(save_id, map_id, mapData).GetAwaiter().GetResult();

        public async UniTask SaveMapAsync(int save_id, string map_id, string mapData)
        {
            await EnsureReadyAsync();
            IndexedDB_SaveMap(save_id, map_id, mapData);
            await WaitOperationAsync();
        }

        public void SaveGlobalStatistics(string globalStatisticsData) =>
            SaveGlobalStatisticsAsync(globalStatisticsData).GetAwaiter().GetResult();

        public async UniTask SaveGlobalStatisticsAsync(string globalStatisticsData)
        {
            await EnsureReadyAsync();
            IndexedDB_SaveGlobalStatistics(globalStatisticsData);
            await WaitOperationAsync();
        }

        public void SaveStatistics(int save_id, string statisticsData) =>
            SaveStatisticsAsync(save_id, statisticsData).GetAwaiter().GetResult();

        public async UniTask SaveStatisticsAsync(int save_id, string statisticsData)
        {
            await EnsureReadyAsync();
            IndexedDB_SaveStatistics(save_id, statisticsData);
            await WaitOperationAsync();
        }

        public void SaveGlobalSettings(Dictionary<string, int> globalSettings) =>
            SaveGlobalSettingsAsync(globalSettings).GetAwaiter().GetResult();

        public async UniTask SaveGlobalSettingsAsync(Dictionary<string, int> globalSettings)
        {
            await EnsureReadyAsync();
            foreach (var setting in globalSettings)
            {
                IndexedDB_SaveGlobalSetting(setting.Key, setting.Value);
                await WaitOperationAsync();
            }
        }

        public void SaveSettings(int save_id, Dictionary<string, int> settings) =>
            SaveSettingsAsync(save_id, settings).GetAwaiter().GetResult();

        public async UniTask SaveSettingsAsync(int save_id, Dictionary<string, int> settings)
        {
            await EnsureReadyAsync();
            foreach (var setting in settings)
            {
                IndexedDB_SaveSetting(save_id, setting.Key, setting.Value);
                await WaitOperationAsync();
            }
        }

        public bool ExistGlobal() =>
            ExistGlobalAsync().GetAwaiter().GetResult();

        public async UniTask<bool> ExistGlobalAsync()
        {
            await EnsureReadyAsync();
            IndexedDB_ExistGlobal();
            await WaitOperationAsync();
            return IndexedDB_GetResultInt() != 0;
        }

        public bool ExistSave(int save_id) =>
            ExistSaveAsync(save_id).GetAwaiter().GetResult();

        public async UniTask<bool> ExistSaveAsync(int save_id)
        {
            await EnsureReadyAsync();
            IndexedDB_ExistSave(save_id);
            await WaitOperationAsync();
            return IndexedDB_GetResultInt() != 0;
        }

        public (string world, float turnWaitTime, int bgm) Load(int save_id) =>
            LoadAsync(save_id).GetAwaiter().GetResult();

        public async UniTask<(string world, float turnWaitTime, int bgm)> LoadAsync(int save_id)
        {
            await EnsureReadyAsync();
            IndexedDB_Load(save_id);
            await WaitOperationAsync();
            var json = TakeUtf8String(IndexedDB_GetResultPtr());
            var row = JsonUtility.FromJson<SaveRowJson>(json);
            return (row.text, row.turnWaitTime, row.bgm);
        }

        public int LoadLatestTurn(int save_id) =>
            LoadLatestTurnAsync(save_id).GetAwaiter().GetResult();

        public async UniTask<int> LoadLatestTurnAsync(int save_id)
        {
            await EnsureReadyAsync();
            IndexedDB_LoadLatestTurn(save_id);
            await WaitOperationAsync();
            return IndexedDB_GetResultInt();
        }

        public string LoadMap(int save_id, string map_id) =>
            LoadMapAsync(save_id, map_id).GetAwaiter().GetResult();

        public async UniTask<string> LoadMapAsync(int save_id, string map_id)
        {
            await EnsureReadyAsync();
            IndexedDB_LoadMap(save_id, map_id);
            await WaitOperationAsync();
            return TakeUtf8String(IndexedDB_GetResultPtr());
        }

        public string? LoadGlobalStatistics() =>
            LoadGlobalStatisticsAsync().GetAwaiter().GetResult();

        public async UniTask<string?> LoadGlobalStatisticsAsync()
        {
            await EnsureReadyAsync();
            IndexedDB_LoadGlobalStatistics();
            await WaitOperationAsync();
            var ptr = IndexedDB_GetResultPtr();
            if (ptr == IntPtr.Zero)
                return null;
            return TakeUtf8String(ptr);
        }

        public string? LoadStatistics(int save_id) =>
            LoadStatisticsAsync(save_id).GetAwaiter().GetResult();

        public async UniTask<string?> LoadStatisticsAsync(int save_id)
        {
            await EnsureReadyAsync();
            IndexedDB_LoadStatistics(save_id);
            await WaitOperationAsync();
            var ptr = IndexedDB_GetResultPtr();
            if (ptr == IntPtr.Zero)
                return null;
            return TakeUtf8String(ptr);
        }

        public Dictionary<string, int> LoadGlobalSettings() =>
            LoadGlobalSettingsAsync().GetAwaiter().GetResult();

        public async UniTask<Dictionary<string, int>> LoadGlobalSettingsAsync()
        {
            await EnsureReadyAsync();
            IndexedDB_LoadGlobalSettings();
            await WaitOperationAsync();
            return LoadSettingsFromJson(IndexedDB_GetResultPtr());
        }

        public Dictionary<string, int> LoadSettings() =>
            LoadSettingsAsync().GetAwaiter().GetResult();

        public async UniTask<Dictionary<string, int>> LoadSettingsAsync()
        {
            await EnsureReadyAsync();
            IndexedDB_LoadSettings();
            await WaitOperationAsync();
            return LoadSettingsFromJson(IndexedDB_GetResultPtr());
        }

        public void ClearSave() =>
            ClearSaveAsync().GetAwaiter().GetResult();

        public async UniTask ClearSaveAsync()
        {
            await EnsureReadyAsync();
            IndexedDB_ClearSave();
            await WaitOperationAsync();
        }

        private static async UniTask WaitOperationAsync()
        {
            while (IndexedDB_GetOperationState() == 0)
                await UniTask.Yield();

            if (IndexedDB_GetOperationState() < 0)
                throw new InvalidOperationException("IndexedDB operation failed.");
        }

        private static Dictionary<string, int> LoadSettingsFromJson(IntPtr ptr)
        {
            var settings = new Dictionary<string, int>();
            if (ptr == IntPtr.Zero)
                return settings;

            var json = TakeUtf8String(ptr);
            var entries = JsonUtility.FromJson<SettingEntriesJson>(json);
            foreach (var entry in entries.entries)
                settings[entry.name] = entry.value;
            return settings;
        }

        private static string TakeUtf8String(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return "";

            var value = Marshal.PtrToStringUTF8(ptr) ?? "";
            IndexedDB_Free(ptr);
            return value;
        }

        [DllImport("__Internal")]
        private static extern void IndexedDB_Init();

        [DllImport("__Internal")]
        private static extern int IndexedDB_GetInitState();

        [DllImport("__Internal")]
        private static extern int IndexedDB_GetOperationState();

        [DllImport("__Internal")]
        private static extern IntPtr IndexedDB_GetResultPtr();

        [DllImport("__Internal")]
        private static extern int IndexedDB_GetResultInt();

        [DllImport("__Internal")]
        private static extern void IndexedDB_Save(int saveId, string text, float turnWaitTime, int bgm);

        [DllImport("__Internal")]
        private static extern void IndexedDB_SaveTurn(int saveId, int turn);

        [DllImport("__Internal")]
        private static extern void IndexedDB_SaveMap(int saveId, string mapId, string text);

        [DllImport("__Internal")]
        private static extern void IndexedDB_SaveGlobalStatistics(string text);

        [DllImport("__Internal")]
        private static extern void IndexedDB_SaveStatistics(int saveId, string text);

        [DllImport("__Internal")]
        private static extern void IndexedDB_SaveGlobalSetting(string name, int value);

        [DllImport("__Internal")]
        private static extern void IndexedDB_SaveSetting(int saveId, string name, int value);

        [DllImport("__Internal")]
        private static extern void IndexedDB_ExistSave(int saveId);

        [DllImport("__Internal")]
        private static extern void IndexedDB_ExistGlobal();

        [DllImport("__Internal")]
        private static extern void IndexedDB_Load(int saveId);

        [DllImport("__Internal")]
        private static extern void IndexedDB_LoadLatestTurn(int saveId);

        [DllImport("__Internal")]
        private static extern void IndexedDB_LoadMap(int saveId, string mapId);

        [DllImport("__Internal")]
        private static extern void IndexedDB_LoadGlobalStatistics();

        [DllImport("__Internal")]
        private static extern void IndexedDB_LoadStatistics(int saveId);

        [DllImport("__Internal")]
        private static extern void IndexedDB_LoadGlobalSettings();

        [DllImport("__Internal")]
        private static extern void IndexedDB_LoadSettings();

        [DllImport("__Internal")]
        private static extern void IndexedDB_ClearSave();

        [DllImport("__Internal")]
        private static extern void IndexedDB_Free(IntPtr ptr);
    }
}
#endif
