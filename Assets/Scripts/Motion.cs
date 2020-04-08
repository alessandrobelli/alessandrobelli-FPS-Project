using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.Nudi.Fpsproject
{
    // if(!photonView.IsMine) return; check if the player I'm trying to move is my player.

    public class Motion : MonoBehaviourPunCallbacks
    {

        #region Variables
        public float speed;
        public float sprintModifier;
        public Camera normalCam;
        public Transform weaponParent;
        public Transform groundDetector;
        private Vector3 targetWeaponBobPosition;
        public LayerMask ground;
        private Rigidbody rig;
        private float movementCounter;
        private float idleCounter;
        private Vector3 weaponParentOrigin;
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
            weaponParentOrigin = weaponParent.localPosition;
        }

        private void Update()
        {
            if(!photonView.IsMine) return;

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

            // headbob
            if (t_hmove == 0 && t_vmove == 0) 
            { 
                HeadBob(idleCounter, 0.025f, 0.025f); 
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else if(!isSprinting)
            { 
                HeadBob(movementCounter, 0.035f, 0.035f); 
                movementCounter += Time.deltaTime * 3f; 
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else
            {
                HeadBob(movementCounter, 0.15f, 0.075f); 
                movementCounter += Time.deltaTime * 7f; 
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
            }
            
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(!photonView.IsMine) return;
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

        void HeadBob(float z, float x_intensity, float y_intensity)
        {
            targetWeaponBobPosition = weaponParentOrigin + new Vector3(Mathf.Cos(z) * x_intensity, Mathf.Sin(z * 2) * y_intensity, weaponParentOrigin.z);


        }

        #endregion
    }
}
