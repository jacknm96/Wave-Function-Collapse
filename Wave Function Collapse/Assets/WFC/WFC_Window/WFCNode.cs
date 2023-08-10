using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using XNode;

public class WFCNode : Node {

	public Texture2D tex;
	public Tile tile;
	public Sprite sprite;
	public bool rotatable;
	public int topSocket = -1;
	public int rightSocket = -1;
	public int bottomSocket = -1;
	public int leftSocket = -1;
	
	// Use this for initialization
	protected override void Init() {
		base.Init();
		
	}

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port) {
		return null; // Replace this
	}
}