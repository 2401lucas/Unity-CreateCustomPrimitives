using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class CreateCustomPrimitives : EditorWindow
{
    enum ObjectType { Plane, Cylinder }
    ObjectType currentObjectType;

    //Plane ----------------------------
    int planeWidth = 10;
    int planeHeight = 10;
    int planeVertexDensity = 1;

    //Cylinder -------------------------
    int cylCircumferenceVertexCount = 12;
    int cylNumberOfSegments = 6;
    float cylSegmentSpacing = 1;
    float cylRadius = 2;

    bool logMeshInfo = false;

    GameObject currentPreview;

    [MenuItem("Tools/CreateCustomPrimitives")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        CreateCustomPrimitives window = (CreateCustomPrimitives)EditorWindow.GetWindow(typeof(CreateCustomPrimitives));
        window.Show();
    }

    private void OnDestroy()
    {
        DelPreview();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 100, 20), "Base Settings", EditorStyles.boldLabel);
        currentObjectType = (ObjectType)EditorGUI.EnumPopup(new Rect(0, 25, 100, 20), currentObjectType);

        switch (currentObjectType)
        {
            case ObjectType.Plane:
                DrawPlaneSettings();
                break;
            case ObjectType.Cylinder:
                DrawCylinderSettings();
                break;
        }
    }

    void DrawEndButtons(int yPos)
    {
        if (GUI.Button(new Rect(0, yPos, position.width, 20), "Generate Preview"))
        {
            Preview();
        }

        if (GUI.Button(new Rect(0, yPos + 25, position.width, 20), "Delete Preview"))
        {
            DelPreview();
        }

        if (GUI.Button(new Rect(0, yPos + 50, position.width, 20), "Generate Object"))
        {
            DelPreview();
            switch (currentObjectType)
            {
                case ObjectType.Plane:
                    GeneratePlane();
                    break;
                case ObjectType.Cylinder:
                    GenerateCylinder();
                    break;
            }
        }
    }

    void DrawPlaneSettings()
    {
        float halfWidth = position.width / 2;
        EditorGUI.LabelField(new Rect(0, 50, halfWidth, 20), "Width");
        planeWidth = EditorGUI.IntField(new Rect(halfWidth, 50, halfWidth, 20), planeWidth);
        EditorGUI.LabelField(new Rect(0, 75, halfWidth, 20), "Height");
        planeHeight = EditorGUI.IntField(new Rect(halfWidth, 75, halfWidth, 20), planeHeight);
        EditorGUI.LabelField(new Rect(0, 100, halfWidth, 20), "Vertex Density");
        planeVertexDensity = EditorGUI.IntField(new Rect(halfWidth, 100, halfWidth, 20), planeVertexDensity);
        EditorGUI.LabelField(new Rect(0, 125, halfWidth, 20), "Log Mesh Info");
        logMeshInfo = EditorGUI.Toggle(new Rect(halfWidth, 125, halfWidth, 20), logMeshInfo);
        DrawEndButtons(150);
    }

    void DrawCylinderSettings()
    {
        float halfWidth = position.width / 2;
        EditorGUI.LabelField(new Rect(0, 50, halfWidth, 20), "Number of Vertex on Circumference");
        cylCircumferenceVertexCount = EditorGUI.IntField(new Rect(halfWidth, 50, halfWidth, 20), cylCircumferenceVertexCount);
        EditorGUI.LabelField(new Rect(0, 75, halfWidth, 20), "Number of Segments");
        cylNumberOfSegments = EditorGUI.IntField(new Rect(halfWidth, 75, halfWidth, 20), cylNumberOfSegments);
        EditorGUI.LabelField(new Rect(0, 100, halfWidth, 20), "Space Between Segments");
        cylSegmentSpacing = EditorGUI.FloatField(new Rect(halfWidth, 100, halfWidth, 20), cylSegmentSpacing);
        EditorGUI.LabelField(new Rect(0, 125, halfWidth, 20), "Radius");
        cylRadius = EditorGUI.FloatField(new Rect(halfWidth, 125, halfWidth, 20), cylRadius);
        DrawEndButtons(150);
    }

    void Preview()
    {
        DelPreview();

        switch (currentObjectType)
        {
            case ObjectType.Plane:
                currentPreview = GeneratePlane();
                break;
            case ObjectType.Cylinder:
                currentPreview = GenerateCylinder();
                break;
        }
    }

    void DelPreview()
    {
        if (currentPreview != null)
        {
            DestroyImmediate(currentPreview);
        }
    }

    GameObject GeneratePlane()
    {
        GameObject go = new GameObject("Plane");
        Mesh mesh;
        MeshFilter filter = go.AddComponent<MeshFilter>();
        go.AddComponent(typeof(MeshRenderer));

        filter.mesh = mesh = new Mesh();
        mesh.name = "Plane";

        int width = planeWidth * planeVertexDensity;
        int height = planeHeight * planeVertexDensity;

        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        Vector2[] uvs = new Vector2[(width + 1) * (height + 1)];

        for (int i = 0, y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; i++, x++)
            {
                vertices[i] = new Vector3(x / planeVertexDensity, 0, y / planeVertexDensity);
                uvs[i] = new Vector2((float)x / (float)width, (float)y / (float)height);
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;

        int[] triangles = new int[height * width * 6];
        for (int ti = 0, vi = 0, y = 0; y < height; y++, vi++)
        {
            for (int x = 0; x < width; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + width + 1;
                triangles[ti + 5] = vi + width + 2;
            }
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        if (logMeshInfo)
        {
            Debug.Log($"Vertices: {mesh.vertexCount}");
        }

        return go;
    }

    GameObject GenerateCylinder()
    {
        GameObject go = new GameObject("Cylinder");
        Mesh mesh;
        MeshFilter filter = go.AddComponent<MeshFilter>();
        go.AddComponent(typeof(MeshRenderer));

        filter.mesh = mesh = new Mesh();
        mesh.name = "Cylinder";

        Vector3[] vertices = new Vector3[(cylCircumferenceVertexCount + 1) * (cylNumberOfSegments + 1)];
        Vector2[] uvs = new Vector2[(cylCircumferenceVertexCount + 1) * (cylNumberOfSegments + 1)];

        float disToMove = (Mathf.PI * 2) / cylCircumferenceVertexCount;

        for (int z = 0, vi = 0; z <= cylNumberOfSegments; z++)
        {
            for (int i = 0; i <= cylCircumferenceVertexCount; i++, vi++)
            {
                var newVertex = new Vector2(Mathf.Sin(i * disToMove), Mathf.Cos(i * disToMove)).normalized * cylRadius;
                vertices[vi] = new Vector3(newVertex.x, newVertex.y, z * cylSegmentSpacing);
                uvs[vi] = new Vector2(i * disToMove / (Mathf.PI * 2), (float)z / (float)cylNumberOfSegments);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;

        int[] triangles = new int[cylCircumferenceVertexCount * cylNumberOfSegments * 6];

        for (int z = 0, vi = 0, ti = 0; z < cylNumberOfSegments; z++, vi++)
        {
            for (int i = 0; i < cylCircumferenceVertexCount; i++, vi++, ti += 6)
            {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + cylCircumferenceVertexCount + 1;
                triangles[ti + 2] = vi + 1;
                triangles[ti + 3] = vi + 1;
                triangles[ti + 4] = vi + cylCircumferenceVertexCount + 1;
                triangles[ti + 5] = vi + cylCircumferenceVertexCount + 2;
            }
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int t = triangles[i];
            triangles[i] = triangles[i + 2];
            triangles[i + 2] = t;
        }
        mesh.triangles = triangles;

        // Reverse the normals;
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
            normals[i] = -normals[i];
        mesh.normals = normals;
        mesh.RecalculateBounds();

        return go;
    }
}
