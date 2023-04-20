// This script is a Unity Editor tool that updates JSON file paths and references for location data in a project.

using System.IO;
using UnityEditor;
using UnityEngine;

public class JsonFilePathUpdater : MonoBehaviour
{
    /// <summary>
    /// Updates JSON file paths and references for location data in the project.
    /// </summary>
    [MenuItem("Tools/Update JSON file paths")]
    public static void UpdateJsonFilePaths()
    {
        // Set the folder path where the JSON files are located.
        string jsonFilesFolderPath = "Assets/Jsons";
        DirectoryInfo directoryInfo = new DirectoryInfo(jsonFilesFolderPath);
        FileInfo[] jsonFiles = directoryInfo.GetFiles("*.json");

        // Get the project path.
        string projectPath = Directory.GetParent(Application.dataPath).FullName.Replace('\\', '/');

        // Update file paths and references for each JSON file.
        foreach (FileInfo file in jsonFiles)
        {
            string jsonContent = File.ReadAllText(file.FullName);
            LocationData locationData = JsonUtility.FromJson<LocationData>(jsonContent);

            // Update obj, images, and nearbyLocations file paths.
            locationData.obj = UpdateFilePath(locationData.obj, projectPath);
            for (int i = 0; i < locationData.images.Length; i++)
            {
                locationData.images[i] = UpdateFilePath(locationData.images[i], projectPath);
            }
            for (int i = 0; i < locationData.nearbyLocations.Length; i++)
            {
                locationData.nearbyLocations[i] = UpdateFilePath(locationData.nearbyLocations[i], projectPath);
            }

            // Save the updated JSON content.
            string updatedJsonContent = JsonUtility.ToJson(locationData, true);
            File.WriteAllText(file.FullName, updatedJsonContent);
            Debug.Log("Updated JSON file" + file.Name);
        }

        // Update nearby location references for each JSON file.
        foreach (FileInfo file in jsonFiles)
        {
            UpdateNearbyLocationReferences(file);
        }
        Debug.Log("JSON file paths updated.");
    }

    /// <summary>
    /// Updates the file path in the location data to match the new project path.
    /// </summary>
    /// <param name="oldFilePath">The old file path.</param>
    /// <param name="newProjectPath">The new project path.</param>
    /// <returns>The updated file path.</returns>
    private static string UpdateFilePath(string oldFilePath, string newProjectPath)
    {
        int assetsIndex = oldFilePath.IndexOf("/Assets/");
        if (assetsIndex >= 0)
        {
            string relativePath = oldFilePath.Substring(assetsIndex);
            return newProjectPath + relativePath;
        }
        return oldFilePath;
    }

    /// <summary>
    /// Updates nearby location references in the JSON file to ensure bidirectional relationships between locations.
    /// </summary>
    /// <param name="jsonFile">The JSON file containing location data.</param>
    private static void UpdateNearbyLocationReferences(FileInfo jsonFile)
    {
        string jsonContent = File.ReadAllText(jsonFile.FullName);
        LocationData locationData = JsonUtility.FromJson<LocationData>(jsonContent);

        // Check if the current location is referenced in each nearby location.
        foreach (string nearbyLocation in locationData.nearbyLocations)
        {
            string nearbyJsonPath = Path.Combine(jsonFile.DirectoryName, nearbyLocation);
            if (File.Exists(nearbyJsonPath))
            {
                string nearbyJsonContent = File.ReadAllText(nearbyJsonPath);
                LocationData nearbyLocationData = JsonUtility.FromJson<LocationData>(nearbyJsonContent);

                bool referenceExists = false;
                string currentFullPath = jsonFile.FullName.Replace("\\", "/");
                foreach (string referencedLocation in nearbyLocationData.nearbyLocations)
                {
                    if (referencedLocation == currentFullPath)
                    {
                        referenceExists = true;
                        break;
                    }
                }

                // Add a reference to the current location if it doesnt exist.
                if (!referenceExists)
                {
                    int newSize = nearbyLocationData.nearbyLocations.Length + 1;
                    System.Array.Resize(ref nearbyLocationData.nearbyLocations, newSize);
                    nearbyLocationData.nearbyLocations[newSize - 1] = currentFullPath;

                    string updatedNearbyJsonContent = JsonUtility.ToJson(nearbyLocationData, true);
                    File.WriteAllText(nearbyJsonPath, updatedNearbyJsonContent);
                }
            }
        }
    }
}

[System.Serializable]
public class LocationData
{
    public string location;
    public string obj;
    public string[] images;
    public string[] nearbyLocations;
}
