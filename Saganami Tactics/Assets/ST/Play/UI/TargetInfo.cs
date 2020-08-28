using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    public class TargetInfo : MonoBehaviour
    {
        public TargetingContext TargetingContext
        {
            get => _targetingContext;
            set
            {
                _targetingContext = value;
                UpdateUi();
            }
        }

        private TargetingContext _targetingContext;

#pragma warning disable 649
        [SerializeField] private Transform sideImage;
        [SerializeField] private TextMeshProUGUI weapon;
        [SerializeField] private TextMeshProUGUI target;
        [SerializeField] private TextMeshProUGUI number;
        [SerializeField] private TextMeshProUGUI distance;
        [SerializeField] private Sprite sideSpriteShortRange;
        [SerializeField] private Sprite sideSpriteLongRange;
#pragma warning restore 649

        private void UpdateUi()
        {
            switch (TargetingContext.Side)
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

            sideImage.GetComponent<Image>().sprite = TargetingContext.ShortRange
                ? sideSpriteShortRange
                : sideSpriteLongRange;

            weapon.text = TargetingContext.Mount.model.name;
            target.text = TargetingContext.Target.name;
            number.text = TargetingContext.Number.ToString();
            distance.text = Mathf.CeilToInt(TargetingContext.LaunchDistance).ToString();
        }
    }
}