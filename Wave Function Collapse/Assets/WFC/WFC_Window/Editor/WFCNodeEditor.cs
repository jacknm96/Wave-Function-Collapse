using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(WFCNode))]
public class WFCNodeEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        WFCNode node = target as WFCNode;
        if (node.tex != null) EditorGUI.DrawPreviewTexture(new Rect(0, 0, 50, 50), node.tex);
    }
}
