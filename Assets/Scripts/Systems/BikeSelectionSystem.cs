using System.Collections.Generic;
using System.Linq;
using TrafficRider.Data;
using UnityEngine;

namespace TrafficRider.Systems
{
    public class BikeSelectionSystem
    {
        private readonly GameConfig _config;
        private readonly SaveData _save;

        public BikeSelectionSystem(GameConfig config, SaveData save)
        {
            _config = config;
            _save = save;
        }

        public IEnumerable<BikeConfig> GetBikes() => _config.bikes;

        public BikeConfig GetSelectedBike()
        {
            return _config.bikes.FirstOrDefault(b => b.id == _save.selectedBikeId) ?? _config.bikes.FirstOrDefault();
        }

        public bool IsOwned(string id) => _save.ownedBikes.Contains(id);

        public bool TryBuyBike(string id)
        {
            BikeConfig bike = _config.bikes.FirstOrDefault(b => b.id == id);
            if (bike == null) return false;
            if (IsOwned(id)) return true;
            if (_save.coins < bike.price) return false;

            _save.coins -= bike.price;
            _save.ownedBikes.Add(id);
            SaveSystem.Save(_save);
            return true;
        }

        public void SelectBike(string id)
        {
            if (!IsOwned(id)) return;
            _save.selectedBikeId = id;
            SaveSystem.Save(_save);
        }
    }
}
