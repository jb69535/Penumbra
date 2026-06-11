using UnityEngine;

namespace Penumbra.Data
{
    [CreateAssetMenu(menuName = "Penumbra/Tuning/Player Movement", fileName = "PlayerMovementTuning")]
    public sealed class PlayerMovementTuning : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 6f;
        public float acceleration = 80f;
        public float airAcceleration = 42f;

        [Header("Jump")]
        public float jumpVelocity = 10.5f;
        public int extraAirJumps = 1;

        [Header("Dash")]
        public float dashSpeed = 16f;
        public float dashDuration = 0.14f;
        public float dashCooldown = 0.45f;
    }
}
