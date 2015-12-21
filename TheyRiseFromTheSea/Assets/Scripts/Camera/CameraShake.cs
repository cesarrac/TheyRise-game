using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {

	public Camera mainCam;

	float shakeAmmt = 0f;

	private float _pixelLockedPPU = 32.0f;

	public PixelPerfectCam pixelCam;

    public static CameraShake Instance { get; protected set; }

	void Awake()
	{
        Instance = this;

		if (mainCam == null)
			mainCam = Camera.main;

//		if (pixelCam == null)
//			GameObject.FindGameObjectWithTag ("Camera").GetComponent<PixelPerfectCam> ();
	}

	public void Shake(float ammt, float length)
	{
		shakeAmmt = ammt;
		InvokeRepeating ("DoShake", 0, 0.01f);
		Invoke ("StopShake", length);
	}


	void DoShake()
	{
		// tell pixel cam we are shaking
//		pixelCam.shaking = true;

		if (shakeAmmt > 0) {

			Vector3 camPos = mainCam.transform.position;

			float offsetX = Random.value * shakeAmmt * 2 - shakeAmmt;
			float offsetY = Random.value * shakeAmmt * 2 - shakeAmmt;

			camPos.x += offsetX;
			camPos.y += offsetY;

			mainCam.transform.position = camPos;
		}
	}

	void StopShake()
	{
		CancelInvoke ("DoShake");
		mainCam.transform.localPosition = Vector3.zero;
		// stop shaking for pixel cam
//		pixelCam.shaking = false;
	}
}
