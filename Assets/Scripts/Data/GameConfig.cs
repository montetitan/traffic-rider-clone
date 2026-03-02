using System;
using System.Collections.Generic;

namespace TrafficRider.Data
{
    [Serializable]
    public class GameConfig
    {
        public List<BikeConfig> bikes = new List<BikeConfig>();
        public List<UpgradeConfig> upgrades = new List<UpgradeConfig>();
        public List<MissionConfig> missions = new List<MissionConfig>();
        public EconomyConfig economy = new EconomyConfig();
    }

    [Serializable]
    public class BikeConfig
    {
        public string id;
        public string name;
        public int price;
        public float maxSpeed;
        public float acceleration;
        public float handling;
        public float brake;
        public bool unlockedByDefault;
    }

    [Serializable]
    public class UpgradeConfig
    {
        public string id;
        public string name;
        public int maxLevel;
        public float valuePerLevel;
        public int basePrice;
    }

    [Serializable]
    public class MissionConfig
    {
        public string id;
        public string name;
        public string type; // distance, overtake
        public int target;
        public int reward;
    }

    [Serializable]
    public class EconomyConfig
    {
        public int coinsPerMeter = 1;
        public int coinsPerOvertake = 25;
    }
}
