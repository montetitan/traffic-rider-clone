using System;
using System.Collections;
using System.IO;
using TrafficRider.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace TrafficRider.Systems
{
    public class ConfigLoader : MonoBehaviour
    {
        public const string ConfigFileName = "game_config.json";

        public IEnumerator LoadConfig(Action<GameConfig> onLoaded)
        {
            string path = Path.Combine(Application.streamingAssetsPath, ConfigFileName);

            if (path.Contains("://") || path.Contains(":\\"))
            {
                using (UnityWebRequest request = UnityWebRequest.Get(path))
                {
                    yield return request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("Failed to load config: " + request.error);
                        onLoaded?.Invoke(new GameConfig());
                        yield break;
                    }
                    GameConfig config = JsonUtility.FromJson<GameConfig>(request.downloadHandler.text);
                    onLoaded?.Invoke(config ?? new GameConfig());
                    yield break;
                }
            }

            if (!File.Exists(path))
            {
                Debug.LogError("Config not found at " + path);
                onLoaded?.Invoke(new GameConfig());
                yield break;
            }

            string json = File.ReadAllText(path);
            GameConfig data = JsonUtility.FromJson<GameConfig>(json);
            onLoaded?.Invoke(data ?? new GameConfig());
        }
    }
}
