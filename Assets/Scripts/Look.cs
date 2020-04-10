using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.Nudi.Fpsproject
{
    public class Look : MonoBehaviourPunCallbacks
    {
        #region Variables

        public static bool cursorLocked = true;

        public Transform player;
        public Transform cams;
        public Transform weapon;
        public float xSensitivy;
        public float ySensitivy;
        public float maxAngle = 60;
        private Quaternion camCenter;

        #endregion

        #region Monobehavior callbacks


        // Start is called before the first frame update
        void Start()
        {
            // set rotation origin for cameras to camCenter
            camCenter = cams.localRotation;

        }

        // Update is called once per frame
        void Update()
        {
            if(!photonView.IsMine) return;
            SetY();
            SetX();
            UpdateCursorLock();
        }

        #endregion

        #region Private methods

        void SetY()
        {
            float t_input = Input.GetAxisRaw("Mouse Y") * ySensitivy * Time.deltaTime;
            Quaternion t_adj = Quaternion.AngleAxis(t_input, -Vector3.right);
            Quaternion t_delta = cams.localRotation * t_adj;


            if (Quaternion.Angle(camCenter, t_delta) < maxAngle)
            {
                cams.localRotation = t_delta;
            }

            weapon.rotation = cams.rotation;

        }

        void SetX()
        {
            float t_input = Input.GetAxisRaw("Mouse X") * xSensitivy * Time.deltaTime;
            Quaternion t_adj = Quaternion.AngleAxis(t_input, Vector3.up);
            Quaternion t_delta = player.localRotation * t_adj;
            player.localRotation = t_delta;


        }

        void UpdateCursorLock()
        {
            if (cursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    cursorLocked = false;
                }

            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    cursorLocked = true;
                }

            }

        }

        #endregion


    }
}
