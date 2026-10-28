using UnityEngine;

// Pixel art of the flying enemy, made in code: a red bird looking left (at the cat),
// with a yellow beak and an angry eye. Two frames - wings up and wings down - that
// SpriteFlap swaps to make it flap. Red so it stands out on the night-blue sky.
public static class DroneArt
{
    private const int Size = 16;

    private static readonly Color Outline = new Color(0.17f, 0.14f, 0.13f); // Same ink as the UI boards
    private static readonly Color Beak = new Color(1f, 0.78f, 0.22f);
    private static readonly Color Eye = Color.white;

    // Shapes in pixel coordinates (y goes up), the bird faces left
    private static readonly Vector2 BodyCenter = new Vector2(9.5f, 6.5f);
    private static readonly Vector2 BodyRadius = new Vector2(4f, 2.7f);
    private static readonly Vector2 HeadCenter = new Vector2(5f, 9f);
    private const float HeadRadius = 2.5f;
    private static readonly Vector2[] BeakShape = { new Vector2(3f, 10.4f), new Vector2(0f, 9f), new Vector2(3f, 7.6f) };
    private static readonly Vector2[] TailShape = { new Vector2(12.8f, 7.6f), new Vector2(16f, 10f), new Vector2(14.6f, 6.6f), new Vector2(16f, 3.6f), new Vector2(12.8f, 5.4f) }; // Forked
    private static readonly Vector2[] WingUp = { new Vector2(7.2f, 8.2f), new Vector2(12.2f, 8.2f), new Vector2(11.2f, 15.6f), new Vector2(7.8f, 12.2f) };
    private static readonly Vector2[] WingDown = { new Vector2(7.2f, 5.8f), new Vector2(12.2f, 5.8f), new Vector2(11.6f, 0.4f), new Vector2(7.8f, 2.6f) };

    // [0] = wings up, [1] = wings down
    public static Sprite[] CreateFrames(Color bodyColor)
    {
        return new[] { CreateFrame(bodyColor, WingUp), CreateFrame(bodyColor, WingDown) };
    }

    private static Sprite CreateFrame(Color bodyColor, Vector2[] wing)
    {
        Color wingColor = Color.Lerp(bodyColor, Color.white, 0.3f);
        Color belly = Color.Lerp(bodyColor, Color.white, 0.45f);
        Color clear = new Color(0f, 0f, 0f, 0f);

        Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point; // Crisp pixels
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                Color c = clear;

                if (InBird(p, wing))
                {
                    c = bodyColor;
                    if (InBody(p) && p.y < BodyCenter.y - 0.8f) c = belly;         // Lighter belly
                    if (InPolygon(p, wing)) c = wingColor;
                    if (InPolygon(p, BeakShape)) c = Beak;
                    if (x == 4 && y == 9) c = Eye;                                 // Eye
                    if ((x == 4 || x == 5) && y == 10) c = Outline;               // Angry brow
                }
                else if (TouchesBird(x, y, wing))
                {
                    c = Outline;
                }

                texture.SetPixel(x, y, c);
            }
        }

        texture.Apply();
        // 16 pixels per unit = 1 x 1 world unit, the size of the enemy picture
        return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
    }

    private static bool InBird(Vector2 p, Vector2[] wing)
    {
        return InBody(p)
            || Vector2.Distance(p, HeadCenter) <= HeadRadius
            || InPolygon(p, BeakShape)
            || InPolygon(p, TailShape)
            || InPolygon(p, wing);
    }

    private static bool InBody(Vector2 p)
    {
        Vector2 d = new Vector2((p.x - BodyCenter.x) / BodyRadius.x, (p.y - BodyCenter.y) / BodyRadius.y);
        return d.sqrMagnitude <= 1f;
    }

    private static bool TouchesBird(int x, int y, Vector2[] wing)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if ((dx != 0 || dy != 0) && InBird(new Vector2(x + dx + 0.5f, y + dy + 0.5f), wing)) return true;
            }
        }
        return false;
    }

    // Standard even-odd point in polygon test
    private static bool InPolygon(Vector2 p, Vector2[] shape)
    {
        bool inside = false;
        for (int i = 0, j = shape.Length - 1; i < shape.Length; j = i++)
        {
            if ((shape[i].y > p.y) != (shape[j].y > p.y) &&
                p.x < (shape[j].x - shape[i].x) * (p.y - shape[i].y) / (shape[j].y - shape[i].y) + shape[i].x)
            {
                inside = !inside;
            }
        }
        return inside;
    }
}
