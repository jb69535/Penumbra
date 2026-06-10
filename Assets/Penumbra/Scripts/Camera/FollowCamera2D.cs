using UnityEngine;

namespace Penumbra.CameraTools
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class FollowCamera2D : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector2 offset = new(0f, 0.8f);
        [SerializeField] float followSharpness = 12f;
        [SerializeField] bool lockY = false;

        Camera followCamera;
        float lockedY;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        void Awake()
        {
            followCamera = GetComponent<Camera>();
            lockedY = transform.position.y;
        }

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desired = new(
                target.position.x + offset.x,
                lockY ? lockedY : target.position.y + offset.y,
                transform.position.z);

            float t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);

            if (followCamera != null)
            {
                followCamera.orthographic = true;
            }
        }
    }
}
