using UnityEngine;
using System.Collections;

public class TestPlayer : MonoBehaviour {

	[SerializeField] float speed;
	[SerializeField] GameObject pfbGareki;

	// Use this for initialization
	Rigidbody rigidbody;
	void Start () {
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.isKinematic = true;
	}
	
	// Update is called once per frame
	void Update () {
		if(rigidbody.isKinematic) {
			Vector3 pos = transform.localPosition;
			pos.y = 2f + 0.1f * Mathf.Sin(Mathf.PI * Time.time);
			transform.localPosition = pos;
		}
	}

	void OnCollisionEnter(Collision collision) {
        foreach (ContactPoint contact in collision.contacts) {

        	if(contact.otherCollider.tag != "Undestruction") {
        		Bounds bounds = contact.otherCollider.bounds;
        		Vector3 position = bounds.center;
        		position.y -= bounds.extents.y;
        		Quaternion rotation = Quaternion.identity;
        		rotation.eulerAngles = new Vector3(90, 0, 0);

//        		GameObject gareki = Instantiate(pfbGareki, position, rotation) as GameObject;
//      		gareki.transform.localScale = Vector3.one * 0.1f;
				ObjectBase objectBase = contact.otherCollider.GetComponent<ObjectBase>();
				if(objectBase != null) {
					objectBase.Destruction();
	        	}
	        	GameManager.Get().RemoveBuilding();
        		Destroy(contact.otherCollider.gameObject);
        	}
        }
	}

	public void Jump() {
		Jump(Quaternion.LookRotation(rigidbody.velocity));
	}

	public void Jump(Quaternion rotation) {
		rigidbody.isKinematic = false;
		rigidbody.velocity = rotation * Vector3.forward * speed;
	}

	public void Stop() {
		rigidbody.isKinematic = true;
	}

	public Vector3 GetVelocity() {
		return rigidbody.velocity;	
	}
}
