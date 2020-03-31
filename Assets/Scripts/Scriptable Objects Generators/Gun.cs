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
        public float firerate;
        public GameObject prefab;
        #endregion
    }
}