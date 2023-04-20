// This script handles teleportation to new OBJ files when the user clicks on floating spheres in a Unity project.

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static RandomObjLoader;

public class TeleportToObj : MonoBehaviour
{
    private RandomObjLoader randomObjLoader;
    public GameObject floatingSpherePrefab;
    private List<GameObject> floatingSpheres;
    private string currentObjName;

    void Start()
    {
        randomObjLoader = GameObject.Find("RandomObjLoader").GetComponent<RandomObjLoader>();
    }

    public void AssignCurrentObjName(string name)
    {
        currentObjName = name;
    }

    private void Update()
    {
        // Check for user input to teleport to a new location.
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (Path.HasExtension(hit.transform.name))
                {
                    string newObjName = Path.GetFileNameWithoutExtension(hit.transform.name);
                    GameObject currentObj = GameObject.Find(currentObjName);
                    string newObjPath = "Assets/Cutouts/" + newObjName + ".obj";
                    TeleportToLocation(currentObj, newObjPath);
                    AssignCurrentObjName(newObjName + "(Clone)");
                }
            }
        }
    }

    /// <summary>
    /// Teleports the camera to a new location and loads the associated OBJ file.
    /// </summary>
    /// <param name="currentObj">The current GameObject.</param>
    /// <param name="newObjPath">The path to the new OBJ file to load.</param>
    public void TeleportToLocation(GameObject currentObj, string newObjPath)
    {
        GameObject objPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(newObjPath);
        if (objPrefab == null)
        {
            Debug.LogError("Failed to load OBJ file: " + newObjPath);
            return;
        }

        // Destroy the current object and floating spheres.
        if (currentObj != null)
        {
            Destroy(currentObj);
            ClearFloatingSpheres();
        }

        LocationData locationData = null;

        string newJsonPath = "Assets/Jsons/" + Path.GetFileNameWithoutExtension(newObjPath) + ".json";
        if (File.Exists(newJsonPath))
        {
            string jsonContent = File.ReadAllText(newJsonPath);
            locationData = JsonUtility.FromJson<LocationData>(jsonContent);
        }

        // Instantiate the new object and teleport the camera.
        currentObj = Instantiate(objPrefab);
        randomObjLoader.TeleportCameraAboveSurface(currentObj);
        SpawnFloatingSpheres(locationData);
    }

    /// <summary>
    /// Spawns floating spheres representing nearby locations.
    /// </summary>
    /// <param name="locationData">The LocationData for the current location.</param>
    public void SpawnFloatingSpheres(LocationData locationData)
    {
        floatingSpheres = new List<GameObject>();
        string currentLocation = locationData.location;
        string pattern = @"Location_(-?\d+)_(-?\d+)";
        int currentX = 0;
        int currentZ = 0;

        // Extract the current locations coordinates.
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

        // Spawn floating spheres for each nearby location.
        for (int i = 0; i < locationData.nearbyLocations.Length; i++)
        {
            // Extract x and z values from the file name
            string fullPath = locationData.nearbyLocations[i];
            string fileName = Path.GetFileNameWithoutExtension(fullPath);

            match = Regex.Match(fileName, pattern);
            if (match.Success)
            {
                int x = int.Parse(match.Groups[1].Value);
                int z = int.Parse(match.Groups[2].Value);

                // Calculate the position based on the comparison of x and z values
                float posX = (x > currentX) ? 3f : (x < currentX) ? -3f : 0f;
                float posZ = (z > currentZ) ? 3f : (z < currentZ) ? -3f : 0f;

                Vector3 position = Camera.main.transform.position + new Vector3(posX, 1f, posZ);

                // Instantiate and configure the sphere
                GameObject sphere = Instantiate(floatingSpherePrefab);
                sphere.name = locationData.nearbyLocations[i];
                sphere.transform.position = position;
                floatingSpheres.Add(sphere);
            }
            else
            {
                Debug.LogError($"Failed to extract coordinates from file name: {fileName}");
            }
        }
    }

    /// <summary>
    /// Destroys all floating spheres in the scene.
    /// </summary>
    public void ClearFloatingSpheres()
    {
        if (floatingSpheres != null)
        {
            foreach (GameObject sphere in floatingSpheres)
            {
                Destroy(sphere);
            }
        }
    }
}
