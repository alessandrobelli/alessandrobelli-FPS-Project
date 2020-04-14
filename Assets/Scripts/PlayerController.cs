using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

namespace Com.Nudi.Fpsproject
{
    // if(!photonView.IsMine) return; check if the player I'm trying to move is my player.

    public class PlayerController : MonoBehaviourPunCallbacks
    {

        #region Variables
        public float speed;
        public float slideModifier;
        public float lengthOfSlide;
        public float sprintModifier;
        public float jumpForce;
        public int max_health;
        public Camera normalCam;
        public GameObject cameraParent;
        public Transform weaponParent;
        public Transform groundDetector;
        public LayerMask ground;
 

        private Rigidbody rig;
        private float movementCounter;
        private float idleCounter;
        private Vector3 weaponParentOrigin;
        private float baseFOV;
        private float sprintFOVModifier = 1.5f;
        private int current_health;
        private Manager manager;
        private Weapon weapon;
        private Transform ui_healthbar;
        private UnityEngine.UI.Text ui_ammo;
        private Vector3 targetWeaponBobPosition;
        private bool sliding;
        private float slide_time;
        private Vector3 slide_dir;
        private Vector3 normalCameraOrigin;
        private Vector3 weaponParentCurrentPosition;

        #endregion

        #region Monobehavior callbacks
        private void Start()
        {
            current_health = max_health;
            manager = GameObject.Find("Manager").GetComponent<Manager>();
            weapon = GetComponent<Weapon>();

            cameraParent.SetActive(photonView.IsMine);

            if (!photonView.IsMine) gameObject.layer = 11;


            baseFOV = normalCam.fieldOfView;
            normalCameraOrigin = normalCam.transform.localPosition;

            if (Camera.main) Camera.main.enabled = false;
            rig = GetComponent<Rigidbody>();
            weaponParentOrigin = weaponParent.localPosition;
            weaponParentCurrentPosition = weaponParentOrigin;

            if (photonView.IsMine)
            {

                ui_healthbar = GameObject.Find("HUD/Health/Bar").transform;
                ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
                RefreshHealthBar();
                weapon.RefreshAmmo(ui_ammo);
            }
        }

        private void Update()
        {
            if (!photonView.IsMine) return;

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

            if (Input.GetKeyDown(KeyCode.U)) TakeDamage(100);

            // jump
            if (isJumping)
            {
                rig.AddForce(Vector3.up * jumpForce);
            }

            // headbob
            if (!sliding) 
            {
                if (t_hmove == 0 && t_vmove == 0)
                {
                    HeadBob(idleCounter, 0.025f, 0.025f);
                    idleCounter += Time.deltaTime;
                    weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
                }
                else if (!isSprinting)
                {
                    HeadBob(movementCounter, 0.035f, 0.035f);
                    movementCounter += Time.deltaTime * 3f;
                    weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
                }
                else
                {
                    HeadBob(movementCounter, 0.12f, 0.060f);
                    movementCounter += Time.deltaTime * 7f;
                    weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
                }
            }

            // UI Refreshes
            RefreshHealthBar();
            weapon.RefreshAmmo(ui_ammo);

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!photonView.IsMine) return;
            // get input
            float t_vmove = Input.GetAxisRaw("Vertical");
            float t_hmove = Input.GetAxisRaw("Horizontal");

            // control
            float t_adjustedSpeed = speed;
            bool jumpButtonPressed = Input.GetKeyDown(KeyCode.Space);
            bool sprintButtonPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool notMovingBackwards = t_vmove > 0;
            bool slideButtonPressed = Input.GetKey(KeyCode.C);

            // #states
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jumpButtonPressed && isGrounded;
            bool isSprinting = sprintButtonPressed && notMovingBackwards && !isJumping && isGrounded;
            bool isSliding = isSprinting && slideButtonPressed && !sliding;


            // movement
            Vector3 t_direction = Vector3.zero;
            if (!sliding)
            {
                t_direction = new Vector3(t_hmove, 0, t_vmove);
                t_direction.Normalize();

                t_adjustedSpeed *= sprintModifier;
                t_direction = transform.TransformDirection(t_direction);

            }
            else
            {
                 t_direction = slide_dir;
                t_adjustedSpeed *= slideModifier;

                slide_time -= Time.fixedDeltaTime;
                if (slide_time <= 0)
                {
                    sliding = false;
                    weaponParentCurrentPosition += Vector3.up * 0.5f;
                }
            }



            Vector3 t_targetVelocity = t_direction * t_adjustedSpeed * Time.fixedDeltaTime;
            t_targetVelocity.y = rig.velocity.y;
            rig.velocity = t_targetVelocity;

            // sliding
            if (isSliding)
            {
                sliding = true;
                slide_dir = t_direction;
                slide_time = lengthOfSlide;
                weaponParentCurrentPosition += Vector3.down * 0.5f;
                // adjust camera
            }

            // Camera Stuff
            if (sliding)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier * 1.25f, Time.deltaTime * 8f);
                normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, normalCameraOrigin + Vector3.down * 0.5f, Time.deltaTime * 6f);
            }
            else
            {
                if (isSprinting)
                {

                    normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
                }
                else
                {
                    normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);

                }

                normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, normalCameraOrigin, Time.deltaTime * 6f);
            }




        }
        #endregion

        #region Private methods

        void HeadBob(float z, float x_intensity, float y_intensity)
        {
            if (!Input.GetMouseButton(1)) targetWeaponBobPosition = weaponParentCurrentPosition + new Vector3(Mathf.Cos(z) * x_intensity, Mathf.Sin(z * 2) * y_intensity, weaponParentOrigin.z);
        }

        private void RefreshHealthBar()
        {

            float t_health_ratio = (float)current_health / (float)max_health;
            ui_healthbar.localScale = Vector3.Lerp(ui_healthbar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 6f);

        }

        #endregion

        #region Public Methods

        public void TakeDamage(int p_damage)
        {
            if (photonView.IsMine)
            {
                current_health -= p_damage;
                RefreshHealthBar();
                Debug.Log(current_health);

                if (current_health <= 0)
                {
                    manager.Spawn();
                    PhotonNetwork.Destroy(gameObject);
                    Debug.Log("You Died");
                }
            }
        }

        #endregion
    }
}
