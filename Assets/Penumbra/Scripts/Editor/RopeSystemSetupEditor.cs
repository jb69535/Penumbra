using System;
using System.Collections.Generic;
using System.IO;
using Penumbra.Combat;
using Penumbra.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Penumbra.EditorTools
{
    public static class RopeSystemSetupEditor
    {
        const string RopeArtFolder = "Assets/Penumbra/Art/Combat/Rope";
        const string AttackFramesFolder = "Assets/Penumbra/Art/Characters/Player/Frames/Attack";
        const string MaterialPath = "Assets/Penumbra/Art/Combat/Rope/M_RopeLine.mat";
        const string SandboxScenePath = "Assets/Penumbra/Scenes/Sandboxes/Sandbox_Movement2D.unity";
        const string ImportScriptPath = "Tools/import_rope_attack.py";
        const float TargetCharacterHeight = 1.8f;

        [MenuItem("Tools/Penumbra/Setup Rope Attack System")]
        public static void SetupRopeAttackSystem()
        {
            SetupRopeAttackSystemInternal(showDialogs: !Application.isBatchMode);
        }

        public static void SetupRopeAttackSystemFromCommandLine()
        {
            SetupRopeAttackSystemInternal(showDialogs: false);
        }

        static void SetupRopeAttackSystemInternal(bool showDialogs)
        {
            RunRopeImport();
            AssetDatabase.Refresh();

            Material ropeMaterial = EnsureRopeMaterial();
            Sprite ropeTipSprite = ImportRopeSprite($"{RopeArtFolder}/rope_tip.png");
            Sprite ropeHandleSprite = ImportRopeSprite($"{RopeArtFolder}/rope_handle.png");
            Sprite[] attackFrames = ImportAttackFrames();

            if (ropeMaterial == null || ropeTipSprite == null || ropeHandleSprite == null || attackFrames.Length == 0)
            {
                if (showDialogs)
                {
                    EditorUtility.DisplayDialog(
                        "Penumbra Rope Setup",
                        "Rope or attack assets are missing.\n\nRun Tools/import_rope_attack.py first, then try again.",
                        "OK");
                }

                return;
            }

            if (!File.Exists(SandboxScenePath))
            {
                if (showDialogs)
                {
                    EditorUtility.DisplayDialog("Penumbra Rope Setup", $"Scene not found:\n{SandboxScenePath}", "OK");
                }

                return;
            }

            Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);
            PenumbraCharacterController2D player = UnityEngine.Object.FindFirstObjectByType<PenumbraCharacterController2D>(FindObjectsInactive.Include);
            if (player == null)
            {
                if (showDialogs)
                {
                    EditorUtility.DisplayDialog("Penumbra Rope Setup", "Wanderer_MovementSandbox was not found.", "OK");
                }

                return;
            }

            DisableLegacyChainAttack(player);
            RopeController2D ropeController = BuildRopeHierarchy(player.transform, ropeMaterial, ropeTipSprite, ropeHandleSprite);
            WireAttackSprites(player, attackFrames, ropeController);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeObject = player.gameObject;

            if (showDialogs)
            {
                EditorUtility.DisplayDialog(
                    "Penumbra Rope Setup",
                    "Rope attack system is wired to Wanderer_MovementSandbox.\n\n" +
                    "- J / gamepad West: attack animation + rope swing\n" +
                    "- RopeSystem: HandPoint, RopeLine, RopeTip, RopeHandle, RopeHitbox\n" +
                    "- Legacy ChainAttack2D input disabled\n\n" +
                    "Press Play in Sandbox_Movement2D to test.",
                    "OK");
            }
        }

        static void RunRopeImport()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string scriptPath = Path.Combine(projectRoot, ImportScriptPath);
            if (!File.Exists(scriptPath))
            {
                return;
            }

            System.Diagnostics.ProcessStartInfo startInfo = new()
            {
                FileName = "py",
                Arguments = $"\"{scriptPath}\"",
                WorkingDirectory = projectRoot,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo);
            process?.WaitForExit();
        }

        static Material EnsureRopeMaterial()
        {
            Texture2D ropeBody = AssetDatabase.LoadAssetAtPath<Texture2D>($"{RopeArtFolder}/rope_body_tile.png");
            if (ropeBody == null)
            {
                return null;
            }

            Material existing = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (existing != null)
            {
                existing.mainTexture = ropeBody;
                EditorUtility.SetDirty(existing);
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                return null;
            }

            Directory.CreateDirectory(RopeArtFolder);
            Material material = new Material(shader)
            {
                name = "M_RopeLine",
                mainTexture = ropeBody
            };
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }

        static Sprite ImportRopeSprite(string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                return null;
            }

            float pixelsPerUnit = GetReferencePixelsPerUnit();
            TextureImporter importer = GetSpriteImporter(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        static void ConfigureRopeBodyTexture(string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                return;
            }

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        static Sprite[] ImportAttackFrames()
        {
            ConfigureRopeBodyTexture($"{RopeArtFolder}/rope_body_tile.png");

            if (!Directory.Exists(AttackFramesFolder))
            {
                return Array.Empty<Sprite>();
            }

            float pixelsPerUnit = GetReferencePixelsPerUnit();
            string[] files = Directory.GetFiles(AttackFramesFolder, "*.png");
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            List<Sprite> sprites = new();
            for (int i = 0; i < files.Length; i++)
            {
                string assetPath = files[i].Replace('\\', '/');
                int assetsIndex = assetPath.IndexOf("Assets/", StringComparison.Ordinal);
                if (assetsIndex >= 0)
                {
                    assetPath = assetPath.Substring(assetsIndex);
                }

                TextureImporter importer = GetSpriteImporter(assetPath);
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.spritePivot = new Vector2(0.5f, 0f);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }

            return sprites.ToArray();
        }

        static float GetReferencePixelsPerUnit()
        {
            string idlePath = "Assets/Penumbra/Art/Characters/Player/player_idle_0.png";
            Texture2D idleTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(idlePath);
            if (idleTexture != null && idleTexture.height > 0)
            {
                return idleTexture.height / TargetCharacterHeight;
            }

            return 100f;
        }

        static TextureImporter GetSpriteImporter(string assetPath)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            return importer;
        }

        static void DisableLegacyChainAttack(PenumbraCharacterController2D player)
        {
            ChainAttack2D legacyChain = player.GetComponent<ChainAttack2D>();
            if (legacyChain == null)
            {
                return;
            }

            SerializedObject serializedChain = new SerializedObject(legacyChain);
            serializedChain.FindProperty("readInput").boolValue = false;
            serializedChain.FindProperty("showIdleChain").boolValue = false;
            serializedChain.ApplyModifiedPropertiesWithoutUndo();
            legacyChain.enabled = false;

            Transform legacyVisual = player.transform.Find("Sample Chain Visual");
            if (legacyVisual != null)
            {
                UnityEngine.Object.DestroyImmediate(legacyVisual.gameObject);
            }
        }

        static RopeController2D BuildRopeHierarchy(
            Transform player,
            Material ropeMaterial,
            Sprite ropeTipSprite,
            Sprite ropeHandleSprite)
        {
            Transform visual = player.Find("Wanderer Visual") ?? player;
            Transform handPoint = EnsureChild(visual, "HandPoint");
            handPoint.localPosition = new Vector3(-0.156f, -0.907f, 0f);

            Transform ropeSystem = EnsureChild(player, "RopeSystem");
            RopeController2D ropeController = ropeSystem.GetComponent<RopeController2D>();
            if (ropeController == null)
            {
                ropeController = ropeSystem.gameObject.AddComponent<RopeController2D>();
            }

            RopeWhipAttack2D legacyWhip = ropeSystem.GetComponent<RopeWhipAttack2D>();
            if (legacyWhip != null)
            {
                UnityEngine.Object.DestroyImmediate(legacyWhip);
            }

            LineRenderer ropeLine = EnsureLineRenderer(EnsureChild(ropeSystem, "RopeLine"), ropeMaterial);
            SpriteRenderer ropeTip = EnsureSpriteRenderer(EnsureChild(ropeSystem, "RopeTip"), ropeTipSprite, 15);
            SpriteRenderer ropeHandle = EnsureSpriteRenderer(EnsureChild(handPoint, "RopeHandle"), ropeHandleSprite, 16);
            CircleCollider2D ropeHitbox = EnsureHitbox(EnsureChild(ropeSystem, "RopeHitbox"));
            ropeTip.transform.localScale = Vector3.one * 0.18f;
            ropeHandle.transform.localPosition = Vector3.zero;
            ropeHandle.transform.localRotation = Quaternion.identity;
            ropeHandle.transform.localScale = Vector3.one * 0.16f;

            ropeController.ConfigureReferences(handPoint, ropeLine, ropeTip, ropeHandle, ropeHitbox);
            ropeController.SetFacing(true);

            SerializedObject serializedRope = new SerializedObject(ropeController);
            serializedRope.FindProperty("pointCount").intValue = 32;
            serializedRope.FindProperty("ropeWidth").floatValue = 0.055f;
            serializedRope.FindProperty("maxLength").floatValue = 1.65f;
            serializedRope.FindProperty("swingDuration").floatValue = 0.5f;
            serializedRope.FindProperty("waveAmplitude").floatValue = 0.24f;
            serializedRope.FindProperty("waveCount").floatValue = 2.1f;
            serializedRope.FindProperty("waveSpeed").floatValue = 2.45f;
            serializedRope.FindProperty("tipScale").floatValue = 0.18f;
            serializedRope.FindProperty("handleScale").floatValue = 0.16f;
            serializedRope.FindProperty("hitboxStartTime").floatValue = 0.22f;
            serializedRope.FindProperty("hitboxEndTime").floatValue = 0.31f;
            serializedRope.FindProperty("startupLength").floatValue = 0.06f;
            serializedRope.FindProperty("settleDuration").floatValue = 0.08f;
            serializedRope.FindProperty("showHandleSprite").boolValue = true;
            serializedRope.FindProperty("showTipSprite").boolValue = true;
            serializedRope.FindProperty("tipAnchorOffset").floatValue = 0f;
            serializedRope.FindProperty("handPointLocalRight").vector3Value = new Vector3(-0.156f, -0.907f, 0f);
            serializedRope.ApplyModifiedPropertiesWithoutUndo();

            return ropeController;
        }

        static Transform EnsureChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null)
            {
                return child;
            }

            GameObject childObject = new(name);
            childObject.transform.SetParent(parent, false);
            return childObject.transform;
        }

        static LineRenderer EnsureLineRenderer(Transform target, Material material)
        {
            LineRenderer lineRenderer = target.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = target.gameObject.AddComponent<LineRenderer>();
            }

            lineRenderer.useWorldSpace = true;
            lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.widthMultiplier = 0.055f;
            lineRenderer.numCapVertices = 4;
            lineRenderer.numCornerVertices = 2;
            lineRenderer.sortingLayerName = "VFX";
            lineRenderer.sortingOrder = 14;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.sharedMaterial = material;
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, target.position);
            lineRenderer.SetPosition(1, target.position);
            return lineRenderer;
        }

        static SpriteRenderer EnsureSpriteRenderer(Transform target, Sprite sprite, int sortingOrder)
        {
            SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = target.gameObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = sprite;
            renderer.sortingLayerName = "VFX";
            renderer.sortingOrder = sortingOrder;
            renderer.enabled = false;
            return renderer;
        }

        static CircleCollider2D EnsureHitbox(Transform target)
        {
            CircleCollider2D collider = target.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = target.gameObject.AddComponent<CircleCollider2D>();
            }

            collider.isTrigger = true;
            collider.radius = 0.16f;
            collider.enabled = false;

            RopeHitbox2D hitbox = target.GetComponent<RopeHitbox2D>();
            if (hitbox == null)
            {
                hitbox = target.gameObject.AddComponent<RopeHitbox2D>();
            }

            return collider;
        }

        static void WireAttackSprites(
            PenumbraCharacterController2D player,
            Sprite[] attackFrames,
            RopeController2D ropeController)
        {
            Transform visual = player.transform.Find("Wanderer Visual");
            Transform handPoint = visual != null ? visual.Find("HandPoint") : player.transform.Find("HandPoint");

            SerializedObject serializedPlayer = new SerializedObject(player);
            serializedPlayer.FindProperty("cinderAttackSprites").arraySize = attackFrames.Length;
            for (int i = 0; i < attackFrames.Length; i++)
            {
                serializedPlayer.FindProperty("cinderAttackSprites").GetArrayElementAtIndex(i).objectReferenceValue = attackFrames[i];
            }

            serializedPlayer.FindProperty("cinderAttackFrameRate").floatValue = 12f;
            serializedPlayer.FindProperty("attackPulseDuration").floatValue = attackFrames.Length / 12f;
            serializedPlayer.FindProperty("ropeController").objectReferenceValue = ropeController;
            serializedPlayer.FindProperty("ropeWhipAttack").objectReferenceValue = null;
            if (handPoint != null)
            {
                serializedPlayer.FindProperty("cinderHandPoint").objectReferenceValue = handPoint;
            }

            serializedPlayer.ApplyModifiedPropertiesWithoutUndo();

            player.SetRopeController(ropeController);
        }
    }
}
