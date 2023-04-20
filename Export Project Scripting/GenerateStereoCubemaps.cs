// This Editor script generates stereo cubemaps from 3D models in the "Assets/OppositeCutouts" folder and saves the resulting stereo images as PNG files.

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class GenerateStereoCubemaps : MonoBehaviour
{
    // Static variables for render textures and stereo settings
    static RenderTexture cubemapLeft;
    static RenderTexture cubemapRight;
    static RenderTexture equirect;
    static bool renderStereo = true;
    static float stereoSeparation = 0.064f;

    // Add a menu item in Unity to generate stereo cubemap images
    [MenuItem("Tools/Generate Stereo Cubemap Images")]
    public static void Generate()
    {
        // Get the main camera
        Camera cam = Camera.main;

        //Assign renderTextures
        cubemapLeft = AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/RenderTextures/cubemap_left.renderTexture");
        cubemapRight = AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/RenderTextures/cubemap_right.renderTexture");
        equirect = AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/RenderTextures/equirect.renderTexture");

        GameObject def = GameObject.Find("default");
        DestroyImmediate(def);
        
        //Create a for loop that iterates through all objs in OppositeCutouts
        // Get all the asset GUIDs in the 'OppositeCutouts' folder
        string[] guids = AssetDatabase.FindAssets("", new[] { "Assets/InvertedCutouts" });

        // Iterate through each GUID and retrieve the corresponding asset path
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Check if the file is an .obj file
            if (path.EndsWith(".obj"))
            {
                Debug.Log("Found OBJ file: " + path);

                //Add in opposite obj at exact same position as def
                GameObject objPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (objPrefab == null)
                {
                    Debug.LogError("Failed to load OBJ file: " + path);
                    return;
                }
                
                //Get x and z coordinates from file name
                string currentLocation = objPrefab.name.Replace("(Clone)", "");
                string pattern = @"Location_(-?\d+)_(-?\d+)";
                int currentX = 0;
                int currentZ = 0;

                Match match = Regex.Match(currentLocation, pattern);
                if (match.Success)
                {
                    currentX = int.Parse(match.Groups[1].Value);
                    currentZ = int.Parse(match.Groups[2].Value);
                }
                else
                {
                    Debug.LogError($"Failed to extract coordinates from file name: {currentLocation}");
                }

                // Set up the instantiated object and camera positions
                GameObject newObj = Instantiate(objPrefab);
                Vector3 offset = new Vector3(-currentX, newObj.transform.position.y + 120, currentZ);
                newObj.transform.position = new Vector3(0,0,0);
                cam.transform.position = offset;

                if (cam == null)
                {
                    Debug.Log("stereo 360 capture node has no camera or parent camera");
                }

                // Render cubemaps for the left and right eyes
                if (renderStereo)
                {
                    cam.stereoSeparation = stereoSeparation;
                    cam.RenderToCubemap(cubemapLeft, 63, Camera.MonoOrStereoscopicEye.Left);
                    cam.RenderToCubemap(cubemapRight, 63, Camera.MonoOrStereoscopicEye.Right);
                }
                else
                {
                    cam.RenderToCubemap(cubemapLeft, 63, Camera.MonoOrStereoscopicEye.Mono);
                }

                //optional: convert cubemaps to equirect

                // Return early if equirect is not assigned
                if (equirect == null)
                {
                    return;
                }

                // Convert cubemaps to equirectangular format based on the rendering mode (stereo or mono)
                if (renderStereo)
                {
                    cubemapLeft.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Left);
                    cubemapRight.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Right);
                }
                else
                {
                    cubemapLeft.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Mono);
                }

                // Convert the RenderTexture to a Texture2D
                Texture2D toTexture2D(RenderTexture rTex)
                {
                    Texture2D tex = new Texture2D(4096, 4096, TextureFormat.ARGB32, false);
                    RenderTexture.active = rTex;
                    tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
                    tex.Apply();
                    return tex;
                }
                Texture2D myTexture = toTexture2D(equirect);

                // Create and save left eye texture
                Texture2D leftTexture = new Texture2D(myTexture.width, (myTexture.height) / 2);
                Rect leftRect = new Rect(0, 0, 4096, 2048);
                leftTexture.SetPixels(myTexture.GetPixels((int)leftRect.x, (int)leftRect.y, (int)leftRect.width, (int)leftRect.height));
                leftTexture.Apply();
                File.WriteAllBytes((Application.dataPath + "/Images/Location_" + currentX + "_" + currentZ + "_leftImage.png"), leftTexture.EncodeToPNG());

                // Create and save right eye texture
                Texture2D rightTexture = new Texture2D(myTexture.width, (myTexture.height) / 2);
                Rect rightRect = new Rect(0, 2048, 4096, 2048);
                rightTexture.SetPixels(myTexture.GetPixels((int)rightRect.x, (int)rightRect.y, (int)rightRect.width, (int)rightRect.height));
                rightTexture.Apply();
                File.WriteAllBytes((Application.dataPath + "/Images/Location_" + currentX + "_" + currentZ + "_rightImage.png"), rightTexture.EncodeToPNG());

                //Remove instantiated object
                DestroyImmediate(newObj);
            }
        }
    }
}
