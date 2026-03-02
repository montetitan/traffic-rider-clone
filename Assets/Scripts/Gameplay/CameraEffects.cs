using System.Collections;
using UnityEngine;

namespace TrafficRider.Gameplay
{
    public class CameraEffects : MonoBehaviour
    {
        public float shakeDuration = 0.5f;
        public float shakeMagnitude = 0.25f;

        private Vector3 _baseLocalPos;
        private Coroutine _shakeRoutine;

        private void Awake()
        {
            _baseLocalPos = transform.localPosition;
        }

        public void PlayCrashShake()
        {
            if (_shakeRoutine != null)
            {
                StopCoroutine(_shakeRoutine);
            }
            _shakeRoutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            float timer = 0f;
            while (timer < shakeDuration)
            {
                timer += Time.unscaledDeltaTime;
                Vector2 offset = Random.insideUnitCircle * shakeMagnitude;
                transform.localPosition = _baseLocalPos + new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }

            transform.localPosition = _baseLocalPos;
            _shakeRoutine = null;
        }
    }
}
