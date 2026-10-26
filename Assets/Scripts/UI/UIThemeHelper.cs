using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates crisp, procedurally cached 9-slice sprites for arcade UI cards, buttons, badges, and keycaps.
/// Guarantees razor-sharp rendering on all resolutions without external UI asset dependencies.
/// </summary>
public static class UIThemeHelper
{
    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite GetRoundedSprite(int cornerRadius, Color fillColor, Color borderColor, int borderWidth)
    {
        int r = Mathf.Max(2, cornerRadius);
        int bw = Mathf.Max(0, borderWidth);
        int size = Mathf.Max(32, (r + bw + 4) * 2);

        string key = $"{size}_{r}_{bw}_{ColorUtility.ToHtmlStringRGBA(fillColor)}_{ColorUtility.ToHtmlStringRGBA(borderColor)}";
        if (cache.TryGetValue(key, out Sprite existing) && existing != null)
            return existing;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];

        float rf = (float)r;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Corner center coordinates
                float cx = (x < r) ? rf : (x >= size - r) ? (size - 1 - rf) : x;
                float cy = (y < r) ? rf : (y >= size - r) ? (size - 1 - rf) : y;
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));

                if (dist > rf + 0.5f)
                {
                    pixels[y * size + x] = Color.clear;
                }
                else
                {
                    float alpha = Mathf.Clamp01(rf + 0.5f - dist);
                    bool isBorder = (dist >= rf - bw) || 
                                    (bw > 0 && (x < bw || x >= size - bw || y < bw || y >= size - bw));
                    Color c = isBorder ? borderColor : fillColor;
                    c.a *= alpha;
                    pixels[y * size + x] = c;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        float slice = r + bw + 2;
        Vector4 border = new Vector4(slice, slice, slice, slice);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        cache[key] = sprite;
        return sprite;
    }
}
