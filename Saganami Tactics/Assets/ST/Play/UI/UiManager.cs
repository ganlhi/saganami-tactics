using System;
using UnityEngine;

namespace ST.Play.UI
{
    [RequireComponent(typeof(GameManager))]
    public class UiManager : MonoBehaviour
    {
        private GameManager _gameManager;

        #region Turn management

        [SerializeField] private TurnPanel turnPanel;

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

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();
        }

        private void Start()
        {
            UpdateTurnPanelOnChanges();
        }
    }
}