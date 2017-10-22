using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Reflection;
public class SeperateAlpha
{
    static TextureImporterFormat currentFormat;

    [MenuItem("Assets/SeperateAlpha/SeperateWithoutMaterial/NormalQuality", false, 10)]
    public static void SeperateAlphaWithoutMaterial_Normal()
    {
        SeperateAllTexturesRGBandAlphaChannel(50, false);
    }

    [MenuItem("Assets/SeperateAlpha/SeperateWithoutMaterial/HighQuality", false, 10)]
    public static void SeperateAlphaWithoutMaterial_High()
    {
        if (EditorUtility.DisplayDialog("Warning", "Seperate with HighQuality MAY take a lot of time depends on the original size of texture", "Confirm", "Cancel"))
        {
            SeperateAllTexturesRGBandAlphaChannel(100, false);
        }
    }

    [MenuItem("Assets/SeperateAlpha/SeperateWithMaterial/NormalQuality", false, 10)]
    public static void SeperateAlphaWithMaterial_Normal()
    {
        SeperateAllTexturesRGBandAlphaChannel(50, true);
    }

    [MenuItem("Assets/SeperateAlpha/SeperateWithMaterial/HighQuality", false, 10)]
    public static void SeperateAlphaWithMaterial_High()
    {
        if (EditorUtility.DisplayDialog("Warning", "Seperate with HighQuality MAY take a lot of time depends on the original size of texture", "Confirm", "Cancel"))
        {
            SeperateAllTexturesRGBandAlphaChannel(100, true);
        }
    }

    static void SeperateAllTexturesRGBandAlphaChannel(int compressQuality, bool isNeedCreateMaterial)
    {
        List<string> assetsPathList = new List<string>();
        Object[] objects = Selection.objects;
        for (int i = 0; i < objects.Length; i++)
        {
            string path = AssetDatabase.GetAssetPath(objects[i]);
            if (path.Contains(".png") || path.Contains(".jpg") || path.Contains(".tga") || path.Contains(".psd"))
            {
                assetsPathList.Add(path);
            }
        }
        for (int i = 0; i < assetsPathList.Count; i++)
        {
            SeperateRGBAandlphaChannel(assetsPathList[i], compressQuality, isNeedCreateMaterial);
        }
    }

    static void SeperateRGBAandlphaChannel(string texPath, int compressQuality, bool isNeedCreateMaterial)
    {
        string assetRelativePath = texPath;
        SetTextureReadable(assetRelativePath);
        Texture2D sourcetex = AssetDatabase.LoadAssetAtPath(assetRelativePath, typeof(Texture2D)) as Texture2D;
        if (!sourcetex)
        {
            Debug.Log("Load Texture Failed : " + assetRelativePath);
            return;
        }
        if (!HasAlphaChannel(sourcetex))
        {
            Debug.Log("Texture does NOT Have Alpha Channel : " + assetRelativePath);
            return;
        }
        Texture2D alphaTex = new Texture2D((int)(sourcetex.width), (int)(sourcetex.height), TextureFormat.RGB24, true);
        for (int i = 0; i < sourcetex.width; ++i)
        {
            for (int j = 0; j < sourcetex.height; ++j)
            {
                Color color = sourcetex.GetPixel(i, j);
                Color rgbColor = color;
                Color alphaColor = color;
                alphaColor.r = color.a;
                alphaColor.g = color.a;
                alphaColor.b = color.a;
                alphaTex.SetPixel(i, j, alphaColor);
            }
        }
        alphaTex.Apply();
        byte[] bytes = alphaTex.EncodeToPNG();
        string alphaTexPath = GetAlphaTexPath(texPath);
        using (Stream fs = new FileStream(Path.Combine(Application.dataPath, alphaTexPath), FileMode.Create, FileAccess.ReadWrite))
        {
            fs.Write(bytes, 0, bytes.Length);
        }
        Debug.Log("Succeed to Seperate RGB and Alpha Channel For Texture : " + assetRelativePath);
        AssetDatabase.Refresh();
        alphaTexPath = Path.Combine("Assets", alphaTexPath);
        alphaTex = AssetDatabase.LoadAssetAtPath(alphaTexPath, typeof(Texture2D)) as Texture2D;
        TextureImporter sourceImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
        TextureImporter alphaImorter = AssetImporter.GetAtPath(alphaTexPath) as TextureImporter;

        if (null != sourceImporter && null != alphaImorter)
        {
            sourceImporter.SetPlatformTextureSettings(BuildTarget.Android.ToString(), sourceImporter.maxTextureSize, TextureImporterFormat.ETC_RGB4, compressQuality, false);
            sourceImporter.SetPlatformTextureSettings("iPhone", sourceImporter.maxTextureSize, TextureImporterFormat.PVRTC_RGB4, compressQuality, false);

            alphaImorter.mipmapEnabled = sourceImporter.mipmapEnabled;
            alphaImorter.wrapMode = TextureWrapMode.Clamp;
            alphaImorter.textureType = TextureImporterType.Advanced;
            alphaImorter.textureFormat = TextureImporterFormat.AutomaticCompressed;
            alphaImorter.SetPlatformTextureSettings(BuildTarget.Android.ToString(), sourceImporter.maxTextureSize, TextureImporterFormat.ETC_RGB4, compressQuality, false);
            alphaImorter.SetPlatformTextureSettings("iPhone", sourceImporter.maxTextureSize, TextureImporterFormat.PVRTC_RGB4, compressQuality, false);
            alphaImorter.npotScale = sourceImporter.npotScale;
            alphaImorter.compressionQuality = compressQuality;
            alphaImorter.spriteImportMode = SpriteImportMode.None;
            if (!string.IsNullOrEmpty(sourceImporter.assetBundleName))
            {
                alphaImorter.assetBundleName = sourceImporter.assetBundleName;
            }
            if (!string.IsNullOrEmpty(sourceImporter.assetBundleVariant))
            {
                alphaImorter.assetBundleVariant = sourceImporter.assetBundleVariant;
            }

            AssetDatabase.ImportAsset(alphaTexPath, ImportAssetOptions.ForceUpdate);
            SetTextureUnReadable(assetRelativePath);

            if (isNeedCreateMaterial)
            {
                string materialPath = texPath.Substring(0, texPath.LastIndexOf("/"));
                materialPath = Path.Combine(materialPath, sourcetex.name + ".mat");
                Material material = new Material(Shader.Find("SeperateAlpha/Normal"));
                material.name = sourcetex.name;
                material.SetTexture("_MainTex", sourcetex);
                material.SetTexture("_AlphaTex", alphaTex);
                AssetDatabase.CreateAsset(material, materialPath);

                AssetDatabase.Refresh();
                AssetImporter materialImporter = AssetImporter.GetAtPath(materialPath);
                if (!string.IsNullOrEmpty(sourceImporter.assetBundleName))
                {
                    materialImporter.assetBundleName = sourceImporter.assetBundleName;
                }
                if (!string.IsNullOrEmpty(sourceImporter.assetBundleVariant))
                {
                    materialImporter.assetBundleVariant = sourceImporter.assetBundleVariant;
                }
            }

            AssetDatabase.Refresh();
        }
    }

    static bool HasAlphaChannel(Texture2D texture)
    {
        for (int i = 0; i < texture.width; ++i)
        {
            for (int j = 0; j < texture.height; ++j)
            {
                Color color = texture.GetPixel(i, j);
                float alpha = color.a;
                if (alpha < 1.0f - 0.001f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    static void SetTextureReadable(string relativeAssetPath)
    {
        TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(relativeAssetPath);
        ti.isReadable = true;
        currentFormat = ti.textureFormat;
        ti.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        ti.ClearPlatformTextureSettings("Android");
        ti.ClearPlatformTextureSettings("iPhone");
        AssetDatabase.ImportAsset(relativeAssetPath);
    }

    static void SetTextureUnReadable(string relativeAssetPath)
    {
        TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(relativeAssetPath);
        ti.isReadable = false;
        ti.textureFormat = currentFormat;
        AssetDatabase.ImportAsset(relativeAssetPath);
    }

    static string GetRGBTexPath(string texPath)
    {
        return GetTexPath(texPath, "_RGB.");
    }

    static string GetAlphaTexPath(string texPath)
    {
        return GetTexPath(texPath, "_Alpha.");
    }

    static string GetTexPath(string texPath, string texRole)
    {
        string result = texPath.Replace(".", texRole);
        result = result.Substring(7);
        return result;
    }
}