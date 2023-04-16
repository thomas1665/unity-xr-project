using UnityEngine;
using System.Text;
using System.IO;

public class GeometryCutAndSave : MonoBehaviour
{
    private StreamWriter writer;

    void Start()
    {
        writer = new StreamWriter("debug_log.txt");
        writer.WriteLine("Starting debug logging...");
        writer.Flush();
    }

    void LogToFile(string message)
    {
        writer.WriteLine(message);
        writer.Flush();
    }

    public void CutAndSaveCube(GameObject original, Vector3 position, float size, GameObject cube)
    {
        Debug.Log("CutAndSaveCube: Original GameObject instance ID: " + original.GetInstanceID());
        MeshFilter meshFilter = original.GetComponent<MeshFilter>();
        Debug.Log("CutAndSaveCube: MeshFilter: " + meshFilter + ", Mesh: " + meshFilter.sharedMesh);

        MeshRenderer meshRenderer = original.GetComponent<MeshRenderer>();
        Material[] materials = meshRenderer.sharedMaterials;

        Bounds cutBounds = new Bounds(position, new Vector3(size, size, size));

        string path = Application.dataPath + "/Cutouts/" + cube.name + ".obj";

        string objFilePath = path;

        string mtlFilePath = path.Replace(".obj", ".mtl");

        Mesh mesh = meshFilter.sharedMesh;

        StringBuilder objFileContent = new StringBuilder();
        StringBuilder mtlFileContent = new StringBuilder();

        objFileContent.AppendLine("# Exported Mesh OBJ File");
        objFileContent.AppendLine($"mtllib {Path.GetFileName(mtlFilePath)}");

        foreach (Vector3 vertex in mesh.vertices)
        {
            objFileContent.AppendLine($"v {vertex.x} {vertex.y} {vertex.z}");
        }
        LogToFile("Added vertices to objFileContent");
        foreach (Vector3 normal in mesh.normals)
        {
            objFileContent.AppendLine($"vn {normal.x} {normal.y} {normal.z}");
        }
        LogToFile("Added normals to objFileContent");
        foreach (Vector2 uv in mesh.uv)
        {
            objFileContent.AppendLine($"vt {uv.x} {uv.y}");
        }
        LogToFile("Added uvs to objFileContent");
        mtlFileContent.AppendLine("# Exported Material MTL File");

        LogToFile("Submesh count: " + mesh.subMeshCount);
        float minx = position.x - size;

        float maxx = position.x + size;

        float miny = position.y - size;

        float maxy = position.y + size;

        float minz = position.z - size;

        float maxz = position.z + size;

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
                mtlFileContent.AppendLine($"map_Kd tex/minecraft/block/{textureFileName}");
            }
            LogToFile("Added material to mtlFileContent"); 
            int[] indices = mesh.GetIndices(i);
            Vector3[] vertices = mesh.vertices;

            objFileContent.AppendLine($"usemtl {materialName}");

            int numExported = 0;

            for (int j = 0; j < indices.Length; j +=3)
            {
                int a = indices[j] + 1;

                int b = indices[j + 1] + 1;

                int c = indices[j + 2] + 1;

                Vector3 vertex0 = vertices[a - 1];

                Vector3 vertex1 = vertices[b - 1];

                Vector3 vertex2 = vertices[c - 1];

                if (
                ((vertex0.x < maxx) && (vertex0.x > minx) && (vertex0.y < maxy) && (vertex0.y > miny) && (vertex0.z < maxz) && (vertex0.z > minz)) ||

                ((vertex1.x < maxx) && (vertex1.x > minx) && (vertex1.y < maxy) && (vertex1.y > miny) && (vertex1.z < maxz) && (vertex1.z > minz)) ||

                ((vertex2.x < maxx) && (vertex2.x > minx) && (vertex2.y < maxy) && (vertex2.y > miny) && (vertex2.z < maxz) && (vertex2.z > minz))
                )
                {
                    objFileContent.AppendLine($"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}");
                    numExported++;
                }
            }
            LogToFile("Finished indices loop");
            if (numExported > 0)

            {
                mtlFileContent.AppendLine($"newmtl {materialName}");

                mtlFileContent.AppendLine($"Kd {material.color.r} {material.color.g} {material.color.b}");

                mtlFileContent.AppendLine($"Ns {material.GetInt("_SpecularPower")}");

                if (material.mainTexture != null)
                {
                    string textureFileName = material.mainTexture.name + ".png";
                    mtlFileContent.AppendLine($"map_Kd {textureFileName}");
                }
                objFileContent.AppendLine($"o {materialName}");
            }
        }
        LogToFile("Finished submesh loop");
        File.WriteAllText(objFilePath, objFileContent.ToString());
        File.WriteAllText(mtlFilePath, mtlFileContent.ToString());
    }
}