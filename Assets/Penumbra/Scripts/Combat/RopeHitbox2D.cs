using System.Collections.Generic;
using UnityEngine;

namespace Penumbra.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class RopeHitbox2D : MonoBehaviour
    {
        [SerializeField] float damage = 14f;
        [SerializeField] Vector2 knockback = new(6.5f, 2.2f);
        [SerializeField] LayerMask targetLayers = ~0;

        readonly HashSet<Damageable2D> hitThisSwing = new();
        Transform attackSource;

        public void BeginSwing(Transform source)
        {
            attackSource = source;
            hitThisSwing.Clear();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActiveAndEnabled || other == null)
            {
                return;
            }

            if (((1 << other.gameObject.layer) & targetLayers.value) == 0)
            {
                return;
            }

            if (attackSource != null && other.transform.IsChildOf(attackSource))
            {
                return;
            }

            Damageable2D damageable = other.GetComponentInParent<Damageable2D>();
            if (damageable == null || hitThisSwing.Contains(damageable))
            {
                return;
            }

            Vector2 sourcePosition = attackSource != null ? attackSource.position : transform.position;
            damageable.ApplyDamage(damage, sourcePosition, knockback);
            hitThisSwing.Add(damageable);
        }

        void OnValidate()
        {
            damage = Mathf.Max(0f, damage);
        }
    }
}
