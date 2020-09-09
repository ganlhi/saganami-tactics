using ST.Common;
using UnityEngine;

namespace ST.Play
{
    [RequireComponent(typeof(ShipView))]
    public class ShipFutureVelocityVector : MonoBehaviour
    {
        public bool visible;

        private ShipView _shipView;
        private LineRenderer _vectorLine;
        private Camera _camera;
        [SerializeField] private float widthCoefficient = 0.004f;
        [SerializeField] private float dotsCountCoefficient = 0.5f;

        private static readonly int RepeatCount = Shader.PropertyToID("_RepeatCount");

        private void Awake()
        {
            _camera = Camera.main;
            _shipView = GetComponent<ShipView>();
            _vectorLine = transform.Find("FutureVelocityVector").GetComponent<LineRenderer>();
        }

        private void Update()
        {
            var futureVelocity = _shipView.ship.velocity + (_shipView.OwnedByClient
                                     ? _shipView.ship.ThrustVector
                                     : Vector3.zero);

            // TODO maybe add toggle ui to show/hide enemy predictions?
            _vectorLine.gameObject.SetActive(visible && _shipView.OwnedByClient);

            var fromPos = _shipView.EndMarker.transform.position;
            var toPos = fromPos + futureVelocity;

            _vectorLine.SetPosition(0, fromPos);
            _vectorLine.SetPosition(1, toPos);

            if (_camera == null) return;

            var camPos = _camera.transform.position;
            _vectorLine.startWidth = camPos.DistanceTo(fromPos) * widthCoefficient;
            _vectorLine.endWidth = camPos.DistanceTo(toPos) * widthCoefficient;


            var length = _camera.WorldToScreenPoint(fromPos).DistanceTo(_camera.WorldToScreenPoint(toPos));
            var meanWidth = (_vectorLine.startWidth + _vectorLine.endWidth) / 2f;
            var nbDots = length * dotsCountCoefficient;

            _vectorLine.material.SetFloat(RepeatCount, nbDots);
        }
    }
}