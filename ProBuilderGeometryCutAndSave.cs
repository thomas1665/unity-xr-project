using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using System.Collections.Generic;
using System;

public class ProBuilderGeometryCutAndSave : MonoBehaviour
{
    private GameObject tempObject;

    private void Awake()
    {
        tempObject = new GameObject("Temporary ProBuilder Object");
    }

    public void CutAndSaveCube(GameObject original, Vector3 position, float size, GameObject cube)
    {
        Debug.Log("CutAndSaveCube: Original GameObject instance ID: " + original.GetInstanceID());
        MeshFilter meshFilter = original.GetComponent<MeshFilter>();
        Debug.Log("CutAndSaveCube: MeshFilter: " + meshFilter + ", Mesh: " + meshFilter.sharedMesh);

        if (meshFilter == null)
        {
            Debug.LogError("The original object does not have a MeshFilter component.");
            return;
        }

        // Convert the mesh of the original GameObject to a ProBuilderMesh
        ProBuilderMesh pbMesh = ConvertToProBuilderMesh(meshFilter);

        Bounds cutBounds = new Bounds(position, new Vector3(size, size, size));

        List<Face> intersectingFaces = GetFacesInBounds(pbMesh, cutBounds);
        ProBuilderMesh newMeshObject = ProBuilderMesh.Create(pbMesh.positions, intersectingFaces);

        List<Vector4> uvs = new List<Vector4>();
        pbMesh.GetUVs(0, uvs);
        newMeshObject.SetUVs(0, uvs);

        for (int i = 0; i < intersectingFaces.Count; i++)
        {
            Face newFace = newMeshObject.faces[i];
            Face originalFace = intersectingFaces[i];

            newFace.submeshIndex = originalFace.submeshIndex;
        }

        // Start of the UV transfer code snippet
        List<Vector4> originalUVs = new List<Vector4>();
        pbMesh.GetUVs(0, originalUVs);

        List<Vector4> newUVs = new List<Vector4>(new Vector4[newMeshObject.vertexCount]);

        for (int i = 0; i < newMeshObject.faces.Count; i++)
        {
            Face newFace = newMeshObject.faces[i];
            Face originalFace = intersectingFaces[i];

            for (int j = 0; j < newFace.indexes.Count; j++)
            {
                int newIndex = newFace.indexes[j];
                int oldIndex = originalFace.indexes[j];

                newUVs[newIndex] = originalUVs[oldIndex];
            }
        }

        newMeshObject.SetUVs(0, newUVs);

        string path = Application.dataPath + "/Cutouts/" + cube.name + ".obj";
        MeshRenderer meshRenderer = original.GetComponent<MeshRenderer>();
        Material[] materials = meshRenderer.sharedMaterials;
        meshRenderer.sharedMaterials = materials;
        Debug.Log("Original sharedMaterials: " + string.Join(", ", Array.ConvertAll(materials, m => m == null ? "null" : m.name)));

        Dictionary<int, Material> materialMapping = new Dictionary<int, Material>();
        for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
        {
            materialMapping[i] = meshRenderer.sharedMaterials[i];
        }

        ObjExporter.MeshToFile(newMeshObject.GetComponent<MeshFilter>().sharedMesh, path, materialMapping, cube.name);
    }

    private List<Face> GetFacesInBounds(ProBuilderMesh pbMesh, Bounds cutBounds)
    {
        List<Face> intersectingFaces = new List<Face>();

        foreach (Face face in pbMesh.faces)
        {
            bool faceInBounds = true;

            foreach (int index in face.indexes)
            {
                if (!cutBounds.Contains(pbMesh.positions[index]))
                {
                    faceInBounds = false;
                    break;
                }
            }

            if (faceInBounds)
            {
                intersectingFaces.Add(face);
            }
        }

        return intersectingFaces;
    }

    private ProBuilderMesh ConvertToProBuilderMesh(MeshFilter meshFilter)
    {
        ProBuilderMesh pbMesh = tempObject.GetComponent<ProBuilderMesh>();

        if (pbMesh == null)
        {
            pbMesh = tempObject.AddComponent<ProBuilderMesh>();
        }

        Debug.Log("ConvertToProBuilderMesh: MeshFilter: " + meshFilter + ", Mesh: " + meshFilter.sharedMesh);
        List<Vector3> positions = new List<Vector3>(meshFilter.sharedMesh.vertices);
        List<Vector4> uv = new List<Vector4>();
        meshFilter.sharedMesh.GetUVs(0, uv);
        List<int> triangles = new List<int>(meshFilter.sharedMesh.triangles);

        List<Vertex> vertices = new List<Vertex>();
        for (int i = 0; i < positions.Count; i++)
        {
            Vertex vertex = new Vertex();
            vertex.position = positions[i];
            vertex.uv0 = new Vector2(uv[i].x, uv[i].y);
            vertices.Add(vertex);
        }

        List<Face> faces = new List<Face>();
        for (int i = 0; i < triangles.Count; i += 3)
        {
            Face face = new Face(new int[] { triangles[i], triangles[i + 1], triangles[i + 2] });
            faces.Add(face);
        }

        pbMesh.Clear();
        pbMesh.SetVertices(vertices);
        pbMesh.RebuildWithPositionsAndFaces(positions, faces);
        pbMesh.SetUVs(0, uv);
        pbMesh.ToMesh();

        pbMesh.Refresh();
        pbMesh.Optimize();
        Destroy(tempObject);

        return pbMesh;
    }

    private void OnDestroy()
    {
        Destroy(tempObject);
    }
}