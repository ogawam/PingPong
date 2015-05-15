using UnityEngine;
using System.Collections;

public class WallController : MonoBehaviour {

	[SerializeField] Material material;
	[SerializeField] Vector2 offset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		material.SetTextureOffset("_MainTex", material.GetTextureOffset("_MainTex") + offset);
	}
}
