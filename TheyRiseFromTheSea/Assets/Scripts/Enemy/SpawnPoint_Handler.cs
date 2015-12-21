using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SpawnPoint_Handler : MonoBehaviour {
//	public ResourceGrid resourceGrid;
//	[HideInInspector]
//	public Vector2[] spawnPositions;
//	[HideInInspector]
//	public List<Vector2> possiblePositions = new List<Vector2> ();

//	public List<Node>[] kamikazePaths;
//	public List<Node>[] paths;

//	public Vector2[] kamikazeDestinations;

//	public Map_Generator map_generator;

//	int map_width, map_height;

//	// Make the MIN Spawn X a 4th of the map's width and Spawn Y a 6th of the map's height
//	int minSpawnX, minSpawnY, maxSpawnX, maxSpawnY;

//	public int numberOfSpawnPositions = 5;

//	int _index = 0;

//	bool canGetPath, canGetKamikazePath;

//	void Awake(){
        
//		if (!map_generator) {
//			map_generator = GameObject.FindGameObjectWithTag ("Map").GetComponent<Map_Generator> ();
//			map_width = map_generator.width;
//			map_height = map_generator.height;
//			minSpawnX = map_width / 4;
//			minSpawnY = map_height / 6;
//			maxSpawnX = map_width - (map_width / 4);
//			maxSpawnY = map_height - (map_height / 6);
//		} else {
//			map_width = map_generator.width;
//			map_height = map_generator.height;
//			minSpawnX = map_width / 4;
//			minSpawnY = map_height / 6;
//			maxSpawnX = map_width - (map_width / 4);
//			maxSpawnY = map_height - (map_height / 6);
//		}
      

//		if (resourceGrid == null)
//			resourceGrid = GameObject.FindGameObjectWithTag ("Map").GetComponent<ResourceGrid> ();

		

//	}



//    /* Make this A LOT simpler by just getting the map width and height and assuming that it will always have at least
//	 *  a 2 Pixel border. So we have to get all the x positions at y = 2 and y = mapHeight - 2, then all the y positions
//	 * at x = 2 and x = mapWidth - 2 */
//    /*
//      void InitializeAllSpawnPositions(int waterTilesCount)
//      {

//          // Bottom
//          for (int y = minSpawnY; y <= minSpawnY + 4; y++) {
//              for (int x = minSpawnX; x < maxSpawnX; x++) {

//                  possiblePositions.Add(new Vector2(x,y)); 
//              }
//          }
//          //Top
//          for (int y = maxSpawnY - 4; y <= maxSpawnY; y++) {
//              for (int x = minSpawnX; x < maxSpawnX; x++) {

//                  possiblePositions.Add(new Vector2(x,y));

//              }
//          }
//          // Left ( more like top left)
//          for (int x = minSpawnX - 1; x <= minSpawnX; x++){
//              for (int y = maxSpawnY - minSpawnY; y <= maxSpawnY; y++)  {

//                  possiblePositions.Add(new Vector2(x,y));

//              }
//          }
//          // Right
//          for (int x = maxSpawnX -1; x <= maxSpawnX; x++){
//              for (int y = maxSpawnY - minSpawnY; y <= maxSpawnY; y++)  {

//                  possiblePositions.Add(new Vector2(x,y));

//              }
//          }

//          InitActualSpawnPositions ();
//      }

//      void InitActualSpawnPositions()
//      {
//          // Initialize the spawn positions array
//          spawnPositions = new Vector2[numberOfSpawnPositions];
//          for (int i = 0; i < spawnPositions.Length; i++) {
//              spawnPositions[i] = GetRandomSpawnPositions();
//  //			Debug.Log("SPAWN POS: " + spawnPositions[i] + " index:" + i);
//          }
//      }
//      Vector2 GetRandomSpawnPositions()
//      {
//          Vector2 returnVector = Vector2.zero;
//  //		int randomPositionPick = Random.Range (0, possibleSpawnPositions.Length / 2);
//          int randomPositionPick = Random.Range (0, possiblePositions.Count - 1);


//  //		return new Vector2 (possibleSpawnPositions [randomPositionPick].x, possibleSpawnPositions [randomPositionPick].y);
//          return new Vector2 (possiblePositions [randomPositionPick].x, possiblePositions [randomPositionPick].y);


//      }
//      */
//      void GetRandomKamikazeDestinations()
//      {
//          kamikazeDestinations = new Vector2[numberOfSpawnPositions];
//          // Loop through the Kamikaze Destinations array and fill each with a random X and Y
//          for (int x = 0; x < kamikazeDestinations.Length; x++) {
//              int randomKamikazeX = Random.Range(minSpawnX + 5, maxSpawnX - 5);
//              int randomKamikazeY = Random.Range(minSpawnY + 5, maxSpawnY - 5);
//              kamikazeDestinations[x] = new Vector2(randomKamikazeX, randomKamikazeY);
//          }
//      }


//      void GetSpawnPositionsInWater()
//      {
//          // Initialize the spawn positions array
//          spawnPositions = new Vector2[numberOfSpawnPositions];

//          if (resourceGrid)
//          {
//              for (int i = 0; i < spawnPositions.Length; i++)
//              {
//                if (resourceGrid.waterTilePositions.Count > 0)
//                {
//                    int randomWaterTile = Random.Range(0, resourceGrid.waterTilePositions.Count - 1);
//                    // pick from water tiles list
//                    if (randomWaterTile < resourceGrid.waterTilePositions.Count)
//                    {
//                        // Fill the spawn position
//                        spawnPositions[i] = resourceGrid.waterTilePositions[randomWaterTile];
//                        // Then remove it from the list so it doesn't get repeated
//                        resourceGrid.waterTilePositions.RemoveAt(randomWaterTile);
//                    }
//                }
//                else
//                {
//                    Debug.LogError("Grid is out of WATER TILES?!?!");
//                }
                
//              }

//          }
//      }

//      void Start () {

//          /*	InitializeAllSpawnPositions (resourceGrid.totalTilesThatAreWater); */
//        GetSpawnPositionsInWater();

//        // Create some random Kamikaze positions
//		GetRandomKamikazeDestinations ();
		
//		paths = new List<Node>[spawnPositions.Length];
//		kamikazePaths = new List<Node>[spawnPositions.Length]; 



//		if (resourceGrid != null) {
//			// Get Paths to capital from all Spawn Positions
//			for (int i =0; i< spawnPositions.Length; i++) {
//				resourceGrid.GenerateWalkPath (resourceGrid.capitalSpawnX, resourceGrid.capitalSpawnY, false, 
//			                              (int)spawnPositions [i].x, (int)spawnPositions [i].y);
//				if (resourceGrid.pathForEnemy != null)
//					FillPath (resourceGrid.pathForEnemy, i, false);
//			}

//			// Then get Paths to kamikaze destinations
//			for (int x =0; x< kamikazeDestinations.Length; x++) {
//				resourceGrid.GenerateWalkPath ((int)kamikazeDestinations[x].x, (int)kamikazeDestinations[x].y, false, 
//				                               (int)spawnPositions [x].x, (int)spawnPositions [x].y);
//				if (resourceGrid.pathForEnemy != null)
//					FillPath (resourceGrid.pathForEnemy, x, true);
//			}

////			canGetPath = true;
////			_index = 0;
//		}

//	}

////	void Update()
////	{
////		if (canGetPath) {
////			StartCoroutine(GetPath());
////		}
////
////		if (canGetKamikazePath && !canGetPath) {
////			StartCoroutine(GetKamikazePath());
////		}
////		Debug.Log (_index);
////	}

////	IEnumerator GetPath()
////	{
////		canGetPath = false;
////
////		// get a path
////		resourceGrid.GenerateWalkPath(resourceGrid.capitalSpawnX, resourceGrid.capitalSpawnY, false, 
////		                              (int)spawnPositions [_index].x, (int)spawnPositions [_index].y);
////		yield return new WaitForSeconds (0.05f);
////		// fill path
////		if (resourceGrid.pathForEnemy != null)
////			FillPath (resourceGrid.pathForEnemy, _index, false);
////
////		if (_index < numberOfSpawnPositions) {
////			_index++;
////			canGetPath = true;
////		}else{
////			_index = 0;
////			canGetKamikazePath = true;
////
////			yield break;
////		}
////	}
////
////	
////	IEnumerator GetKamikazePath()
////	{
////		canGetKamikazePath = false;
////		
////		// get a path
////		resourceGrid.GenerateWalkPath ((int)kamikazeDestinations[_index].x, (int)kamikazeDestinations[_index].y, false, 
////		                               (int)spawnPositions [_index].x, (int)spawnPositions [_index].y);
////		yield return new WaitForSeconds (0.05f);
////		// fill path
////		if (resourceGrid.pathForEnemy != null)
////			FillPath (resourceGrid.pathForEnemy, _index, true);
////		
////		if (_index < numberOfSpawnPositions) {
////			_index++;
////			canGetKamikazePath = true;
////		}else{
////			
////			yield break;
////		}
////	}


//	void FillPath(List<Node> currPath, int i, bool trueIfKamikaze){

//		if (!trueIfKamikaze) {
//			paths [i] = new List<Node> ();
//			for (int p = 0; p < currPath.Count; p++) {
//				paths [i].Add (currPath [p]);
//			}

//		//Debug.Log ("PATH TO CAPITAL: " + i + " From: " + paths [i] [0].x + " " + paths [i] [0].y + " To: " + paths [i] [paths [i].Count - 1].x + " " + paths [i] [paths [i].Count - 1].y);

//		} else {
//			kamikazePaths [i] = new List<Node> ();
//			for (int k = 0; k < currPath.Count; k++) {
//				kamikazePaths [i].Add (currPath [k]);
//			}
////			Debug.Log ("KAMIKAZE PATH: " + i + " From: " + kamikazePaths [i] [0].x + " " + kamikazePaths [i] [0].y + " To: " + kamikazePaths [i] [kamikazePaths [i].Count - 1].x + " " + kamikazePaths [i] [kamikazePaths [i].Count - 1].y);

//		}
//	}
	
}
