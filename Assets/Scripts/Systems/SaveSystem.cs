using System;
using System.Collections.Generic;
using TrafficRider.Data;
using UnityEngine;

namespace TrafficRider.Systems
{
    [Serializable]
    public class SaveData
    {
        public int coins = 0;
        public string selectedBikeId = "bike_basic";
        public List<string> ownedBikes = new List<string>();
        public List<UpgradeLevel> upgrades = new List<UpgradeLevel>();
        public List<MissionProgress> missionProgress = new List<MissionProgress>();
        public float topScoreKm = 0f;
        public string googlePlayId = "GPlay_User";
        public bool twoWayTraffic = true;
        public string orientation = "auto";
        public string background = "city";
        public string quality = "auto";
        public string selectedMissionId = "m_distance_1";
        public string selectedMode = "endless";
    }

    [Serializable]
    public class UpgradeLevel
    {
        public string id;
        public int level;
    }

    [Serializable]
    public class MissionProgress
    {
        public string id;
        public int value;
        public bool completed;
    }

    public static class SaveSystem
    {
        private const string SaveKey = "traffic_rider_save";

        public static SaveData Load(GameConfig config)
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                SaveData fresh = new SaveData();
                foreach (BikeConfig bike in config.bikes)
                {
                    if (bike.unlockedByDefault)
                    {
                        fresh.ownedBikes.Add(bike.id);
                    }
                }
                EnsureUpgradeEntries(fresh, config);
                EnsureMissionEntries(fresh, config);
                return fresh;
            }

            string json = PlayerPrefs.GetString(SaveKey);
            SaveData data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            EnsureUpgradeEntries(data, config);
            EnsureMissionEntries(data, config);
            return data;
        }

        public static void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        private static void EnsureUpgradeEntries(SaveData data, GameConfig config)
        {
            foreach (UpgradeConfig upgrade in config.upgrades)
            {
                if (!data.upgrades.Exists(u => u.id == upgrade.id))
                {
                    data.upgrades.Add(new UpgradeLevel { id = upgrade.id, level = 0 });
                }
            }
        }

        private static void EnsureMissionEntries(SaveData data, GameConfig config)
        {
            foreach (MissionConfig mission in config.missions)
            {
                if (!data.missionProgress.Exists(m => m.id == mission.id))
                {
                    data.missionProgress.Add(new MissionProgress { id = mission.id, value = 0, completed = false });
                }
            }
        }
    }
}
