using UnityEngine;
using UnityEngine.InputSystem;

namespace Penumbra.Core
{
    [DisallowMultipleComponent]
    public sealed class LightShadowStateController : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] PenumbraState currentState = PenumbraState.Shadow;
        [SerializeField] bool readPrototypeInput = true;

        [Header("Resources")]
        [SerializeField] float maxLight = 100f;
        [SerializeField] float maxShadow = 100f;
        [SerializeField] float lightResource = 35f;
        [SerializeField] float shadowResource = 35f;
        [SerializeField] float lightSkillCost = 20f;
        [SerializeField] float shadowSkillCost = 20f;
        [SerializeField] float shadowSkillDuration = 2.5f;

        [Header("Visuals")]
        [SerializeField] SpriteRenderer[] tintRenderers;
        [SerializeField] Color lightTint = new(0.95f, 0.9f, 0.62f, 1f);
        [SerializeField] Color shadowTint = new(0.48f, 0.58f, 0.9f, 1f);

        float shadowSkillTimer;

        public PenumbraState CurrentState => currentState;
        public bool IsLight => currentState == PenumbraState.Light;
        public bool IsShadow => currentState == PenumbraState.Shadow;
        public bool IsShadowSkillActive => shadowSkillTimer > 0f;
        public float Light01 => maxLight <= 0f ? 0f : lightResource / maxLight;
        public float Shadow01 => maxShadow <= 0f ? 0f : shadowResource / maxShadow;

        public void ToggleState()
        {
            currentState = IsLight ? PenumbraState.Shadow : PenumbraState.Light;
            ApplyTint();
        }

        public void AddResource(PenumbraState state, float amount)
        {
            if (state == PenumbraState.Light)
            {
                lightResource = Mathf.Clamp(lightResource + amount, 0f, maxLight);
            }
            else
            {
                shadowResource = Mathf.Clamp(shadowResource + amount, 0f, maxShadow);
            }
        }

        public bool TrySpend(PenumbraState state, float amount)
        {
            if (state == PenumbraState.Light)
            {
                if (lightResource < amount)
                {
                    return false;
                }

                lightResource -= amount;
                return true;
            }

            if (shadowResource < amount)
            {
                return false;
            }

            shadowResource -= amount;
            return true;
        }

        void Reset()
        {
            tintRenderers = GetComponentsInChildren<SpriteRenderer>();
        }

        void Awake()
        {
            ClampValues();
            ApplyTint();
        }

        void OnValidate()
        {
            ClampValues();
            ApplyTint();
        }

        void Update()
        {
            shadowSkillTimer = Mathf.Max(0f, shadowSkillTimer - Time.deltaTime);

            if (!readPrototypeInput)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.pKey.wasPressedThisFrame)
                {
                    ToggleState();
                }

                if (keyboard.kKey.wasPressedThisFrame)
                {
                    UseStateSkill();
                }
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.selectButton.wasPressedThisFrame)
                {
                    ToggleState();
                }

                if (gamepad.buttonNorth.wasPressedThisFrame)
                {
                    UseStateSkill();
                }
            }
        }

        void UseStateSkill()
        {
            if (IsLight)
            {
                TrySpend(PenumbraState.Light, lightSkillCost);
                return;
            }

            if (TrySpend(PenumbraState.Shadow, shadowSkillCost))
            {
                shadowSkillTimer = shadowSkillDuration;
            }
        }

        void ApplyTint()
        {
            if (tintRenderers == null)
            {
                return;
            }

            Color tint = IsLight ? lightTint : shadowTint;
            for (int i = 0; i < tintRenderers.Length; i++)
            {
                if (tintRenderers[i] != null)
                {
                    tintRenderers[i].color = tint;
                }
            }
        }

        void ClampValues()
        {
            maxLight = Mathf.Max(1f, maxLight);
            maxShadow = Mathf.Max(1f, maxShadow);
            lightResource = Mathf.Clamp(lightResource, 0f, maxLight);
            shadowResource = Mathf.Clamp(shadowResource, 0f, maxShadow);
            lightSkillCost = Mathf.Max(0f, lightSkillCost);
            shadowSkillCost = Mathf.Max(0f, shadowSkillCost);
            shadowSkillDuration = Mathf.Max(0f, shadowSkillDuration);
        }
    }
}
