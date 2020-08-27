using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    [RequireComponent(typeof(Animator))]
    public class TargetingPanel : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private TargetInfo targetInfoPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject deployedLabel;
        [SerializeField] private Button deployButton;
        [SerializeField] private TextMeshProUGUI decoysNumberText;
#pragma warning restore 649

        private Animator _animator;

        public event EventHandler OnDeployDecoy;

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

        private void Start()
        {
            deployButton.onClick.AddListener(() => OnDeployDecoy?.Invoke(this, EventArgs.Empty));
        }

        public void UpdateDecoys(int remaining, bool deployed)
        {
            deployedLabel.SetActive(deployed);
            deployButton.gameObject.SetActive(!deployed);
            deployButton.interactable = remaining > 0;
            decoysNumberText.text = remaining.ToString();
        }

        public void UpdateContent(List<TargetingContext> locks)
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            var orderedLocks = locks.ToList();
            orderedLocks.Sort((a, b) =>
            {
                var sideCmp = a.Side.CompareTo(b.Side);
                if (sideCmp != 0) return sideCmp;

                var weaponCmp = string.Compare(a.Mount.model.name, b.Mount.model.name, StringComparison.Ordinal);
                if (weaponCmp != 0) return weaponCmp;

                return string.Compare(a.Target.name, b.Target.name, StringComparison.Ordinal);
            });
            
            foreach (var target in orderedLocks)
            {
                var info = Instantiate(targetInfoPrefab, content).GetComponent<TargetInfo>();
                info.TargetingContext = target;
            }
        }
    }
}