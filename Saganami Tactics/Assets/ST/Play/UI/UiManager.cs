using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Michsky.UI.Shift;
using ST.Common;
using ST.Scriptable;
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
        [SerializeField] private FullscreenPanels fullscreenPanels;
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
            plottingPanel.Active = _gameManager.Step == TurnStep.Plotting &&
                                   _gameManager.SelectedShip.OwnedByClient &&
                                   _gameManager.SelectedShip.ship.Status == ShipStatus.Ok;
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
            targetingPanel.Active = _gameManager.Step == TurnStep.Targeting &&
                                    _gameManager.SelectedShip.OwnedByClient &&
                                    _gameManager.SelectedShip.ship.Status == ShipStatus.Ok;
        }

        #endregion Targeting panel

        #region Crew actions panel

#pragma warning disable 649
        [SerializeField] private CrewActionsPanel crewActionsPanel;
        [SerializeField] private ModalWindowManager modalDisengage;
        [SerializeField] private ModalWindowManager modalSurrender;
#pragma warning restore 649

        private void UpdateCrewActionsPanelOnChanges()
        {
            _gameManager.OnTurnStepChange += (sender, step) => SetCrewActionsPanelVisibility();
            _gameManager.OnSelectShip += (sender, ship) => SetCrewActionsPanelVisibility();
            _gameManager.OnShipStatusChanged += (sender, ship) => SetCrewActionsPanelVisibility();

            // TODO add repair mode
            crewActionsPanel.OnRepair += (sender, args) => fullscreenPanels.ShowEngineeringPanel();
            crewActionsPanel.OnDisengage += (sender, args) => modalDisengage.ModalWindowIn();
            crewActionsPanel.OnSurrender += (sender, args) => modalSurrender.ModalWindowIn();
        }

        private void SetCrewActionsPanelVisibility()
        {
            crewActionsPanel.Active = _gameManager.Step == TurnStep.CrewActions &&
                                      _gameManager.SelectedShip.OwnedByClient &&
                                      _gameManager.SelectedShip.ship.Status == ShipStatus.Ok;

            if (crewActionsPanel.Active)
            {
                var ship = _gameManager.SelectedShip.ship;

                // can repair if has alterations
                crewActionsPanel.CanRepair = ship.alterations.Any();

                // can disengage if at least 50 away from all operating enemy ships and able to move
                var isFarEnough = !GameManager.GetAllShips().Any(s =>
                    s.ship.team != ship.team &&
                    s.ship.Status == ShipStatus.Ok &&
                    s.ship.position.DistanceTo(ship.position) < 50);

                var canMove = SsdHelper.GetMaxThrust(ship.Ssd, ship.alterations) > 0;

                crewActionsPanel.CanDisengage = isFarEnough && canMove;

                // can surrender if is OK
                crewActionsPanel.CanSurrender = true;
            }
        }

        #endregion Crew actions panel

        #region Hover info

#pragma warning disable 649
        [SerializeField] private GameObject hoverInfo;
        [SerializeField] private TextMeshProUGUI hoverInfoText;
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

        #region Reports

#pragma warning disable 649
        [SerializeField] private ReportsPanel reportsPanel;
        [SerializeField] private ReportsPanel fullReportsPanel;
#pragma warning restore 649

        private void UpdateReportsPanelsOnChanges()
        {
            if (_gameManager.SelectedShip != null)
            {
                ListSelectedShipReports();
            }

            _gameManager.OnSelectShip += (sender, ship) => ListSelectedShipReports();
            _gameManager.OnTurnChange += (sender, turn) => ListSelectedShipReports();
        }

        private void ListSelectedShipReports()
        {
            if (_gameManager.SelectedShip == null) return;
            SetReports();
            _gameManager.SelectedShip.GetComponent<ShipLog>().OnReportLogged += (sender, args) => SetReports();
        }

        private void SetReports()
        {
            var ship = _gameManager.SelectedShip;

            var log = ship.GetComponent<ShipLog>();
            var reports = log.Reports.ToList();
            reportsPanel.Reports = reports.Where(r => r.turn == _gameManager.Turn).ToList();
            fullReportsPanel.Reports = reports;
        }

        #endregion Reports

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
            UpdateCrewActionsPanelOnChanges();
            UpdateReportsPanelsOnChanges();
        }

        private void Update()
        {
            UpdatePlottingPanelEachFrame();
            UpdateTargetingPanelEachFrame();
        }
    }
}