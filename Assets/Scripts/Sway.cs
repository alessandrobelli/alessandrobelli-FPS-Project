using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Com.Nudi.Fpsproject
{
    public class Sway : MonoBehaviour
    {
        #region Variables
        public float intensity;
        public float smooth;
        private Quaternion origin_rotation;
        #endregion

        #region Monobehavior callbacks
        private void Start()
        {
            origin_rotation = transform.localRotation;
        }
        private void Update()
        {
            UpdateSway();
        }
        #endregion


        #region Private methods
        private void UpdateSway()
        {
            //controls
            float t_x_mouse = Input.GetAxis("Mouse X");
            float t_y_mouse = Input.GetAxis("Mouse Y");

            //calculate target rotation
            Quaternion t_x_adjust = Quaternion.AngleAxis(-intensity * t_x_mouse, Vector3.up);
            Quaternion t_y_adjust = Quaternion.AngleAxis(intensity * t_y_mouse, Vector3.up);
            Quaternion target_rotation = origin_rotation * t_x_adjust * t_y_adjust;

            // rotate towards target rotation
            transform.localRotation = Quaternion.Lerp(transform.localRotation, target_rotation, Time.deltaTime * smooth);

        }
        #endregion
    }
}