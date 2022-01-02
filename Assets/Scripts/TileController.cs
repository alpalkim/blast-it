using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Tile tile;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void Initialize(Tile tile) {
        this.tile = tile;
        spriteRenderer.sprite = TileManager.instance.tileSprites[tile.tileIndex];
        SetName();
        // The sprite sorting order is used to make the tiles seen on top of each other
        spriteRenderer.sortingOrder = tile.coordinates.y;
    }
    
    public void SetName() {
        name = string.Format("Tile: {0}, {1}",tile.coordinates.x.ToString(),tile.coordinates.y.ToString());
    }
    public void ResetSprite() {
        spriteRenderer.sprite = TileManager.instance.tileSprites[tile.tileIndex];
    }
    
    public void ResetTileSortingOrder() {
        spriteRenderer.sortingOrder = tile.coordinates.y;
    }
    
    private void OnMouseDown() {
        TileManager.instance.TileClicked(this);
    }
}
