using System;
using System.Collections.Generic;
using System.IO;
using Penumbra.CameraTools;
using Penumbra.Player;
using Penumbra.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Penumbra.EditorTools
{
    public static class PlayerArtSetupEditor
    {
        const string PlayerArtFolder = "Assets/Penumbra/Art/Characters/Player";
        const string PlayerPrefabPath = "Assets/Penumbra/Prefabs/Player/CinderWisp_Player.prefab";
        const string SandboxScenePath = "Assets/Penumbra/Scenes/Sandboxes/Sandbox_Movement2D.unity";
        const string RunFramesFolder = "Assets/Penumbra/Art/Characters/Player/Frames/Run";
        const string JumpFramesFolder = "Assets/Penumbra/Art/Characters/Player/Frames/Jump";
        const float TargetCharacterHeight = 1.8f;

        static readonly string[] CharacterSheetFiles =
        {
            "character_sheet_0.png",
            "character_sheet_1.png",
            "character_sheet_2.png"
        };

        [MenuItem("Tools/Penumbra/Open Movement Sandbox Scene")]
        public static void OpenMovementSandboxScene()
        {
            if (!File.Exists(SandboxScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Penumbra",
                    $"Scene file missing:\n{SandboxScenePath}\n\nCheck that OneDrive finished syncing the project.",
                    "OK");
                return;
            }

            EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(SandboxScenePath);
        }

        [MenuItem("Tools/Penumbra/Assign Player Run Jump Sprites")]
        public static void AssignPlayerRunJumpSprites()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string scriptPath = Path.Combine(projectRoot, "Tools", "assign_player_sprites.py");
            if (!File.Exists(scriptPath))
            {
                EditorUtility.DisplayDialog("Penumbra", "assign_player_sprites.py not found in Tools folder.", "OK");
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

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Penumbra",
                "Run/Jump sprite arrays assigned on CinderWisp_Player prefab.\n\nIf Player is in the scene, it inherits from the prefab automatically.",
                "OK");
        }

        [MenuItem("Tools/Penumbra/Setup Cinder Wisp Player (Fix Everything)")]
        public static void SetupCinderWispPlayer()
        {
            SetupCinderWispPlayerInternal(showDialogs: !Application.isBatchMode);
        }

        public static void SetupCinderWispPlayerFromCommandLine()
        {
            SetupCinderWispPlayerInternal(showDialogs: false);
        }

        static void SetupCinderWispPlayerInternal(bool showDialogs)
        {
            if (!Directory.Exists(PlayerArtFolder))
            {
                if (showDialogs)
                {
                    EditorUtility.DisplayDialog(
                        "Penumbra Player Setup",
                        $"Art folder not found:\n{PlayerArtFolder}\n\nCopy PNG files into that folder first.",
                        "OK");
                }

                return;
            }

            Directory.CreateDirectory("Assets/Penumbra/Prefabs/Player");
            RunPythonSpriteProcessing();
            AssetDatabase.Refresh();

            float pixelsPerUnit = TargetCharacterHeight > 0f ? 100f : 100f;
            Sprite idle = ImportSingleSprite($"{PlayerArtFolder}/player_idle_0.png", ref pixelsPerUnit);
            Sprite dash = ImportSingleSprite($"{PlayerArtFolder}/player_dash_0.png", pixelsPerUnit);
            Sprite[] runFrames = ImportFrameFolder(RunFramesFolder, pixelsPerUnit);
            Sprite[] jumpFrames = ImportFrameFolder(JumpFramesFolder, pixelsPerUnit);

            if (idle == null)
            {
                if (showDialogs)
                {
                    EditorUtility.DisplayDialog("Penumbra", "player_idle_0.png sprite import failed.", "OK");
                }

                return;
            }

            if (runFrames.Length == 0 || jumpFrames.Length == 0)
            {
                if (showDialogs)
                {
                    EditorUtility.DisplayDialog(
                        "Penumbra",
                        "Run/Jump frame PNGs are missing.\n\nRun Tools/process_sprite_sheets.py or use this menu again.",
                        "OK");
                }

                return;
            }

            for (int i = 0; i < CharacterSheetFiles.Length; i++)
            {
                string assetPath = $"{PlayerArtFolder}/{CharacterSheetFiles[i]}";
                if (!File.Exists(assetPath))
                {
                    continue;
                }

                PrepareCharacterSheetForManualSlice(assetPath);
            }

            GameObject prefab = CreatePlayerPrefab(idle, runFrames, jumpFrames, dash);
            PlacePlayerInSandbox(prefab, idle, runFrames, jumpFrames, dash);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = prefab;

            if (showDialogs)
            {
                EditorUtility.DisplayDialog(
                    "Penumbra Player Setup",
                    "Cinder Wisp player is ready.\n\n" +
                    "- White backgrounds removed (transparent PNG).\n" +
                    "- Run/Jump sheets sliced into 6 frames each.\n" +
                    "- Prefab: CinderWisp_Player\n" +
                    "- Scene: Sandbox_Movement2D\n\n" +
                    "Play and use A/D + Space.",
                    "OK");
            }
        }

        static void RunPythonSpriteProcessing()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string scriptPath = Path.Combine(projectRoot, "Tools", "process_sprite_sheets.py");
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

        static Sprite ImportSingleSprite(string assetPath, ref float pixelsPerUnit)
        {
            if (!File.Exists(assetPath))
            {
                return null;
            }

            TextureImporter importer = GetSpriteImporter(assetPath);
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture == null)
            {
                return null;
            }

            if (texture.height > 0)
            {
                pixelsPerUnit = Mathf.Max(1f, texture.height / TargetCharacterHeight);
            }

            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.spritePivot = new Vector2(0.5f, 0f);
            importer.isReadable = false;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        static Sprite ImportSingleSprite(string assetPath, float pixelsPerUnit)
        {
            if (!File.Exists(assetPath))
            {
                return null;
            }

            TextureImporter importer = GetSpriteImporter(assetPath);
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = Mathf.Max(1f, pixelsPerUnit);
            importer.spritePivot = new Vector2(0.5f, 0f);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.isReadable = false;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        static Sprite[] ImportFrameFolder(string folderPath, float pixelsPerUnit)
        {
            if (!Directory.Exists(folderPath))
            {
                return new Sprite[0];
            }

            string[] files = Directory.GetFiles(folderPath, "*.png");
            System.Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            List<Sprite> sprites = new();
            for (int i = 0; i < files.Length; i++)
            {
                string assetPath = files[i].Replace('\\', '/');
                int assetsIndex = assetPath.IndexOf("Assets/", StringComparison.Ordinal);
                if (assetsIndex >= 0)
                {
                    assetPath = assetPath.Substring(assetsIndex);
                }

                Sprite sprite = ImportFrameSprite(assetPath, pixelsPerUnit);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }

            return sprites.ToArray();
        }

        static Sprite ImportFrameSprite(string assetPath, float fallbackPixelsPerUnit)
        {
            if (!File.Exists(assetPath))
            {
                return null;
            }

            TextureImporter importer = GetSpriteImporter(assetPath);
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePivot = new Vector2(0.5f, 0f);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.isReadable = true;
            importer.SaveAndReimport();

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            float framePixelsPerUnit = fallbackPixelsPerUnit;
            if (texture != null && texture.height > 0)
            {
                framePixelsPerUnit = texture.height / TargetCharacterHeight;
            }

            importer.spritePixelsPerUnit = Mathf.Max(1f, framePixelsPerUnit);
            importer.isReadable = false;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        static void PrepareCharacterSheetForManualSlice(string assetPath)
        {
            TextureImporter importer = GetSpriteImporter(assetPath);
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.isReadable = false;
            importer.SaveAndReimport();
        }

        static TextureImporter GetSpriteImporter(string assetPath)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            return importer;
        }

        static GameObject CreatePlayerPrefab(Sprite idle, Sprite[] runFrames, Sprite[] jumpFrames, Sprite dash)
        {
            GameObject player = new("CinderWisp_Player");

            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 3.2f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CapsuleCollider2D capsule = player.AddComponent<CapsuleCollider2D>();
            capsule.direction = CapsuleDirection2D.Vertical;
            capsule.size = new Vector2(0.85f, 1.8f);

            PlayerController controller = player.AddComponent<PlayerController>();
            controller.ConfigureSprites(idle, runFrames, jumpFrames, dash);

            if (File.Exists(PlayerPrefabPath))
            {
                AssetDatabase.DeleteAsset(PlayerPrefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            UnityEngine.Object.DestroyImmediate(player);

            return AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        }

        static void PlacePlayerInSandbox(
            GameObject playerPrefab,
            Sprite idle,
            Sprite[] runFrames,
            Sprite[] jumpFrames,
            Sprite dash)
        {
            if (!File.Exists(SandboxScenePath))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);
            float floorTopY = FindFloorTopY(scene);
            float playerCenterY = floorTopY + TargetCharacterHeight * 0.5f;

            PlayerController existing = UnityEngine.Object.FindFirstObjectByType<PlayerController>();

            foreach (PenumbraCharacterController2D legacy in UnityEngine.Object.FindObjectsByType<PenumbraCharacterController2D>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                legacy.gameObject.SetActive(false);
            }

            GameObject playerObject;
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            playerObject = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerObject.name = "Player";

            playerObject.transform.position = new Vector3(-4.75f, playerCenterY, 0f);

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                FollowCamera2D follow = mainCamera.GetComponent<FollowCamera2D>();
                if (follow != null)
                {
                    follow.Target = playerObject.transform;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static float FindFloorTopY(Scene scene)
        {
            float bestTop = -1.5f;

            foreach (LevelBlock2D block in UnityEngine.Object.FindObjectsByType<LevelBlock2D>())
            {
                if (!block.name.Contains("Floor") && !block.name.Contains("Ground"))
                {
                    continue;
                }

                BoxCollider2D collider = block.GetComponent<BoxCollider2D>();
                if (collider == null || !collider.enabled)
                {
                    continue;
                }

                float top = block.transform.position.y + collider.offset.y + collider.size.y * 0.5f;
                if (top > bestTop)
                {
                    bestTop = top;
                }
            }

            return bestTop;
        }
    }
}
