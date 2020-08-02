using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    public class ShipsListButton : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Image teamColorImage;
        [SerializeField] private TextMeshProUGUI buttonTitleObj;
        [SerializeField] private Ship ship;
#pragma warning restore 649

        private void Start()
        {
            buttonTitleObj.text = ship.name;
            teamColorImage.color = ship.team.ToColor();
            GetComponent<Button>().interactable = ship.Status == ShipStatus.Ok;
        }
    }
}