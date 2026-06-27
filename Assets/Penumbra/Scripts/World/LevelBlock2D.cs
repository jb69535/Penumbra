using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Penumbra.World
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class LevelBlock2D : MonoBehaviour
    {
        [SerializeField] Vector2 size = Vector2.one;
        [SerializeField] Color color = new(0.22f, 0.25f, 0.31f, 1f);
        [SerializeField] bool collisionEnabled = true;
        [SerializeField] bool isTrigger;
        [SerializeField] string sortingLayerName = "Default";
        [SerializeField] int sortingOrder;
        [SerializeField] Sprite blockSprite;

        static Sprite runtimeBlockSprite;

        BoxCollider2D boxCollider;
        SpriteRenderer spriteRenderer;

        public void ConfigureBlock(Vector2 newSize, Color newColor, bool trigger, Sprite sprite)
        {
            ConfigureBlock(newSize, newColor, true, trigger, sortingLayerName, sortingOrder, sprite);
        }

        public void ConfigureBlock(Vector2 newSize, Color newColor, bool collidable, bool trigger, string layerName, int order, Sprite sprite)
        {
            size = new Vector2(Mathf.Max(0.01f, newSize.x), Mathf.Max(0.01f, newSize.y));
            color = newColor;
            collisionEnabled = collidable;
            isTrigger = trigger;
            sortingLayerName = string.IsNullOrWhiteSpace(layerName) ? "Default" : layerName;
            sortingOrder = order;
            blockSprite = sprite;
            ApplyConfiguration();
        }

        void Reset()
        {
            ApplyConfiguration();
        }

        void Awake()
        {
            ApplyConfiguration();
        }

        void OnEnable()
        {
            ApplyConfiguration();
        }

        void OnValidate()
        {
            size = new Vector2(Mathf.Max(0.01f, size.x), Mathf.Max(0.01f, size.y));
#if UNITY_EDITOR
            EditorApplication.delayCall += DeferredApplyConfiguration;
#else
            ApplyConfiguration(false);
#endif
        }

#if UNITY_EDITOR
        void DeferredApplyConfiguration()
        {
            if (this == null)
            {
                return;
            }

            ApplyConfiguration(false);
        }
#endif

        void ApplyConfiguration(bool createMissing = true)
        {
            CacheComponents(createMissing);

            if (boxCollider != null)
            {
                boxCollider.enabled = collisionEnabled;
                boxCollider.size = size;
                boxCollider.offset = Vector2.zero;
                boxCollider.isTrigger = isTrigger;
            }

            if (spriteRenderer != null)
            {
                if (blockSprite != null)
                {
                    spriteRenderer.sprite = blockSprite;
                }
                else if (spriteRenderer.sprite == null)
                {
                    spriteRenderer.sprite = GetRuntimeBlockSprite();
                }

                spriteRenderer.color = color;
                spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                spriteRenderer.size = size;
                spriteRenderer.sortingLayerName = sortingLayerName;
                spriteRenderer.sortingOrder = sortingOrder;
            }
        }

        void CacheComponents(bool createMissing = true)
        {
            if (boxCollider == null && !TryGetComponent(out boxCollider) && createMissing)
            {
                boxCollider = gameObject.AddComponent<BoxCollider2D>();
            }

            if (spriteRenderer == null && !TryGetComponent(out spriteRenderer) && createMissing)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        static Sprite GetRuntimeBlockSprite()
        {
            if (runtimeBlockSprite != null)
            {
                return runtimeBlockSprite;
            }

            const int size = 32;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
            {
                name = "Runtime Penumbra Block",
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color center = Color.white;
            Color edge = new(0.8f, 0.86f, 0.94f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isEdge = x < 3 || x >= size - 3 || y < 3 || y >= size - 3;
                    texture.SetPixel(x, y, isEdge ? edge : center);
                }
            }

            texture.Apply();
            runtimeBlockSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect, new Vector4(4f, 4f, 4f, 4f));
            runtimeBlockSprite.name = "Runtime Penumbra Block Sprite";
            runtimeBlockSprite.hideFlags = HideFlags.HideAndDontSave;
            return runtimeBlockSprite;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = isTrigger ? new Color(1f, 0.2f, 0.2f, 0.55f) : new Color(0.2f, 0.75f, 1f, 0.55f);
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0.05f));
        }
    }
}
