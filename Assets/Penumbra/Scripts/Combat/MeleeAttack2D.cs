using Penumbra.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Penumbra.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PenumbraCharacterController2D))]
    public sealed class MeleeAttack2D : MonoBehaviour
    {
        const int HitBufferSize = 12;

        readonly Collider2D[] hitBuffer = new Collider2D[HitBufferSize];

        [SerializeField] LayerMask targetLayers = ~0;
        [SerializeField] Vector2 attackOffset = new(0.85f, 0.05f);
        [SerializeField] Vector2 attackSize = new(1.15f, 0.8f);
        [SerializeField] float damage = 10f;
        [SerializeField] Vector2 knockback = new(4f, 2f);
        [SerializeField] float cooldown = 0.22f;

        PenumbraCharacterController2D character;
        float nextAttackTime;

        public void AttackNow()
        {
            if (Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + cooldown;
            Vector2 center = (Vector2)transform.position + new Vector2(attackOffset.x * character.FacingSign, attackOffset.y);
            ContactFilter2D targetFilter = new();
            targetFilter.SetLayerMask(targetLayers);
            targetFilter.useTriggers = true;
            int hitCount = Physics2D.OverlapBox(center, attackSize, 0f, targetFilter, hitBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                if (hitBuffer[i] == null || hitBuffer[i].transform.IsChildOf(transform))
                {
                    continue;
                }

                Damageable2D damageable = hitBuffer[i].GetComponentInParent<Damageable2D>();
                if (damageable != null)
                {
                    damageable.ApplyDamage(damage, transform.position, knockback);
                }
            }
        }

        void Awake()
        {
            character = GetComponent<PenumbraCharacterController2D>();
        }

        void OnValidate()
        {
            attackSize = new Vector2(Mathf.Max(0.01f, attackSize.x), Mathf.Max(0.01f, attackSize.y));
            damage = Mathf.Max(0f, damage);
            cooldown = Mathf.Max(0f, cooldown);
        }

        void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.jKey.wasPressedThisFrame)
            {
                AttackNow();
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && gamepad.buttonWest.wasPressedThisFrame)
            {
                AttackNow();
            }
        }

        void OnDrawGizmosSelected()
        {
            if (character == null)
            {
                character = GetComponent<PenumbraCharacterController2D>();
            }

            int facing = character != null ? character.FacingSign : 1;
            Vector2 center = (Vector2)transform.position + new Vector2(attackOffset.x * facing, attackOffset.y);
            Gizmos.color = new Color(1f, 0.82f, 0.2f, 0.55f);
            Gizmos.DrawWireCube(center, attackSize);
        }
    }
}
