using UnityEngine;
using System.Collections;

public class ObjectBase : MonoBehaviour {

	[SerializeField] Vector3 bodyOffset;
	[SerializeField] GameObject destroyEffect;

	public Vector3 bodyPos {
		get { return transform.position + transform.rotation * bodyOffset; }
	}

	public void Destruction() {
		Instantiate(destroyEffect, transform.position, transform.rotation);
	}

	void OnDestroy() {
		if(GameManager.Get() != null)
			GameManager.Get().RemoveEnemy(this);
	}
}
