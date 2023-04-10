using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public static class ObjExporter
{
    public static void MeshToFile(Mesh mesh, string path, Dictionary<int, Material> materialMapping, string name)
    {
        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.Write(MeshToString(mesh, name, materialMapping));
            sw.Close();
        }

        string mtlPath = Path.ChangeExtension(path, "mtl");
        string originalMTLPath = Application.dataPath + "/TestWorld.mtl"; 
        CopyAndRenameMTLFile(originalMTLPath, mtlPath);
    }

    public static string MeshToString(Mesh mesh, string name, Dictionary<int, Material> materialMapping)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"mtllib {name}.mtl\n");

        foreach (Vector2 v in mesh.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }

        sb.Append("\n");

        foreach (Vector3 v in mesh.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }

        for (int materialIndex = 0; materialIndex < mesh.subMeshCount; materialIndex++)
        {
            sb.Append("\n");

            if (materialMapping.TryGetValue(materialIndex, out Material material))
            {
                sb.Append("o ").Append(material.name).Append("\n");
                sb.Append("usemtl ").Append(material.name).Append("\n");
            }

            int[] triangles = mesh.GetTriangles(materialIndex);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v1 = triangles[i] + 1;
                int v2 = triangles[i + 1] + 1;
                int v3 = triangles[i + 2] + 1;

                sb.Append(string.Format("f {0}/{0} {1}/{1} {2}/{2}\n", v1, v2, v3));
            }
        }
        return sb.ToString();
    }

    public static void CopyAndRenameMTLFile(string originalMTLPath, string newMTLPath)
    {
        if (File.Exists(originalMTLPath))
        {
            File.Copy(originalMTLPath, newMTLPath, true);
        }
        else
        {
            Debug.LogError("Original MTL file not found at " + originalMTLPath);
        }
    }
}