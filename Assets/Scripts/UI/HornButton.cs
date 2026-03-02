using UnityEngine;
using UnityEngine.EventSystems;

namespace TrafficRider.UI
{
    public class HornButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public AudioSource source;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (source == null) return;
            source.Play();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (source == null) return;
            source.Stop();
        }
    }
}
