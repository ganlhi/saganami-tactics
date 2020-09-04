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
        
        public event EventHandler<Report> OnReportLogged;
        public event EventHandler OnReportsLogged;

        public void AddReport(Report report)
        {
            photonView.RPC("RPC_AddReport", RpcTarget.All, report.type, report.turn, report.message);
        }

        [PunRPC]
        private void RPC_AddReport(ReportType type, int turn, string message)
        {
            var report = new Report()
            {
                type = type,
                turn = turn,
                message = message,
            };
            
            _reports.Add(report);   

            OnReportLogged?.Invoke(this, report);
        }
        public void AddReports(List<Report> reports)
        {
            var nb = reports.Count;
            var data = new object[nb * 3];

            var i = 0;
            foreach (var report in reports)
            {
                data[i] = report.type;
                data[i+1] = report.turn;
                data[i+2] = report.message;
                i += 3;
            }

            photonView.RPC("RPC_AddReports", RpcTarget.All, nb, data);
        }

        [PunRPC]
        private void RPC_AddReports(int nb, object[] data)
        {
            for (var i = 0; i < nb * 3; i += 3)
            {
                var report = new Report()
                {
                    type = (ReportType) data[i],
                    turn = (int) data[i+1],
                    message = (string) data[i+2],
                };
                
                _reports.Add(report);
            }
            
            OnReportsLogged?.Invoke(this, EventArgs.Empty);
        }

//        private void Start()
//        {
//            var shipView = GetComponent<ShipView>();
//            
//            _reports.Add(new Report()
//            {
//                type = ReportType.Info,
//                turn = 1,
//                message = $"Initialize ship log - {shipView.ship.name}",
//            });
//        }
    }
}