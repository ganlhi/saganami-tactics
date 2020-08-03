using Michsky.UI.Shift;
using Photon.Realtime;
using ST.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    public class PlayersListButton : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Image teamColorImage;
        [SerializeField] private Image readyImage;
        [SerializeField] private TextMeshProUGUI nameText;
#pragma warning restore 649

        public Player player;

        private void Start()
        {
            if (player == null) return;

            var team = player.GetTeam();
            teamColorImage.color = team?.ToColor() ?? Color.white;

            nameText.text = player.NickName;
            if (player.IsLocal)
            {
                nameText.GetComponent<UIManagerText>().colorType = UIManagerText.ColorType.SECONDARY;
            }
            
            readyImage.gameObject.SetActive(player.IsReady());
        }
    }
}