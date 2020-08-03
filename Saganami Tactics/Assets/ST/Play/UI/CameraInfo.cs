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
        [SerializeField] private new Moba_Camera camera;
        [SerializeField] private UIManagerImage icon;
        [SerializeField] private TextMeshProUGUI shipName;
#pragma warning restore 649

        private void Update()
        {
            if (camera.settings.cameraLocked)
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