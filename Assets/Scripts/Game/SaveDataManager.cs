#nullable enable
using System.Collections.Generic;
using Domain.Model;
using Domain.Model.Map;
using Domain.Model.Memento;
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace Game
{
    public record GlobalSaveData(
        GlobalStatisticsMemento GlobalStatistics,
        Dictionary<string, int> GlobalSettings);
    public record SaveData(
        WorldMemento World,
        Dictionary<Id<IMap>, MapMemento> Maps,
        StatisticsMemento Statistics,
        Dictionary<string, int> Settings,
        float TurnWaitTime,
        bool IsRollbacked,
        BGM Bgm);
    public class SaveDataManager
    {
        private SQLiteDatabase db;
        private int _saveDataSlot;

        public SaveDataManager(int id)
        {
            db = new SQLiteDatabase();
            _saveDataSlot = id;
        }

        public void SetSaveDataSlot(int id)
        {
            _saveDataSlot = id;
        }

        public bool IsExistSave()
        {
            return db.ExistSave(_saveDataSlot);
        }

        public void SaveLight(int turn)
        {
            Log.Debug("[Save]Start Save Light");
            db.SaveTurn(_saveDataSlot, turn);
            Log.Debug("[Save]End Save Light");
        }

        public void SaveFull(GlobalSaveData globalSaveData, SaveData saveData)
        {
            Log.Debug("[Save]Start Save"); 
            db.SaveGlobalStatistics(JsonUtility.ToJson(globalSaveData.GlobalStatistics));
            db.SaveGlobalSettings(globalSaveData.GlobalSettings);

            db.Save(_saveDataSlot, JsonUtility.ToJson(saveData.World), saveData.TurnWaitTime, (int)saveData.Bgm);
            db.SaveTurn(_saveDataSlot, saveData.Statistics.Turn);
            foreach (var map in saveData.Maps)
            {
                db.SaveMap(_saveDataSlot, map.Key.ToString(), JsonUtility.ToJson(map.Value));
            }
            db.SaveStatistics(_saveDataSlot, JsonUtility.ToJson(saveData.Statistics));
            db.SaveSettings(_saveDataSlot, saveData.Settings);

            Log.Debug("[Save]End Save");
        }

        public GlobalSaveData? LoadGlobal()
        {
            if (!IsExistSave())
            {
                return null;
            }
            Log.Debug("[Save]Start Load Global");
            var globalStatisticsData = db.LoadGlobalStatistics();
            var globalStatistics = JsonUtility.FromJson<GlobalStatisticsMemento>(globalStatisticsData);
            var globalSettings = db.LoadGlobalSettings();
            Log.Debug("[Save]End Load Global");
            return new GlobalSaveData(globalStatistics, globalSettings);
        }

        public SaveData? Load()
        {
            if (!IsExistSave())
            {
                return null;
            }
            Log.Debug("[Save]Start Load");
            var (worldData, turnWaitTime, bgm) = db.Load(_saveDataSlot);
            var world = JsonUtility.FromJson<WorldMemento>(worldData);
            Dictionary<Id<IMap>, MapMemento> maps = new();
            foreach (var mapId in world.MapIds)
            {
                var mapData = db.LoadMap(_saveDataSlot, mapId.ToString());
                maps.Add(mapId, JsonUtility.FromJson<MapMemento>(mapData));
            }
            var statisticsData = db.LoadStatistics(_saveDataSlot);
            var statistics = JsonUtility.FromJson<StatisticsMemento>(statisticsData);

            var settings = db.LoadSettings();

            var latestTurn = LoadLatestTurn();
            var isRollbacked = latestTurn != statistics.Turn;

            Log.Debug("[Save]End Load");
            return new SaveData(world, maps, statistics, settings, turnWaitTime, isRollbacked, (BGM)bgm);
        }

        private int LoadLatestTurn()
        {
            if (!db.ExistSave(_saveDataSlot))
            {
                return 0;
            }
            return db.LoadLatestTurn(_saveDataSlot);
        }

        public void ClearSave()
        {
            db.ClearSave();
        }
    }
}