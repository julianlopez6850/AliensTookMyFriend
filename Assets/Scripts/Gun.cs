using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    private Vector3 gunToMouseDirection;
    private Vector3 playerToMouseDirection;

    //public GameObject target;
    public GameObject player;
    public GameObject bullet;
    public Transform frontOfGun;

    //public GameObject mousePosCube;

    
    private bool canShoot = true;
    private bool reloading = false;
    private float fireRate = 15f;
    private float reloadTime = 2f;
    private float nextTimeToFire = 0f;

    private int maxAmmo = 30;
    private int currAmmo;

    public Text ammoText;

    public AudioSource shootingSound;
    public AudioClip[] pewPew;
    public AudioClip reload;

    public bool canPayReloadSound = true;

    public GameObject lightObject;

    // Start is called before the first frame update
    void Start()
    {
        currAmmo = maxAmmo;
    }

    // Update is called once per frame
    void Update()
    {
        if(!player.GetComponent<PlayerMovement>().isDead && !player.GetComponent<PlayerMovement>().gameOver && !player.GetComponent<PlayerMovement>().inActive)
        {
                // Sets proper mouse position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
            mousePos = new Vector3(mousePos.x, transform.position.y, mousePos.z);

            //mousePosCube.transform.position = mousePos;

            // Sets proper direction for gun to look in
            gunToMouseDirection = mousePos - transform.position;
            gunToMouseDirection.y = 0f;

            // Rotates the gun so it looks in that direction
            transform.rotation = Quaternion.LookRotation(gunToMouseDirection, Vector3.up);

            // Gets the direction of the player looking at the cursor
            playerToMouseDirection = (mousePos - player.transform.position).normalized;

            // Sets the position of the gun to be an offset on the direction between the player and the cursor
            transform.position = player.transform.position + playerToMouseDirection * 1.2f;
            transform.position = new Vector3(transform.position.x, 3f, transform.position.z);

            if(Input.GetButton("Fire1"))
            {
                tryShoot();
            }

            if(Input.GetKeyDown(KeyCode.R))
            {
                tryReload();
            }
        }
        else
            this.gameObject.SetActive(false);

        ammoText.text = "AMMO: " + currAmmo;
    }

    void tryShoot()
    {
        if(Time.time >= nextTimeToFire)
            canShoot = true;
        else
            canShoot = false;

        if(canShoot)
        {
            if(currAmmo > 0)
            {
                nextTimeToFire = Time.time + 1f/fireRate;
                Instantiate(bullet, frontOfGun.position, frontOfGun.rotation);
                shootingSound.clip = pewPew[Random.Range(0, pewPew.Length)];
                shootingSound.pitch = Random.Range(0.85f, 1.15f);
                shootingSound.Play();
                currAmmo--;
            }
            else if(currAmmo == 0)
            {
                tryReload();
            }
        }
    }

    void tryReload()
    {
        if(reloading == false)
        {
            canShoot = false;
            shootingSound.clip = reload;
            shootingSound.pitch = 1f;

            if(canPayReloadSound)
                shootingSound.Play();

            canPayReloadSound = false;
            
            reloading = true;
            lightObject.SetActive(false);
            //Debug.Log("Reloading...");
            nextTimeToFire = Time.time + reloadTime;
            StartCoroutine(setCurrentAmmoToMax());
        }
    }

    IEnumerator setCurrentAmmoToMax()
    {
        yield return new WaitForSeconds(reloadTime);
        currAmmo = maxAmmo;
        reloading = false;
        canPayReloadSound = true;
        lightObject.SetActive(true);
        //Debug.Log("Shoot");
    }
}
