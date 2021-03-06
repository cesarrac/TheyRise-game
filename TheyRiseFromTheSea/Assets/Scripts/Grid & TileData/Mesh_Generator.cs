﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Mesh_Generator : MonoBehaviour {

	public SquareGrid squareGrid;
	public MeshFilter walls;
	public MeshFilter water;
    public MeshFilter ground;
    public MeshFilter shoreWatermap;

	public GameObject _FLOOR;
	
	List<Vector3> vertices;
	List<int> triangles;
	
	Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>> ();
	
	List<List<int>> outlines = new List<List<int>> ();
	HashSet<int> checkedVertices = new HashSet<int> ();

	public Sprite[] shoreTiles;
	Vector3 nextVert;
	public GameObject shorefabtest;
	public Transform shoreHolder;

	public ObjectPool objPool;


    private Mesh islandMesh, waterShoreMesh;



    public void GenerateMesh(int[,] map, float squareSize){
		
		outlines.Clear ();
		checkedVertices.Clear ();
		triangleDictionary.Clear ();
		
		squareGrid = new SquareGrid (map, squareSize);
		
		vertices = new List<Vector3> ();
		triangles = new List<int> ();
		
		for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
			for (int y =0; y <squareGrid.squares.GetLength(1); y++) {
				TriangulateSquare(squareGrid.squares[x,y]);
			}
		}
		
		Mesh mesh = new Mesh ();
		water.mesh = mesh;
		
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.RecalculateNormals ();
		
		int tileAmount = 32;
		Vector2[] uvs = new Vector2[vertices.Count];
		for (int i = 0; i < vertices.Count; i++) {
			float percentX = Mathf.InverseLerp(-map.GetLength(0)/2*squareSize, map.GetLength(0)/2*squareSize, vertices[i].x) * tileAmount;
			float percentY = Mathf.InverseLerp(-map.GetLength(0)/2*squareSize, map.GetLength(0)/2*squareSize, vertices[i].z) * tileAmount;
			uvs[i] = new Vector2(percentX, percentY);
		}
		mesh.uv = uvs;

		

        // Generate 2D Edge Colliders
	    Generate2DColliders();

		
	}

    public void GenerateIslandMesh(int[,]map)
    {
    
        ground.GetComponent<MeshFilter>().mesh = islandMesh = new Mesh();
        islandMesh.name = "Island Mesh";

        Vector3[] landVertices = new Vector3[(map.GetLength(0) + 1) * (map.GetLength(1) + 1)];
        Vector2[] uvs = new Vector2[landVertices.Length];


        for (int i = 0, y = 0; y <= map.GetLength(1); y++)
        {
            for (int x = 0; x <= map.GetLength(0); x++, i++)
            {
                landVertices[i] = new Vector3(x, y);
                uvs[i] = new Vector2((float)x /map.GetLength(0), (float)y / map.GetLength(1));
            
            }
        }

        islandMesh.vertices = landVertices;
        islandMesh.uv = uvs;

        int[] landTriangles = new int[map.GetLength(0) * map.GetLength(1) * 6];
        for (int ti = 0, vi = 0, y = 0; y < map.GetLength(1); y++, vi++)
        {
            for (int x = 0; x < map.GetLength(0); x++, ti += 6, vi++)
            {
                landTriangles[ti] = vi;
                landTriangles[ti + 3] = landTriangles[ti + 2] = vi + 1;
                landTriangles[ti + 4] = landTriangles[ti + 1] = vi + map.GetLength(0) + 1;
                landTriangles[ti + 5] = vi + map.GetLength(0) + 2;
            }
        }
        islandMesh.triangles = landTriangles;
        islandMesh.RecalculateNormals();

        // Give the Mesh to the ground's mesh collider
        ground.GetComponent<MeshCollider>().sharedMesh = islandMesh;

        GenerateShoreWaterMesh(islandMesh, map);

    }

    public void GenerateShoreWaterMesh(Mesh newMesh, int[,] map)
    {

        shoreWatermap.GetComponent<MeshFilter>().mesh = newMesh;
        //Mesh shoreMesh;
        //shoreWatermap.GetComponent<MeshFilter>().mesh = shoreMesh = new Mesh();
        //shoreMesh.name = "Shore Mesh";

        ////CalculateMeshOutlines();

        //// Each of the shore vertices have to point to each point in of the outline

        //List<Vector3> shoreVerts = new List<Vector3>();
        //List<int> shoreTris = new List<int>();

        //// Vector3[] wallVerts = new Vector3[outlines.Count];
        //List<Vector2> uvs = new List<Vector2>();

        //// int triCount = 0;
        //foreach (List<int> outline in outlines)
        //{
        //    for (int i = 0; i < outline.Count; i++)
        //    {

        //        Vector3 v = new Vector3(vertices[outline[i]].x, vertices[outline[i]].z, 0);
        //        shoreVerts.Add(v);
        //        uvs.Add(new Vector2(v.x, v.y));

        //        shoreTris.Add(i);
        //        shoreTris.Add(i + 2);
        //        shoreTris.Add(i + 3);

        //        shoreTris.Add(i);
        //        shoreTris.Add(i + 3);
        //        shoreTris.Add(i + 4);

        //        shoreTris.Add(i);
        //        shoreTris.Add(i + 4);
        //        shoreTris.Add(i + 5);

        //    }

        //    shoreMesh.vertices = shoreVerts.ToArray();
        //    shoreMesh.uv = uvs.ToArray();

        //    shoreMesh.triangles = shoreTris.ToArray();
        //    shoreMesh.RecalculateNormals();


            //int[] shoreTriangles = new int[map.GetLength(0) * map.GetLength(1) * 6];
            //for (int ti = 0, vi = 0, y = 0; y < map.GetLength(1); y++, vi++)
            //{
            //    for (int x = 0; x < map.GetLength(0); x++, ti += 6, vi++)
            //    {
            //        shoreTriangles[ti] = vi;
            //        shoreTriangles[ti + 3] = shoreTriangles[ti + 2] = vi + 1;
            //        shoreTriangles[ti + 4] = shoreTriangles[ti + 1] = vi + map.GetLength(0) + 1;
            //        shoreTriangles[ti + 5] = vi + map.GetLength(0) + 2;
            //    }
            //}

            //shoreMesh.triangles = shoreTriangles;
            //shoreMesh.RecalculateNormals();


            //foreach (List<int> outline in outlines)
            //{
            //    for (int i = 0; i < outline.Count - 1; i++)
            //    {
            //        int startIndex = wallVertices.Count;
            //        wallVertices.Add(vertices[outline[i]]); // left vertex
            //        wallVertices.Add(vertices[outline[i + 1]]); // right vertex
            //        wallVertices.Add(vertices[outline[i]] + Vector3.up); // top left
            //        wallVertices.Add(vertices[outline[i + 1]] + Vector3.up); // top right

            //        // First triangle
            //        wallTriangles.Add(startIndex + 0);
            //        wallTriangles.Add(startIndex + 1);
            //        wallTriangles.Add(startIndex + 3);
            //        // Second triangle
            //        wallTriangles.Add(startIndex + 0);
            //        wallTriangles.Add(startIndex + 3);
            //        wallTriangles.Add(startIndex + 2);

            //    }
            //}

            //wallMesh.vertices = wallVertices.ToArray();
            //wallMesh.triangles = wallTriangles.ToArray();
            //wallMesh.RecalculateNormals();

            //int tileAmount = 32;
            //Vector2[] uvs = new Vector2[wallVertices.Count];
            //for (int i = 0; i < wallVertices.Count; i++)
            //{
            //    float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2, map.GetLength(0) / 2, wallVertices[i].x) * tileAmount;
            //    float percentY = Mathf.InverseLerp(-map.GetLength(0) / 2, map.GetLength(0) / 2, wallVertices[i].z) * tileAmount;
            //    uvs[i] = new Vector2(percentX, percentY);
            //}
            //wallMesh.uv = uvs;





       // }
    }
    void Generate2DColliders()
	{
		EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D> ();
		for (int i =0; i < currentColliders.Length; i ++) {
			Destroy(currentColliders[i]);
		}
		
		CalculateMeshOutlines ();

		//List<Vector2> EmptyTilesOutline = new List<Vector2> ();
		
		foreach (List<int> outline in outlines) {
			EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
			Vector2[] edgepoints = new Vector2[outline.Count];
			
			for (int i = 0; i < outline.Count; i++){
				edgepoints[i] = new Vector2(vertices[outline[i]].x,vertices[outline[i]].z) ;

				//EmptyTilesOutline.Add(edgepoints[i]);
                /*
                if (i + 1 < outline.Count){
					GenerateShoreLine(outline, edgepoints[i].x, edgepoints[i].y, i + 1);
					
				}else{
					GenerateShoreLine(outline, edgepoints[i].x, edgepoints[i].y, i);
					
				}
                */
			}
			edgeCollider.points = edgepoints;

			// move the shore holder to the map's position to set the shore tiles to the right place
			//shoreHolder.transform.localPosition = transform.position;

			// Build island Texture
			//Vector2[] emptyTilesArray = EmptyTilesOutline.ToArray();

			//TileTexture tileTexture = _FLOOR.GetComponent<TileTexture>();
			//tileTexture.BuildTexture(emptyTilesArray, iMap.GetLength(0), iMap.GetLength(1));
		}
	}

//	void GenerateShoreLine(List<int> outline, float edgePointX, float edgePointY, int i)
//	{
//		// get the shore fab from the obj pool and assign it a sprite from the shore sheet
////		GameObject shore = objPool.GetObjectForType ("shore_fab", false, new Vector3(edgePointX,edgePointY, 0.0f));
//		GameObject shore = Instantiate (shorefabtest, new Vector3 (edgePointX, edgePointY, 0.0f), Quaternion.identity) as GameObject;
//		int randomSpriteSelection = Random.Range (0, shoreTiles.Length - 1);
//		if (shore) {
//			shore.GetComponent<SpriteRenderer> ().sprite = shoreTiles [randomSpriteSelection];

//			shore.transform.SetParent(shoreHolder);
		
//			// Using the NEXT item in the vertices array I can know which direction the edge is heading to
		
//			nextVert = new Vector3 (vertices [outline [i]].x, vertices [outline [i]].z, 0.0f);
		
		
//			// Give the shore its angle 
//			Vector3 dir = nextVert - shore.transform.position;
//			float angle = Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg;
//			shore.transform.eulerAngles = new Vector3 (0, 0, angle);
		
//			/* To figure out its X scale we need to get the distance from the new X position (bottom left corner)
//				 * to the last x position. Then add that distance to the x scale of the shore. */
//			// distance = newX - lastX
//			//				if (i > 0){
//			//					float distanceFromLastShore = Vector2.Distance(edgepoints[i], edgepoints[i-1]);
//			//					shore.transform.localScale = new Vector3(shore.transform.localScale.x + distanceFromLastShore, 1, 1);
//			//				}
//		} else {
//			Debug.Log ("Could not get shore from pool!");
//		}
//	}


	
	void TriangulateSquare(Square square) {
		switch (square.configuration) {
		case 0:
			break;
			
			// 1 points:
		case 1:
			MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
			break;
		case 2:
			MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
			break;
		case 4:
			MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
			break;
		case 8:
			MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
			break;
			
			// 2 points:
		case 3:
			MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
			break;
		case 6:
			MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
			break;
		case 9:
			MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
			break;
		case 12:
			MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
			break;
		case 5:
			MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
			break;
		case 10:
			MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
			break;
			
			// 3 point:
		case 7:
			MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
			break;
		case 11:
			MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
			break;
		case 13:
			MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
			break;
		case 14:
			MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
			break;
			
			// 4 point:
		case 15:
			MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
			
			// None of these can have an outline so just add these to the list to not check them
			checkedVertices.Add(square.topLeft.vertexIndex);
			checkedVertices.Add(square.topRight.vertexIndex);
			checkedVertices.Add(square.bottomRight.vertexIndex);
			checkedVertices.Add(square.bottomLeft.vertexIndex);
			
			break;
		}
	}
	
	void MeshFromPoints(params Junction[] points)
	{
		AssignVertices (points);
		
		if (points.Length >= 3) {
			CreateTriangles(points[0], points[1], points[2]);
		}
		if (points.Length >= 4) {
			CreateTriangles(points[0], points[2], points[3]);
		}
		if (points.Length >= 5) {
			CreateTriangles(points[0], points[3], points[4]);
		}
		if (points.Length >= 6) {
			CreateTriangles(points[0], points[4], points[5]);
		}
		
	}
	
	void AssignVertices (Junction[] points)
	{
		for (int i = 0; i < points.Length; i++) {
			if (points[i].vertexIndex == -1){
				points[i].vertexIndex = vertices.Count;
				vertices.Add(points[i].position);
			}
		}
	}
	
	void CreateTriangles(Junction a, Junction b, Junction c)
	{
		triangles.Add (a.vertexIndex);
		triangles.Add (b.vertexIndex);
		triangles.Add (c.vertexIndex);
		
		Triangle triangle = new Triangle (a.vertexIndex, b.vertexIndex, c.vertexIndex);
		AddTriangleToDictionary (triangle.vertexIndexA, triangle);
		AddTriangleToDictionary (triangle.vertexIndexB, triangle);
		AddTriangleToDictionary (triangle.vertexIndexC, triangle);
		
	}
	
	
	void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
	{
		if (triangleDictionary.ContainsKey (vertexIndexKey)) {
			triangleDictionary [vertexIndexKey].Add (triangle);
		} else {
			List<Triangle> triangleList = new List<Triangle>();
			triangleList.Add(triangle);
			triangleDictionary.Add(vertexIndexKey, triangleList);
		}
	}
	
	void CalculateMeshOutlines()
	{
		for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
			if (!checkedVertices.Contains(vertexIndex)){
				int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
				// If it is = -1 it means it couldn't find an outline for this vertex index
				if (newOutlineVertex != -1){
					checkedVertices.Add(vertexIndex);
					
					List<int> newOutline = new List<int>();
					newOutline.Add(vertexIndex);
					outlines.Add(newOutline);
					FollowOutline(newOutlineVertex, outlines.Count - 1);
					outlines[outlines.Count - 1].Add(vertexIndex);
				}
			}
		}
        
    }



    void FollowOutline(int vertexIndex, int outlineIndex)
	{
		outlines [outlineIndex].Add (vertexIndex);
		checkedVertices.Add (vertexIndex);
		int nextVertexIndex = GetConnectedOutlineVertex (vertexIndex);
		
		if (nextVertexIndex != -1) {
			// continue following the outline
			FollowOutline(nextVertexIndex, outlineIndex);
		}
	}
	
	int GetConnectedOutlineVertex(int vertexIndex)
	{
		List<Triangle> trianglesContainingVertex = triangleDictionary [vertexIndex];
		
		for (int i =0; i < trianglesContainingVertex.Count; i++){
			Triangle triangle = trianglesContainingVertex[i];
			
			for (int j = 0; j< 3; j++){
				int vertexB = triangle[j];
				if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB)){
					if (IsOutlineEdge(vertexIndex, vertexB)){
						return vertexB;
					}
				}
			}
		}
		
		return -1;
	}
	
	bool IsOutlineEdge(int vertexA, int vertexB)
	{
		// is this an outline edge of a triangle or not (it is an outline if they only share 1 common triangle)
		List<Triangle> trianglesContainingVertexA = triangleDictionary [vertexA];
		int sharedTriangleCount = 0;
		
		for (int i =0; i< trianglesContainingVertexA.Count; i++) {
			if (trianglesContainingVertexA[i].Contains(vertexB)){
				sharedTriangleCount ++;
				if (sharedTriangleCount > 1){
					break;
				}
			}
		}
		
		return sharedTriangleCount == 1;
	}
	
	
	
	struct Triangle{
		public int vertexIndexA;
		public int vertexIndexB;
		public int vertexIndexC;
		int[] vertices;
		
		public Triangle (int a, int b, int c)
		{
			vertexIndexA = a;
			vertexIndexB = b;
			vertexIndexC = c;
			
			vertices = new int[3];
			vertices[0] = a;
			vertices[1] = b;
			vertices[2] = c;
			
			
		}
		
		public int this[int i]{
			get{ return vertices[i];}
		}
		
		public bool Contains(int vertexIndex)
		{
			return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
		}
		
	}
	
	public class SquareGrid{
		public Square[,] squares;
		
		public SquareGrid(int[,] map, float squareSize){
			
			int nodeCountX = map.GetLength(0);
			int nodeCountY = map.GetLength(1);
			
			float mapWidth = nodeCountX * squareSize;
			float mapHeight = nodeCountY * squareSize;
			
			ControlJunction[,] controlJunction = new ControlJunction[nodeCountX,nodeCountY];
			
			for (int x = 0; x < nodeCountX; x++){
				for (int y =0; y <nodeCountY; y++){
					Vector3 pos = new Vector3(-mapWidth/2 + x * squareSize + squareSize/2, 0, -mapHeight/2 + y * squareSize  + squareSize/2);
					controlJunction[x,y] = new ControlJunction(pos, map[x,y] == 1, squareSize);
				}
			}
			
			squares = new Square[nodeCountX - 1, nodeCountY - 1];
			
			for (int x = 0; x < nodeCountX-1; x++){
				for (int y =0; y <nodeCountY-1; y++){
					squares[x,y] = new Square(controlJunction[x,y+1], controlJunction[x+1,y+1], controlJunction[x+1,y], controlJunction[x,y]);
				}
			}
		}
	}
	
	
	public class Square{
		public ControlJunction topLeft, topRight, bottomRight, bottomLeft;
		public Junction centreTop, centreRight, centreBottom, centreLeft;
		public int configuration;
		
		
		public Square(ControlJunction _topLeft, ControlJunction _topRight, ControlJunction _bottomRight, ControlJunction _bottomLeft) {
			
			topLeft = _topLeft;
			topRight = _topRight;
			bottomRight = _bottomRight;
			bottomLeft = _bottomLeft;
			
			centreTop = topLeft.right;
			centreRight = bottomRight.above;
			centreBottom = bottomLeft.right;
			centreLeft = bottomLeft.above;
			
			if (topLeft.active)
				configuration += 8;
			if (topRight.active)
				configuration += 4;
			if (bottomRight.active)
				configuration += 2;
			if (bottomLeft.active)
				configuration += 1;
		}
	}
	
	public class Junction{
		public Vector3 position;
		public int vertexIndex = -1;
		
		public Junction (Vector3 _pos){
			position = _pos;
		}
	}
	
	public class ControlJunction : Junction{
		// If active it's a wall (1) if not it's empty (0)
		public bool active;
		public Junction above, right;
		
		public ControlJunction(Vector3 _pos, bool _active, float squareSize) : base (_pos){
			active = _active;
			above = new Junction(position + Vector3.forward * squareSize/2f);
			right = new Junction(position + Vector3.right * squareSize/2f);
			
		}
		
	}
}
