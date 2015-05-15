using UnityEngine;
using System.Collections;

public class MyCameraController : MonoBehaviour {

	[SerializeField] float offsetCameraRotate;
	[SerializeField] float offsetCameraCharge;
	[SerializeField] float offsetCameraChace;
	[SerializeField] float offsetCameraShoot;
	[SerializeField] float offsetCameraShooted;

	[SerializeField] GameObject camera;
	[SerializeField] TestPlayer player;

	[SerializeField] float xLookAngleMin;
	[SerializeField] float xLookAngleMax;
	[SerializeField] float yLookAngleMin;
	[SerializeField] float yLookAngleMax;

	[SerializeField] float fall2Offset;
	[SerializeField] float offsetMax;

	public Camera GetCameraObject() {
		return camera.GetComponent<Camera>();
	}

	enum Mode {
		Rotate,
		Charge,
		Chase,
		Shoot,
		Shooted,
	};

	Mode mode = Mode.Rotate;

	Vector3 beganAngles = Vector3.zero;

	// Use this for initialization
	void Start () {
		beganAngles = transform.localEulerAngles;
		Satellite(1);
	}

	public void SetRotateAngles(Vector3 angles) {
		angles = beganAngles + angles;
		angles.x = Mathf.Clamp(angles.x, -60, 60);
		transform.localEulerAngles = angles;
	}
	
	public void SetRotateAnglesUpdate(Vector3 angles) {
		beganAngles += angles;
		beganAngles.x = Mathf.Clamp(beganAngles.x, -60, 60);
		transform.localEulerAngles = beganAngles;
	}

	public void SetModeRotate() {
		beganAngles = transform.localEulerAngles;
		mode = Mode.Rotate;
	}
	
	public void SetModeCharge() {
		mode = Mode.Charge;
	}
	
	public void SetModeChase() {
		mode = Mode.Chase;
	}

	public void SetModeShoot() {
		mode = Mode.Shoot;
	}

	public void SetModeShooted() {
		mode = Mode.Shooted;
		modeSec = 0;
	}

	// Update is called once per frame
	float cameraOffsetZ = 0;
	float modeSec = 0;
	float zoomRate = 0;

	void Update () {
		zoomRate = 0.1f;
		cameraOffsetZ = Mathf.Abs(camera.transform.localPosition.z);

		switch(mode) {
		case Mode.Rotate:			
			cameraOffsetZ = offsetCameraRotate;
			Satellite(0.5f);
			GazePlayer();
			Zoom();
			break;

		case Mode.Charge:
			cameraOffsetZ = offsetCameraCharge;
			Satellite(0.5f);
			GazePlayer();
			Zoom();
			break;
		
		case Mode.Chase:
			cameraOffsetZ = offsetCameraChace;
			Chase(0.01f);
			GazePlayer();
			break;

		case Mode.Shoot:
			cameraOffsetZ = offsetCameraShoot;
			Chase(0.25f);
			GazePlayer();
			break;

		case Mode.Shooted:
			cameraOffsetZ = offsetCameraShooted;
			Chase(0.1f);
			GazePlayer();
			if(modeSec > 1)
				SetModeChase();
			break;
		}

		modeSec += Time.deltaTime;

		Vector3 vec = (Vector3.back * cameraOffsetZ) - camera.transform.localPosition;
		camera.transform.localPosition += vec * zoomRate;
	}

	void Satellite(float rate) {
		Vector3 posvec = player.transform.position - transform.position;

		transform.position = 
		transform.position + posvec * rate;
	}

	float chaseSpeed = 0;
	void Chase(float chaseRate) {
		Vector3 posvec = player.transform.position - transform.position;
		Vector3 eyevec = player.transform.position - camera.transform.position;
		Vector3 eyerot = Quaternion.LookRotation(eyevec).eulerAngles;

		chaseSpeed = Mathf.Min(posvec.magnitude, chaseSpeed + (posvec.magnitude - chaseSpeed) * chaseRate);
		Vector3 pos = transform.position + posvec.normalized * chaseSpeed;
		transform.position = pos;

		eyevec = player.transform.position - camera.transform.position;
		eyerot = Quaternion.LookRotation(eyevec).eulerAngles;

		Vector3 rot = new Vector3(22.5f, Common.Rad2Deg(Mathf.Atan2(eyevec.x,eyevec.z)), 0);
		rot -= transform.localEulerAngles;
		rot.x = Common.ClipDegMin(rot.x);
		rot.y = Common.ClipDegMin(rot.y);
		transform.localEulerAngles += rot; 

	}

	void GazePlayer() {

		Vector3 eyevec = player.transform.position - camera.transform.position;
		Vector3 eyerot = Quaternion.LookRotation(eyevec).eulerAngles;
		Vector3 velocity = player.GetVelocity();
		eyerot.x += Mathf.Clamp((velocity.y / fall2Offset) * -offsetMax, -offsetMax, offsetMax);

		eyerot = Common.ClipDegMin(eyerot - camera.transform.eulerAngles);
		float rate = (Mathf.Abs(eyerot.x) - xLookAngleMin) / (xLookAngleMax - xLookAngleMin);
		eyerot.x = eyerot.x * Mathf.Clamp(rate, 0, 1);

		rate = (Mathf.Abs(eyerot.y) - yLookAngleMin) / (yLookAngleMax - yLookAngleMin);
		eyerot.y = eyerot.y * Mathf.Clamp(rate, 0, 1);

		camera.transform.eulerAngles = 
		camera.transform.eulerAngles + eyerot;		
	}

	void Zoom() {
		Vector3 eyevec = player.transform.position - camera.transform.position;

		RaycastHit hitInfo;
		Ray ray = new Ray(player.transform.position, -eyevec);
		Debug.DrawRay(ray.origin, ray.direction);
		if(Physics.Raycast(ray, out hitInfo, cameraOffsetZ)) {
//			cameraOffsetZ = hitInfo.distance - 0.1f;
//			zoomRate = 1;
		}		
	}
}
