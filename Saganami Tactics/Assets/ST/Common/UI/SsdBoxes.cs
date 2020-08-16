using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Common.UI
{
    public class SsdBoxes : MonoBehaviour
    {
        public int CurrentValue
        {
            set => curValueText.text = value.ToString();
        }

        public Tuple<int, int> Damages
        {
            set
            {
                var (destroyed, damaged) = value;
                
                destroyedObj.SetActive(destroyed > 0);
                destroyedText.text = destroyed.ToString();
                
                damagedObj.SetActive(damaged > 0);
                damagedText.text = damaged.ToString();
            }
        }

        public bool CanRepair
        {
            set => repairButton.gameObject.SetActive(value);
        }

        public event EventHandler OnRepair;

#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI curValueText;
        [SerializeField] private TextMeshProUGUI destroyedText;
        [SerializeField] private GameObject destroyedObj;
        [SerializeField] private TextMeshProUGUI damagedText;
        [SerializeField] private GameObject damagedObj;
        [SerializeField] private Button repairButton;
#pragma warning restore 649

        private void Start()
        {
            repairButton.onClick.AddListener(() => OnRepair?.Invoke(this, EventArgs.Empty));
        }
    }
}