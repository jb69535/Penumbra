using System.Collections.Generic;
using Penumbra.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Penumbra.Combat
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PenumbraCharacterController2D))]
    public sealed class ChainAttack2D : MonoBehaviour
    {
        const string ChainVisualName = "Sample Chain Visual";
        const int HitBufferSize = 16;

        static Material runtimeLineMaterial;

        readonly Collider2D[] hitBuffer = new Collider2D[HitBufferSize];
        readonly HashSet<Damageable2D> hitThisSwing = new();

        [Header("Input")]
        [SerializeField] bool readInput = true;

        [Header("Attack")]
        [SerializeField] LayerMask targetLayers = ~0;
        [SerializeField] float attackRange = 2.75f;
        [SerializeField] float swingDuration = 0.26f;
        [SerializeField] float cooldown = 0.42f;
        [SerializeField] float damage = 14f;
        [SerializeField] Vector2 knockback = new(6.5f, 2.2f);
        [SerializeField] float hitRadius = 0.22f;

        [Header("Visual")]
        [SerializeField] Vector2 anchorOffset = new(0.35f, 0.32f);
        [SerializeField] float idleLength = 0.58f;
        [SerializeField] float idleDrop = 0.42f;
        [SerializeField] float swingStartAngle = -18f;
        [SerializeField] float swingEndAngle = 22f;
        [SerializeField] float chainSlack = 0.18f;
        [SerializeField] float whipBend = 0.2f;
        [SerializeField] int visualSegments = 7;
        [SerializeField] float lineWidth = 0.08f;
        [SerializeField] bool showIdleChain = true;
        [SerializeField] string sortingLayerName = "VFX";
        [SerializeField] int sortingOrder = 14;
        [SerializeField] Color chainColor = new(0.82f, 0.9f, 1f, 1f);
        [SerializeField] Color activeChainColor = new(1f, 0.9f, 0.42f, 1f);

        PenumbraCharacterController2D character;
        LineRenderer lineRenderer;
        Transform chainVisual;
        Vector3[] chainPoints;
        float swingTimer;
        float nextAttackTime;

        bool IsSwinging => swingTimer > 0f;

        public void AttackNow()
        {
            if (!Application.isPlaying || Time.time < nextAttackTime)
            {
                return;
            }

            swingTimer = swingDuration;
            nextAttackTime = Time.time + cooldown;
            hitThisSwing.Clear();
            EnsureVisual();
            UpdateChainVisual();
        }

        void Reset()
        {
            CacheComponents();
            EnsureVisual();
            UpdateChainVisual();
        }

        void Awake()
        {
            CacheComponents();
            EnsureVisual();
            UpdateChainVisual();
        }

        void OnEnable()
        {
            CacheComponents();
            EnsureVisual();
            UpdateChainVisual();
        }

        void OnValidate()
        {
            attackRange = Mathf.Max(0.01f, attackRange);
            swingDuration = Mathf.Max(0.01f, swingDuration);
            cooldown = Mathf.Max(0f, cooldown);
            damage = Mathf.Max(0f, damage);
            hitRadius = Mathf.Max(0.01f, hitRadius);
            idleLength = Mathf.Max(0f, idleLength);
            idleDrop = Mathf.Max(0f, idleDrop);
            visualSegments = Mathf.Max(1, visualSegments);
            lineWidth = Mathf.Max(0.01f, lineWidth);
            chainSlack = Mathf.Max(0f, chainSlack);
            whipBend = Mathf.Max(0f, whipBend);

            if (!gameObject.scene.IsValid())
            {
                return;
            }

            CacheComponents(false);
            EnsureVisual();
            UpdateChainVisual();
        }

        void OnDisable()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                UpdateChainVisual();
                return;
            }

            if (readInput)
            {
                ReadInput();
            }

            if (IsSwinging)
            {
                swingTimer = Mathf.Max(0f, swingTimer - Time.deltaTime);
                UpdateChainVisual();
                DamageAlongChain();
            }
            else
            {
                UpdateChainVisual();
            }
        }

        void ReadInput()
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

        void DamageAlongChain()
        {
            if (chainPoints == null || chainPoints.Length == 0)
            {
                return;
            }

            ContactFilter2D targetFilter = new();
            targetFilter.SetLayerMask(targetLayers);
            targetFilter.useTriggers = true;

            for (int pointIndex = 1; pointIndex < chainPoints.Length; pointIndex++)
            {
                int hitCount = Physics2D.OverlapCircle(chainPoints[pointIndex], hitRadius, targetFilter, hitBuffer);
                for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
                {
                    Collider2D hitCollider = hitBuffer[hitIndex];
                    if (hitCollider == null || hitCollider.transform.IsChildOf(transform))
                    {
                        continue;
                    }

                    Damageable2D damageable = hitCollider.GetComponentInParent<Damageable2D>();
                    if (damageable == null || hitThisSwing.Contains(damageable))
                    {
                        continue;
                    }

                    damageable.ApplyDamage(damage, transform.position, knockback);
                    hitThisSwing.Add(damageable);
                }
            }
        }

        void CacheComponents(bool createMissing = true)
        {
            if (character == null && !TryGetComponent(out character) && createMissing)
            {
                character = gameObject.AddComponent<PenumbraCharacterController2D>();
            }

            if (chainVisual == null)
            {
                chainVisual = transform.Find(ChainVisualName);
            }

            if (chainVisual == null && createMissing)
            {
                GameObject visual = new(ChainVisualName);
                visual.transform.SetParent(transform, false);
                chainVisual = visual.transform;
            }

            if (chainVisual == null)
            {
                return;
            }

            if (lineRenderer == null && !chainVisual.TryGetComponent(out lineRenderer) && createMissing)
            {
                lineRenderer = chainVisual.gameObject.AddComponent<LineRenderer>();
            }
        }

        void EnsureVisual()
        {
            CacheComponents();

            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = Mathf.Max(2, visualSegments + 1);
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth * 0.72f;
            lineRenderer.numCapVertices = 3;
            lineRenderer.numCornerVertices = 2;
            lineRenderer.sortingLayerName = string.IsNullOrWhiteSpace(sortingLayerName) ? "Default" : sortingLayerName;
            lineRenderer.sortingOrder = sortingOrder;

            Material material = GetRuntimeLineMaterial();
            if (material != null)
            {
                lineRenderer.sharedMaterial = material;
            }

            EnsurePointBuffer();
        }

        void EnsurePointBuffer()
        {
            int pointCount = Mathf.Max(2, visualSegments + 1);
            if (chainPoints == null || chainPoints.Length != pointCount)
            {
                chainPoints = new Vector3[pointCount];
            }

            if (lineRenderer != null && lineRenderer.positionCount != pointCount)
            {
                lineRenderer.positionCount = pointCount;
            }
        }

        void UpdateChainVisual()
        {
            EnsurePointBuffer();

            if (chainPoints == null)
            {
                return;
            }

            int facing = character != null ? character.FacingSign : 1;
            Vector2 anchor = GetAnchor(facing);
            float progress = IsSwinging ? 1f - swingTimer / swingDuration : 0f;
            float reach = IsSwinging ? attackRange * Mathf.Sin(progress * Mathf.PI) : idleLength;
            float angle = IsSwinging ? Mathf.Lerp(swingStartAngle, swingEndAngle, progress) : -Mathf.Atan2(idleDrop, Mathf.Max(0.01f, idleLength)) * Mathf.Rad2Deg;
            float angleRadians = angle * Mathf.Deg2Rad;
            Vector2 direction = new(Mathf.Cos(angleRadians) * facing, Mathf.Sin(angleRadians));
            Vector2 tip = anchor + direction * reach;

            for (int i = 0; i < chainPoints.Length; i++)
            {
                float t = i / (float)(chainPoints.Length - 1);
                Vector2 point = Vector2.Lerp(anchor, tip, t);
                float slack = Mathf.Sin(t * Mathf.PI) * chainSlack;
                float bend = IsSwinging ? Mathf.Sin(progress * Mathf.PI * 2f) * Mathf.Sin(t * Mathf.PI) * whipBend : 0f;

                point.x += bend * facing;
                point.y -= slack;
                chainPoints[i] = new Vector3(point.x, point.y, transform.position.z);
            }

            if (lineRenderer != null)
            {
                lineRenderer.enabled = showIdleChain || IsSwinging;
                lineRenderer.startColor = IsSwinging ? activeChainColor : chainColor;
                lineRenderer.endColor = IsSwinging ? activeChainColor : chainColor;
                lineRenderer.SetPositions(chainPoints);
            }
        }

        Vector2 GetAnchor(int facing)
        {
            return (Vector2)transform.position + new Vector2(anchorOffset.x * facing, anchorOffset.y);
        }

        static Material GetRuntimeLineMaterial()
        {
            if (runtimeLineMaterial != null)
            {
                return runtimeLineMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            runtimeLineMaterial = new Material(shader)
            {
                name = "Runtime Sample Chain Line",
                hideFlags = HideFlags.HideAndDontSave
            };
            return runtimeLineMaterial;
        }

        void OnDrawGizmosSelected()
        {
            int facing = character != null ? character.FacingSign : 1;
            Vector2 anchor = GetAnchor(facing);
            Gizmos.color = new Color(1f, 0.86f, 0.28f, 0.55f);
            Gizmos.DrawWireSphere(anchor, hitRadius);
            Gizmos.DrawLine(anchor, anchor + Vector2.right * facing * attackRange);

            int samples = Mathf.Max(1, visualSegments);
            for (int i = 1; i <= samples; i++)
            {
                float t = i / (float)samples;
                Vector2 sample = anchor + Vector2.right * facing * attackRange * t;
                Gizmos.DrawWireSphere(sample, hitRadius);
            }
        }
    }
}
