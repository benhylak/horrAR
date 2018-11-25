using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System.Threading;
using System.Threading.Tasks;

public class ChuckyBehavior : MonoBehaviour {

	public GameObject head;
	public Camera camera;
	
	public bool LookAt = false;
	public bool outOfRange = false;

	public Quaternion lastRotation = Quaternion.identity;

	private ChuckyBehaviorState currentState = ChuckyBehaviorState.IDLE;
	private Animator _anim;

	private float timeOfLastTrigger = 0f;

	private bool justEntered = true;

	public enum ChuckyBehaviorState
	{
		IDLE = 0,
		IDLE_LOOKING = 1,
		IDLE_DID_WAVE = 2,
		RUNNING = 3,
		FIGHTING = 4,
		DEAD = 5
	}

	// Use this for initialization
	void Start () {
		_anim = GetComponent<Animator>();

		MLInput.OnControllerButtonDown += (byteVal, button) =>
		{
			if(button == MLInputControllerButton.Bumper)
			{
				if(currentState == ChuckyBehaviorState.IDLE_DID_WAVE)
				{
					currentState = ChuckyBehaviorState.RUNNING;
					_anim.SetBool("running", true);
				}
				else
				{
					currentState = ChuckyBehaviorState.IDLE;
					LookAt = false;
					_anim.SetBool("running", false);
					_anim.SetTrigger("default");
				}
			}
		};
	}

	private void TryLookAtCamera()
	{
		var dir = camera.transform.position - head.transform.position;
		var targetRot = Quaternion.LookRotation(dir) * Quaternion.Euler(15f, 0, 0);
		//targetRot *= Quaternion.Euler(0f, -90f, -90f); //flip for orientation

		//constrain
		var identity = (head.transform.parent != null) ? head.transform.parent.rotation : Quaternion.identity;
		var a = Quaternion.Angle(identity, targetRot);

		//deadband of 60<->90, prevents flickering
		if(a > 90f) outOfRange = true;
		else if(a < 60f) outOfRange = false;
		
		outOfRange = false;

		if (!outOfRange)
		{
			if(lastRotation == Quaternion.identity) //initialize lastRotation if a perfect identity quaternion
			{
				lastRotation = head.transform.rotation;
			}

			lastRotation = Quaternion.Slerp(lastRotation, targetRot, 0.3f);
			head.transform.rotation = lastRotation;
		}
	}

	public void Hit()
	{
		if(currentState == ChuckyBehaviorState.RUNNING)
		{
			currentState = ChuckyBehaviorState.FIGHTING;
			_anim.SetBool("running", false);
			_anim.SetTrigger("agony");
			LookAt = false;
		}
	}

	void LateUpdate()
	{
		if(LookAt) TryLookAtCamera();
	
		if(!LookAt || outOfRange)
		{
			lastRotation = Quaternion.Slerp(lastRotation, head.transform.rotation, 0.2f);
		}

		head.transform.rotation = lastRotation;
	}
	
	// Update is called once per frame
	async void Update () {
		
		var directLookVec = head.transform.position - camera.transform.position;

		if(currentState == ChuckyBehaviorState.RUNNING)
		{
			this.transform.position += this.transform.forward * 0.4f *  Time.deltaTime;
		}
		else if(Vector3.Dot(directLookVec, camera.transform.forward) > 0.8f && justEntered && Time.time - timeOfLastTrigger > 2.5f)
		{
			switch(currentState)
			{
				case ChuckyBehaviorState.IDLE:
					await Task.Delay(1500);
					LookAt = true;
					currentState = ChuckyBehaviorState.IDLE_LOOKING;
					break;

				case ChuckyBehaviorState.IDLE_LOOKING:
					await Task.Delay(750);

					_anim.SetTrigger("wave");
					currentState = ChuckyBehaviorState.IDLE_DID_WAVE;
					break;
			}

			timeOfLastTrigger = Time.time;
			justEntered = false;
		}
		else if(Vector3.Dot(directLookVec, camera.transform.forward) < 0.73f) justEntered = true;
	}
}
