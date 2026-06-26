using UnityEngine;
using UnityEngine.InputSystem;

namespace Penumbra.CameraTools
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class FollowCamera2D : MonoBehaviour
    {
        static FollowCamera2D activeCamera;

        [SerializeField] Transform target;
        [SerializeField] Vector2 offset = new(0f, 0.8f);
        [SerializeField] float followSharpness = 12f;
        [SerializeField] Vector2 deadZone = new(0.16f, 0.18f);
        [SerializeField] float verticalLookOffset = 1.25f;
        [SerializeField] float verticalLookDelay = 0.38f;
        [SerializeField] float verticalLookSharpness = 8f;
        [SerializeField] bool lockY = false;

        Camera followCamera;
        Vector2 currentShakeOffset;
        float lockedY;
        float verticalLookTimer;
        float currentVerticalLook;
        float shakeTimer;
        float shakeDuration;
        float shakeIntensity;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public static void ShakeActiveCamera(float intensity, float duration)
        {
            if (activeCamera != null)
            {
                activeCamera.Shake(intensity, duration);
            }
        }

        public void Shake(float intensity, float duration)
        {
            if (intensity <= 0f || duration <= 0f)
            {
                return;
            }

            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
            shakeDuration = Mathf.Max(shakeDuration, duration);
            shakeTimer = Mathf.Max(shakeTimer, duration);
        }

        void Awake()
        {
            followCamera = GetComponent<Camera>();
            lockedY = transform.position.y;
        }

        void OnEnable()
        {
            activeCamera = this;
        }

        void OnDisable()
        {
            if (activeCamera == this)
            {
                activeCamera = null;
            }
        }

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;
            UpdateVerticalLook(deltaTime);

            Vector3 basePosition = transform.position - new Vector3(currentShakeOffset.x, currentShakeOffset.y, 0f);
            Vector2 tracked = new(
                target.position.x + offset.x,
                lockY ? lockedY : target.position.y + offset.y + currentVerticalLook);

            Vector3 desired = basePosition;
            Vector2 delta = tracked - new Vector2(basePosition.x, basePosition.y);

            if (Mathf.Abs(delta.x) > deadZone.x)
            {
                desired.x = tracked.x - Mathf.Sign(delta.x) * deadZone.x;
            }

            if (!lockY && Mathf.Abs(delta.y) > deadZone.y)
            {
                desired.y = tracked.y - Mathf.Sign(delta.y) * deadZone.y;
            }

            desired.z = transform.position.z;

            float t = 1f - Mathf.Exp(-followSharpness * deltaTime);
            Vector3 smoothed = Vector3.Lerp(basePosition, desired, t);
            UpdateShake(deltaTime);
            transform.position = smoothed + new Vector3(currentShakeOffset.x, currentShakeOffset.y, 0f);

            if (followCamera != null)
            {
                followCamera.orthographic = true;
            }
        }

        void UpdateVerticalLook(float deltaTime)
        {
            float verticalInput = GetVerticalLookInput();
            bool canLook = Mathf.Abs(verticalInput) > 0.5f;

            if (canLook)
            {
                verticalLookTimer += deltaTime;
            }
            else
            {
                verticalLookTimer = 0f;
            }

            float targetLook = canLook && verticalLookTimer >= verticalLookDelay
                ? Mathf.Sign(verticalInput) * verticalLookOffset
                : 0f;
            float t = 1f - Mathf.Exp(-verticalLookSharpness * deltaTime);
            currentVerticalLook = Mathf.Lerp(currentVerticalLook, targetLook, t);
        }

        void UpdateShake(float deltaTime)
        {
            if (shakeTimer <= 0f)
            {
                currentShakeOffset = Vector2.zero;
                shakeIntensity = 0f;
                shakeDuration = 0f;
                return;
            }

            shakeTimer = Mathf.Max(0f, shakeTimer - deltaTime);
            float duration = Mathf.Max(0.01f, shakeDuration);
            float strength = shakeIntensity * Mathf.Clamp01(shakeTimer / duration);
            currentShakeOffset = Random.insideUnitCircle * strength;
        }

        static float GetVerticalLookInput()
        {
            float vertical = 0f;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    vertical += 1f;
                }

                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    vertical -= 1f;
                }
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (Mathf.Abs(stick.y) > 0.45f)
                {
                    vertical += stick.y;
                }

                if (gamepad.dpad.up.isPressed)
                {
                    vertical += 1f;
                }

                if (gamepad.dpad.down.isPressed)
                {
                    vertical -= 1f;
                }
            }

            return Mathf.Clamp(vertical, -1f, 1f);
        }
    }
}
