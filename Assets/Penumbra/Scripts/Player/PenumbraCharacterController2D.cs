using Penumbra.Art;
using Penumbra.Combat;
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

        [Header("Cinder Wisp Sprites")]
        [SerializeField] bool useCinderWispSpriteAnimation;
        [SerializeField] Sprite[] cinderIdleSprites;
        [SerializeField] Sprite[] cinderRunSprites;
        [SerializeField] Sprite[] cinderJumpSprites;
        [SerializeField] Sprite[] cinderSitSprites;
        [SerializeField] Sprite cinderSitIdleSprite;
        [SerializeField] Sprite[] cinderDashSprites;
        [SerializeField] Sprite[] cinderSlideSprites;
        [SerializeField] Sprite[] cinderAttackSprites;
        [SerializeField] Sprite cinderFrontIdleSprite;
        [SerializeField] Sprite cinderSideLeftSprite;
        [SerializeField] Sprite cinderSideRightSprite;
        [SerializeField] float cinderIdleFrameRate = 6f;
        [SerializeField] float cinderRunFrameRate = 12f;
        [SerializeField] float cinderDashFrameRate = 14f;
        [SerializeField] float cinderSlideFrameRate = 12f;
        [SerializeField] float cinderSitFrameRate = 8f;
        [SerializeField] float cinderAttackFrameRate = 12f;
        [SerializeField] float cinderSitMoveSpeed = 3.5f;
        [SerializeField] RopeWhipAttack2D ropeWhipAttack;
        [SerializeField] RopeController2D ropeController;
        [SerializeField] Transform cinderHandPoint;
        [Header("Hand Point Tuning")]
        [SerializeField] Vector2 cinderIdleHandLocal = new(-0.156f, -0.907f);
        [SerializeField] Vector2[] cinderAttackHandLocals;
        [SerializeField] bool cinderMirrorHandXOnFlip;
        [SerializeField] float cinderSlideSpeed = 10f;
        [SerializeField] float cinderSlideDuration = 0.52f;
        [SerializeField] float cinderSlideCooldown = 0.35f;
        [SerializeField] float cinderSlideCapsuleHeight = 0.74f;
        [SerializeField] float cinderSlideCapsuleWidth = 0.78f;
        [SerializeField] float cinderSlideVisualOffsetY = -0.1f;
        [SerializeField] float cinderSitCapsuleHeight = 1.05f;
        [SerializeField] float cinderSitCapsuleWidth = 0.68f;

        const string HandPointName = "HandPoint";

        Rigidbody2D body;
        CapsuleCollider2D capsule;
        SpriteRenderer visualRenderer;
        Transform visualTransform;
        Animator cachedAnimatorParameterSource;

        Vector2 moveInput;
        bool dashQueued;
        bool slideQueued;
        bool attackQueued;
        bool jumpReleased;
        int jumpsRemaining;
        int facingSign = 1;
        float dashTimer;
        float dashCooldownTimer;
        float slideTimer;
        float slideCooldownTimer;
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
        bool sitHeld;
        bool sitMovingHeld;
        bool slideInputHeld;
        bool wasSlideComboHeld;
        bool cinderWasSitting;
        bool cinderWasSitMoving;
        bool wasSitHeld;
        bool wasSittingPhysics;
        bool cinderUseFrontIdle = true;
        bool cinderSpriteFlipX;
        float cinderCycleTimer;
        int cinderFrame;
        Sprite[] cinderActiveLoopFrames;

        public bool IsSitting => useCinderWispSpriteAnimation && sitHeld && isGrounded && slideTimer <= 0f;
        public bool IsSitMoving => IsSitting && sitMovingHeld;
        public bool IsSliding => slideTimer > 0f;
        public bool IsAttacking => attackPulseTimer > 0f;
        public float AttackPulseDuration => attackPulseDuration;

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

        public void ConfigureCinderWispSprites(
            Sprite[] idleFrames,
            Sprite[] runFrames,
            Sprite[] jumpFrames,
            Sprite[] sitFrames,
            Sprite[] dashFrames,
            Sprite[] slideFrames,
            Sprite frontIdle = null,
            Sprite sideLeft = null,
            Sprite sideRight = null,
            Sprite sitIdle = null,
            Sprite[] attackFrames = null)
        {
            cinderIdleSprites = idleFrames;
            cinderRunSprites = runFrames;
            cinderJumpSprites = jumpFrames;
            cinderSitSprites = sitFrames;
            cinderSitIdleSprite = sitIdle;
            cinderDashSprites = dashFrames;
            cinderSlideSprites = slideFrames;
            cinderAttackSprites = attackFrames;
            if (attackFrames != null && attackFrames.Length > 0)
            {
                attackPulseDuration = attackFrames.Length / Mathf.Max(cinderAttackFrameRate, 0.01f);
            }
            cinderFrontIdleSprite = frontIdle ?? (idleFrames != null && idleFrames.Length > 0 ? idleFrames[0] : null);
            cinderSideLeftSprite = sideLeft;
            cinderSideRightSprite = sideRight;
            cinderUseFrontIdle = true;
            cinderSpriteFlipX = false;
            ResetCinderSpriteLoopState();
            useCinderWispSpriteAnimation = idleFrames != null && idleFrames.Length > 0;
            useConceptSpriteAnimation = false;
            useGeneratedWandererAnimation = false;
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
            slideTimer = 0f;
            hitStunTimer = hitStunDuration;
            hitFlashTimer = hitFlashDuration;
            body.linearVelocity = new Vector2(direction * knockback.x, knockback.y);
        }

        void BeginAttack()
        {
            attackPulseTimer = attackPulseDuration;
            attackQueued = false;
            cinderActiveLoopFrames = null;

            if (ropeWhipAttack != null)
            {
                ropeWhipAttack.SetFacing(facingSign > 0);
                ropeWhipAttack.PlayAttack(attackPulseDuration);
            }
            else if (ropeController != null)
            {
                ropeController.SetFacing(facingSign > 0);
                ropeController.PlaySwingAttack(attackPulseDuration);
            }
        }

        public void SetRopeWhipAttack(RopeWhipAttack2D attack)
        {
            ropeWhipAttack = attack;
        }

        public void SetRopeController(RopeController2D controller)
        {
            ropeController = controller;
        }

        void Reset()
        {
            CacheComponents();
            ConfigurePhysics();
            ConfigureVisual();
        }

        void Awake()
        {
            cinderUseFrontIdle = true;
            cinderSpriteFlipX = false;
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
            cinderIdleFrameRate = Mathf.Max(1f, cinderIdleFrameRate);
            cinderRunFrameRate = Mathf.Max(1f, cinderRunFrameRate);
            cinderDashFrameRate = Mathf.Max(1f, cinderDashFrameRate);
            cinderSlideFrameRate = Mathf.Max(1f, cinderSlideFrameRate);
            cinderSitFrameRate = Mathf.Max(1f, cinderSitFrameRate);
            cinderAttackFrameRate = Mathf.Max(1f, cinderAttackFrameRate);
            if (cinderAttackSprites != null && cinderAttackSprites.Length > 0)
            {
                attackPulseDuration = cinderAttackSprites.Length / cinderAttackFrameRate;
            }
            cinderSitMoveSpeed = Mathf.Max(0f, cinderSitMoveSpeed);
            cinderSlideSpeed = Mathf.Max(0f, cinderSlideSpeed);
            cinderSlideDuration = Mathf.Max(0f, cinderSlideDuration);
            cinderSlideCooldown = Mathf.Max(0f, cinderSlideCooldown);
            cinderSlideCapsuleHeight = Mathf.Clamp(cinderSlideCapsuleHeight, 0.55f, ColliderHeight);
            cinderSlideCapsuleWidth = Mathf.Clamp(cinderSlideCapsuleWidth, 0.4f, ColliderWidth);
            cinderSlideVisualOffsetY = Mathf.Clamp(cinderSlideVisualOffsetY, -0.35f, 0.05f);
            cinderSitCapsuleHeight = Mathf.Clamp(cinderSitCapsuleHeight, 0.5f, ColliderHeight);
            cinderSlideCapsuleHeight = Mathf.Min(cinderSlideCapsuleHeight, cinderSitCapsuleHeight - 0.01f);
            cinderSitCapsuleWidth = Mathf.Clamp(cinderSitCapsuleWidth, 0.4f, ColliderWidth);
            EnsureCinderAttackHandLocalsSize();

            if (!gameObject.scene.IsValid())
            {
                return;
            }

            CacheComponents(false);
            ConfigurePhysics();
            if (!Application.isPlaying)
            {
                ConfigureVisual();
            }
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

            ApplyColliderPose();
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
                slideQueued = false;
                return;
            }

            if (slideTimer > 0f)
            {
                body.linearVelocity = new Vector2(facingSign * cinderSlideSpeed, 0f);
                jumpBufferTimer = 0f;
                MaintainLowProfileGroundContact();
                UpdateGrounded();
                return;
            }

            if (dashTimer > 0f)
            {
                body.linearVelocity = new Vector2(facingSign * dashSpeed, 0f);
                jumpBufferTimer = 0f;
                return;
            }

            if (slideQueued && isGrounded && slideCooldownTimer <= 0f && dashTimer <= 0f)
            {
                StartSlide();
            }

            if (dashQueued && !slideQueued && dashCooldownTimer <= 0f && slideTimer <= 0f)
            {
                StartDash();
            }

            if (jumpBufferTimer > 0f && !IsSitting && slideTimer <= 0f)
            {
                TryJump();
            }

            bool sittingNow = sitHeld && isGrounded && slideTimer <= 0f;
            if (UsesLowProfileCollider())
            {
                if (sittingNow && !wasSitHeld)
                {
                    SnapFeetToGround();
                }

                MaintainLowProfileGroundContact();
                UpdateGrounded();
            }
            else if (wasSittingPhysics && isGrounded)
            {
                SnapFeetToGround();
                UpdateGrounded();
            }

            wasSitHeld = sittingNow;
            wasSittingPhysics = sittingNow || IsSliding;

            ApplyHorizontalMovement();
            ApplyBetterGravity();

            dashQueued = false;
            slideQueued = false;
        }

        void ReadInput()
        {
            float horizontal = 0f;
            sitHeld = false;
            sitMovingHeld = false;
            slideInputHeld = false;
            bool shiftHeld = false;
            bool downHeld = false;
            bool horizontalPressed = false;

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

                shiftHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
                downHeld = keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed || keyboard.cKey.isPressed;

                if (useCinderWispSpriteAnimation)
                {
                    sitHeld = downHeld && !shiftHeld;
                    sitMovingHeld = sitHeld && Mathf.Abs(horizontal) > 0.01f;
                    slideInputHeld = shiftHeld && downHeld && Mathf.Abs(horizontal) > 0.01f;
                }

                if (keyboard.spaceKey.wasPressedThisFrame)
                {
                    jumpBufferTimer = jumpBufferTime;
                }

                jumpReleased |= keyboard.spaceKey.wasReleasedThisFrame;

                bool shiftPressed = keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame;
                bool downPressed = keyboard.downArrowKey.wasPressedThisFrame
                    || keyboard.sKey.wasPressedThisFrame
                    || keyboard.cKey.wasPressedThisFrame;
                horizontalPressed |= keyboard.aKey.wasPressedThisFrame
                    || keyboard.dKey.wasPressedThisFrame
                    || keyboard.leftArrowKey.wasPressedThisFrame
                    || keyboard.rightArrowKey.wasPressedThisFrame;

                if (shiftPressed && !downHeld)
                {
                    dashQueued = true;
                }

                if (TryQueueSlide(shiftHeld, downHeld, horizontal, shiftPressed, downPressed, horizontalPressed))
                {
                    slideQueued = true;
                    dashQueued = false;
                    cinderUseFrontIdle = false;
                }

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
                bool gamepadShiftHeld = gamepad.leftShoulder.isPressed || gamepad.rightShoulder.isPressed;
                bool gamepadDownHeld = stick.y < -0.75f && Mathf.Abs(stick.x) < 0.35f;
                bool shoulderPressed = gamepad.leftShoulder.wasPressedThisFrame || gamepad.rightShoulder.wasPressedThisFrame;
                shiftHeld |= gamepadShiftHeld;
                downHeld |= gamepadDownHeld;

                if (useCinderWispSpriteAnimation)
                {
                    bool gamepadSitHeld = gamepadDownHeld && !gamepadShiftHeld;
                    sitHeld |= gamepadSitHeld;
                    sitMovingHeld |= gamepadSitHeld && Mathf.Abs(horizontal) > 0.01f;
                    bool gamepadSlideInput = gamepadShiftHeld && stick.y < -0.65f && Mathf.Abs(stick.x) > 0.35f;
                    slideInputHeld |= gamepadSlideInput;
                }

                if (shoulderPressed && !gamepadDownHeld)
                {
                    dashQueued = true;
                }

                if (TryQueueSlide(
                        gamepadShiftHeld,
                        stick.y < -0.65f,
                        stick.x,
                        shoulderPressed,
                        false,
                        Mathf.Abs(stick.x) > 0.45f))
                {
                    slideQueued = true;
                    dashQueued = false;
                    cinderUseFrontIdle = false;
                }

                if (gamepad.buttonSouth.wasPressedThisFrame)
                {
                    jumpBufferTimer = jumpBufferTime;
                }

                jumpReleased |= gamepad.buttonSouth.wasReleasedThisFrame;

                attackQueued |= gamepad.buttonWest.wasPressedThisFrame;

                if (gamepad.buttonNorth.wasPressedThisFrame)
                {
                    ApplyHitFrom((Vector2)transform.position + new Vector2(facingSign, 0f));
                }
            }

            horizontal = Mathf.Clamp(horizontal, -1f, 1f);
            moveInput = new Vector2(horizontal, 0f);
            wasSlideComboHeld = useCinderWispSpriteAnimation && slideInputHeld;

            if (useCinderWispSpriteAnimation && cinderUseFrontIdle && Mathf.Abs(horizontal) > 0.01f)
            {
                cinderUseFrontIdle = false;
            }

            if (Mathf.Abs(horizontal) > 0.01f && !slideInputHeld)
            {
                facingSign = horizontal > 0f ? 1 : -1;
            }

            if (attackQueued)
            {
                BeginAttack();
            }
        }

        void TickTimers(float deltaTime)
        {
            dashTimer = Mathf.Max(0f, dashTimer - deltaTime);
            dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - deltaTime);
            slideTimer = Mathf.Max(0f, slideTimer - deltaTime);
            slideCooldownTimer = Mathf.Max(0f, slideCooldownTimer - deltaTime);
            hitStunTimer = Mathf.Max(0f, hitStunTimer - deltaTime);
            hitFlashTimer = Mathf.Max(0f, hitFlashTimer - deltaTime);
            attackPulseTimer = Mathf.Max(0f, attackPulseTimer - deltaTime);
            jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - deltaTime);
        }

        void ApplyHorizontalMovement()
        {
            if (slideTimer > 0f || dashTimer > 0f)
            {
                return;
            }

            float speed = IsSitMoving ? cinderSitMoveSpeed : IsSitting ? 0f : moveSpeed;
            float targetSpeed = moveInput.x * speed;
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

            cinderUseFrontIdle = false;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            slideTimer = 0f;
            body.linearVelocity = new Vector2(facingSign * dashSpeed, 0f);
        }

        void StartSlide()
        {
            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                facingSign = moveInput.x > 0f ? 1 : -1;
            }

            cinderUseFrontIdle = false;
            slideTimer = cinderSlideDuration;
            slideCooldownTimer = cinderSlideCooldown;
            dashTimer = 0f;
            ResetCinderSpriteLoopState();
            if (cinderSlideSprites != null && cinderSlideSprites.Length > 0)
            {
                cinderFrame = Mathf.Min(2, cinderSlideSprites.Length - 1);
            }

            ApplyColliderPose();
            SnapFeetToGround();
            UpdateGrounded();
            body.linearVelocity = new Vector2(facingSign * cinderSlideSpeed, 0f);
        }

        bool TryQueueSlide(bool shiftHeld, bool downHeld, float horizontal, bool shiftPressed, bool downPressed, bool horizontalPressed)
        {
            if (!useCinderWispSpriteAnimation)
            {
                return false;
            }

            if (!shiftHeld || !downHeld || Mathf.Abs(horizontal) <= 0.01f)
            {
                return false;
            }

            return !wasSlideComboHeld || shiftPressed || downPressed || horizontalPressed;
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
            ApplyColliderPose();
        }

        void ApplyColliderPose()
        {
            if (capsule == null)
            {
                return;
            }

            float height = ColliderHeight;
            float width = ColliderWidth;
            if (IsSliding)
            {
                height = cinderSlideCapsuleHeight;
                width = cinderSlideCapsuleWidth;
            }
            else if (UsesSitProfileCollider())
            {
                height = cinderSitCapsuleHeight;
                width = cinderSitCapsuleWidth;
            }

            float offsetY = (ColliderHeight - height) * 0.5f;
            capsule.size = new Vector2(width, height);
            capsule.offset = new Vector2(0f, -offsetY);
        }

        bool UsesSitProfileCollider()
        {
            return sitHeld && (isGrounded || wasSitHeld);
        }

        bool UsesLowProfileCollider()
        {
            if (slideTimer > 0f)
            {
                return isGrounded || wasSittingPhysics;
            }

            return UsesSitProfileCollider();
        }

        void ConfigureVisual()
        {
            if (visualRenderer == null)
            {
                return;
            }

            if (UsesCinderWispSpriteAnimation())
            {
                if (!Application.isPlaying)
                {
                    Sprite[] idleFrames = cinderIdleSprites;
                    visualRenderer.sprite = idleFrames != null && idleFrames.Length > 0 ? idleFrames[0] : bodySprite;
                    ResetCinderSpriteLoopState();
                    cinderWasSitting = false;
                    cinderWasSitMoving = false;
                }
            }
            else if (UsesGeneratedPrototypeVisual())
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

            visualTransform.localRotation = Quaternion.identity;
            if (UsesCinderWispSpriteAnimation())
            {
                ApplyCinderVisualFeetPose();
            }
            else
            {
                visualTransform.localPosition = Vector3.zero;
                visualTransform.localScale = new Vector3(VisualWidthScale, VisualHeightScale, 1f);
            }
        }

        void UpdateVisualMotion()
        {
            if (visualRenderer == null || visualTransform == null)
            {
                return;
            }

            Color color = idleColor;
            Vector3 scale = UsesCinderWispSpriteAnimation()
                ? Vector3.one
                : new(VisualWidthScale, VisualHeightScale, 1f);

            if (UsesCinderWispSpriteAnimation())
            {
                UpdateCinderWispSpriteAnimation();
            }
            else
            {
                UpdateConceptSpriteAnimation();
                UpdateGeneratedPrototypeAnimation();
            }

            if (dashTimer > 0f)
            {
                color = dashColor;
                if (!UsesCinderWispSpriteAnimation())
                {
                    scale = new Vector3(VisualWidthScale * 1.12f, VisualHeightScale * 0.86f, 1f);
                }
            }

            if (attackPulseTimer > 0f)
            {
                color = attackColor;
                if (!UsesCinderWispSpriteAnimation())
                {
                    scale = new Vector3(VisualWidthScale * 1.08f, VisualHeightScale * 0.96f, 1f);
                }
            }

            if (hitFlashTimer > 0f)
            {
                color = hitColor;
                if (!UsesCinderWispSpriteAnimation())
                {
                    scale = new Vector3(VisualWidthScale * 1.14f, VisualHeightScale * 0.88f, 1f);
                }
            }

            visualRenderer.color = color;
            visualRenderer.flipX = UsesCinderWispSpriteAnimation() ? cinderSpriteFlipX : facingSign < 0;

            if (UsesCinderWispSpriteAnimation())
            {
                ApplyCinderVisualFeetPose();
            }
            else
            {
                visualTransform.localScale = scale;
            }

            UpdateAnimatorParameters();
        }

        void ApplyCinderVisualFeetPose(bool skipHandSync)
        {
            if (visualTransform == null || capsule == null)
            {
                return;
            }

            Sprite activeSprite = visualRenderer != null ? visualRenderer.sprite : null;
            float feetLocalY = capsule.offset.y - capsule.size.y * 0.5f;
            float spriteBottomLocalY = activeSprite != null ? activeSprite.bounds.min.y : 0f;
            float uniformScale = 1f;

            if (IsSitting)
            {
                Sprite sideReferenceSprite = GetCinderSideReferenceSprite();
                Sprite sitReferenceSprite = GetCinderSitReferenceSprite();
                uniformScale = GetCinderLowProfileVisualScale(activeSprite, sitReferenceSprite, sideReferenceSprite);
            }

            visualTransform.localScale = new Vector3(uniformScale, uniformScale, 1f);
            float visualOffsetY = IsSliding ? cinderSlideVisualOffsetY : 0f;
            visualTransform.localPosition = new Vector3(0f, feetLocalY - spriteBottomLocalY * uniformScale + visualOffsetY, 0f);
            if (!skipHandSync)
            {
                SyncCinderHandPoint();
            }
        }

        void ApplyCinderVisualFeetPose()
        {
            ApplyCinderVisualFeetPose(skipHandSync: false);
        }

        void EnsureCinderAttackHandLocalsSize()
        {
            int targetCount = cinderAttackSprites != null && cinderAttackSprites.Length > 0
                ? cinderAttackSprites.Length
                : Mathf.Max(cinderAttackHandLocals?.Length ?? 0, 1);

            if (targetCount <= 0)
            {
                return;
            }

            if (cinderAttackHandLocals != null && cinderAttackHandLocals.Length == targetCount)
            {
                return;
            }

            Vector2[] resized = new Vector2[targetCount];
            for (int i = 0; i < targetCount; i++)
            {
                if (cinderAttackHandLocals != null && i < cinderAttackHandLocals.Length)
                {
                    resized[i] = cinderAttackHandLocals[i];
                }
                else
                {
                    resized[i] = cinderIdleHandLocal;
                }
            }

            cinderAttackHandLocals = resized;
        }

        int GetCinderAttackFrameIndex()
        {
            if (attackPulseTimer <= 0f)
            {
                return 0;
            }

            int frameCount = cinderAttackSprites != null && cinderAttackSprites.Length > 0
                ? cinderAttackSprites.Length
                : cinderAttackHandLocals != null ? cinderAttackHandLocals.Length : 1;
            float attackProgress = 1f - attackPulseTimer / Mathf.Max(0.01f, attackPulseDuration);
            int frame = Mathf.Clamp(Mathf.FloorToInt(attackProgress * frameCount), 0, frameCount - 1);
            if (attackProgress >= 0.999f)
            {
                frame = frameCount - 1;
            }

            return frame;
        }

        Vector2 GetCurrentCinderHandLocal()
        {
            if (attackPulseTimer > 0f && cinderAttackHandLocals != null && cinderAttackHandLocals.Length > 0)
            {
                int frame = Mathf.Clamp(GetCinderAttackFrameIndex(), 0, cinderAttackHandLocals.Length - 1);
                return cinderAttackHandLocals[frame];
            }

            return cinderIdleHandLocal;
        }

        Vector2 ApplyCinderHandFacing(Vector2 local)
        {
            if (cinderMirrorHandXOnFlip && cinderSpriteFlipX)
            {
                local.x = -local.x;
            }

            return local;
        }

        public void PreviewCinderAttackHandFrame(int frameIndex)
        {
            CacheComponents(false);
            EnsureCinderAttackHandLocalsSize();

            if (cinderAttackSprites != null && cinderAttackSprites.Length > 0 && visualRenderer != null)
            {
                frameIndex = Mathf.Clamp(frameIndex, 0, cinderAttackSprites.Length - 1);
                visualRenderer.sprite = cinderAttackSprites[frameIndex];
            }

            if (cinderAttackHandLocals != null && cinderAttackHandLocals.Length > 0)
            {
                frameIndex = Mathf.Clamp(frameIndex, 0, cinderAttackHandLocals.Length - 1);
                CacheCinderHandPoint();
                if (cinderHandPoint != null)
                {
                    Vector2 local = ApplyCinderHandFacing(cinderAttackHandLocals[frameIndex]);
                    cinderHandPoint.localPosition = new Vector3(local.x, local.y, 0f);
                }
            }

            if (visualTransform != null)
            {
                ApplyCinderVisualFeetPose(skipHandSync: true);
            }
        }

        public bool TryCaptureCinderHandPointToAttackFrame(int frameIndex)
        {
            EnsureCinderAttackHandLocalsSize();
            if (cinderAttackHandLocals == null || cinderAttackHandLocals.Length == 0)
            {
                return false;
            }

            CacheCinderHandPoint();
            if (cinderHandPoint == null)
            {
                return false;
            }

            frameIndex = Mathf.Clamp(frameIndex, 0, cinderAttackHandLocals.Length - 1);
            Vector3 local = cinderHandPoint.localPosition;
            if (cinderMirrorHandXOnFlip && cinderSpriteFlipX)
            {
                local.x = -local.x;
            }

            cinderAttackHandLocals[frameIndex] = new Vector2(local.x, local.y);
            return true;
        }

        public void FillCinderAttackHandLocalsFromIdle()
        {
            EnsureCinderAttackHandLocalsSize();
            if (cinderAttackHandLocals == null)
            {
                return;
            }

            for (int i = 0; i < cinderAttackHandLocals.Length; i++)
            {
                cinderAttackHandLocals[i] = cinderIdleHandLocal;
            }
        }

        void CacheCinderHandPoint()
        {
            if (cinderHandPoint != null)
            {
                return;
            }

            if (visualTransform != null)
            {
                cinderHandPoint = visualTransform.Find(HandPointName);
            }

            if (cinderHandPoint == null)
            {
                cinderHandPoint = transform.Find(HandPointName);
            }
        }

        void SyncCinderHandPoint()
        {
            if (!UsesCinderWispSpriteAnimation())
            {
                return;
            }

            CacheCinderHandPoint();
            if (cinderHandPoint == null)
            {
                return;
            }

            Vector2 local = ApplyCinderHandFacing(GetCurrentCinderHandLocal());
            cinderHandPoint.localPosition = new Vector3(local.x, local.y, 0f);
        }

        Sprite GetCinderSitReferenceSprite()
        {
            if (cinderSitSprites != null && cinderSitSprites.Length > 0)
            {
                return cinderSitSprites[0];
            }

            return cinderSitIdleSprite;
        }

        static float GetCinderLowProfileVisualScale(
            Sprite activeSprite,
            Sprite sitReferenceSprite,
            Sprite sideReferenceSprite)
        {
            if (activeSprite == null || sitReferenceSprite == null || sideReferenceSprite == null)
            {
                return 1f;
            }

            float referenceWidth = sideReferenceSprite.bounds.size.x;
            float sitWidth = sitReferenceSprite.bounds.size.x;
            float sitScale = sitWidth > referenceWidth + 0.001f ? referenceWidth / sitWidth : 1f;
            float targetHeight = sitReferenceSprite.bounds.size.y * sitScale;
            return targetHeight / Mathf.Max(activeSprite.bounds.size.y, 0.001f);
        }

        void MaintainLowProfileGroundContact()
        {
            MaintainSitGroundContact();
        }

        void MaintainSitGroundContact()
        {
            if (body == null)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;
            if (velocity.y < 0f)
            {
                body.linearVelocity = new Vector2(velocity.x, 0f);
            }

            SnapFeetToGround();
        }

        void SnapFeetToGround()
        {
            if (body == null || capsule == null || !TrySampleGroundY(out float groundY))
            {
                return;
            }

            float delta = groundY - capsule.bounds.min.y;
            if (Mathf.Abs(delta) <= 0.002f)
            {
                return;
            }

            if ((IsSitting || IsSliding) && delta < 0f)
            {
                return;
            }

            body.MovePosition(body.position + new Vector2(0f, delta));
        }

        bool TrySampleGroundY(out float groundY)
        {
            groundY = 0f;
            if (capsule == null)
            {
                return false;
            }

            ContactFilter2D filter = new();
            filter.SetLayerMask(groundLayers);
            filter.useTriggers = false;

            Bounds bounds = capsule.bounds;
            float halfWidth = Mathf.Min(bounds.extents.x, groundCheckWidth * 0.5f);
            float probeDistance = groundCheckDistance + GroundProbeSkin + 0.35f;
            float probeY = bounds.min.y + GroundProbeSkin;

            Vector2[] origins =
            {
                new(bounds.center.x, probeY),
                new(bounds.center.x - halfWidth, probeY),
                new(bounds.center.x + halfWidth, probeY),
            };

            bool found = false;
            float bestY = float.NegativeInfinity;

            for (int i = 0; i < origins.Length; i++)
            {
                int hitCount = Physics2D.Raycast(origins[i], Vector2.down, filter, groundHits, probeDistance);
                for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
                {
                    Collider2D hitCollider = groundHits[hitIndex].collider;
                    if (hitCollider == null || hitCollider == capsule || hitCollider.transform.IsChildOf(transform))
                    {
                        continue;
                    }

                    float hitY = groundHits[hitIndex].point.y;
                    if (!found || hitY > bestY)
                    {
                        bestY = hitY;
                        found = true;
                    }
                }
            }

            if (!found)
            {
                return false;
            }

            groundY = bestY;
            return true;
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

        void UpdateCinderWispSpriteAnimation()
        {
            bool animationGrounded = IsAnimationGrounded();
            bool sitting = IsSitting;
            bool sitMoving = IsSitMoving;
            if (sitting && !cinderWasSitting)
            {
                ResetCinderSpriteLoopState();
            }

            if (sitMoving != cinderWasSitMoving)
            {
                ResetCinderSpriteLoopState();
            }

            cinderWasSitting = sitting;
            cinderWasSitMoving = sitMoving;
            cinderSpriteFlipX = false;

            float horizontalSpeed = GetCinderHorizontalSpeed();
            bool wantsMoveAnimation = WantsCinderMoveAnimation(sitting);

            if (cinderUseFrontIdle && dashTimer <= 0f && slideTimer <= 0f && animationGrounded && !sitting)
            {
                if (!wantsMoveAnimation && horizontalSpeed <= animationMovementThreshold && TrySetCinderFrontIdleSprite())
                {
                    return;
                }
            }

            cinderSpriteFlipX = facingSign < 0;

            if (slideTimer > 0f && cinderSlideSprites != null && cinderSlideSprites.Length > 0)
            {
                float slideFrameRate = cinderSlideSprites.Length / Mathf.Max(cinderSlideDuration, 0.01f);
                PlayCinderSpriteLoop(cinderSlideSprites, slideFrameRate, Time.deltaTime);
                return;
            }

            if (dashTimer > 0f && cinderDashSprites != null && cinderDashSprites.Length > 0)
            {
                PlayCinderSpriteLoop(cinderDashSprites, cinderDashFrameRate, Time.deltaTime);
                return;
            }

            if (attackPulseTimer > 0f && cinderAttackSprites != null && cinderAttackSprites.Length > 0)
            {
                cinderActiveLoopFrames = null;
                float attackProgress = 1f - attackPulseTimer / Mathf.Max(0.01f, attackPulseDuration);
                int frameCount = cinderAttackSprites.Length;
                int frame = Mathf.Clamp(Mathf.FloorToInt(attackProgress * frameCount), 0, frameCount - 1);
                if (attackProgress >= 0.999f)
                {
                    frame = frameCount - 1;
                }

                cinderSpriteFlipX = facingSign < 0;
                SetCinderSprite(cinderAttackSprites[frame]);
                return;
            }

            if (dashTimer > 0f)
            {
                return;
            }

            if (!animationGrounded && cinderJumpSprites != null && cinderJumpSprites.Length > 0)
            {
                cinderActiveLoopFrames = null;
                int lastFrame = cinderJumpSprites.Length - 1;
                float verticalSpeed = body != null ? body.linearVelocity.y : 0f;
                int frame = verticalSpeed > 0.35f
                    ? 0
                    : verticalSpeed > -0.35f
                        ? Mathf.Min(1, lastFrame)
                        : Mathf.Min(2, lastFrame);
                SetCinderSprite(cinderJumpSprites[frame]);
                return;
            }

            if (sitting)
            {
                if (sitMoving && cinderSitSprites != null && cinderSitSprites.Length > 0)
                {
                    PlayCinderSpriteLoop(cinderSitSprites, cinderSitFrameRate, Time.deltaTime);
                    return;
                }

                cinderActiveLoopFrames = null;
                if (cinderSitIdleSprite != null)
                {
                    SetCinderSprite(cinderSitIdleSprite);
                    return;
                }

                if (cinderSitSprites != null && cinderSitSprites.Length > 0)
                {
                    SetCinderSprite(cinderSitSprites[0]);
                    return;
                }
            }

            if (hitStunTimer <= 0f && wantsMoveAnimation && cinderRunSprites != null && cinderRunSprites.Length > 0)
            {
                PlayCinderSpriteLoop(cinderRunSprites, cinderRunFrameRate, Time.deltaTime);
                return;
            }

            cinderActiveLoopFrames = null;
            if (!wantsMoveAnimation && TrySetCinderSideIdleSprite())
            {
                return;
            }

            if (cinderIdleSprites != null && cinderIdleSprites.Length > 0)
            {
                PlayCinderSpriteLoop(cinderIdleSprites, cinderIdleFrameRate, Time.deltaTime);
            }
        }

        bool IsAnimationGrounded()
        {
            if (isGrounded)
            {
                return true;
            }

            if (body == null)
            {
                return false;
            }

            return coyoteTimer > 0f && body.linearVelocity.y <= 0.35f;
        }

        float GetCinderHorizontalSpeed()
        {
            float moveSpeedForAnimation = IsSitMoving ? cinderSitMoveSpeed : IsSitting ? 0f : moveSpeed;
            float inputSpeed = Mathf.Abs(moveInput.x) * moveSpeedForAnimation;
            if (body == null)
            {
                return inputSpeed;
            }

            return Mathf.Max(Mathf.Abs(body.linearVelocity.x), inputSpeed);
        }

        bool WantsCinderMoveAnimation(bool sitting)
        {
            if (sitting || dashTimer > 0f || slideTimer > 0f)
            {
                return false;
            }

            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                return true;
            }

            return GetCinderHorizontalSpeed() > animationMovementThreshold;
        }

        void ResetCinderSpriteLoopState()
        {
            cinderActiveLoopFrames = null;
            cinderCycleTimer = 0f;
            cinderFrame = 0;
        }

        bool TrySetCinderFrontIdleSprite()
        {
            Sprite frontSprite = cinderFrontIdleSprite;
            if (frontSprite == null && cinderIdleSprites != null && cinderIdleSprites.Length > 0)
            {
                frontSprite = cinderIdleSprites[0];
            }

            if (frontSprite == null)
            {
                return false;
            }

            SetCinderSprite(frontSprite);
            return true;
        }

        bool TrySetCinderSideIdleSprite()
        {
            if (facingSign >= 0 && cinderSideRightSprite != null)
            {
                SetCinderSprite(cinderSideRightSprite);
                cinderSpriteFlipX = false;
                return true;
            }

            if (facingSign < 0 && cinderSideLeftSprite != null)
            {
                SetCinderSprite(cinderSideLeftSprite);
                cinderSpriteFlipX = false;
                return true;
            }

            return false;
        }

        Sprite GetCinderSideReferenceSprite()
        {
            if (cinderSideRightSprite != null)
            {
                return cinderSideRightSprite;
            }

            if (cinderIdleSprites != null && cinderIdleSprites.Length > 0)
            {
                return cinderIdleSprites[0];
            }

            return cinderFrontIdleSprite;
        }

        void PlayCinderSpriteLoop(Sprite[] frames, float frameRate, float deltaTime)
        {
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            if (frames != cinderActiveLoopFrames)
            {
                cinderActiveLoopFrames = frames;
                cinderCycleTimer = 0f;
                cinderFrame = 0;
            }

            if (frames.Length == 1)
            {
                SetCinderSprite(frames[0]);
                return;
            }

            cinderCycleTimer += deltaTime;
            float frameDuration = 1f / Mathf.Max(frameRate, 0.01f);

            while (cinderCycleTimer >= frameDuration)
            {
                cinderCycleTimer -= frameDuration;
                cinderFrame = (cinderFrame + 1) % frames.Length;
            }

            SetCinderSprite(frames[cinderFrame]);
        }

        void SetCinderSprite(Sprite sprite)
        {
            if (sprite == null || visualRenderer == null)
            {
                return;
            }

            visualRenderer.sprite = sprite;
        }

        bool UsesCinderWispSpriteAnimation()
        {
            return useCinderWispSpriteAnimation && cinderIdleSprites != null && cinderIdleSprites.Length > 0;
        }

        bool UsesGeneratedPrototypeVisual()
        {
            return !UsesCinderWispSpriteAnimation() && (useGeneratedWandererAnimation || bodySprite == null);
        }

        bool UsesConceptSpriteAnimation()
        {
            return useConceptSpriteAnimation && !UsesCinderWispSpriteAnimation() && !UsesGeneratedPrototypeVisual();
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
