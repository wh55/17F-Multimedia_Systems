using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	//for walking or running
	public float walkSpeed = 2.0f;
	public float runSpeed = 6.0f;
	public float gravity = -12.0f;
	public float jumpHeight = 1.0f;
	//for smoothing the moves
	public float turnSmoothTime = 0.2f;
	float turnSmoothVelocity;
	public float speedSmoothTime = 0.1f;
	float speedSmoothVelocity;
	//current speed
	float currentSpeed;
	//for jumping
	float velocityY;
    //click and move
    private Vector3 target;
    private bool isOver = true;
    //animator
	Animator animator;
	//controller
	CharacterController controller;
	//indicator
	public Transform player;
	public Transform indicator;
	//sound effect
	public AudioSource audioWalk;
	public AudioSource audioRun;
	public AudioSource audioJump;


	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
		controller = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {
		//inputDir is the distance to go, in x and z directions
		Vector2 inputDir;
		//hold shift key to run
		bool running = Input.GetKey(KeyCode.LeftShift);
		//when control the character using keyboard, remove the indicator
		if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.RightArrow)
			||Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.DownArrow)){
			indicator.GetComponent<Renderer>().enabled = false;
		}
		//click to move
		if(Input.GetMouseButtonDown(0)){
			//show the indicator
            indicator.GetComponent<Renderer>().enabled = true;
            //print("MouseDown"); //for testing
            //calculate a 3D point according to the mouse click position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo = new RaycastHit();
           //hitInfo contains the 3D position
            if (Physics.Raycast(ray, out hitInfo)){           	        
                if (hitInfo.collider.tag == "plane"){	              	
                    indicator.position = new Vector3(hitInfo.point.x, hitInfo.point.y + 0.3f, hitInfo.point.z);
                    target = hitInfo.point;
                    isOver = false;
                    //print(target); //for testing
                }
            }
        }

        //if the character hasn't reach the target
        if(!isOver){
            Vector2 offset = new Vector2((target.x - player.position.x),(target.z - player.position.z));
            //inputDir is the distance to go, in x and z directions
            inputDir = offset.normalized;

            //if the character reaches the target
            if(Vector3.Distance(target, player.position) < 0.3f){
                isOver = true;
                //remove the indicator
                indicator.GetComponent<Renderer>().enabled = false;
				//clear the inputDir            
                inputDir = Vector2.zero;
            }
        }
        else{
        	//update the inputDir
        	Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			inputDir = input.normalized;

        }

        //call move() with the inputDir
		Move(inputDir, running);

		//space key to control the jump
		if(Input.GetKeyDown(KeyCode.Space)){
			Jump();
		}		

		//animator
		//if running is true, take the running speed, or take the walking speed
		float animationSpeedPercent = ((running) ? currentSpeed/runSpeed : currentSpeed/walkSpeed * 0.5f);
		animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);

		PlayFootsteps();
    }  


  
    //walk and run
	void Move(Vector2 inputDir, bool running){
		if(inputDir != Vector2.zero){
			//turn to the correct direction first
			float targetRotation =  Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg;
			transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
			//print(inputDir.x+";"+inputDir.y); //for testing
		}
		
		//when we don't have an input, target speed will be zero
		float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
		//print("magnitude:"+inputDir.magnitude);

		//for jumping
		velocityY += Time.deltaTime * gravity;
		//for smoothing the speed
		currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

		//in three directions
		Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;
		//move the character
		controller.Move(velocity * Time.deltaTime);

		//speed in x and z directions
		currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;		

		//don't do jump
		if(controller.isGrounded){
		velocityY = 0;
		}	

	}

	//jump
	void Jump(){
		if(controller.isGrounded){
			float jumpVelocity = Mathf.Sqrt(-2.0f * gravity * jumpHeight);
			velocityY = jumpVelocity;
		}
	}

	//sound effects
	private void PlayFootsteps(){
		//when not walking or running
		if(currentSpeed < 0.1f){
			audioWalk.enabled = false;
			audioWalk.loop = false;
			audioRun.enabled = false;
			audioRun.loop = false;
			audioJump.enabled = false;
			audioJump.loop = false;
				//when jump
				if(Input.GetKeyDown(KeyCode.Space)){			
					audioJump.enabled = true;
				}
		}
		//when walk
		if(currentSpeed > 0.1f && currentSpeed < walkSpeed + 0.1f){
			audioWalk.enabled = true;
			audioWalk.loop = true;
			audioRun.enabled = false;
			audioRun.loop = false;
			audioJump.enabled = false;
			audioJump.loop = false;
		}
		//when run
		if(currentSpeed > walkSpeed + 0.1f && currentSpeed < runSpeed + 0.1f){
			audioWalk.enabled = false;
			audioWalk.loop = false;
			audioRun.enabled = true;
			audioRun.loop = true;
			audioJump.enabled = false;
			audioJump.loop = false;
		}

	}

}
