using UnityEngine;

// 16x16 pixel-art icon of the speed power-up made in code: a yellow lightning bolt
// with a dark outline on a round night-blue badge with a lavender rim
// (the colours of the UI). The prefab's own sprite is missing from the project.
public static class PowerUpArt
{
    private const int Size = 16;

    private static Sprite speed;

    public static Sprite Speed => speed != null ? speed : speed = CreateSpeed();

    // Bolt shape in pixel coordinates (y goes up)
    private static readonly Vector2[] Bolt =
    {
        new Vector2(9.5f, 14f), new Vector2(4f, 7f), new Vector2(7.5f, 7f),
        new Vector2(5.5f, 1.5f), new Vector2(12f, 9f), new Vector2(8.5f, 9f), new Vector2(11.5f, 14f),
    };

    private static Sprite CreateSpeed()
    {
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color outline = new Color(0.17f, 0.14f, 0.13f);
        Color yellow = new Color(1f, 0.86f, 0.22f);
        Color yellowLight = new Color(1f, 0.97f, 0.7f);

        Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point; // Crisp pixels
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(7.5f, 7.5f);
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Vector2.Distance(new Vector2(x, y), center);
                Color c = clear;

                // Badge: rim + dark inside
                if (d <= 7.6f) c = d > 6.5f ? RetroUi.Accent : RetroUi.Inset;

                if (InBolt(p))
                {
                    c = x < 8 && y > 7 ? yellowLight : yellow; // Light upper-left part = shine
                }
                else if (d <= 6.5f && TouchesBolt(x, y))
                {
                    c = outline;
                }

                texture.SetPixel(x, y, c);
            }
        }

        texture.Apply();
        // 16 pixels per unit = 1 x 1 world unit, the size of the power-up collider
        return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
    }

    private static bool TouchesBolt(int x, int y)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (InBolt(new Vector2(x + dx + 0.5f, y + dy + 0.5f))) return true;
            }
        }
        return false;
    }

    // Standard even-odd point in polygon test
    private static bool InBolt(Vector2 p)
    {
        bool inside = false;
        for (int i = 0, j = Bolt.Length - 1; i < Bolt.Length; j = i++)
        {
            if ((Bolt[i].y > p.y) != (Bolt[j].y > p.y) &&
                p.x < (Bolt[j].x - Bolt[i].x) * (p.y - Bolt[i].y) / (Bolt[j].y - Bolt[i].y) + Bolt[i].x)
            {
                inside = !inside;
            }
        }
        return inside;
    }
}
