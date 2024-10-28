using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BugSpawner : MonoBehaviour
{
    public enum BugTypes { Fly };
    public Tilemap tilemap;
    public GameObject[] objectPrefabs;
    public float flyProbability = 0.2f;
    public int maxObject = 5;
    public float spawnInterval = 0.5f;

    private List<Vector3> validSpawnPositions = new List<Vector3>();
    private List<GameObject> spawnObjects = new List<GameObject>();
    private bool isSpawning = false;
    private int spawnCount = 0;

    void Start()
    {
        //Debug.Log("BugSpawner Start");
        GatherValidPositions();
        //Debug.Log("Valid positions gathered: " + validSpawnPositions.Count);
        StartCoroutine(SpawnObjectsIfNeeded());
    }

    void Update()
    {
        if (!isSpawning && ActiveObjectCount() < maxObject)
        {
            //Debug.Log("Starting SpawnObjectsIfNeeded coroutine");
            StartCoroutine(SpawnObjectsIfNeeded());
        }
    }

    private int ActiveObjectCount()
    {
        spawnObjects.RemoveAll(item => item == null);
        //Debug.Log(spawnObjects.Count);
        return spawnObjects.Count;
    }

    private IEnumerator SpawnObjectsIfNeeded()
    {
        isSpawning = true;
        while (ActiveObjectCount() < maxObject && spawnCount != maxObject)
        {
            if(validSpawnPositions.Count == 0) 
            {
                Debug.Log("No valid spawn positions.");
                break; 
            }
            Debug.Log("Spawning object, current active count: " + ActiveObjectCount());
            SpawnObject();
            yield return new WaitForSeconds(spawnInterval);
        }
        isSpawning = false;
    }

    private bool PositionHasObject(Vector3 position)
    {
        return spawnObjects.Any(checkObj => checkObj && Vector3.Distance(checkObj.transform.position, position) < 1.0f);
    }

    private void GatherValidPositions()
    {
        validSpawnPositions.Clear(); // In case our platforms changed due to next level

        BoundsInt boundsInt = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(boundsInt);
        //Debug.LogWarning("tilemap: " + tilemap.cellBounds);

        for (int x = 0; x < boundsInt.size.x; x++)
        {
            for (int y = 0; y < boundsInt.size.y; y++)
            {
                TileBase tile = allTiles[x + y * boundsInt.size.x];
                if (tile is LabeledTile labeledTile && labeledTile.label == "Grass")
                {
                    Vector3Int cellPosition = new Vector3Int(boundsInt.xMin + x, boundsInt.yMin + y, 0);
                    Vector3 worldPosition = tilemap.CellToWorld(cellPosition);

                    // Adjust the position to be on top of the tile, centered
                    worldPosition.x += tilemap.cellSize.x / 2;
                    worldPosition.y += tilemap.cellSize.y + 0.5f; // Add slight offset to ensure it's on top
                    validSpawnPositions.Add(worldPosition);
                    //Debug.Log("valid position added");
                }
            }
        }

        Debug.Log("Total valid positions found: " + validSpawnPositions.Count);
    }

    private void SpawnObject()
    {
        if (validSpawnPositions.Count == 0)
        {
            Debug.Log("No valid spawn positions available");
            return;
        }

        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;

        while (!validPositionFound && validSpawnPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, validSpawnPositions.Count);
            Vector3 potentialPosition = validSpawnPositions[randomIndex];
            Vector3 leftPosition = potentialPosition + Vector3.left;
            Vector3 rightPosition = potentialPosition + Vector3.right;

            if (!PositionHasObject(leftPosition) && !PositionHasObject(rightPosition))
            {
                spawnPosition = potentialPosition;
                validPositionFound = true;
            }
            validSpawnPositions.RemoveAt(randomIndex);
        }

        if (validPositionFound)
        {
            BugTypes objectType = BugTypes.Fly;
            GameObject gameObject = Instantiate(objectPrefabs[(int)objectType], spawnPosition, Quaternion.identity);
            if(spawnObjects.Count != maxObject)
            {
                spawnObjects.Add(gameObject);
                spawnCount++;
            }
            else
            {
                //finish the level

            }
        }
    }
}