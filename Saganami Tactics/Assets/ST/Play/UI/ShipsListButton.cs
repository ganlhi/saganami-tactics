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
#pragma warning restore 649

        private void Start()
        {
            buttonTitleObj.text = ship.name;
            teamColorImage.color = ship.team.ToColor();

            UpdateSelectedState();

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
    }
}