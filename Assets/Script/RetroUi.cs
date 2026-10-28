using System;
using UnityEngine;
using UnityEngine.UI;

// Shared look of the UI built in code (settings window, tutorial, speed bar):
// the same blue-tinted boards and lavender text as the Main Menu buttons, and the
// ArcadeClassic pixel font of the game title (it has no % ( ) / : - signs).
public static class RetroUi
{
    public static readonly Color BoardTint = new Color(0.049f, 0.072f, 0.613f);  // Tint of the Main Menu buttons
    public static readonly Color TextColor = new Color(0.47f, 0.44f, 1f);        // Text of the Main Menu buttons
    public static readonly Color Accent = new Color(0.8f, 0.78f, 1f);            // Headings, fills, checkmarks
    public static readonly Color Highlight = new Color(0.95f, 0.95f, 1f);
    public static readonly Color LineColor = new Color(0.4f, 0.4f, 0.95f);       // Dividers, key hints
    public static readonly Color Inset = new Color(0.03f, 0.04f, 0.25f);         // Slider tracks, inside of boxes
    public static readonly Color Backdrop = new Color(0.02f, 0.02f, 0.12f, 0.8f);

    private static Font font;
    private static Sprite parchment;

    public static Font Font
    {
        get
        {
            if (font == null) font = Resources.Load<Font>("Fonts/ARCADECLASSIC");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }
    }

    public static Sprite Parchment
    {
        get
        {
            if (parchment == null) parchment = Resources.Load<Sprite>("UI/Parchment");
            return parchment;
        }
    }

    // Screen space canvas that scales from Full HD, like the in-game HUD
    public static Canvas CreateCanvas(GameObject host, int sortingOrder, bool clickable)
    {
        Canvas canvas = host.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = host.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (clickable)
        {
            host.AddComponent<GraphicRaycaster>();
        }
        return canvas;
    }

    // Blue board like the menu buttons: their parchment sprite with the same tint
    // (9-sliced, so it can be any size)
    public static GameObject CreateBoard(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject("Board", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = go.GetComponent<Image>();
        if (Parchment != null)
        {
            image.sprite = Parchment;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = size.y < 200f ? 2.2f : 1f; // Thinner torn edges on small boards
            image.color = BoardTint;
        }
        else
        {
            image.color = BoardTint; // Plain board if the sprite is missing
        }
        return go;
    }

    // Clickable blue board with a label, reacting like the Main Menu buttons
    public static GameObject CreateButton(Transform parent, string label, Vector2 position, Vector2 size, int fontSize, Action onClick)
    {
        GameObject go = CreateBoard(parent, position, size);
        go.name = label + " Button";

        Button button = go.AddComponent<Button>();
        button.targetGraphic = go.GetComponent<Image>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f); // Same as the Main Menu buttons
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f);
        button.colors = colors;
        button.onClick.AddListener(() => onClick());

        Text text = CreateText(go.transform, label, fontSize, TextColor, Vector2.zero, size, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform);
        return go;
    }

    public static Text CreateText(Transform parent, string content, int size, Color color, Vector2 position, Vector2 boxSize, TextAnchor alignment)
    {
        GameObject go = new GameObject(content, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = boxSize;

        Text text = go.GetComponent<Text>();
        StyleText(text, content, size, color, alignment);
        return text;
    }

    public static void StyleText(Text text, string content, int size, Color color, TextAnchor alignment)
    {
        text.font = Font;
        text.text = content;
        text.fontSize = size;
        text.fontStyle = FontStyle.Normal; // Bold looks blurry on a pixel font
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
