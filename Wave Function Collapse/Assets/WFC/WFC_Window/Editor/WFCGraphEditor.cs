using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using XNode;
using XNodeEditor;

[CustomNodeGraphEditor(typeof(WFCGraph))]
public class WFCGraphEditor : NodeGraphEditor
{
    [SerializeField] SerializedProperty spriteMap;
    WFCGraph graph;

    public override void OnOpen()
    {
        base.OnOpen();
        spriteMap = serializedObject.FindProperty(nameof(WFCGraph.spriteMap));
    }
    public override void OnGUI()
    {
        base.OnGUI();
        graph = target as WFCGraph;
        Rect rect = new Rect(5, 5, 200, 50);
        EditorGUI.DrawRect(rect, Color.gray);
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(rect, spriteMap);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(graph, "Change Spritemap");
            graph.spriteMap = (Texture2D)spriteMap.objectReferenceValue;
            ChangeSpritesheet();
            EditorUtility.SetDirty(graph);
        }
    }

    public void ChangeSpritesheet()
    {
        if (graph.nodes.Count > 0)
        {
            foreach (Node n in graph.nodes)
                AssetDatabase.RemoveObjectFromAsset(n);
            graph.Clear();
        }
        Sprite[] sprites = Resources.LoadAll<Sprite>(graph.spriteMap.name);
        int sqrt = (int)Mathf.Sqrt(sprites.Length) + 1;
        string path = AssetDatabase.GetAssetPath(graph);
        path = path.Replace(graph.name + ".asset", "");
        Debug.Log(path);
        for (int i = 0; i < sprites.Length; i++)
        {
            /*WFCNode node = graph.AddNode<WFCNode>();
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(graph))) AssetDatabase.AddObjectToAsset(node, graph);*/
            WFCNode node = CreateNode(typeof(WFCNode), new Vector2(200 * (i % sqrt), 200 * (i / sqrt))) as WFCNode;
            node.name = sprites[i].name;
            Texture2D croppedTexture = new Texture2D((int)sprites[i].rect.width, (int)sprites[i].rect.height);
            Color[] pixels = sprites[i].texture.GetPixels((int)sprites[i].textureRect.x,
                                                    (int)sprites[i].textureRect.y,
                                                    (int)sprites[i].textureRect.width,
                                                    (int)sprites[i].textureRect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            AssetDatabase.CreateAsset(croppedTexture, path + node.name + ".png");
            node.tex = croppedTexture;
            node.sprite = sprites[i];
            //node.tile = (Tile)ScriptableObject.CreateInstance(typeof(Tile));
            node.tile = Tile.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(node.tile, path + node.name + ".asset");
            node.tile.name = node.name + "_tile";
            node.tile.sprite = node.sprite;
            //node.position = new Vector2(200 * (i % sqrt), 200 * (i / sqrt));
        }
    }
}
