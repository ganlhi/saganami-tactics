using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ST.Common.UI
{
    public class InputNavigator : MonoBehaviour
    {
        private EventSystem _system;

        private void Start()
        {
            _system = EventSystem.current; // EventSystemManager.currentSystem;
        }

        // Update is called once per frame
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Tab)) return;

            Selectable next = null;

            if (_system.currentSelectedGameObject == null)
            {
                var firstSelectable = FindObjectOfType<Selectable>();
                if (firstSelectable != null)
                {
                    next = firstSelectable;
                }
            }
            else
            {
                next = _system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            }

            if (next == null) return;

            var inputfield = next.GetComponent<InputField>();

            if (inputfield != null)
                inputfield.OnPointerClick(
                    new PointerEventData(_system)); //if it's an input field, also set the text caret

            _system.SetSelectedGameObject(next.gameObject, new BaseEventData(_system));
        }
    }
}