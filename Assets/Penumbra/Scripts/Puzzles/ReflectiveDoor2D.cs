using UnityEngine;

namespace Penumbra.Puzzles
{
    [DisallowMultipleComponent]
    public sealed class ReflectiveDoor2D : MonoBehaviour
    {
        [SerializeField] Collider2D blockingCollider;
        [SerializeField] SpriteRenderer doorRenderer;
        [SerializeField] Color closedColor = new(0.35f, 0.42f, 0.58f, 1f);
        [SerializeField] Color openColor = new(0.35f, 0.42f, 0.58f, 0.2f);

        bool isOpen;

        public void SetOpen(bool open)
        {
            if (isOpen == open)
            {
                return;
            }

            isOpen = open;
            ApplyState();
        }

        void Reset()
        {
            blockingCollider = GetComponent<Collider2D>();
            doorRenderer = GetComponentInChildren<SpriteRenderer>();
            ApplyState();
        }

        void Awake()
        {
            if (blockingCollider == null)
            {
                blockingCollider = GetComponent<Collider2D>();
            }

            if (doorRenderer == null)
            {
                doorRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            ApplyState();
        }

        void ApplyState()
        {
            if (blockingCollider != null)
            {
                blockingCollider.enabled = !isOpen;
            }

            if (doorRenderer != null)
            {
                doorRenderer.color = isOpen ? openColor : closedColor;
            }
        }
    }
}
