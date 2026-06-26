using Penumbra.Art;
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
        const float GroundProbeSkin = 0.04f;
        const int GeneratedMoveFrameCount = 8;
        const int GeneratedAttackFrameCount = 4;
        const int ConceptIdleFrameCount = 6;
        const int ConceptRunFrameCount = 8;
        const int ConceptAttackFrameCount = 4;
        const string ConceptAnimationResourcePath = "Characters/WandererConcept";
        const float ColliderWidth = 0.72f;
        const float ColliderHeight = 1.68f;
        const float VisualWidthScale = 0.72f;
        const float VisualHeightScale = 0.84f;

        static Sprite runtimeIdleCharacterSprite;
        static Sprite runtimeJumpCharacterSprite;
        static Sprite runtimeFallCharacterSprite;
        static Sprite[] runtimeMoveCharacterSprites;
        static Sprite[] runtimeAttackCharacterSprites;
        static Sprite[] conceptIdleSprites;
        static Sprite[] conceptRunSprites;
        static Sprite[] conceptJumpSprites;
        static Sprite[] conceptFallSprites;
        static Sprite[] conceptAttackSprites;

        readonly RaycastHit2D[] groundHits = new RaycastHit2D[GroundHitBufferSize];

        [Header("Movement")]
        [SerializeField] float moveSpeed = 7.5f;
        [SerializeField] float acceleration = 80f;
        [SerializeField] float groundDeceleration = 95f;
        [SerializeField] float airAcceleration = 55f;
        [SerializeField] float airDeceleration = 48f;
        [SerializeField] float jumpVelocity = 13.5f;
        [SerializeField] int extraAirJumps = 1;
        [SerializeField] float dashSpeed = 16f;
        [SerializeField] float dashDuration = 0.14f;
        [SerializeField] float dashCooldown = 0.45f;
        [SerializeField] LayerMask groundLayers = ~0;
        [SerializeField] float groundCheckDistance = 0.08f;
        [SerializeField] float groundCheckWidth = 0.56f;

        [Header("Jump Feel")]
        [SerializeField] float coyoteTime = 0.1f;
        [SerializeField] float jumpBufferTime = 0.12f;
        [SerializeField] float jumpCutMultiplier = 0.45f;
        [SerializeField] float fallGravityMultiplier = 1.7f;
        [SerializeField] float maxFallSpeed = -22f;

        [Header("Hit Motion")]
        [SerializeField] Vector2 testHitKnockback = new(8f, 5f);
        [SerializeField] float hitStunDuration = 0.24f;
        [SerializeField] float hitFlashDuration = 0.2f;
        [SerializeField] float attackPulseDuration = 0.24f;

        [Header("Visual")]
        [SerializeField] Sprite bodySprite;
        [SerializeField] bool useGeneratedWandererAnimation = false;
        [SerializeField] bool useConceptSpriteAnimation = true;
        [SerializeField] float animationMovementThreshold = 0.08f;
        [SerializeField] float idleFrameRate = 5f;
        [SerializeField] float walkFrameRate = 8f;
        [SerializeField] float runFrameRate = 14f;
        [SerializeField] float attackFrameRate = 18f;
        [SerializeField] string sortingLayerName = "Gameplay";
        [SerializeField] Color idleColor = Color.white;
        [SerializeField] Color dashColor = new(0.74f, 1f, 0.9f, 1f);
        [SerializeField] Color attackColor = Color.white;
        [SerializeField] Color hitColor = new(1f, 0.33f, 0.28f, 1f);
        [SerializeField] Animator animator;

        Rigidbody2D body;
        CapsuleCollider2D capsule;
        SpriteRenderer visualRenderer;
        Transform visualTransform;
        Animator cachedAnimatorParameterSource;

        Vector2 moveInput;
        bool dashQueued;
        bool attackQueued;
        bool jumpReleased;
        int jumpsRemaining;
        int facingSign = 1;
        float dashTimer;
        float dashCooldownTimer;
        float hitStunTimer;
        float hitFlashTimer;
        float attackPulseTimer;
        float coyoteTimer;
        float jumpBufferTimer;
        float walkCycleTimer;
        float conceptCycleTimer;
        int currentGeneratedFrame = -1;
        PrototypeWandererSpriteFactory.WandererPose currentGeneratedPose = PrototypeWandererSpriteFactory.WandererPose.Idle;
        int currentConceptFrame = -1;
        PrototypeWandererSpriteFactory.WandererPose currentConceptPose = PrototypeWandererSpriteFactory.WandererPose.Idle;
        bool isGrounded;
        bool hasAnimSpeed;
        bool hasAnimYVelocity;
        bool hasAnimGrounded;
        bool hasAnimWalking;
        bool hasAnimJumping;
        bool hasAnimDashing;
        bool hasAnimAttacking;
        bool hasAnimHurt;

        static readonly int AnimSpeed = Animator.StringToHash("Speed");
        static readonly int AnimYVelocity = Animator.StringToHash("YVelocity");
        static readonly int AnimGrounded = Animator.StringToHash("Grounded");
        static readonly int AnimWalking = Animator.StringToHash("Walking");
        static readonly int AnimJumping = Animator.StringToHash("Jumping");
        static readonly int AnimDashing = Animator.StringToHash("Dashing");
        static readonly int AnimAttacking = Animator.StringToHash("Attacking");
        static readonly int AnimHurt = Animator.StringToHash("Hurt");

        public int FacingSign => facingSign;
        public bool IsGrounded => isGrounded;

        public void ApplyPogoBounce(float verticalVelocity)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            CacheComponents();

            if (body == null)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;
            velocity.y = Mathf.Max(velocity.y, verticalVelocity);
            body.linearVelocity = velocity;
            isGrounded = false;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            jumpReleased = false;
        }

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
            groundDeceleration = Mathf.Max(0f, groundDeceleration);
            airAcceleration = Mathf.Max(0f, airAcceleration);
            airDeceleration = Mathf.Max(0f, airDeceleration);
            jumpVelocity = Mathf.Max(0f, jumpVelocity);
            extraAirJumps = Mathf.Max(0, extraAirJumps);
            dashSpeed = Mathf.Max(0f, dashSpeed);
            dashDuration = Mathf.Max(0f, dashDuration);
            dashCooldown = Mathf.Max(0f, dashCooldown);
            groundCheckDistance = Mathf.Max(0.01f, groundCheckDistance);
            groundCheckWidth = Mathf.Max(0.01f, groundCheckWidth);
            coyoteTime = Mathf.Max(0f, coyoteTime);
            jumpBufferTime = Mathf.Max(0f, jumpBufferTime);
            jumpCutMultiplier = Mathf.Clamp01(jumpCutMultiplier);
            fallGravityMultiplier = Mathf.Max(1f, fallGravityMultiplier);
            maxFallSpeed = Mathf.Min(0f, maxFallSpeed);
            hitStunDuration = Mathf.Max(0f, hitStunDuration);
            hitFlashDuration = Mathf.Max(0f, hitFlashDuration);
            attackPulseDuration = Mathf.Max(0f, attackPulseDuration);
            animationMovementThreshold = Mathf.Max(0f, animationMovementThreshold);
            idleFrameRate = Mathf.Max(0f, idleFrameRate);
            walkFrameRate = Mathf.Max(0f, walkFrameRate);
            runFrameRate = Mathf.Max(walkFrameRate, runFrameRate);
            attackFrameRate = Mathf.Max(0f, attackFrameRate);

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
                coyoteTimer = coyoteTime;
                jumpsRemaining = extraAirJumps;
            }
            else
            {
                coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.fixedDeltaTime);
            }

            if (hitStunTimer > 0f)
            {
                jumpBufferTimer = 0f;
                dashQueued = false;
                return;
            }

            if (dashTimer > 0f)
            {
                body.linearVelocity = new Vector2(facingSign * dashSpeed, 0f);
                jumpBufferTimer = 0f;
                return;
            }

            if (dashQueued && dashCooldownTimer <= 0f)
            {
                StartDash();
            }

            if (jumpBufferTimer > 0f)
            {
                TryJump();
            }

            ApplyHorizontalMovement();
            ApplyBetterGravity();

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

                if (keyboard.spaceKey.wasPressedThisFrame)
                {
                    jumpBufferTimer = jumpBufferTime;
                }

                jumpReleased |= keyboard.spaceKey.wasReleasedThisFrame;
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
                if (gamepad.buttonSouth.wasPressedThisFrame)
                {
                    jumpBufferTimer = jumpBufferTime;
                }

                jumpReleased |= gamepad.buttonSouth.wasReleasedThisFrame;
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
            jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - deltaTime);
        }

        void ApplyHorizontalMovement()
        {
            float targetSpeed = moveInput.x * moveSpeed;
            bool hasInput = Mathf.Abs(moveInput.x) > 0.01f;
            float rate = isGrounded
                ? hasInput ? acceleration : groundDeceleration
                : hasInput ? airAcceleration : airDeceleration;
            Vector2 velocity = body.linearVelocity;
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, rate * Time.fixedDeltaTime);
            body.linearVelocity = velocity;
        }

        void TryJump()
        {
            bool canGroundJump = isGrounded || coyoteTimer > 0f;
            if (!canGroundJump && jumpsRemaining <= 0)
            {
                return;
            }

            if (!canGroundJump)
            {
                jumpsRemaining--;
            }

            Vector2 velocity = body.linearVelocity;
            velocity.y = jumpVelocity;
            body.linearVelocity = velocity;
            isGrounded = false;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            jumpReleased = false;
        }

        void ApplyBetterGravity()
        {
            Vector2 velocity = body.linearVelocity;

            if (jumpReleased && velocity.y > 0f)
            {
                velocity.y *= jumpCutMultiplier;
            }

            jumpReleased = false;

            if (velocity.y < 0f)
            {
                velocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
            }

            velocity.y = Mathf.Max(velocity.y, maxFallSpeed);
            body.linearVelocity = velocity;
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

            isGrounded = capsule.Cast(Vector2.down, filter, groundHits, groundCheckDistance) > 0 || GroundRaycastProbesHit(filter);
        }

        bool GroundRaycastProbesHit(ContactFilter2D filter)
        {
            if (capsule == null)
            {
                return false;
            }

            Bounds bounds = capsule.bounds;
            float halfWidth = Mathf.Min(bounds.extents.x, groundCheckWidth * 0.5f);
            float originY = bounds.min.y + GroundProbeSkin;
            float distance = groundCheckDistance + GroundProbeSkin;

            return GroundRaycastHits(new Vector2(bounds.center.x, originY), distance, filter)
                || GroundRaycastHits(new Vector2(bounds.center.x - halfWidth, originY), distance, filter)
                || GroundRaycastHits(new Vector2(bounds.center.x + halfWidth, originY), distance, filter);
        }

        bool GroundRaycastHits(Vector2 origin, float distance, ContactFilter2D filter)
        {
            int hitCount = Physics2D.Raycast(origin, Vector2.down, filter, groundHits, distance);
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hitCollider = groundHits[i].collider;
                if (hitCollider == null || hitCollider == capsule || hitCollider.transform.IsChildOf(transform))
                {
                    continue;
                }

                return true;
            }

            return false;
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

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        void ConfigurePhysics()
        {
            if (body == null || capsule == null)
            {
                return;
            }

            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 4.4f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            capsule.direction = CapsuleDirection2D.Vertical;
            capsule.size = new Vector2(ColliderWidth, ColliderHeight);
            capsule.offset = Vector2.zero;
        }

        void ConfigureVisual()
        {
            if (visualRenderer == null)
            {
                return;
            }

            if (UsesGeneratedPrototypeVisual())
            {
                visualRenderer.sprite = GetRuntimeCharacterSprite(PrototypeWandererSpriteFactory.WandererPose.Idle, 0);
                currentGeneratedFrame = 0;
                currentGeneratedPose = PrototypeWandererSpriteFactory.WandererPose.Idle;
            }
            else if (UsesConceptSpriteAnimation() && GetConceptSprite(PrototypeWandererSpriteFactory.WandererPose.Idle, 0) != null)
            {
                visualRenderer.sprite = GetConceptSprite(PrototypeWandererSpriteFactory.WandererPose.Idle, 0);
                currentConceptFrame = 0;
                currentConceptPose = PrototypeWandererSpriteFactory.WandererPose.Idle;
            }
            else if (bodySprite != null)
            {
                visualRenderer.sprite = bodySprite;
            }
            else if (visualRenderer.sprite == null)
            {
                visualRenderer.sprite = GetRuntimeCharacterSprite(PrototypeWandererSpriteFactory.WandererPose.Idle, 0);
            }

            visualRenderer.color = idleColor;
            visualRenderer.sortingLayerName = string.IsNullOrWhiteSpace(sortingLayerName) ? "Default" : sortingLayerName;
            visualRenderer.sortingOrder = 10;
            visualRenderer.flipX = facingSign < 0;

            visualTransform.localPosition = Vector3.zero;
            visualTransform.localRotation = Quaternion.identity;
            visualTransform.localScale = new Vector3(VisualWidthScale, VisualHeightScale, 1f);
        }

        void UpdateVisualMotion()
        {
            if (visualRenderer == null || visualTransform == null)
            {
                return;
            }

            Color color = idleColor;
            Vector3 scale = new(VisualWidthScale, VisualHeightScale, 1f);

            UpdateConceptSpriteAnimation();
            UpdateGeneratedPrototypeAnimation();

            if (dashTimer > 0f)
            {
                color = dashColor;
                scale = new Vector3(VisualWidthScale * 1.12f, VisualHeightScale * 0.86f, 1f);
            }

            if (attackPulseTimer > 0f)
            {
                color = attackColor;
                scale = new Vector3(VisualWidthScale * 1.08f, VisualHeightScale * 0.96f, 1f);
            }

            if (hitFlashTimer > 0f)
            {
                color = hitColor;
                scale = new Vector3(VisualWidthScale * 1.14f, VisualHeightScale * 0.88f, 1f);
            }

            visualRenderer.color = color;
            visualRenderer.flipX = facingSign < 0;
            visualTransform.localScale = scale;
            UpdateAnimatorParameters();
        }

        void UpdateAnimatorParameters()
        {
            CacheAnimatorParameters();

            if (animator == null)
            {
                return;
            }

            Vector2 velocity = body != null ? body.linearVelocity : Vector2.zero;
            float horizontalSpeed = Mathf.Abs(velocity.x);
            bool walking = isGrounded && horizontalSpeed > animationMovementThreshold && hitStunTimer <= 0f;
            bool jumping = !isGrounded;
            bool dashing = dashTimer > 0f;
            bool attacking = attackPulseTimer > 0f;
            bool hurt = hitFlashTimer > 0f || hitStunTimer > 0f;

            if (hasAnimSpeed)
            {
                animator.SetFloat(AnimSpeed, horizontalSpeed);
            }

            if (hasAnimYVelocity)
            {
                animator.SetFloat(AnimYVelocity, velocity.y);
            }

            if (hasAnimGrounded)
            {
                animator.SetBool(AnimGrounded, isGrounded);
            }

            if (hasAnimWalking)
            {
                animator.SetBool(AnimWalking, walking);
            }

            if (hasAnimJumping)
            {
                animator.SetBool(AnimJumping, jumping);
            }

            if (hasAnimDashing)
            {
                animator.SetBool(AnimDashing, dashing);
            }

            if (hasAnimAttacking)
            {
                animator.SetBool(AnimAttacking, attacking);
            }

            if (hasAnimHurt)
            {
                animator.SetBool(AnimHurt, hurt);
            }
        }

        void CacheAnimatorParameters()
        {
            if (animator == cachedAnimatorParameterSource)
            {
                return;
            }

            cachedAnimatorParameterSource = animator;
            hasAnimSpeed = false;
            hasAnimYVelocity = false;
            hasAnimGrounded = false;
            hasAnimWalking = false;
            hasAnimJumping = false;
            hasAnimDashing = false;
            hasAnimAttacking = false;
            hasAnimHurt = false;

            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == AnimSpeed)
                {
                    hasAnimSpeed = parameter.type == AnimatorControllerParameterType.Float;
                }
                else if (parameter.nameHash == AnimYVelocity)
                {
                    hasAnimYVelocity = parameter.type == AnimatorControllerParameterType.Float;
                }
                else if (parameter.nameHash == AnimGrounded)
                {
                    hasAnimGrounded = parameter.type == AnimatorControllerParameterType.Bool;
                }
                else if (parameter.nameHash == AnimWalking)
                {
                    hasAnimWalking = parameter.type == AnimatorControllerParameterType.Bool;
                }
                else if (parameter.nameHash == AnimJumping)
                {
                    hasAnimJumping = parameter.type == AnimatorControllerParameterType.Bool;
                }
                else if (parameter.nameHash == AnimDashing)
                {
                    hasAnimDashing = parameter.type == AnimatorControllerParameterType.Bool;
                }
                else if (parameter.nameHash == AnimAttacking)
                {
                    hasAnimAttacking = parameter.type == AnimatorControllerParameterType.Bool;
                }
                else if (parameter.nameHash == AnimHurt)
                {
                    hasAnimHurt = parameter.type == AnimatorControllerParameterType.Bool;
                }
            }
        }

        void UpdateGeneratedPrototypeAnimation()
        {
            if (!UsesGeneratedPrototypeVisual())
            {
                return;
            }

            float horizontalSpeed = body != null ? Mathf.Abs(body.linearVelocity.x) : Mathf.Abs(moveInput.x) * moveSpeed;
            PrototypeWandererSpriteFactory.WandererPose pose = PrototypeWandererSpriteFactory.WandererPose.Idle;
            int frame = 0;

            if (attackPulseTimer > 0f)
            {
                pose = PrototypeWandererSpriteFactory.WandererPose.Attack;
                float attackProgress = 1f - attackPulseTimer / Mathf.Max(0.01f, attackPulseDuration);
                frame = Mathf.Clamp(Mathf.FloorToInt(attackProgress * GeneratedAttackFrameCount), 0, GeneratedAttackFrameCount - 1);
            }
            else if (!isGrounded)
            {
                float verticalSpeed = body != null ? body.linearVelocity.y : 0f;
                pose = verticalSpeed > 0.1f ? PrototypeWandererSpriteFactory.WandererPose.Jump : PrototypeWandererSpriteFactory.WandererPose.Fall;
                walkCycleTimer = 0f;
            }
            else if (hitStunTimer <= 0f && horizontalSpeed > animationMovementThreshold)
            {
                pose = PrototypeWandererSpriteFactory.WandererPose.Move;
                float speedBlend = moveSpeed > 0f ? Mathf.Clamp01(horizontalSpeed / moveSpeed) : 0f;
                float frameRate = Mathf.Lerp(walkFrameRate, runFrameRate, speedBlend);
                walkCycleTimer = (walkCycleTimer + Time.deltaTime * frameRate) % GeneratedMoveFrameCount;
                frame = Mathf.FloorToInt(walkCycleTimer) % GeneratedMoveFrameCount;
            }
            else
            {
                walkCycleTimer = 0f;
            }

            if (visualRenderer != null && (frame != currentGeneratedFrame || pose != currentGeneratedPose))
            {
                visualRenderer.sprite = GetRuntimeCharacterSprite(pose, frame);
                currentGeneratedFrame = frame;
                currentGeneratedPose = pose;
            }
        }

        void UpdateConceptSpriteAnimation()
        {
            if (!UsesConceptSpriteAnimation())
            {
                return;
            }

            float horizontalSpeed = body != null ? Mathf.Abs(body.linearVelocity.x) : Mathf.Abs(moveInput.x) * moveSpeed;
            PrototypeWandererSpriteFactory.WandererPose pose = PrototypeWandererSpriteFactory.WandererPose.Idle;
            int frameCount = ConceptIdleFrameCount;
            float frameRate = idleFrameRate;

            if (attackPulseTimer > 0f)
            {
                pose = PrototypeWandererSpriteFactory.WandererPose.Attack;
                frameCount = ConceptAttackFrameCount;
                frameRate = attackFrameRate;
            }
            else if (!isGrounded)
            {
                float verticalSpeed = body != null ? body.linearVelocity.y : 0f;
                pose = verticalSpeed > 0.1f ? PrototypeWandererSpriteFactory.WandererPose.Jump : PrototypeWandererSpriteFactory.WandererPose.Fall;
                frameCount = 1;
                frameRate = 0f;
            }
            else if (hitStunTimer <= 0f && horizontalSpeed > animationMovementThreshold)
            {
                pose = PrototypeWandererSpriteFactory.WandererPose.Move;
                frameCount = ConceptRunFrameCount;
                float speedBlend = moveSpeed > 0f ? Mathf.Clamp01(horizontalSpeed / moveSpeed) : 0f;
                frameRate = Mathf.Lerp(walkFrameRate, runFrameRate, speedBlend);
            }

            if (pose != currentConceptPose)
            {
                conceptCycleTimer = 0f;
                currentConceptFrame = -1;
            }

            int frame = 0;
            if (pose == PrototypeWandererSpriteFactory.WandererPose.Attack)
            {
                float attackProgress = 1f - attackPulseTimer / Mathf.Max(0.01f, attackPulseDuration);
                frame = Mathf.Clamp(Mathf.FloorToInt(attackProgress * frameCount), 0, frameCount - 1);
            }
            else if (frameCount > 1 && frameRate > 0f)
            {
                conceptCycleTimer = (conceptCycleTimer + Time.deltaTime * frameRate) % frameCount;
                frame = Mathf.FloorToInt(conceptCycleTimer) % frameCount;
            }

            if (visualRenderer == null || (frame == currentConceptFrame && pose == currentConceptPose))
            {
                return;
            }

            Sprite sprite = GetConceptSprite(pose, frame);
            if (sprite == null)
            {
                sprite = bodySprite;
            }

            if (sprite != null)
            {
                visualRenderer.sprite = sprite;
                currentConceptFrame = frame;
                currentConceptPose = pose;
            }
        }

        bool UsesGeneratedPrototypeVisual()
        {
            return useGeneratedWandererAnimation || bodySprite == null;
        }

        bool UsesConceptSpriteAnimation()
        {
            return useConceptSpriteAnimation && !UsesGeneratedPrototypeVisual();
        }

        static Sprite GetConceptSprite(PrototypeWandererSpriteFactory.WandererPose pose, int frame)
        {
            Sprite[] sprites = GetConceptSprites(pose);
            if (sprites == null || sprites.Length == 0)
            {
                return null;
            }

            int clampedFrame = Mathf.Abs(frame) % sprites.Length;
            return sprites[clampedFrame];
        }

        static Sprite[] GetConceptSprites(PrototypeWandererSpriteFactory.WandererPose pose)
        {
            switch (pose)
            {
                case PrototypeWandererSpriteFactory.WandererPose.Move:
                    conceptRunSprites ??= LoadConceptSpriteSequence("Run", ConceptRunFrameCount);
                    return conceptRunSprites;
                case PrototypeWandererSpriteFactory.WandererPose.Jump:
                    conceptJumpSprites ??= LoadConceptSpriteSequence("Jump", 1);
                    return conceptJumpSprites;
                case PrototypeWandererSpriteFactory.WandererPose.Fall:
                    conceptFallSprites ??= LoadConceptSpriteSequence("Fall", 1);
                    return conceptFallSprites;
                case PrototypeWandererSpriteFactory.WandererPose.Attack:
                    conceptAttackSprites ??= LoadConceptSpriteSequence("Attack", ConceptAttackFrameCount);
                    return conceptAttackSprites;
                default:
                    conceptIdleSprites ??= LoadConceptSpriteSequence("Idle", ConceptIdleFrameCount);
                    return conceptIdleSprites;
            }
        }

        static Sprite[] LoadConceptSpriteSequence(string prefix, int count)
        {
            Sprite[] sprites = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                sprites[i] = Resources.Load<Sprite>($"{ConceptAnimationResourcePath}/{prefix}_{i}");
            }

            return sprites;
        }

        static Sprite GetRuntimeCharacterSprite(PrototypeWandererSpriteFactory.WandererPose pose, int frame)
        {
            if (pose == PrototypeWandererSpriteFactory.WandererPose.Move)
            {
                if (runtimeMoveCharacterSprites == null || runtimeMoveCharacterSprites.Length != GeneratedMoveFrameCount)
                {
                    runtimeMoveCharacterSprites = new Sprite[GeneratedMoveFrameCount];
                }

                int clampedFrame = Mathf.Abs(frame) % GeneratedMoveFrameCount;
                if (runtimeMoveCharacterSprites[clampedFrame] == null)
                {
                    float stride = clampedFrame / (float)GeneratedMoveFrameCount;
                    runtimeMoveCharacterSprites[clampedFrame] = CreateRuntimeSprite($"Runtime Penumbra Wanderer Move {clampedFrame}", pose, stride, 1f);
                }

                return runtimeMoveCharacterSprites[clampedFrame];
            }

            if (pose == PrototypeWandererSpriteFactory.WandererPose.Jump)
            {
                if (runtimeJumpCharacterSprite == null)
                {
                    runtimeJumpCharacterSprite = CreateRuntimeSprite("Runtime Penumbra Wanderer Jump", pose, 0f, 1f);
                }

                return runtimeJumpCharacterSprite;
            }

            if (pose == PrototypeWandererSpriteFactory.WandererPose.Fall)
            {
                if (runtimeFallCharacterSprite == null)
                {
                    runtimeFallCharacterSprite = CreateRuntimeSprite("Runtime Penumbra Wanderer Fall", pose, 0f, 1f);
                }

                return runtimeFallCharacterSprite;
            }

            if (pose == PrototypeWandererSpriteFactory.WandererPose.Attack)
            {
                if (runtimeAttackCharacterSprites == null || runtimeAttackCharacterSprites.Length != GeneratedAttackFrameCount)
                {
                    runtimeAttackCharacterSprites = new Sprite[GeneratedAttackFrameCount];
                }

                int clampedFrame = Mathf.Abs(frame) % GeneratedAttackFrameCount;
                if (runtimeAttackCharacterSprites[clampedFrame] == null)
                {
                    float[] attackMotion = { 0.2f, 0.65f, 1f, 0.45f };
                    float motion = attackMotion[Mathf.Clamp(clampedFrame, 0, attackMotion.Length - 1)];
                    runtimeAttackCharacterSprites[clampedFrame] = CreateRuntimeSprite($"Runtime Penumbra Wanderer Attack {clampedFrame}", pose, motion, motion);
                }

                return runtimeAttackCharacterSprites[clampedFrame];
            }

            if (runtimeIdleCharacterSprite == null)
            {
                runtimeIdleCharacterSprite = CreateRuntimeSprite("Runtime Penumbra Wanderer Idle", PrototypeWandererSpriteFactory.WandererPose.Idle, 0f, 0f);
            }

            return runtimeIdleCharacterSprite;
        }

        static Sprite CreateRuntimeSprite(string name, PrototypeWandererSpriteFactory.WandererPose pose, float stride, float motion)
        {
            Texture2D texture = PrototypeWandererSpriteFactory.CreateTexture(name, pose, stride, motion);
            texture.hideFlags = HideFlags.HideAndDontSave;
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, PrototypeWandererSpriteFactory.Width, PrototypeWandererSpriteFactory.Height),
                new Vector2(0.5f, 0.5f),
                PrototypeWandererSpriteFactory.PixelsPerUnit);
            sprite.name = name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(ColliderWidth, ColliderHeight, 0.05f));

            Gizmos.color = Color.yellow;
            float halfWidth = Mathf.Min(ColliderWidth * 0.5f, groundCheckWidth * 0.5f);
            Vector3 groundProbeCenter = transform.position + Vector3.down * (ColliderHeight * 0.5f - GroundProbeSkin);
            Vector3 groundProbeEndOffset = Vector3.down * (groundCheckDistance + GroundProbeSkin);
            Gizmos.DrawLine(groundProbeCenter, groundProbeCenter + groundProbeEndOffset);
            Gizmos.DrawLine(groundProbeCenter + Vector3.left * halfWidth, groundProbeCenter + Vector3.left * halfWidth + groundProbeEndOffset);
            Gizmos.DrawLine(groundProbeCenter + Vector3.right * halfWidth, groundProbeCenter + Vector3.right * halfWidth + groundProbeEndOffset);
        }
    }
}
