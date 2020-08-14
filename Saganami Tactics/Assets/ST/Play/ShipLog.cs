using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace ST.Play
{
    [RequireComponent(typeof(ShipView))]
    public class ShipLog : MonoBehaviourPun
    {
        private readonly List<Report> _reports = new List<Report>();
        public IReadOnlyList<Report> Reports => _reports.AsReadOnly();
        
        public event EventHandler OnReportLogged;

        public void AddReport(Report report)
        {
            Debug.Log($"Add Report {report.type}: {report.message}");
            photonView.RPC("RPC_AddReport", RpcTarget.All, report.type, report.turn, report.message);
        }

        [PunRPC]
        private void RPC_AddReport(ReportType type, int turn, string message)
        {
            _reports.Add(new Report()
            {
                type = type,
                turn = turn,
                message = message,
            });   
            
            OnReportLogged?.Invoke(this, EventArgs.Empty);
        }

        private void Start()
        {
            var shipView = GetComponent<ShipView>();
            
            _reports.Add(new Report()
            {
                type = ReportType.Info,
                turn = 1,
                message = $"Initialize ship log - {shipView.ship.name}",
            });
        }
    }
}