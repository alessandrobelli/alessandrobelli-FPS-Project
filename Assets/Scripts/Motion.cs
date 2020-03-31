using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Nudi.Fpsproject
{

    public class Motion : MonoBehaviour
    {

        #region Variables
        public float speed;
        public float sprintModifier;
        public Camera normalCam;
        public Transform groundDetector;
        public LayerMask ground;
        private Rigidbody rig;
        private float baseFOV;
        private float sprintFOVModifier = 1.5f;
        public float jumpForce;

        #endregion

        #region Monobehavior callbacks
        private void Start()
        {
            baseFOV = normalCam.fieldOfView;
            Camera.main.enabled = false;
            rig = GetComponent<Rigidbody>();
        }

        private void Update()
        {

            // get input
            float t_vmove = Input.GetAxisRaw("Vertical");
            float t_hmove = Input.GetAxisRaw("Horizontal");

            // control
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool notMovingBackwards = t_vmove > 0;

            // states
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && notMovingBackwards && !isJumping && isGrounded;

            // jump
            if (isJumping)
            {
                rig.AddForce(Vector3.up * jumpForce);
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // get input
            float t_vmove = Input.GetAxisRaw("Vertical");
            float t_hmove = Input.GetAxisRaw("Horizontal");

            // control
            float t_adjustedSpeed = speed;
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool notMovingBackwards = t_vmove > 0;

            // states
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && notMovingBackwards && !isJumping && isGrounded;

            // movement
            Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
            t_direction.Normalize();
            


            // FOV
            if (isSprinting)
            {
                t_adjustedSpeed *= sprintModifier;
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
            }
            else
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);

            }
            
            Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * t_adjustedSpeed * Time.fixedDeltaTime;
            t_targetVelocity.y = rig.velocity.y;
            rig.velocity = t_targetVelocity;


        }
        #endregion

        #region Private methods

        #endregion
    }
}
