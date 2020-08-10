using System;
using System.Collections.Generic;
using ST.Scriptable;
using UnityEngine;

namespace ST.Play.UI
{
    [RequireComponent(typeof(Animator))]
    public class TargetingPanel : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private GameObject targetSelectorPrefab;
        [SerializeField] private Transform forwardContent;
        [SerializeField] private Transform aftContent;
        [SerializeField] private Transform portContent;
        [SerializeField] private Transform starboardContent;
#pragma warning restore 649

        private Animator _animator;

        private bool _active;

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;

                if (!_active && _animator.GetCurrentAnimatorStateInfo(0).IsName("Window In"))
                {
                    _animator.CrossFade("Window Out", 0.1f);
                }
                else if (_active && _animator.GetCurrentAnimatorStateInfo(0).IsName("Window Out"))
                {
                    _animator.CrossFade("Window In", 0.1f);
                }
            }
        }

        public List<Tuple<WeaponMount, List<TargettingContext>>> ForwardWeapons {
            set => UpdateContent(forwardContent, value);
        }

        public List<Tuple<WeaponMount, List<TargettingContext>>> AftWeapons {
            set => UpdateContent(aftContent, value);
        }
        public List<Tuple<WeaponMount, List<TargettingContext>>> PortWeapons {
            set => UpdateContent(portContent, value);
        }
        public List<Tuple<WeaponMount, List<TargettingContext>>> StarboardWeapons {
            set => UpdateContent(starboardContent, value);
        }

        private void UpdateContent(Transform content, List<Tuple<WeaponMount, List<TargettingContext>>> weaponsWithTargets)
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            foreach (var (weaponMount, targets) in weaponsWithTargets)
            {
                var selector = Instantiate(targetSelectorPrefab, content).GetComponent<WeaponTargetSelector>();
                selector.Mount = weaponMount;
                selector.PotentialTargets = targets;
            }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
        }
    }
}