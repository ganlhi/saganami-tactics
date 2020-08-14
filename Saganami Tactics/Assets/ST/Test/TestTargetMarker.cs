using System.Collections.Generic;
using System.Linq;
using ST.Play.UI;
using UnityEngine;

namespace ST.Test
{
    public class TestTargetMarker : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private TargetMarker targetMarker;
        [SerializeField] private List<TargetingContext> potentialTargets;
#pragma warning restore 649

        private void Start()
        {
            var mounts = targetMarker.shipView.ship.Ssd.weaponMounts.Where(m => m.side == Side.Port);
            
            potentialTargets = new List<TargetingContext>();
            foreach (var mount in mounts)
            {
                potentialTargets.Add(new TargetingContext()
                {
                    Number = 5,
                    Side = Side.Port,
                    LaunchDistance = 15,
                    LaunchPoint = new Vector3(-15, 0, 0),
                    Target = targetMarker.shipView.ship,
                    Mount = mount
                });
            }
            
            targetMarker.targetingContexts = potentialTargets;
        }
    }
}