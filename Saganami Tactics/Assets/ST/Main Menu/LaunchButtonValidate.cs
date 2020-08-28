using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ST.Main_Menu
{
    public class LaunchButtonValidate : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private List<TMP_InputField> requiredInputFields;
#pragma warning restore 649

        private void Start()
        {
            Validate();
            requiredInputFields.ForEach(
                input => input.onValueChanged.AddListener(
                    v => Validate()
                )
            );
        }

        private void Validate()
        {
            var btn = GetComponent<Button>();
            btn.interactable = requiredInputFields.All(input => input.text.Length > 0);
        }
    }
}