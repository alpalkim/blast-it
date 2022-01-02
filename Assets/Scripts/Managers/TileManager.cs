using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public Sprite[] tileSprites;
    public GameObject tilePrefab;
    public List<TileController> tiles = new List<TileController>();

    public int row = 10;
    public int column = 10;
    public int colorCount = 6;
    public int conditionA = 4;
    public int conditionB = 7;
    public int conditionC = 9;
    private float tileWidth = 0.222f;

    public static TileManager instance;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

    private void Start() {
        GenerateSpawnPoints();
        StartCoroutine("GenearateTiles");
    }
    GameObject[] spawnPoint = new GameObject[10];
    private void GenerateSpawnPoints() {
        for (int i = 0; i < column; i++)
        {
            spawnPoint[i] = new GameObject();
            spawnPoint[i].name = "Column_" + i;
            // Generate spawn point locations at the above of the board based on column count
            spawnPoint[i].transform.position = new Vector3(tileWidth * (i*2+1) - tileWidth * column,3,0);
            spawnPoint[i].transform.parent = GameObject.Find("Tiles").transform;
        }
    }
 
    private IEnumerator GenearateTiles() {
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                GameObject tile = Instantiate(tilePrefab,spawnPoint[j].transform);
                tile.GetComponent<Rigidbody2D>().velocity = new Vector2(0,-2f);
                Vector2Int coordinates = new Vector2Int(j,i);
                Tile tileData = new Tile(coordinates,colorCount);
                tile.GetComponent<TileController>().Initialize(tileData);
                tiles.Add(tile.GetComponent<TileController>());
                yield return new WaitForSeconds(0.2f/column);
            }
        }
        yield return null;
        if (IsDeadlock())
            ShuffleBoard();
        StartCoroutine(ReassignIcons());
    }

    // Generate a tile at above the board when any tile is removed from the board
    private void GenearateTileAtColumn(Vector2Int coordinates) {
        GameObject tile = Instantiate(tilePrefab,spawnPoint[coordinates.x].transform);
        Tile tileData = new Tile(coordinates,colorCount);
        // Add velocity for better UX
        tile.GetComponent<Rigidbody2D>().velocity = new Vector2(0,-4f);
        tile.GetComponent<TileController>().Initialize(tileData);
        tiles.Add(tile.GetComponent<TileController>());
        StartCoroutine(ReassignCoordinates());
        if (IsDeadlock())
            ShuffleBoard();
        
        StartCoroutine(ReassignIcons());
    }

    // Change color of all tiles until deadlock is solved
    private void ShuffleBoard() {
        do
        foreach (TileController t in tiles)
        {
            t.tile.RandomColor(colorCount);
        }
        while (IsDeadlock());
    }

    // When any tile is clicked, tile removal process will began if the tile is in a group of same color 
    internal void TileClicked(TileController tileController) {
        List<TileController> sameColorNeighborTiles = new List<TileController>();
        sameColorNeighborTiles.Add(tileController);
        sameColorNeighborTiles = GetSameColorTiles(tileController,sameColorNeighborTiles);
        if (sameColorNeighborTiles.Count > 1) {
            StartCoroutine(ClearAndGenerateTiles(tileController,sameColorNeighborTiles));
        }
    }

    private IEnumerator ClearAndGenerateTiles(TileController tileController,List<TileController> sameColorNeighborTiles) {
        Vector2Int[] newTilesCoordinates = new Vector2Int[sameColorNeighborTiles.Count];
        for (int i = 0; i < sameColorNeighborTiles.Count; i++)
        {
            newTilesCoordinates[i] = sameColorNeighborTiles[i].tile.coordinates;
        }
        foreach (TileController t in sameColorNeighborTiles)
        {
            tiles.Remove(t);
            Destroy(t.gameObject);
        }
        foreach (Vector2Int coordinates in newTilesCoordinates)
        {
            GenearateTileAtColumn(coordinates);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // Reassign coordinates of each tile after relocation of tiles in the board
    private IEnumerator ReassignCoordinates() {
        yield return new WaitForEndOfFrame();
        foreach (TileController t in tiles)
        {
            t.tile.coordinates.y = t.transform.GetSiblingIndex();
            t.SetName();
            t.ResetTileSortingOrder();
        }
    }

    // Set icons of each tile regarding the size of their group
    private IEnumerator ReassignIcons() {
        yield return new WaitForEndOfFrame();

        // Group of tiles in same color will be added into a list to prevent check them again an again
        List<TileController> checkedTiles = new List<TileController>();

        foreach (TileController t in tiles)
        {
            if (checkedTiles.Contains(t)) continue;
            List<TileController> sameColorNeighborTiles = new List<TileController>();
            sameColorNeighborTiles.Add(t);
            sameColorNeighborTiles = GetSameColorTiles(t,sameColorNeighborTiles);
            if (sameColorNeighborTiles.Count > conditionC)
                SetIconsBySegment(sameColorNeighborTiles,TileSegment.thirdIcon);
            else if (sameColorNeighborTiles.Count > conditionB)
                SetIconsBySegment(sameColorNeighborTiles,TileSegment.secondIcon);
            else if (sameColorNeighborTiles.Count > conditionA)
                SetIconsBySegment(sameColorNeighborTiles,TileSegment.firstIcon);
            else
                SetIconsBySegment(sameColorNeighborTiles,TileSegment.defaultIcon);
            foreach (TileController checkedTile in sameColorNeighborTiles)
            {
                checkedTiles.Add(checkedTile);
            }
        }
    }

    // Check the deadlock situtation to shuffle the board
    private bool IsDeadlock() {

        foreach (TileController t in tiles)
        {
            List<TileController> sameColorNeighborTiles = new List<TileController>();
            sameColorNeighborTiles.Add(t);
            sameColorNeighborTiles = GetSameColorTiles(t,sameColorNeighborTiles);
            if (sameColorNeighborTiles.Count > 1)
                return false;
        }
        return true;
    }

    // Reset each tile's icon regarding their segment
    private void SetIconsBySegment(List<TileController> sameColorNeighborTiles,TileSegment segment){
        foreach (TileController t in sameColorNeighborTiles)
        {
            t.tile.segment = segment;
            t.ResetSprite();
        }
    }

    // This function is recursive to find chain of color group.
    // It adds same color tiles around a tile to the list and calls same function again with the added tile and the updated list.
    // It returns the whole list of chains at the end.
    public List<TileController> GetSameColorTiles(TileController tileController,List<TileController> sameColorNeighborTiles) {
        foreach (TileController t in tiles)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    if (Mathf.Abs(i) == 1 && Mathf.Abs(j) == 1) continue;
                    if (t.tile.coordinates.y + i == tileController.tile.coordinates.y &&
                        t.tile.coordinates.x + j == tileController.tile.coordinates.x &&
                        t.tile.color == tileController.tile.color)
                    {
                        if (sameColorNeighborTiles.Contains(t)) continue;
                        sameColorNeighborTiles.Add(t);
                        sameColorNeighborTiles = GetSameColorTiles(t,sameColorNeighborTiles);
                    }
                }
            }
        }
        return sameColorNeighborTiles;
    }
}
