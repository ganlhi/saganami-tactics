using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    public class TargetInfo : MonoBehaviour
    {
        public TargettingContext TargettingContext
        {
            get => _targettingContext;
            set
            {
                _targettingContext = value;
                UpdateUi();
            }
        }

        private TargettingContext _targettingContext;

#pragma warning disable 649
        [SerializeField] private Transform sideImage;
        [SerializeField] private TextMeshProUGUI weapon;
        [SerializeField] private TextMeshProUGUI target;
        [SerializeField] private TextMeshProUGUI number;
        [SerializeField] private TextMeshProUGUI distance;
#pragma warning restore 649

        private void UpdateUi()
        {
            switch (TargettingContext.Side)
            {
                case Side.Forward:
                    sideImage.localRotation = Quaternion.Euler(0, 0, 90);
                    break;
                case Side.Port:
                    sideImage.localRotation = Quaternion.Euler(0, 0, 180);
                    break;
                case Side.Aft:
                    sideImage.localRotation = Quaternion.Euler(0, 0, 270);
                    break;
            }

            weapon.text = TargettingContext.Mount.model.name;
            target.text = TargettingContext.Target.name;
            number.text = TargettingContext.Number.ToString();
            distance.text = TargettingContext.LaunchDistance.ToString(CultureInfo.InvariantCulture);
        }
    }
}