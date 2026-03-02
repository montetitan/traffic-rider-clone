using System.Collections.Generic;
using System.Linq;
using TrafficRider.Data;
using UnityEngine;

namespace TrafficRider.Systems
{
    public class MissionSystem
    {
        private readonly GameConfig _config;
        private readonly SaveData _save;

        public MissionSystem(GameConfig config, SaveData save)
        {
            _config = config;
            _save = save;
        }

        public IEnumerable<MissionProgress> GetProgress() => _save.missionProgress;

        public MissionConfig GetConfig(string id) => _config.missions.FirstOrDefault(m => m.id == id);

        public int AddProgress(string type, int amount, bool onlySelected = false, string selectedId = "")
        {
            int rewardEarned = 0;
            foreach (MissionConfig mission in _config.missions)
            {
                if (mission.type != type) continue;
                if (onlySelected && mission.id != selectedId) continue;

                MissionProgress progress = _save.missionProgress.FirstOrDefault(m => m.id == mission.id);
                if (progress == null || progress.completed) continue;

                progress.value += amount;
                if (progress.value >= mission.target)
                {
                    progress.completed = true;
                    rewardEarned += mission.reward;
                }
            }

            return rewardEarned;
        }
    }
}
