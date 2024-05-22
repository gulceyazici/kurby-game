using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BugSpawner : MonoBehaviour
{
    public enum BugTypes { Fly };
    public Tilemap tilemap;
    public GameObject[] objectPrefabs;
    public float flyProbility = 0.2f;
    public int maxObject = 5;
    public float spawnInterval = 0.5f;

    private List<Vector3> validSpawnPositions = new List<Vector3>();
    private List<GameObject> spawnObjects = new List<GameObject>();
    private bool isSpawning = false;
    // Start is called before the first frame update
    void Start()
    {
        //We need to gather valid positions
        GatherValidPositions();
        StartCoroutine(SpawnObjectsIfNeeded());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private int ActiveObjectCount()
    {
        spawnObjects.RemoveAll(item => item == null);
        return spawnObjects.Count;
    }
    private IEnumerator SpawnObjectsIfNeeded()
    {
        isSpawning= true;
        while(ActiveObjectCount() < maxObject)
        {
            yield return new WaitForSeconds(spawnInterval); //waiting while spawning new object
        }
        isSpawning= false;
    }
    private bool PositionHasObject(Vector3 position)
    {
        return spawnObjects.Any(checkObj => checkObj && Vector3.Distance(checkObj.transform.position, position) < 1.0f);
    }

    private void SpawnObject()
    {
        if (validSpawnPositions.Count == 0) return;
        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;

        while(!validPositionFound && validSpawnPositions.Count> 0)
        {
            int randomIndex = Random.Range(0, validSpawnPositions.Count);
            Vector3 potentialPosition = validSpawnPositions[randomIndex];
            Vector3 leftPosition = potentialPosition + Vector3.left;
            Vector3 rigthPosition = potentialPosition + Vector3.right;

            if(!PositionHasObject(leftPosition) && !PositionHasObject(rigthPosition))
            {
                spawnPosition = potentialPosition;
                validPositionFound= true;
            }
            validSpawnPositions.RemoveAt(randomIndex);
        }
        //if(validPositionFound)
        //{
        //    ObjectType objectType = 
        //}
    }

    private void GatherValidPositions()
    {
        validSpawnPositions.Clear(); //in case if our platforms changed due to next level
        //-----------------------------------------------------------
        BoundsInt boundsInt = tilemap.cellBounds; 
        TileBase[] allTilles = tilemap.GetTilesBlock(boundsInt);
        Vector3 start = tilemap.CellToWorld(new Vector3Int(boundsInt.xMin, boundsInt.yMin, 0));

        for(int x = 0; x <boundsInt.size.x; x++)
        {

            for (int y = 0; y < boundsInt.size.y; y++)
            {
                TileBase tile = allTilles[x + y * boundsInt.size.x];
                if(tile != null)
                {
                    Vector3 place = start + new Vector3(x + 0.5f, y + 2f, 0);
                    validSpawnPositions.Add(place);
                }
            }

        }
    }
}
