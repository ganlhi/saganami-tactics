using System;
using System.Collections.Generic;
using System.Linq;
using ST.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ST.Play.UI
{
    public class TargetMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public FireControl fcon;
        public ShipView shipView;
        public List<TargetingContext> targetingContexts;

        public event EventHandler<Tuple<TargetingContext, bool>> OnLockTarget;

#pragma warning disable 649
        [SerializeField] private Animator animator;
        [SerializeField] private TargetMarkerWeaponSwitch weaponSwitchPrefab;
        [SerializeField] private Transform weaponSwitchesContainer;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private CanvasGroup weaponsCanvasGroup;
#pragma warning restore 649

        private Dictionary<TargetingContext, TargetMarkerWeaponSwitch> _selectors =
            new Dictionary<TargetingContext, TargetMarkerWeaponSwitch>();

        private Camera _camera;

        private readonly Dictionary<TargetingContext, TargetMarkerWeaponSwitch> _switches =
            new Dictionary<TargetingContext, TargetMarkerWeaponSwitch>();

        private void Start()
        {
            weaponsCanvasGroup.alpha = 0;
            
            _camera = Camera.main;

            foreach (Transform child in weaponSwitchesContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var targetingContext in targetingContexts)
            {
                var sw = Instantiate(weaponSwitchPrefab).GetComponent<TargetMarkerWeaponSwitch>();

                sw.TargetingContext = targetingContext;
                if (fcon.Locks.TryGetValue(targetingContext.Mount, out var lockedTargetingContext))
                {
                    if (lockedTargetingContext == targetingContext)
                    {
                        sw.IsLocked = true;
                    }
                    else
                    {
                        sw.IsDisabled = true;
                    }
                }

                sw.transform.SetParent(weaponSwitchesContainer, false);

                _switches.Add(targetingContext, sw);

                var side = targetingContext.Side;
                sw.OnToggle += (sender, isOn) =>
                {
                    if (isOn)
                    {
                        fcon.LockTarget(targetingContext.Mount, targetingContext);
                    }
                    else if (fcon.Locks.TryGetValue(targetingContext.Mount, out var otherLockedTargetingContext))
                    {
                        if (otherLockedTargetingContext == targetingContext)
                        {
                            fcon.UnlockTarget(targetingContext.Mount);
                        }
                    }

                    var nbLocks = fcon.Locks.Keys.Count(m => m.side == side);
                    if (nbLocks > 0 && animator.GetCurrentAnimatorStateInfo(0).IsName("Potential"))
                    {
                        animator.CrossFade("Actual", 0.1f);
                    }
                    else if (nbLocks == 0 && animator.GetCurrentAnimatorStateInfo(0).IsName("Actual"))
                    {
                        animator.CrossFade("Potential", 0.1f);
                    }

                    OnLockTarget?.Invoke(this, new Tuple<TargetingContext, bool>(targetingContext, isOn));
                };
            }
        }

        public void UpdateUi()
        {
            foreach (var kv in _switches)
            {
                var targetingContext = kv.Key;
                var sw = kv.Value;

                if (!fcon.Locks.TryGetValue(targetingContext.Mount, out var lockedTargetingContext))
                {
                    sw.IsLocked = false;
                    sw.IsDisabled = false;
                    continue;
                }

                if (lockedTargetingContext == targetingContext)
                {
                    sw.IsLocked = true;
                }
                else
                {
                    sw.IsDisabled = true;
                }
            }
        }

        private void Update()
        {
            var position = shipView.transform.position;
            var shipDir = _camera.transform.position.DirectionTo(position);

            if (Vector3.Angle(shipDir, _camera.transform.forward) > 90)
            {
                canvasGroup.alpha = 0;
            }
            else
            {
                var pt = _camera.WorldToScreenPoint(position);

                canvasGroup.alpha = 1;
                transform.position = pt;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            weaponsCanvasGroup.alpha = 1;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            weaponsCanvasGroup.alpha = 0;
        }
    }
}