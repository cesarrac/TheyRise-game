using UnityEngine;
/**
 * A camera to help with Orthagonal mode when you need it to lock to pixels.  Desiged to be used on android and retina devices.
 */
public class PixelPerfectCam : MonoBehaviour {
	/**
	 * The target size of the view port.
	 */
	public Vector2 targetViewportSizeInPixels = new Vector2(1424.0f, 890.0f);
	/**
	 * Snap movement of the camera to pixels.
	 */
	public bool lockToPixels = true;
	/**
	 * The number of target pixels in every Unity unit.
	 */
	public float pixelsPerUnit = 32.0f;
	/**
	 * A game object that the camera will follow the x and y position of.
	 */
	public GameObject followTarget;
	Vector3 target;
	public float dampTime = 0.15f;
	private Vector3 velocity = Vector3.zero;
	
	private Camera _camera;
	private Transform _cameraHolder;

	private int _currentScreenWidth = 0;
	private int _currentScreenHeight = 0;
	
	private float _pixelLockedPPU = 32.0f;
	private Vector2 _winSize;

	float vertExtent, horzExtent, leftBound, rightBound, bottomBound, topBound;
	public float mapX, mapY;

	// Map boundaries
	float left = 20, bottom = 10, right = 40, top = 36;

	public bool lockToMap = false;

	// Camera zoom
	private bool _zoomedIn;
	private float _startOrthoSize, _zoomOrthoSize = 8.15f;
	public float curOrthoSize;

    bool playerReady = false;
	
	protected void Start(){

		_camera = this.GetComponentInChildren<Camera>();

    

        if (!_camera){
			Debug.LogWarning("No camera for pixel perfect cam to use");
		}else{
			_camera.orthographic = true;

            // If camera has access to player, resize
       //     if (followTarget)
			    //ResizeCamToTargetSize();

         

        }

		if (!_cameraHolder)
        {
			_cameraHolder = this.transform;
		}

        // Move the camera to the center of the map and zoom out
        _cameraHolder.transform.position = new Vector3(ResourceGrid.Grid.transform.position.x, ResourceGrid.Grid.transform.position.y, -10f);
        _camera.orthographicSize = 37f;
    }
	
	public void ResizeCamToTargetSize(){
		if(_currentScreenWidth != Screen.width || _currentScreenHeight != Screen.height){
			// check our target size here to see how much we want to scale this camera
			float percentageX = Screen.width/targetViewportSizeInPixels.x;
			float percentageY = Screen.height/targetViewportSizeInPixels.y;
			float targetSize = 0.0f;
			if(percentageX > percentageY){
				targetSize = percentageY;
			}else{
				targetSize = percentageX;
			}
			int floored = Mathf.FloorToInt(targetSize);
			if(floored < 1){
				floored = 1;
			}
			// now we have our percentage let's make the viewport scale to that
			float camSize = ((Screen.height/2)/floored)/pixelsPerUnit;
			_camera.orthographicSize = camSize - (camSize / 4);
			_pixelLockedPPU = floored * pixelsPerUnit;

			// store size for zoom
			_startOrthoSize = _camera.orthographicSize;
			curOrthoSize = _startOrthoSize;


			vertExtent = camSize;  
			
			horzExtent = camSize * Screen.width / Screen.height;
			
			leftBound = (horzExtent - (mapX / 2.0f)) + horzExtent / 2;
			
			rightBound = (horzExtent + (mapX / 2.0f)) - horzExtent /2;
			
			topBound = (vertExtent + (mapY / 2.0f)) + vertExtent / 2;
			
			bottomBound = (vertExtent - (mapY / 2.0f)) + vertExtent;
		}
		_winSize = new Vector2(Screen.width, Screen.height);
	}
	
	public void Update(){
        
        if (!playerReady && followTarget)
        {
            playerReady = true;
            ResizeCamToTargetSize();
        }

		if(_winSize.x != Screen.width || _winSize.y != Screen.height){
            if (playerReady)
			    ResizeCamToTargetSize();
		}
        
		if (_cameraHolder && followTarget) {
			Vector3 newPosition = new Vector3 (followTarget.transform.position.x, followTarget.transform.position.y, 0.0F);
			float nextX = Mathf.Round (_pixelLockedPPU * newPosition.x);
			float nextY = Mathf.Round (_pixelLockedPPU * newPosition.y);

		
			if (lockToMap){
				// Check that if next move position is one of the map boundaries
				if (newPosition.x < left || newPosition.x > right || newPosition.y >top  || newPosition.y < bottom) {
					//Dont move
				}else{
					
					Vector3 pixelPerfectPosition = new Vector3(nextX / _pixelLockedPPU, nextY / _pixelLockedPPU, _cameraHolder.transform.position.z);
					Vector3 point = _camera.WorldToViewportPoint(pixelPerfectPosition);
					Vector3 delta = pixelPerfectPosition - _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); 
					Vector3 destination = _cameraHolder.position + delta;
					transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
				}
			} else {
//				_cameraHolder.transform.position = new Vector3 (nextX / _pixelLockedPPU, nextY / _pixelLockedPPU, _cameraHolder.transform.position.z);

				Vector3 pixelPerfectPosition = new Vector3(nextX / _pixelLockedPPU, nextY / _pixelLockedPPU, _cameraHolder.transform.position.z);
				Vector3 point = _camera.WorldToViewportPoint(pixelPerfectPosition);
				Vector3 delta = pixelPerfectPosition - _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); 
				Vector3 destination = _cameraHolder.position + delta;
				transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);

			}
		}

//		float inputX = Input.GetAxis ("Horizontal");
//		float inputY = Input.GetAxis ("Vertical");
//		if (_camera) {
//			if (inputX > 0 || inputX < 0 || inputY > 0 || inputY < 0){
//			
//				Vector3 move = new Vector3 (_camera.transform.position.x + inputX, _camera.transform.position.y + inputY, 0);
//				float nextX = Mathf.Round(_pixelLockedPPU * move.x);
//				float nextY = Mathf.Round(_pixelLockedPPU * move.y);
//
//				target = new Vector3 (Mathf.Clamp (nextX / pixelsPerUnit, (leftBound + horzExtent / 2), (rightBound - horzExtent / 2)),
//				                      Mathf.Clamp(nextY / pixelsPerUnit, vertExtent /2, topBound - vertExtent /2 + 10f), 
//				                      -10f);
//
////				target = new Vector3(nextX/_pixelLockedPPU, nextY/_pixelLockedPPU, 0);
//
//			}else {
//				target = Vector3.zero;
//			}
//		}
//
//		if (_camera && target != Vector3.zero) {
//			Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target);
//			Vector3 delta = target - _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); 
//			Vector3 destination = _camera.transform.position + delta;
////			_camera.transform.position = Vector3.SmoothDamp(_camera.transform.position, destination, ref velocity, dampTime);
//			_camera.transform.position = Vector3.MoveTowards(_camera.transform.position, destination, 6f * Time.deltaTime);
//
//		}
	}
}