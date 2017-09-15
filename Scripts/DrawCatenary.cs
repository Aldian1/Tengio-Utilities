namespace Tengio
{
    using UnityEngine;

    public class DrawCatenary : MonoBehaviour
    {
        [SerializeField]
        private GameObject segmentPrefab;
        [SerializeField]
        private Transform startTransform;
        [SerializeField]
        private Transform endTransform;
        [SerializeField]
        private int steps;
        [SerializeField]
        private Space space;
        [SerializeField]
        private float cableWidth = 0.08f;
        [SerializeField]
        private float overlap = 0.05f;

        private Catenary catenary;
        private GameObject[] segments;
        private Vector3 previousStartPosition;
        private Vector3 previousEndPosition;
        private Vector3 initialSegmentScale;


        public Transform StartTransform
        {
            get
            {
                return startTransform;
            }

            set
            {
                startTransform = value;
            }
        }

        public Transform EndTransform
        {
            get
            {
                return endTransform;
            }

            set
            {
                endTransform = value;
            }
        }


        private void Awake()
        {
            segments = new GameObject[steps];
            space = Space.World;
            catenary = new Catenary(startTransform.position, endTransform.position, steps);
        }

        private void Start()
        {
            previousStartPosition = startTransform.position;
            previousEndPosition = endTransform.position;
            InstantiateSegments();
            DrawCable();
        }

        private void LateUpdate()
        {
            if (startTransform.position != previousStartPosition || endTransform.position != previousEndPosition)
            {
                Redraw();
            }
            previousStartPosition = startTransform.position;
            previousEndPosition = endTransform.position;
        }

        private void InstantiateSegments()
        {
            Transform segmentsParent = new GameObject().transform;
            segmentsParent.parent = transform;
            segmentsParent.name = "CatenarySegments";
            for (int i = 0; i < steps - 1; i++)
            {
                segments[i] = Instantiate(segmentPrefab, segmentsParent);
            }
            initialSegmentScale = segments[0].transform.lossyScale;
            initialSegmentScale.x = cableWidth;
        }

        private void DrawCable()
        {
            Vector3[] points;
            catenary.ShouldRegenPoints = true;
            catenary.StartPoint = startTransform.position;
            catenary.EndPoint = endTransform.position;
            points = catenary.GetPoints();

            if (space == Space.Self)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = transform.worldToLocalMatrix.MultiplyPoint(points[i]);
                }
            }

            for (int i = 0; i < points.Length - 1; i++)
            {
                float distance = Vector3.Distance(points[i], points[i + 1]);
                Vector3 direction = points[i] - points[i + 1];
                float angle = Vector3.Angle(transform.up, direction);
                angle *= Mathf.Sign(Vector3.Cross(transform.up, direction).z); // Get the direction of the rotation.
                Vector3 newPosition = (points[i] + points[i + 1]) / 2;
                Vector3 newScale = initialSegmentScale;
                newScale.y *= (1 + overlap) * distance;

                segments[i].transform.position = newPosition;
                segments[i].transform.localScale = newScale;
                segments[i].transform.rotation = Quaternion.identity;
                segments[i].transform.Rotate(transform.forward, angle);
            }
        }

        private void Redraw()
        {
            DrawCable();
        }
    }
}