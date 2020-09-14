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
                shipName.gameObject.SetActive(true);
                
                if (camera.settings.lockTargetTransform.TryGetComponent<ShipView>(out var shipView))
                {
                    shipName.text = shipView.ship.name;
                } else if (camera.settings.lockTargetTransform.TryGetComponent<ShipMarker>(out var shipMarker))
                {
                    shipName.text = shipMarker.shipView.ship.name + " (fut.)";
                } else if (camera.settings.lockTargetTransform.TryGetComponent<MissileView>(out var missileView))
                {
                    shipName.text = "Missiles";
                }
            }
            else
            {
                icon.colorType = UIManagerImage.ColorType.PRIMARY;
                shipName.gameObject.SetActive(false);
            }
        }
    }
}