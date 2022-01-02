using UnityEngine;

public class Tile
{
    public Vector2Int coordinates;
    public TileColor color;
    public TileSegment segment = TileSegment.defaultIcon;
    public int iconCount = 4;   // Number of icons that is added as sprite for each color


    // The index of the tile that corresponds order in the whole tile sprite list.
    public int tileIndex => (int) color * iconCount + (int)segment;
    public Tile(Vector2Int coordinates,int colorCount) {
        this.coordinates = coordinates;
        RandomColor(colorCount);
    }

    // Assign random color to the tile with the given count of color size
    public void RandomColor(int colorCount) {
        segment = TileSegment.defaultIcon;
        int rnd = Random.Range(0,colorCount);
        this.color = (TileColor) rnd;
    }
}
