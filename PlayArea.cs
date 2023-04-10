using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class LocationData
{
    public string location;
    public string[] images;
    public string[] nearbyLocations;
}

public class PlayArea : MonoBehaviour
{
    private GenerateStereoCubemaps gsc;
    public GameObject area;
    public int MinHeight;
    private int Width = 1000;
    private int Height = 1000;
    private int Depth = 1000;
    public int space = 100;
    private float raycastDistance = 300f;
    private GameObject def;
    private ProBuilderGeometryCutAndSave pbGCS;

    private void Awake()
    {
        pbGCS = gameObject.AddComponent<ProBuilderGeometryCutAndSave>();
        gsc = Camera.main.GetComponent<GenerateStereoCubemaps>();
        def = GameObject.Find("default");
        Debug.Log("Default Gameobject: " + def + " Position: " + def.transform.position);
        Debug.Log("Awake: Original GameObject instance ID: " + def.GetInstanceID());
    }
    public void PlaceAreas()
    {
        HashSet<Vector3> placedPositions = new HashSet<Vector3>();
        for (int x = -Width; x < Width; x += space)
        {
            for (int y = MinHeight; y < Height; y += space)
            {
                for (int z = -Depth; z < Depth; z += space)
                {
                    if (x > -Width && x < Width - 1
                        && y > MinHeight && y < Height - 1 &&
                        z > -Depth && z < Depth - 1)
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

    void SpawnCube(Vector3 position)
    {
        GameObject cube = Instantiate(area, position, Quaternion.identity);
        cube.name = "Location_" + position.x + "_" + position.z;
        cube.tag = "cube";
        CreateJSONForCube(cube);
        gsc.Generate(position);
    }

    void CreateJSONForCube(GameObject cube)
    {
        // Create a new LocationData instance
        LocationData locationData = new LocationData();

        // Set the location name
        string locationName = cube.name;
        locationData.location = locationName;

        // Get the file paths of the stereo cubemap images for this location
        string leftImage = Application.dataPath + "/Images/" + locationName + "_leftImage.png";
        string rightImage = Application.dataPath + "/Images/" + locationName + "_rightImage.png";

        string[] imagePaths = { leftImage, rightImage };
        if (imagePaths != null)
        {
            // Add the file paths to the location data
            locationData.images = imagePaths;
        }

        // Serialize the LocationData instance to a JSON string
        string json = JsonUtility.ToJson(locationData);

        // Write the JSON string to a file
        string filePath = Application.dataPath + "/Jsons/" + locationName + ".json";
        File.WriteAllText(filePath, json);

        Debug.Log("JSON file created: " + filePath);
    }
    void CheckNearbyCubes()
    {
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

                    SaveCubeGeometry(def, cube.transform.position, cube);
                   
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
        Debug.Log("Checked All Nearby Cubes");
    }

    // Helper method to update the JSON files for two cubes
    void UpdateJsonFile(GameObject cube, GameObject cubeTarget)
    {
        string jsonFilePath1 = Application.dataPath + "/Jsons/" + cube.name + ".json";
        string jsonFilePath2 = Application.dataPath + "/Jsons/" + cubeTarget.name + ".json";

        // Read the contents of the JSON file into a string
        string jsonContent1 = File.ReadAllText(jsonFilePath1);
        string jsonContent2 = File.ReadAllText(jsonFilePath2);

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

    private void SaveCubeGeometry(GameObject originalObject, Vector3 cutPosition, GameObject cube)
    {
        Debug.Log("Default Gameobject: " + originalObject);
        MeshFilter meshFilter = originalObject.GetComponent<MeshFilter>();
        Debug.Log("OriginalObject MeshFilter: " + meshFilter + ", Mesh: " + meshFilter.sharedMesh);
        Debug.Log("SaveCubeGeometry: Original GameObject instance ID: " + originalObject.GetInstanceID());
        float cubeSize = 10f; // Adjust the size of the cube as needed
        pbGCS.CutAndSaveCube(originalObject, cutPosition, cubeSize, cube);
    }
}
