using UnityEngine;

namespace TrafficRider.Gameplay
{
    public class CrashEffects : MonoBehaviour
    {
        private ParticleSystem _particles;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = 0.35f;
            _audioSource.clip = BuildBeep();

            GameObject fx = new GameObject("CrashParticles");
            fx.transform.SetParent(transform);
            _particles = fx.AddComponent<ParticleSystem>();

            var main = _particles.main;
            main.playOnAwake = false;
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            main.loop = false;
            main.duration = 0.5f;
            main.startLifetime = 0.4f;
            main.startSpeed = 4f;
            main.startSize = 0.2f;
            main.startColor = new Color(1f, 0.6f, 0.1f);
            main.maxParticles = 80;

            var emission = _particles.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

            var shape = _particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void Play(Vector3 position)
        {
            transform.position = position;
            _particles.Play();
            _audioSource.Play();
        }

        private AudioClip BuildBeep()
        {
            int sampleRate = 44100;
            float length = 0.25f;
            int sampleCount = Mathf.CeilToInt(sampleRate * length);
            float[] data = new float[sampleCount];
            float freq = 520f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float env = Mathf.Exp(-6f * t);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env;
            }

            AudioClip clip = AudioClip.Create("CrashBeep", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
