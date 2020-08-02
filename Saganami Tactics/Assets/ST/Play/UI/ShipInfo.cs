using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    public class ShipInfo : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Image flagImage;
        [SerializeField] private Image teamColorImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI classText;
        [SerializeField] private Ship ship;
#pragma warning restore 649

        private void Update()
        {
            // flagImage.sprite = TODO get ssd
            teamColorImage.color = ship.team.ToColor();
            nameText.text = ship.name;
            classText.text = ship.ssdName; // TODO + (ssd.category.code);
        }
    }
}