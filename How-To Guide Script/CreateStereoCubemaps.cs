using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateStereoCubemaps : MonoBehaviour
{
    public RenderTexture cubemapLeft;
    public RenderTexture cubemapRight;
    public RenderTexture equirect;
    public bool renderStereo = true;
    public float stereoSeparation = 0.064f;

    void Start()
    {
        Camera cam = GetComponent<Camera>();

        if (cam == null)
        {
            cam = GetComponentInParent<Camera>();
        }

        if (cam == null)
        {
            Debug.Log("stereo 360 capture node has no camera or parent camera");
        }

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

        if (equirect == null)
        {
            return;
        }

        if (renderStereo)
        {
            cubemapLeft.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Left);
            cubemapRight.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Right);
        }
        else
        {
            cubemapLeft.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Mono);
        }

        Texture2D toTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(4096, 4096, TextureFormat.ARGB32, false);
            // ReadPixels looks at the active RenderTexture.
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
        File.WriteAllBytes(AssetDatabase.GenerateUniqueAssetPath(Application.dataPath + "/Images/leftImage.png"), leftTexture.EncodeToPNG());

        // Create and save right eye texture
        Texture2D rightTexture = new Texture2D(myTexture.width, (myTexture.height) / 2);
        Rect rightRect = new Rect(0, 2048, 4096, 2048);
        rightTexture.SetPixels(myTexture.GetPixels((int)rightRect.x, (int)rightRect.y, (int)rightRect.width, (int)rightRect.height));
        rightTexture.Apply();
        File.WriteAllBytes(AssetDatabase.GenerateUniqueAssetPath(Application.dataPath + "/Images/rightImage.png"), rightTexture.EncodeToPNG());
    }
}
