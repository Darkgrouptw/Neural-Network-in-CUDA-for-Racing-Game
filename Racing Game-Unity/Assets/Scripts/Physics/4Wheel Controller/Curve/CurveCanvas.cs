using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CurveCanvas
{
    private Texture2D texture;                                  // 貼圖
    public const int width = 320, height = 80;                  // 長寬

    private Color32[] pixels;                                   // 每個要畫在 texture 上的每個點
    private float alpha = 0.5f;                                 // 圖片的 alpha 值
    private bool changed = false;                               // 是否有變化，

    private List<Vector2> points = new List<Vector2>();         // 所有的點
    
    // 設定邊框
    private float TopY;
    private float BottomY;
    private float LeftX;
    private float RightX;
    private const int MaxPointSize = 500;                       // 點最多可以有幾個

    // 設定 Grid 的間隔
    private float GridGapX;
    private float GridGapY;

    public CurveCanvas(float ty, float by, float lx, float rx, float gapX, float gapY)
    {
        TopY = ty;
        BottomY = by;
        LeftX = lx;
        RightX = rx;
        GridGapX = gapX;
        GridGapY = gapY;

        texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        pixels = new Color32[width * height];
        ClearAllPoint();
    }

    public void ClearAllPoint()
    {
        points.Clear();

        Clear(Color.black);
        DrawGrid();
        changed = true;
    }
    public void AddPoint(float p1, float p2)
    {
        // 如果剛好達到 MaxPointSize ，把第一個點刪掉
        if (points.Count == MaxPointSize)
            points.RemoveAt(0);
        points.Add(new Vector2(p1, p2));

        Clear(Color.black);
        DrawGrid();
        for (int i = 1; i < points.Count; i++)
            DrawLine(points[i - 1].x, points[i - 1].y, points[i].x, points[i].y, Color.yellow);

        if(points.Count > 1)
            changed = true;
    }

    //
    // 圖片的操作
    // 
    public void Clear(Color32 color)
    {
        if (alpha >= 0)
            color.a = (byte)(Mathf.Clamp(alpha, 0, 1) * 255);

        for (int i = 0; i < width * height; i++)
            pixels[i] = color;

        changed = true;
    }
    private void ApplyChange()
    {
        if(changed)
        {
            texture.SetPixels32(pixels);
            texture.Apply(false);
            changed = false;
        }
    }
    private void Swap(ref float a, ref float b)
    {
        float temp = a;
        a = b;
        b = temp;
    }
    public void Draw(int x, int y)
    {
        ApplyChange();
        GUI.DrawTexture(new Rect(x, y, width, height), texture);
    }
    
    public void Save(string output)
    {
        System.IO.File.WriteAllBytes(output, texture.EncodeToPNG());
    }

    // 要座標的點 轉換到 pixel 上
    private void DrawLine(float x0, float y0, float x1, float y1, Color32 color)
    {
        if (alpha >= 0)
            color.a = (byte)(Mathf.Clamp(alpha, 0, 1) * 255);

        float scaleX = width / (RightX - LeftX);
        float scaleY = height / (TopY - BottomY);

        if (x0 > x1)
        {
            Swap(ref x0, ref x1);
            Swap(ref y0, ref y1);
        }

       TextureDrawLine((int)((x0 - LeftX) * scaleX), (int)((y0 - BottomY) * scaleY), (int)((x1 - LeftX) * scaleX), (int)((y1 - BottomY) * scaleY), color);
    }
    private void DrawGrid()
    {
        for (float i = LeftX; i <= RightX; i += GridGapX)
            DrawLine(i, TopY, i, BottomY, Color.white);
        for (float i = BottomY; i <= TopY; i += GridGapY)
            DrawLine(LeftX, i, RightX, i, Color.white);
    }


    #region 設定 Texture 的 Function
    private void TextureSetPixel(int x,int y, Color32 color)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
            pixels[y * width + x] = color;
    }
    private void TextureDrawLine(int x0, int y0, int x1, int y1, Color32 color)
    {
        int dy = y1 - y0;
        int dx = x1 - x0;
        int step = 1, org;

        TextureSetPixel(x0, y0, color);
        if(Mathf.Abs(dx) > Mathf.Abs(dy))
        {
            org = x0;
            if(dx < 0)
                step = -1;

            while(x0 != x1)
            {
                x0 += step;
                TextureSetPixel(x0, (y1 - y0) * (x0 - org) / (x1 - org) + y0 ,color);
            }
        }
        else
        {
            org = y0;
            if (dy < 0)
                step = -1;
            while(y0 != y1)
            {
                y0 += step;
                TextureSetPixel((x1 - x0) * (y0 - org)/ (y1 - org) + x0, y0, color);
            }
        }
    }
    #endregion
}
