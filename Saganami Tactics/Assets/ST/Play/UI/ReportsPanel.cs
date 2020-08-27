using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ST.Play.UI
{
    public class ReportsPanel : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private bool fullLog;
        [SerializeField] private GameObject turnHeaderPrefab;
        [SerializeField] private TextMeshProUGUI defaultReportLinePrefab;
        [SerializeField] private TextMeshProUGUI warningReportLinePrefab;
        [SerializeField] private TextMeshProUGUI dangerReportLinePrefab;
        [SerializeField] private Transform content;
#pragma warning restore 649

        public List<Report> Reports
        {
            set => UpdateUi(value);
        }

        public void AddReport(Report report)
        {
            AddReportLine(report);
        }

        private void UpdateUi(IReadOnlyCollection<Report> reports)
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            var lastTurnInReports = reports.Any() ? reports.Max(r => r.turn) : 0;

            for (var turn = 1; turn <= lastTurnInReports; turn++)
            {
                var curTurn = turn;
                var turnReports = reports.Where(r => r.turn == curTurn).ToList();

                if (!turnReports.Any()) continue;

                if (fullLog)
                {
                    var header = Instantiate(turnHeaderPrefab, content).GetComponentInChildren<TextMeshProUGUI>();
                    header.text = $"Turn " + curTurn;
                }

                foreach (var report in turnReports)
                {
                    AddReportLine(report);
                }
            }
        }

        private void AddReportLine(Report report)
        {
            TextMeshProUGUI prefab;
            switch (report.type)
            {
                case ReportType.ShipDestroyed:
                case ReportType.DamageTaken:
                    prefab = dangerReportLinePrefab;
                    break;
                case ReportType.ShipSurrendered:
                case ReportType.MissilesHit:
                case ReportType.BeamsHit:
                    prefab = warningReportLinePrefab;
                    break;
                case ReportType.ShipDisengaged:
                case ReportType.MissilesMissed:
                case ReportType.MissilesStopped:
                case ReportType.BeamsMiss:
                case ReportType.Info:
                    prefab = defaultReportLinePrefab;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var txt = Instantiate(prefab, content).GetComponent<TextMeshProUGUI>();
            txt.text = report.message;
        }
    }
}