using System;
using Michsky.UI.Shift;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    [RequireComponent(typeof(Animator))]
    public class CrewActionsPanel : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Button repairBtn;
        [SerializeField] private Button disengageBtn;
        [SerializeField] private Button surrenderBtn;
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

        public bool CanRepair
        {
            set => repairBtn.interactable = value;
        }

        public bool CanDisengage
        {
            set => disengageBtn.interactable = value;
        }

        public bool CanSurrender
        {
            set => surrenderBtn.interactable = value;
        }

        public event EventHandler OnRepair;
        public event EventHandler OnDisengage;
        public event EventHandler OnSurrender;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
           repairBtn.onClick.AddListener(() => OnRepair?.Invoke(this, EventArgs.Empty));
           disengageBtn.onClick.AddListener(() => OnDisengage?.Invoke(this, EventArgs.Empty));
           surrenderBtn.onClick.AddListener(() => OnSurrender?.Invoke(this, EventArgs.Empty));
        }
    }
}