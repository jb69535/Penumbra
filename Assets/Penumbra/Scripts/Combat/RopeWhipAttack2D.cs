using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Penumbra.Combat
{
    [DisallowMultipleComponent]
    public sealed class RopeWhipAttack2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform handPoint;
        [SerializeField] LineRenderer ropeLine;
        [SerializeField] SpriteRenderer ropeTip;
        [SerializeField] SpriteRenderer ropeHandle;
        [SerializeField] Collider2D ropeHitbox;

        [Header("Shape")]
        [SerializeField] int pointCount = 32;
        [FormerlySerializedAs("ropeLength")]
        [SerializeField] float maxLength = 1.65f;
        [SerializeField] float attackDuration = 0.42f;
        [SerializeField] float waveAmplitude = 0.24f;
        [SerializeField] float waveCount = 2.1f;
        [SerializeField] float waveSpeed = 2.45f;
        [SerializeField] float ropeWidth = 0.055f;
        [SerializeField] float tipScale = 0.18f;
        [SerializeField] float handleScale = 0.16f;

        [Header("Timing")]
        [SerializeField] float hitboxStartTime = 0.22f;
        [SerializeField] float hitboxEndTime = 0.31f;
        [SerializeField] float settleDuration = 0.08f;

        [Header("Placement")]
        [SerializeField] Vector3 handPointLocalRight = new(0.32f, 0.42f, 0f);
        [SerializeField] float startupLength = 0.06f;
        [SerializeField] float tipAnchorOffset;
        [SerializeField] bool facingRight = true;

        [Header("Sorting")]
        [SerializeField] string sortingLayerName = "VFX";
        [SerializeField] int ropeSortingOrder = 14;
        [SerializeField] int tipSortingOrder = 15;
        [SerializeField] int handleSortingOrder = 16;

        [Header("Visual Toggles")]
        [SerializeField] bool showTipSprite = true;
        [SerializeField] bool showHandleSprite = true;

        Vector3[] points;
        Coroutine attackRoutine;
        Transform attackSource;

        public bool IsAttacking => attackRoutine != null;
        public Transform HandPoint => handPoint;
        public Transform CurrentAttachTarget { get; private set; }
        public DistanceJoint2D ActiveDistanceJoint { get; private set; }

        public void SetFacing(bool right)
        {
            facingRight = right;
            if (!IsAttacking)
            {
                UpdateHandPointLocalPosition();
            }
        }

        public void PlayAttack(float duration = -1f)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (duration > 0f)
            {
                attackDuration = duration;
            }

            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
            }

            attackRoutine = StartCoroutine(AttackRoutine());
        }

        public void ConfigureReferences(
            Transform hand,
            LineRenderer line,
            SpriteRenderer tip,
            SpriteRenderer handle,
            Collider2D hitbox)
        {
            handPoint = hand;
            ropeLine = line;
            ropeTip = tip;
            ropeHandle = handle;
            ropeHitbox = hitbox;
            ConfigureRenderers();
            HideRope();
        }

        public void AttachToTarget(Transform target)
        {
            CurrentAttachTarget = target;
        }

        public void ClearAttachment()
        {
            CurrentAttachTarget = null;
            ActiveDistanceJoint = null;
        }

        void Awake()
        {
            attackSource = transform.root;
            AllocatePoints();
            ConfigureRenderers();
            UpdateHandPointLocalPosition();
            HideRope();
        }

        void OnValidate()
        {
            pointCount = Mathf.Max(4, pointCount);
            maxLength = Mathf.Max(0.01f, maxLength);
            attackDuration = Mathf.Max(0.01f, attackDuration);
            waveSpeed = Mathf.Max(0.01f, waveSpeed);
            ropeWidth = Mathf.Max(0.01f, ropeWidth);
            tipScale = Mathf.Max(0.01f, tipScale);
            handleScale = Mathf.Max(0.01f, handleScale);
            waveAmplitude = Mathf.Max(0f, waveAmplitude);
            waveCount = Mathf.Max(0.01f, waveCount);
            hitboxStartTime = Mathf.Max(0f, hitboxStartTime);
            hitboxEndTime = Mathf.Max(hitboxStartTime, hitboxEndTime);
            settleDuration = Mathf.Max(0f, settleDuration);
            startupLength = Mathf.Max(0.01f, startupLength);
            tipAnchorOffset = Mathf.Max(0f, tipAnchorOffset);

            AllocatePoints();
            ConfigureRenderers();
        }

        void OnDisable()
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }

            HideRope();
        }

        IEnumerator AttackRoutine()
        {
            ShowRope();
            float activeStart = Mathf.Min(hitboxStartTime, attackDuration);
            float activeEnd = Mathf.Clamp(hitboxEndTime, activeStart, attackDuration);

            if (ropeHitbox != null && ropeHitbox.TryGetComponent(out RopeHitbox2D hitbox))
            {
                hitbox.BeginSwing(attackSource);
            }

            float elapsed = 0f;
            while (elapsed < attackDuration)
            {
                DrawTravelingWave(elapsed, activeStart, activeEnd);
                SetHitboxActive(elapsed >= activeStart && elapsed <= activeEnd);

                elapsed += Time.deltaTime;
                yield return null;
            }

            DrawTravelingWave(attackDuration, activeStart, activeEnd);
            SetHitboxActive(false);

            if (settleDuration > 0f)
            {
                yield return new WaitForSeconds(settleDuration);
            }

            HideRope();
            UpdateHandPointLocalPosition();
            attackRoutine = null;
        }

        void DrawTravelingWave(float elapsed, float activeStart, float activeEnd)
        {
            if (handPoint == null || ropeLine == null || points == null || points.Length < 2)
            {
                return;
            }

            float t = Mathf.Clamp01(elapsed / attackDuration);
            Vector3 start = handPoint.position;
            float directionSign = facingRight ? 1f : -1f;
            Vector3 forward = Vector3.right * directionSign;
            Vector3 normal = Vector3.up;
            float reveal = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.72f));
            float length = Mathf.Lerp(startupLength, maxLength, reveal);
            int visiblePointCount = Mathf.Clamp(Mathf.CeilToInt(Mathf.Lerp(2f, pointCount, reveal)), 2, pointCount);
            float waveEnvelope = Mathf.Sin(t * Mathf.PI);
            float strikeProgress = GetStrikeProgress(elapsed, activeStart, activeEnd);
            float snapEnvelope = Mathf.Sin(strikeProgress * Mathf.PI);
            float lagFade = activeStart > 0f ? Mathf.SmoothStep(activeStart * 0.45f, activeStart, elapsed) : 1f;
            float lagAmount = maxLength * 0.18f * Mathf.SmoothStep(0.12f, 0.7f, t) * (1f - lagFade);
            float snapAmount = maxLength * 0.2f * snapEnvelope;
            float waveTravel = t * waveSpeed;

            for (int i = 0; i < pointCount; i++)
            {
                float s = i / (float)(pointCount - 1);
                Vector3 basePosition = start + forward * (s * length);
                float phase = (s * waveCount - waveTravel) * Mathf.PI * 2f;
                float tipInfluence = Mathf.SmoothStep(0.08f, 1f, s);
                float centerInfluence = Mathf.Sin(s * Mathf.PI);
                float travelingPulse = Mathf.Exp(-Mathf.Pow((s - Mathf.Clamp01(waveTravel / Mathf.Max(waveCount, 0.01f))) * 3.2f, 2f));
                float primaryWave = Mathf.Sin(phase);
                float counterWave = Mathf.Sin(phase * 0.55f + Mathf.PI * 0.35f) * 0.45f;
                float wave = (primaryWave + counterWave) * waveAmplitude * waveEnvelope * Mathf.Lerp(centerInfluence, tipInfluence, 0.55f);
                float crackLift = travelingPulse * waveAmplitude * 0.36f * waveEnvelope;
                float tipSnapInfluence = Mathf.SmoothStep(0.58f, 1f, s);
                float settleDrop = Mathf.SmoothStep(0.74f, 1f, t) * s * 0.2f;

                points[i] = basePosition
                    + normal * (wave + crackLift)
                    + forward * ((snapAmount - lagAmount) * tipSnapInfluence)
                    + Vector3.down * settleDrop;
            }

            ropeLine.positionCount = visiblePointCount;
            for (int i = 0; i < visiblePointCount; i++)
            {
                ropeLine.SetPosition(i, points[i]);
            }

            UpdateTipAndHandle(start, visiblePointCount);
            UpdateHitbox(points[visiblePointCount - 1]);
        }

        float GetStrikeProgress(float elapsed, float activeStart, float activeEnd)
        {
            if (activeEnd <= activeStart)
            {
                return elapsed >= activeStart ? 1f : 0f;
            }

            return Mathf.Clamp01(Mathf.InverseLerp(activeStart, activeEnd, elapsed));
        }

        void UpdateTipAndHandle(Vector3 start, int visiblePointCount)
        {
            int tipIndex = Mathf.Clamp(visiblePointCount - 1, 1, points.Length - 1);
            Vector3 end = points[tipIndex];
            Vector3 previous = points[tipIndex - 1];
            Vector3 tangent = (end - previous).sqrMagnitude > 0.0001f ? (end - previous).normalized : Vector3.right;

            if (ropeTip != null)
            {
                ropeTip.enabled = showTipSprite;
                if (showTipSprite)
                {
                    ropeTip.transform.position = end - tangent * tipAnchorOffset;
                    ropeTip.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg);
                    ropeTip.transform.localScale = Vector3.one * tipScale;
                    ropeTip.flipX = !facingRight;
                }
            }

            if (ropeHandle != null)
            {
                ropeHandle.enabled = showHandleSprite;
                if (showHandleSprite)
                {
                    ropeHandle.transform.position = start;
                    ropeHandle.transform.rotation = Quaternion.identity;
                    ropeHandle.transform.localScale = Vector3.one * handleScale;
                    ropeHandle.flipX = !facingRight;
                }
            }
        }

        void UpdateHitbox(Vector3 tipPosition)
        {
            if (ropeHitbox != null)
            {
                ropeHitbox.transform.position = tipPosition;
            }
        }

        void ConfigureRenderers()
        {
            if (ropeLine != null)
            {
                ropeLine.positionCount = pointCount;
                ropeLine.useWorldSpace = true;
                ropeLine.widthMultiplier = ropeWidth;
                ropeLine.startWidth = ropeWidth;
                ropeLine.endWidth = ropeWidth;
                ropeLine.textureMode = LineTextureMode.Tile;
                ropeLine.numCapVertices = 4;
                ropeLine.numCornerVertices = 2;
                ropeLine.alignment = LineAlignment.View;
                ropeLine.sortingLayerName = sortingLayerName;
                ropeLine.sortingOrder = ropeSortingOrder;
            }

            ConfigureSpriteRenderer(ropeTip, tipSortingOrder);
            ConfigureSpriteRenderer(ropeHandle, handleSortingOrder);
        }

        void ConfigureSpriteRenderer(SpriteRenderer target, int sortingOrder)
        {
            if (target == null)
            {
                return;
            }

            target.sortingLayerName = sortingLayerName;
            target.sortingOrder = sortingOrder;
        }

        void AllocatePoints()
        {
            if (points == null || points.Length != pointCount)
            {
                points = new Vector3[pointCount];
            }
        }

        void ShowRope()
        {
            if (ropeLine != null)
            {
                ropeLine.enabled = true;
            }

            SetHitboxActive(false);
        }

        void HideRope()
        {
            if (ropeLine != null)
            {
                ropeLine.enabled = false;
            }

            if (ropeTip != null)
            {
                ropeTip.enabled = false;
            }

            if (ropeHandle != null)
            {
                ropeHandle.enabled = false;
            }

            SetHitboxActive(false);
        }

        void SetHitboxActive(bool active)
        {
            if (ropeHitbox != null)
            {
                ropeHitbox.enabled = active;
            }
        }

        void UpdateHandPointLocalPosition()
        {
            if (handPoint == null)
            {
                return;
            }

            handPoint.localPosition = new Vector3(
                facingRight ? handPointLocalRight.x : -handPointLocalRight.x,
                handPointLocalRight.y,
                handPointLocalRight.z);
        }
    }
}
