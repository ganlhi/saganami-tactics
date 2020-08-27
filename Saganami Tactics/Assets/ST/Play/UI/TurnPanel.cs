using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    public class TurnPanel : MonoBehaviour
    {
        public int turn;
        public TurnStep step;
        public bool busy;
        public bool ready;
        public event EventHandler OnReady;

#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private TextMeshProUGUI stepText;
        [SerializeField] private GameObject content;
        [SerializeField] private GameObject loading;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Button readyBtn;
        [SerializeField] private Image readyBtnImage;
        [SerializeField] private Sprite notReadySprite;
        [SerializeField] private Sprite readySprite;
#pragma warning restore 649

        private void Start()
        {
            readyBtn.onClick.AddListener(() => { OnReady?.Invoke(this, EventArgs.Empty); });
        }

        private void Update()
        {
            turnText.text = turn.ToString();
            stepText.text = step.ToFriendlyString();
            readyBtnImage.sprite = ready ? readySprite : notReadySprite;
            content.SetActive(!busy);
            loading.SetActive(busy);

            switch (step)
            {
                case TurnStep.Beams:
                    loadingText.text = "Firing";
                    break;
                default:
                    loadingText.text = "Moving";
                    break;
            }
        }
    }
}