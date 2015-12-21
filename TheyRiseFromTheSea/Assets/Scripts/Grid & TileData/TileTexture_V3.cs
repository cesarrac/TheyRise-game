using UnityEngine;
using System.Collections;

public class TileTexture_V3 : MonoBehaviour {

    [Header("Tile Sheet Settings:")]
    public Texture2D tileSheet;
    public int tileResolution = 32;
    public int tilesPerRow, numberOfRows;

    // Tile types:
    enum TilePositionTypes
    {
        BOTTOM, TOP, LEFT, RIGHT,
        BOTTOM_LEFT_CORNER, BOTTOM_RIGHT_CORNER,
        TOP_LEFT_CORNER, TOP_RIGHT_CORNER,
        LEFT_BOTTOM_DIAG, RIGHT_BOTTOM_DIAG,
        LEFT_TOP_DIAG, RIGHT_TOP_DIAG, CENTER
    }

    enum TileEdgeTypes{ CLIFF_SHORE, SHORE_CLIFF, CLIFF, SHORE, CENTER, CLEAR, UNDEFINED }

    enum TileLandTypes { ASH, SAND, MUD }

    // Renderer Component
    MeshRenderer mesh_renderer;


}
