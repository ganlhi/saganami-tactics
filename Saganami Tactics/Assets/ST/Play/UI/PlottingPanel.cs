using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Play.UI
{
    [RequireComponent(typeof(Animator))]
    public class PlottingPanel : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Image resetPivotImage;
        [SerializeField] private Image resetRollImage;
        [SerializeField] private Button pivotLeftBtn;
        [SerializeField] private Button pivotRightBtn;
        [SerializeField] private Button pivotUpBtn;
        [SerializeField] private Button pivotDownBtn;
        [SerializeField] private Button pivotResetBtn;
        [SerializeField] private Button rollLeftBtn;
        [SerializeField] private Button rollRightBtn;
        [SerializeField] private Button rollResetBtn;
        [SerializeField] private Button thrustPlusBtn;
        [SerializeField] private Button thrustMinusBtn;
        [SerializeField] private TextMeshProUGUI thrustValue;
#pragma warning restore 649

        private Animator _animator;

        private bool _active;
        private float _usedPivots;
        private float _usedRolls;
        private int _thrust;

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

        public float UsedPivots
        {
            get => _usedPivots;
            set
            {
                _usedPivots = Mathf.Clamp01(value);
                resetPivotImage.fillAmount = _usedPivots;
            }
        }

        public float UsedRolls
        {
            get => _usedRolls;
            set
            {
                _usedRolls = Mathf.Clamp01(value);
                resetRollImage.fillAmount = _usedRolls;
            }
        }

        public int Thrust
        {
            get => _thrust;
            set
            {
                _thrust = value;
                thrustValue.text = value.ToString();
            }
        }

        public int MaxThrust { get; set; }

        public event EventHandler<int> OnYaw;
        public event EventHandler<int> OnPitch;
        public event EventHandler<int> OnRoll;
        public event EventHandler<int> OnSetThrust;
        public event EventHandler OnResetPivot;
        public event EventHandler OnResetRoll;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            thrustPlusBtn.onClick.AddListener(() =>
                OnSetThrust?.Invoke(this, Mathf.Clamp(Mathf.RoundToInt(_thrust + 1), 0, MaxThrust)));
            
            thrustMinusBtn.onClick.AddListener(() =>
                OnSetThrust?.Invoke(this, Mathf.Clamp(Mathf.RoundToInt(_thrust - 1), 0, MaxThrust)));

            pivotLeftBtn.onClick.AddListener(() => OnYaw?.Invoke(this, -1));
            pivotRightBtn.onClick.AddListener(() => OnYaw?.Invoke(this, 1));
            pivotUpBtn.onClick.AddListener(() => OnPitch?.Invoke(this, -1));
            pivotDownBtn.onClick.AddListener(() => OnPitch?.Invoke(this, 1));
            rollLeftBtn.onClick.AddListener(() => OnRoll?.Invoke(this, 1));
            rollRightBtn.onClick.AddListener(() => OnRoll?.Invoke(this, -1));
            pivotResetBtn.onClick.AddListener(() => OnResetPivot?.Invoke(this, EventArgs.Empty));
            rollResetBtn.onClick.AddListener(() => OnResetRoll?.Invoke(this, EventArgs.Empty));
        }
    }
}