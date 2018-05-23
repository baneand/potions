using System;
using UnityEngine;
using UnityEditor;
using WaterPlusEditorInternal;
using System.IO;

// ReSharper disable CompareOfFloatsByEqualityOperator

public enum WpColorChannels
{
    R = 0,
    G = 1,
    B = 2,
    A = 3,
    Rgb = 4
}

public enum WpBlurType
{
    Gaussian = 0,
    Expand,
    Box
}

public enum WpGradientType
{
    Linear = 0,
    OneMinusSqr = 1,
    SqrOfOneMinusG = 2
}

public enum WpFilteringMethod
{
    Bilinear = 0,
    Bicubic
}

public class WpGrayscaleImage
{
    private byte[] m_Pixels;
    public int Width;
    public int Height;

    public WpGrayscaleImage(Texture2D texture, WpColorChannels channel)
    {
        Width = texture.width;
        Height = texture.height;

        m_Pixels = new byte[Width*Height];

        Color[] srcPixels = texture.GetPixels();

        for (int i = 0; i < Width*Height; i++)
        {
            switch (channel)
            {
                default:
                    m_Pixels[i] = (byte) (srcPixels[i].r*255.0f);
                    break;

                case WpColorChannels.G:
                    m_Pixels[i] = (byte) (srcPixels[i].g*255.0f);
                    break;

                case WpColorChannels.B:
                    m_Pixels[i] = (byte) (srcPixels[i].b*255.0f);
                    break;

                case WpColorChannels.A:
                    m_Pixels[i] = (byte) (srcPixels[i].a*255.0f);
                    break;
            }
        }
    }

    public WpGrayscaleImage(int width, int height, byte[] pixels)
    {
        Width = width;
        Height = height;

        SetPixels(pixels);
    }

    public WpGrayscaleImage(int width, int height, int[] pixels)
    {
        Width = width;
        Height = height;

        SetPixels(pixels);
    }

    public byte[] GetPixels()
    {
        byte[] pixelsCopy = new byte[Width*Height];

        for (int i = 0; i < Width*Height; i++)
        {
            pixelsCopy[i] = m_Pixels[i];
        }

        return pixelsCopy;
    }

    public void SetPixels(byte[] pixels)
    {
        m_Pixels = new byte[Width*Height];

        for (int i = 0; i < Width*Height; i++)
        {
            m_Pixels[i] = pixels[i];
        }
    }

    public void SetPixels(int[] pixels)
    {
        m_Pixels = new byte[Width*Height];

        for (int i = 0; i < Width*Height; i++)
        {
            m_Pixels[i] = (byte) pixels[i];
        }
    }

    public static Texture2D MakeTexture2D(WpGrayscaleImage r, WpGrayscaleImage g, WpGrayscaleImage b, WpGrayscaleImage a)
    {
        bool doDimensionsMatch = true;

        if (r != null && g != null)
        {
            if (r.Width != g.Width || r.Height != g.Height)
                doDimensionsMatch = false;
        }

        if (g != null && b != null)
        {
            if (g.Width != b.Width || g.Height != b.Height)
                doDimensionsMatch = false;
        }

        if (b != null && r != null)
        {
            if (b.Width != r.Width || b.Height != r.Height)
                doDimensionsMatch = false;
        }

        if (!doDimensionsMatch)
        {
            Debug.LogError("Cannot make a texture - dimensions mismatch.");
            g = null;
            b = null;
            a = null;
        }
        if (r == null)
        {
            return null;
        }

        Texture2D resultTexture = new Texture2D(r.Width, r.Height, TextureFormat.ARGB32, false);
        Color[] resPixels = new Color[r.Width*r.Height];

        byte[] gPixels = null;
        byte[] bPixels = null;
        byte[] aPixels = null;

        var rPixels = r.GetPixels();

        if (g != null)
            gPixels = g.GetPixels();

        if (b != null)
            bPixels = b.GetPixels();

        if (a != null)
            aPixels = a.GetPixels();

        for (int i = 0; i < r.Width*r.Height; i++)
        {
            resPixels[i].r = rPixels[i]/255.0f;


            if (g != null)
                resPixels[i].g = gPixels[i]/255.0f;
            else
                resPixels[i].g = 0.0f;


            if (b != null)
                resPixels[i].b = bPixels[i]/255.0f;
            else
                resPixels[i].b = 0.0f;


            if (a != null)
                resPixels[i].a = aPixels[i]/255.0f;
            else
                resPixels[i].a = 1.0f;
        }

        resultTexture.SetPixels(resPixels);
        resultTexture.Apply();

        return resultTexture;
    }

    public static WpGrayscaleImage ValueImage(int width, int height, byte value)
    {
        byte[] pixels = new byte[width*height];

        for (int i = 0; i < width*height; i++)
        {
            pixels[i] = value;
        }

        return new WpGrayscaleImage(width, height, pixels);
    }

    public static byte[] ValuePixels(int width, int height, byte value)
    {
        byte[] pixels = new byte[width*height];

        for (int i = 0; i < width*height; i++)
        {
            pixels[i] = value;
        }

        return pixels;
    }

    public static int[] ValuePixelsInt(int width, int height, int value)
    {
        int[] pixels = new int[width*height];

        for (int i = 0; i < width*height; i++)
        {
            pixels[i] = value;
        }

        return pixels;
    }
}

public static class WpHelper
{
    public static string WaterSystemPath = "Assets/WaterPlus/";
    #region Lightmapping Helpers

    public static float Max(float a, float b, float c)
    {
        float max = a;

        if (b > max)
            max = b;

        if (c > max)
            max = c;

        return max;
    }

    public static float Min(float a, float b, float c)
    {
        float min = a;

        if (b < min)
            min = b;

        if (c < min)
            min = c;

        return min;
    }

    public static void UvToVertex(Vector2 uv, WPMesh mesh, Vector2[] uVs, out bool vertexFound, out Vector3 vertexPos)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        int[] triangleFoundVertices = new int[3];

        bool triangleFound = false;

        int uvsLength = uVs.Length;

        //Find to what triangle the UV belongs to.
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (triangles[i] >= uvsLength || triangles[i + 1] >= uvsLength || triangles[i + 2] >= uvsLength)
                continue;

            Vector2 uv1 = uVs[triangles[i]];
            Vector2 uv2 = uVs[triangles[i + 1]];
            Vector2 uv3 = uVs[triangles[i + 2]];
            if (IsPointWithinTriangle(uv1, uv2, uv3, uv))
            {
                triangleFoundVertices[0] = triangles[i];
                triangleFoundVertices[1] = triangles[i + 1];
                triangleFoundVertices[2] = triangles[i + 2];

                triangleFound = true;
                break;
            }
        }

        if (triangleFound)
        {
            vertexFound = true;
            int vertex0 = triangleFoundVertices[0];
            int vertex1 = triangleFoundVertices[1];
            int vertex2 = triangleFoundVertices[2];

            Vector3 barycentricCoords = GetBarycentricCoords(uVs[vertex0], uVs[vertex1], uVs[vertex2], uv);
            vertexPos = barycentricCoords.x*vertices[vertex0] + barycentricCoords.y*vertices[vertex1] +
                        barycentricCoords.z*vertices[vertex2];
        }
        else
        {
            vertexFound = false;
            vertexPos = Vector3.zero;
        }
    }


    public static bool IsPointWithinTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 p)
    {
        float s = GetBarycentricX(v1, v2, v3, p);

        if (!(s >= 0.0f && s <= 1.0f))
            return false;

        float t = GetBarycentricY(v1, v2, v3, p);

        if (!(t >= 0.0f && t <= 1.0f))
            return false;

        if (s + t <= 1.0f)
            return true;
        else
            return false;
    }

    public static float GetBarycentricX(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 p)
    {
        return ((v2.y - v3.y)*(p.x - v3.x) + (v3.x - v2.x)*(p.y - v3.y))/
               ((v2.y - v3.y)*(v1.x - v3.x) + (v3.x - v2.x)*(v1.y - v3.y));
    }

    public static float GetBarycentricY(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 p)
    {
        return ((v3.y - v1.y)*(p.x - v3.x) + (v1.x - v3.x)*(p.y - v3.y))/
               ((v3.y - v1.y)*(v2.x - v3.x) + (v1.x - v3.x)*(v2.y - v3.y));
    }

    public static Vector3 GetBarycentricCoords(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 p)
    {
        Vector3 b = new Vector3
        {
            x = ((v2.y - v3.y)*(p.x - v3.x) + (v3.x - v2.x)*(p.y - v3.y))/
                ((v2.y - v3.y)*(v1.x - v3.x) + (v3.x - v2.x)*(v1.y - v3.y)),
            y = ((v3.y - v1.y)*(p.x - v3.x) + (v1.x - v3.x)*(p.y - v3.y))/
                ((v3.y - v1.y)*(v2.x - v3.x) + (v1.x - v3.x)*(v2.y - v3.y))
        };
        b.z = 1 - b.x - b.y;
        return b;
    }

    #endregion

    public static float SampleLayerHeight(Vector3 position, int layerMask)
    {
        //We should only raycast downwards
        RaycastHit hitInfo;
        if (Physics.Raycast(position + 500.0f*Vector3.up, Vector3.down, out hitInfo, 1000.0f, layerMask))
        {
            return hitInfo.point.y;
        }

        return 0.0f;
    }

    public static float SampleLayerSlope(Vector3 position, int layerMask)
    {
        //We should only raycast downwards
        RaycastHit hitInfo;
        if (Physics.Raycast(position + 500.0f*Vector3.up, Vector3.down, out hitInfo, 1000.0f, layerMask))
        {
            float cosSlope = Vector3.Dot(hitInfo.normal, Vector3.up);

            return Mathf.Rad2Deg*Mathf.Acos(cosSlope);
        }

        return 0.0f;
    }

    public static Vector3 SampleLayerNormal(Vector3 position, int layerMask)
    {
        //We should only raycast downwards
        RaycastHit hitInfo;
        if (Physics.Raycast(position + 500.0f*Vector3.up, Vector3.down, out hitInfo, 1000.0f, layerMask))
        {
            return hitInfo.normal;
        }

        return Vector3.up;
    }

    public static Texture2D SetPixelsForChannel(Texture2D texture, float[] pixels, WpColorChannels channel)
    {
        Color[] srcPixels = texture.GetPixels();

        for (int i = 0; i < texture.width*texture.height; i++)
        {
            switch (channel)
            {
                default:
                    srcPixels[i].r = pixels[i];
                    break;

                case WpColorChannels.G:
                    srcPixels[i].g = pixels[i];
                    break;

                case WpColorChannels.B:
                    srcPixels[i].b = pixels[i];
                    break;

                case WpColorChannels.A:
                    srcPixels[i].a = pixels[i];
                    break;
            }
        }

        Texture2D resultTexture = texture;
        resultTexture.SetPixels(srcPixels);
        resultTexture.Apply();

        return resultTexture;
    }

    public enum TextureBlurMode
    {
        BlurAll = 0,
        BlurIgnoreAlphaPixelsOnly
    }

    public static Texture2D BlurTexture(Texture2D texture, int blurSize)
    {
        return BlurTexture(texture, blurSize, -1.0f, TextureBlurMode.BlurAll);
    }

    public static Texture2D BlurTexture(Texture2D texture, int blurSize, float ignoreAlpha, TextureBlurMode mode)
    {
        DateTime startTime = DateTime.Now;

        Color[] origPixels = texture.GetPixels();

        Color[] texPixels = texture.GetPixels();

        bool shouldWriteAlpha = texture.format == TextureFormat.ARGB32 || texture.format == TextureFormat.RGBA32;

        // look at every pixel in the blur rectangle
        for (int xx = 0; xx < texture.width; xx++)
        {
            for (int yy = 0; yy < texture.height; yy++)
            {
                if (mode == TextureBlurMode.BlurIgnoreAlphaPixelsOnly)
                    if (origPixels[yy*texture.width + xx].a != ignoreAlpha)
                        continue;

                float avgR = 0.0f, avgG = 0.0f, avgB = 0.0f, avgA = 0.0f;
                int blurPixelCount = 0;

                // average the color of the red, green and blue for each pixel in the
                // blur size while making sure you don't go outside the image bounds
                for (int x = xx; (x < xx + blurSize && x < texture.width); x++)
                {
                    for (int y = yy; (y < yy + blurSize && y < texture.height); y++)
                    {
                        Color pixel = origPixels[y*texture.width + x];

                        //Ignore alpha is set
                        if (ignoreAlpha >= 0.0f)
                        {
                            if (pixel.a != ignoreAlpha)
                            {
                                avgR += pixel.r;
                                avgG += pixel.g;
                                avgB += pixel.b;

                                if (shouldWriteAlpha)
                                    avgA += pixel.a;

                                blurPixelCount++;
                            }
                        }
                        else
                        {
                            //Ignore alpha isn't set
                            avgR += pixel.r;
                            avgG += pixel.g;
                            avgB += pixel.b;

                            if (shouldWriteAlpha)
                                avgA += pixel.a;

                            blurPixelCount++;
                        }
                    }
                }
                if (blurPixelCount <= 0)
                    continue;

                avgR /= blurPixelCount;
                avgG /= blurPixelCount;
                avgB /= blurPixelCount;
                avgA /= blurPixelCount;

                // now that we know the average for the blur size, set each pixel to that color
                for (int x = xx; x < xx + blurSize && x < texture.width; x++)
                {
                    for (int y = yy; y < yy + blurSize && y < texture.height; y++)
                    {
                        if (shouldWriteAlpha)
                            texPixels[y*texture.width + x] = new Color(avgR, avgG, avgB, avgA);
                        else
                            texPixels[y*texture.width + x] = new Color(avgR, avgG, avgB);
                    }
                }
            }
        }

        Texture2D resultTexture = new Texture2D(texture.width, texture.height, texture.format, false);
        resultTexture.SetPixels(texPixels);
        resultTexture.Apply();

        Debug.Log("Image blur took " + (DateTime.Now - startTime).TotalSeconds + " seconds.");

        return resultTexture;
    }

    private static float GetGaussianWeight(float delta, float x, float y)
    {
        return (1.0f/(2.0f*Mathf.PI*delta*delta))*Mathf.Exp(-(x*x + y*y)/(2.0f*delta*delta));
    }

    private static float GetExpandBlurWeight(float delta)
    {
        return 1.0f/delta*delta;
    }

    private static float GetBoxBlurWeight(float delta)
    {
        return 1.0f/((delta*2 + 1)*(delta*2 + 1));
    }

    public static WpGrayscaleImage Blur(WpGrayscaleImage texture, int blurSize, WpBlurType blurType)
    {
        //Build weight table
        float[,] blurWeightTable = new float[blurSize + 1, blurSize + 1];
        for (int x = 0; x <= blurSize; x++)
        {
            for (int y = 0; y <= blurSize; y++)
            {
                switch (blurType)
                {
                    case WpBlurType.Box:
                        blurWeightTable[x, y] = GetBoxBlurWeight(blurSize);
                        break;

                    case WpBlurType.Expand:
                        blurWeightTable[x, y] = GetExpandBlurWeight(blurSize);
                        break;

                    default:
                        blurWeightTable[x, y] = GetGaussianWeight(blurSize, x, y);
                        break;
                }
            }
        }

        byte[] srcPixels = texture.GetPixels();
        byte[] resPixels = WpGrayscaleImage.ValuePixels(texture.Width, texture.Height, 0);

        // look at every pixel in the blur rectangle
        for (int x = 0; x < texture.Width; x++)
        {
            for (int y = 0; y < texture.Height; y++)
            {
                //Keep alpha intact
                float blurredColor = 0.0f;
                for (int xx = x - blurSize; xx <= x + blurSize; xx++)
                {
                    if (xx < 0 || xx >= texture.Width)
                        continue;

                    for (int yy = y - blurSize; yy <= y + blurSize; yy++)
                    {
                        if (yy < 0 || yy >= texture.Height)
                            continue;

                        float blurWeight = blurWeightTable[Mathf.Abs(xx - x), Mathf.Abs(yy - y)];
                        blurredColor += blurWeight*srcPixels[yy*texture.Width + xx]/255.0f;
                    }
                }
                resPixels[y*texture.Width + x] = (byte) (blurredColor*255.0f);
            }
        }
        return new WpGrayscaleImage(texture.Width, texture.Height, resPixels);
    }

    public static Color[] CopyPixels(Texture2D texture)
    {
        Color[] srcPixels = texture.GetPixels();
        Color[] dstPixels = new Color[texture.width*texture.height];

        for (int i = 0; i < texture.width*texture.height; i++)
        {
            dstPixels[i].r = srcPixels[i].r;
            dstPixels[i].g = srcPixels[i].g;
            dstPixels[i].b = srcPixels[i].b;
            dstPixels[i].a = srcPixels[i].a;
        }

        return dstPixels;
    }

    public static Texture2D CopyTexture(Texture2D texture)
    {
        Texture2D resultTexture = new Texture2D(texture.width, texture.height, texture.format, false);
        resultTexture.SetPixels(CopyPixels(texture));
        resultTexture.Apply();

        return resultTexture;
    }

    public static WpGrayscaleImage ExpandBorder(WpGrayscaleImage borderMask, int expandByPixels)
    {
        byte[] borderMaskPixels = borderMask.GetPixels();
        byte[] resPixels = WpGrayscaleImage.ValuePixels(borderMask.Width, borderMask.Height, 0); //Fill with black

        for (int x = 0; x < borderMask.Width; x++)
        {
            for (int y = 0; y < borderMask.Height; y++)
            {
                //Keep only border pixels
                if (borderMaskPixels[y*borderMask.Width + x] < 255)
                    continue;

                for (int xx = x - expandByPixels; xx <= x + expandByPixels; xx++)
                {
                    if (xx < 0 || xx >= borderMask.Width)
                        continue;

                    for (int yy = y - expandByPixels; yy <= y + expandByPixels; yy++)
                    {
                        if (yy < 0 || yy >= borderMask.Height)
                            continue;

                        resPixels[yy*borderMask.Width + xx] = 255;
                    }
                }
            }
        }

        WpGrayscaleImage resTexture = new WpGrayscaleImage(borderMask.Width, borderMask.Height, resPixels);

        return resTexture;
    }

    public static WpGrayscaleImage Gradient(WpGrayscaleImage borderMask, int gradientSize, float gradientMin,
        float gradientMax,
        WpGradientType gradientType)
    {
        byte[] resPixels = WpGrayscaleImage.ValuePixels(borderMask.Width, borderMask.Height, 255);
        //Colorize (start from the end, i.e. most expanded borders).
        for (int i = gradientSize; i >= 0; i--)
        {
            WpGrayscaleImage dilatedBorder = ExpandBorder(borderMask, i);
            byte[] currentBorderPixels = dilatedBorder.GetPixels();
            for (int x = 0; x < borderMask.Width; x++)
            {
                for (int y = 0; y < borderMask.Height; y++)
                {
                    if (currentBorderPixels[y*borderMask.Width + x] < 255)
                        continue;

                    float gradientAmount = i/(float) gradientSize;

                    switch (gradientType)
                    {
                        default:
                            gradientAmount = 1.0f - gradientAmount;
                            break;

                        case WpGradientType.OneMinusSqr:
                            gradientAmount = 1.0f - gradientAmount*gradientAmount;
                            break;

                        case WpGradientType.SqrOfOneMinusG:
                            gradientAmount = 1.0f - gradientAmount;
                            gradientAmount = gradientAmount*gradientAmount;
                            break;
                    }
                    gradientAmount = gradientAmount*(gradientMax - gradientMin) + gradientMin;
                    resPixels[y*borderMask.Width + x] = (byte) (gradientAmount*255.0f);
                }
            }
            GC.Collect();
        }
        WpGrayscaleImage resTexture = new WpGrayscaleImage(borderMask.Width, borderMask.Height, resPixels);

        return resTexture;
    }

    public static WpGrayscaleImage NormalizeImage(WpGrayscaleImage texture)
    {
        byte[] origPixels = texture.GetPixels();

        byte[] resPixels = texture.GetPixels();

        float maxBrightness = 0.0f;
        float minBrightness = 1.0f;

        //Get max brightness
        for (int x = 0; x < texture.Width; x++)
        {
            for (int y = 0; y < texture.Height; y++)
            {
                maxBrightness = Mathf.Max(maxBrightness, origPixels[y*texture.Width + x]/255.0f);
                minBrightness = Mathf.Min(minBrightness, origPixels[y*texture.Width + x]/255.0f);
            }
        }

        //Normalize
        for (int x = 0; x < texture.Width; x++)
        {
            for (int y = 0; y < texture.Height; y++)
            {
                resPixels[y*texture.Width + x] =
                    (byte)
                        (255.0f*(resPixels[y*texture.Width + x]/255.0f - minBrightness)/(maxBrightness - minBrightness));
            }
        }

        WpGrayscaleImage resultTexture = new WpGrayscaleImage(texture.Width, texture.Height, resPixels);

        return resultTexture;
    }

    public static Texture2D FlipImage(Texture2D texture, bool flipX, bool flipY)
    {
        if (!texture)
            return null;

        Color[] srcPixels = texture.GetPixels();
        Color[] resPixels = texture.GetPixels();

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                int flippedX = x;
                int flippedY = y;

                if (flipX)
                    flippedX = (texture.width - 1 - x);

                if (flipY)
                    flippedY = (texture.height - 1 - y);

                resPixels[y*texture.width + x] = srcPixels[flippedY*texture.width + flippedX];
            }
        }

        Texture2D resTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, true);
        resTexture.SetPixels(resPixels);
        resTexture.Apply();

        return resTexture;
    }

    public static void SaveTextureAsPng(Texture2D texture, string path)
    {
        if (File.Exists(path))
            File.Delete(path);

        if (texture == null)
        {
            Debug.LogError("SaveTextureAsPng: Input texture is null. Aborting.");
            return;
        }

        if (texture.format != TextureFormat.ARGB32 && texture.format != TextureFormat.RGB24)
        {
            var convertToFormat = texture.format == TextureFormat.DXT1 ? TextureFormat.RGB24 : TextureFormat.ARGB32;
            Texture2D convertedTexture = new Texture2D(texture.width, texture.height, convertToFormat, false);
            Color[] srcPixels = texture.GetPixels();
            convertedTexture.SetPixels(srcPixels);
            convertedTexture.Apply();
            texture = convertedTexture;
        }
        byte[] bytes = texture.EncodeToPNG();

        FileStream file = File.Open(path, FileMode.Create);
        BinaryWriter binary = new BinaryWriter(file);
        binary.Write(bytes);
        file.Close();

        AssetDatabase.ImportAsset(path);
        AssetDatabase.Refresh();
    }

    public static WpGrayscaleImage ResizeImage(WpGrayscaleImage texture, int width, int height,
        WpFilteringMethod filteringMethod)
    {
        if (texture == null)
        {
            Debug.LogWarning("ResizeImage: input texture is null");
            return null;
        }

        byte[] srcPixels = texture.GetPixels();
        byte[] dstPixels = new byte[width*height];

        float xRatio = texture.Width/(float) width;
        float yRatio = texture.Height/(float) height;
        //Resize using bilinear interpolation
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xx = x*xRatio;
                float yy = y*yRatio;

                //Get neighbour pixels in the original image

                var x0 = Mathf.FloorToInt(xx);
                var x1 = Mathf.CeilToInt(xx);
                var y0 = Mathf.FloorToInt(yy);
                var y1 = Mathf.CeilToInt(yy);
                //Avoid having the same pixel
                if (x1 == x0)
                    x1 = x0 + 1;

                if (y1 == y0)
                    y1 = y0 + 1;

                //Avoid crossing the image borders
                if (x1 < 0)
                    x1 = 0;

                if (x1 >= texture.Width)
                    x1 = texture.Width - 1;

                if (y1 < 0)
                    y1 = 0;

                if (y1 >= texture.Height)
                    y1 = texture.Height - 1;

                float b1 = srcPixels[y0*texture.Width + x0];
                float b2 = srcPixels[y0*texture.Width + x1] - (float) srcPixels[y0*texture.Width + x0];
                float b3 = srcPixels[y1*texture.Width + x0] - (float) srcPixels[y0*texture.Width + x0];
                float b4 = srcPixels[y0*texture.Width + x0] - (float) srcPixels[y0*texture.Width + x1] -
                           srcPixels[y1*texture.Width + x0] + srcPixels[y1*texture.Width + x1];

                var interpolatedColor = (byte) ((b1 + b2*(xx - x0) + b3*(yy - y0) + b4*(xx - x0)*(yy - y0)));
                dstPixels[y*width + x] = interpolatedColor;
            }
        }

        WpGrayscaleImage resTexture = new WpGrayscaleImage(width, height, dstPixels);
        return resTexture;
    }

    public static Texture2D ResizeImage(Texture2D texture, int width, int height)
    {
        if (texture == null)
        {
            Debug.LogWarning("ResizeImage: input texture is null");
            return null;
        }

        DateTime startTime = DateTime.Now;

        Color[] srcPixels = texture.GetPixels();
        Color[] dstPixels = new Color[width*height];

        float xRatio = texture.width/(float) width;
        float yRatio = texture.height/(float) height;
        //Resize using bilinear interpolation
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xx = x*xRatio;
                float yy = y*yRatio;

                //Get neighbour pixels in the original image
                var x0 = Mathf.FloorToInt(xx);
                var x1 = Mathf.CeilToInt(xx);
                var y0 = Mathf.FloorToInt(yy);
                var y1 = Mathf.CeilToInt(yy);

                //Avoid having the same pixel
                if (x1 == x0)
                    x1 = x0 + 1;

                if (y1 == y0)
                    y1 = y0 + 1;

                //Avoid crossing the image borders
                if (x1 < 0)
                    x1 = x0 + 1;

                if (x1 >= texture.width)
                    x1 = x0 - 1;

                if (y1 < 0)
                    y1 = y0 + 1;

                if (y1 >= texture.height)
                    y1 = y0 - 1;

                Color b1 = srcPixels[y0*texture.width + x0];
                Color b2 = srcPixels[y0*texture.width + x1] - srcPixels[y0*texture.width + x0];
                Color b3 = srcPixels[y1*texture.width + x0] - srcPixels[y0*texture.width + x0];
                Color b4 = srcPixels[y0*texture.width + x0] - srcPixels[y0*texture.width + x1] -
                           srcPixels[y1*texture.width + x0] + srcPixels[y1*texture.width + x1];

                Color interpolatedColor = b1 + b2*(xx - x0) + b3*(yy - y0) + b4*(xx - x0)*(yy - y0);

                dstPixels[y*width + x] = interpolatedColor;
            }
        }

        Texture2D resTexture = new Texture2D(width, height, texture.format, false);
        
        resTexture.SetPixels(dstPixels);
        resTexture.Apply();

        Debug.Log("Successfully resized the texture in " + (DateTime.Now - startTime).TotalSeconds + " seconds.");

        return resTexture;
    }

    public static Vector2 CalculateGeometricalCenter(Texture2D texture, Color lookupColor, float tolerance,
        WpColorChannels channel)
    {
        int totalX = 0, totalY = 0, totalPoints = 0;

        Color[] srcPixels = texture.GetPixels();

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Color pixelColor = srcPixels[y*texture.width + x];
                switch (channel)
                {
                    case WpColorChannels.R:
                        if (Mathf.Abs(pixelColor.r - lookupColor.r) <= tolerance)
                        {
                            totalX += x;
                            totalY += y;
                            totalPoints++;
                        }
                        break;

                    case WpColorChannels.G:
                        if (Mathf.Abs(pixelColor.g - lookupColor.g) <= tolerance)
                        {
                            totalX += x;
                            totalY += y;
                            totalPoints++;
                        }
                        break;

                    case WpColorChannels.B:
                        if (Mathf.Abs(pixelColor.b - lookupColor.b) <= tolerance)
                        {
                            totalX += x;
                            totalY += y;
                            totalPoints++;
                        }
                        break;

                    default:
                        if (Mathf.Abs(pixelColor.a - lookupColor.a) <= tolerance)
                        {
                            totalX += x;
                            totalY += y;
                            totalPoints++;
                        }
                        break;

                    case WpColorChannels.Rgb:
                        if (Mathf.Abs(pixelColor.r - lookupColor.r) <= tolerance &&
                            Mathf.Abs(pixelColor.g - lookupColor.g) <= tolerance &&
                            Mathf.Abs(pixelColor.b - lookupColor.b) <= tolerance)
                        {
                            totalX += x;
                            totalY += y;
                            totalPoints++;
                        }
                        break;
                }
            }
        }

        if (totalPoints <= 0)
            return new Vector2(texture.width/2f, texture.height/2f);

        totalX /= totalPoints;
        totalY /= totalPoints;

        Debug.Log("Found geometrical center at" + DimensionsString(totalX, totalY));

        return new Vector2(totalX, totalY);
    }

    public static Texture2D FillChannelWithValue(Texture2D texture, WpColorChannels channel, float value)
    {
        Color[] srcPixels = texture.GetPixels();

        for (int i = 0; i < texture.width*texture.height; i++)
        {
            switch (channel)
            {
                case WpColorChannels.R:
                    srcPixels[i].r = value;
                    break;

                case WpColorChannels.G:
                    srcPixels[i].g = value;
                    break;

                case WpColorChannels.B:
                    srcPixels[i].b = value;
                    break;
                default:
                    srcPixels[i].a = value;
                    break;

                case WpColorChannels.Rgb:
                    srcPixels[i].r = value;
                    srcPixels[i].g = value;
                    srcPixels[i].b = value;
                    break;
            }
        }

        Texture2D resultsTexture = new Texture2D(texture.width, texture.height, texture.format, false);
        resultsTexture.SetPixels(srcPixels);
        resultsTexture.Apply();

        return resultsTexture;
    }

    public static void SetTextureImporterFormat(Texture2D texture, TextureImporterFormat format, bool isReadable)
    {
        string assetPath = AssetDatabase.GetAssetPath(texture);
        TextureImporter tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (tImporter != null)
        {
            tImporter.textureType = TextureImporterType.Default;
            tImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
            {
                format = format
            });
            tImporter.isReadable = isReadable;

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }
    }

    public static void MakeTexturesReadable(Texture2D[] textures, bool isReadable)
    {
        if (null == textures)
        {
            Debug.LogError("null == _textures");
            return;
        }

        foreach (Texture2D tex in textures)
        {
            if (null == tex)
                continue;

            MakeTextureReadable(tex, isReadable);
        }
    }

    public static void MakeTextureReadable(Texture2D texture, bool isReadable)
    {
        if (!texture)
            return;

        TextureImporter tImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
        if (tImporter != null)
        {
            tImporter.isReadable = isReadable;
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture));
        }
    }

    public static void CompressTextures(Texture2D[] textures, bool isCompressed)
    {
        foreach (Texture2D tex in textures)
        {
            if (null == tex)
                continue;

            CompressTexture(tex, isCompressed);
        }
    }

    public static void CompressTexture(Texture2D texture, bool isCompressed)
    {
        if (!texture)
            return;

        TextureImporter tImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
        if (tImporter != null)
        {
            tImporter.textureCompression = isCompressed
                ? TextureImporterCompression.CompressedLQ
                : TextureImporterCompression.CompressedHQ;

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture));
        }
    }

    public static string SaveTextureAsPngAtAssetPath(Texture2D texture, string assetPath, bool shouldDeleteOriginal)
    {
        string oldFilePath = AssetPathToFilePath(assetPath);
        string newAssetPath = Path.ChangeExtension(assetPath, ".png");
        string newFilePath = AssetPathToFilePath(newAssetPath);
        SaveTextureAsPng(texture, newFilePath);

        if (shouldDeleteOriginal)
        {
            if (Path.GetExtension(oldFilePath) != ".png")
            {
                Debug.Log("Extension is not png, deleting older file.");
                AssetDatabase.DeleteAsset(assetPath);
            }
        }

        return newAssetPath;
    }

    public static string AssetPathToFilePath(string assetPath)
    {
        return Application.dataPath + "/" +
               assetPath.Remove(assetPath.IndexOf("Assets/", StringComparison.Ordinal), "Assets/".Length);
    }

    public static string FilePathToAssetPath(string filePath)
    {
        return filePath.Substring(filePath.LastIndexOf("Assets/", StringComparison.Ordinal));
    }

    public static int GetNearestPot(int number)
    {
        number--;
        number |= number >> 1;
        number |= number >> 2;
        number |= number >> 4;
        number |= number >> 8;
        number |= number >> 16;
        number++;

        return number;
    }

    public static void CreateWaterSystemDirs()
    {
        Directory.CreateDirectory(AssetPathToFilePath(WaterSystemPath) + "Temp/");
    }

    public static void CleanupTempFiles()
    {
        if (!Directory.Exists(AssetPathToFilePath(WaterSystemPath) + "Temp/"))
            return;
        DirectoryInfo directory = new DirectoryInfo(AssetPathToFilePath(WaterSystemPath) + "Temp/");
        foreach (FileInfo file in directory.GetFiles()) file.Delete();
    }

    public static bool HasSuffix(string path, string suffix)
    {
        int suffixIndex = path.LastIndexOf(suffix, StringComparison.Ordinal);

        if (suffixIndex == -1)
            return false;

        return true;
    }

    public static string RemoveSuffixFromFilename(string path, string suffix)
    {
        string directory = Path.GetDirectoryName(path);
        string filenameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        string extension = Path.GetExtension(path);

        // ReSharper disable once PossibleNullReferenceException
        int suffixIndex = filenameWithoutExtension.LastIndexOf(suffix, StringComparison.Ordinal);

        if (suffixIndex == -1)
        {
            Debug.LogError("suffix " + suffix + " not found for path " + path);
            return null;
        }

        filenameWithoutExtension = filenameWithoutExtension.Remove(suffixIndex, suffix.Length);

        return directory + "/" + filenameWithoutExtension + extension;
    }

    public static string AddSuffixToFilename(string path, string suffix)
    {
        string directory = Path.GetDirectoryName(path);
        string filenameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        string extension = Path.GetExtension(path);
        return directory + "/" + filenameWithoutExtension + suffix + extension;
    }

    public static string DimensionsString(int x, int y)
    {
        return " (" + x + "; " + y + ") ";
    }

    public static string DimensionsString(float x, float y)
    {
        return " (" + x + "; " + y + ") ";
    }

    private static string s_ProgressLog = "";

    public static void LogToProgressFile(string stringToLog)
    {
        string progressFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Water_Progress.txt";
        s_ProgressLog += DateTime.Now.ToShortTimeString() + ": " + stringToLog + Environment.NewLine;
        bool wasException = false;

        try
        {
            File.AppendAllText(progressFilePath, s_ProgressLog);
        }
        catch
        {
            wasException = true;
        }

        if (!wasException)
            s_ProgressLog = "";
    }

    public static void DeleteProgressFile()
    {
        string progressFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Water_Progress.txt";

        if (File.Exists(progressFilePath))
            File.Delete(progressFilePath);
    }
}
