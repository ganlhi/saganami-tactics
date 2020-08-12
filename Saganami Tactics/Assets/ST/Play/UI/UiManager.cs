using System.Linq;
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
                GameManager.SetReady(true);
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
            _gameManager.OnShipsInit += (sender, args) => { shipsList.Ships = GameManager.GetAllShips(); };
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

        #region Targeting panel

#pragma warning disable 649
        [SerializeField] private TargetingPanel targetingPanel;
#pragma warning restore 649

        private void UpdateTargetingPanelOnChanges()
        {
            _gameManager.OnTurnStepChange += (sender, step) => SetTargetingPanelVisibility();
            _gameManager.OnSelectShip += (sender, ship) => SetTargetingPanelVisibility();
        }

        private void UpdateTargetingPanelEachFrame()
        {
            if (!targetingPanel.Active) return;
            targetingPanel.UpdateContent(_gameManager.SelectedShip.GetComponent<FireControl>().Locks.Values.ToList());
        }

        private void SetTargetingPanelVisibility()
        {
            targetingPanel.Active = _gameManager.Step == TurnStep.Targeting && _gameManager.SelectedShip.OwnedByClient;
        }

        #endregion Targeting panel

        #region Hover info

#pragma warning disable 649
        [SerializeField] private GameObject hoverInfo;
        [SerializeField] private TextMeshProUGUI hoverInfoText;
        // TODO handle missiles
#pragma warning restore 649

        public void SetHoverInfo([CanBeNull] ShipView shipView, [CanBeNull] MissileView missileView)
        {
            if (shipView != null)
            {
                hoverInfo.SetActive(true);
                hoverInfoText.text = shipView.ship.name;
                hoverInfoText.color = shipView.ship.team.ToColor();
            }
            else if (missileView != null)
            {
                var missile = missileView.missile;
                var attacker = GameManager.GetShipById(missile.attackerId);
                var target = GameManager.GetShipById(missile.targetId);

                if (attacker != null && target != null)
                {
                    hoverInfo.SetActive(true);
                    hoverInfoText.text = $"{missile.number} missiles from {attacker.ship.name} to {target.ship.name}";
                    hoverInfoText.color = attacker.ship.team.ToColor();
                }
                else
                {
                    hoverInfo.SetActive(false);
                }
            }
            else
            {
                hoverInfo.SetActive(false);
            }
        }

        #endregion Hover info

        private void Awake()
        {
            loadingGroup.alpha = 1f;
            loadingGroup.blocksRaycasts = true;

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
            UpdateTargetingPanelOnChanges();
        }

        private void Update()
        {
            UpdatePlottingPanelEachFrame();
            UpdateTargetingPanelEachFrame();
        }
    }
}