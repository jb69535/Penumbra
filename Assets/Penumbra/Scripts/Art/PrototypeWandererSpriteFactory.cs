using UnityEngine;

namespace Penumbra.Art
{
    public static class PrototypeWandererSpriteFactory
    {
        public enum WandererPose
        {
            Idle,
            Move,
            Jump,
            Fall,
            Attack
        }

        public const int Width = 96;
        public const int Height = 192;
        public const float PixelsPerUnit = 96f;

        static readonly Color Clear = new(1f, 1f, 1f, 0f);
        static readonly Color Outline = new(0.08f, 0.09f, 0.1f, 1f);
        static readonly Color DeepShadow = new(0.04f, 0.05f, 0.06f, 1f);
        static readonly Color Hood = new(0.26f, 0.45f, 0.36f, 1f);
        static readonly Color HoodLight = new(0.42f, 0.62f, 0.42f, 1f);
        static readonly Color Cloth = new(0.18f, 0.35f, 0.31f, 1f);
        static readonly Color Trim = new(0.72f, 0.58f, 0.37f, 1f);
        static readonly Color Face = new(0.9f, 0.82f, 0.66f, 1f);
        static readonly Color Scarf = new(0.72f, 0.65f, 0.5f, 1f);
        static readonly Color Leather = new(0.42f, 0.25f, 0.13f, 1f);
        static readonly Color Boot = new(0.5f, 0.33f, 0.18f, 1f);
        static readonly Color Metal = new(0.64f, 0.66f, 0.62f, 1f);
        static readonly Color MetalDark = new(0.32f, 0.34f, 0.34f, 1f);
        static readonly Color LeafGold = new(0.78f, 0.68f, 0.38f, 1f);
        static readonly Color Gem = new(0.36f, 0.78f, 0.82f, 1f);
        static readonly Color GemDark = new(0.12f, 0.34f, 0.38f, 1f);

        public static Texture2D CreateTexture(string name)
        {
            return CreateTexture(name, WandererPose.Idle, 0f, 0f);
        }

        public static Texture2D CreateTexture(string name, float stride, float motion)
        {
            return CreateTexture(name, WandererPose.Move, stride, motion);
        }

        public static Texture2D CreateTexture(string name, WandererPose pose, float stride, float motion)
        {
            Texture2D texture = new(Width, Height, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear
            };

            Fill(texture, Clear);
            DrawWanderer(texture, pose, Mathf.Repeat(stride, 1f), Mathf.Clamp01(motion));
            texture.Apply();
            return texture;
        }

        static void DrawWanderer(Texture2D texture, WandererPose pose, float stride, float motion)
        {
            float cycle = stride * Mathf.PI * 2f;
            float legSwing = Mathf.Sin(cycle) * motion;
            float armSwing = Mathf.Sin(cycle + Mathf.PI) * motion;
            float cloakSwing = Mathf.Sin(cycle - Mathf.PI * 0.35f) * motion;
            float gemSwing = Mathf.Sin(cycle + Mathf.PI * 0.25f) * motion;
            float bob = Mathf.Abs(Mathf.Sin(cycle)) * motion * 2.2f;

            switch (pose)
            {
                case WandererPose.Jump:
                    legSwing = 0.25f;
                    armSwing = 0.45f;
                    cloakSwing = -0.75f;
                    gemSwing = -0.85f;
                    bob = 4.5f;
                    break;
                case WandererPose.Fall:
                    legSwing = -0.15f;
                    armSwing = -0.25f;
                    cloakSwing = 0.65f;
                    gemSwing = 0.75f;
                    bob = -1.5f;
                    break;
                case WandererPose.Attack:
                    float attackPower = Mathf.Lerp(0.35f, 1f, motion);
                    float attackSnap = Mathf.Sin(attackPower * Mathf.PI);
                    legSwing = Mathf.Lerp(0.3f, 0.95f, attackPower);
                    armSwing = attackPower;
                    cloakSwing = -Mathf.Lerp(0.3f, 1.1f, attackPower);
                    gemSwing = -Mathf.Lerp(0.25f, 0.9f, attackPower);
                    bob = 0.4f + attackSnap * 1.2f;
                    break;
            }

            DrawLegsAndBoots(texture, legSwing, pose);
            DrawCloakAndBody(texture, bob, cloakSwing, pose);
            DrawArmsAndCuffs(texture, bob, armSwing, pose);
            DrawHoodAndFace(texture, bob, gemSwing, pose);
        }

        static void DrawLegsAndBoots(Texture2D texture, float legSwing, WandererPose pose)
        {
            float rearX = -legSwing * 3.2f;
            float frontX = legSwing * 4.4f;
            float rearLift = Mathf.Max(0f, legSwing) * 3.2f;
            float frontLift = Mathf.Max(0f, -legSwing) * 3.2f;

            if (pose == WandererPose.Jump)
            {
                rearX = -4f;
                frontX = 7f;
                rearLift = 8f;
                frontLift = 7f;
            }
            else if (pose == WandererPose.Fall)
            {
                rearX = -2f;
                frontX = 3f;
                rearLift = -2f;
                frontLift = -1f;
            }
            else if (pose == WandererPose.Attack)
            {
                float attackPower = Mathf.Clamp01(Mathf.Abs(legSwing));
                rearX = Mathf.Lerp(-3f, -7f, attackPower);
                frontX = Mathf.Lerp(5f, 9f, attackPower);
                rearLift = 0f;
                frontLift = Mathf.Lerp(0.5f, 1.5f, attackPower);
            }

            DrawPolygon(texture, new[]
            {
                new Vector2(36f + rearX, 66f),
                new Vector2(45f + rearX, 66f),
                new Vector2(47f + rearX, 35f + rearLift),
                new Vector2(38f + rearX, 35f + rearLift)
            }, Outline);
            DrawPolygon(texture, new[]
            {
                new Vector2(53f + frontX, 67f),
                new Vector2(63f + frontX, 65f),
                new Vector2(61f + frontX, 35f + frontLift),
                new Vector2(51f + frontX, 35f + frontLift)
            }, Outline);
            DrawPolygon(texture, new[]
            {
                new Vector2(38f + rearX, 65f),
                new Vector2(44f + rearX, 65f),
                new Vector2(45f + rearX, 37f + rearLift),
                new Vector2(39f + rearX, 37f + rearLift)
            }, new Color(0.18f, 0.16f, 0.14f, 1f));
            DrawPolygon(texture, new[]
            {
                new Vector2(55f + frontX, 66f),
                new Vector2(61f + frontX, 64f),
                new Vector2(59f + frontX, 37f + frontLift),
                new Vector2(53f + frontX, 37f + frontLift)
            }, new Color(0.18f, 0.16f, 0.14f, 1f));

            DrawEllipse(texture, new Vector2(43f + rearX, 28f + rearLift), new Vector2(13f, 7f), Outline);
            DrawEllipse(texture, new Vector2(62f + frontX, 28f + frontLift), new Vector2(14f, 7f), Outline);
            DrawEllipse(texture, new Vector2(45f + rearX, 30f + rearLift), new Vector2(10f, 5f), Boot);
            DrawEllipse(texture, new Vector2(64f + frontX, 30f + frontLift), new Vector2(11f, 5f), Boot);
            DrawRect(texture, Mathf.RoundToInt(35f + rearX), Mathf.RoundToInt(37f + rearLift), Mathf.RoundToInt(47f + rearX), Mathf.RoundToInt(48f + rearLift), Outline);
            DrawRect(texture, Mathf.RoundToInt(51f + frontX), Mathf.RoundToInt(37f + frontLift), Mathf.RoundToInt(63f + frontX), Mathf.RoundToInt(49f + frontLift), Outline);
            DrawRect(texture, Mathf.RoundToInt(37f + rearX), Mathf.RoundToInt(39f + rearLift), Mathf.RoundToInt(46f + rearX), Mathf.RoundToInt(47f + rearLift), Boot);
            DrawRect(texture, Mathf.RoundToInt(53f + frontX), Mathf.RoundToInt(39f + frontLift), Mathf.RoundToInt(62f + frontX), Mathf.RoundToInt(48f + frontLift), Boot);
            DrawLine(texture, new Vector2(38f + rearX, 46f + rearLift), new Vector2(45f + rearX, 46f + rearLift), 1.2f, Trim);
            DrawLine(texture, new Vector2(54f + frontX, 47f + frontLift), new Vector2(61f + frontX, 47f + frontLift), 1.2f, Trim);
        }

        static void DrawCloakAndBody(Texture2D texture, float bob, float cloakSwing, WandererPose pose)
        {
            float attackLean = Mathf.Clamp01(Mathf.Abs(cloakSwing));
            float lean = pose == WandererPose.Attack ? Mathf.Lerp(2f, 5f, attackLean) : pose == WandererPose.Jump ? 1.5f : pose == WandererPose.Fall ? -1.5f : 0f;
            float flare = pose == WandererPose.Jump ? 4f : pose == WandererPose.Fall ? -3f : 0f;
            Vector2 P(float x, float y) => new(x + lean, y + bob);
            float hemSwing = cloakSwing * 3.5f;

            DrawPolygon(texture, new[]
            {
                P(36f, 120f),
                P(20f - hemSwing - flare, 72f),
                P(28f - hemSwing - flare, 54f),
                P(54f, 48f),
                P(77f + hemSwing + flare, 57f),
                P(73f, 100f),
                P(60f, 122f)
            }, Outline);

            DrawPolygon(texture, new[]
            {
                P(37f, 117f),
                P(24f - hemSwing - flare, 73f),
                P(31f - hemSwing - flare, 59f),
                P(55f, 53f),
                P(73f + hemSwing + flare, 60f),
                P(69f, 98f),
                P(58f, 118f)
            }, Cloth);

            DrawPolygon(texture, new[]
            {
                P(42f, 78f),
                P(62f, 76f),
                P(58f + hemSwing * 0.3f, 45f),
                P(47f, 39f),
                P(39f - hemSwing * 0.2f, 49f)
            }, Outline);
            DrawPolygon(texture, new[]
            {
                P(44f, 75f),
                P(59f, 74f),
                P(56f + hemSwing * 0.25f, 49f),
                P(48f, 43f),
                P(42f - hemSwing * 0.15f, 51f)
            }, new Color(0.78f, 0.7f, 0.54f, 1f));

            DrawLine(texture, P(34f, 109f), P(70f, 66f), 4.5f, Outline);
            DrawLine(texture, P(35f, 108f), P(69f, 67f), 2.5f, Leather);
            DrawEllipse(texture, P(57f, 86f), new Vector2(6.5f, 6.5f), Outline);
            DrawEllipse(texture, P(57f, 86f), new Vector2(4.8f, 4.8f), Trim);

            DrawEllipse(texture, P(52f, 93f), new Vector2(24f, 7f), Outline);
            DrawEllipse(texture, P(52f, 94f), new Vector2(21f, 5f), Scarf);

            DrawLeaf(texture, P(31f - hemSwing * 0.3f - flare * 0.2f, 75f), 6f, HoodLight);
            DrawLeaf(texture, P(68f + hemSwing * 0.4f + flare * 0.25f, 57f), 5f, HoodLight);
            DrawLine(texture, P(50f, 75f), P(49f, 45f), 1.2f, Trim);
        }

        static void DrawArmsAndCuffs(Texture2D texture, float bob, float armSwing, WandererPose pose)
        {
            float lean = pose == WandererPose.Attack ? 5f : pose == WandererPose.Jump ? 1.5f : pose == WandererPose.Fall ? -1.5f : 0f;
            Vector2 P(float x, float y) => new(x + lean, y + bob);
            float rearArm = -armSwing * 3f;
            float frontArm = armSwing * 5f;
            float frontArmLift = 0f;
            float rearArmLift = 0f;

            if (pose == WandererPose.Jump)
            {
                frontArm = 5f;
                rearArm = -4f;
                frontArmLift = 5f;
                rearArmLift = 3f;
            }
            else if (pose == WandererPose.Fall)
            {
                frontArm = -2f;
                rearArm = 2f;
                frontArmLift = -6f;
                rearArmLift = -4f;
            }
            else if (pose == WandererPose.Attack)
            {
                float attackPower = Mathf.Clamp01(Mathf.Abs(armSwing));
                frontArm = Mathf.Lerp(6f, 15f, attackPower);
                rearArm = Mathf.Lerp(-2f, -5f, attackPower);
                frontArmLift = Mathf.Lerp(1.5f, 4f, attackPower);
                rearArmLift = Mathf.Lerp(0f, -1f, attackPower);
            }

            DrawLine(texture, P(38f, 92f), P(31f + rearArm, 70f + rearArmLift), 6f, Outline);
            DrawLine(texture, P(39f, 91f), P(32f + rearArm, 71f + rearArmLift), 4f, new Color(0.16f, 0.13f, 0.1f, 1f));
            DrawEllipse(texture, P(31f + rearArm, 66f + rearArmLift), new Vector2(5f, 6f), Outline);
            DrawEllipse(texture, P(31f + rearArm, 67f + rearArmLift), new Vector2(3.5f, 4.8f), Leather);
            DrawRect(texture, Mathf.RoundToInt(27f + rearArm + lean), Mathf.RoundToInt(70f + bob + rearArmLift), Mathf.RoundToInt(37f + rearArm + lean), Mathf.RoundToInt(78f + bob + rearArmLift), Outline);
            DrawRect(texture, Mathf.RoundToInt(29f + rearArm + lean), Mathf.RoundToInt(72f + bob + rearArmLift), Mathf.RoundToInt(36f + rearArm + lean), Mathf.RoundToInt(76f + bob + rearArmLift), Metal);

            DrawLine(texture, P(63f, 91f), P(76f + frontArm, 70f + frontArmLift), 7f, Outline);
            DrawLine(texture, P(62f, 90f), P(75f + frontArm, 71f + frontArmLift), 5f, new Color(0.16f, 0.13f, 0.1f, 1f));
            DrawEllipse(texture, P(79f + frontArm, 66f + frontArmLift), new Vector2(7f, 7f), Outline);
            DrawEllipse(texture, P(78f + frontArm, 67f + frontArmLift), new Vector2(5f, 5f), Leather);
            DrawRect(texture, Mathf.RoundToInt(70f + frontArm + lean), Mathf.RoundToInt(70f + bob + frontArmLift), Mathf.RoundToInt(84f + frontArm + lean), Mathf.RoundToInt(79f + bob + frontArmLift), Outline);
            DrawRect(texture, Mathf.RoundToInt(72f + frontArm + lean), Mathf.RoundToInt(72f + bob + frontArmLift), Mathf.RoundToInt(82f + frontArm + lean), Mathf.RoundToInt(77f + bob + frontArmLift), Metal);
            DrawLine(texture, P(73f + frontArm, 77f + frontArmLift), P(81f + frontArm, 77f + frontArmLift), 1f, MetalDark);
            DrawEllipseRing(texture, P(84f + frontArm, 68f + frontArmLift), new Vector2(3.7f, 4.8f), 0.5f, Metal);
        }

        static void DrawHoodAndFace(Texture2D texture, float bob, float gemSwing, WandererPose pose)
        {
            float attackLean = Mathf.Clamp01(Mathf.Abs(gemSwing));
            float lean = pose == WandererPose.Attack ? Mathf.Lerp(2f, 5f, attackLean) : pose == WandererPose.Jump ? 1.5f : pose == WandererPose.Fall ? -1.5f : 0f;
            Vector2 P(float x, float y) => new(x + lean, y + bob);

            DrawPolygon(texture, new[]
            {
                P(46f, 181f),
                P(24f, 119f),
                P(29f, 98f),
                P(45f, 88f),
                P(66f, 93f),
                P(80f, 111f),
                P(76f, 134f),
                P(60f, 158f)
            }, Outline);

            DrawPolygon(texture, new[]
            {
                P(48f, 176f),
                P(28f, 120f),
                P(32f, 102f),
                P(46f, 93f),
                P(64f, 97f),
                P(76f, 112f),
                P(72f, 131f),
                P(59f, 154f)
            }, Hood);

            DrawPolygon(texture, new[]
            {
                P(46f, 181f),
                P(54f, 169f),
                P(63f, 158f),
                P(58f, 177f)
            }, HoodLight);

            DrawLine(texture, P(48f, 170f), P(58f, 158f), 1.1f, LeafGold);
            DrawLine(texture, P(47f, 170f), P(36f, 122f), 1.2f, new Color(0.18f, 0.32f, 0.26f, 1f));
            DrawLine(texture, P(62f, 151f), P(73f, 116f), 1.2f, new Color(0.18f, 0.32f, 0.26f, 1f));

            DrawEllipse(texture, P(57f, 122f), new Vector2(25f, 27f), DeepShadow);
            DrawEllipse(texture, P(60f, 121f), new Vector2(17f, 22f), Face);
            DrawEllipse(texture, P(66f, 122f), new Vector2(3.6f, 6f), Outline);
            DrawLine(texture, P(74f, 119f), P(78f, 121f), 1.3f, Face);

            DrawLeafMark(texture, P(61f, 134f));
            DrawLine(texture, P(31f, 102f), P(72f, 102f), 2f, Trim);
            DrawLeaf(texture, P(65f, 139f), 4f, LeafGold);
            DrawLeaf(texture, P(34f, 150f), 3f, LeafGold);
            DrawHoodGem(texture, bob, gemSwing, lean);
        }

        static void DrawHoodGem(Texture2D texture, float bob, float gemSwing, float lean)
        {
            Vector2 cordStart = new(48f + lean, 176f + bob);
            Vector2 ringCenter = new(34f + lean + gemSwing * 4.5f, 153f + bob - Mathf.Abs(gemSwing) * 2f);
            Vector2 gemCenter = ringCenter + new Vector2(0f, -10f);

            DrawLine(texture, cordStart, ringCenter + Vector2.up * 4f, 1.2f, Outline);
            DrawEllipseRing(texture, ringCenter, new Vector2(3.5f, 4.2f), 0.45f, Trim);
            DrawPolygon(texture, new[]
            {
                gemCenter + new Vector2(0f, 7f),
                gemCenter + new Vector2(5f, 0f),
                gemCenter + new Vector2(0f, -8f),
                gemCenter + new Vector2(-5f, 0f)
            }, Outline);
            DrawPolygon(texture, new[]
            {
                gemCenter + new Vector2(0f, 5.5f),
                gemCenter + new Vector2(3.8f, 0f),
                gemCenter + new Vector2(0f, -6f),
                gemCenter + new Vector2(-3.8f, 0f)
            }, Gem);
            DrawLine(texture, gemCenter + new Vector2(0f, 5f), gemCenter + new Vector2(0f, -5f), 0.7f, GemDark);
        }

        static void DrawLeaf(Texture2D texture, Vector2 center, float size, Color color)
        {
            DrawEllipse(texture, center, new Vector2(size * 0.65f, size), Outline);
            DrawEllipse(texture, center, new Vector2(size * 0.45f, size * 0.78f), color);
            DrawLine(texture, center - Vector2.up * size * 0.55f, center + Vector2.up * size * 0.55f, 0.8f, LeafGold);
        }

        static void DrawLeafMark(Texture2D texture, Vector2 center)
        {
            DrawEllipse(texture, center + new Vector2(0f, 2f), new Vector2(2.3f, 5f), LeafGold);
            DrawEllipse(texture, center + new Vector2(-5f, -1f), new Vector2(2f, 4.2f), LeafGold);
            DrawEllipse(texture, center + new Vector2(5f, -1f), new Vector2(2f, 4.2f), LeafGold);
        }

        static void Fill(Texture2D texture, Color color)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }

        static void DrawRect(Texture2D texture, int minX, int minY, int maxX, int maxY, Color color)
        {
            minX = Mathf.Clamp(minX, 0, texture.width - 1);
            minY = Mathf.Clamp(minY, 0, texture.height - 1);
            maxX = Mathf.Clamp(maxX, 0, texture.width - 1);
            maxY = Mathf.Clamp(maxY, 0, texture.height - 1);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }

        static void DrawEllipse(Texture2D texture, Vector2 center, Vector2 radius, Color color)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius.x));
            int maxX = Mathf.Min(texture.width - 1, Mathf.CeilToInt(center.x + radius.x));
            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius.y));
            int maxY = Mathf.Min(texture.height - 1, Mathf.CeilToInt(center.y + radius.y));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = (x + 0.5f - center.x) / radius.x;
                    float dy = (y + 0.5f - center.y) / radius.y;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        static void DrawEllipseRing(Texture2D texture, Vector2 center, Vector2 radius, float innerScale, Color color)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius.x));
            int maxX = Mathf.Min(texture.width - 1, Mathf.CeilToInt(center.x + radius.x));
            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius.y));
            int maxY = Mathf.Min(texture.height - 1, Mathf.CeilToInt(center.y + radius.y));
            float inner = innerScale * innerScale;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = (x + 0.5f - center.x) / radius.x;
                    float dy = (y + 0.5f - center.y) / radius.y;
                    float distance = dx * dx + dy * dy;
                    if (distance <= 1f && distance >= inner)
                    {
                        texture.SetPixel(x, y, distance > 0.82f ? Outline : color);
                    }
                }
            }
        }

        static void DrawLine(Texture2D texture, Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= Mathf.Epsilon)
            {
                DrawEllipse(texture, start, Vector2.one * width, color);
                return;
            }

            int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(start.x, end.x) - width));
            int maxX = Mathf.Min(texture.width - 1, Mathf.CeilToInt(Mathf.Max(start.x, end.x) + width));
            int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(start.y, end.y) - width));
            int maxY = Mathf.Min(texture.height - 1, Mathf.CeilToInt(Mathf.Max(start.y, end.y) + width));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 point = new(x + 0.5f, y + 0.5f);
                    float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
                    Vector2 closest = start + segment * t;
                    if (Vector2.Distance(point, closest) <= width)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        static void DrawPolygon(Texture2D texture, Vector2[] vertices, Color color)
        {
            float minXValue = vertices[0].x;
            float maxXValue = vertices[0].x;
            float minYValue = vertices[0].y;
            float maxYValue = vertices[0].y;

            for (int i = 1; i < vertices.Length; i++)
            {
                minXValue = Mathf.Min(minXValue, vertices[i].x);
                maxXValue = Mathf.Max(maxXValue, vertices[i].x);
                minYValue = Mathf.Min(minYValue, vertices[i].y);
                maxYValue = Mathf.Max(maxYValue, vertices[i].y);
            }

            int minX = Mathf.Max(0, Mathf.FloorToInt(minXValue));
            int maxX = Mathf.Min(texture.width - 1, Mathf.CeilToInt(maxXValue));
            int minY = Mathf.Max(0, Mathf.FloorToInt(minYValue));
            int maxY = Mathf.Min(texture.height - 1, Mathf.CeilToInt(maxYValue));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (IsInsidePolygon(new Vector2(x + 0.5f, y + 0.5f), vertices))
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        static bool IsInsidePolygon(Vector2 point, Vector2[] vertices)
        {
            bool inside = false;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                bool intersects = vertices[i].y > point.y != vertices[j].y > point.y
                    && point.x < (vertices[j].x - vertices[i].x) * (point.y - vertices[i].y) / (vertices[j].y - vertices[i].y) + vertices[i].x;
                if (intersects)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
