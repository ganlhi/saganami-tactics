using System;
using ST.Scriptable;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ST.Setup
{
    public class SetupShipListItem : MonoBehaviour
    {
        private Tuple<Ssd, string> _ship;

        public Tuple<Ssd, string> Ship
        {
            get => _ship;
            set
            {
                var oldName = _ship?.Item2;
                _ship = value;
                UpdateUi();

                if (oldName != _ship.Item2) OnNameChange?.Invoke(this, _ship.Item2);
            }
        }

        public event EventHandler<string> OnNameChange;
        public event EventHandler OnDelete;
        public event EventHandler OnShowSsd;

#pragma warning disable 649
        [SerializeField] private TMP_InputField unitNameField;
        [SerializeField] private TextMeshProUGUI classAndCategoryText;
        [SerializeField] private TextMeshProUGUI costText;
#pragma warning restore 649

        public void SetName(string newName)
        {
            OnNameChange?.Invoke(this, newName);
        }

        private void UpdateUi()
        {
            if (_ship.Item2.Length > 0)
            {
                unitNameField.text = _ship.Item2;
            }
            
            classAndCategoryText.text =  $"<b>{_ship.Item1.className}</b> class {_ship.Item1.category.Name}";
            costText.text = _ship.Item1.baseCost.ToString();
        }

        public void RandomizeName()
        {
            var nbSampleNames = _ship.Item1.sampleNames.Count;
            if (nbSampleNames == 0) return;
            var rnd = Random.Range(0, nbSampleNames);
            var sampleName = _ship.Item1.sampleNames[rnd];
            unitNameField.text = sampleName;
            SetName(sampleName);
        }

        public void Delete()
        {
            OnDelete?.Invoke(this, EventArgs.Empty);
        }

        public void ShowSsd()
        {
            OnShowSsd?.Invoke(this, EventArgs.Empty);
        }
    }
}