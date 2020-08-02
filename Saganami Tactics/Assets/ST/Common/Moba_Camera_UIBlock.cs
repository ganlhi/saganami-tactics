using UnityEngine;
using UnityEngine.EventSystems;

namespace ST.Common
{
    public class Moba_Camera_UIBlock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
#pragma warning disable 0649
        [SerializeField]
        private new Moba_Camera camera;
#pragma warning restore 0649
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (camera != null)
            {
                camera.overInteractableUi = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (camera != null)
            {
                camera.overInteractableUi = false;
            }
        }
    }
}
