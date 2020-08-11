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
        [SerializeField] private TargetInfo targetInfoPrefab;
        [SerializeField] private Transform content;
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

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void UpdateContent(List<TargettingContext> locks)
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            foreach (var target in locks)
            {
                var info = Instantiate(targetInfoPrefab, content).GetComponent<TargetInfo>();
                info.TargettingContext = target;
            }
        }
    }
}