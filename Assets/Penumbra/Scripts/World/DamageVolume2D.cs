using Penumbra.Player;
using UnityEngine;

namespace Penumbra.World
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class DamageVolume2D : MonoBehaviour
    {
        [SerializeField] Vector2 knockback = new(8f, 5f);
        [SerializeField] float hitCooldown = 0.65f;

        float nextHitTime;

        void Reset()
        {
            ForceTriggerCollider();
        }

        void OnValidate()
        {
            hitCooldown = Mathf.Max(0f, hitCooldown);
            ForceTriggerCollider();
        }

        void Awake()
        {
            ForceTriggerCollider();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            TryApplyHit(other);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            TryApplyHit(other);
        }

        void TryApplyHit(Collider2D other)
        {
            if (!Application.isPlaying || Time.time < nextHitTime)
            {
                return;
            }

            PenumbraCharacterController2D character = other.GetComponentInParent<PenumbraCharacterController2D>();
            if (character == null)
            {
                return;
            }

            character.ApplyHitFrom(transform.position, knockback);
            nextHitTime = Time.time + hitCooldown;
        }

        void ForceTriggerCollider()
        {
            if (!TryGetComponent(out Collider2D hitCollider))
            {
                hitCollider = gameObject.AddComponent<BoxCollider2D>();
            }

            if (hitCollider != null)
            {
                hitCollider.isTrigger = true;
            }
        }
    }
}
