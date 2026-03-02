using System.Linq;
using TrafficRider.Data;
using UnityEngine;

namespace TrafficRider.Systems
{
    public class UpgradeSystem
    {
        private readonly GameConfig _config;
        private readonly SaveData _save;

        public UpgradeSystem(GameConfig config, SaveData save)
        {
            _config = config;
            _save = save;
        }

        public int GetLevel(string id)
        {
            UpgradeLevel level = _save.upgrades.FirstOrDefault(u => u.id == id);
            return level != null ? level.level : 0;
        }

        public float GetModifier(string id)
        {
            UpgradeConfig upgrade = _config.upgrades.FirstOrDefault(u => u.id == id);
            if (upgrade == null) return 0f;
            return GetLevel(id) * upgrade.valuePerLevel;
        }

        public bool TryBuyUpgrade(string id)
        {
            UpgradeConfig upgrade = _config.upgrades.FirstOrDefault(u => u.id == id);
            if (upgrade == null) return false;

            UpgradeLevel level = _save.upgrades.FirstOrDefault(u => u.id == id);
            if (level == null) return false;

            if (level.level >= upgrade.maxLevel) return false;

            int price = upgrade.basePrice * (level.level + 1);
            if (_save.coins < price) return false;

            _save.coins -= price;
            level.level += 1;
            SaveSystem.Save(_save);
            return true;
        }
    }
}
