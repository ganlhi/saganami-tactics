using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace ST.Play.UI
{
    [RequireComponent(typeof(GameManager))]
    public class UiManager : MonoBehaviour
    {
        private GameManager _gameManager;
#pragma warning disable 649
        [SerializeField] private CanvasGroup loadingGroup;
#pragma warning restore 649

        #region Turn

#pragma warning disable 649
        [SerializeField] private TurnPanel turnPanel;
#pragma warning restore 649

        private void UpdateTurnPanelOnChanges()
        {
            turnPanel.turn = _gameManager.Turn;
            turnPanel.step = _gameManager.Step;
            turnPanel.busy = _gameManager.Busy;

            _gameManager.OnTurnChange += (sender, turn) => { turnPanel.turn = turn; };
            _gameManager.OnTurnStepChange += (sender, step) =>
            {
                turnPanel.step = step;
                turnPanel.ready = false;
            };
            _gameManager.OnBusyChange += (sender, busy) => { turnPanel.busy = busy; };
            turnPanel.OnReady += (sender, args) =>
            {
                _gameManager.SetReady(true);
                turnPanel.ready = true;
            };
        }

        #endregion Turn

        #region Ships

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

        #endregion Ships

        #region Ship info

#pragma warning disable 649
        [SerializeField] private ShipInfo shipInfo;
#pragma warning restore 649

        private void UpdateShipInfoOnChanges()
        {
            _gameManager.OnSelectShip += (sender, shipView) =>
            {
                shipInfo.ship = shipView.ship;
                shipInfo.ssd = shipView.ship.Ssd;
            };
        }

        #endregion Ship info

        #region Plotting panel

#pragma warning disable 649
        [SerializeField] private PlottingPanel plottingPanel;
#pragma warning restore 649

        private void UpdatePlottingPanelOnChanges()
        {
            _gameManager.OnTurnStepChange += (sender, step) => SetPlottingPanelVisibility();
            _gameManager.OnSelectShip += (sender, ship) => SetPlottingPanelVisibility();

            plottingPanel.OnResetPivot += (sender, args) =>
                _gameManager.SelectedShip.Plot(PlottingAction.ResetPivot, 0);

            plottingPanel.OnResetRoll += (sender, args) =>
                _gameManager.SelectedShip.Plot(PlottingAction.ResetRoll, 0);

            plottingPanel.OnSetThrust += (sender, thrust) =>
                _gameManager.SelectedShip.Plot(PlottingAction.SetThrust, thrust);

            plottingPanel.OnYaw += (sender, yaw) =>
                _gameManager.SelectedShip.Plot(PlottingAction.Yaw, yaw);

            plottingPanel.OnPitch += (sender, pitch) =>
                _gameManager.SelectedShip.Plot(PlottingAction.Pitch, pitch);

            plottingPanel.OnRoll += (sender, roll) =>
                _gameManager.SelectedShip.Plot(PlottingAction.Roll, roll);
        }

        private void UpdatePlottingPanelEachFrame()
        {
            if (!plottingPanel.Active) return;

            var ship = _gameManager.SelectedShip.ship;

            plottingPanel.Thrust = ship.Thrust;
            plottingPanel.MaxThrust = ship.MaxThrust;
            plottingPanel.UsedPivots = (float) ship.UsedPivots / ship.MaxPivots;
            plottingPanel.UsedRolls = (float) ship.UsedRolls / ship.MaxRolls;
        }

        private void SetPlottingPanelVisibility()
        {
            plottingPanel.Active = _gameManager.Step == TurnStep.Plotting && _gameManager.SelectedShip.OwnedByClient;
        }

        #endregion Plotting panel

        #region Hover info

#pragma warning disable 649
        [SerializeField] private GameObject hoverShipInfo;
        [SerializeField] private TextMeshProUGUI hoverShipInfoText;
        // TODO handle missiles
#pragma warning restore 649

        public void SetHoverShipInfo([CanBeNull] ShipView shipView)
        {
            if (shipView == null)
            {
                hoverShipInfo.SetActive(false);
            }
            else
            {
                hoverShipInfo.SetActive(true);
                hoverShipInfoText.text = shipView.ship.name;
                hoverShipInfoText.color = shipView.ship.team.ToColor();
            }
        }

        #endregion Hover info

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();

            _gameManager.OnShipsInit += (sender, args) =>
            {
                loadingGroup.alpha = 0f;
                loadingGroup.blocksRaycasts = false;
            };
        }

        private void Start()
        {
            UpdateTurnPanelOnChanges();
            UpdateShipsListOnChanges();
            UpdateShipInfoOnChanges();
            UpdatePlottingPanelOnChanges();
        }

        private void Update()
        {
            UpdatePlottingPanelEachFrame();
        }
    }
}