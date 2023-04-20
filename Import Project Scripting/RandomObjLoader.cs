// This script loads a random OBJ file with associated images and location data in a Unity project.

using System.IO;
using UnityEditor;
using UnityEngine;

public class RandomObjLoader : MonoBehaviour
{
    public Material skyboxMaterial;
    private GameObject currentObj;
    private TeleportToObj teleportToObj;

    private void Start()
    {
        teleportToObj = GameObject.Find("TeleportToObj").GetComponent<TeleportToObj>();
        LoadRandomObj();
    }

    /// <summary>
    /// Loads a random OBJ file with associated images and location data.
    /// </summary>
    public void LoadRandomObj()
    {
        string cutoutsFolderPath = "Assets/Cutouts";
        DirectoryInfo dirInfo = new DirectoryInfo(cutoutsFolderPath);
        FileInfo[] objFiles = dirInfo.GetFiles("*.obj", SearchOption.TopDirectoryOnly);

        if (objFiles.Length == 0)
        {
            Debug.LogError("No OBJ files found in the Cutouts folder.");
            return;
        }

        FileInfo selectedObjFile = null;
        LocationData locationData = null;

        // Choose a random OBJ file with a valid associated JSON file.
        while (selectedObjFile == null)
        {
            FileInfo candidateObjFile = objFiles[Random.Range(0, objFiles.Length)];
            string candidateObjPath = candidateObjFile.FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets");

            string candidateJsonPath = "Assets/Jsons/" + Path.GetFileNameWithoutExtension(candidateObjPath) + ".json";
            if (File.Exists(candidateJsonPath))
            {
                string jsonContent = File.ReadAllText(candidateJsonPath);
                locationData = JsonUtility.FromJson<LocationData>(jsonContent);

                if (locationData.nearbyLocations.Length > 0)
                {
                    selectedObjFile = candidateObjFile;
                }
            }
        }

        string selectedObjPath = selectedObjFile.FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets");

        // Load and instantiate the selected OBJ file.
        GameObject objPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(selectedObjPath);
        if (objPrefab == null)
        {
            Debug.LogError("Failed to load OBJ file: " + selectedObjPath);
            return;
        }
        currentObj = Instantiate(objPrefab);

        // Set left and right images for the material
        string baseName = Path.GetFileNameWithoutExtension(selectedObjPath);
        string leftImagePath = "Assets/Images/" + baseName + "_leftImage.png";
        string rightImagePath = "Assets/Images/" + baseName + "_rightImage.png";

        Cubemap leftImage = AssetDatabase.LoadAssetAtPath<Cubemap>(leftImagePath);
        Cubemap rightImage = AssetDatabase.LoadAssetAtPath<Cubemap>(rightImagePath);

        // Apply the images to the skybox material.
        if (leftImage != null && rightImage != null)
        {
            skyboxMaterial.SetTexture("_TexLeft", leftImage);
            skyboxMaterial.SetTexture("_TexRight", rightImage);
            RenderSettings.skybox = skyboxMaterial;
        }
        else
        {
            Debug.LogError("Failed to load left or right image for the OBJ file: " + selectedObjPath);
        }

        // Teleport camera above the object's surface.
        TeleportCameraAboveSurface(currentObj);

        // Assign object's name and spawn floating spheres for nearby locations.
        teleportToObj.AssignCurrentObjName(baseName + "(Clone)");
        teleportToObj.SpawnFloatingSpheres(locationData);
    }

    /// <summary>
    /// Teleports the camera to a position above the surface of the given GameObject.
    /// </summary>
    /// <param name="obj">The GameObject to teleport the camera above.</param>
    public void TeleportCameraAboveSurface(GameObject obj)
    {
        //Set the position of the raycast origin to the center of the object's bounds.
        Renderer objRenderer = obj.GetComponentInChildren<Renderer>();
        Bounds objBounds = objRenderer.bounds;
        Vector3 raycastOrigin = objBounds.center + new Vector3(0, objBounds.extents.y + 1000f, 0);
        RaycastHit hit;

        //Teleport the camera to the point where the raycast hits the object's surface.
        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, Mathf.Infinity))
        {
            Camera.main.transform.position = hit.point + new Vector3(0, 3f, 0);
        }
        else
        {
            Debug.LogWarning("Raycast failed to hit the object's surface.");
        }
    }

    [System.Serializable]
    public class LocationData
    {
        public string location;
        public string[] images;
        public string[] nearbyLocations;
    }
}
