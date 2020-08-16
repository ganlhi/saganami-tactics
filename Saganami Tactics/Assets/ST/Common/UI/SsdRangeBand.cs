using ST.Scriptable;
using TMPro;
using UnityEngine;

namespace ST.Common.UI
{
    public class SsdRangeBand : MonoBehaviour
    {
        public RangeBand RangeBand
        {
            set => UpdateUi(value);
        }

#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI rangeText;
        [SerializeField] private TextMeshProUGUI accuracyText;
        [SerializeField] private TextMeshProUGUI damagesText;
        [SerializeField] private TextMeshProUGUI penetrationText;
#pragma warning restore 649

        private void UpdateUi(RangeBand band)
        {
            rangeText.text = band.from != band.to ? $"{band.from}-{band.to}" : band.from.ToString();
            accuracyText.text = $"{band.accuracy}+";
            damagesText.text = band.damage.ToString();
            penetrationText.text = band.penetration.ToString();
        }
    }
}