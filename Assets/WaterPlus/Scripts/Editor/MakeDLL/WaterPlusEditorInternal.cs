using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.IO;

#endif

namespace WaterPlusEditorInternal
{
#if UNITY_EDITOR
    public static class WaterPlusBaker
    {
        public static string WaterSystemPath = "Assets/WaterPlus/";

        public static Transform WaterSurfaceTransform;

        public static LayerMask TerrainLayerMask = 1 << 0;
        public static LayerMask RefractionLayerMask = 1 << 0;

        public static float FoamDistance = 4f;

        private const float ExportDepth = 25.0f;
        private static float s_MaxDepth01;

        public static float BakeStartTime = -100.0f;
        public static int BakeStage = -1;

        public static string WaterMapResString = "1024";
        public static string RefractionMapResString = "1024";

        public static float BakeProgress = -1.0f;

        public static string BakingTask = "Bake progress";

        public static bool ShouldProjectRefractionMap;

        public static string RefractionMapScaleString = "1.0";

        public static string LightmapWetnessHeightString = "1.0";
        public static string LightmapWetnessAmountString = ".2";

        public static void EditorUpdate()
        {
            //Read config from file
            if (WaterSurfaceTransform == null)
            {
                if (!ReadBakeSettings())
                    return;
            }
            else
            {
                if (WaterSurfaceTransform.gameObject == null)
                {
                    if (!ReadBakeSettings())
                        return;
                }
            }

            //Start baking in playmode
            if (BakeStartTime > -1.0f)
            {
                if (!LocateSystem(false))
                {
                    BakeStartTime = -1.0f;
                    UpdateBakeStage(-1);
                    return;
                }

                if (WaterSurfaceTransform == null)
                {
                    Debug.LogError("Please assign a water surface first.");
                    BakeStartTime = -1.0f;
                    UpdateBakeStage(-1);
                    return;
                }

                if (Time.realtimeSinceStartup >= BakeStartTime)
                {
                    if (!LocateSystem(false))
                    {
                        BakeStartTime = -1.0f;
                        UpdateBakeStage(-1);
                        return;
                    }

                    //Disable water static lightmapping flags
                    StaticEditorFlags waterFlags =
                        GameObjectUtility.GetStaticEditorFlags(WaterSurfaceTransform.gameObject);
                    waterFlags = waterFlags & ~StaticEditorFlags.LightmapStatic;
                    GameObjectUtility.SetStaticEditorFlags(WaterSurfaceTransform.gameObject, waterFlags);

                    WpHelper.CreateWaterSystemDirs();
                    BakeStartTime = -100.0f;
                    WpHelper.WaterSystemPath = WaterSystemPath;

                    //Bake watermaps right away, skip baking refraction map
                    UpdateBakeStage(13);
                    return;
                }
            }

            switch (BakeStage)
            {
                case 0: //Done baking in playmode
                    ReadBakeSettings();
                    break;
                case 13:
                    BuildWaterMapWrapper();
                    BakeProgress = .9f;
                    BakingTask = "Baking anisotropy map";
                    UpdateBakeStage(14);
                    break;
                case 14:
                    BakeAnisotropyMap();
                    UpdateBakeStage(15);
                    break;

                case 15:
                    Debug.Log("Successfully baked water maps.");
                    BakeProgress = 1.0f;
                    BakingTask = "Bake progress";
                    UpdateBakeStage(-1);
                    WpHelper.CleanupTempFiles();
                    break;
            }
        }

        private static bool LocateSystem(bool isSilent)
        {
            if (!EditorPrefs.HasKey("WaterPlusSystemPath"))
                EditorPrefs.SetString("WaterPlusSystemPath", "Assets/WaterPlus/");
            WaterSystemPath = EditorPrefs.GetString("WaterPlusSystemPath");
            if (!Directory.Exists(WpHelper.AssetPathToFilePath(WaterSystemPath)))
            {
                if (!isSilent)
                    Debug.Log("Unable to locate the WaterPlus system at path: '" + WaterSystemPath + "'. Relocating.");

                string[] wpDirs = Directory.GetDirectories(WpHelper.AssetPathToFilePath("Assets/"), "WaterPlus",
                    SearchOption.AllDirectories);

                bool wasSystemFound = false;

                foreach (string dir in wpDirs)
                {
                    if (Directory.Exists(Path.Combine(dir, "Shaders")))
                    {
                        WaterSystemPath = WpHelper.FilePathToAssetPath(dir) + "/";
                        wasSystemFound = true;
                        break;
                    }
                }

                if (wasSystemFound)
                {
                    Debug.Log("The system was relocated to: '" + WaterSystemPath + "'");

                    EditorPrefs.SetString("WaterPlusSystemPath", WaterSystemPath);

                    Debug.Log("Editor WaterPlusSystemPath key: " + EditorPrefs.GetString("WaterPlusSystemPath"));
                }
                else
                {
                    if (!isSilent)
                        Debug.LogError("Unable to locate WaterPlus system. Have you renamed the root system directory?");

                    return false;
                }
            }

            WpHelper.WaterSystemPath = WaterSystemPath;

            return true;
        }

        public static void UpdateLightmaps()
        {
            WpLightmapping.UpdateLightmaps(WaterSurfaceTransform, LightmapWetnessHeightString,
                LightmapWetnessAmountString);
        }

        #region Cubemaps

        public static void BakeCubemap()
        {
            if (WaterSurfaceTransform == null)
            {
                Debug.LogError("Please assign a water surface first.");
                return;
            }

            DuplicateMaterial();
            Skybox levelSkybox = null;

            //Look for a camera with a skybox
            foreach (Camera cam in Camera.allCameras)
            {
                if (cam.GetComponent<Skybox>() != null)
                {
                    levelSkybox = cam.GetComponent<Skybox>();
                    break;
                }
            }

            var skyboxMaterial = levelSkybox == null ? RenderSettings.skybox : levelSkybox.material;

            if (skyboxMaterial == null)
            {
                Debug.LogError("Cannot bake the cubemap - no skybox found. Please attach a skybox to the scene.");
                return;
            }
            Texture2D frontTexture = skyboxMaterial.GetTexture("_FrontTex") as Texture2D;
            Texture2D backTexture = skyboxMaterial.GetTexture("_BackTex") as Texture2D;
            Texture2D leftTexture = skyboxMaterial.GetTexture("_LeftTex") as Texture2D;
            Texture2D rightTexture = skyboxMaterial.GetTexture("_RightTex") as Texture2D;
            Texture2D upTexture = skyboxMaterial.GetTexture("_UpTex") as Texture2D;
            Texture2D downTexture = skyboxMaterial.GetTexture("_DownTex") as Texture2D; //Optional

            if (frontTexture == null || backTexture == null || leftTexture == null || rightTexture == null ||
                upTexture == null)
            {
                Debug.LogError(
                    "Cannot bake the cubemap - one or more of the skybox textures is missing. Skybox name: " +
                    skyboxMaterial.name);
                return;
            }

            Texture2D[] srcTextures = {frontTexture, backTexture, leftTexture, rightTexture, upTexture, downTexture};

            WpHelper.MakeTexturesReadable(srcTextures, true);
            WpHelper.CompressTextures(srcTextures, false);

            Cubemap tempCubemap = new Cubemap(frontTexture.width, TextureFormat.RGB24, true);

            frontTexture = WpHelper.FlipImage(frontTexture, false, true);
            backTexture = WpHelper.FlipImage(backTexture, false, true);
            leftTexture = WpHelper.FlipImage(leftTexture, false, true);
            rightTexture = WpHelper.FlipImage(rightTexture, false, true);
            upTexture = WpHelper.FlipImage(upTexture, false, true);
            downTexture = WpHelper.FlipImage(downTexture, false, true);

            //Gradients
            Color cubemapTint = Color.blue;
            if (WaterSurfaceTransform.GetComponent<Renderer>())
            {
                if (WaterSurfaceTransform.GetComponent<Renderer>().sharedMaterial)
                {
                    cubemapTint = Color.white;
                }
            }

            //Less = higher
            float gradientStart = .5f;
            float gradientEnd = .4f;

            frontTexture = ApplyLinearYGradientToTexture(frontTexture, cubemapTint, gradientStart, gradientEnd);
            backTexture = ApplyLinearYGradientToTexture(backTexture, cubemapTint, gradientStart, gradientEnd);
            leftTexture = ApplyLinearYGradientToTexture(leftTexture, cubemapTint, gradientStart, gradientEnd);
            rightTexture = ApplyLinearYGradientToTexture(rightTexture, cubemapTint, gradientStart, gradientEnd);

            if (downTexture)
                downTexture = ApplyLinearYGradientToTexture(downTexture, cubemapTint, 1.0f, 0.0f);

            Color[] frontPixels = frontTexture.GetPixels();
            Color[] backPixels = backTexture.GetPixels();
            Color[] leftPixels = leftTexture.GetPixels();
            Color[] rightPixels = rightTexture.GetPixels();
            Color[] upPixels = upTexture.GetPixels();
            Color[] downPixels = null;

            if (downTexture)
                downPixels = downTexture.GetPixels();

            tempCubemap.SetPixels(frontPixels, CubemapFace.PositiveZ);
            tempCubemap.SetPixels(backPixels, CubemapFace.NegativeZ);
            tempCubemap.SetPixels(rightPixels, CubemapFace.NegativeX);
            tempCubemap.SetPixels(leftPixels, CubemapFace.PositiveX);
            tempCubemap.SetPixels(upPixels, CubemapFace.PositiveY);

            if (downTexture)
                tempCubemap.SetPixels(downPixels, CubemapFace.NegativeY);

            tempCubemap.Apply();

            Directory.CreateDirectory(WpHelper.AssetPathToFilePath(WaterSystemPath) + "Cubemaps/");
            Material mat = WaterSurfaceTransform.GetComponent<Renderer>().sharedMaterial;

            if (mat == null)
            {
                Debug.LogError("No material assigned to the water surface.");
                return;
            }

            string cubemapPath = WaterSystemPath + "Cubemaps/" + mat.name + ".cubemap";


            if (AssetDatabase.LoadAssetAtPath(cubemapPath, typeof(Cubemap)) != null)
            {
                Debug.LogWarning("Asset at path " + cubemapPath + " already exists. Deleting.");
                AssetDatabase.DeleteAsset(cubemapPath);
            }

            AssetDatabase.Refresh();
            AssetDatabase.CreateAsset(tempCubemap, cubemapPath);
            AssetDatabase.Refresh();

            Debug.Log("Successfully saved the cubemap to " + cubemapPath);
            //Try to assign cubemap to water gameobject
            if (WaterSurfaceTransform)
            {
                mat = WaterSurfaceTransform.GetComponent<Renderer>().sharedMaterial;

                if (mat)
                {
                    mat.SetTexture("_Cube", tempCubemap);
                }
            }

            WpHelper.CleanupTempFiles();

            WpHelper.MakeTexturesReadable(srcTextures, false);
            WpHelper.CompressTextures(srcTextures, true);
        }

        private static Texture2D ApplyLinearYGradientToTexture(Texture2D texture, Color toColor, float fromY, float toY)
        {
            if (!texture)
                return null;

            int fromYPixels = (int) (fromY*texture.height);
            int toYPixels = (int) (toY*texture.height);

            Color[] srcPixels = texture.GetPixels();
            Color[] resPixels = texture.GetPixels();

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    //0.1 .. 0.8
                    float alpha = (y - fromYPixels)/(float) (toYPixels - fromYPixels);
                    alpha = Mathf.Clamp01(alpha);
                    resPixels[y*texture.width + x] = Color.Lerp(toColor, srcPixels[y*texture.width + x], alpha);
                }
            }

            Texture2D resultTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, true);
            resultTexture.SetPixels(resPixels);
            resultTexture.Apply();

            return resultTexture;
        }

        #endregion

        #region Water maps

        private static void DuplicateMaterial()
        {
            if (WaterSurfaceTransform == null)
                return;

            if (WaterSurfaceTransform.GetComponent<Renderer>().sharedMaterial.name == "WaterPlusMaterial")
            {
                string targetMaterialName = WaterSurfaceTransform.name;
                string targetPath;
                int i = 0;

                while (true)
                {
                    targetPath = WaterSystemPath + "Materials/" + targetMaterialName + ".mat";
                    Debug.Log("Duplicating the material. Path: " + WaterSystemPath + "Materials/" + targetMaterialName +
                              ".mat");
                    if (!File.Exists(WpHelper.AssetPathToFilePath(targetPath)))
                        break;
                    i++;
                    targetMaterialName = WaterSurfaceTransform.name + "_" + i;
                }

                string srcPath = WaterSystemPath + "Materials/WaterPlusMaterial.mat";
                string dstPath = targetPath;

                bool copied = AssetDatabase.CopyAsset(srcPath, dstPath);
                if (!copied)
                {
                    Debug.LogError("Failed to copy the material from " + srcPath + " to " + dstPath);
                    return;
                }

                AssetDatabase.ImportAsset(dstPath);
                AssetDatabase.Refresh();

                Material mat = AssetDatabase.LoadAssetAtPath(dstPath, typeof(Material)) as Material;

                if (mat == null)
                {
                    Debug.LogError("Failed to load material at path " + dstPath);
                    return;
                }

                WaterSurfaceTransform.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }

        private static bool CheckUVsConsistency(Transform transform)
        {
            MeshFilter meshFilter = transform.gameObject.GetComponent<MeshFilter>();

            if (!meshFilter)
            {
                Debug.LogError("The water surface has no MeshFilter. Aborting.");
                return false;
            }

            Mesh mesh = meshFilter.sharedMesh;

            if (!mesh)
            {
                Debug.LogError("The water surface has no Mesh. Aborting.");
                return false;
            }

            Vector2[] uvs = mesh.uv;

            bool foundUVs = true;

            if (uvs == null)
            {
                foundUVs = false;
            }
            else
            {
                if (uvs.Length <= 0)
                    foundUVs = false;
            }

            if (!foundUVs)
            {
                Debug.LogError("The water surface has no UVs. Aborting.");
                return false;
            }

            foreach (Vector2 uv in uvs)
            {
                if (uv.x < 0.0f || uv.x > 1.0f || uv.y < 0.0f || uv.y > 1.0f)
                {
                    Debug.LogError(
                        "The UVs of the water surface are not in 0..1 space. Unable to continue. Please fix the UVs and try again.");
                    return false;
                }
            }

            return true;
        }

        private static void BuildWaterMapWrapper()
        {
            if (WaterSurfaceTransform == null)
            {
                UpdateBakeStage(-1);
                Debug.LogError("Please assign a water surface first.");
                return;
            }

            if (!LocateSystem(false))
            {
                UpdateBakeStage(-1);
                return;
            }

            if (!CheckUVsConsistency(WaterSurfaceTransform))
            {
                UpdateBakeStage(-1);
                return;
            }

            DuplicateMaterial();

            WriteBakeSettings();

            int waterMapResolution;
            if (!int.TryParse(WaterMapResString, out waterMapResolution))
            {
                UpdateBakeStage(-1);
                Debug.LogError("Please enter a correct value into water map resolution");
                return;
            }

            waterMapResolution = Mathf.Clamp(WpHelper.GetNearestPot(waterMapResolution), 64, 4096);
            Texture2D waterMapTexture = BuildDepthMapAndMask(waterMapResolution, waterMapResolution,
                WaterSurfaceTransform.gameObject);
            System.GC.Collect();

            WpGrayscaleImage depthMap = new WpGrayscaleImage(waterMapTexture, WpColorChannels.R);

            waterMapTexture = WpGrayscaleImage.MakeTexture2D(depthMap, depthMap, depthMap, null);

            WpGrayscaleImage terrainMask = new WpGrayscaleImage(waterMapTexture, WpColorChannels.A);


            WpGrayscaleImage foamMap = CalculateFoamMap(depthMap, FoamDistance);
            WpGrayscaleImage transparencyMap = CalculateTransparencyMap(depthMap, terrainMask);
            WpGrayscaleImage.MakeTexture2D(depthMap, foamMap, transparencyMap, null);
            System.GC.Collect();

            WpGrayscaleImage refractionStrengthMap = CalculateRefractionStrengthMap(depthMap, terrainMask);
            System.GC.Collect();

            WpGrayscaleImage noiseGradient = ApplyGradientToDepthmap(depthMap, terrainMask, true);
            depthMap = ApplyGradientToDepthmap(depthMap, terrainMask, false);
            System.GC.Collect();
            int downsizeToRes = waterMapResolution/2;

            depthMap = WpHelper.ResizeImage(depthMap, downsizeToRes, downsizeToRes, WpFilteringMethod.Bilinear);
            WpGrayscaleImage dsNoiseGradient = WpHelper.ResizeImage(noiseGradient, downsizeToRes, downsizeToRes,
                WpFilteringMethod.Bilinear);
            depthMap = ApplyNoiseToDepthmap(depthMap, dsNoiseGradient, true);
            depthMap = WpHelper.Blur(depthMap, Mathf.Clamp(waterMapResolution/128, 3, 16), WpBlurType.Gaussian);
            depthMap = WpHelper.ResizeImage(depthMap, waterMapResolution, waterMapResolution, WpFilteringMethod.Bilinear);
            depthMap = WpHelper.Blur(depthMap, Mathf.Clamp(waterMapResolution/512, 3, 16), WpBlurType.Gaussian);

            depthMap = WpHelper.NormalizeImage(depthMap);

            depthMap = ApplyNoiseToDepthmap(depthMap, noiseGradient, false);

            waterMapTexture = WpGrayscaleImage.MakeTexture2D(depthMap, foamMap, transparencyMap, refractionStrengthMap);

            if (waterMapTexture == null)
            {
                Debug.LogError("waterMapTexture == null");
                UpdateBakeStage(-1);
                return;
            }
            Material mat = WaterSurfaceTransform.GetComponent<Renderer>().sharedMaterial;
            if (mat == null)
            {
                Debug.LogError("No material assigned to the water surface.");
                return;
            }

            Directory.CreateDirectory(WpHelper.AssetPathToFilePath(WaterSystemPath) + "WaterMaps/");
            string waterMapPath = WaterSystemPath + "WaterMaps/" + mat.name + "_watermap.png";

            WpHelper.SaveTextureAsPng(waterMapTexture, WpHelper.AssetPathToFilePath(waterMapPath));

            TextureImporter tImporter = AssetImporter.GetAtPath(waterMapPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.textureType = TextureImporterType.Default;
                tImporter.sRGBTexture = true;
                tImporter.textureCompression = TextureImporterCompression.Uncompressed;
                tImporter.maxTextureSize = 4096;
                tImporter.wrapMode = TextureWrapMode.Clamp;
                tImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings
                {
                    name = "iPhone",
                    maxTextureSize = 512
                });
                tImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "Android",
                    maxTextureSize = 512
                });
                AssetDatabase.ImportAsset(waterMapPath);
            }
            //Try to assign depthmap to water gameobject
            if (WaterSurfaceTransform)
            {
                if (mat)
                {
                    mat.SetTexture("_WaterMap",
                        AssetDatabase.LoadAssetAtPath(waterMapPath, typeof(Texture2D)) as Texture2D);
                }
            }
        }

        private static Texture2D BuildDepthMapAndMask(int width, int height, GameObject waterGameObject)
        {
            AttachMeshColliderToWater();
            Texture2D resultTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            Color[] resultsPixels = new Color[width*height];
            int oldLayer = waterGameObject.layer;
            //Set layer to water
            waterGameObject.layer = 4;

            Bounds objBounds = waterGameObject.GetComponent<Renderer>().bounds;

            float xIncrement = .9f*objBounds.size.x/width;
            float zIncrement = .9f*objBounds.size.z/height;

            float yOrigin = objBounds.max.y + 100.0f;

            float[,] heightsArray = new float[width, height];
            //Init the array
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    heightsArray[x, y] = -10.0f; //Nothing
                }
            }

            for (float x = objBounds.min.x; x <= objBounds.max.x; x += xIncrement)
            {
                for (float z = objBounds.min.z; z <= objBounds.max.z; z += zIncrement)
                {
                    Vector3 origin = new Vector3(x, yOrigin, z);
                    RaycastHit waterHitInfo;

                    if (Physics.Raycast(origin, Vector3.down, out waterHitInfo, 1000.0f, 1 << waterGameObject.layer))
                    {
                        //Raycast all but water
                        RaycastHit[] raycasts = Physics.RaycastAll(origin, Vector3.down, 1000.0f, TerrainLayerMask);

                        float heightValue;

                        if (raycasts.Length > 0)
                        {
                            //Find heighest point
                            float heighestPoint = -100000.0f;
                            foreach (RaycastHit hit in raycasts)
                            {
                                if (hit.point.y > heighestPoint && hit.point.y <= waterHitInfo.point.y)
                                    heighestPoint = hit.point.y;
                            }

                            //Did we hit something?
                            if (heighestPoint > -100000.0f)
                            {
                                heightValue = waterHitInfo.point.y - heighestPoint;
                                if (heightValue > ExportDepth)
                                {
                                    heightValue = ExportDepth;
                                }
                            }
                            else //Hit nothing (terrain is above us)
                                heightValue = -1.0f;
                        }
                        else
                        {
                            //Hit nothing
                            heightValue = -10.0f;
                        }

                        if (waterHitInfo.textureCoord.x >= 1.0f || waterHitInfo.textureCoord.y >= 1.0f)
                            continue;

                        heightsArray[
                            (int) (waterHitInfo.textureCoord.x*width), (int) (waterHitInfo.textureCoord.y*height)] =
                            heightValue;
                    }
                }
            }

            //Set pixel values
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float alphaValue;
                    float depthChannelValue;

                    //Hit something
                    if (heightsArray[x, y] > -1.0f)
                    {
                        depthChannelValue = (heightsArray[x, y]/ExportDepth);

                        s_MaxDepth01 = Mathf.Max(s_MaxDepth01, depthChannelValue);

                        if (depthChannelValue > 1.0f)
                        {
                            depthChannelValue = 1.0f;
                        }

                        alphaValue = 1.0f;
                    }
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    else if (heightsArray[x, y] == -1.0f)
                    {
                        //Hit from above
                        depthChannelValue = 0.0f;
                        alphaValue = 1.0f;
                    }
                    else
                    {
                        //Nothing was hit
                        depthChannelValue = 1.0f;
                        alphaValue = 0.0f;
                    }
                    Color resultColor = new Color(depthChannelValue, 0.0f, 0.0f, alphaValue);
                    resultsPixels[y*width + x] = resultColor;
                }
            }
            resultTexture.SetPixels(resultsPixels);
            resultTexture.Apply();
            waterGameObject.layer = oldLayer;
            RestoreOriginalWaterCollider();
            return resultTexture;
        }

        private static WpGrayscaleImage CalculateRefractionStrengthMap(WpGrayscaleImage depthMap, WpGrayscaleImage mask)
        {
            byte[] srcPixels = depthMap.GetPixels();
            byte[] resPixels = mask.GetPixels();
            byte[] maskPixels = mask.GetPixels();
            for (int i = 0; i < depthMap.Width*depthMap.Height; i++)
            {
                //Outside of the mask no refraction
                if (maskPixels[i] <= 0)
                    resPixels[i] = 255;
                else
                {
                    float refrStrength = (srcPixels[i]/255.0f)/(s_MaxDepth01*0.5f);
                    resPixels[i] = (byte) (Mathf.Clamp01(refrStrength)*255.0f);
                }
            }
            WpGrayscaleImage resultTexture = new WpGrayscaleImage(depthMap.Width, depthMap.Height, resPixels);
            resultTexture = WpHelper.Blur(resultTexture, 3, WpBlurType.Box);
            return resultTexture;
        }

        private static WpGrayscaleImage BuildShoreBorder(WpGrayscaleImage texture, bool borderAtZeroPixels)
        {
            byte[] sourcePixels = texture.GetPixels();
            byte[] resultPixels = WpGrayscaleImage.ValuePixels(texture.Width, texture.Height, 0);

            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    //Skip empty pixels
                    if (borderAtZeroPixels)
                    {
                        if (sourcePixels[y*texture.Width + x] > 0)
                            continue;
                    }
                    else
                    {
                        if (sourcePixels[y*texture.Width + x] < 255)
                            continue;
                    }

                    bool isBorderPixel = false;
                    for (int xx = x - 1; xx <= x + 1 && !isBorderPixel; xx++)
                    {
                        for (int yy = y - 1; yy <= y + 1; yy++)
                        {
                            if (xx < 0 || yy < 0 || xx >= texture.Width || yy >= texture.Height)
                                continue;
                            //Skip self
                            if (xx == x || yy == y)
                                continue;
                            //Is this pixel empty? If so, we're at border
                            bool isPixelEmpty = false;
                            if (borderAtZeroPixels)
                            {
                                if (sourcePixels[yy*texture.Width + xx] > 0)
                                    isPixelEmpty = true;
                            }
                            else
                            {
                                if (sourcePixels[yy*texture.Width + xx] < 255)
                                    isPixelEmpty = true;
                            }

                            if (isPixelEmpty)
                            {
                                resultPixels[y*texture.Width + x] = 255;

                                isBorderPixel = true;
                                break;
                            }
                        }
                    }
                }
            }
            WpGrayscaleImage resultTexture = new WpGrayscaleImage(texture.Width, texture.Height, resultPixels);
            return resultTexture;
        }

        private static WpGrayscaleImage CalculateFoamMap(WpGrayscaleImage depthMap, float methodFoamDistance)
        {
            int foamDistanceInPixels = Mathf.RoundToInt(methodFoamDistance*(depthMap.Width + depthMap.Height)/
                                                        (WaterSurfaceTransform.GetComponent<Renderer>().bounds.size.x +
                                                         WaterSurfaceTransform.GetComponent<Renderer>().bounds.size.z));

            //Default value
            if (methodFoamDistance <= 0.0f)
                foamDistanceInPixels = 1;

            //Prevent from hanging by having too much foam to calculate
            foamDistanceInPixels = Mathf.Min(10, foamDistanceInPixels);
            //Build shore border
            WpGrayscaleImage shoreBorderTexture = BuildShoreBorder(depthMap, true);
            //GrayscaleImage resultTexture = Helper.Gradient(shoreBorderTexture, shoreMask, foamDistanceInPixels, GradientType.linear);
            WpGrayscaleImage resultTexture = WpHelper.Gradient(shoreBorderTexture, foamDistanceInPixels, 1.0f, 0.0f,
                WpGradientType.Linear);

            //Reverse pixels color
            byte[] resultPixels = resultTexture.GetPixels();
            for (int i = 0; i < depthMap.Width*depthMap.Height; i++)
            {
                resultPixels[i] = (byte) (255 - resultPixels[i]);
            }

            resultTexture.SetPixels(resultPixels);

            resultTexture = WpHelper.Blur(resultTexture, Mathf.Clamp(depthMap.Width/2048, 1, 2), WpBlurType.Gaussian);
            resultTexture = WpHelper.NormalizeImage(resultTexture);
            return resultTexture;
        }

        private static WpGrayscaleImage CalculateTransparencyMap(WpGrayscaleImage depthMap, WpGrayscaleImage terrainMask)
        {
            //Build shore border
            WpGrayscaleImage shoreBorderTexture = BuildShoreBorder(depthMap, true);
            int transparencyDistance = depthMap.Width/256;

            transparencyDistance = Mathf.Clamp(transparencyDistance, 2, 10);
            WpGrayscaleImage resultTexture = WpHelper.Gradient(shoreBorderTexture, transparencyDistance, 1.0f, 0.0f,
                WpGradientType.SqrOfOneMinusG);
            resultTexture = WpHelper.Blur(resultTexture, Mathf.Clamp(depthMap.Width/2048, 1, 2), WpBlurType.Gaussian);
            resultTexture = WpHelper.NormalizeImage(resultTexture);
            //Recover border after the blur
            byte[] resultPixels = resultTexture.GetPixels();
            byte[] borderPixels = shoreBorderTexture.GetPixels();
            for (int i = 0; i < depthMap.Width*depthMap.Height; i++)
            {
                if (borderPixels[i] > 0)
                    resultPixels[i] = 0; //1.0f - borderPixels[i].b;
            }
            //Remove transparency on the inside (where the terrain is)
            byte[] depthPixels = depthMap.GetPixels();
            byte[] terrainMaskPixels = terrainMask.GetPixels();
            for (int i = 0; i < depthMap.Width*depthMap.Height; i++)
            {
                if (depthPixels[i] <= 0 && terrainMaskPixels[i] > 0)
                    resultPixels[i] = 0;
            }
            resultTexture.SetPixels(resultPixels);
            return resultTexture;
        }

        private static WpGrayscaleImage ApplyGradientToDepthmap(WpGrayscaleImage depthMap, WpGrayscaleImage terrainMask,
            bool isFullGradient)
        {
            int downsampleToResolution = Mathf.Clamp(WpHelper.GetNearestPot(depthMap.Width/10), 32, 256);
            WpGrayscaleImage downsampledTerrainMask = WpHelper.ResizeImage(terrainMask, downsampleToResolution,
                downsampleToResolution, WpFilteringMethod.Bilinear);
            if (downsampledTerrainMask == null)
            {
                Debug.LogError("failed to resize image");
                return depthMap;
            }
            //Build border for the gradient
            WpGrayscaleImage dsBorder = BuildShoreBorder(downsampledTerrainMask, true);
            float gradientFromValue;
            if (isFullGradient)
                gradientFromValue = 0.0f;
            else
                gradientFromValue = s_MaxDepth01*.9f;
            //Apply gradient
            WpGrayscaleImage gradientTexture = WpHelper.Gradient(dsBorder, (int) (downsampleToResolution/(2.0f*1.42f)),
                1.0f, gradientFromValue,
                WpGradientType.Linear);
            //Resize gradient back to the original size
            gradientTexture = WpHelper.ResizeImage(gradientTexture, depthMap.Width, depthMap.Height,
                WpFilteringMethod.Bilinear);
            if (gradientTexture == null)
            {
                Debug.LogError("failed to resize image");
                return depthMap;
            }
            //Recover the actual depthmap
            byte[] gradientPixels = gradientTexture.GetPixels();
            byte[] maskPixels = terrainMask.GetPixels();
            byte[] resultPixels = depthMap.GetPixels();

            for (int i = 0; i < depthMap.Width*depthMap.Height; i++)
            {
                resultPixels[i] = (byte) (Mathf.Lerp(gradientPixels[i]/255.0f,
                    resultPixels[i]/255.0f,
                    maskPixels[i]/255.0f)*255.0f);
            }
            return new WpGrayscaleImage(depthMap.Width, depthMap.Height, resultPixels);
        }

        private static float GetNoiseForPixel(int x, int y, int width, int height, int seed)
        {
            float noise0 = SimplexNoise.SeamlessNoise01(x/((float) width),
                y/((float) height),
                2.5f, 2.5f, seed);

            float noise1 = SimplexNoise.SeamlessNoise01(x/((float) width),
                y/((float) height),
                5.0f, 5.0f, seed);

            float noise2 = SimplexNoise.SeamlessNoise01(x/((float) width),
                y/((float) height),
                10.0f, 10.0f, seed);

            float noise3 = SimplexNoise.SeamlessNoise01(x/((float) width),
                y/((float) height),
                20.0f, 20.0f, seed);

            float noise4 = SimplexNoise.SeamlessNoise01(x/((float) width),
                y/((float) height),
                30.0f, 30.0f, seed);

            float noise = 0.0f;

            noise += noise0*1.0f; // noiseAmount += 1.0f;
            noise += noise1*.5f; // noiseAmount += .5f;
            noise += noise2*.25f; //	noiseAmount += .25f;
            noise += noise3*.12f; //	noiseAmount += .12f;
            noise += noise4*.06f; //	noiseAmount += .06f;

            noise /= 2.0f; //2 is the sum of all

            noise = Mathf.Pow(noise, 3.0f)*2.5f;
            return noise;
        }

        private static WpGrayscaleImage ApplyNoiseToDepthmap(WpGrayscaleImage texture, WpGrayscaleImage noiseGradient,
            bool addLargeNoise)
        {
            byte[] sourcePixels = texture.GetPixels();
            byte[] noiseGradientPixels = noiseGradient.GetPixels();
            byte[] resultPixels = WpGrayscaleImage.ValuePixels(texture.Width, texture.Height, 0);

            int seed = System.DateTime.Now.Millisecond;

            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    float sourceColor = sourcePixels[y*texture.Width + x]/255.0f;
                    float noise;
                    if (addLargeNoise)
                    {
                        noise = GetNoiseForPixel(x, y, texture.Width, texture.Height, seed)*.9f + .1f;
                        float noiseGradientAmount = noiseGradientPixels[y*texture.Width + x]/255.0f;

                        float noiseAmount = 1.0f - Mathf.Pow(1.0f - noiseGradientAmount, 2.0f);

                        noise = 1.0f - Mathf.Pow(1.0f - noise, 5.0f); //Have more deep(white) colors
                        noise = noise*2.0f - 1.0f;
                        noise = sourceColor + noise*noiseAmount*.1f;
                    }
                    else
                    {
                        float noise1 = SimplexNoise.SeamlessNoise01(x/((float) texture.Width),
                            y/((float) texture.Height),
                            15.0f, 15.0f, seed);
                        float noise2 = SimplexNoise.SeamlessNoise01(x/((float) texture.Width),
                            y/((float) texture.Height),
                            50.0f, 50.0f, seed);

                        float noise3 = SimplexNoise.SeamlessNoise01(x/((float) texture.Width),
                            y/((float) texture.Height),
                            100.0f, 100.0f, seed);

                        noise1 = noise1*2.0f - 1.0f;
                        noise2 = noise2*2.0f - 1.0f;
                        noise3 = noise3*2.0f - 1.0f;

                        noise = sourceColor + noise1*.1f + noise2*.05f + noise3*.025f;
                    }
                    resultPixels[y*texture.Width + x] = (byte) (Mathf.Clamp01(noise)*255.0f);
                }
            }

            return new WpGrayscaleImage(texture.Width, texture.Height, resultPixels);
        }
        #endregion

        #region Refractions

        private static void WriteBakeSettings()
        {
            if (WaterSurfaceTransform == null)
                return;

            //Get resolution
            int refrRes; // compute the next highest power of 2 of 32-bit v

            if (!int.TryParse(RefractionMapResString, out refrRes))
            {
                Debug.LogError("Please enter a valid integer into the resolution field. Aborting.");
                return;
            }

            refrRes = Mathf.Clamp(WpHelper.GetNearestPot(refrRes), 32, 4096);

            int watermapRes; // compute the next highest power of 2 of 32-bit v

            if (!int.TryParse(WaterMapResString, out watermapRes))
            {
                Debug.LogError("Please enter a valid integer into the resolution field. Aborting.");
                return;
            }

            watermapRes = Mathf.Clamp(WpHelper.GetNearestPot(watermapRes), 32, 4096);

            string[] bakeSettings = new string[10];
            bakeSettings[0] = GetGameObjectPath(WaterSurfaceTransform.gameObject);
            bakeSettings[1] = BakeStage.ToString();
            bakeSettings[2] = "bake";
            bakeSettings[3] = TerrainLayerMask.value.ToString();
            bakeSettings[4] = RefractionLayerMask.value.ToString();
            bakeSettings[5] = refrRes.ToString();
            bakeSettings[6] = "not set";
            bakeSettings[7] = ShouldProjectRefractionMap.ToString();
            bakeSettings[8] = RefractionMapScaleString;
            bakeSettings[9] = watermapRes.ToString();

            File.WriteAllLines(WpHelper.AssetPathToFilePath(WaterSystemPath) + "/bakesettings.txt", bakeSettings);
        }

        private static bool ReadBakeSettings()
        {
            if (!LocateSystem(true))
            {
                return false;
            }

            string bakeSettingsPath = WpHelper.AssetPathToFilePath(WaterSystemPath) + "/bakesettings.txt";

            if (!File.Exists(bakeSettingsPath))
                return false;

            WpHelper.CreateWaterSystemDirs();

            string[] bakeSettings = File.ReadAllLines(bakeSettingsPath);

            GameObject waterSurfaceGameObject = GameObject.Find(bakeSettings[0]);

            if (waterSurfaceGameObject)
                WaterSurfaceTransform = waterSurfaceGameObject.transform;

            BakeStage = int.Parse(bakeSettings[1]);
            TerrainLayerMask = int.Parse(bakeSettings[3]);
            RefractionLayerMask = int.Parse(bakeSettings[4]);
            RefractionMapResString = bakeSettings[5];
            ShouldProjectRefractionMap = bool.Parse(bakeSettings[7]);
            RefractionMapScaleString = bakeSettings[8];
            WaterMapResString = bakeSettings[9];

            return true;
        }
        private static void UpdateBakeStage(int stage)
        {
            BakeStage = stage;

            string bakeSettingsPath = WpHelper.AssetPathToFilePath(WaterSystemPath) + "/bakesettings.txt";
            if (!File.Exists(bakeSettingsPath))
                return;

            string[] bakeSettings = File.ReadAllLines(bakeSettingsPath);
            bakeSettings[1] = stage.ToString();
            File.WriteAllLines(bakeSettingsPath, bakeSettings);
        }

        #endregion

        #region Anisotropy

        public static void BakeAnisotropyMap()
        {
            if (!WaterSurfaceTransform)
                return;
            if (!WaterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial)
                return;
            Light dirLight = FindTheBrightestDirectionalLight();
            if (!dirLight)
            {
                Debug.LogError("Cannot bake anisotropic map - no directional light found.");
                return;
            }

            Vector3 lightDirection = dirLight.transform.forward;

            Material mat = WaterSurfaceTransform.GetComponent<Renderer>().sharedMaterial;

            if (mat == null)
            {
                Debug.LogError("No material assigned to the water surface.");
                return;
            }

            string anisoMapAssetPath = WaterSystemPath + "WaterMaps/" + mat.name + "_anisomap.png";

            Texture2D anisoMapTexture = new Texture2D(512, 512, TextureFormat.ARGB32, true);
            Color[] anisoMapPixels = new Color[anisoMapTexture.width*anisoMapTexture.height];

            //Works only for flat surfaces!!!
            Vector3 surfaceDir = WaterSurfaceTransform.up;
            float shininess =
                WaterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial.GetFloat("_Shininess");

            for (int x = 0; x < anisoMapTexture.width; x++)
            {
                for (int y = 0; y < anisoMapTexture.height; y++)
                {
                    int mapIndex = y*anisoMapTexture.width + x;
                    //Aniso dir
                    Vector3 anisoDirection = Vector3.Cross(surfaceDir, lightDirection);
                    anisoDirection = (anisoDirection + new Vector3(1.0f, 1.0f, 1.0f))*.5f;
                    //Aniso lookup
                    float lightDotT = (x/(float) anisoMapTexture.width)*2.0f - 1.0f;
                    float viewDotT = (y/(float) anisoMapTexture.height)*2.0f - 1.0f;

                    float anisoLookup = Mathf.Sqrt(1.0f - lightDotT*lightDotT)*Mathf.Sqrt(1.0f - viewDotT*viewDotT) -
                                        lightDotT*viewDotT;
                    anisoLookup = Mathf.Pow(anisoLookup, shininess*128.0f); // * gloss;

                    anisoMapPixels[mapIndex] = new Color(anisoDirection.x, anisoDirection.y, anisoDirection.z,
                        anisoLookup);
                }
            }

            anisoMapTexture.SetPixels(anisoMapPixels);
            anisoMapTexture.Apply();

            Directory.CreateDirectory(WpHelper.AssetPathToFilePath(WaterSystemPath) + "WaterMaps/");
            anisoMapTexture = AssetDatabase.LoadAssetAtPath(anisoMapAssetPath, typeof(Texture2D)) as Texture2D;

            WaterSurfaceTransform.gameObject.GetComponent<Renderer>()
                .sharedMaterial.SetTexture("_AnisoMap", anisoMapTexture);

            TextureImporter tImporter = AssetImporter.GetAtPath(anisoMapAssetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.textureType = TextureImporterType.Default;
                tImporter.sRGBTexture = true;
                tImporter.textureCompression = TextureImporterCompression.CompressedHQ;
                tImporter.wrapMode = TextureWrapMode.Repeat;
                tImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "iPhone",
                    maxTextureSize = 512
                });
                tImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "Android",
                    maxTextureSize = 512
                });
                AssetDatabase.ImportAsset(anisoMapAssetPath);
            }
        }

        #endregion

        #region Specularity

        private static Light FindTheBrightestDirectionalLight()
        {
            Light[] lights = Object.FindObjectsOfType(typeof(Light)) as Light[];
            List<Light> directionalLights = new List<Light>();

            // ReSharper disable once PossibleNullReferenceException
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                    directionalLights.Add(light);
            }

            if (directionalLights.Count <= 0)
                return null;

            var resultLight = directionalLights[0];

            foreach (Light light in directionalLights)
            {
                if (light.intensity > resultLight.intensity)
                    resultLight = light;
            }

            return resultLight;
        }

        #endregion

        #region Flow Maps

        public static void AdjustFlowmap()
        {
            if (WaterSurfaceTransform == null)
            {
                Debug.LogError("Please assign a water surface first.");
                return;
            }


            if (!WaterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial)
            {
                Debug.LogError("No material assigned to the water surface. Aborting.");
                return;
            }

            Texture2D flowmapTexture =
                WaterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial.GetTexture("_FlowMap") as
                    Texture2D;

            if (!flowmapTexture)
            {
                Debug.LogError("No flow map texture assigned to the water surface. Aborting.");
                return;
            }
            //Make texture readable
            string flowmapTextureAssetPath = AssetDatabase.GetAssetPath(flowmapTexture);

            if (flowmapTextureAssetPath == null)
            {
                Debug.LogError("flowmapTexturePath is null");
                return;
            }

            TextureImporter tImporter = AssetImporter.GetAtPath(flowmapTextureAssetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.isReadable = true;
                tImporter.textureType = TextureImporterType.Default;
                tImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    format = TextureImporterFormat.ARGB32
                });
                tImporter.wrapMode = TextureWrapMode.Repeat;

                AssetDatabase.ImportAsset(flowmapTextureAssetPath);
                AssetDatabase.Refresh();
            }

            //Normalize first (convert to min..1)
            Color[] flowmapPixels = flowmapTexture.GetPixels();
            float minSpeed = 1000.0f;
            float maxSpeed = -1000.0f;
            for (int x = 0; x < flowmapTexture.width; x++)
            {
                for (int y = 0; y < flowmapTexture.height; y++)
                {
                    int flowmapIndex = y*flowmapTexture.width + x;

                    Vector2 flowVelocity = new Vector2(flowmapPixels[flowmapIndex].r, flowmapPixels[flowmapIndex].g);
                    flowVelocity = flowVelocity*2.0f - (new Vector2(1.0f, 1.0f));

                    float currentSpeed = flowVelocity.magnitude;
                    minSpeed = Mathf.Min(currentSpeed, minSpeed);
                    maxSpeed = Mathf.Max(currentSpeed, maxSpeed);
                }
            }


            Debug.Log("minSpeed: " + minSpeed + " maxSpeed " + maxSpeed);

            for (int x = 0; x < flowmapTexture.width; x++)
            {
                for (int y = 0; y < flowmapTexture.height; y++)
                {
                    int flowmapIndex = y*flowmapTexture.width + x;

                    Vector2 flowVelocity = new Vector2(flowmapPixels[flowmapIndex].r, flowmapPixels[flowmapIndex].g);
                    flowVelocity = flowVelocity*2.0f - (new Vector2(1.0f, 1.0f));

                    float currentSpeed = flowVelocity.magnitude;

                    //Convert from min..max to min..1
                    currentSpeed = (currentSpeed - minSpeed)/(maxSpeed - minSpeed);
                    currentSpeed *= 1.0f - minSpeed;
                    currentSpeed += minSpeed;

                    //Update the flowmap
                    if (x == flowmapTexture.width/2)
                        Debug.Log("currentSpeed: " + currentSpeed);

                    flowVelocity = flowVelocity.normalized*currentSpeed;
                    flowmapPixels[flowmapIndex].r = (flowVelocity.x + 1.0f)*.5f;
                    flowmapPixels[flowmapIndex].g = (flowVelocity.y + 1.0f)*.5f;
                }
            }

            flowmapTexture.SetPixels(flowmapPixels);
            flowmapTexture.Apply();

            string newFlowmapPath = WpHelper.SaveTextureAsPngAtAssetPath(flowmapTexture, flowmapTextureAssetPath, true);

            Debug.Log("newFlowmapPath: " + newFlowmapPath);

            flowmapTexture = AssetDatabase.LoadAssetAtPath(newFlowmapPath, typeof(Texture2D)) as Texture2D;

            WaterSurfaceTransform.gameObject.GetComponent<Renderer>()
                .sharedMaterial.SetTexture("_FlowMap", flowmapTexture);

            Debug.Log("Successfully normalized the flowmap");

            //Adjust speed
            Texture2D watermapTexture =
                WaterSurfaceTransform.gameObject.GetComponent<Renderer>().sharedMaterial.GetTexture("_WaterMap") as
                    Texture2D;

            string watermapTextureAssetPath = AssetDatabase.GetAssetPath(watermapTexture);

            tImporter = AssetImporter.GetAtPath(watermapTextureAssetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.isReadable = true;
                tImporter.textureType = TextureImporterType.Default;
                AssetDatabase.ImportAsset(watermapTextureAssetPath);
                AssetDatabase.Refresh();
            }

            if (watermapTexture == null || flowmapTexture == null)
            {
                Debug.LogError("Cannot adjust the flowmap, because the water map cannot be found.");
                return;
            }

            float widthAspect = (float) watermapTexture.width/flowmapTexture.width;
            float heightAspect = (float) watermapTexture.height/flowmapTexture.height;

            const float minFlowSpeed = 0.1f;
            const float maxFlowSpeed = .7f;
            const float maxAdjustmentDepth = ExportDepth*.5f; //After 50% deep speed won't change

            for (int x = 0; x < flowmapTexture.width; x++)
            {
                for (int y = 0; y < flowmapTexture.height; y++)
                {
                    float currentDepth = watermapTexture.GetPixel((int) (x*widthAspect), (int) (y*heightAspect)).r*
                                         ExportDepth;

                    int flowmapIndex = y*flowmapTexture.width + x;

                    Vector2 flowVelocity = new Vector2(flowmapPixels[flowmapIndex].r, flowmapPixels[flowmapIndex].g);
                    flowVelocity = flowVelocity*2.0f - (new Vector2(1.0f, 1.0f));

                    float currentSpeed = flowVelocity.magnitude;

                    //Calculate new adjusted speed
                    float newSpeed;
                    //If the pixel is too deep, don't change its speed
                    if (currentDepth <= maxAdjustmentDepth)
                    {
                        newSpeed = 1.0f - (maxAdjustmentDepth - currentDepth)/maxAdjustmentDepth;
                    }
                    else
                    {
                        newSpeed = 1.0f;
                    }

                    //Convert to 0..1
                    newSpeed *= (currentSpeed - minSpeed)/(1.0f - minSpeed);

                    newSpeed = newSpeed*(maxFlowSpeed - minFlowSpeed) + minFlowSpeed;
                    //Update the flowmap
                    flowVelocity = flowVelocity.normalized*newSpeed;

                    flowmapPixels[flowmapIndex].r = (flowVelocity.x + 1.0f)*.5f;
                    flowmapPixels[flowmapIndex].g = (flowVelocity.y + 1.0f)*.5f;
                }
            }

            flowmapTexture.SetPixels(flowmapPixels);
            flowmapTexture.Apply();

            //Save flowmap
            newFlowmapPath = WpHelper.SaveTextureAsPngAtAssetPath(flowmapTexture, flowmapTextureAssetPath, true);
            Debug.Log("newFlowmapPath: " + newFlowmapPath);

            flowmapTexture = AssetDatabase.LoadAssetAtPath(newFlowmapPath, typeof(Texture2D)) as Texture2D;
            WaterSurfaceTransform.gameObject.GetComponent<Renderer>()
                .sharedMaterial.SetTexture("_FlowMap", flowmapTexture);
            Debug.Log("Successfully adjusted the flowmap to the terrain.");
            //Reimport flowmap
            tImporter = AssetImporter.GetAtPath(flowmapTextureAssetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.isReadable = true;
                tImporter.textureType = TextureImporterType.Default;
                tImporter.textureCompression = TextureImporterCompression.Compressed;
                tImporter.wrapMode = TextureWrapMode.Repeat;

                AssetDatabase.ImportAsset(flowmapTextureAssetPath);
                AssetDatabase.Refresh();
            }
        }

        #endregion

        #region Helpers

        public static void AttachMeshColliderToWater()
        {
            MeshCollider waterMeshCollider = WaterSurfaceTransform.GetComponent<MeshCollider>();
            //Disable all colliders apart from mesh colliders
            foreach (Collider collider in WaterSurfaceTransform.GetComponents<Collider>())
            {
                if (collider is MeshCollider)
                {
                    continue;
                }

                collider.enabled = false;
            }

            if (null == waterMeshCollider)
            {
                waterMeshCollider = WaterSurfaceTransform.gameObject.AddComponent<MeshCollider>();
            }

            if (waterMeshCollider.enabled == false)
                waterMeshCollider.enabled = true;
        }

        private static void RestoreOriginalWaterCollider()
        {
            int nonMeshColliders = 0;
            foreach (Collider collider in WaterSurfaceTransform.GetComponents<Collider>())
            {
                if (collider is MeshCollider)
                {
                    continue;
                }
                nonMeshColliders++;
            }
            if (nonMeshColliders > 0)
            {
                foreach (Collider collider in WaterSurfaceTransform.GetComponents<Collider>())
                {
                    if (collider is MeshCollider)
                    {
                        collider.enabled = false;
                        continue;
                    }
                    collider.enabled = true;
                }
            }
        }

        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        #endregion
    }
#endif
}