using System;
using Michsky.UI.Shift;
using ST.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    public class TargetMarkerWeaponSwitch : MonoBehaviour
    {
        private TargetingContext _targetingContext;

        public TargetingContext TargetingContext
        {
            get => _targetingContext;
            set
            {
                _targetingContext = value;
                UpdateUi();
            }
        }

        private bool _isLocked;

        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                _isLocked = value;
                switchManager.isOn = _isLocked;
            }
        }

        private bool _isDisabled;

        public bool IsDisabled
        {
            get => _isDisabled;
            set
            {
                if (!_isDisabled && value)
                {
                    switchManager.GetComponent<Animator>().Play("Disable");
                }
                else if (!value && _isDisabled)
                {
                    switchManager.GetComponent<Animator>().Play("Enable");
                }
                _isDisabled = value;
                switchManager.GetComponent<Button>().interactable = !_isDisabled;
            }
        }

        public event EventHandler<bool> OnToggle;


#pragma warning disable 649
        [SerializeField] private Transform iconTransform;
        [SerializeField] private Image icon;
        [SerializeField] private Sprite spriteShortRange;
        [SerializeField] private Sprite spriteLongRange;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private SwitchManager switchManager;
#pragma warning restore 649

        public void Toggle()
        {
            OnToggle?.Invoke(this, switchManager.isOn);
        }
        
        private void Start()
        {
            switchManager.switchTag = Utils.GenerateId();
            
            foreach (Transform child in switchManager.transform.parent)
            {
                child.gameObject.SetActive(true);
            }
        }

        private void UpdateUi()
        {
            switch (_targetingContext.Side)
            {
                case Side.Forward:
                    iconTransform.localRotation = Quaternion.Euler(0, 0, 90);
                    break;
                case Side.Port:
                    iconTransform.localRotation = Quaternion.Euler(0, 0, 180);
                    break;
                case Side.Aft:
                    iconTransform.localRotation = Quaternion.Euler(0, 0, 270);
                    break;
            }

            icon.sprite = _targetingContext.ShortRange ? spriteShortRange : spriteLongRange;

            // TODO handle other types
            switch (TargetingContext.Mount.model.type)
            {
                case WeaponType.Missile:
                    typeText.text = "M";
                    break;
                case WeaponType.Laser:
                    typeText.text = "L";
                    break;
                case WeaponType.Graser:
                    typeText.text = "G";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switchManager.isOn = IsLocked;
        }
    }
}