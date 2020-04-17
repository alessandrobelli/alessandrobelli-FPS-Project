using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System;

namespace Com.Nudi.Fpsproject
{
    // if(!photonView.IsMine) return; check if the player I'm trying to move is my player.

    public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
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
        public float crouchModifier;

        public float slideAmount;
        public float crouchAmount;
        public GameObject standingCollider;
        public GameObject crouchingCollider;


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
        private bool crouched;
        private int aimAngle;

        #endregion

        #region Monobehavior callbacks

        private void ChangeLayerRecursively(Transform p_trans, int p_layer)
        {
            p_trans.gameObject.layer = p_layer;
            foreach (Transform t in p_trans) ChangeLayerRecursively(t, p_layer);
        }
        private void Start()
        {
            current_health = max_health;
            manager = GameObject.Find("Manager").GetComponent<Manager>();
            weapon = GetComponent<Weapon>();

            if (!photonView.IsMine)
            {
                gameObject.layer = 11;
                standingCollider.layer = 11;
                crouchingCollider.layer = 11;
                //ChangeLayerRecursively(mesh.transform, 11);
            }

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
            if (!photonView.IsMine)
            {
                RefreshMultiplayerState();
                return;
            }

            float t_vmove, t_hmove;
            bool isJumping, isSprinting, isCrouching;
            UpdateVarInit(out t_vmove, out t_hmove, out isJumping, out isSprinting, out isCrouching);

            if (Input.GetKeyDown(KeyCode.U)) TakeDamage(100);

            if (isCrouching)
            {
                photonView.RPC("SetCrouch", RpcTarget.All, !crouched);
            }


            // jump
            if (isJumping)
            {
                if (crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
                rig.AddForce(Vector3.up * jumpForce);
            }

            Headbob(t_vmove, t_hmove, isSprinting);

            // UI Refreshes
            RefreshHealthBar();
            weapon.RefreshAmmo(ui_ammo);

        }

        private void RefreshMultiplayerState()
        {
            float cachEulY = weaponParent.localEulerAngles.y;

            Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
            weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

            Vector3 finalRotation = weaponParent.localEulerAngles;
            finalRotation.y = cachEulY;

            weaponParent.localEulerAngles = finalRotation;
        }



        // Update is called once per frame
        void FixedUpdate()
        {
            if (!photonView.IsMine) return;

            float t_vmove, t_hmove, t_adjustedSpeed;
            bool isSprinting, isSliding;
            Vector3 t_direction;
            FixedUpdateVarInit(out t_vmove, out t_hmove, out t_adjustedSpeed, out isSprinting, out isSliding, out t_direction);




            if (!sliding)
            {
                t_direction = new Vector3(t_hmove, 0, t_vmove);
                t_direction.Normalize();
                t_direction = transform.TransformDirection(t_direction);

                if (isSprinting)
                {
                    if (crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
                    t_adjustedSpeed *= sprintModifier;
                }
                else if (crouched)
                {
                    t_adjustedSpeed *= crouchModifier;
                }

            }
            else
            {
                t_direction = slide_dir;
                t_adjustedSpeed *= slideModifier;

                slide_time -= Time.fixedDeltaTime;
                if (slide_time <= 0)
                {
                    sliding = false;
                    weaponParentCurrentPosition -= Vector3.down * (slideAmount - crouchAmount);
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
                weaponParentCurrentPosition += Vector3.down * (slideAmount - crouchAmount);
                if (!crouched) photonView.RPC("SetCrouch", RpcTarget.All, true);

            }

            // Camera Stuff
            if (sliding)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier * 1.15f, Time.deltaTime * 8f);
                normalCam.transform.localPosition = Vector3.MoveTowards(normalCam.transform.localPosition, normalCameraOrigin + Vector3.down * slideAmount, Time.deltaTime);
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

                if (crouched)
                {
                    normalCam.transform.localPosition = Vector3.MoveTowards(normalCam.transform.localPosition, normalCameraOrigin + Vector3.down * crouchAmount, Time.deltaTime * 6f);
                }
                else
                {
                    normalCam.transform.localPosition = Vector3.MoveTowards(normalCam.transform.localPosition, normalCameraOrigin, Time.deltaTime * 6f);
                }


            }




        }

        #endregion

        #region Private methods

        [PunRPC]
        void SetCrouch(bool p_state)
        {
            if (crouched == p_state) return;

            crouched = p_state;

            if (crouched)
            {
                standingCollider.SetActive(false);
                crouchingCollider.SetActive(true);
                weaponParentCurrentPosition += Vector3.down * crouchAmount;
            }
            else
            {
                standingCollider.SetActive(true);
                crouchingCollider.SetActive(false);
                weaponParentCurrentPosition -= Vector3.down * crouchAmount;
            }
        }

        private void Headbob(float t_vmove, float t_hmove, bool isSprinting)
        {
            // headbob
            if (sliding)
            {
                SetHeadBob(movementCounter, 0.15f, 0.070f);
                movementCounter += Time.deltaTime * 7f;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);

            }
            else if (t_hmove == 0 && t_vmove == 0)
            {
                // idle
                SetHeadBob(idleCounter, 0.025f, 0.025f);
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else if (!isSprinting && !crouched)
            {
                // walking
                SetHeadBob(movementCounter, 0.035f, 0.035f);
                movementCounter += Time.deltaTime * 3f;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else if (crouched)
            {
                // crouching
                SetHeadBob(movementCounter, 0.02f, 0.02f);
                movementCounter += Time.deltaTime * 4f;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);

            }
            else
            {
                // sprinting 
                SetHeadBob(movementCounter, 0.15f, 0.070f);
                movementCounter += Time.deltaTime * 7f;
                weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
            }
        }
        private void FixedUpdateVarInit(out float t_vmove, out float t_hmove, out float t_adjustedSpeed, out bool isSprinting, out bool isSliding, out Vector3 t_direction)
        {
            // get input
            t_vmove = Input.GetAxisRaw("Vertical");
            t_hmove = Input.GetAxisRaw("Horizontal");

            // control
            t_adjustedSpeed = speed;
            bool jumpButtonPressed = Input.GetKeyDown(KeyCode.Space);
            bool sprintButtonPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool notMovingBackwards = t_vmove > 0;
            bool slideButtonPressed = Input.GetKey(KeyCode.C);

            // #states
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jumpButtonPressed && isGrounded;
            isSprinting = sprintButtonPressed && notMovingBackwards && !isJumping && isGrounded;
            isSliding = isSprinting && slideButtonPressed && !sliding;


            // movement
            t_direction = Vector3.zero;
        }

        private void UpdateVarInit(out float t_vmove, out float t_hmove, out bool isJumping, out bool isSprinting, out bool isCrouching)
        {

            // get input
            t_vmove = Input.GetAxisRaw("Vertical");
            t_hmove = Input.GetAxisRaw("Horizontal");

            // control
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool notMovingBackwards = t_vmove > 0;
            bool crouch = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);

            // states
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.15f, ground);
            isJumping = jump && isGrounded;
            isSprinting = sprint && notMovingBackwards && !isJumping && isGrounded;
            isCrouching = crouch && !isSprinting && !isJumping && isGrounded;
        }

        void SetHeadBob(float z, float x_intensity, float y_intensity)
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

        public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message)
        {
            if(p_stream.IsWriting)
            {
                p_stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100));
            }
            else
            {
                aimAngle = (int)p_stream.ReceiveNext() / 100;
                
            }


            throw new System.NotImplementedException();
        }

        #endregion
    }
}
