using System;
using System.Collections.Generic;
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

        private void UpdateUi(IEnumerable<Report> reports)
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            var turn = 0;

            foreach (var report in reports)
            {
                if (fullLog && report.turn > turn)
                {
                    turn++;

                    var header = Instantiate(turnHeaderPrefab, content).GetComponentInChildren<TextMeshProUGUI>();
                    header.text = $"Turn " + turn;
                }
                
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
}