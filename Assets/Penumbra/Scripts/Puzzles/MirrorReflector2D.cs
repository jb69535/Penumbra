using UnityEngine;

namespace Penumbra.Puzzles
{
    [DisallowMultipleComponent]
    public sealed class MirrorReflector2D : MonoBehaviour
    {
        public Vector2 Normal => transform.up;

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.65f, 0.9f, 1f, 0.8f);
            Gizmos.DrawLine(transform.position, transform.position + transform.up);
        }
    }
}
