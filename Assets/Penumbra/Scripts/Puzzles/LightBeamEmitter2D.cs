using System.Collections.Generic;
using Penumbra.Core;
using UnityEngine;

namespace Penumbra.Puzzles
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class LightBeamEmitter2D : MonoBehaviour
    {
        readonly List<Vector3> beamPoints = new();

        [SerializeField] LayerMask hitMask = ~0;
        [SerializeField] float maxDistance = 14f;
        [SerializeField] int maxReflections = 2;
        [SerializeField] LightShadowStateController stateSource;
        [SerializeField] float lightStateRangeMultiplier = 1.15f;
        [SerializeField] float shadowStateRangeMultiplier = 0.65f;
        [SerializeField] Color beamColor = new(1f, 0.9f, 0.42f, 0.95f);

        LineRenderer beamLine;

        public void SetStateSource(LightShadowStateController source)
        {
            stateSource = source;
        }

        void Reset()
        {
            ConfigureLine();
        }

        void Awake()
        {
            ConfigureLine();
        }

        void OnValidate()
        {
            maxDistance = Mathf.Max(0.1f, maxDistance);
            maxReflections = Mathf.Max(0, maxReflections);
            lightStateRangeMultiplier = Mathf.Max(0f, lightStateRangeMultiplier);
            shadowStateRangeMultiplier = Mathf.Max(0f, shadowStateRangeMultiplier);
            ConfigureLine();
        }

        void LateUpdate()
        {
            EmitBeam();
        }

        void EmitBeam()
        {
            ConfigureLine();
            beamPoints.Clear();

            float stateMultiplier = 1f;
            if (stateSource != null)
            {
                stateMultiplier = stateSource.IsLight ? lightStateRangeMultiplier : shadowStateRangeMultiplier;
            }

            float remainingDistance = maxDistance * stateMultiplier;
            Vector2 origin = transform.position;
            Vector2 direction = transform.right;
            beamPoints.Add(origin);

            for (int reflection = 0; reflection <= maxReflections && remainingDistance > 0f; reflection++)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, direction, remainingDistance, hitMask);
                if (hit.collider == null)
                {
                    beamPoints.Add(origin + direction * remainingDistance);
                    break;
                }

                beamPoints.Add(hit.point);

                LightReceiver2D receiver = hit.collider.GetComponentInParent<LightReceiver2D>();
                if (receiver != null)
                {
                    receiver.ReceiveBeam(this);
                    break;
                }

                MirrorReflector2D mirror = hit.collider.GetComponentInParent<MirrorReflector2D>();
                if (mirror == null)
                {
                    break;
                }

                remainingDistance -= hit.distance;
                direction = Vector2.Reflect(direction, mirror.Normal).normalized;
                origin = hit.point + direction * 0.03f;
            }

            beamLine.positionCount = beamPoints.Count;
            for (int i = 0; i < beamPoints.Count; i++)
            {
                beamLine.SetPosition(i, beamPoints[i]);
            }
        }

        void ConfigureLine()
        {
            if (beamLine == null && !TryGetComponent(out beamLine))
            {
                return;
            }

            beamLine.useWorldSpace = true;
            beamLine.positionCount = 0;
            beamLine.startWidth = 0.08f;
            beamLine.endWidth = 0.05f;
            beamLine.startColor = beamColor;
            beamLine.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, 0.35f);

            if (beamLine.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    beamLine.sharedMaterial = new Material(shader);
                }
            }
        }
    }
}
