using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct WFCTile
{
    public Tile tile;
    public List<int> topNeighbors;
    public List<int> rightNeighbors;
    public List<int> bottomNeighbors;
    public List<int> leftNeighbors;
}
