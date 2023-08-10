using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using XNode;

public class WFCManager : MonoBehaviour
{
    [SerializeField] WFCGraph wfcGraph;
    [SerializeField] int width;
    [SerializeField] int height;

    // Start is called before the first frame update
    void Start()
    {
        GenerateTiles();
    }

    void GenerateTiles()
    {
        /*List<WFCTile> tiles = new List<WFCTile>();
        foreach (Node node in wfcGraph.nodes)
        {
            WFCNode n = node as WFCNode;
            if (n != null && CheckIfValid(n))
            {
                WFCTile tile = new WFCTile();
                tile.image = n.tile;
                tile.rotation = 0;
                tile.sprockets = new int[4] { n.topSocket, n.rightSocket, n.bottomSocket, n.leftSocket };
                tiles.Add(tile);
                if (n.rotatable)
                {
                    for (int i = 1; i < 4; i++)
                    {
                        WFCTile subTile = new WFCTile();
                        subTile.image = n.tile;
                        subTile.rotation = 90 * i;
                        subTile.sprockets = new int[4] { n.topSocket, n.rightSocket, n.bottomSocket, n.leftSocket };
                        tiles.Add(subTile);
                    }
                }
            }
        }*/
    }

    bool CheckIfValid(WFCNode node) => node.topSocket >= 0 && node.rightSocket >= 0 && node.bottomSocket >= 0 && node.leftSocket >= 0;
}
