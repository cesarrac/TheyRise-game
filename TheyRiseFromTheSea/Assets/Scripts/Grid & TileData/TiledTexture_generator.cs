using UnityEngine;
using System.Collections;

public class TiledTexture_generator : MonoBehaviour {

    public int size_x, size_z;
    public int tileSize;

	// Use this for initialization
	void Start () {
        BuildMesh();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void BuildMesh()
    {

        int numTiles = size_x * size_z;
        int numTris = numTiles * 2;

        int vsize_x = size_x + 1;
        int vsize_z = size_z + 1;
        int numVerts = vsize_x * vsize_z;

        //Generate Mesh data
        Vector3[] vertices = new Vector3[numVerts];
        int[] triangles = new int[numTris * 3];
        Vector3[] normals = new Vector3[numVerts];
        Vector2[] uv = new Vector2[numVerts];

        int x, z;
        for (z = 0; z < size_z; z++)
        {
            for (x = 0; x < size_x; x++)
            {
                vertices[z * vsize_x + x] = new Vector3(x * tileSize, Random.Range(0f, 0.1f), z * tileSize);
                normals[z * vsize_x + x] = Vector3.up;
                uv[z * vsize_x + x] = new Vector2((float)x / size_x, (float)z / size_z);
                // when x = 0, uv.x =0
                //when x = 101 uv.x should = 1
                //uv.x = (float) x / vsize_x
            }
        }

        for (z = 0; z < size_z; z++)
        {
            for (x = 0; x < size_x; x++)
            {
                int squareIndex = z * size_x + x;
                int triOffset = squareIndex * 6;

                triangles[triOffset + 0] = z * vsize_x + x + 0;
                triangles[triOffset + 1] = z * vsize_x + x + vsize_x + 1;
                triangles[triOffset + 2] = z * vsize_x + x + vsize_x + 0;
                triangles[triOffset + 3] = z * vsize_x + x + 0;
                triangles[triOffset + 4] = z * vsize_x + x + 1;
                triangles[triOffset + 5] = z * vsize_x + x + vsize_x + 1;
            }
        }


        //Create a new Mesh using data
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        //Assign new Mesh to filter/collider/renderer
        MeshFilter mesh_filter = GetComponent<MeshFilter>();
        //		MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
        MeshCollider mesh_collider = GetComponent<MeshCollider>();

        mesh_filter.mesh = mesh;
        mesh_collider.sharedMesh = mesh;
        //
        //BuildTexture();
    }
}
