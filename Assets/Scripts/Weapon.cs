using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Com.Nudi.Fpsproject
{
    public class Weapon : MonoBehaviourPunCallbacks
    {
        #region Variables
        public Gun[] loadout;
        public Transform weaponParent;
        private GameObject currentWeapon;
        public GameObject bulletHolePrefab;
        public LayerMask canBeShot;
        private int currentWeaponEquipped;
        private float currentCooldown;
        private int OTHERPLAYERS = 11;
        private bool isReloading = false;
        #endregion

        #region Monobehavior callbacks
        // Start is called before the first frame update
        void Start()
        {
            foreach (Gun g in loadout) g.Init();
            if (photonView.IsMine)
            {
                photonView.RPC("Equip", RpcTarget.All, 0);
            }
        }

        // Update is called once per frame
        void Update()
        {



            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1))
            {
                photonView.RPC("Equip", RpcTarget.All, 0);
            }

            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2))
            {
                photonView.RPC("Equip", RpcTarget.All, 1);
            }

            if (currentWeapon != null)
            {
                if (photonView.IsMine)
                {
                    Aim(Input.GetMouseButton(1));

                    if (loadout[currentWeaponEquipped].fireType != burst.Auto)
                    {
                        if (Input.GetMouseButtonDown(0) && currentCooldown <= 0f)
                        {
                            if (loadout[currentWeaponEquipped].CanFireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                            else StartCoroutine(Reload(loadout[currentWeaponEquipped].reloadTime));
                        }

                    }
                    else
                    {
                        if (Input.GetMouseButton(0) && currentCooldown <= 0f)
                        {
                            if (loadout[currentWeaponEquipped].CanFireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                            else StartCoroutine(Reload(loadout[currentWeaponEquipped].reloadTime));
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.R)) StartCoroutine(Reload(loadout[currentWeaponEquipped].reloadTime));

                    // cooldown
                    if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
                }

                // weapon elasticity
                currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
            }


        }

        #endregion


        #region Public methods

        public void RefreshAmmo(Text p_text)
        {
            int t_clip = loadout[currentWeaponEquipped].GetClip();
            int t_stache = loadout[currentWeaponEquipped].GetAmmo();

            p_text.text = t_clip.ToString("D2") + " / " + t_stache.ToString("D2");

        }

        #endregion

        #region Private methods

        IEnumerator Reload(float p_wait)
        {

            isReloading = true;

            currentWeapon.SetActive(false);

            yield return new WaitForSeconds(p_wait);

            loadout[currentWeaponEquipped].Reload();
            currentWeapon.SetActive(true);

            isReloading = false;

        }


        [PunRPC]
        void Equip(int p_ind)
        {
            if (currentWeapon != null)
            {
                if (isReloading) StopCoroutine("Reload");
                Destroy(currentWeapon);
            }

            currentWeaponEquipped = p_ind;

            // instantiate new weapon
            GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
            // 0,0,0 in the parent
            t_newWeapon.transform.localPosition = Vector3.zero;
            t_newWeapon.transform.localEulerAngles = Vector3.zero;
            t_newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;

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
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_ads.position, Time.deltaTime * loadout[currentWeaponEquipped].aimSpeed);
            }
            else
            {
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_hip.position, Time.deltaTime * loadout[currentWeaponEquipped].aimSpeed);
            }

        }

        [PunRPC]
        void Shoot()
        {
            Transform t_spawn = transform.Find("Cameras/NormalCamera");

            // bloom
            Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
            t_bloom += Random.Range(-loadout[currentWeaponEquipped].bloom, loadout[currentWeaponEquipped].bloom) * t_spawn.up;
            t_bloom += Random.Range(-loadout[currentWeaponEquipped].bloom, loadout[currentWeaponEquipped].bloom) * t_spawn.right;
            t_bloom -= t_spawn.position;
            t_bloom.Normalize();


            // cooldown
            currentCooldown = loadout[currentWeaponEquipped].firerate;

            // raycast 
            RaycastHit t_hit = new RaycastHit();

            if (Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 1000f, canBeShot))
            {
                GameObject t_newHole = Instantiate(bulletHolePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
                Destroy(t_newHole, 5f);

                if (photonView.IsMine)
                {
                    bool youShootAnotherPlayer = t_hit.collider.gameObject.layer == OTHERPLAYERS;
                    if (youShootAnotherPlayer)
                    {
                        PhotonView Enemy = t_hit.collider.gameObject.GetPhotonView();
                        Enemy.RPC("TakeDamage", RpcTarget.All, loadout[currentWeaponEquipped].damage);
                    }
                }

            }

            // gun fx
            currentWeapon.transform.Rotate(-loadout[currentWeaponEquipped].recoil, 0, 0);
            currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentWeaponEquipped].kickback;



        }

        [PunRPC]
        private void TakeDamage(int p_damage)
        {
            GetComponent<PlayerController>().TakeDamage(p_damage);
        }

        #endregion
    }
}