using UnityEngine;

namespace Penumbra.Puzzles
{
    [DisallowMultipleComponent]
    public sealed class LightReceiver2D : MonoBehaviour
    {
        [SerializeField] ReflectiveDoor2D linkedDoor;
        [SerializeField] SpriteRenderer statusRenderer;
        [SerializeField] Color idleColor = new(0.36f, 0.35f, 0.42f, 1f);
        [SerializeField] Color activeColor = new(1f, 0.86f, 0.35f, 1f);

        int lastBeamFrame = -100;

        public void SetLinkedDoor(ReflectiveDoor2D door)
        {
            linkedDoor = door;
        }

        public void ReceiveBeam(LightBeamEmitter2D source)
        {
            lastBeamFrame = Time.frameCount;
        }

        void Reset()
        {
            statusRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        void LateUpdate()
        {
            bool active = Time.frameCount - lastBeamFrame <= 1;

            if (statusRenderer != null)
            {
                statusRenderer.color = active ? activeColor : idleColor;
            }

            if (linkedDoor != null)
            {
                linkedDoor.SetOpen(active);
            }
        }
    }
}
