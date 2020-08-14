using System;
using TMPro;
using UnityEngine;

namespace ST.Play.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ReportLine : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private RectTransform _rect;
        
#pragma warning disable 649
        [SerializeField] private float baseHeight;
#pragma warning restore 649

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _rect = _text.GetComponent<RectTransform>();
        }

        private void Update()
        {
            var sz = _rect.sizeDelta;
            _rect.sizeDelta = new Vector2(sz.x, baseHeight * _text.textInfo.lineCount);
        }
    }
}