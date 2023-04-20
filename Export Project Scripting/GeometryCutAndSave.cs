//This script is used to cut out the geometry of a cube from a larger mesh and save it as an .obj file along with its .mtl file.
//It is used in the GeometryCutAndSave.cs script.

using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;

public class GeometryCutAndSave : MonoBehaviour
{
    private StreamWriter writer;

    // Initialize the writer for debug logging
    void Start()
    {
        writer = new StreamWriter("debug_log.txt");
        writer.WriteLine("Starting debug logging...");
        writer.Flush();
    }

    /// <summary>
    /// Log a message to the debug file.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogToFile(string message)
    {
        writer.WriteLine(message);
        writer.Flush();
    }

    /// <summary>
    /// Cut and save a portion of the original mesh with the given position and size.
    /// </summary>
    /// <param name="original">The original GameObject with the mesh to be cut.</param>
    /// <param name="position">The position of the center of the portion to be cut.</param>
    /// <param name="size">The size of the portion to be cut.</param>
    /// <param name="cube">The cube GameObject at the location to be cut</param>
    public void CutAndSaveCube(GameObject original, Vector3 position, float size, GameObject cube)
    {
        MeshFilter meshFilter = original.GetComponent<MeshFilter>();

        MeshRenderer meshRenderer = original.GetComponent<MeshRenderer>();
        Material[] materials = meshRenderer.sharedMaterials;

        // Set up file paths for exported OBJ and MTL files
        string path = Application.dataPath + "/Cutouts/" + cube.name + ".obj";
        string objFilePath = path;
        string mtlFilePath = path.Replace(".obj", ".mtl");

        Mesh mesh = meshFilter.sharedMesh;

        // Initialize string builders to store OBJ and MTL file content
        StringBuilder objFileContent = new StringBuilder();
        StringBuilder mtlFileContent = new StringBuilder();

        // Add headers and MTL file reference to OBJ file content
        objFileContent.AppendLine("# Exported Mesh OBJ File");
        objFileContent.AppendLine($"mtllib {Path.GetFileName(mtlFilePath)}");

        // Create a dictionary to store unique UV coordinates and their corresponding index
        Dictionary<Vector2, int> uniqueUVs = new Dictionary<Vector2, int>();
        // Iterate through the UV coordinates and store them in the dictionary if they are unique
        int uvIndex = 1;
        foreach (Vector2 uv in mesh.uv)
        {
            if (!uniqueUVs.ContainsKey(uv))
            {
                uniqueUVs.Add(uv, uvIndex++);
                objFileContent.AppendLine($"vt {uv.x} {uv.y}");
            }
        }
        LogToFile("Added uvs to objFileContent");

        // Write vertex coordinates to the OBJ file content
        foreach (Vector3 vertex in mesh.vertices)
        {
            objFileContent.AppendLine($"v {vertex.x} {vertex.y} {vertex.z}");
        }
        LogToFile("Added vertices to objFileContent");

        // Add header to MTL file content
        mtlFileContent.AppendLine("# Exported Material MTL File");

        LogToFile("Submesh count: " + mesh.subMeshCount);

        // Set up bounds for the mesh portion to be exported
        float minx = position.x - size;
        float maxx = position.x + size;
        float miny = position.y - 50;
        float maxy = position.y + size;
        float minz = position.z - size;
        float maxz = position.z + size;

        // Iterate through submeshes and write material and face information to the OBJ and MTL files
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            Material material = materials[i];

            string materialName = $"Material_{i}";
            objFileContent.AppendLine($"usemtl {materialName}");

            // Write material data to the OBJ and MTL files
            mtlFileContent.AppendLine($"newmtl {materialName}");
            mtlFileContent.AppendLine($"Kd {material.color.r} {material.color.g} {material.color.b}");
            mtlFileContent.AppendLine($"Ns {material.GetFloat("_Glossiness")}");

            // Check if the main texture exists and write the appropriate texture path to the MTL file
            if (material.mainTexture != null)
            {
                string textureFileName = material.mainTexture.name + ".png";
                if (textureFileName == "chicken.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/chicken.png");
                }
                else if (textureFileName == "cow.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/cow/cow.png");
                }
                else if (textureFileName == "squid.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/squid/squid.png");
                }
                else if (textureFileName == "pig.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/pig/pig.png");
                }
                else if (textureFileName == "sheep.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/sheep/sheep.png");
                }
                else if (textureFileName == "normal.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/chest/normal.png");
                }
                else
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/block/{textureFileName}");
                }
            }
            LogToFile("Added material to mtlFileContent");

            // Get mesh data (indices, vertices, uvs) for the current submesh
            int[] indices = mesh.GetIndices(i);
            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = mesh.uv;

            objFileContent.AppendLine($"usemtl {materialName}");

            int numExported = 0;

            // Iterate through the triangle indices and export those within the specified bounds
            for (int j = 0; j < indices.Length; j += 3)
            {
                int a = indices[j] + 1;
                int b = indices[j + 1] + 1;
                int c = indices[j + 2] + 1;

                // Get the correct UV coordinate indices for each vertex
                int uvA = uniqueUVs[uvs[indices[j]]];
                int uvB = uniqueUVs[uvs[indices[j + 1]]];
                int uvC = uniqueUVs[uvs[indices[j + 2]]];

                Vector3 vertex0 = vertices[a - 1];
                Vector3 vertex1 = vertices[b - 1];
                Vector3 vertex2 = vertices[c - 1];

                // Determine if any of the triangle's vertices are within the specified bounds
                if (
                ((vertex0.x < maxx) && (vertex0.x > minx) && (vertex0.y < maxy) && (vertex0.y > miny) && (vertex0.z < maxz) && (vertex0.z > minz)) ||

                ((vertex1.x < maxx) && (vertex1.x > minx) && (vertex1.y < maxy) && (vertex1.y > miny) && (vertex1.z < maxz) && (vertex1.z > minz)) ||

                ((vertex2.x < maxx) && (vertex2.x > minx) && (vertex2.y < maxy) && (vertex2.y > miny) && (vertex2.z < maxz) && (vertex2.z > minz))
                )
                {
                    // If any vertex is within bounds, export the triangle to the OBJ file
                    objFileContent.AppendLine($"f {a}/{uvA}/{a} {b}/{uvB}/{b} {c}/{uvC}/{c}");
                    numExported++;
                }
            }
            LogToFile("Finished indices loop");

            // If any triangles were exported, write additional material data to the MTL file
            if (numExported > 0)
            {
                // Write material data to the MTL file
                mtlFileContent.AppendLine($"newmtl {materialName}");
                mtlFileContent.AppendLine($"Kd {material.color.r} {material.color.g} {material.color.b}");

                if (material.HasProperty("_Glossiness"))
                {
                    mtlFileContent.AppendLine($"Ns {material.GetFloat("_Glossiness")}");

                }

                // Write texture data to the MTL file if the main texture exists
                if (material.mainTexture != null)
                {
                    string textureFileName = material.mainTexture.name + ".png";
                    mtlFileContent.AppendLine($"map_Kd {textureFileName}");
                }
                objFileContent.AppendLine($"o {materialName}");
            }
        }
        LogToFile("Finished submesh loop");

        // Write the final OBJ and MTL file content to disk
        File.WriteAllText(objFilePath, objFileContent.ToString());
        File.WriteAllText(mtlFilePath, mtlFileContent.ToString());
    }

    /// <summary>
    /// Saves an inverted version of the cutout cube of a game object in the OBJ and MTL file formats, considering only the perimeter of the cube.
    /// </summary>
    /// <param name="original">The original game object to be processed.</param>
    /// <param name="position">The center position of the cube.</param>
    /// <param name="size">The size of the cube.</param>
    /// <param name="cube">The cube GameObject at the location to be cut.</param>
    public void SaveInvertedCube(GameObject original, Vector3 position, float size, GameObject cube)
    {
        // Get the mesh filter and mesh renderer components from the original game object
        MeshFilter meshFilter = original.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = original.GetComponent<MeshRenderer>();

        // Get the materials from the mesh renderer
        Material[] materials = meshRenderer.sharedMaterials;

        // Set the file paths for the OBJ and MTL files
        string path = Application.dataPath + "/InvertedCutouts/" + cube.name + "_inverted.obj";
        string objFilePath = path;
        string mtlFilePath = path.Replace(".obj", ".mtl");

        // Get the mesh from the mesh filter
        Mesh mesh = meshFilter.sharedMesh;

        // Initialize string builders for the OBJ and MTL file content
        StringBuilder objFileContent = new StringBuilder();
        StringBuilder mtlFileContent = new StringBuilder();

        objFileContent.AppendLine("# Exported Mesh OBJ File");
        objFileContent.AppendLine($"mtllib {Path.GetFileName(mtlFilePath)}");

        // Create a dictionary to store unique UV coordinates and their corresponding index
        Dictionary<Vector2, int> uniqueUVs = new Dictionary<Vector2, int>();
        // Iterate through the UV coordinates and store them in the dictionary if they are unique
        int uvIndex = 1;
        foreach (Vector2 uv in mesh.uv)
        {
            if (!uniqueUVs.ContainsKey(uv))
            {
                uniqueUVs.Add(uv, uvIndex++);
                objFileContent.AppendLine($"vt {uv.x} {uv.y}");
            }
        }
        LogToFile("Added uvs to objFileContent");

        foreach (Vector3 vertex in mesh.vertices)
        {
            objFileContent.AppendLine($"v {vertex.x} {vertex.y} {vertex.z}");
        }
        LogToFile("Added vertices to objFileContent");

        mtlFileContent.AppendLine("# Exported Material MTL File");
        LogToFile("Submesh count: " + mesh.subMeshCount);

        // Set up bounds for the mesh portion to not be exported
        float minx = position.x - size;
        float maxx = position.x + size;
        float miny = position.y - 50;
        float maxy = position.y + size;
        float minz = position.z - size;
        float maxz = position.z + size;

        // Set up bounds for the perimeter of the mesh portion to be exported
        float perimeterSize = 100;
        float pMinX = minx - perimeterSize;
        float pMaxX = maxx + perimeterSize;
        float pMinY = miny;
        float pMaxY = maxy + perimeterSize;
        float pMinZ = minz - perimeterSize;
        float pMaxZ = maxz + perimeterSize;

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            Material material = materials[i];

            string materialName = $"Material_{i}";
            objFileContent.AppendLine($"usemtl {materialName}");

            mtlFileContent.AppendLine($"newmtl {materialName}");
            mtlFileContent.AppendLine($"Kd {material.color.r} {material.color.g} {material.color.b}");
            mtlFileContent.AppendLine($"Ns {material.GetFloat("_Glossiness")}");
            if (material.mainTexture != null)
            {
                string textureFileName = material.mainTexture.name + ".png";
                if (textureFileName == "chicken.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/chicken.png");
                }
                else if (textureFileName == "cow.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/cow/cow.png");
                }
                else if (textureFileName == "squid.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/squid/squid.png");
                }
                else if (textureFileName == "pig.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/pig/pig.png");
                }
                else if (textureFileName == "sheep.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/sheep/sheep.png");
                }
                else if (textureFileName == "normal.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/chest/normal.png");
                }
                else if (textureFileName == "white.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/bed/white.png");
                }
                else if (textureFileName == "yellow.png")
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/entity/bed/yellow.png");
                }
                else
                {
                    mtlFileContent.AppendLine($"map_Kd tex/minecraft/block/{textureFileName}");
                }
            }
            LogToFile("Added material to mtlFileContent");

            int[] indices = mesh.GetIndices(i);
            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = mesh.uv;

            objFileContent.AppendLine($"usemtl {materialName}");

            int numExported = 0;

            for (int j = 0; j < indices.Length; j += 3)
            {
                int a = indices[j] + 1;
                int b = indices[j + 1] + 1;
                int c = indices[j + 2] + 1;

                int uvA = uniqueUVs[uvs[indices[j]]];
                int uvB = uniqueUVs[uvs[indices[j + 1]]];
                int uvC = uniqueUVs[uvs[indices[j + 2]]];

                Vector3 vertex0 = vertices[a - 1];
                Vector3 vertex1 = vertices[b - 1];
                Vector3 vertex2 = vertices[c - 1];

                // Determine if triangles vertices are outside the bounds, then export them
                if (
                    (((vertex0.x < pMaxX) && (vertex0.x > pMinX) && (vertex0.y < pMaxY) && (vertex0.y > pMinY) && (vertex0.z < pMaxZ) && (vertex0.z > pMinZ)) &&
                    !((vertex0.x < maxx) && (vertex0.x > minx) && (vertex0.y < maxy) && (vertex0.y > miny) && (vertex0.z < maxz) && (vertex0.z > minz))) ||

                     (((vertex1.x < pMaxX) && (vertex1.x > pMinX) && (vertex1.y < pMaxY) && (vertex1.y > pMinY) && (vertex1.z < pMaxZ) && (vertex1.z > pMinZ)) &&
                     !((vertex1.x < maxx) && (vertex1.x > minx) && (vertex1.y < maxy) && (vertex1.y > miny) && (vertex1.z < maxz) && (vertex1.z > minz))) ||

                     (((vertex2.x < pMaxX) && (vertex2.x > pMinX) && (vertex2.y < pMaxY) && (vertex2.y > pMinY) && (vertex2.z < pMaxZ) && (vertex2.z > pMinZ)) &&
                     !((vertex2.x < maxx) && (vertex2.x > minx) && (vertex2.y < maxy) && (vertex2.y > miny) && (vertex2.z < maxz) && (vertex2.z > minz)))
                   )
                {
                    objFileContent.AppendLine($"f {a}/{uvA}/{a} {b}/{uvB}/{b} {c}/{uvC}/{c}");
                    numExported++;
                }
            }
            LogToFile("Finished indices loop");

            if (numExported > 0)
            {
                mtlFileContent.AppendLine($"newmtl {materialName}");
                mtlFileContent.AppendLine($"Kd {material.color.r} {material.color.g} {material.color.b}");

                if (material.HasProperty("_Glossiness"))
                {
                    mtlFileContent.AppendLine($"Ns {material.GetFloat("_Glossiness")}");

                }

                if (material.mainTexture != null)
                {
                    string textureFileName = material.mainTexture.name + ".png";
                    mtlFileContent.AppendLine($"map_Kd {textureFileName}");
                }
                objFileContent.AppendLine($"o {materialName}");
            }
            LogToFile("Finished submesh loop");
            File.WriteAllText(objFilePath, objFileContent.ToString());
            File.WriteAllText(mtlFilePath, mtlFileContent.ToString());
        }
    }
}
