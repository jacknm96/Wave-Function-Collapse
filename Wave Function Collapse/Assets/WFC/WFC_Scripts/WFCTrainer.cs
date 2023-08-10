using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class WFCTrainer : MonoBehaviour
{
    [SerializeField] Texture2D spriteMap;
    [SerializeField] Texture2D trainingImage;
    //[HideInInspector, SerializeField] Sprite[] sprites;
    [HideInInspector, SerializeField] List<Texture2D> textures;
    //[HideInInspector, SerializeField] Dictionary<Texture2D, List<Texture2D>> neighbors;
    //[HideInInspector, SerializeField] Color[][] spriteMapPixels;

    [SerializeField] Tilemap map;

    int[,] trainingTileArray;
    WFCTile[] possibleTiles;

    private void OnValidate()
    {
        if (spriteMap != null)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(spriteMap.name);
            textures = new List<Texture2D>();
            //neighbors = new Dictionary<Texture2D, List<Texture2D>>();
            //spriteMapPixels = new Color[sprites.Length][];
            if (sprites.Length == 0)
            {
                spriteMap = null;
                throw new System.Exception("Sprite Map must be multiple sprites");
            } else
            {
                int tileSize = (int)sprites[0].rect.width;
                for (int i = 0; i < sprites.Length; i++)
                {
                    //spriteMapPixels[i] = spriteMap.GetPixels((int)sprites[i].rect.x, (int)sprites[i].rect.y, tileSize, tileSize);
                    Texture2D temp = new Texture2D(tileSize, tileSize);
                    temp.SetPixels(0, 0, tileSize, tileSize, spriteMap.GetPixels((int)sprites[i].rect.x, (int)sprites[i].rect.y, tileSize, tileSize));
                    temp.filterMode = FilterMode.Point;
                    temp.wrapMode = TextureWrapMode.Clamp;
                    //temp.SetPixels(0, 0, tileSize, tileSize, sprites[i].texture.GetPixels());
                    temp.Apply(); 
                    textures.Add(temp);
                    Texture2D tempRot = temp;

                    // add rotated textures
                    for (int j = 1; j < 4; j++)
                    {
                        //Texture2D tempRot = RotateTexture(temp, 90 * j);
                        tempRot = RotateTexture(tempRot, true);
                        if (!ComparePixels(temp.GetPixels(0, 0, tileSize, tileSize), tempRot.GetPixels(0, 0, tileSize, tileSize)))
                        {
                            textures.Add(tempRot);
                        }
                    }
                    // add flipped textures
                    tempRot = FlipTextureHorizontal(temp);
                    if (!ComparePixels(temp.GetPixels(0, 0, tileSize, tileSize), tempRot.GetPixels(0, 0, tileSize, tileSize)))
                        textures.Add(tempRot);
                    tempRot = FlipTextureVertical(temp);
                    if (!ComparePixels(temp.GetPixels(0, 0, tileSize, tileSize), tempRot.GetPixels(0, 0, tileSize, tileSize)))
                        textures.Add(tempRot);
                }
                /*for (int i = 0; i < textures.Count; i++)
                {
                    neighbors.Add(textures[i], new List<Texture2D>());
                }*/
                Debug.Log($"Number of sprites: {sprites.Length}; Number of rotated textures: {textures.Count}");
                ParseTrainingData();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //possibleTiles = new WFCTile[textures.Count];
        //ParseTrainingData();
        //StartCoroutine(VisuallyShowParse());
        //PopulateTileButtons();
        //StartCoroutine(ShowWFC());
        GenerateNewMap();
        //Test();
    }

    void Test()
    {
        int iMax = trainingImage.width / 8;
        int jMax = trainingImage.height / 8;
        for (int i = 0; i < iMax; i++)
        {
            for (int j = 0; j < jMax; j++)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                Texture2D temp = new Texture2D(8, 8);
                temp.SetPixels(trainingImage.GetPixels(i * 8, j * 8, 8, 8));
                temp.filterMode = FilterMode.Point;
                temp.wrapMode = TextureWrapMode.Clamp;
                temp.Apply();
                tile.sprite = Sprite.Create(
                            temp,
                            new Rect(0, 0, 8, 8),
                            new Vector2(0.5f, 0.5f),
                            8f);
                /*tile.sprite = Sprite.Create(
                            trainingImage,
                            new Rect(i * 8, j * 8, 8, 8),
                            new Vector2(0.5f, 0.5f),
                            8f);*/
                map.SetTile(new Vector3Int(i, j, 0), tile);
            }
        }
    }

    void ParseTrainingData()
    {
        //int tileSize = (int)Mathf.Sqrt(spriteMapPixels[0].Length);
        int tileSize = textures[0].width;
        int trainingWidth = (int)trainingImage.width;
        int trainingHeight = (int)trainingImage.height;
        Debug.Log(tileSize);
        Debug.Log(trainingWidth + "x" + trainingHeight);
        int numFound = 0;
        trainingTileArray = new int[trainingWidth / tileSize, trainingHeight / tileSize];
        possibleTiles = new WFCTile[textures.Count];
        int iMax = trainingTileArray.GetLength(0);
        int jMax = trainingTileArray.GetLength(1);

        // initialize training array to be -1
        for (int i = 0; i < iMax; i++)
        {
            for (int j = 0; j < jMax; j++)
            {
                trainingTileArray[i, j] = -1;
            }
        }

        int count = 0;
        //Texture2D temp = new Texture2D(tileSize, tileSize);
        for (int i = 0; i < iMax; i++)
        {
            for (int j = 0; j < jMax; j++)
            {
                Color[] pixels = trainingImage.GetPixels(i * tileSize, j * tileSize, tileSize, tileSize);
                int textureIndex = CompareTile(pixels, tileSize);
                if (textureIndex >= 0)
                {
                    numFound++;
                    if (possibleTiles[textureIndex].Equals(default(WFCTile)))
                    {
                        count++;
                        possibleTiles[textureIndex] = new WFCTile();
                        possibleTiles[textureIndex].tile = ScriptableObject.CreateInstance<Tile>();
                        possibleTiles[textureIndex].tile.sprite = Sprite.Create(
                            textures[textureIndex],
                            new Rect(0, 0, textures[textureIndex].width, textures[textureIndex].height),
                            new Vector2(0.5f, 0.5f),
                            tileSize);
                        
                        /*possibleTiles[textureIndex].tile.sprite = Sprite.Create(
                            trainingImage,
                            new Rect(i * tileSize, j * tileSize, tileSize, tileSize),
                            new Vector2(0.5f, 0.5f),
                            tileSize);*/
                        possibleTiles[textureIndex].topNeighbors = new List<int>();
                        possibleTiles[textureIndex].rightNeighbors = new List<int>();
                        possibleTiles[textureIndex].bottomNeighbors = new List<int>();
                        possibleTiles[textureIndex].leftNeighbors = new List<int>();
                    }
                    trainingTileArray[i, j] = textureIndex;
                }
                /*temp.SetPixels(0, 0, tileSize, tileSize, pixels);
                for (int d = 1; d < 4; d++)
                {
                    pixels = RotateTexture(temp, 90 * d).GetPixels(0, 0, tileSize, tileSize);
                    if (CompareTile(pixels, tileSize) >= 0)
                    {
                        numFound++;
                        break;
                    }
                }*/
            }
        }
        // get rid of unused tiles
        WFCTile[] temp = new WFCTile[count];
        int c = 0;
        for (int i = 0; i < possibleTiles.Length; i++)
        {
            if (!possibleTiles[i].Equals(default(WFCTile)))
            {
                temp[c] = possibleTiles[i];
                // reassign trainingTileArray values to new index
                for (int x = 0; x < iMax; x++)
                    for (int y = 0; y < jMax; y++)
                        if (trainingTileArray[x, y] == i)
                            trainingTileArray[x, y] = c;
                c++;
            } else
            {
                // destroy unused tiles
                ScriptableObject.DestroyImmediate(possibleTiles[i].tile);
            }
        } 
        possibleTiles = temp;
        for (int i = 0; i < iMax; i++)
        {
            for (int j = 0; j < jMax; j++)
            {
                if (trainingTileArray[i, j] == -1)
                    continue;
                try
                {
                    if (i > 0 && !possibleTiles[trainingTileArray[i, j]].leftNeighbors.Contains(trainingTileArray[i - 1, j]))
                        possibleTiles[trainingTileArray[i, j]].leftNeighbors.Add(trainingTileArray[i - 1, j]);
                    if (i < iMax - 1 && !possibleTiles[trainingTileArray[i, j]].rightNeighbors.Contains(trainingTileArray[i + 1, j]))
                        possibleTiles[trainingTileArray[i, j]].rightNeighbors.Add(trainingTileArray[i + 1, j]);
                    if (j > 0 && !possibleTiles[trainingTileArray[i, j]].bottomNeighbors.Contains(trainingTileArray[i, j - 1]))
                        possibleTiles[trainingTileArray[i, j]].bottomNeighbors.Add(trainingTileArray[i, j - 1]);
                    if (j < jMax - 1 && !possibleTiles[trainingTileArray[i, j]].topNeighbors.Contains(trainingTileArray[i, j + 1]))
                        possibleTiles[trainingTileArray[i, j]].topNeighbors.Add(trainingTileArray[i, j + 1]);
                } catch (System.IndexOutOfRangeException e)
                {
                    Debug.LogException(e);
                }
            }
        } 
        Debug.Log(numFound);
        Debug.Log($"Index 0 left count: {possibleTiles[0].leftNeighbors.Count}, right count: {possibleTiles[0].rightNeighbors.Count}, bottom count: {possibleTiles[0].bottomNeighbors.Count}, top count: {possibleTiles[0].topNeighbors.Count}");
    }

    void DrawMap()
    {
        for (int i = 0; i < trainingTileArray.GetLength(1); i++) 
        {
            for (int j = 0; j < trainingTileArray.GetLength(0); j++)
            {
                if (!trainingTileArray[i, j].Equals(default(WFCTile))) 
                    map.SetTile(new Vector3Int(i, j, 0), possibleTiles[trainingTileArray[i, j]].tile);
            }
        } 
    }

    int CompareTile(Color[] pixels, int tileSize)
    {
        for (int i = 0; i < textures.Count; i++)
        {
            if (ComparePixels(pixels, textures[i].GetPixels(0, 0, tileSize, tileSize)))
                return i;
        }
        return -1;
    }

    bool ComparePixels(Color[] trainingPixels, Color[] spritePixels)
    {
        if (trainingPixels.Length != spritePixels.Length)
            return false;
        for (int i = 0; i < trainingPixels.Length; i++)
            if (trainingPixels[i] != spritePixels[i])
                return false;
        return true;
    }

    /*bool AlreadyHaveTile(int index)
    {
        foreach (WFCTile tile in possibleTiles)
            if (tile.tile.sprite == sprites[index])
                return true;
        return false;
    }*/

    #region Rotation

    /*Texture2D RotateTexture(Texture2D tex, float angle)
    {
        Texture2D rotImage = new Texture2D(tex.width, tex.height);
        int x, y;
        float x1, y1, x2, y2;

        int w = tex.width;
        int h = tex.height;
        float x0 = RotateX(angle, -w / 2.0f, -h / 2.0f) + w / 2.0f;
        float y0 = RotateY(angle, -w / 2.0f, -h / 2.0f) + h / 2.0f;

        float dx_x = RotateX(angle, 1.0f, 0.0f);
        float dx_y = RotateY(angle, 1.0f, 0.0f);
        float dy_x = RotateX(angle, 0.0f, 1.0f);
        float dy_y = RotateY(angle, 0.0f, 1.0f);


        x1 = x0;
        y1 = y0;

        for (x = 0; x < tex.width; x++)
        {
            x2 = x1;
            y2 = y1;
            for (y = 0; y < tex.height; y++)
            {
                //rotImage.SetPixel (x1, y1, Color.clear);          

                x2 += dx_x;//rot_x(angle, x1, y1);
                y2 += dx_y;//rot_y(angle, x1, y1);
                rotImage.SetPixel((int)Mathf.Floor(x), (int)Mathf.Floor(y), GetPixel(tex, x2, y2));
            }

            x1 += dy_x;
            y1 += dy_y;

        }

        rotImage.Apply();
        return rotImage;
    }*/

    Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
    {
        Color[] original = originalTexture.GetPixels();
        Color[] rotated = new Color[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    Texture2D FlipTextureHorizontal(Texture2D originalTexture)
    {
        Color[] original = originalTexture.GetPixels();
        Color[] flipped = new Color[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iFlipped, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iFlipped = i + j * w;
                iOriginal = (w - i - 1) + j * w;
                flipped[iFlipped] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels(flipped);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    Texture2D FlipTextureVertical(Texture2D originalTexture)
    {
        Color[] original = originalTexture.GetPixels();
        Color[] flipped = new Color[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iFlipped, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iFlipped = i + j * w;
                iOriginal = i + (h - j - 1) * w;
                flipped[iFlipped] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels(flipped);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    private Color GetPixel(Texture2D tex, float x, float y)
    {
        Color pix;
        int x1 = (int)Mathf.Floor(x);
        int y1 = (int)Mathf.Floor(y);

        if (x1 > tex.width || x1 < 0 ||
           y1 > tex.height || y1 < 0)
        {
            pix = Color.clear;
        }
        else
        {
            pix = tex.GetPixel(x1, y1);
        }

        return pix;
    }

    private float RotateX(float angle, float x, float y)
    {
        float cos = Mathf.Cos(angle / 180.0f * Mathf.PI);
        float sin = Mathf.Sin(angle / 180.0f * Mathf.PI);
        return (x * cos + y * (-sin));
    }
    private float RotateY(float angle, float x, float y)
    {
        float cos = Mathf.Cos(angle / 180.0f * Mathf.PI);
        float sin = Mathf.Sin(angle / 180.0f * Mathf.PI);
        return (x * sin + y * cos);
    }

    #endregion

    #region Visual Coroutine

    [SerializeField] int generatedMapWidth;
    [SerializeField] int generatedMapHeight;

    public void GenerateNewMap()
    {
        StopAllCoroutines();
        StartCoroutine(ShowWFC());
    }

    IEnumerator ShowWFC()
    {
        bool[,] collapsedTiles = new bool[generatedMapWidth, generatedMapHeight];
        List<int>[,] possibilities = new List<int>[generatedMapWidth, generatedMapHeight];

        // initialize possibilities
        for (int i = 0; i < generatedMapWidth; i++)
        {
            for (int j = 0; j < generatedMapHeight; j++)
            {
                possibilities[i, j] = new List<int>();
                for (int x = 0; x < possibleTiles.Length; x++)
                {
                    /*// must create new tile in memory, otherwise neighbor collapses will be shared across all tiles of same type
                    WFCTile temp = new WFCTile();
                    temp.tile = t.tile;
                    temp.topNeighbors = new List<int>();
                    temp.rightNeighbors = new List<int>();
                    temp.bottomNeighbors = new List<int>();
                    temp.leftNeighbors = new List<int>();
                    for (int x = 0; x < possibleTiles.Length; x++)
                    {
                        int holder = x;
                        temp.topNeighbors.Add(holder);
                        temp.rightNeighbors.Add(holder);
                        temp.bottomNeighbors.Add(holder);
                        temp.leftNeighbors.Add(holder);
                    }*/
                    possibilities[i, j].Add(x);
                }
            }
        }
        map.ClearAllTiles();
        yield return null;
        // collapse
        int[] toCollapse = new int[2] { Random.Range(0, generatedMapWidth), Random.Range(0, generatedMapHeight) };
        //List<int[]> nodesToCollapse;
        while (toCollapse != null)
        {
            /*nodesToCollapse = new List<int[]>();
            nodesToCollapse.Add(toCollapse);
            while (nodesToCollapse.Count > 0)
            {
                int index = Random.Range(0, nodesToCollapse.Count);
                toCollapse = nodesToCollapse[index];
                nodesToCollapse.RemoveAt(index);
                if (collapsedTiles[toCollapse[0], toCollapse[1]])
                    continue;
                
                WaveCollapse(toCollapse, ref collapsedTiles, ref possibilities);
                yield return null;
            }

            toCollapse = FindLowestEntropy(collapsedTiles, possibilities);
            if (toCollapse == null)
                break;*/
            int randIndex = Random.Range(0, possibilities[toCollapse[0], toCollapse[1]].Count);
            try { int chosen = possibilities[toCollapse[0], toCollapse[1]][randIndex];
            possibilities[toCollapse[0], toCollapse[1]].Clear();
            possibilities[toCollapse[0], toCollapse[1]].Add(chosen);
            collapsedTiles[toCollapse[0], toCollapse[1]] = true;
            map.SetTile(new Vector3Int(toCollapse[0], toCollapse[1], 0), possibleTiles[chosen].tile); }

            catch (System.ArgumentOutOfRangeException e) { Debug.Log("catching"); }

            Collapse(toCollapse, ref collapsedTiles, ref possibilities);

            toCollapse = FindLowestEntropy(collapsedTiles, possibilities);

            yield return null;
        }
    }

    int[] FindLowestEntropy(bool[,] collapsed, List<int>[,] possibilities)
    {
        int lowest = possibleTiles.Length + 1;
        int[] node = null;
        for (int i = 0; i < possibilities.GetLength(0); i++)
        {
            for (int j = 0; j < possibilities.GetLength(1); j++)
            {
                if (!collapsed[i, j] && possibilities[i, j].Count < lowest)
                {
                    lowest = possibilities[i, j].Count;
                    node = new int[2] { i, j };
                }
            }
        }
        return node;
    }

    void WaveCollapse(int[] node, ref bool[,] collapsedTiles, ref List<int>[,] possibilities)
    {
        //List<int> p = possibilities[node[0], node[1]];
        if (possibilities[node[0], node[1]].Count <= 0)
        {
            Debug.Log("node got to zero possibilities");
            return;
        }
        int chosen = possibilities[node[0], node[1]][Random.Range(0, possibilities[node[0], node[1]].Count)];
        possibilities[node[0], node[1]].Clear();
        possibilities[node[0], node[1]].Add(chosen);
        collapsedTiles[node[0], node[1]] = true;
        map.SetTile(new Vector3Int(node[0], node[1], 0), possibleTiles[chosen].tile);

        Stack<int[]> nodesToCollapse = new Stack<int[]>();
        nodesToCollapse.Push(node);

        //List<int[]> toPropogate = new List<int[]>();
        while (nodesToCollapse.Count > 0)
        {
            node = nodesToCollapse.Pop();
            //nodesToCollapse.RemoveAt(0);
            List<int> toRemove = new List<int>();
            // check top neighbors
            if (node[1] + 1 < generatedMapHeight && !collapsedTiles[node[0], node[1] + 1])
            {
                toRemove.Clear();
                int x = node[0];
                int y = node[1] + 1;
                /*foreach (int t in possibilities[x, y])
                    foreach (int b in possibilities[node[0], node[1]])
                        if (!possibleTiles[b].topNeighbors.Contains(t))
                            toRemove.Add(t);*/
                int before = possibilities[x, y].Count;
                List<int> allPossibles = new List<int>();
                foreach (int t in possibilities[node[0], node[1]])
                    allPossibles.AddRange(possibleTiles[t].topNeighbors);
                possibilities[x, y] = allPossibles.Intersect(possibilities[x, y]).ToList();
                if (possibilities[x, y].Count < before)
                {
                    /*foreach (int t in toRemove)
                        possibilities[x, y].Remove(t);*/
                    //AddNodeToCollapse(new int[2] { x, y }, ref nodesToCollapse);
                    if (possibilities[x, y].Count == 1)
                    {
                        chosen = possibilities[x, y][0];
                        possibilities[x, y].Clear();
                        possibilities[x, y].Add(chosen);
                        collapsedTiles[x, y] = true;
                        map.SetTile(new Vector3Int(x, y, 0), possibleTiles[chosen].tile);
                    }
                    nodesToCollapse.Push(new int[2] { x, y });
                }

                /*if (possibilities[x, y].Count == 1 && !collapsedTiles[x, y])
                {
                    AddNodeToCollapse(new int[2] { x, y }, ref toPropogate);
                }*/
            }

            // check right neighbors
            if (node[0] + 1 < generatedMapWidth && !collapsedTiles[node[0] + 1, node[1]])
            {
                toRemove.Clear();
                int x = node[0] + 1;
                int y = node[1];
                /*foreach (int t in possibilities[x, y])
                    foreach (int b in possibilities[node[0], node[1]])
                        if (!possibleTiles[b].rightNeighbors.Contains(t))
                            toRemove.Add(t);
                if (toRemove.Count > 0)
                {
                    foreach (int t in toRemove)
                        possibilities[x, y].Remove(t);
                    if (possibilities[x, y].Count == 1)
                    {
                        chosen = possibilities[x, y][0];
                        possibilities[x, y].Clear();
                        possibilities[x, y].Add(chosen);
                        collapsedTiles[x, y] = true;
                        map.SetTile(new Vector3Int(x, y, 0), possibleTiles[chosen].tile);
                    }
                    //AddNodeToCollapse(new int[2] { x, y }, ref nodesToCollapse);
                    nodesToCollapse.Push(new int[2] { x, y });
                }*/
                int before = possibilities[x, y].Count;
                List<int> allPossibles = new List<int>();
                foreach (int t in possibilities[node[0], node[1]])
                    allPossibles.AddRange(possibleTiles[t].topNeighbors);
                possibilities[x, y] = allPossibles.Intersect(possibilities[x, y]).ToList();
                if (possibilities[x, y].Count < before)
                {
                    /*foreach (int t in toRemove)
                        possibilities[x, y].Remove(t);*/
                    //AddNodeToCollapse(new int[2] { x, y }, ref nodesToCollapse);
                    if (possibilities[x, y].Count == 1)
                    {
                        chosen = possibilities[x, y][0];
                        possibilities[x, y].Clear();
                        possibilities[x, y].Add(chosen);
                        collapsedTiles[x, y] = true;
                        map.SetTile(new Vector3Int(x, y, 0), possibleTiles[chosen].tile);
                    }
                    nodesToCollapse.Push(new int[2] { x, y });
                }

                /*if (possibilities[x, y].Count == 1 && !collapsedTiles[x, y])
                {
                    AddNodeToCollapse(new int[2] { x, y }, ref toPropogate);
                }*/
            }

            // check bottom neighbors
            if (node[1] - 1 >= 0 && !collapsedTiles[node[0], node[1] - 1])
            {
                toRemove.Clear();
                int x = node[0];
                int y = node[1] - 1;
                /*foreach (int t in possibilities[x, y])
                    foreach (int b in possibilities[node[0], node[1]])
                        if (!possibleTiles[b].bottomNeighbors.Contains(t))
                            toRemove.Add(t);
                if (toRemove.Count > 0)
                {
                    foreach (int t in toRemove)
                        possibilities[x, y].Remove(t);
                    //AddNodeToCollapse(new int[2] { x, y }, ref nodesToCollapse);
                    if (possibilities[x, y].Count == 1)
                    {
                        chosen = possibilities[x, y][0];
                        possibilities[x, y].Clear();
                        possibilities[x, y].Add(chosen);
                        collapsedTiles[x, y] = true;
                        map.SetTile(new Vector3Int(x, y, 0), possibleTiles[chosen].tile);
                    }
                    nodesToCollapse.Push(new int[2] { x, y });
                }*/

                int before = possibilities[x, y].Count;
                List<int> allPossibles = new List<int>();
                foreach (int t in possibilities[node[0], node[1]])
                    allPossibles.AddRange(possibleTiles[t].topNeighbors);
                possibilities[x, y] = allPossibles.Intersect(possibilities[x, y]).ToList();
                if (possibilities[x, y].Count < before)
                {
                    /*foreach (int t in toRemove)
                        possibilities[x, y].Remove(t);*/
                    //AddNodeToCollapse(new int[2] { x, y }, ref nodesToCollapse);
                    if (possibilities[x, y].Count == 1)
                    {
                        chosen = possibilities[x, y][0];
                        possibilities[x, y].Clear();
                        possibilities[x, y].Add(chosen);
                        collapsedTiles[x, y] = true;
                        map.SetTile(new Vector3Int(x, y, 0), possibleTiles[chosen].tile);
                    }
                    nodesToCollapse.Push(new int[2] { x, y });
                }

                /*if (possibilities[x, y].Count == 1 && !collapsedTiles[x, y])
                {
                    AddNodeToCollapse(new int[2] { x, y }, ref toPropogate);
                }*/
            }

            // check left neighbors
            if (node[0] - 1 >= 0 && !collapsedTiles[node[0] - 1, node[1]])
            {
                toRemove.Clear();
                int x = node[0] - 1;
                int y = node[1];
                /*foreach (int t in possibilities[x, y])
                    foreach (int b in possibilities[node[0], node[1]])
                        if (!possibleTiles[b].leftNeighbors.Contains(t))
                            toRemove.Add(t);
                if (toRemove.Count > 0)
                {
                    foreach (int t in toRemove)
                        possibilities[x, y].Remove(t);
                    //AddNodeToCollapse(new int[2] { x, y }, ref nodesToCollapse);
                    if (possibilities[x, y].Count == 1)
                    {
                        chosen = possibilities[x, y][0];
                        possibilities[x, y].Clear();
                        possibilities[x, y].Add(chosen);
                        collapsedTiles[x, y] = true;
                        map.SetTile(new Vector3Int(x, y, 0), possibleTiles[chosen].tile);
                    }
                    nodesToCollapse.Push(new int[2] { x, y });
                }*/

                int before = possibilities[x, y].Count;
                List<int> allPossibles = new List<int>();
                foreach (int t in possibilities[node[0], node[1]])
                    allPossibles.AddRange(possibleTiles[t].topNeighbors);
                possibilities[x, y] = allPossibles.Intersect(possibilities[x, y]).ToList();
                if (possibilities[x, y].Count < before)
                {
                    /*foreach (int t in toRemove)
                        possibilities[x, y].Remove(t);*/
                    //AddNodeToCollapse(new int[2] { x, y }, ref nodesToCollapse);
                    if (possibilities[x, y].Count == 1)
                    {
                        chosen = possibilities[x, y][0];
                        possibilities[x, y].Clear();
                        possibilities[x, y].Add(chosen);
                        collapsedTiles[x, y] = true;
                        map.SetTile(new Vector3Int(x, y, 0), possibleTiles[chosen].tile);
                    }
                    nodesToCollapse.Push(new int[2] { x, y });
                }

                /*if (possibilities[x, y].Count == 1 && !collapsedTiles[x, y])
                {
                    AddNodeToCollapse(new int[2] { x, y }, ref toPropogate);
                }*/
            }
        }

        /*foreach (int[] n in toPropogate)
            try
            {
                WaveCollapse(n, ref collapsedTiles, ref possibilities);
            }
            catch { };*/
    }

    int[][] dirs = new int[4][] { new int[2] { 0, 1 }, new int[2] { 1, 0 }, new int[2] { 0, -1 }, new int[2] { -1, 0 } };
    void Collapse(int[] node, ref bool[,] collapsedTiles, ref List<int>[,] possibilities)
    {
        Stack<int[]> toCollapse = new Stack<int[]>();
        toCollapse.Push(node);
        while (toCollapse.Count > 0)
        {
            int[] n = toCollapse.Pop();
            for (int i = 0; i < 4; i++)
            {
                int[] dir = dirs[i];
                int x = n[0] + dir[0];
                int y = n[1] + dir[1];
                if (x < 0 || y < 0 || x >= generatedMapWidth || y >= generatedMapHeight)
                    continue;
                if (collapsedTiles[x, y])
                    continue;
                List<int> options = new List<int>();
                foreach (int t in possibilities[n[0], n[1]])
                {
                    List<int> neighbors;
                    switch (i)
                    {
                        case 0:
                            neighbors = possibleTiles[t].topNeighbors;
                            break;
                        case 1:
                            neighbors = possibleTiles[t].rightNeighbors;
                            break;
                        case 2:
                            neighbors = possibleTiles[t].bottomNeighbors;
                            break;
                        default:
                            neighbors = possibleTiles[t].leftNeighbors;
                            break;
                    }
                    foreach (int p in neighbors)
                        options.Add(p);
                }
                int before = possibilities[x, y].Count;
                options = options.Distinct().ToList();
                possibilities[x, y] = possibilities[x, y].Intersect(options).ToList();
                if (possibilities[x, y].Count < before)
                {
                    if (possibilities[x, y].Count > 0)
                        toCollapse.Push(new int[2] { x, y });
                    else // contraciction
                        GenerateNewMap();
                }   
                else if (possibilities[x, y].Count > before)
                    Debug.Log("How tf did we end up here");
            }
        }
    }

    /*void Propogate(int[] node, ref bool[,] collapsedTiles, ref List<int>[,] possibilities, ref List<int[]> toPropogate)
    {

    }*/

    void AddNodeToCollapse(int[] node, ref List<int[]> nodesToCollapse)
    {
        for (int i = 0; i < nodesToCollapse.Count; i++)
            if (node[0] == nodesToCollapse[i][0] && node[1] == nodesToCollapse[i][1])
                return;
        nodesToCollapse.Add(node);
    }

    [SerializeField] Image trainingVisual;
    [SerializeField] Image foundDisplayImage;
    [SerializeField] Image scrollingImage;
    //[SerializeField] float visualScale;
    [SerializeField] float waitTime;

    

    IEnumerator VisuallyShowParse()
    {
        Vector3[] corners = new Vector3[4];
        trainingVisual.rectTransform.GetWorldCorners(corners);
        int trainingWidth = (int)trainingImage.width;
        int trainingHeight = (int)trainingImage.height;

        float pixelsPerUnit = (float)(corners[3].x - corners[0].x) / trainingWidth;

        trainingVisual.sprite = Sprite.Create(trainingImage, new Rect(0, 0, trainingImage.width, trainingImage.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        int tileSize = textures[0].width;
        Vector3 origin = corners[0];
        origin.x += pixelsPerUnit * tileSize / 2;
        origin.y += pixelsPerUnit * tileSize / 2;
        /*origin.x -= trainingVisual.rectTransform.sizeDelta.x / 2;
        origin.y -= trainingVisual.rectTransform.sizeDelta.y / 2;*/
        /*float spriteOffsetX = trainingVisual.rectTransform.sizeDelta.x / tileSize;
        float spriteOffsetY = trainingVisual.rectTransform.sizeDelta.y / tileSize;*/
        scrollingImage.rectTransform.sizeDelta = new Vector2(tileSize, tileSize);
        //coord.z = spriteOffsetX;
        
        Debug.Log(tileSize);
        Debug.Log(trainingWidth + "x" + trainingHeight);
        int numFound = 0;
        Texture2D temp;

        Texture2D drawingTex = new Texture2D(trainingWidth, trainingHeight);
        drawingTex.SetPixels(trainingImage.GetPixels(0, 0, trainingWidth, trainingHeight));
        drawingTex.Apply();
        for (int i = 0; i < trainingWidth; i += tileSize)
        {
            for (int j = 0; j < trainingHeight; j += tileSize)
            {
                Color[] pixels = trainingImage.GetPixels(i, j, tileSize, tileSize);
                int index = CompareTile(pixels, tileSize);
                //coord.x = origin.x + i * spriteOffsetX;
                //coord.y = origin.y + j * spriteOffsetY;
                scrollingImage.transform.position = new Vector3(origin.x + i * pixelsPerUnit, origin.y + j * pixelsPerUnit, origin.z);
                if (index >= 0)
                {
                    numFound++;
                    temp = textures[index];
                    foundDisplayImage.sprite = Sprite.Create(temp, new Rect(0, 0, temp.width, temp.height), new Vector2(0.5f, 0.5f));
                    for (int tI = i; tI < i + tileSize; tI++)
                    {
                        for (int tJ = j; tJ < j + tileSize; tJ++)
                        {
                            drawingTex.SetPixel(tI, tJ, Color.green);
                        }
                    }
                    drawingTex.Apply();
                    trainingVisual.sprite = Sprite.Create(drawingTex, new Rect(0, 0, trainingImage.width, trainingImage.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                    //yield return new WaitForSeconds(waitTime);
                }
                yield return null;
                /*temp.SetPixels(0, 0, tileSize, tileSize, pixels);
                for (int d = 1; d < 4; d++)
                {
                    pixels = RotateTexture(temp, 90 * d).GetPixels(0, 0, tileSize, tileSize);
                    if (CompareTile(pixels, tileSize) >= 0)
                    {
                        numFound++;
                        break;
                    }
                }*/
            }
        }
        Debug.Log(numFound);
    }

    //Vector3 coord = new Vector3();

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(new Vector3(coord.x, coord.y, 5), new Vector3(coord.x + coord.z, coord.y, 5));
        Gizmos.DrawLine(new Vector3(coord.x, coord.y, 5), new Vector3(coord.x, coord.y + coord.z, 5));
        Gizmos.DrawLine(new Vector3(coord.x, coord.y + coord.z, 5), new Vector3(coord.x + coord.z, coord.y + coord.z, 5));
        Gizmos.DrawLine(new Vector3(coord.x + coord.z, coord.y, 5), new Vector3(coord.x + coord.z, coord.y + coord.z, 5));
    }*/

    #endregion

    #region ShowNeighbors

    [SerializeField] Button tileButtonPrefab;
    [SerializeField] GameObject displayPanel;
    [SerializeField] Transform topNeighborParent;
    [SerializeField] Transform rightNeighborParent;
    [SerializeField] Transform bottomNeighborParent;
    [SerializeField] Transform leftNeighborParent;
    [SerializeField] Image tileDisplayImage;

    void PopulateTileButtons()
    {
        Transform buttonParent = tileButtonPrefab.transform.parent;
        for (int i = 0; i < possibleTiles.Length; i++)
        {
            Button b;
            if (i > 0)
                b = Instantiate(tileButtonPrefab, buttonParent);
            else
                b = tileButtonPrefab;
            b.image.sprite = possibleTiles[i].tile.sprite;
            b.onClick.RemoveAllListeners();
            int temp = i;
            b.onClick.AddListener(() => DisplayTileOnPanel(temp));
        }
    }

    void DisplayTileOnPanel(int index)
    {
        displayPanel.gameObject.SetActive(true);
        tileDisplayImage.sprite = possibleTiles[index].tile.sprite;

        foreach (Transform t in topNeighborParent)
            if (t != topNeighborParent)
                Destroy(t.gameObject);
        foreach (Transform t in rightNeighborParent)
            if (t != rightNeighborParent)
                Destroy(t.gameObject);
        foreach (Transform t in bottomNeighborParent)
            if (t != bottomNeighborParent)
                Destroy(t.gameObject);
        foreach (Transform t in leftNeighborParent)
            if (t != leftNeighborParent)
                Destroy(t.gameObject);

        /*while (topNeighborParent.childCount > 0)
        {
            DestroyImmediate(topNeighborParent.GetChild(0).gameObject);
        }

        while (rightNeighborParent.childCount > 0)
        {
            DestroyImmediate(rightNeighborParent.GetChild(0).gameObject);
        }

        while (bottomNeighborParent.childCount > 0)
        {
            DestroyImmediate(bottomNeighborParent.GetChild(0).gameObject);
        }

        while (leftNeighborParent.childCount > 0)
        {
            DestroyImmediate(leftNeighborParent.GetChild(0).gameObject);
        }*/

        foreach (int t in possibleTiles[index].topNeighbors)
        {
            Image i = Instantiate(tileDisplayImage, topNeighborParent);
            i.sprite = possibleTiles[t].tile.sprite;
        }

        foreach (int t in possibleTiles[index].rightNeighbors)
        {
            Image i = Instantiate(tileDisplayImage, rightNeighborParent);
            i.sprite = possibleTiles[t].tile.sprite;
        }

        foreach (int t in possibleTiles[index].bottomNeighbors)
        {
            Image i = Instantiate(tileDisplayImage, bottomNeighborParent);
            i.sprite = possibleTiles[t].tile.sprite;
        }

        foreach (int t in possibleTiles[index].leftNeighbors)
        {
            Image i = Instantiate(tileDisplayImage, leftNeighborParent);
            i.sprite = possibleTiles[t].tile.sprite;
        }
    }

    #endregion
}
