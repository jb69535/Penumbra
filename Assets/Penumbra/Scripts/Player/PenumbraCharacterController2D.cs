using UnityEngine;
using UnityEngine.InputSystem;

namespace Penumbra.Player
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public sealed class PenumbraCharacterController2D : MonoBehaviour
    {
        const string VisualName = "Wanderer Visual";
        const int GroundHitBufferSize = 8;

        static Sprite runtimeCharacterSprite;

        readonly RaycastHit2D[] groundHits = new RaycastHit2D[GroundHitBufferSize];

        [Header("Movement")]
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float acceleration = 80f;
        [SerializeField] float airAcceleration = 42f;
        [SerializeField] float jumpVelocity = 10.5f;
        [SerializeField] int extraAirJumps = 1;
        [SerializeField] float dashSpeed = 16f;
        [SerializeField] float dashDuration = 0.14f;
        [SerializeField] float dashCooldown = 0.45f;
        [SerializeField] LayerMask groundLayers = ~0;
        [SerializeField] float groundCheckDistance = 0.08f;

        [Header("Hit Motion")]
        [SerializeField] Vector2 testHitKnockback = new(8f, 5f);
        [SerializeField] float hitStunDuration = 0.24f;
        [SerializeField] float hitFlashDuration = 0.2f;
        [SerializeField] float attackPulseDuration = 0.12f;

        [Header("Visual")]
        [SerializeField] Sprite bodySprite;
        [SerializeField] string sortingLayerName = "Gameplay";
        [SerializeField] Color idleColor = new(0.48f, 0.82f, 1f, 1f);
        [SerializeField] Color dashColor = new(0.72f, 1f, 0.88f, 1f);
        [SerializeField] Color attackColor = new(1f, 0.92f, 0.42f, 1f);
        [SerializeField] Color hitColor = new(1f, 0.33f, 0.28f, 1f);

        Rigidbody2D body;
        CapsuleCollider2D capsule;
        SpriteRenderer visualRenderer;
        Transform visualTransform;

        Vector2 moveInput;
        bool jumpQueued;
        bool dashQueued;
        bool attackQueued;
        int jumpsRemaining;
        int facingSign = 1;
        float dashTimer;
        float dashCooldownTimer;
        float hitStunTimer;
        float hitFlashTimer;
        float attackPulseTimer;
        bool isGrounded;

        public int FacingSign => facingSign;
        public bool IsGrounded => isGrounded;

        public void SetBodySprite(Sprite sprite)
        {
            bodySprite = sprite;
            CacheComponents();
            ConfigureVisual();
        }

        public void ApplyHitFrom(Vector2 sourcePosition)
        {
            ApplyHitFrom(sourcePosition, testHitKnockback);
        }

        public void ApplyHitFrom(Vector2 sourcePosition, Vector2 knockback)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            CacheComponents();

            float direction = Mathf.Sign(((Vector2)transform.position - sourcePosition).x);
            if (Mathf.Approximately(direction, 0f))
            {
                direction = -facingSign;
            }

            dashTimer = 0f;
            hitStunTimer = hitStunDuration;
            hitFlashTimer = hitFlashDuration;
            body.linearVelocity = new Vector2(direction * knockback.x, knockback.y);
        }

        void Reset()
        {
            CacheComponents();
            ConfigurePhysics();
            ConfigureVisual();
        }

        void Awake()
        {
            CacheComponents();
            ConfigurePhysics();
            ConfigureVisual();
        }

        void OnEnable()
        {
            CacheComponents();
            ConfigurePhysics();
            ConfigureVisual();
        }

        void OnValidate()
        {
            moveSpeed = Mathf.Max(0f, moveSpeed);
            acceleration = Mathf.Max(0f, acceleration);
            airAcceleration = Mathf.Max(0f, airAcceleration);
            jumpVelocity = Mathf.Max(0f, jumpVelocity);
            extraAirJumps = Mathf.Max(0, extraAirJumps);
            dashSpeed = Mathf.Max(0f, dashSpeed);
            dashDuration = Mathf.Max(0f, dashDuration);
            dashCooldown = Mathf.Max(0f, dashCooldown);
            groundCheckDistance = Mathf.Max(0.01f, groundCheckDistance);
            hitStunDuration = Mathf.Max(0f, hitStunDuration);
            hitFlashDuration = Mathf.Max(0f, hitFlashDuration);
            attackPulseDuration = Mathf.Max(0f, attackPulseDuration);

            if (!gameObject.scene.IsValid())
            {
                return;
            }

            CacheComponents(false);
            ConfigurePhysics();
            ConfigureVisual();
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                ConfigureVisual();
                return;
            }

            ReadInput();
            TickTimers(Time.deltaTime);
            UpdateVisualMotion();
        }

        void FixedUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            UpdateGrounded();

            if (isGrounded && body.linearVelocity.y <= 0.05f)
            {
                jumpsRemaining = extraAirJumps;
            }

            if (hitStunTimer > 0f)
            {
                jumpQueued = false;
                dashQueued = false;
                return;
            }

            if (dashTimer > 0f)
            {
                body.linearVelocity = new Vector2(facingSign * dashSpeed, 0f);
                jumpQueued = false;
                return;
            }

            if (dashQueued && dashCooldownTimer <= 0f)
            {
                StartDash();
            }

            if (jumpQueued)
            {
                TryJump();
            }

            ApplyHorizontalMovement();

            jumpQueued = false;
            dashQueued = false;
        }

        void ReadInput()
        {
            float horizontal = 0f;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    horizontal -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    horizontal += 1f;
                }

                jumpQueued |= keyboard.spaceKey.wasPressedThisFrame;
                dashQueued |= keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame;
                attackQueued |= keyboard.jKey.wasPressedThisFrame;

                if (keyboard.hKey.wasPressedThisFrame)
                {
                    ApplyHitFrom((Vector2)transform.position + new Vector2(facingSign, 0f));
                }
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                horizontal += stick.x;
                jumpQueued |= gamepad.buttonSouth.wasPressedThisFrame;
                dashQueued |= gamepad.leftShoulder.wasPressedThisFrame || gamepad.rightShoulder.wasPressedThisFrame;
                attackQueued |= gamepad.buttonWest.wasPressedThisFrame;

                if (gamepad.buttonNorth.wasPressedThisFrame)
                {
                    ApplyHitFrom((Vector2)transform.position + new Vector2(facingSign, 0f));
                }
            }

            horizontal = Mathf.Clamp(horizontal, -1f, 1f);
            moveInput = new Vector2(horizontal, 0f);

            if (Mathf.Abs(horizontal) > 0.01f)
            {
                facingSign = horizontal > 0f ? 1 : -1;
            }

            if (attackQueued)
            {
                attackPulseTimer = attackPulseDuration;
                attackQueued = false;
            }
        }

        void TickTimers(float deltaTime)
        {
            dashTimer = Mathf.Max(0f, dashTimer - deltaTime);
            dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - deltaTime);
            hitStunTimer = Mathf.Max(0f, hitStunTimer - deltaTime);
            hitFlashTimer = Mathf.Max(0f, hitFlashTimer - deltaTime);
            attackPulseTimer = Mathf.Max(0f, attackPulseTimer - deltaTime);
        }

        void ApplyHorizontalMovement()
        {
            float targetSpeed = moveInput.x * moveSpeed;
            float rate = isGrounded ? acceleration : airAcceleration;
            Vector2 velocity = body.linearVelocity;
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, rate * Time.fixedDeltaTime);
            body.linearVelocity = velocity;
        }

        void TryJump()
        {
            if (!isGrounded && jumpsRemaining <= 0)
            {
                return;
            }

            if (!isGrounded)
            {
                jumpsRemaining--;
            }

            Vector2 velocity = body.linearVelocity;
            velocity.y = jumpVelocity;
            body.linearVelocity = velocity;
            isGrounded = false;
        }

        void StartDash()
        {
            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                facingSign = moveInput.x > 0f ? 1 : -1;
            }

            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            body.linearVelocity = new Vector2(facingSign * dashSpeed, 0f);
        }

        void UpdateGrounded()
        {
            ContactFilter2D filter = new();
            filter.SetLayerMask(groundLayers);
            filter.useTriggers = false;

            isGrounded = capsule.Cast(Vector2.down, filter, groundHits, groundCheckDistance) > 0;
        }

        void CacheComponents(bool createMissing = true)
        {
            if (body == null && !TryGetComponent(out body) && createMissing)
            {
                body = gameObject.AddComponent<Rigidbody2D>();
            }

            if (capsule == null && !TryGetComponent(out capsule) && createMissing)
            {
                capsule = gameObject.AddComponent<CapsuleCollider2D>();
            }

            if (visualTransform == null)
            {
                visualTransform = transform.Find(VisualName);
            }

            if (visualTransform == null && createMissing)
            {
                GameObject visual = new(VisualName);
                visual.transform.SetParent(transform, false);
                visualTransform = visual.transform;
            }

            if (visualTransform == null)
            {
                return;
            }

            if (visualRenderer == null && !visualTransform.TryGetComponent(out visualRenderer) && createMissing)
            {
                visualRenderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
            }
        }

        void ConfigurePhysics()
        {
            if (body == null || capsule == null)
            {
                return;
            }

            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 3.2f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            capsule.direction = CapsuleDirection2D.Vertical;
            capsule.size = new Vector2(0.85f, 1.8f);
            capsule.offset = Vector2.zero;
        }

        void ConfigureVisual()
        {
            if (visualRenderer == null)
            {
                return;
            }

            if (bodySprite != null)
            {
                visualRenderer.sprite = bodySprite;
            }
            else if (visualRenderer.sprite == null)
            {
                visualRenderer.sprite = GetRuntimeCharacterSprite();
            }

            visualRenderer.color = idleColor;
            visualRenderer.sortingLayerName = string.IsNullOrWhiteSpace(sortingLayerName) ? "Default" : sortingLayerName;
            visualRenderer.sortingOrder = 10;
            visualRenderer.flipX = facingSign < 0;

            visualTransform.localPosition = Vector3.zero;
            visualTransform.localRotation = Quaternion.identity;
            visualTransform.localScale = new Vector3(0.85f, 0.9f, 1f);
        }

        void UpdateVisualMotion()
        {
            if (visualRenderer == null || visualTransform == null)
            {
                return;
            }

            Color color = idleColor;
            Vector3 scale = new(0.85f, 0.9f, 1f);

            if (dashTimer > 0f)
            {
                color = dashColor;
                scale = new Vector3(1.08f, 0.76f, 1f);
            }

            if (attackPulseTimer > 0f)
            {
                color = attackColor;
                scale = new Vector3(1.04f, 0.86f, 1f);
            }

            if (hitFlashTimer > 0f)
            {
                color = hitColor;
                scale = new Vector3(1.12f, 0.78f, 1f);
            }

            visualRenderer.color = color;
            visualRenderer.flipX = facingSign < 0;
            visualTransform.localScale = scale;
        }

        static Sprite GetRuntimeCharacterSprite()
        {
            if (runtimeCharacterSprite != null)
            {
                return runtimeCharacterSprite;
            }

            const int width = 64;
            const int height = 128;
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
            {
                name = "Runtime Penumbra Wanderer",
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color clear = new(1f, 1f, 1f, 0f);
            Color fill = Color.white;
            Vector2 centerTop = new(width * 0.5f, height - width * 0.5f);
            Vector2 centerBottom = new(width * 0.5f, width * 0.5f);
            float radius = width * 0.42f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 point = new(x + 0.5f, y + 0.5f);
                    bool inMiddle = point.y >= centerBottom.y && point.y <= centerTop.y && Mathf.Abs(point.x - width * 0.5f) <= radius;
                    bool inCaps = Vector2.Distance(point, centerTop) <= radius || Vector2.Distance(point, centerBottom) <= radius;
                    texture.SetPixel(x, y, inMiddle || inCaps ? fill : clear);
                }
            }

            texture.Apply();
            runtimeCharacterSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 64f);
            runtimeCharacterSprite.name = "Runtime Penumbra Wanderer Sprite";
            runtimeCharacterSprite.hideFlags = HideFlags.HideAndDontSave;
            return runtimeCharacterSprite;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(0.85f, 1.8f, 0.05f));

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.down * 0.9f, transform.position + Vector3.down * (0.9f + groundCheckDistance));
        }
    }
}
