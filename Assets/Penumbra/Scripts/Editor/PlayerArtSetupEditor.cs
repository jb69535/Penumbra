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
        const string SandboxScenePath = "Assets/Penumbra/Scenes/Sandboxes/Sandbox_Movement2D.unity";
        const string RunFramesFolder = "Assets/Penumbra/Art/Characters/Player/Frames/Run";
        const string JumpFramesFolder = "Assets/Penumbra/Art/Characters/Player/Frames/Jump";
        const string IdleFramesFolder = "Assets/Penumbra/Art/Characters/Player/Frames/Idle";
        const string SitFramesFolder = "Assets/Penumbra/Art/Characters/Player/Frames/Sit";
        const string DashFramesFolder = "Assets/Penumbra/Art/Characters/Player/Frames/Dash";
        const string SlideFramesFolder = "Assets/Penumbra/Art/Characters/Player/Frames/Slide";
        const string GameArtSourceFolder = @"C:\Users\FLEXBOY\Downloads\game";
        const float TargetCharacterHeight = 1.8f;

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
            RunGameSpriteImport();
            AssetDatabase.Refresh();

            float pixelsPerUnit = TargetCharacterHeight > 0f ? 100f : 100f;
            Sprite idle = ImportSingleSprite($"{PlayerArtFolder}/player_idle_0.png", ref pixelsPerUnit);
            Sprite[] idleFrames = ImportFrameFolder(IdleFramesFolder, pixelsPerUnit);
            Sprite[] runFrames = ImportFrameFolder(RunFramesFolder, pixelsPerUnit);
            Sprite[] jumpFrames = ImportFrameFolder(JumpFramesFolder, pixelsPerUnit);
            Sprite[] sitFrames = ImportFrameFolder(SitFramesFolder, pixelsPerUnit);
            Sprite[] dashFrames = ImportFrameFolder(DashFramesFolder, pixelsPerUnit);
            Sprite[] slideFrames = ImportFrameFolder(SlideFramesFolder, pixelsPerUnit);
            Sprite frontIdle = ImportSingleSprite($"{PlayerArtFolder}/player_idle_0.png", pixelsPerUnit);
            Sprite sideLeft = ImportSingleSprite($"{PlayerArtFolder}/player_left_0.png", pixelsPerUnit);
            Sprite sideRight = ImportSingleSprite($"{PlayerArtFolder}/player_right_0.png", pixelsPerUnit);
            Sprite sitIdle = ImportSingleSprite($"{PlayerArtFolder}/player_sit_0.png", pixelsPerUnit);
            if (sideRight == null)
            {
                sideRight = dashFrames.Length > 0 ? dashFrames[0] : null;
            }

            if (idle == null)
            {
                if (showDialogs)
                {
                    EditorUtility.DisplayDialog("Penumbra", "player_idle_0.png sprite import failed.", "OK");
                }

                return;
            }

            if (runFrames.Length == 0 || jumpFrames.Length == 0 || sitFrames.Length == 0 || dashFrames.Length == 0 || slideFrames.Length == 0)
            {
                if (showDialogs)
                {
                    EditorUtility.DisplayDialog(
                        "Penumbra",
                        "Run/Jump/Sit/Dash/Slide frame PNGs are missing.\n\nRun Tools/import_game_sprites.py or use this menu again.",
                        "OK");
                }

                return;
            }

            GameObject wanderer = ConfigureWandererInSandbox(
                idleFrames,
                runFrames,
                jumpFrames,
                sitFrames,
                dashFrames,
                slideFrames,
                frontIdle,
                sideLeft,
                sideRight,
                sitIdle);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (wanderer != null)
            {
                Selection.activeObject = wanderer;
            }

            if (showDialogs)
            {
                EditorUtility.DisplayDialog(
                    "Penumbra Player Setup",
                    "Cinder Wisp sprites are wired to the git-base Wanderer controller.\n\n" +
                    "- Movement: PenumbraCharacterController2D (Shift dash, Shift+Down+dir slide, J attack)\n" +
                    "- Sprites: idle/run/jump/sit/dash/slide from Downloads/game\n" +
                    "- Down/S/C: sit idle; Down+Left/Right: crouch walk\n" +
                    "- Scene: Sandbox_Movement2D\n\n" +
                    "Play with the Wanderer_MovementSandbox object.",
                    "OK");
            }
        }

        static void RunGameSpriteImport()
        {
            if (!Directory.Exists(GameArtSourceFolder))
            {
                return;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string scriptPath = Path.Combine(projectRoot, "Tools", "import_game_sprites.py");
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

        static Sprite ImportFrameSprite(string assetPath, float pixelsPerUnit)
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

        static TextureImporter GetSpriteImporter(string assetPath)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            return importer;
        }

        static GameObject ConfigureWandererInSandbox(
            Sprite[] idleFrames,
            Sprite[] runFrames,
            Sprite[] jumpFrames,
            Sprite[] sitFrames,
            Sprite[] dashFrames,
            Sprite[] slideFrames,
            Sprite frontIdle,
            Sprite sideLeft,
            Sprite sideRight,
            Sprite sitIdle)
        {
            if (!File.Exists(SandboxScenePath))
            {
                return null;
            }

            Scene scene = EditorSceneManager.OpenScene(SandboxScenePath, OpenSceneMode.Single);
            float floorTopY = FindFloorTopY(scene);
            const float wandererColliderHeight = 1.68f;
            float playerCenterY = floorTopY + wandererColliderHeight * 0.5f;

            PenumbraCharacterController2D wanderer = UnityEngine.Object.FindFirstObjectByType<PenumbraCharacterController2D>(FindObjectsInactive.Include);
            if (wanderer == null)
            {
                EditorUtility.DisplayDialog(
                    "Penumbra",
                    "Wanderer_MovementSandbox was not found in the sandbox scene.",
                    "OK");
                return null;
            }

            wanderer.gameObject.SetActive(true);
            wanderer.transform.position = new Vector3(-4.75f, playerCenterY, 0f);
            wanderer.ConfigureCinderWispSprites(
                idleFrames,
                runFrames,
                jumpFrames,
                sitFrames,
                dashFrames,
                slideFrames,
                frontIdle,
                sideLeft,
                sideRight,
                sitIdle);

            SerializedObject serializedWanderer = new SerializedObject(wanderer);
            serializedWanderer.FindProperty("moveSpeed").floatValue = 7.5f;
            serializedWanderer.FindProperty("jumpVelocity").floatValue = 13.5f;
            serializedWanderer.FindProperty("groundDeceleration").floatValue = 95f;
            serializedWanderer.FindProperty("airAcceleration").floatValue = 55f;
            serializedWanderer.FindProperty("airDeceleration").floatValue = 48f;
            serializedWanderer.FindProperty("coyoteTime").floatValue = 0.1f;
            serializedWanderer.FindProperty("jumpBufferTime").floatValue = 0.12f;
            serializedWanderer.FindProperty("jumpCutMultiplier").floatValue = 0.45f;
            serializedWanderer.FindProperty("fallGravityMultiplier").floatValue = 1.7f;
            serializedWanderer.FindProperty("maxFallSpeed").floatValue = -22f;
            serializedWanderer.ApplyModifiedPropertiesWithoutUndo();

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                FollowCamera2D follow = mainCamera.GetComponent<FollowCamera2D>();
                if (follow != null)
                {
                    follow.Target = wanderer.transform;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return wanderer.gameObject;
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
