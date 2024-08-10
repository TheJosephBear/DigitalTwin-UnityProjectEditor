using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectSerializer {
    public static string Serialize(GameObject gameObject) {
        SerializableGameObject serializableObject = new SerializableGameObject(gameObject);
        return JsonUtility.ToJson(serializableObject);
    }

    public static GameObject Deserialize(string json) {
        SerializableGameObject serializableObject = JsonUtility.FromJson<SerializableGameObject>(json);
        return serializableObject.Deserialize();
    }
}




[Serializable]
public class SerializableGameObject {
    public string Name;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public List<SerializableMesh> Meshes = new List<SerializableMesh>();
    public List<SerializableGameObject> Children = new List<SerializableGameObject>();

    public SerializableGameObject(GameObject obj) {
        Name = obj.name;
        Position = obj.transform.localPosition;
        Rotation = obj.transform.localRotation;
        Scale = obj.transform.localScale;

        var meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null) {
            Meshes.Add(new SerializableMesh(meshFilter.sharedMesh, obj.GetComponent<MeshRenderer>()));
        }

        // Recursively serialize children
        foreach (Transform child in obj.transform) {
            Children.Add(new SerializableGameObject(child.gameObject));
        }
    }

    public GameObject Deserialize() {
        GameObject newObj = new GameObject(Name);
        newObj.transform.localPosition = Position;
        newObj.transform.localRotation = Rotation;
        newObj.transform.localScale = Scale;

        foreach (var serializableMesh in Meshes) {
            serializableMesh.ApplyTo(newObj);
        }

        // Recursively deserialize children
        foreach (var child in Children) {
            GameObject childObj = child.Deserialize();
            childObj.transform.SetParent(newObj.transform);
        }

        return newObj;
    }
}

[Serializable]
public class SerializableMesh {
    public Vector3[] Vertices;
    public int[] Triangles;
    public Vector3[] Normals;
    public Vector2[] UVs;
    public SerializableMaterial Material;

    public SerializableMesh(Mesh mesh, MeshRenderer renderer) {
        Vertices = mesh.vertices;
        Triangles = mesh.triangles;
        Normals = mesh.normals;
        UVs = mesh.uv;

        if (renderer != null && renderer.sharedMaterial != null) {
            Material = new SerializableMaterial(renderer.sharedMaterial);
        }
    }

    public void ApplyTo(GameObject obj) {
        var mesh = new Mesh {
            vertices = Vertices,
            triangles = Triangles,
            normals = Normals,
            uv = UVs
        };

        var meshFilter = obj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        var meshRenderer = obj.AddComponent<MeshRenderer>();
        if (Material != null) {
            meshRenderer.material = Material.ToMaterial();
        }
    }
}

[Serializable]
public class SerializableMaterial {
    public Color Color;

    public SerializableMaterial(Material material) {
        Color = material.color;
    }

    public Material ToMaterial() {
        var mat = new Material(Shader.Find("Standard")) {
            color = Color
        };
        return mat;
    }
}
