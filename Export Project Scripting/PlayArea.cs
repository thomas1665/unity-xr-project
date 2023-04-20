/// <summary>
/// This script is used to generate a play area consisting of multiple cubes with
/// associated data such as location, .obj file and image paths. It also manages
/// the creation of JSON files for each cube and checks if cubes can teleport between each other.
/// </summary>

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Represents location-related data in a JSON file, such as the location name, .obj file path,
/// and associated images and nearby locations.
/// </summary>
[System.Serializable]
public class LocationData
{
    public string location;
    public string obj;
    public string[] images;
    public string[] nearbyLocations;
}

public class PlayArea : MonoBehaviour
{
    public GameObject area;
    public int MinHeight;
    private int Width = 1000;
    private int Height = 1000;
    private int Depth = 1000;
    public int space = 100;
    private float raycastDistance = 300f;
    private GameObject def;
    private GeometryCutAndSave gcs;

    private void Awake()
    {
        gcs = gameObject.AddComponent<GeometryCutAndSave>();
        def = GameObject.Find("default");
    }

    /// <summary>
    /// Determines the positions for cubes to be placed and spawns them in the scene.
    /// </summary>
    public void PlaceAreas()
    {
        HashSet<Vector3> placedPositions = new HashSet<Vector3>();

        Bounds defaultBounds = def.GetComponent<MeshRenderer>().bounds;
        float maxX = defaultBounds.max.x - 100;
        float maxZ = defaultBounds.max.z - 100;

        for (int x = -Width; x < Width; x += space)
        {
            for (int y = MinHeight; y < Height; y += space)
            {
                for (int z = -Depth; z < Depth; z += space)
                {
                    if (x > -Width && x < Width - 1
                        && y > MinHeight && y < Height - 1 &&
                        z > -Depth && z < Depth - 1 &&
                        x < maxX && z < maxZ)
                    {
                        Vector3 position = PositionRaycast(x, y, z);
                        if (!placedPositions.Contains(position) && position.y > MinHeight && position.x % 10 == 0 && position.z % 10 == 0)
                        {
                            placedPositions.Add(position);
                            SpawnCube(position);
                        }
                    }
                }
            }
        }
        CheckNearbyCubes();
    }

    /// <summary>
    /// Performs a raycast from the given coordinates to find a valid position for a cube.
    /// </summary>
    /// <param name="x">X-Coordinate</param>
    /// <param name="y">Y-Coordinate</param>
    /// <param name="z">Z-Coordinate</param>
    /// <returns>The valid position for a cube.</returns>
    Vector3 PositionRaycast(int x, int y, int z)
    {
        RaycastHit hit;
        Vector3 position = Vector3.zero;
        if (Physics.Raycast(new Vector3(x, y, z), Vector3.down, out hit, raycastDistance) && y > MinHeight)
        {
            position = hit.point;
            transform.position = hit.point + hit.normal * 2;
        }
        return position;
    }

    /// <summary>
    /// Instantiates a cube at the given position, assigns a name, creates a JSON file for it, and saves its surrounding geometry.
    /// </summary>
    /// <param name="position">The position where the cube will be instantiated.</param>
    void SpawnCube(Vector3 position)
    {
        // Instantiate cube at the specified position
        GameObject cube = Instantiate(area, position, Quaternion.identity);
        cube.name = "Location_" + position.x + "_" + position.z;
        cube.tag = "cube";
        CreateJSONForCube(cube);
        SaveCubeGeometry(def, cube.transform.position, cube);
    }

    /// <summary>
    /// Creates a JSON file containing location-related data for the given cube.
    /// </summary>
    /// <param name="cube">The cube for which the JSON file will be created.</param>
    void CreateJSONForCube(GameObject cube)
    {
        // Create a new LocationData instance
        LocationData locationData = new LocationData();

        // Set the location name
        string locationName = cube.name;
        locationData.location = locationName;

        // Set path for the .obj file
        string objPath = Application.dataPath + "/Cutouts/" + cube.name + ".obj";
        locationData.obj = objPath;

        // Get the file paths of the stereo cubemap images for this location
        string leftImage = Application.dataPath + "/Images/" + locationName + "_leftImage.png";
        string rightImage = Application.dataPath + "/Images/" + locationName + "_rightImage.png";

        // Create array of image paths
        string[] imagePaths = { leftImage, rightImage };
        if (imagePaths != null)
        {
            // Add the file paths to the location data
            locationData.images = imagePaths;
        }

        // Serialize the LocationData instance to a JSON string
        string json = JsonUtility.ToJson(locationData, true);

        // Write the JSON string to a file
        string filePath = Application.dataPath + "/Jsons/" + locationName + ".json";
        File.WriteAllText(filePath, json);

        Debug.Log("JSON file created: " + filePath);
    }

    /// <summary>
    /// Iterates through all possible cube positions in the defined width and depth, checks for nearby cubes, and updates their JSON files if they can teleport to each other.
    /// </summary>
    void CheckNearbyCubes()
    {
        // Iterate through all possible cube positions
        for (int x = -Width; x < Width; x += space)
        {
            for (int z = -Depth; z < Depth; z += space)
            {
                GameObject cube = GameObject.Find("Location_" + x + "_" + z);
                if (cube != null)
                {
                    // Get the box collider component of the cube
                    BoxCollider boxCollider = cube.GetComponent<BoxCollider>();
                    // Enable the box collider
                    boxCollider.enabled = true;

                    // Loop through all nearby positions
                    for (int dx = -space; dx <= space; dx += space)
                    {
                        for (int dz = -space; dz <= space; dz += space)
                        {
                            if (dx == 0 && dz == 0)
                            {
                                continue;
                            }
                            else
                            {
                                // Check if there is a cube at the target position
                                GameObject targetCube = GameObject.Find("Location_" + (x + dx) + "_" + (z + dz));
                                if (targetCube == null)
                                {
                                    Debug.Log("Null Target Cube");
                                    continue;
                                }

                                Debug.Log(cube.name + " nearby cube found at " + targetCube.name);

                                // Check if the two cubes can teleport between each other
                                if (CanTeleportBetween(cube, targetCube))
                                {
                                    // Update the JSON files for the two cubes
                                    UpdateJsonFile(cube, targetCube);
                                    Debug.Log("Updated JSON File: " + cube.name);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Helper method to update the JSON files for two cubes.
    /// </summary>
    /// <param name="cube">The first cube to be updated.</param>
    /// <param name="cubeTarget">The second cube to be updated.</param>
    void UpdateJsonFile(GameObject cube, GameObject cubeTarget)
    {
        string jsonFilePath1 = Application.dataPath + "/Jsons/" + cube.name + ".json";
        string jsonFilePath2 = Application.dataPath + "/Jsons/" + cubeTarget.name + ".json";

        // Read the contents of the JSON file into a string
        string jsonContent1 = File.ReadAllText(jsonFilePath1);

        // Deserialize the string into a JSON object
        JObject jsonObject1 = JObject.Parse(jsonContent1);
        JObject jsonObject2 = JObject.Parse(jsonContent2);

        // Get the nearbyLocations array from the JSON object, or create it if it doesn't exist
        JArray nearbyLocationsArray1 = (JArray)jsonObject1["nearbyLocations"];
        if (nearbyLocationsArray1 == null)
        {
            nearbyLocationsArray1 = new JArray();
            jsonObject1["nearbyLocations"] = nearbyLocationsArray1;
        }
        nearbyLocationsArray1.Add(jsonFilePath2);

        // Serialize the JSON object back into a string
        string updatedJsonContent1 = jsonObject1.ToString();

        // Write the updated string back to the JSON file for the current cube
        File.WriteAllText(jsonFilePath1, updatedJsonContent1);
    }

    /// <summary>
    /// Checks if it's possible to teleport between two cubes without any obstruction.
    /// </summary>
    /// <param name="cube">The first cube.</param>
    /// <param name="cubeToTeleportTo">The second cube.</param>
    /// <returns>Returns true if the cubes can teleport between each other, otherwise false.</returns>
    bool CanTeleportBetween(GameObject cube, GameObject cubeToTeleportTo)
    {
        float verticalOffset = 5f;
        Vector3 origin = cube.transform.position;
        origin.y += verticalOffset;
        Vector3 destination = cubeToTeleportTo.transform.position;
        destination.y += verticalOffset;
        Vector3 direction = destination - origin;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, direction.magnitude))
        {
            if (hit.collider.gameObject == cubeToTeleportTo)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Saves the geometry of a cube and its surrounding area.
    /// </summary>
    /// <param name="originalObject">The original object to cut geometry from.</param>
    /// <param name="cutPosition">The position where the cube is to be cut.</param>
    /// <param name="cube">The instantiated cube.</param>
    private void SaveCubeGeometry(GameObject originalObject, Vector3 cutPosition, GameObject cube)
    {
        float cubeSize = 200f; // Adjust the size of the cube as needed

        // Cut and save the cube geometry using GeometryCutAndSave
        gcs.CutAndSaveCube(originalObject, cutPosition, cubeSize, cube);

        // Save the inverted cube geometry
        gcs.SaveInvertedCube(originalObject, cutPosition, 20, cube);
    }
}
