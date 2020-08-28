using TMPro;
using UnityEngine;

namespace ST.Common.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public class AutofillWithPlayerPref : MonoBehaviour
    {
        private TMP_InputField _field;

#pragma warning disable 649
        [SerializeField] private string prefKey;
#pragma warning restore 649

        private void Start()
        {
            _field = GetComponent<TMP_InputField>();
            
            if (PlayerPrefs.HasKey(prefKey))
            {
                _field.text = PlayerPrefs.GetString(prefKey);
            }
        }
    }
}