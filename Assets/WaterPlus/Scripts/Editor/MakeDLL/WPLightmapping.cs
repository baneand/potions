using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace WaterPlusEditorInternal
{
    public class ImageData
    {
        public int Width;
        public int Height;

        public Color[] SrcPixels;
        public Color[] DstPixels;

        public string FilePath;
        public string AssetPath;

        public ImageData(string filePath)
        {
            FilePath = filePath;

            if (!File.Exists(FilePath))
            {
                Debug.LogError("lightmap doesn't exist at path: '" + FilePath + "'");
            }

            AssetPath = WpHelper.FilePathToAssetPath(FilePath);

            TextureImporter tImporter = AssetImporter.GetAtPath(AssetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.textureType = TextureImporterType.Default;
                tImporter.sRGBTexture = true;
                tImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    format = TextureImporterFormat.RGBA32
                });
                tImporter.isReadable = true;
                AssetDatabase.ImportAsset(AssetPath);
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath(AssetPath, typeof(Texture2D)) as Texture2D;

            if (null == tex)
            {
                Debug.LogError("Failed to load the lightmap at path: " + AssetPath);
                return;
            }

            Width = tex.width;
            Height = tex.height;

            SrcPixels = tex.GetPixels();
            DstPixels = tex.GetPixels();

            if (tImporter != null)
            {
                tImporter.textureType = TextureImporterType.Lightmap;
                tImporter.sRGBTexture = true;
                tImporter.textureCompression = TextureImporterCompression.CompressedLQ;
                tImporter.isReadable = false;
                AssetDatabase.ImportAsset(AssetPath);
            }

            //WPHelper.MakeTextureReadable( tex, false );

//			dstPixels = new Color[width * height];
//			
//			for (int i = 0; i < width*height; i++) {
//				dstPixels[i] = new Color(.5f, .5f, .5f, 1f);
//			}

//			for (int x = 0; x < width; x++) {
//				//Debug.Log(dstPixels[x].r * 255f + " " + dstPixels[x].g * 255f + " " + dstPixels[x].b * 255f);	
//			}

            Save();
        }

        public void Save()
        {
            //Flip on Y axis first
            Color[] flippedPixels = new Color[Width*Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    int index = y*Width + x;
                    int indexFlipped = (Height - y - 1)*Width + x;

                    flippedPixels[indexFlipped] = DstPixels[index];
                }
            }

            MiniEXR.MiniEXR.MiniEXRWrite(FilePath, (uint) Width, (uint) Height, flippedPixels);
        }
    }

    public class WpLightmapping
    {
        private static Transform s_WaterSurfaceTransform;
        private static string s_LightmapWetnessHeightString, s_LightmapWetnessAmountString;


        private static void BakeLightmaps(WPLightmapData lightmapData)
        {
//			Debug.Log ("waterLevel: " + lightmapData.waterLevel);
//			Debug.Log ("wetnessHeight: " + lightmapData.wetnessHeight);
//			Debug.Log ("wetnessAmount: " + lightmapData.wetnessAmount);
            //
            //Load all of the lightmaps
            ImageData[] imagesData = new ImageData[lightmapData.lightmapFiles.Length];

            for (int lightmapIndex = 0; lightmapIndex < lightmapData.lightmapFiles.Length; lightmapIndex++)
            {
                bool shouldSkipFile = false;
                //Don't load non-used lightmaps
                if (lightmapData.lightmapFiles[lightmapIndex] == null)
                    shouldSkipFile = true;
                else if (lightmapData.lightmapFiles.Length <= 0)
                    shouldSkipFile = true;

                if (shouldSkipFile)
                {
                    Debug.Log("Skipping lightmap " + lightmapIndex + " because the lightmap is non-used");
                    continue;
                }

                string filePath = lightmapData.lightmapFiles[lightmapIndex];
                //string filePath = "C:/Users/Me/Desktop/Water+/iPhone4_hq_1.png";
                //string filePath = "1/LightmapFar-0.exr";
                //string filePath = "C:/Users/Me/waterplus/Assets/WaterPlus/Water_Island/LightmapFar-0.exr";
                //Debug.Log("lightmap path: " + filePath);
                imagesData[lightmapIndex] = new ImageData(filePath);

                /*for (int i = 0; i < imagesData[lightmapIndex].width * imagesData[lightmapIndex].height; i++) {
					imagesData[lightmapIndex].pixels[i].r = imagesData[lightmapIndex].pixels[i].r * imagesData[lightmapIndex].pixels[i].r * .5f;
					imagesData[lightmapIndex].pixels[i].g = imagesData[lightmapIndex].pixels[i].g * imagesData[lightmapIndex].pixels[i].g * 2.0f;
					imagesData[lightmapIndex].pixels[i].b = imagesData[lightmapIndex].pixels[i].b * imagesData[lightmapIndex].pixels[i].b;
				}*/

                //Debug.Log("Successfully loaded lightmap at path " + filePath);
                //break;
            }

            //Debug.Log ("Successfully loaded lightmaps.");

            //return;

            System.DateTime bakeStartTime = System.DateTime.Now;

            //int count = 0;

            //1. Go over all pixels of the mesh
            //2. Convert UV to vertex
            //3. If the vertex is within water line, update the lightmap.
            //Debug.Log ("Updating lightmaps.");
            //int totalObjectsToBake = lightmapData.meshes.Length + lightmapData.terrains.Length;

            //
            //Regular meshes
            foreach (WPMesh obj in lightmapData.meshes)
            {
                if (obj.lightmapIndex < 0 || obj.lightmapIndex >= imagesData.Length)
                {
                    Debug.Log("skipping " + obj + " because of a wrong lightmapIndex: " + obj.lightmapIndex);
                    continue;
                }

                //Calculate object's texel size
                ImageData lightmapUsed = imagesData[obj.lightmapIndex];
                int lightmapResolution = lightmapUsed.Width;
                Vector4 tilingOffset = obj.tilingOffset;

                if (lightmapResolution == 0)
                {
                    Debug.Log("error: lightmapResolution == 0");
                    continue;
                }

                if (tilingOffset.x == 0)
                {
                    Debug.LogError("error: tilingOffset.x == 0");
                    continue;
                }

                float pixelSize = 1.0f/(tilingOffset.x*lightmapResolution);

                //Debug.Log("tilingOffset: " + tilingOffset.ToString() + " " + lightmapResolution);

                //Console

                //Debug.Log("2");

                Vector2[] meshUVs = obj.uvs;
                if (meshUVs == null)
                {
                    Debug.Log("No UVs found for " + obj.name);
                    continue;
                }

                //Debug.Log("pixelSize: " + pixelSize);
                Matrix4x4 tempMatrix = new Matrix4x4();
                tempMatrix.SetRow(0, obj.localToWorldMatrix[0]);
                tempMatrix.SetRow(1, obj.localToWorldMatrix[1]);
                tempMatrix.SetRow(2, obj.localToWorldMatrix[2]);
                tempMatrix.SetRow(3, obj.localToWorldMatrix[3]);
                //Debug.Log( tempMatrix.GetRow(3) );
                //Debug.Log("1 * matrix: " + tempMatrix.MultiplyPoint(Vector3.one) );


                for (float u = 0.0f; u < 1.0f; u += pixelSize)
                {
                    //Progress for large objects
                    //if (1.0f / (pixelSize * pixelSize) >= 10000.0f) {
                    //Make sure to log only every percent, not less
//					if (u - previousProgress >= .01f) {
//						Console.Write("\r\t" + obj.name + " progress: " + (u * 100.0f).ToString("0") + "%");
//						previousProgress = u;
//					}
                    //}

                    for (float v = 0.0f; v < 1.0f; v += pixelSize)
                    {
                        //totalPixels++;

                        //Convert UV to vertex position
                        Vector2 currentUv = new Vector2(u, v);
                        bool vertexFound;
                        Vector3 vertexPos;
                        WpHelper.UvToVertex(currentUv, obj, meshUVs, out vertexFound, out vertexPos);

                        //Update the lightmap
                        //vertexFound = true;
                        if (vertexFound)
                        {
                            vertexPos = tempMatrix.MultiplyPoint(vertexPos);

                            //if (u >= .45f && u <= .55f)
                            //if (1.0f / (pixelSize * pixelSize) >= 10000.0f)
                            //	Console.Write(vertexPos.y + "\t");

                            //Debug.Log("vertexPos.y: " + vertexPos.y);
                            if (vertexPos.y <= lightmapData.waterLevel + lightmapData.wetnessHeight)
                            {
                                //Convert object's UV to lightmap's UV
                                Vector2 lightmapUv = currentUv;
                                lightmapUv.x *= tilingOffset.x;
                                lightmapUv.y *= tilingOffset.y;

                                lightmapUv.x += tilingOffset.z;
                                lightmapUv.y += tilingOffset.w;

                                int lightmapX = (int) (lightmapUv.x*lightmapResolution);
                                int lightmapY = (int) (lightmapUv.y*lightmapResolution);


                                if (lightmapX < 0 || lightmapX >= lightmapResolution || lightmapY < 0 ||
                                    lightmapY >= lightmapResolution)
                                {
                                    //Debug.LogWarning("lightmapX: " + lightmapX + " lightmapY: " + lightmapY);
                                }
                                else
                                {
                                    //lightmapPixels[lightmapY * lightmapResolution + lightmapX] = Color.yellow;
                                    float gradientAmount;

                                    if (vertexPos.y <= lightmapData.waterLevel)
                                    {
                                        gradientAmount = 1.0f;
                                    }
                                    else if (vertexPos.y > lightmapData.waterLevel + lightmapData.wetnessHeight)
                                    {
                                        gradientAmount = 0.0f;
                                    }
                                    else
                                    {
                                        gradientAmount = 1.0f -
                                                         (vertexPos.y - lightmapData.waterLevel)/
                                                         lightmapData.wetnessHeight;
                                    }

                                    gradientAmount = 1.0f - gradientAmount*lightmapData.wetnessAmount;

                                    gradientAmount = Mathf.Clamp01(gradientAmount); //Just in case

                                    lightmapUsed.DstPixels[lightmapY*lightmapResolution + lightmapX].r =
                                        lightmapUsed.SrcPixels[lightmapY*lightmapResolution + lightmapX].r*
                                        gradientAmount;
                                    lightmapUsed.DstPixels[lightmapY*lightmapResolution + lightmapX].g =
                                        lightmapUsed.SrcPixels[lightmapY*lightmapResolution + lightmapX].g*
                                        gradientAmount;
                                    lightmapUsed.DstPixels[lightmapY*lightmapResolution + lightmapX].b =
                                        lightmapUsed.SrcPixels[lightmapY*lightmapResolution + lightmapX].b*
                                        gradientAmount;
                                }
                            }
                        }
                    }
                }
            }

            //Terrains
            foreach (WPTerrainData obj in lightmapData.terrains)
            {
                if (obj.lightmapIndex < 0 || obj.lightmapIndex >= imagesData.Length)
                {
                    Debug.Log("skipping " + obj + " because of a wrong lightmapIndex: " + obj.lightmapIndex);
                    continue;
                }

                //Calculate object's texel size
                ImageData lightmapUsed = imagesData[obj.lightmapIndex];
                int lightmapResolution = lightmapUsed.Width;

                if (lightmapResolution == 0)
                {
                    Debug.Log("error: lightmapResolution == 0");
                    continue;
                }

                float pixelSize = 1.0f/lightmapResolution;

                for (float u = 0.0f; u < 1.0f; u += pixelSize)
                {
                    for (float v = 0.0f; v < 1.0f; v += pixelSize)
                    {
                        //Update the lightmap
                        int heightmapX = (int) (v*obj.height);
                        int heightmapY = (int) (u*obj.width);

                        if (heightmapX < 0 || heightmapX >= obj.width || heightmapY < 0 || heightmapY >= obj.height)
                            continue;

                        int lightmapX = (int) (u*lightmapResolution);
                        int lightmapY = (int) (v*lightmapResolution);

                        if (lightmapX < 0 || lightmapX >= lightmapResolution || lightmapY < 0 ||
                            lightmapY >= lightmapResolution)
                            continue;

                        float yPos = obj.position.y + obj.heightmap[heightmapY*obj.height + heightmapX];

                        if (yPos <= lightmapData.waterLevel + lightmapData.wetnessHeight)
                        {
                            //if (yPos > 0)
                            //	Debug.Log("yPos: " + yPos + " waterLevel: " + lightmapData.waterLevel);

                            float gradientAmount;

                            if (yPos <= lightmapData.waterLevel)
                            {
                                gradientAmount = 1.0f;
                            }
                            else if (yPos > lightmapData.waterLevel + lightmapData.wetnessHeight)
                            {
                                gradientAmount = 0.0f;
                            }
                            else
                            {
                                gradientAmount = 1.0f - (yPos - lightmapData.waterLevel)/lightmapData.wetnessHeight;
                            }

                            gradientAmount = 1.0f - gradientAmount*lightmapData.wetnessAmount;

                            gradientAmount = Mathf.Clamp01(gradientAmount); //Just in case

                            lightmapUsed.DstPixels[lightmapY*lightmapResolution + lightmapX].r =
                                lightmapUsed.SrcPixels[lightmapY*lightmapResolution + lightmapX].r*gradientAmount;
                            lightmapUsed.DstPixels[lightmapY*lightmapResolution + lightmapX].g =
                                lightmapUsed.SrcPixels[lightmapY*lightmapResolution + lightmapX].r*gradientAmount;
                            lightmapUsed.DstPixels[lightmapY*lightmapResolution + lightmapX].b =
                                lightmapUsed.SrcPixels[lightmapY*lightmapResolution + lightmapX].r*gradientAmount;
                        }
                    }
                }
            }


            //Save all the lightmaps
            foreach (ImageData data in imagesData)
            {
                if (null == data)
                    continue;
                data.Save();
            }

            Debug.Log("Successfully updated the lightmaps in " + (System.DateTime.Now - bakeStartTime).TotalSeconds +
                      " seconds.");
        }

        private static WPLightmapData PrepareLightmapData(string[] lightmapPaths, GameObject[] affectedObjects,
            float waterLevel, float wetnessHeight, float wetnessAmount)
        {
            //Debug.Log("waterLevel: " + waterLevel);
            WPLightmapData wpLightmapData = new WPLightmapData
            {
                waterLevel = waterLevel,
                wetnessHeight = wetnessHeight,
                wetnessAmount = wetnessAmount,
                lightmapFiles = new string[lightmapPaths.Length]
            };


            for (int i = 0; i < lightmapPaths.Length; i++)
            {
                wpLightmapData.lightmapFiles[i] = lightmapPaths[i];
                //Debug.Log("Adding lightmap: " + lightmapPaths[i]);
            }

            //
            //Normal meshes
            List<WPMesh> wpMeshes = new List<WPMesh>();

            foreach (GameObject obj in affectedObjects)
            {
                Renderer rend = obj.GetComponent<Renderer>();

                if (rend == null)
                    continue;

                //Skip terrains for now
                Terrain terrain = obj.GetComponent<Terrain>();
                if (terrain != null)
                {
                    Debug.LogWarning("skipping terrain");
                    continue;
                }

                if (rend.lightmapIndex < 0 || rend.lightmapIndex >= LightmapSettings.lightmaps.Length)
                    continue;

                MeshFilter meshFilter = rend.gameObject.GetComponent<MeshFilter>();

                if (meshFilter == null)
                    continue;

                Mesh mesh = meshFilter.sharedMesh;

                WPMesh wpMesh = new WPMesh();

                string objName = rend.name;
                if (objName == "default")
                {
                    objName = obj.gameObject.name;
                }

                if (objName == "default")
                {
                    objName = obj.gameObject.transform.parent.gameObject.name;
                }

                if (null == mesh)
                {
                    Debug.LogError("mesh of " + objName + " is null. Skipping.");
                    continue;
                }

                wpMesh.name = objName;

                wpMesh.lightmapIndex = rend.lightmapIndex;
                wpMesh.tilingOffset = rend.lightmapScaleOffset;

                wpMesh.localToWorldMatrix = new Vector4[4];
                wpMesh.localToWorldMatrix[0] = obj.transform.localToWorldMatrix.GetRow(0);
                wpMesh.localToWorldMatrix[1] = obj.transform.localToWorldMatrix.GetRow(1);
                wpMesh.localToWorldMatrix[2] = obj.transform.localToWorldMatrix.GetRow(2);
                wpMesh.localToWorldMatrix[3] = obj.transform.localToWorldMatrix.GetRow(3);

                //Debug.LogWarning("1 * matrix: " + obj.transform.localToWorldMatrix.MultiplyVector(Vector3.one) );

                wpMesh.vertexCount = mesh.vertexCount;
                wpMesh.vertices = mesh.vertices;

                if (mesh.uv2 == null)
                {
                    wpMesh.uvs = mesh.uv;
                }
                else
                {
                    wpMesh.uvs = mesh.uv2.Length <= 0 ? mesh.uv : mesh.uv2;
                }

                wpMesh.triangles = mesh.triangles;

                if (wpMesh.vertices == null)
                {
                    Debug.LogWarning("No vertices found for " + objName + ". Skipping.");
                    continue;
                }
                else
                {
                    if (wpMesh.vertices.Length <= 0)
                    {
                        Debug.LogWarning("No vertices found for " + objName + ". Skipping.");
                        continue;
                    }
                }

                if (wpMesh.uvs == null)
                {
                    Debug.LogWarning("No UVs found for " + objName + ". Skipping.");
                    continue;
                }
                else
                {
                    if (wpMesh.uvs.Length <= 0)
                    {
                        Debug.LogWarning("No UVs found for " + objName + ". Skipping.");
                        continue;
                    }
                }

                wpMeshes.Add(wpMesh);
            }

            wpLightmapData.meshes = wpMeshes.ToArray();

            //
            //Terrains meshes
            List<WPTerrainData> wpTerrains = new List<WPTerrainData>();

            foreach (GameObject obj in affectedObjects)
            {
                //break;

                Terrain terrain = obj.GetComponent<Terrain>();

                if (terrain == null)
                    continue;

                //Debug.LogWarning("Adding terrain");

                /*Renderer rend = obj.renderer;
				
				if (rend == null) {
					Debug.LogWarning("No renderer found for the terrain.");
					continue;
				}*/

                TerrainData terrainData = terrain.terrainData;

                //Debug.Log("terrainData.size.y: " + terrainData.size.y);

                //Debug.Log("terrain resolution: " + terrainData.heightmapResolution);

                if (terrain.lightmapIndex < 0 || terrain.lightmapIndex >= LightmapSettings.lightmaps.Length)
                    continue;

                string objName = obj.gameObject.name;
                if (objName == "default")
                {
                    objName = obj.gameObject.name;
                }

                if (objName == "default")
                {
                    objName = obj.gameObject.transform.parent.gameObject.name;
                }

                WPTerrainData wpTerrainData = new WPTerrainData
                {
                    name = objName,
                    width = terrainData.heightmapWidth,
                    height = terrainData.heightmapHeight,
                    lightmapIndex = terrain.lightmapIndex,
                    position = obj.transform.position
                };
                //wpTerrainData.tilingOffset = rend.lightmapTilingOffset;

                float[,] tempHeightmap2D = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                    terrainData.heightmapHeight);

                if (tempHeightmap2D == null)
                {
                    Debug.LogWarning("No heightmap found for " + objName + ". Skipping.");
                    continue;
                }
                else
                {
                    if (tempHeightmap2D.Length <= 0)
                    {
                        Debug.LogWarning("No heightmap found for " + objName + ". Skipping.");
                        continue;
                    }
                }

                //Convert the heightmap
                float[] tempHeightmap1D = new float[wpTerrainData.width*wpTerrainData.height];

                for (int x = 0; x < wpTerrainData.width; x++)
                {
                    for (int y = 0; y < wpTerrainData.height; y++)
                    {
                        tempHeightmap1D[y*wpTerrainData.width + x] = tempHeightmap2D[x, y]*terrainData.size.y;
                    }
                }

                wpTerrainData.heightmap = tempHeightmap1D;

                wpTerrains.Add(wpTerrainData);
            }

            wpLightmapData.terrains = wpTerrains.ToArray();

            return wpLightmapData;
        }

        private static GameObject[] BuildListOfAffectedObjects(float wetnessLineY)
        {
            List<GameObject> affectedObjects = new List<GameObject>();

            foreach (var o in Object.FindObjectsOfType(typeof(GameObject)))
            {
                var obj = (GameObject) o;
                //Skip non-static objects
                if (!obj.gameObject.isStatic)
                    continue;

                Renderer rend = obj.GetComponent<Renderer>();

                if (rend == null)
                {
                    if (obj.GetComponent<Terrain>() == null)
                        continue;
                }
                else
                {
                    //Skip objects above the water line
                    if (rend.bounds.min.y > wetnessLineY)
                        continue;
                }

                affectedObjects.Add(obj);
            }

            return affectedObjects.ToArray();
        }

        private static string[] PrepareLightmaps(GameObject[] affectedObjects)
        {
            //
            //Duplicate original lightmaps
            List<int> lightmapsToBackup = new List<int>();

            foreach (GameObject obj in affectedObjects)
            {
                int lightmapIndex = -1;

                if (obj.GetComponent<Renderer>() != null)
                {
                    lightmapIndex = obj.GetComponent<Renderer>().lightmapIndex;
                }
                else
                {
                    Terrain terrain = obj.GetComponent<Terrain>();
                    if (terrain != null)
                    {
                        lightmapIndex = terrain.lightmapIndex;
                    }
                }

                if (lightmapIndex < 0)
                    continue;

                //Make sure that we keep only lightmapped objects
                if (lightmapIndex < 0 || lightmapIndex >= LightmapSettings.lightmaps.Length)
                    continue;

                if (!lightmapsToBackup.Contains(lightmapIndex))
                {
                    if (LightmapSettings.lightmaps[lightmapIndex] != null)
                        lightmapsToBackup.Add(lightmapIndex);
                }

                //Find to what triangle does the point belong to
                //UpdateLightmap(obj);
            }

            string[] lightmapPaths = new string[LightmapSettings.lightmaps.Length];

            int lightmapsCount = 0;

            foreach (int lightmapIndex in lightmapsToBackup)
            {
                Texture2D lightmapFar = LightmapSettings.lightmaps[lightmapIndex].lightmapColor;

                if (lightmapFar == null)
                    continue;

                string lightMapAssetPath = AssetDatabase.GetAssetPath(lightmapFar);

                //Debug.Log("lightMapAssetPath for " + lightmapIndex + " is " + lightMapAssetPath);

                if (lightMapAssetPath == null)
                    continue;

                if (lightMapAssetPath.Length <= 0)
                    continue;

                if (WpHelper.HasSuffix(lightMapAssetPath, "__WP"))
                {
                    string originalLightmapPath = WpHelper.RemoveSuffixFromFilename(lightMapAssetPath, "__WP");
                    //Debug.Log("WaterPlus lightmap already exists. Will be using the original one at path " + originalLightmapPath);

                    if (!File.Exists(originalLightmapPath))
                    {
                        Debug.LogError("Cannot find the original lightmap at path " + originalLightmapPath +
                                       ". Aborting");
                        return null;
                    }

                    lightMapAssetPath = originalLightmapPath;
                }

                string waterPlusLightmapAssetPath = WpHelper.AddSuffixToFilename(lightMapAssetPath, "__WP");

                AssetDatabase.DeleteAsset(waterPlusLightmapAssetPath);
                AssetDatabase.CopyAsset(lightMapAssetPath, waterPlusLightmapAssetPath);

                AssetDatabase.ImportAsset(waterPlusLightmapAssetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();

                lightmapPaths[lightmapIndex] = WpHelper.AssetPathToFilePath(waterPlusLightmapAssetPath);

                lightmapsCount++;

                //LightmapSettings.lightmaps[ lightmapIndex ].lightmapFar = AssetDatabase.LoadAssetAtPath( waterPlusLightmapAssetPath,
                //																		typeof(Texture2D) ) as Texture2D;
            }

            //Load the new lightmaps
            LightmapData[] lightmapsData = new LightmapData[LightmapSettings.lightmaps.Length];

            for (int i = 0; i < lightmapPaths.Length; i++)
            {
                bool shouldUseOriginalLm = false;

                if (lightmapPaths[i] == null)
                    shouldUseOriginalLm = true;
                else if (lightmapPaths[i].Length <= 0)
                    shouldUseOriginalLm = true;

                if (!shouldUseOriginalLm)
                {
                    LightmapData lmData = new LightmapData
                    {
                        lightmapColor =
                            AssetDatabase.LoadAssetAtPath(WpHelper.FilePathToAssetPath(lightmapPaths[i]),
                                typeof(Texture2D))
                                as Texture2D
                    };
                    if (lmData.lightmapColor == null)
                    {
                        Debug.LogWarning("lmData.lightmapFar == null for " +
                                         WpHelper.FilePathToAssetPath(lightmapPaths[i]));
                        lmData.lightmapColor = LightmapSettings.lightmaps[i].lightmapColor;
                    }

                    //if (LightmapSettings.lightmaps[i].lightmapNear != null)
                    lmData.lightmapDir = LightmapSettings.lightmaps[i].lightmapDir;

                    lightmapsData[i] = lmData;
                }
                else
                {
                    lightmapsData[i] = LightmapSettings.lightmaps[i];
                }
            }

            LightmapSettings.lightmaps = lightmapsData;

            if (lightmapsCount <= 0)
            {
                Debug.LogError("Nothing to bake - no lightmaps found.");
                return null;
            }

            return lightmapPaths;
        }

        public static void UpdateLightmaps(Transform waterSurfaceTransform, string lightmapWetnessHeightString,
            string lightmapWetnessAmountString)
        {
            s_WaterSurfaceTransform = waterSurfaceTransform;
            s_LightmapWetnessHeightString = lightmapWetnessHeightString;
            s_LightmapWetnessAmountString = lightmapWetnessAmountString;

            if (s_WaterSurfaceTransform == null)
            {
                Debug.LogError("Please assign a water surface first.");
                return;
            }

            float wetnessHeight;
            float wetnessAmount;


            if (!float.TryParse(s_LightmapWetnessHeightString, out wetnessHeight))
            {
                Debug.LogError("Please enter a correct value into the 'Lightmap wetness height' field.");
                return;
            }

            if (!float.TryParse(s_LightmapWetnessAmountString, out wetnessAmount))
            {
                Debug.LogError("Please enter a correct value into the 'Lightmap wetness amount' field.");
                return;
            }

            if (LightmapSettings.lightmaps.Length <= 0)
            {
                Debug.LogError("No lightmaps found. Please bake lightmaps first before updating them in Water+.");
                return;
            }

            //WriteBakeSettings();
            wetnessAmount = Mathf.Clamp(wetnessAmount, 0.0f, 0.7f);


            //Debug.Log("waterHeight: " + wetnessHeight + " maxWetness: " + wetnessAmount);

            float wetnessLineY = s_WaterSurfaceTransform.GetComponent<Renderer>().bounds.max.y + wetnessHeight;

            GameObject[] affectedObjects = BuildListOfAffectedObjects(wetnessLineY);

            string[] lightmapPaths = PrepareLightmaps(affectedObjects);

            if (null == lightmapPaths)
                return;

            WPLightmapData wpLightmapData = PrepareLightmapData(lightmapPaths, affectedObjects,
                s_WaterSurfaceTransform.GetComponent<Renderer>().bounds.max.y, wetnessHeight, wetnessAmount);

            BakeLightmaps(wpLightmapData);


            //Debug.Log("lightmapPaths.Length: " + lightmapPaths.Length);
//			ExportLightmapsDataToXML(xmlPath, lightmapPaths, affectedObjects.ToArray(), waterSurfaceTransform.renderer.bounds.max.y, wetnessHeight, wetnessAmount);
//			
//			Debug.Log("Sending lightmaps data to the external baker. xmlPath:" + xmlPath);
//			

            s_WaterSurfaceTransform.GetComponent<Renderer>()
                .sharedMaterial.SetFloat("_refractionsWetness", 1.0f - wetnessAmount*.5f);
        }
    }
}