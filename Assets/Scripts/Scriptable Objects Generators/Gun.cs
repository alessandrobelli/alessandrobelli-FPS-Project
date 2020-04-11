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
        public float firerate;
        public float aimSpeed;
        public float bloom;
        public float recoil;
        public float kickback;
        public GameObject prefab;


        #endregion
    }
}