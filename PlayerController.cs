using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;

public class PlayerController : MonoBehaviourPunCallbacks
{
    
    [Header("Character Controller Parameter")]
    public CharacterController charCon;

    [Space]

    [Header("Transform Parameters")]
    public Transform viewPoint;
    public Transform groundCheckPoint;

    [Space]

    [Header("Sensitivity & Store Parameters")]
    public float mouseSensitivity = 1f;
    private float verticalRotStore;

    [Space]

    [Header("Boolean Parameters")]
    public bool invertLook;
    private bool isGrounded;

    [Space]

    [Header("Speed Parameters")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    private float activeMoveSpeed;

    [Space]

    [Header("Vector Parameters")]
    private Vector2 mouseInput;
    private Vector3 moveDir;
    private Vector3 movement;

    [Space]

    [Header("Camera Parameters")]
    private Camera cam;

    [Space]
    [Header("Jump Parameters")]
    public float jumpForce = 12f;
    public float gravityMod = 2.5f;

    [Space]
    [Header("Layer Mask Parameters")]
    public LayerMask groundLayers;

    [Space]
    [Header("GameObject Parameters")]
    public GameObject bulletImpactPrefab;

    [Space]

    [Header("Shot Time Parameters")]
    //public float timeBetweenShots = .1f;
    private float shotCounter;

    [Space]

    [Header("Weapon Heat Parameters")]
    public float maxHeat = 10f;
    //public float heatPerShot = 1f;
    public float coolRate = 4f;
    public float overheatCoolRate = 5f;
    private float heatCounter;
    private bool overheated;

    [Space]

    [Header("Gun Parameters")]
    public Gun[] allGuns;
    private int selectedGun;

    [Space]
    [Header("Muzzle Parameters")]
    public float muzzleDisplayTime;
    private float muzzleCounter;

    [Space]

    [Header("Hit Impact Parameters")]
    public GameObject playerHitImpact;

    [Space]

    [Header("Health Paramters")]
    public int maxHealth = 100;
    private float currentHealth;

    [Space]

    [Header("Animation Parameters")]
    public Animator anim;

    [Space]
    [Header("Player Model Parameters")]
    public GameObject playerModel;
    public Transform modelGunPoint;
    public Transform gunHolder;   
    public Material[] allSkins;

    [Space]
    [Header("Aim Down Sight Parameters")]
    public float adsSpeed = 5f;
    public Transform adsOutPoint;
    public Transform adsInPoint;

    [Space]
    [Header("Audio Source Parameters")]
    public AudioSource footstepSlow;
    public AudioSource footstepFast;


    void Start()
    {

        Cursor.lockState = CursorLockMode.Locked;

        cam = Camera.main;

        UIController.instance.weaponTempSlider.maxValue = maxHeat;
        UIController.instance.crosshair.gameObject.SetActive(true);

        //SwitchGun();

        photonView.RPC("SetGun", RpcTarget.All, selectedGun);


        //Transform newTransform = SpawnManager.instance.GetSpawnPoint();
        //transform.position = newTransform.position;
        //transform.rotation = newTransform.rotation;

        currentHealth = maxHealth;
        

        if(photonView.IsMine)
        {

            playerModel.SetActive(false);

            UIController.instance.healthSlider.maxValue = maxHealth;
            UIController.instance.healthSlider.value = currentHealth;


        } else
        {

            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }

        playerModel.GetComponent<Renderer>().material = allSkins[photonView.Owner.ActorNumber % allSkins.Length];

    }

    
    void Update() 
    {

        if(photonView.IsMine)
        {

        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);

        // This is mouse invert option in the games.

        if(invertLook)
        {

            viewPoint.rotation = Quaternion.Euler(verticalRotStore, viewPoint.transform.rotation.eulerAngles.y, viewPoint.transform.rotation.eulerAngles.z);

        } else
        {

            viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);

        }


        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        

            if (Input.GetKey(KeyCode.LeftShift))
            {

                activeMoveSpeed = runSpeed;
               
                if(!footstepFast.isPlaying && moveDir != Vector3.zero)
                {

                    footstepFast.Play();
                    footstepSlow.Stop();
                }

            }
            else
            {

               activeMoveSpeed = moveSpeed;

                if(!footstepSlow.isPlaying && moveDir != Vector3.zero)
                {

                    footstepFast.Stop();
                    footstepSlow.Play();
                    

                }

            }

            if(moveDir == Vector3.zero || !isGrounded)
            {
                footstepSlow.Stop();
                footstepFast.Stop();

            }



        float yVel = movement.y;
        movement = (transform.right * moveDir.x + transform.forward * moveDir.z).normalized * activeMoveSpeed;
        movement.y = yVel;

        if (charCon.isGrounded)
        {

            movement.y = 0f;
            

        }

        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down,.25f, groundLayers);

        if(Input.GetButtonDown("Jump") && isGrounded)
        {

            movement.y = jumpForce;

        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;

        charCon.Move(movement * Time.deltaTime);

        if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
        {

            muzzleCounter -= Time.deltaTime;

            if(muzzleCounter <= 0)
            {

                allGuns[selectedGun].muzzleFlash.SetActive(false);

            }


        }  
        

        if (!overheated) { 

        if(Input.GetMouseButtonDown(0))
        {

            Shoot();

        }

        if(Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
        {

            shotCounter -= Time.deltaTime;

            if(shotCounter <= 0)
            {

                Shoot();


            }

        }

            heatCounter -= coolRate * Time.deltaTime;

        } else
        {

            heatCounter -= overheatCoolRate * Time.deltaTime; 

            if(heatCounter <= 0)
            {

               
                overheated = false;
                UIController.instance.overheatedMessage.gameObject.SetActive(false);
                UIController.instance.crosshair.gameObject.SetActive(true);
            }


        }

        if (heatCounter <= 0f)
        {
            heatCounter = 0f;
            
        }

        UIController.instance.weaponTempSlider.value = heatCounter;


        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f) // Fare Tekerlegini Yukariya Kaydirmak
        {

            selectedGun++; // Default Gun Type is Pistol and When We Scroll Up, The Gun Index Increase 1 Step

            if (selectedGun >= allGuns.Length)
            {
                selectedGun = 0;
            }

                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);

        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f) {


            selectedGun--;

            if(selectedGun < 0) // Secilen silahin indisi 0'dan kucuk olamaz.
            {

                selectedGun = allGuns.Length - 1; // Silah (All Guns) Dizisindeki En Son Silaha Gidiyor

            }

                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);


        }

        for(int i = 0; i < allGuns.Length; i++)
        {

            if(Input.GetKeyDown((i + 1).ToString()))
            {

                selectedGun = i;
                    //SwitchGun();
                    photonView.RPC("SetGun", RpcTarget.All, selectedGun);


            }


        }

            anim.SetBool("grounded", isGrounded);
            anim.SetFloat("speed", moveDir.magnitude);


            if(Input.GetMouseButton(1))
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,allGuns[selectedGun].adsZoom, adsSpeed * Time.deltaTime);
                gunHolder.position = Vector3.Lerp(gunHolder.position, adsInPoint.position, adsSpeed * Time.deltaTime);

            } else
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, adsSpeed * Time.deltaTime); // 60f is default camera fov
                gunHolder.position = Vector3.Lerp(gunHolder.position, adsOutPoint.position, adsSpeed * Time.deltaTime);
            }




        if (Input.GetKeyDown(KeyCode.Escape)) // ESC Tuþu
        {
            Cursor.lockState = CursorLockMode.None;

        } else if(Cursor.lockState == CursorLockMode.None)
        {
            
            if(Input.GetMouseButtonDown(0) && !UIController.instance.optionsScreen.activeInHierarchy)
            {

                Cursor.lockState = CursorLockMode.Locked;

            }

        }

        }

    }

    private void Shoot()
    {

        if(photonView.IsMine && !UIController.instance.isPaused) { 

        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        ray.origin = cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {

            //Debug.Log("We hit " + hit.collider.gameObject.name); // Vurdugumuz nesnenin collider'i

            if (hit.collider.gameObject.tag == "Player")
            {

                Debug.Log("Hit : " + hit.collider.gameObject.GetPhotonView().Owner.NickName);

                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, allGuns[selectedGun].shotDamage, PhotonNetwork.LocalPlayer.ActorNumber);


            }
            else
            {



                PhotonNetwork.Instantiate(bulletImpactPrefab.name, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                
                if(photonView.IsMine && PhotonNetwork.IsConnected)
                     PhotonNetwork.Destroy(bulletImpactPrefab);

                    Debug.Log(photonView.gameObject);

                //GameObject bulletImpactObject = Instantiate(bulletImpactPrefab, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));

                //Destroy(bulletImpactObject, 10f);



            }


        }

        shotCounter = allGuns[selectedGun].timeBetweenShots;

        heatCounter += allGuns[selectedGun].heatPerShot;

        if (heatCounter >= maxHeat)
        {

            heatCounter = maxHeat;

            overheated = true;

            UIController.instance.overheatedMessage.gameObject.SetActive(true);
            UIController.instance.crosshair.gameObject.SetActive(false);


        }


        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;

            #region Shot Sound Play
            allGuns[selectedGun].shotSound.Stop();    
        allGuns[selectedGun].shotSound.Play();
            #endregion
        }

    }

    
    [PunRPC]
    public void DealDamage(string damager, int damageAmount, int actor)
    {

        TakeDamage(damager, damageAmount,actor);


    }

    public void TakeDamage(string damager, int damageAmount, int actor)
    {

        if (photonView.IsMine)
        {

            //Debug.Log(photonView.Owner.NickName + " been hit by : " + damager);

            currentHealth -= damageAmount;

            if(currentHealth <= 0)
            {

                currentHealth = 0;

                PlayerSpawner.instance.Die(damager);

                MatchManager.instance.UpdateStatsSend(actor, 0, 1);

            }

            UIController.instance.healthSlider.value = currentHealth;

        }
    }


    
    private void LateUpdate()
    {

        if (photonView.IsMine)
        {

            if (MatchManager.instance.state == MatchManager.GameState.Playing)
            {

                cam.transform.position = viewPoint.position;
                cam.transform.rotation = viewPoint.rotation;
            } else
            {

                cam.transform.position = MatchManager.instance.mapCamPoint.position;
                cam.transform.rotation = MatchManager.instance.mapCamPoint.rotation;

            }
        }
    }

    void SwitchGun()
    {

        foreach (Gun gun in allGuns)
        {
            
            gun.gameObject.SetActive(false);

        }

        allGuns[selectedGun].gameObject.SetActive(true);

        allGuns[selectedGun].muzzleFlash.gameObject.SetActive(false);

    }

    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {

        if(gunToSwitchTo < allGuns.Length)
        {

            selectedGun = gunToSwitchTo;
            SwitchGun();


        }


    }
   
    

}
