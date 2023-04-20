//This script is a Unity Editor Tool that changes the texture shape of all images in the specified folder to TextureCube for use as a skybox.

using UnityEditor;
using UnityEngine;

public class TextureShapeChanger : EditorWindow
{
    /// <summary>
    /// Change the texture shape of all images in the specified folder to TextureCube.
    /// </summary>
    [MenuItem("Tools/Change Texture Shape to Cube")]
    public static void ChangeTextureShapeToCube()
    {
        string folderPath = "Assets/Images"; // Change this to the folder path containing your images

        // Find all Texture2D assets in the specified folder
        string[] guids = AssetDatabase.FindAssets("t:texture2D", new[] { folderPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            // Change the texture shape to TextureCube if it's not already set
            if (importer != null && importer.textureShape != TextureImporterShape.TextureCube)
            {
                importer.textureShape = TextureImporterShape.TextureCube;
                AssetDatabase.ImportAsset(assetPath);
            }
        }

        Debug.Log("Texture shape changed to Cube for all images in the folder.");
    }
}
