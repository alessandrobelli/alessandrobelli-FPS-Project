using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Nudi.Fpsproject
{
    public class Weapon : MonoBehaviour
    {
        #region Variables
        public Gun[] loadout;
        public Transform weaponParent;
        private GameObject currentWeapon;
        public GameObject bulletHolePrefab;
        public LayerMask canBeShot;
        private int currentIndex;
        #endregion

        #region Monobehavior callbacks
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (currentWeapon != null)
            {
                Aim(Input.GetMouseButton(1));
                if (Input.GetMouseButtonDown(0)) Shoot();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) equip(0);
        }

        #endregion


        #region Private methods
        void equip(int p_ind)
        {
            if (currentWeapon != null) Destroy(currentWeapon);

            currentIndex = p_ind;

            // instantiate new weapon
            GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
            // 0,0,0 in the parent
            t_newWeapon.transform.localPosition = Vector3.zero;
            t_newWeapon.transform.localEulerAngles = Vector3.zero;

            currentWeapon = t_newWeapon;

        }

        void Aim(bool p_isAiming)
        {
            Transform t_anchor = currentWeapon.transform.Find("Anchor");
            Transform t_state_ads = currentWeapon.transform.Find("States/ADS");
            Transform t_state_hip = currentWeapon.transform.Find("States/Hip");

            if (p_isAiming)
            {
                // aim
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_ads.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
            }
            else
            {
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_hip.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
            }

        }

        void Shoot()
        {
            Transform t_spawn = transform.Find("Cameras/NormalCamera");
            RaycastHit t_hit = new RaycastHit();

            if (Physics.Raycast(t_spawn.position, t_spawn.forward, out t_hit, 1000f, canBeShot))
            {
                GameObject t_newHole = Instantiate (bulletHolePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
                Destroy(t_newHole,5f);
            }
        }
        #endregion
    }
}