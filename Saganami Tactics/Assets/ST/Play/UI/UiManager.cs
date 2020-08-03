using System;
using UnityEngine;

namespace ST.Play.UI
{
    [RequireComponent(typeof(GameManager))]
    public class UiManager : MonoBehaviour
    {
        private GameManager _gameManager;

        #region Turn management

#pragma warning disable 649
        [SerializeField] private TurnPanel turnPanel;
#pragma warning restore 649

        private void UpdateTurnPanelOnChanges()
        {
            turnPanel.turn = _gameManager.Turn;
            turnPanel.step = _gameManager.Step;
            turnPanel.busy = _gameManager.Busy;

            _gameManager.OnTurnChange += (sender, turn) => { turnPanel.turn = turn; };
            _gameManager.OnTurnStepChange += (sender, step) => { turnPanel.step = step; };
            _gameManager.OnBusyChange += (sender, busy) => { turnPanel.busy = busy; };
            turnPanel.OnReady += (sender, args) => { _gameManager.SetReady(true); };
        }

        #endregion Turn management

        #region Ships management

#pragma warning disable 649
        [SerializeField] private ShipsList shipsList;
#pragma warning restore 649

        private void UpdateShipsListOnChanges()
        {
            _gameManager.OnShipsInit += (sender, args) => { shipsList.Ships = _gameManager.GetAllShips(); };
            _gameManager.OnSelectShip += (sender, shipView) => { shipsList.SetSelectedShip(shipView); };

            shipsList.OnSelectShip += (sender, shipView) => { _gameManager.SelectedShip = shipView; };
            shipsList.OnFocusCameraOnShip += (sender, shipView) => { _gameManager.LockCameraToShip(shipView); };
        }

        #endregion Ships management

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();
        }

        private void Start()
        {
            UpdateTurnPanelOnChanges();
            UpdateShipsListOnChanges();
        }
    }
}