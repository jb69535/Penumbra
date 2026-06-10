using UnityEngine;

namespace Penumbra.World
{
    [DisallowMultipleComponent]
    public sealed class ParallaxLayer2D : MonoBehaviour
    {
        [SerializeField] Transform cameraTransform;
        [SerializeField, Range(0f, 1.5f)] float horizontalStrength = 0.5f;
        [SerializeField, Range(0f, 1.5f)] float verticalStrength = 0.15f;

        Vector3 startPosition;
        Vector3 cameraStartPosition;

        public void Configure(Transform newCameraTransform, float horizontal, float vertical)
        {
            cameraTransform = newCameraTransform;
            horizontalStrength = Mathf.Clamp(horizontal, 0f, 1.5f);
            verticalStrength = Mathf.Clamp(vertical, 0f, 1.5f);
            CaptureStartPositions();
        }

        void Awake()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            CaptureStartPositions();
        }

        void CaptureStartPositions()
        {
            startPosition = transform.position;
            cameraStartPosition = cameraTransform != null ? cameraTransform.position : Vector3.zero;
        }

        void LateUpdate()
        {
            if (cameraTransform == null)
            {
                return;
            }

            Vector3 delta = cameraTransform.position - cameraStartPosition;
            transform.position = startPosition + new Vector3(
                delta.x * horizontalStrength,
                delta.y * verticalStrength,
                0f);
        }
    }
}
