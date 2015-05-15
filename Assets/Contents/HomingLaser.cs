using UnityEngine;
using System.Collections;

public class HomingLaser : MonoBehaviour {

	[SerializeField] float speedMin;
	[SerializeField] float speedMax;
	[SerializeField] float angleMax;

	[SerializeField] Vector3 speed;
	[SerializeField] GameObject target = null;

	float dot;

	public void Shoot(Vector3 speed_, GameObject target_ = null) {
		speed = speed_;
		transform.rotation = Quaternion.FromToRotation(Vector3.forward, speed_);
		target = target_;

		dot = Vector3.Dot(speed, target.transform.position - transform.position);
	}

	// Use this for initialization
	ParticleSystem particle = null;
	void Start () {
		particle = GetComponentInChildren<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {

		if(target != null && particle != null) {
			Vector3 toTarget = target.transform.position - transform.position;
			Quaternion rotation = Quaternion.FromToRotation(speed, toTarget);

			Vector3 vec = Common.ClipDegMin(rotation.eulerAngles);
			float diff = (Mathf.Abs(vec.x) / 180f) * (Mathf.Abs(vec.y) / 180f);
			float accel = speedMax + (speedMin - speedMax) * diff;
			vec.x = Mathf.Clamp(vec.x, -angleMax, angleMax);
			vec.y = Mathf.Clamp(vec.y, -angleMax, angleMax);

			rotation.eulerAngles = vec;
			speed = rotation * speed;
			transform.localEulerAngles = new Vector3(
				Mathf.Atan2(Mathf.Sqrt(speed.x*speed.x+speed.z*speed.z), speed.y), 
				Common.Rad2Deg(Mathf.Atan2(speed.x,speed.z)), 0
			);

			Vector3 add = (speed.normalized * accel) * Time.deltaTime;
			transform.position =
			transform.position + add;

			if(toTarget.magnitude < 1) {
				float preDot = dot;
				dot = Vector3.Dot(speed, toTarget);
				if(preDot < 0 && dot > 0) {
					ObjectBase objectBase = target.GetComponent<ObjectBase>();
					if(objectBase != null) {
						objectBase.Destruction();
						Destroy(objectBase.gameObject);
						Destroy(particle);
					}
				}
			}
		}
	}
}
