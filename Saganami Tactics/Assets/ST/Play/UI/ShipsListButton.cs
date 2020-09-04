using System;
using Michsky.UI.Shift;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    public class ShipsListButton : MonoBehaviour
    {
        public Ship ship;
        
        private bool _selected;

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                UpdateSelectedState();
            }
        }

        public event EventHandler OnSelect;
        public event EventHandler OnFocusCamera;

#pragma warning disable 649
        [SerializeField] private Image teamColorImage;
        [SerializeField] private TextMeshProUGUI buttonTitleObj;
        [SerializeField] private Button focusCameraBtn;
        [SerializeField] private UIManagerImage icon;
        [SerializeField] private UIManagerText text;
        [SerializeField] private GameObject notification;
        [SerializeField] private Color infoColor;
        [SerializeField] private Color warningColor;
        [SerializeField] private Color dangerColor;
#pragma warning restore 649

        private void Start()
        {
            buttonTitleObj.text = ship.name;
            teamColorImage.color = ship.team.ToColor();

            UpdateSelectedState();
            if (_selected) UpdateNotification(null);

            var button = GetComponent<Button>();
            button.interactable = ship.Status == ShipStatus.Ok;
            button.onClick.AddListener(() => OnSelect?.Invoke(this, EventArgs.Empty));

            focusCameraBtn.onClick.AddListener(() => OnFocusCamera?.Invoke(this, EventArgs.Empty));
        }

        private void UpdateSelectedState()
        {
            icon.colorType = _selected ? UIManagerImage.ColorType.SECONDARY : UIManagerImage.ColorType.PRIMARY;
            text.colorType = _selected ? UIManagerText.ColorType.SECONDARY : UIManagerText.ColorType.PRIMARY;
        }

        public void UpdateNotification(ReportSeverity? worstReportSeverity)
        {
            notification.SetActive(worstReportSeverity.HasValue);
            if (!worstReportSeverity.HasValue) return;

            Color color;

            switch (worstReportSeverity.Value)
            {
                case ReportSeverity.Danger:
                    color = dangerColor;
                    break;
                case ReportSeverity.Warning:
                    color = warningColor;
                    break;
                case ReportSeverity.Info:
                    color = infoColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            notification.transform.Find("Dot").GetComponent<Image>().color = color;
        }
    }
}