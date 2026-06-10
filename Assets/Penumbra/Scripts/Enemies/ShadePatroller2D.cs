using Penumbra.Player;
using UnityEngine;

namespace Penumbra.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class ShadePatroller2D : MonoBehaviour
    {
        [SerializeField] float speed = 1.35f;
        [SerializeField] Vector2 patrolExtents = new(2.5f, 0f);
        [SerializeField] Vector2 contactKnockback = new(7f, 4f);
        [SerializeField] float contactCooldown = 0.7f;

        Rigidbody2D body;
        Collider2D contactCollider;
        SpriteRenderer spriteRenderer;
        Vector2 startPosition;
        int direction = 1;
        float nextContactTime;

        void Reset()
        {
            CacheComponents();
            ConfigureBody();
        }

        void Awake()
        {
            CacheComponents();
            ConfigureBody();
            startPosition = transform.position;
        }

        void OnValidate()
        {
            speed = Mathf.Max(0f, speed);
            patrolExtents.x = Mathf.Max(0f, patrolExtents.x);
            contactCooldown = Mathf.Max(0f, contactCooldown);
        }

        void FixedUpdate()
        {
            Vector2 position = body.position;
            float left = startPosition.x - patrolExtents.x;
            float right = startPosition.x + patrolExtents.x;

            if (position.x <= left)
            {
                direction = 1;
            }
            else if (position.x >= right)
            {
                direction = -1;
            }

            body.linearVelocity = new Vector2(direction * speed, body.linearVelocity.y);

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction < 0;
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time < nextContactTime)
            {
                return;
            }

            PenumbraCharacterController2D character = other.GetComponentInParent<PenumbraCharacterController2D>();
            if (character == null)
            {
                return;
            }

            character.ApplyHitFrom(transform.position, contactKnockback);
            nextContactTime = Time.time + contactCooldown;
        }

        void CacheComponents()
        {
            body = GetComponent<Rigidbody2D>();
            contactCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        void ConfigureBody()
        {
            if (body != null)
            {
                body.bodyType = RigidbodyType2D.Kinematic;
                body.gravityScale = 0f;
                body.freezeRotation = true;
            }

            if (contactCollider != null)
            {
                contactCollider.isTrigger = true;
            }
        }

        void OnDrawGizmosSelected()
        {
            Vector3 center = Application.isPlaying ? (Vector3)startPosition : transform.position;
            Gizmos.color = new Color(0.78f, 0.46f, 1f, 0.55f);
            Gizmos.DrawLine(center - new Vector3(patrolExtents.x, 0f, 0f), center + new Vector3(patrolExtents.x, 0f, 0f));
        }
    }
}
