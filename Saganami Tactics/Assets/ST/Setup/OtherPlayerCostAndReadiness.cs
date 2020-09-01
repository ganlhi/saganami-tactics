using Michsky.UI.Shift;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Setup
{
    public class OtherPlayerCostAndReadiness : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Image teamColorImage;
        [SerializeField] private UIManagerText totalCostManager;
        [SerializeField] private TextMeshProUGUI totalCostText;
        [SerializeField] private GameObject readyIcon;
#pragma warning restore 649

        private Team _team;
        public Team Team
        {
            get => _team;
            set
            {
                _team = value;
                teamColorImage.color = value.ToColor();
            }
        }

        public void SetCost(int cost, bool overflow)
        {
            totalCostManager.colorType =
                overflow ? UIManagerText.ColorType.NEGATIVE : UIManagerText.ColorType.PRIMARY;
            totalCostText.text = cost.ToString();
        }

        public void SetReady(bool ready)
        {
            readyIcon.SetActive(ready);
        } 
    }
}