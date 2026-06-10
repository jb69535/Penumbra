using UnityEngine;
using UnityEngine.InputSystem;

namespace Penumbra.Puzzles
{
    [DisallowMultipleComponent]
    public sealed class MirrorAimController2D : MonoBehaviour
    {
        [SerializeField] Transform mirror;
        [SerializeField] float rotateSpeed = 80f;
        [SerializeField] bool requireAimHeld = true;

        public void Configure(Transform mirrorTransform, bool requireAimButton)
        {
            mirror = mirrorTransform;
            requireAimHeld = requireAimButton;
        }

        void Reset()
        {
            mirror = transform;
        }

        void Update()
        {
            Transform target = mirror != null ? mirror : transform;
            float input = ReadInput();
            if (Mathf.Abs(input) <= 0.01f)
            {
                return;
            }

            target.Rotate(0f, 0f, -input * rotateSpeed * Time.deltaTime);
        }

        float ReadInput()
        {
            float input = 0f;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (!requireAimHeld || keyboard.rKey.isPressed))
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    input -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    input += 1f;
                }

                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    input += 0.35f;
                }

                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    input -= 0.35f;
                }
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                bool canAim = !requireAimHeld || gamepad.leftTrigger.isPressed || gamepad.rightTrigger.isPressed;
                if (canAim)
                {
                    input += gamepad.rightStick.ReadValue().x;
                }
            }

            return Mathf.Clamp(input, -1f, 1f);
        }
    }
}
