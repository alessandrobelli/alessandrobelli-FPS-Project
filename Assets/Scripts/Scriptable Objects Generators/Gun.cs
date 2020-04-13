using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Nudi.Fpsproject
{
    [CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
    public class Gun : ScriptableObject
    {
        #region Variables
        public string name;
        public int damage;
        public int ammo;
        public int clipSize;
        public float reloadTime;

        public float firerate;
        public float aimSpeed;
        public float bloom;
        public float recoil;
        public float kickback;
        public GameObject prefab;
        private int currentAmmo;
        private int currentClip;

        #endregion

        #region Private Methods


        public void Init()
        {
            currentClip = clipSize;
            currentAmmo = ammo;
        }
        public bool CanFireBullet()
        {
            if (currentClip > 0)
            {
                currentClip -= 1;
                return true;
            }
            else return false;
        }

        public void Reload()
        {
            currentAmmo += currentClip;

            currentClip = Mathf.Min(clipSize, currentAmmo);

            currentAmmo -= currentClip;
        }

        public int GetAmmo()
        {
            return currentAmmo;
        }

        public int GetClip()
        {
            return currentClip;
        }

        #endregion
    }
}