using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Nudi.Fpsproject
{

    public class Motion : MonoBehaviour
    {

        public float speed;

        private Rigidbody rig;

        public void Start()
        {
            Camera.main.enabled = false;
            rig = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
             float t_vmove = Input.GetAxisRaw("Vertical");
            float t_hmove = Input.GetAxisRaw("Horizontal");
           

            Vector3 t_direction = new Vector3(t_hmove, 0,  t_vmove);
            t_direction.Normalize();

            rig.velocity = transform.TransformDirection(t_direction) * speed * Time.fixedDeltaTime;



        }
    }
}
