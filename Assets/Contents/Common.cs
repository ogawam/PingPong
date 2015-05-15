using UnityEngine;
using System.Collections;

public class Common {

	static public float viewWidth = 640f;
	static public float viewHeight = 1136f;
	static public float vanishRange = 128f;
	static public float vanishWidth = viewWidth * 0.5f + vanishRange;
	static public float vanishHeight = viewHeight * 0.5f + vanishRange;
	static public float scrn2View { get { return viewHeight / Screen.height; } }

	static public float r2d = (180f / Mathf.PI);
	static public float Rad2Deg(float radian) { return radian * r2d; }
	static public float ClipDeg(float degree) { 
		while(degree >= 360) degree -= 360;
		while(degree < 0) degree += 360;
		return degree;
	}
	static public float ClipDegMin(float degree) {
		degree = ClipDeg(degree);
		if(degree > 180)
			degree -= 360;
		return degree;	
	} 
	static public Vector3 ClipDegMin(Vector3 degrees) { 
		degrees.x = ClipDegMin(degrees.x);
		degrees.y = ClipDegMin(degrees.y);
		degrees.z = ClipDegMin(degrees.z);
		return degrees;
	}


	const float fps = 1 / 60f;
	static public float frame {
		get { return fps / Time.deltaTime; }
	}
}
