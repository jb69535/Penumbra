using UnityEngine;
using UnityEngine.InputSystem;

namespace Penumbra.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        const string VisualChildName = "Player Visual";
        const int GroundHitBufferSize = 8;
        const float TargetVisualHeight = 1.8f;

        readonly RaycastHit2D[] groundHits = new RaycastHit2D[GroundHitBufferSize];

        [Header("Movement")]
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float acceleration = 80f;
        [SerializeField] float airAcceleration = 42f;
        [SerializeField] float jumpVelocity = 10.5f;
        [SerializeField] int extraAirJumps = 1;
        [SerializeField] LayerMask groundLayers = ~0;
        [SerializeField] float groundCheckDistance = 0.08f;

        [Header("Sprites")]
        [SerializeField] Sprite idleSprite;
        [SerializeField] Sprite[] runSprites;
        [SerializeField] Sprite[] jumpSprites;
        [SerializeField] Sprite dashSprite;
        [SerializeField] float runFramesPerSecond = 12f;
        [SerializeField] float jumpFramesPerSecond = 10f;
        [SerializeField] string sortingLayerName = "Gameplay";

        Rigidbody2D body;
        CapsuleCollider2D capsule;
        SpriteRenderer spriteRenderer;
        Transform visualTransform;

        Vector2 moveInput;
        bool jumpQueued;
        int facingSign = 1;
        int jumpsRemaining;
        bool isGrounded;
        float animationTimer;
        int animationFrame;

        public bool IsGrounded => isGrounded;
        public int FacingSign => facingSign;

        public void ConfigureSprites(Sprite idle, Sprite[] runFrames, Sprite[] jumpFrames, Sprite dash)
        {
            idleSprite = idle;
            runSprites = runFrames;
            jumpSprites = jumpFrames;
            dashSprite = dash;
            CacheComponents();
            ApplySprite();
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
            groundCheckDistance = Mathf.Max(0.01f, groundCheckDistance);
            runFramesPerSecond = Mathf.Max(1f, runFramesPerSecond);
            jumpFramesPerSecond = Mathf.Max(1f, jumpFramesPerSecond);

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
            UpdateSprite(Time.deltaTime);
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

            if (jumpQueued)
            {
                TryJump();
                jumpQueued = false;
            }

            ApplyHorizontalMovement();
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
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                horizontal += gamepad.leftStick.ReadValue().x;
                jumpQueued |= gamepad.buttonSouth.wasPressedThisFrame;
            }

            horizontal = Mathf.Clamp(horizontal, -1f, 1f);
            moveInput = new Vector2(horizontal, 0f);

            if (Mathf.Abs(horizontal) > 0.01f)
            {
                facingSign = horizontal > 0f ? 1 : -1;
            }
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
            animationTimer = 0f;
            animationFrame = 0;
        }

        void UpdateGrounded()
        {
            ContactFilter2D filter = new();
            filter.SetLayerMask(groundLayers);
            filter.useTriggers = false;

            isGrounded = capsule.Cast(Vector2.down, filter, groundHits, groundCheckDistance) > 0;
        }

        void UpdateSprite(float deltaTime)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (!isGrounded && jumpSprites != null && jumpSprites.Length > 0)
            {
                PlaySpriteLoop(jumpSprites, jumpFramesPerSecond, deltaTime);
            }
            else if (isGrounded && Mathf.Abs(moveInput.x) > 0.01f && runSprites != null && runSprites.Length > 0)
            {
                PlaySpriteLoop(runSprites, runFramesPerSecond, deltaTime);
            }
            else
            {
                animationTimer = 0f;
                animationFrame = 0;

                if (idleSprite != null)
                {
                    SetDisplayedSprite(idleSprite);
                }
            }

            spriteRenderer.flipX = facingSign < 0;
        }

        void PlaySpriteLoop(Sprite[] frames, float framesPerSecond, float deltaTime)
        {
            animationTimer += deltaTime;
            float frameDuration = 1f / framesPerSecond;

            while (animationTimer >= frameDuration)
            {
                animationTimer -= frameDuration;
                animationFrame = (animationFrame + 1) % frames.Length;
            }

            SetDisplayedSprite(frames[animationFrame]);
        }

        void ApplySprite()
        {
            if (spriteRenderer == null || idleSprite == null)
            {
                return;
            }

            SetDisplayedSprite(idleSprite);
        }

        void SetDisplayedSprite(Sprite sprite)
        {
            if (spriteRenderer == null || sprite == null)
            {
                return;
            }

            spriteRenderer.sprite = sprite;
            SyncVisualScale(sprite);
        }

        void SyncVisualScale(Sprite sprite)
        {
            if (visualTransform == null || sprite == null)
            {
                return;
            }

            float spriteHeight = sprite.bounds.size.y;
            if (spriteHeight <= 0.0001f)
            {
                visualTransform.localScale = Vector3.one;
                return;
            }

            float referenceHeight = idleSprite != null && idleSprite.bounds.size.y > 0.0001f
                ? idleSprite.bounds.size.y
                : TargetVisualHeight;

            float uniformScale = referenceHeight / spriteHeight;
            visualTransform.localScale = new Vector3(uniformScale, uniformScale, 1f);
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
                visualTransform = transform.Find(VisualChildName);
            }

            if (visualTransform == null && createMissing)
            {
                GameObject visual = new(VisualChildName);
                visual.transform.SetParent(transform, false);
                visualTransform = visual.transform;
            }

            if (visualTransform == null)
            {
                return;
            }

            if (spriteRenderer == null && !visualTransform.TryGetComponent(out spriteRenderer) && createMissing)
            {
                spriteRenderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
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
            if (spriteRenderer == null)
            {
                return;
            }

            ApplySprite();
            spriteRenderer.sortingLayerName = string.IsNullOrWhiteSpace(sortingLayerName) ? "Default" : sortingLayerName;
            spriteRenderer.sortingOrder = 10;
            visualTransform.localPosition = Vector3.zero;
            visualTransform.localRotation = Quaternion.identity;
            visualTransform.localScale = Vector3.one;
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
