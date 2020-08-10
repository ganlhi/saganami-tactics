using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Michsky.UI.Shift;
using ST.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ST.Play.UI
{
    public class WeaponTargetSelector : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Sprite missileIcon;
        [SerializeField] private Sprite laserIcon;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI weaponName;
        [SerializeField] private HorizontalSelector selector;
#pragma warning restore 649

        [CanBeNull]
        public WeaponMount Mount
        {
            set
            {
                icon.sprite = value.model.type == WeaponType.Missile ? missileIcon : laserIcon; // TODO handle other types
                weaponName.text = value.model.name;
                // TODO add an "amount" indicator?
            }  
        }

        public List<TargettingContext> PotentialTargets
        {
            set => UpdateSelectorContent(value);
        }

        public event EventHandler<TargettingContext> OnSelectTarget; 

        private void UpdateSelectorContent(List<TargettingContext> targettingContexts)
        {
            selector.itemList.Clear();
            foreach (var targettingContext in targettingContexts)
            {
                var onSelect = new UnityEvent();
                onSelect.AddListener(() => OnSelectTarget?.Invoke(this, targettingContext));
                
                selector.itemList.Add(new HorizontalSelector.Item()
                {
                    itemTitle = $"{targettingContext.Target.name} ({Mathf.CeilToInt(targettingContext.LaunchDistance)})",
                    onValueChanged = onSelect,
                });
            }
            
        }
    }
}