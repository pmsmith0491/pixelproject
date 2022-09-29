using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    //[SerializeField] private Transform groundCheckTransform = null;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float rotationSpeed;
    private float speed;

    //Animator 
    private Animator animator;

    private Rigidbody rigidBodyComponent;
    private float horizontalInput;
    private float verticalInput;
    private bool isRunning;

    // Start is called before the first frame update
    void Start()
    {
        speed = 0.2f;
        isRunning = false;
        animator = GetComponent<Animator>();
        rigidBodyComponent = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
        } 
        else
        {
            isRunning = false;
        }

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }


    // PRE: isRunning is well defined.
    // POST: speed = 2.0f iff isRunning and rigidBodyComponent.velocity.magnitude > 0.001. 0.2f otherwise. 
    private void RunningController()
    {

        if(rigidBodyComponent.velocity.magnitude < 0.001)
        {
            isRunning = false;
        }

        if(isRunning)
        {
            speed = 2.0f;
        } 
        else
        {
            speed = 0.2f;
        }
    }

    // FixedUpdate is called once every physic update
    private void FixedUpdate()
    {
        // Running controller
        RunningController();

        // Change velocity based on user input
        rigidBodyComponent.velocity = new Vector3(horizontalInput, 0, verticalInput);
        rigidBodyComponent.velocity.Normalize();
        transform.Translate((rigidBodyComponent.velocity) * speed * Time.deltaTime, Space.World);
      
        // player model rotation
        if(rigidBodyComponent.velocity != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(rigidBodyComponent.velocity, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        // Change parameters in animator
        animator.SetFloat("Speed", rigidBodyComponent.velocity.magnitude);
        animator.SetBool("IsRunning", isRunning);
    }    

}
