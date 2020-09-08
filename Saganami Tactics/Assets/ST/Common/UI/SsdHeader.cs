using ST.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Common.UI
{
    public class SsdHeader : MonoBehaviour
    {
        public Ssd Ssd
        {
            set => UpdateUi(value); 
        }
        public string ShipName
        {
            set => shipNameText.text = value;
        }

#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI shipNameText;
        [SerializeField] private TextMeshProUGUI classNameAndCategoryText;
        [SerializeField] private TextMeshProUGUI factionText;
        [SerializeField] private TextMeshProUGUI costAndCrewText;
        [SerializeField] private TextMeshProUGUI scaleText;
        [SerializeField] private Image flagImage;
#pragma warning restore 649

        private void UpdateUi(Ssd ssd)
        {
            classNameAndCategoryText.text = $"<b>{ssd.className}</b> class {ssd.category.Name}";
            factionText.text = ssd.faction.Name;
            costAndCrewText.text = $"Cost: {ssd.baseCost}\nCrew: {ssd.crew} ({ssd.crewOfficers}/{ssd.crewEnlisted})";
            flagImage.sprite = ssd.faction.Flag;
            scaleText.text = $"Scale: {ssd.scale}";
        }
    }
}