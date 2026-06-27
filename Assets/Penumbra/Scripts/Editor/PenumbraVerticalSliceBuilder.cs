using System.IO;
using Penumbra.Art;
using Penumbra.CameraTools;
using Penumbra.Combat;
using Penumbra.Core;
using Penumbra.Data;
using Penumbra.Enemies;
using Penumbra.Player;
using Penumbra.Puzzles;
using Penumbra.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Penumbra.EditorTools
{
    public static class PenumbraVerticalSliceBuilder
    {
        const string PrototypeScenePath = "Assets/Penumbra/Scenes/Prototype_Cave.unity";
        const string SandboxScenePath = "Assets/Penumbra/Scenes/Sandboxes/Sandbox_Movement2D.unity";

        const string PlayerPrefabPath = "Assets/Penumbra/Prefabs/Player/Wanderer_Player.prefab";
        const string CameraPrefabPath = "Assets/Penumbra/Prefabs/Cameras/Camera_Follow2D.prefab";
        const string BlockPrefabPath = "Assets/Penumbra/Prefabs/Environment/Block_Greybox.prefab";
        const string EnemyPrefabPath = "Assets/Penumbra/Prefabs/Enemies/ShadePatroller.prefab";
        const string LightSourcePrefabPath = "Assets/Penumbra/Prefabs/Puzzles/LightSource_Beam.prefab";
        const string MirrorPrefabPath = "Assets/Penumbra/Prefabs/Puzzles/Mirror_Aimable.prefab";
        const string ReceiverPrefabPath = "Assets/Penumbra/Prefabs/Puzzles/MirrorReceiver.prefab";
        const string DoorPrefabPath = "Assets/Penumbra/Prefabs/Puzzles/ReceiverDoor.prefab";

        const string MovementTuningPath = "Assets/Penumbra/Data/Player/PlayerMovementTuning.asset";
        const string WandererConceptSpritePath = "Assets/Penumbra/Art/Characters/Wanderer_Redesign_Concept.png";
        const string WandererSpritePath = "Assets/Penumbra/Art/Characters/Wanderer_Prototype.png";
        const string ShadeSpritePath = "Assets/Penumbra/Art/Characters/ShadePatroller_Prototype.png";
        const string BlockSpritePath = "Assets/Penumbra/Art/Environment/Block_CavePrototype.png";
        const string MirrorSpritePath = "Assets/Penumbra/Art/Environment/Mirror_Prototype.png";
        const string ReceiverSpritePath = "Assets/Penumbra/Art/VFX/Receiver_Prototype.png";

        [MenuItem("Tools/Penumbra/Rebuild Prototype Vertical Slice")]
        public static void BuildAll()
        {
            EnsureFolders();

            Sprite wandererSprite = CreateWandererSprite();
            Sprite shadeSprite = CreateShadeSprite();
            Sprite blockSprite = CreateBlockSprite();
            Sprite mirrorSprite = CreateMirrorSprite();
            Sprite receiverSprite = CreateReceiverSprite();

            CreateMovementTuning();

            GameObject playerPrefab = CreatePlayerPrefab(wandererSprite);
            GameObject cameraPrefab = CreateCameraPrefab();
            GameObject blockPrefab = CreateBlockPrefab(blockSprite);
            GameObject enemyPrefab = CreateEnemyPrefab(shadeSprite);
            GameObject lightSourcePrefab = CreateLightSourcePrefab(receiverSprite);
            GameObject mirrorPrefab = CreateMirrorPrefab(mirrorSprite);
            GameObject receiverPrefab = CreateReceiverPrefab(receiverSprite);
            GameObject doorPrefab = CreateDoorPrefab(blockSprite);

            BuildPrototypeScene(playerPrefab, cameraPrefab, blockPrefab, enemyPrefab, lightSourcePrefab, mirrorPrefab, receiverPrefab, doorPrefab, blockSprite);
            BuildMovementSandbox(playerPrefab, cameraPrefab, blockPrefab, blockSprite);
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(PrototypeScenePath);
        }

        public static void BuildAllFromCommandLine()
        {
            BuildAll();
        }

        static void EnsureFolders()
        {
            string[] folders =
            {
                "Assets/Penumbra/Art/Characters",
                "Assets/Penumbra/Art/Environment",
                "Assets/Penumbra/Art/VFX",
                "Assets/Penumbra/Data/Player",
                "Assets/Penumbra/Data/Enemies",
                "Assets/Penumbra/Data/Puzzles",
                "Assets/Penumbra/Prefabs/Player",
                "Assets/Penumbra/Prefabs/Cameras",
                "Assets/Penumbra/Prefabs/Enemies",
                "Assets/Penumbra/Prefabs/Environment",
                "Assets/Penumbra/Prefabs/Puzzles",
                "Assets/Penumbra/Scenes/Sandboxes"
            };

            for (int i = 0; i < folders.Length; i++)
            {
                Directory.CreateDirectory(folders[i]);
            }
        }

        static void BuildPrototypeScene(GameObject playerPrefab, GameObject cameraPrefab, GameObject blockPrefab, GameObject enemyPrefab, GameObject lightSourcePrefab, GameObject mirrorPrefab, GameObject receiverPrefab, GameObject doorPrefab, Sprite blockSprite)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Prototype_Cave";

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.24f, 0.3f, 1f);

            GameObject root = new("Penumbra Prototype - movement, combat, state, mirror");
            GameObject depthRoot = new("Layered Cave Art");
            GameObject greyboxRoot = new("Greybox Rooms");
            GameObject puzzleRoot = new("Mirror Puzzle Loop");
            depthRoot.transform.SetParent(root.transform);
            greyboxRoot.transform.SetParent(root.transform);
            puzzleRoot.transform.SetParent(root.transform);

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.name = "Wanderer_Player";
            player.transform.position = new Vector3(-6.4f, -0.75f, 0f);
            player.transform.SetParent(root.transform);
            LightShadowStateController playerState = player.GetComponent<LightShadowStateController>();

            GameObject cameraObject = (GameObject)PrefabUtility.InstantiatePrefab(cameraPrefab);
            cameraObject.name = "Main Camera";
            cameraObject.transform.position = new Vector3(-4.6f, 0.8f, -10f);
            cameraObject.GetComponent<FollowCamera2D>().Target = player.transform;
            cameraObject.transform.SetParent(root.transform);

            CreateDirectionalLight(root.transform);
            CreateDepthLayers(blockSprite, cameraObject.transform, depthRoot.transform);
            CreatePrototypeRooms(blockPrefab, greyboxRoot.transform);

            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
            enemy.name = "ShadePatroller_FirstPattern";
            enemy.transform.position = new Vector3(4.2f, -0.85f, 0f);
            enemy.transform.SetParent(greyboxRoot.transform);

            GameObject lightSource = (GameObject)PrefabUtility.InstantiatePrefab(lightSourcePrefab);
            lightSource.name = "LightSource_MirrorPuzzle";
            lightSource.transform.position = new Vector3(12.4f, -0.65f, 0f);
            lightSource.transform.rotation = Quaternion.identity;
            lightSource.GetComponent<LightBeamEmitter2D>().SetStateSource(playerState);
            lightSource.transform.SetParent(puzzleRoot.transform);

            GameObject mirror = (GameObject)PrefabUtility.InstantiatePrefab(mirrorPrefab);
            mirror.name = "AimableMirror_RHold_ADWS";
            mirror.transform.position = new Vector3(16.25f, -0.65f, 0f);
            mirror.transform.rotation = Quaternion.Euler(0f, 0f, -135f);
            mirror.transform.SetParent(puzzleRoot.transform);

            GameObject door = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
            door.name = "ReceiverDoor_ExitGate";
            door.transform.position = new Vector3(22.2f, 0.1f, 0f);
            door.GetComponent<LevelBlock2D>().ConfigureBlock(new Vector2(0.65f, 4.2f), new Color(0.3f, 0.38f, 0.55f, 1f), true, false, "Environment", 5, blockSprite);
            door.transform.SetParent(puzzleRoot.transform);

            GameObject receiver = (GameObject)PrefabUtility.InstantiatePrefab(receiverPrefab);
            receiver.name = "MirrorReceiver_OpensExit";
            receiver.transform.position = new Vector3(16.25f, 2.4f, 0f);
            receiver.GetComponent<LightReceiver2D>().SetLinkedDoor(door.GetComponent<ReflectiveDoor2D>());
            receiver.transform.SetParent(puzzleRoot.transform);

            CreateBlockInstance(blockPrefab, "Exit Ledge", new Vector2(25.5f, -0.1f), new Vector2(5.2f, 0.45f), new Color(0.28f, 0.32f, 0.4f, 1f), true, false, "Environment", 1, blockSprite, greyboxRoot.transform);
            CreateMarker("Scene Goal - chain the shade with J, toggle P, hold R and aim mirror to open the exit door", new Vector3(18.8f, 3.3f, 0f), root.transform);

            EditorSceneManager.SaveScene(scene, PrototypeScenePath);
        }

        static void BuildMovementSandbox(GameObject playerPrefab, GameObject cameraPrefab, GameObject blockPrefab, Sprite blockSprite)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Sandbox_Movement2D";

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.45f, 1f);

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.name = "Wanderer_MovementSandbox";
            player.transform.position = new Vector3(-4.75f, -0.55f, 0f);

            GameObject cameraObject = (GameObject)PrefabUtility.InstantiatePrefab(cameraPrefab);
            cameraObject.name = "Main Camera";
            cameraObject.transform.position = new Vector3(0f, 0.8f, -10f);
            cameraObject.GetComponent<FollowCamera2D>().Target = player.transform;

            CreateDirectionalLight(null);
            CreateDepthLayers(blockSprite, cameraObject.transform, null);

            CreateBlockInstance(blockPrefab, "Ground", new Vector2(0f, -2f), new Vector2(16f, 1f), new Color(0.24f, 0.27f, 0.33f, 1f), true, false, "Environment", 0, blockSprite, null);
            CreateBlockInstance(blockPrefab, "Left Wall", new Vector2(-7.75f, 0.15f), new Vector2(0.5f, 4.3f), new Color(0.2f, 0.24f, 0.3f, 1f), true, false, "Environment", 0, blockSprite, null);
            CreateBlockInstance(blockPrefab, "Right Wall", new Vector2(7.75f, 0.15f), new Vector2(0.5f, 4.3f), new Color(0.2f, 0.24f, 0.3f, 1f), true, false, "Environment", 0, blockSprite, null);
            CreateBlockInstance(blockPrefab, "Jump Test Platform", new Vector2(-2.4f, 0.15f), new Vector2(3.1f, 0.35f), new Color(0.32f, 0.36f, 0.44f, 1f), true, false, "Environment", 2, blockSprite, null);
            CreateBlockInstance(blockPrefab, "Dash Test Platform", new Vector2(3.15f, 1.4f), new Vector2(2.6f, 0.35f), new Color(0.32f, 0.36f, 0.44f, 1f), true, false, "Environment", 2, blockSprite, null);
            CreateBlockInstance(blockPrefab, "Hit Motion Test Pad", new Vector2(5.7f, -1.15f), new Vector2(0.85f, 1.65f), new Color(1f, 0.22f, 0.18f, 0.72f), true, true, "VFX", 10, blockSprite, null).AddComponent<DamageVolume2D>();
            CreateMarker("Controls - A/D move, Space double jump, Shift dash, J chain, P light/shadow, R aim mirrors", new Vector3(0f, 3.4f, 0f), null);

            EditorSceneManager.SaveScene(scene, SandboxScenePath);
        }

        static void CreatePrototypeRooms(GameObject blockPrefab, Transform parent)
        {
            Sprite blockSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BlockSpritePath);

            CreateBlockInstance(blockPrefab, "Floor_IntroCombatPuzzle", new Vector2(8f, -2f), new Vector2(34f, 1f), new Color(0.23f, 0.26f, 0.32f, 1f), true, false, "Environment", 0, blockSprite, parent);
            CreateBlockInstance(blockPrefab, "Left Cave Wall", new Vector2(-8.75f, 0.2f), new Vector2(0.5f, 4.6f), new Color(0.16f, 0.19f, 0.25f, 1f), true, false, "Environment", 0, blockSprite, parent);
            CreateBlockInstance(blockPrefab, "Combat Ledge", new Vector2(2.3f, 0.0f), new Vector2(3.5f, 0.35f), new Color(0.31f, 0.35f, 0.43f, 1f), true, false, "Environment", 2, blockSprite, parent);
            CreateBlockInstance(blockPrefab, "Dash Ledge", new Vector2(7.0f, 1.05f), new Vector2(2.6f, 0.35f), new Color(0.31f, 0.35f, 0.43f, 1f), true, false, "Environment", 2, blockSprite, parent);
            CreateBlockInstance(blockPrefab, "Puzzle Floor Marker", new Vector2(15.9f, -1.48f), new Vector2(8f, 0.08f), new Color(0.75f, 0.66f, 0.32f, 1f), false, false, "VFX", 4, blockSprite, parent);
            CreateBlockInstance(blockPrefab, "Foreground Mask Left", new Vector2(-5.8f, -2.55f), new Vector2(4.2f, 0.8f), new Color(0.09f, 0.1f, 0.14f, 1f), false, false, "Foreground", 25, blockSprite, parent);
            CreateBlockInstance(blockPrefab, "Foreground Mask Right", new Vector2(20.4f, -2.58f), new Vector2(6.5f, 0.85f), new Color(0.09f, 0.1f, 0.14f, 1f), false, false, "Foreground", 25, blockSprite, parent);
        }

        static void CreateDepthLayers(Sprite blockSprite, Transform camera, Transform parent)
        {
            GameObject far = CreateBlockInstance(null, "Far Cave Silhouette", new Vector3(6f, 1.35f, 6f), new Vector2(42f, 5.2f), new Color(0.045f, 0.055f, 0.085f, 1f), false, false, "Background Far", -60, blockSprite, parent);
            far.AddComponent<ParallaxLayer2D>().Configure(camera, 0.12f, 0.04f);

            GameObject mid = CreateBlockInstance(null, "Mid Cave Mass", new Vector3(5.2f, 0.75f, 4f), new Vector2(31f, 3.4f), new Color(0.075f, 0.095f, 0.14f, 1f), false, false, "Background Mid", -40, blockSprite, parent);
            mid.AddComponent<ParallaxLayer2D>().Configure(camera, 0.3f, 0.08f);

            GameObject near = CreateBlockInstance(null, "Near Stalactite Band", new Vector3(7f, 3.35f, 2.5f), new Vector2(28f, 0.75f), new Color(0.12f, 0.14f, 0.19f, 1f), false, false, "Foreground", 30, blockSprite, parent);
            near.AddComponent<ParallaxLayer2D>().Configure(camera, 0.82f, 0.18f);
        }

        static GameObject CreateBlockInstance(GameObject blockPrefab, string name, Vector2 position, Vector2 size, Color color, bool collidable, bool trigger, string sortingLayer, int sortingOrder, Sprite blockSprite, Transform parent)
        {
            return CreateBlockInstance(blockPrefab, name, new Vector3(position.x, position.y, 0f), size, color, collidable, trigger, sortingLayer, sortingOrder, blockSprite, parent);
        }

        static GameObject CreateBlockInstance(GameObject blockPrefab, string name, Vector3 position, Vector2 size, Color color, bool collidable, bool trigger, string sortingLayer, int sortingOrder, Sprite blockSprite, Transform parent)
        {
            GameObject blockObject = blockPrefab != null ? (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab) : new GameObject(name);
            blockObject.name = name;
            blockObject.transform.position = position;
            blockObject.transform.SetParent(parent);

            LevelBlock2D block = blockObject.GetComponent<LevelBlock2D>();
            if (block == null)
            {
                block = blockObject.AddComponent<LevelBlock2D>();
            }

            block.ConfigureBlock(size, color, collidable, trigger, sortingLayer, sortingOrder, blockSprite);
            return blockObject;
        }

        static void CreateDirectionalLight(Transform parent)
        {
            GameObject lightObject = new("Directional Light");
            lightObject.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
            lightObject.transform.SetParent(parent);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = Color.white;
        }

        static void CreateMarker(string name, Vector3 position, Transform parent)
        {
            GameObject marker = new(name);
            marker.transform.position = position;
            marker.transform.SetParent(parent);
        }

        static GameObject CreatePlayerPrefab(Sprite sprite)
        {
            GameObject player = new("Wanderer_Player");

            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 4.4f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CapsuleCollider2D capsule = player.AddComponent<CapsuleCollider2D>();
            capsule.direction = CapsuleDirection2D.Vertical;
            capsule.size = new Vector2(0.72f, 1.68f);

            PenumbraCharacterController2D controller = player.AddComponent<PenumbraCharacterController2D>();
            controller.SetBodySprite(sprite);
            player.AddComponent<LightShadowStateController>();
            player.AddComponent<ChainAttack2D>();

            return SavePrefab(player, PlayerPrefabPath);
        }

        static GameObject CreateCameraPrefab()
        {
            GameObject cameraObject = new("Camera_Follow2D");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0.8f, -10f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.08f, 0.11f, 1f);

            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraObject.AddComponent<FollowCamera2D>();

            return SavePrefab(cameraObject, CameraPrefabPath);
        }

        static GameObject CreateBlockPrefab(Sprite blockSprite)
        {
            GameObject blockObject = new("Block_Greybox");
            LevelBlock2D block = blockObject.AddComponent<LevelBlock2D>();
            block.ConfigureBlock(Vector2.one, new Color(0.25f, 0.29f, 0.35f, 1f), true, false, "Environment", 0, blockSprite);
            return SavePrefab(blockObject, BlockPrefabPath);
        }

        static GameObject CreateEnemyPrefab(Sprite shadeSprite)
        {
            GameObject enemy = new("ShadePatroller");
            enemy.AddComponent<Rigidbody2D>();

            CapsuleCollider2D collider = enemy.AddComponent<CapsuleCollider2D>();
            collider.direction = CapsuleDirection2D.Vertical;
            collider.size = new Vector2(0.9f, 1.35f);
            collider.isTrigger = true;

            SpriteRenderer renderer = enemy.AddComponent<SpriteRenderer>();
            renderer.sprite = shadeSprite;
            renderer.color = new Color(0.55f, 0.48f, 0.8f, 1f);
            renderer.sortingLayerName = "Gameplay";
            renderer.sortingOrder = 5;

            enemy.AddComponent<Damageable2D>();
            enemy.AddComponent<ShadePatroller2D>();
            return SavePrefab(enemy, EnemyPrefabPath);
        }

        static GameObject CreateLightSourcePrefab(Sprite receiverSprite)
        {
            GameObject source = new("LightSource_Beam");
            source.AddComponent<LineRenderer>();
            source.AddComponent<LightBeamEmitter2D>();

            GameObject visual = new("Source Visual");
            visual.transform.SetParent(source.transform, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = receiverSprite;
            renderer.color = new Color(1f, 0.85f, 0.35f, 1f);
            renderer.sortingLayerName = "VFX";
            renderer.sortingOrder = 12;

            return SavePrefab(source, LightSourcePrefabPath);
        }

        static GameObject CreateMirrorPrefab(Sprite mirrorSprite)
        {
            GameObject mirror = new("Mirror_Aimable");
            BoxCollider2D collider = mirror.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.25f, 1.35f);

            SpriteRenderer renderer = mirror.AddComponent<SpriteRenderer>();
            renderer.sprite = mirrorSprite;
            renderer.color = new Color(0.72f, 0.9f, 1f, 1f);
            renderer.sortingLayerName = "Environment";
            renderer.sortingOrder = 8;

            mirror.AddComponent<MirrorReflector2D>();
            mirror.AddComponent<MirrorAimController2D>().Configure(mirror.transform, true);
            return SavePrefab(mirror, MirrorPrefabPath);
        }

        static GameObject CreateReceiverPrefab(Sprite receiverSprite)
        {
            GameObject receiver = new("MirrorReceiver");
            CircleCollider2D collider = receiver.AddComponent<CircleCollider2D>();
            collider.radius = 0.38f;
            collider.isTrigger = true;

            SpriteRenderer renderer = receiver.AddComponent<SpriteRenderer>();
            renderer.sprite = receiverSprite;
            renderer.color = new Color(0.4f, 0.38f, 0.48f, 1f);
            renderer.sortingLayerName = "VFX";
            renderer.sortingOrder = 11;

            receiver.AddComponent<LightReceiver2D>();
            return SavePrefab(receiver, ReceiverPrefabPath);
        }

        static GameObject CreateDoorPrefab(Sprite blockSprite)
        {
            GameObject door = new("ReceiverDoor");
            LevelBlock2D block = door.AddComponent<LevelBlock2D>();
            block.ConfigureBlock(new Vector2(0.65f, 3.2f), new Color(0.3f, 0.38f, 0.55f, 1f), true, false, "Environment", 5, blockSprite);
            door.AddComponent<ReflectiveDoor2D>();
            return SavePrefab(door, DoorPrefabPath);
        }

        static GameObject SavePrefab(GameObject source, string path)
        {
            AssetDatabase.DeleteAsset(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
            Object.DestroyImmediate(source);
            return prefab;
        }

        static void CreateMovementTuning()
        {
            AssetDatabase.DeleteAsset(MovementTuningPath);
            Directory.CreateDirectory(Path.GetDirectoryName(MovementTuningPath));

            PlayerMovementTuning tuning = ScriptableObject.CreateInstance<PlayerMovementTuning>();
            tuning.moveSpeed = 6f;
            tuning.acceleration = 80f;
            tuning.airAcceleration = 42f;
            tuning.jumpVelocity = 10.5f;
            tuning.extraAirJumps = 1;
            tuning.dashSpeed = 16f;
            tuning.dashDuration = 0.14f;
            tuning.dashCooldown = 0.45f;

            AssetDatabase.CreateAsset(tuning, MovementTuningPath);
        }

        static void UpdateBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(PrototypeScenePath, true)
            };
        }

        static Sprite CreateWandererSprite()
        {
            Sprite conceptSprite = AssetDatabase.LoadAssetAtPath<Sprite>(WandererConceptSpritePath);
            if (conceptSprite != null)
            {
                return conceptSprite;
            }

            Texture2D texture = PrototypeWandererSpriteFactory.CreateTexture("Wanderer Prototype Redesign");
            return WriteSpriteAsset(texture, WandererSpritePath, PrototypeWandererSpriteFactory.PixelsPerUnit, Vector4.zero);
        }

        static Sprite CreateShadeSprite()
        {
            const int size = 96;
            Texture2D texture = CreateTransparentTexture(size, size);
            Vector2 center = new(size * 0.5f, size * 0.42f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new(x + 0.5f, y + 0.5f);
                    float distance = Vector2.Distance(point, center);
                    if (distance < size * 0.36f)
                    {
                        float alpha = Mathf.InverseLerp(size * 0.36f, size * 0.12f, distance);
                        texture.SetPixel(x, y, new Color(0.42f, 0.33f, 0.76f, Mathf.Clamp01(alpha)));
                    }
                }
            }

            return WriteSpriteAsset(texture, ShadeSpritePath, 64f, Vector4.zero);
        }

        static Sprite CreateBlockSprite()
        {
            const int size = 32;
            Texture2D texture = CreateTransparentTexture(size, size);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool edge = x < 3 || x >= size - 3 || y < 3 || y >= size - 3;
                    texture.SetPixel(x, y, edge ? new Color(0.72f, 0.78f, 0.86f, 1f) : Color.white);
                }
            }

            return WriteSpriteAsset(texture, BlockSpritePath, 32f, new Vector4(4f, 4f, 4f, 4f));
        }

        static Sprite CreateMirrorSprite()
        {
            const int width = 32;
            const int height = 128;
            Texture2D texture = CreateTransparentTexture(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inside = x > 8 && x < 23;
                    bool edge = x == 9 || x == 22 || y < 4 || y >= height - 4;
                    texture.SetPixel(x, y, inside ? edge ? new Color(0.88f, 0.96f, 1f, 1f) : Color.white : Color.clear);
                }
            }

            return WriteSpriteAsset(texture, MirrorSpritePath, 64f, Vector4.zero);
        }

        static Sprite CreateReceiverSprite()
        {
            const int size = 64;
            Texture2D texture = CreateTransparentTexture(size, size);
            Vector2 center = new(size * 0.5f, size * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    bool ring = distance > size * 0.28f && distance < size * 0.4f;
                    bool core = distance < size * 0.12f;
                    texture.SetPixel(x, y, ring || core ? Color.white : Color.clear);
                }
            }

            return WriteSpriteAsset(texture, ReceiverSpritePath, 64f, Vector4.zero);
        }

        static Texture2D CreateTransparentTexture(int width, int height)
        {
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear
            };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            return texture;
        }

        static bool IsInCapsule(Vector2 point, Vector2 top, Vector2 bottom, float radius, int width)
        {
            bool inMiddle = point.y >= bottom.y && point.y <= top.y && Mathf.Abs(point.x - width * 0.5f) <= radius;
            bool inCaps = Vector2.Distance(point, top) <= radius || Vector2.Distance(point, bottom) <= radius;
            return inMiddle || inCaps;
        }

        static Sprite WriteSpriteAsset(Texture2D texture, string path, float pixelsPerUnit, Vector4 border)
        {
            texture.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.spriteBorder = border;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
