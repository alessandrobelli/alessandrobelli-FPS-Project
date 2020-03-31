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
        #endregion

        #region Monobehavior callbacks
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) equip(0);
        }

        #endregion


        #region Private methods
        void equip(int p_ind)
        {
            if (currentWeapon != null) Destroy(currentWeapon);

            // instantiate new weapon
            GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
            // 0,0,0 in the parent
            t_newWeapon.transform.localPosition = Vector3.zero;
            t_newWeapon.transform.localEulerAngles = Vector3.zero;

            currentWeapon = t_newWeapon;

        }
        #endregion
    }
}