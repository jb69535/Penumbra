using System.Collections;
using UnityEngine;

namespace Penumbra.Combat
{
    [DisallowMultipleComponent]
    public sealed class RopeController2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform handPoint;
        [SerializeField] LineRenderer ropeLine;
        [SerializeField] SpriteRenderer ropeTip;
        [SerializeField] SpriteRenderer ropeHandle;
        [SerializeField] Collider2D ropeHitbox;

        [Header("Whip Settings")]
        [SerializeField] int pointCount = 32;
        [SerializeField] float maxLength = 1.65f;
        [SerializeField] float swingDuration = 0.5f;
        [SerializeField] float ropeWidth = 0.055f;
        [SerializeField] float waveAmplitude = 0.24f;
        [SerializeField] float waveCount = 2.1f;
        [SerializeField] float waveSpeed = 2.45f;
        [SerializeField] float tipScale = 0.18f;
        [SerializeField] float handleScale = 0.16f;
        [SerializeField] float hitboxStartTime = 0.22f;
        [SerializeField] float hitboxEndTime = 0.31f;
        [SerializeField] float startupLength = 0.06f;
        [SerializeField] float settleDuration = 0.08f;
        [SerializeField] Vector3 handPointLocalRight = new(0.32f, 0.42f, 0f);

        [Header("Visual")]
        [SerializeField] bool showHandleSprite = true;
        [SerializeField] bool showTipSprite = true;
        [SerializeField] float tipAnchorOffset;
        [SerializeField] Vector2 handleLocalOffset;
        [SerializeField] Vector2 ropeAttachLocalRight = new(0.078f, 0f);

        [Header("Facing")]
        [SerializeField] bool facingRight = true;

        Vector3[] points;
        Coroutine attackRoutine;
        Transform playerRoot;

        public bool IsSwinging => attackRoutine != null;

        public void SetFacing(bool right)
        {
            facingRight = right;
        }

        public void PlaySwingAttack(float duration = -1f)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
            }

            if (duration > 0f)
            {
                swingDuration = duration;
            }

            attackRoutine = StartCoroutine(WhipAttackRoutine());
        }

        void Awake()
        {
            playerRoot = transform.root;
            pointCount = Mathf.Max(2, pointCount);
            points = new Vector3[pointCount];
            ConfigureLineRenderer();
            CacheHandleAttachOffset();
            AttachHandleToHandPoint();
            HideRope();
        }

        void OnValidate()
        {
            pointCount = Mathf.Max(2, pointCount);
            ropeWidth = Mathf.Max(0.01f, ropeWidth);
            maxLength = Mathf.Max(0.01f, maxLength);
            swingDuration = Mathf.Max(0.01f, swingDuration);
            waveAmplitude = Mathf.Max(0f, waveAmplitude);
            waveCount = Mathf.Max(0.01f, waveCount);
            waveSpeed = Mathf.Max(0.01f, waveSpeed);
            tipScale = Mathf.Max(0.01f, tipScale);
            handleScale = Mathf.Max(0.01f, handleScale);
            hitboxStartTime = Mathf.Max(0f, hitboxStartTime);
            hitboxEndTime = Mathf.Max(hitboxStartTime, hitboxEndTime);
            startupLength = Mathf.Max(0.01f, startupLength);
            settleDuration = Mathf.Max(0f, settleDuration);
            tipAnchorOffset = Mathf.Max(0f, tipAnchorOffset);
            ConfigureLineRenderer();
            CacheHandleAttachOffset();
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

        void ConfigureLineRenderer()
        {
            if (ropeLine == null)
            {
                return;
            }

            ropeLine.positionCount = pointCount;
            ropeLine.useWorldSpace = true;
            ropeLine.widthMultiplier = ropeWidth;
            ropeLine.startWidth = ropeWidth;
            ropeLine.endWidth = ropeWidth;
            ropeLine.textureMode = LineTextureMode.Tile;
            ropeLine.numCapVertices = 4;
            ropeLine.numCornerVertices = 2;
            ropeLine.alignment = LineAlignment.View;
        }

        IEnumerator WhipAttackRoutine()
        {
            ShowRope();
            float activeStart = Mathf.Min(hitboxStartTime, swingDuration);
            float activeEnd = Mathf.Clamp(hitboxEndTime, activeStart, swingDuration);

            if (ropeHitbox != null && ropeHitbox.TryGetComponent(out RopeHitbox2D hitbox))
            {
                hitbox.BeginSwing(playerRoot);
            }

            float elapsed = 0f;

            while (elapsed < swingDuration)
            {
                DrawWhipWave(elapsed, activeStart, activeEnd);

                if (ropeHitbox != null)
                {
                    ropeHitbox.enabled = elapsed >= activeStart && elapsed <= activeEnd;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            DrawWhipWave(swingDuration, activeStart, activeEnd);

            if (ropeHitbox != null)
            {
                ropeHitbox.enabled = false;
            }

            if (settleDuration > 0f)
            {
                yield return new WaitForSeconds(settleDuration);
            }

            HideRope();
            attackRoutine = null;
        }

        void DrawWhipWave(float elapsed, float activeStart, float activeEnd)
        {
            if (handPoint == null || ropeLine == null || points == null || points.Length == 0)
            {
                return;
            }

            float t = Mathf.Clamp01(elapsed / swingDuration);
            UpdateHandleAtWrist();
            Vector3 start = GetRopeAttachPosition();
            float dir = facingRight ? 1f : -1f;
            Vector3 direction = new Vector3(dir, 0f, 0f);
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
                Vector3 basePos = start + direction * (s * length);
                float wavePhase = (s * waveCount - waveTravel) * Mathf.PI * 2f;
                float tipInfluence = Mathf.SmoothStep(0.1f, 1f, s);
                float centerInfluence = Mathf.Sin(s * Mathf.PI);
                float travelingPulse = Mathf.Exp(-Mathf.Pow((s - Mathf.Clamp01(waveTravel / Mathf.Max(waveCount, 0.01f))) * 3.2f, 2f));
                float primaryWave = Mathf.Sin(wavePhase);
                float counterWave = Mathf.Sin(wavePhase * 0.55f + Mathf.PI * 0.35f) * 0.45f;
                float wave = (primaryWave + counterWave) * waveAmplitude * waveEnvelope * Mathf.Lerp(centerInfluence, tipInfluence, 0.55f);
                float crackLift = travelingPulse * waveAmplitude * 0.36f * waveEnvelope;
                float tipSnapInfluence = Mathf.SmoothStep(0.58f, 1f, s);
                float drop = Mathf.SmoothStep(0.74f, 1f, t) * s * 0.2f;

                points[i] = basePos
                    + normal * (wave + crackLift)
                    + direction * ((snapAmount - lagAmount) * tipSnapInfluence)
                    + Vector3.down * drop;
            }

            ropeLine.enabled = true;
            ropeLine.positionCount = visiblePointCount;
            for (int i = 0; i < visiblePointCount; i++)
            {
                ropeLine.SetPosition(i, points[i]);
            }

            int tipIndex = visiblePointCount - 1;
            Vector3 end = points[tipIndex];
            Vector3 prev = points[Mathf.Max(0, tipIndex - 1)];
            Vector3 tangent = (end - prev).sqrMagnitude > 0.0001f ? (end - prev).normalized : direction;

            if (showTipSprite && ropeTip != null)
            {
                ropeTip.enabled = true;
                ropeTip.transform.position = end - tangent * tipAnchorOffset;
                float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
                ropeTip.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                ropeTip.transform.localScale = Vector3.one * tipScale;
            }
            else if (ropeTip != null)
            {
                ropeTip.enabled = false;
            }

            if (showHandleSprite && ropeHandle != null)
            {
                ropeHandle.enabled = true;
            }
            else if (ropeHandle != null)
            {
                ropeHandle.enabled = false;
            }

            if (ropeHitbox != null)
            {
                ropeHitbox.transform.position = end;
            }
        }

        void AttachHandleToHandPoint()
        {
            if (ropeHandle == null || handPoint == null)
            {
                return;
            }

            Transform handleTransform = ropeHandle.transform;
            if (handleTransform.parent != handPoint)
            {
                handleTransform.SetParent(handPoint, false);
            }

            handleTransform.localPosition = new Vector3(handleLocalOffset.x, handleLocalOffset.y, 0f);
            handleTransform.localRotation = Quaternion.identity;
        }

        void UpdateHandleAtWrist()
        {
            if (!showHandleSprite || ropeHandle == null || handPoint == null)
            {
                return;
            }

            AttachHandleToHandPoint();
            float flip = facingRight ? 1f : -1f;
            ropeHandle.transform.localScale = new Vector3(handleScale * flip, handleScale, 1f);
            ropeHandle.enabled = true;
        }

        Vector3 GetRopeAttachPosition()
        {
            if (handPoint == null)
            {
                return transform.position;
            }

            float dir = facingRight ? 1f : -1f;
            Vector3 localAttach = new Vector3(ropeAttachLocalRight.x * dir, ropeAttachLocalRight.y, 0f);
            return handPoint.TransformPoint(localAttach);
        }

        void CacheHandleAttachOffset()
        {
            if (ropeHandle == null || ropeHandle.sprite == null)
            {
                return;
            }

            float halfWidth = ropeHandle.sprite.bounds.extents.x * handleScale;
            ropeAttachLocalRight = new Vector2(halfWidth * 0.95f, handleLocalOffset.y);
        }

        float GetStrikeProgress(float elapsed, float activeStart, float activeEnd)
        {
            if (activeEnd <= activeStart)
            {
                return elapsed >= activeStart ? 1f : 0f;
            }

            return Mathf.Clamp01(Mathf.InverseLerp(activeStart, activeEnd, elapsed));
        }

        void ShowRope()
        {
            if (ropeLine != null)
            {
                ropeLine.enabled = true;
            }

            if (ropeHitbox != null)
            {
                ropeHitbox.enabled = false;
            }
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

            if (ropeHitbox != null)
            {
                ropeHitbox.enabled = false;
            }
        }

        void UpdateHandPointLocalPosition(Vector3 localRight)
        {
            if (handPoint == null)
            {
                return;
            }

            float x = facingRight ? localRight.x : -localRight.x;
            handPoint.localPosition = new Vector3(x, localRight.y, localRight.z);
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
            ConfigureLineRenderer();
            CacheHandleAttachOffset();
            AttachHandleToHandPoint();
        }
    }
}
