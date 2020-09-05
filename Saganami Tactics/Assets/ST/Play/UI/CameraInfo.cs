using Michsky.UI.Shift;
using ST.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    public class CameraInfo : MonoBehaviour
    {
#pragma warning disable 649
#pragma warning disable 108,114
        [SerializeField] private Moba_Camera camera;
#pragma warning restore 108,114
        [SerializeField] private UIManagerImage icon;
        [SerializeField] private TextMeshProUGUI shipName;
#pragma warning restore 649

        private void Update()
        {
            if (camera.settings.cameraLocked && camera.settings.lockTargetTransform != null)
            {
                icon.colorType = UIManagerImage.ColorType.SECONDARY;
                shipName.text = camera.settings.lockTargetTransform.GetComponent<ShipView>()?.ship.name;
                shipName.gameObject.SetActive(true);
            }
            else
            {
                icon.colorType = UIManagerImage.ColorType.PRIMARY;
                shipName.gameObject.SetActive(false);
            }
        }
    }
}