// This script is a Unity Editor tool that enables mesh colliders for all OBJ files in the "Assets/Cutouts" folder.

using UnityEditor;
using System.IO;
using UnityEngine;

public class ColliderGenerator : MonoBehaviour
{
    // Add "Tools/Enable Generate Colliders for OBJs" to the Unity Editor menu.
    [MenuItem("Tools/Enable Generate Colliders for OBJs")]
    public static void EnableGenerateCollidersForObjs()
    {
        // Set the folder path where the OBJ files are located.
        string folderPath = "Assets/Cutouts";

        // Get the directory info for the folder.
        DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

        // Find all OBJ files in the folder.
        FileInfo[] objFiles = dirInfo.GetFiles("*.obj", SearchOption.TopDirectoryOnly);

        // Loop through all the OBJ files.
        for (int i = 0; i < objFiles.Length; i++)
        {
            // Get the asset path for the current OBJ file.
            string objPath = objFiles[i].FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets");

            // Get the ModelImporter for the current OBJ file.
            ModelImporter modelImporter = AssetImporter.GetAtPath(objPath) as ModelImporter;
            if (modelImporter != null)
            {
                // Enable the collider generation for the current OBJ file.
                modelImporter.addCollider = true;

                // Save the changes and reimport the OBJ file.
                modelImporter.SaveAndReimport();

                // Log the change made to the current OBJ file.
                Debug.Log("Generate Colliders enabled for: " + objFiles[i].Name);
            }
        }
        // Log that the process is completed.
        Debug.Log("Done!");
    }
}
