using ST.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    public class ShipInfo : MonoBehaviour
    {
        public Ship ship;
        public Ssd ssd;
        
#pragma warning disable 649
        [SerializeField] private Image flagImage;
        [SerializeField] private Image teamColorImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI classText;
#pragma warning restore 649

        private void Update()
        {
            if (ssd != null)
            {
                flagImage.sprite = ssd.faction.Flag;
                teamColorImage.color = ship.team.ToColor();
                nameText.text = ship.name;
                classText.text = ship.ssdName + " (" + ssd.category.Code + ")";
            }
        }
    }
}