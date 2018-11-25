using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class ExperienceManager : MonoBehaviour {
	GameObject _objectBeingPlaced = null;
	public Transform _controller;

	LineRenderer _lineRenderer; 

	public GameObject controllerDisplay;

	bool canMoveItems = true;

	public Camera _camera;

	float _tractorBeamDist = 0f;

	[Tooltip("Put Chucky and TV in here")]
	public LayerMask moveLayer;

	// Use this for initialization
	void Start () {
		_lineRenderer = GetComponent<LineRenderer>();

		MLInput.OnControllerButtonDown += (byteData, button) =>
		{
			if(button == MLInputControllerButton.HomeTap)
			{
				canMoveItems = !canMoveItems;
				controllerDisplay.SetActive(canMoveItems);
			}
		};
	}
	
	// Update is called once per frame
	void Update () {

		bool triggerDown = MLInput.GetController(0).TriggerValue > MLInput.TriggerDownThreshold;
		
		if(canMoveItems && triggerDown && _objectBeingPlaced == null)
		{
			// Virtual Raycast
			Ray ray = new Ray(_controller.position, _controller.forward);
			RaycastHit result;

			if (Physics.Raycast(ray, out result, 100, moveLayer))
			{
				_objectBeingPlaced = result.collider.gameObject;
				_tractorBeamDist = Vector3.Distance(_controller.position, _objectBeingPlaced.transform.position);
				_lineRenderer.positionCount = 2;
			}
			//try raycast
			//if you hit an item
		}
		else if(canMoveItems && triggerDown)
		{
			Vector3 tractorBeamEndPos = _controller.position + _controller.forward * _tractorBeamDist;
			
			_objectBeingPlaced.transform.position = tractorBeamEndPos;
			_objectBeingPlaced.transform.forward = -_controller.forward;

			_lineRenderer.SetPosition(0, _objectBeingPlaced.transform.position);
			_lineRenderer.SetPosition(1, _controller.transform.position);
		}
		else if(_objectBeingPlaced != null) 
		{
			_lineRenderer.positionCount = 0;
			_objectBeingPlaced = null;
		}
	}

	public void OnRaycastHit(MLWorldRays.MLWorldRaycastResultState state, RaycastHit result, float confidence)
	{
		// if(!_frozen)
		// {
		// 	if (state != MLWorldRays.MLWorldRaycastResultState.RequestFailed && state != MLWorldRays.MLWorldRaycastResultState.NoCollision)
		// 	{
		// 		// Update the cursor position and normal.
		// 		transform.position = result.point;
		// 		transform.LookAt(result.normal + result.point);
		// 		transform.localScale = Vector3.one;

		// 		// Set the color to yellow if the hit is unobserved.
		// 		_render.material.color = (state == MLWorldRays.MLWorldRaycastResultState.HitObserved)? _color : Color.yellow;

		// 		if (_scaleWhenClose)
		// 		{
		// 			// Check the hit distance.
		// 			if (result.distance < 1.0f)
		// 			{
		// 				// Apply a downward scale to the cursor.
		// 				transform.localScale = new Vector3(result.distance, result.distance, result.distance);
		// 			}
		// 		}

		// 		_hit = true;
		// 	}
		// 	else
		// 	{
		// 		// Update the cursor position and normal.
		// 		transform.position = (_raycast.RayOrigin + (_raycast.RayDirection * _defaultDistance));
		// 		transform.LookAt(_raycast.RayOrigin);
		// 		transform.localScale = Vector3.one;

		// 		_render.material.color = Color.red;

		// 		_hit = false;
		// 	}
		// }
	}
}
