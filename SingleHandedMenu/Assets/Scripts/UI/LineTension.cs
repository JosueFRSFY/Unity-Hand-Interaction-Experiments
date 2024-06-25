using System.Collections;
using UnityEngine;

namespace Leap.Unity.Interaction.Storage
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineTension : MonoBehaviour
    {
        [SerializeField] public GameObject _particlesPrefab;

        private LineRenderer _lineRenderer;
        private float _maxWidth;

        public AnimationCurve widthAnimation;

        private bool isDestroying = false;
        private float _tensionPercentage;
        private Transform _start, _end;

        /// <summary>
        /// Sets up a Line Tension
        /// </summary>
        /// <param name="start">Where the Line Tension starts</param>
        /// <param name="tensionPercentage"> Changes the max distance to be maxDistance * (1-tensionPercentage).
        /// Any distance after this, the line will appear straight.
        /// This is partly used to create the illusion of tension in the line.</param>
        public void SetupLineTension(Transform start, Transform end, float tensionPercentage)
        {
            _start = start;
            _end = end;
            _tensionPercentage = tensionPercentage;

            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = 2;
            _maxWidth = _lineRenderer.widthMultiplier;
            _lineRenderer.enabled = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (_lineRenderer == null || !_lineRenderer.enabled) { return; }
            if (!isDestroying)
            {
                UpdateLineTensionPositions();             
            }
        }

        void UpdateLineTensionPositions()
        {
            Vector3[] lineRendererPositions = new Vector3[_lineRenderer.positionCount];
            lineRendererPositions[0] = _start.position;
            lineRendererPositions[1] = _end.position;
            _lineRenderer.SetPositions(lineRendererPositions);
        }

        internal void UpdateLineTension(float distancePercentage)
        {
            if(_lineRenderer == null) { return; }
            float minTensionRange = distancePercentage - _tensionPercentage;
            if (distancePercentage < distancePercentage - _tensionPercentage) { return; }

            float t = Mathf.InverseLerp(minTensionRange, 1, distancePercentage);

            _lineRenderer.widthMultiplier = Mathf.Lerp(_maxWidth, 0, widthAnimation.Evaluate(t));

            if (t == 1)
            {
                SpawnParticles();
                Destroy(gameObject);
            }
        }

        public void SpawnParticles()
        {
            Instantiate(_particlesPrefab, _end.position, _end.rotation);
            Instantiate(_particlesPrefab, _start.position, _start.rotation);
        }

    }
}
