using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

	static GameManager instance = null;
	public static GameManager Get() {
		if(instance == null)
			instance = Object.FindObjectOfType<GameManager>();
		return instance;
	}

	void Awake() {
		instance = this;
	}

	float boostGauge = 0;
	float shootGauge = 1;

	[SerializeField] ObjectBase pfEnemy;
	[SerializeField] GameObject enemyBox;
	[SerializeField] TestPlayer player;
	[SerializeField] MyCameraController cameraPivot;
	[SerializeField] HomingLaser prefabLaser;
	[SerializeField] float laserSpeed;

	[SerializeField] Vector3 shootAngles;
	[SerializeField] float shootSec;
	[SerializeField] float fingerRadius;

	[SerializeField] float appUfoHeightMin;
	[SerializeField] float appUfoHeightMax;
	[SerializeField] float appUfoRange;
	[SerializeField] float appSlimeAlturaMin;
	[SerializeField] float appSlimeAlturaMax;
	[SerializeField] int appUfoCount;

	[SerializeField] float timeScaleDefault;
	[SerializeField] float timeScaleBoost;
	[SerializeField] float timeScaleShoot;

	[SerializeField] UISprite spriteButtonShoot;
	[SerializeField] UISprite spriteButtonBoost;
	[SerializeField] UISprite spriteBlur;
	[SerializeField] UISprite pfSpriteTarget;
	[SerializeField] GameObject goLockon;

	[SerializeField] UILabel labelTargetNum;

	enum State {
		Wait,
		Start,
		Rotate,
		Charge,
		Bounce,
		Shoot,
	};

	State state = State.Wait;
	float stateCount = 0;

	Camera camera = null;

	List<ObjectBase> enemies = new List<ObjectBase>();
	public void RemoveEnemy(ObjectBase enemy) {
		enemies.Remove(enemy);
		labelTargetNum.text = "ターゲット あと "+ enemies.Count();
		UpdateBoostGauge(boostGauge + 0.2f);
	}

	public void RemoveBuilding() {
		UpdateBoostGauge(boostGauge + 0.2f);
	}

	public void UpdateBoostGauge(float gauge) {
		boostGauge = gauge;
		spriteButtonBoost.fillAmount = boostGauge;
		spriteButtonBoost.color = boostGauge < 1 ? Color.gray: Color.white;
	}

	public void UpdateShootGauge(float gauge) {
		shootGauge = gauge;
		spriteButtonShoot.fillAmount = shootGauge;
		spriteButtonShoot.color = shootGauge < 1 ? Color.gray: Color.white;
	}

	[SerializeField] int targetSpriteMax;
	List<ObjectBase> lockonObjects = new List<ObjectBase>();
	List<UISprite> spriteTargets = new List<UISprite>();
	List<ObjectBase> insightObjects = new List<ObjectBase>();

	// Use this for initialization
	List<Vector3> hitPoints = new List<Vector3>();
	List<Vector3> hitNormals = new List<Vector3>();
	IEnumerator Start () {
		camera = cameraPivot.GetCameraObject();
		Time.timeScale = timeScaleDefault;

		int count = 0;
		foreach(Transform child in enemyBox.transform) {
			Quaternion rotation = Quaternion.identity;
			float angle = 360 / appUfoCount;
			for(int i = 0; i < appUfoCount; ++i) {
				rotation.eulerAngles = new Vector3(
					Random.Range(appSlimeAlturaMin, appSlimeAlturaMax), (angle * i), 0
				);
				Vector3 vec = rotation * (Vector3.forward);
				Vector3 pos = child.position;
				Vector3 nml = Vector3.up;

				RaycastHit hitInfo;
				if(Physics.Raycast(child.position, vec, out hitInfo)) {
					pos = hitInfo.point;
					nml = hitInfo.normal;
					hitPoints.Add(hitInfo.point);
					hitNormals.Add(hitInfo.normal);
				}

				ObjectBase enemy = Instantiate(pfEnemy, pos, Quaternion.identity) as ObjectBase;
				enemy.transform.up = nml;
				enemy.transform.parent = child;
				enemies.Add(enemy);

				if(i % 3 == 0)
					yield return null;
			}
			yield return null;
		}

		labelTargetNum.text = "ターゲット あと "+ enemies.Count();

		for(int i = 0; i < targetSpriteMax; ++i) {
			UISprite spriteTarget = Instantiate(pfSpriteTarget);
			spriteTarget.transform.parent = goLockon.transform;
			spriteTarget.gameObject.SetActive(false);
			spriteTargets.Add(spriteTarget);
			insightObjects.Add(null);
		}
		state = State.Start;
	}

	Vector3 beganTouchPos = Vector3.zero;
	Vector3 movedTouchPos = Vector3.zero;

	Vector3 beganViewPos = Vector3.zero;
	Vector3 movedViewPos = Vector3.zero;

	// Update is called once per frame
	void Update () {
		for(int i = 0; i < hitPoints.Count; ++i)
			Debug.DrawRay(hitPoints[i],hitNormals[i]);

		UpdateShootGauge(shootGauge + 0.3333f * Time.deltaTime);

		TouchPhase inputPhase = TouchPhase.Canceled;

		if(Input.GetMouseButtonDown(0)) {
			beganViewPos = Scrn2View(beganTouchPos = Input.mousePosition);
			inputPhase = TouchPhase.Began;
		}
		else if(Input.GetMouseButton(0)) {
			movedViewPos = Scrn2View(movedTouchPos = Input.mousePosition);
			inputPhase = TouchPhase.Moved;
		}
		else if(Input.GetMouseButtonUp(0)) {
			movedViewPos = Scrn2View(movedTouchPos = Input.mousePosition);
			inputPhase = TouchPhase.Ended;
		}

		foreach(Touch touch in Input.touches) {
			if(touch.fingerId != 0)
				continue;

			switch(touch.phase) {
			case TouchPhase.Began:
				beganViewPos = Scrn2View(beganTouchPos = touch.position);
				inputPhase = TouchPhase.Began;
				break;
			case TouchPhase.Moved:
			case TouchPhase.Stationary:
				movedViewPos = Scrn2View(movedTouchPos = touch.position);
				inputPhase = TouchPhase.Moved;
				break;
			case TouchPhase.Ended:
			case TouchPhase.Canceled:
				movedViewPos = Scrn2View(movedTouchPos = touch.position);
				inputPhase = TouchPhase.Ended;
				break;
			}
		}

		RaycastHit hitInfo;
		Vector3 movedViewVec = movedViewPos - beganViewPos;
		switch(state) {
		case State.Start:
			if(inputPhase == TouchPhase.Began)
				state = State.Rotate;
			break;

		case State.Rotate:
			switch(inputPhase) {
			case TouchPhase.Moved:
				cameraPivot.SetRotateAngles(new Vector3(-movedViewVec.y * 0.25f, movedViewVec.x * 0.5f, 0));
				break;
			case TouchPhase.Ended:
				cameraPivot.SetRotateAnglesUpdate(new Vector3(-movedViewVec.y * 0.25f, movedViewVec.x * 0.5f, 0));
				state = State.Start;
				break;
			}

			break;

		case State.Charge:
			switch(inputPhase) {
			case TouchPhase.Moved:
				if(movedViewVec.magnitude > 64) {
					cameraPivot.SetModeRotate();
					state = State.Start;
				}
				break;

			case TouchPhase.Ended:
				cameraPivot.SetModeChase();
				Time.timeScale = timeScaleDefault;
				spriteBlur.gameObject.SetActive(false);
				state = State.Bounce;
				player.Jump(cameraPivot.transform.rotation);

				UpdateBoostGauge(0);
				break;
			}
			break;

		case State.Bounce:
			UpdateTarget();
			break;

		case State.Shoot:
			UpdateTarget();
			if(stateCount < shootSec) {
				bool check = false;
				Vector3 touchPos = Vector3.zero;

				switch(inputPhase) {
				case TouchPhase.Began:
					touchPos = beganTouchPos;
					check = true;
					break;
				case TouchPhase.Moved:
					touchPos = movedTouchPos;
					check = true;
					break;
				}

				if(check) {
					if(UICamera.Raycast (touchPos, out hitInfo)) {				
						UISprite sprite = hitInfo.collider.GetComponent<UISprite>();
						if(sprite != null) {
							if(spriteTargets.Contains(sprite)) {
								int index = spriteTargets.IndexOf(sprite);
								ObjectBase entry = insightObjects[index];

								// コリジョンを無効化する
								sprite.GetComponent<Collider>().enabled = false;
								if(!lockonObjects.Contains(entry))
									lockonObjects.Add(entry);
							}
						}
					}
				}
				stateCount += Time.unscaledDeltaTime;
			}
			else {
				Quaternion rotation = camera.transform.rotation;
				rotation.eulerAngles += shootAngles;
				int targetCount = lockonObjects.Count;
				float angle = 360f / targetCount;
				Vector3 position = player.transform.position + player.GetVelocity() / 30;
				foreach(ObjectBase objectBase in lockonObjects) {
					if(objectBase != null) {
						HomingLaser hl = Instantiate(prefabLaser, position, rotation) as HomingLaser;
						hl.Shoot(rotation * Vector3.forward, objectBase.gameObject);
					}
					rotation.eulerAngles += Vector3.forward * angle;
				}
				lockonObjects.Clear();

				cameraPivot.SetModeShooted();
				Time.timeScale = timeScaleDefault;
				spriteBlur.gameObject.SetActive(false);
				UpdateShootGauge(0);
				state = State.Bounce;
			}
			break;
		}
	}

	void Lockon(GameObject go) {
		Debug.Log("lockon "+ go);
	}

	void UpdateTarget() {
		for(int i = 0; i < spriteTargets.Count; ++i)
			spriteTargets[i].gameObject.SetActive(insightObjects[i] != null);

		Vector3 camvec = camera.transform.rotation * Vector3.forward;
		foreach(ObjectBase enemy in enemies) {
			int index = insightObjects.IndexOf(enemy);
			Vector3 vec = camera.transform.position - enemy.transform.position;
			float dot = Vector3.Dot(camvec, vec);
			if(dot < 0 && vec.magnitude < 30) {
				Debug.Log("dot "+ dot);
				if(index < 0) {
					index = 0;
					while(index < insightObjects.Count) {
						if(insightObjects[index] != null)
							index++;
						else break;
					}
				}

				if(index < insightObjects.Count) {
					insightObjects[index] = enemy;
					spriteTargets[index].gameObject.SetActive(true);
					spriteTargets[index].GetComponent<Collider>().enabled = true;

					Vector3 pos = camera.WorldToScreenPoint(enemy.bodyPos);
					spriteTargets[index].transform.localPosition = (pos - new Vector3(Screen.width / 2, Screen.height / 2, 0)) * Common.scrn2View;
					spriteTargets[index].MakePixelPerfect();
					float scale = 0.1f + 0.9f * (1 - (vec.magnitude / 30));
					spriteTargets[index].transform.localScale = 
					spriteTargets[index].transform.localScale * scale;
					if(lockonObjects.Contains(enemy))
						spriteTargets[index].color = Color.red;
					else spriteTargets[index].color = Color.white;
				}
			}
			else {
				if(index >= 0) {
					spriteTargets[index].gameObject.SetActive(false);
					insightObjects[index] = null;
				}
			}
		}
	}

	void ClearTarget() {
		for(int i = 0; i < spriteTargets.Count; ++i)
			spriteTargets[i].gameObject.SetActive(false);
	}

	void OnGUI() { 
		GUILayout.BeginVertical("", "box");

			GUILayout.Label("delta "+ Time.deltaTime);
			GUILayout.Label("count "+ stateCount);

			GUILayout.Label("state "+ state);
			GUILayout.Label("began "+ beganTouchPos);
			GUILayout.Label("moved "+ movedTouchPos);

		GUILayout.EndVertical();
	}	

	void PressShoot() {
		if(state != State.Bounce || shootGauge < 1)
			return;

		if(state != State.Shoot) {
			cameraPivot.SetModeShoot();
			Time.timeScale = timeScaleShoot;
			stateCount = 0;
			state = State.Shoot;
			spriteBlur.gameObject.SetActive(true);
		}
	}

	void PressBoost() {
		if(state == State.Rotate) {
			cameraPivot.SetModeChase();
			Time.timeScale = timeScaleDefault;
			spriteBlur.gameObject.SetActive(false);
			state = State.Bounce;
			player.Jump(cameraPivot.transform.rotation);

			UpdateBoostGauge(0);
			return;
		}
		if(state != State.Bounce || boostGauge < 1)
			return;

		cameraPivot.SetModeRotate();
		Time.timeScale = timeScaleBoost;
		ClearTarget();
		state = State.Start;
		spriteBlur.gameObject.SetActive(true);
	}

	Vector3 Scrn2View(Vector3 pos) {
		pos.x -= Screen.width / 2;
		pos.y -= Screen.height / 2;
		return pos;
	}
}
