using UnityEngine;

namespace Penumbra.Combat
{
    [DisallowMultipleComponent]
    public sealed class Damageable2D : MonoBehaviour
    {
        [SerializeField] float maxHealth = 30f;
        [SerializeField] bool disableOnDeath = true;
        [SerializeField] SpriteRenderer flashRenderer;
        [SerializeField] Color hitColor = new(1f, 0.72f, 0.42f, 1f);
        [SerializeField] float hitFlashDuration = 0.08f;

        Rigidbody2D body;
        Color originalColor = Color.white;
        float currentHealth;
        float flashTimer;
        bool hasOriginalColor;

        public bool IsAlive => currentHealth > 0f;

        public void ApplyDamage(float amount, Vector2 sourcePosition, Vector2 knockback)
        {
            if (!IsAlive)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - Mathf.Max(0f, amount));
            flashTimer = hitFlashDuration;

            if (body != null && knockback.sqrMagnitude > 0f)
            {
                float direction = Mathf.Sign(((Vector2)transform.position - sourcePosition).x);
                if (Mathf.Approximately(direction, 0f))
                {
                    direction = 1f;
                }

                body.AddForce(new Vector2(direction * knockback.x, knockback.y), ForceMode2D.Impulse);
            }

            if (!IsAlive && disableOnDeath)
            {
                gameObject.SetActive(false);
            }
        }

        void Reset()
        {
            flashRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        void Awake()
        {
            currentHealth = Mathf.Max(1f, maxHealth);
            body = GetComponent<Rigidbody2D>();
            CaptureOriginalColor();
        }

        void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            hitFlashDuration = Mathf.Max(0f, hitFlashDuration);
        }

        void Update()
        {
            if (flashRenderer == null)
            {
                return;
            }

            if (flashTimer > 0f)
            {
                flashTimer = Mathf.Max(0f, flashTimer - Time.deltaTime);
                flashRenderer.color = hitColor;
            }
            else if (hasOriginalColor)
            {
                flashRenderer.color = originalColor;
            }
        }

        void CaptureOriginalColor()
        {
            if (flashRenderer == null)
            {
                return;
            }

            originalColor = flashRenderer.color;
            hasOriginalColor = true;
        }
    }
}
