using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Alien : MonoBehaviour
{

    public PlayerHealth playerHealth;

    public enum alienTypes
    {
        Standard,
        Fast,
        Heavy,
        Shooter,
        Boss,
    }

    public enum alienStates
    {
        Idle,
        Hostile,
        Dead
    }

    public alienTypes alienType;
    public alienStates alienState = alienStates.Idle;

    public GameObject bullet;
    public Transform Gun;
    public Transform Gun2;
    public Transform frontOfGun;
    public Transform frontOfGun2;
    public GameObject lightObject;
    public GameObject lightObject2;

    public GameObject theKey;

    private Vector3 gunToPlayerDirection;
    private Vector3 alienToPlayerDirection;

    private Vector3 gunToPlayerDirection2;

    private int maxHealth;
    private int currHealth;
    private bool canRegen;
    private int regenRate;
    private bool regen = true;
    private float regenTime = 1f;
    private float nextTimeToRegen = 0f;
    private bool waitingOnRegen;

    public Slider healthBar;
    public Slider bossBar;

    private bool bossRage = false;
    private bool bossRage2 = false;

    private float speed;
    Vector3 moveDirection = Vector3.zero;
    public float turnTime = 0.05f;
    float turnSmoothVel;

    private float stopDistance = 8f;
    private float retreatDistance = 7f;

    Vector3 targetDirection = Vector3.zero;
    private bool isShooter;

    private int damage;
    private float attackSpeed;
    private float attackRange;
    private bool canAttack = true;
    private float nextTimeToAttack = 0f;

    private bool canShoot = true;
    private bool reloading = false;
    private float fireRate;
    private Quaternion randSpray;
    private float reloadTime;
    private float nextTimeToFire = 0f;
    private int maxAmmo = 5;
    private int currAmmo;

    public AudioSource shootingSound;
    public AudioClip[] pewPew;
    public AudioClip reload;

    public bool canPlayReloadSound = false;

    public GameObject player;
    public CharacterController controller;

    RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("/Player");
        playerHealth = GameObject.Find("Player Health Bar").GetComponent<PlayerHealth>();
        //bossBar = GameObject.Find("Boss Health Bar").GetComponentInChildren<Slider>();


        if(alienType == alienTypes.Standard)
        {
            maxHealth = 10;
            currHealth = maxHealth;
            speed = 2;
            damage = 5;
            attackSpeed = 1.5f;
            attackRange = 2f;
            isShooter = false;
            canRegen = false;
        }
        else if(alienType == alienTypes.Fast)
        {
            maxHealth = 10;
            currHealth = maxHealth;
            speed = 5;
            damage = 5;
            attackSpeed = 2f;
            attackRange = 2f;
            isShooter = false;
            canRegen = false;
        }
        else if(alienType == alienTypes.Shooter)
        {
            maxHealth = 6;
            currHealth = maxHealth;
            speed = 2;
            damage = 10;
            isShooter = true;
            fireRate = 5f;
            reloadTime = 2f;
            currAmmo = 0;
            lightObject.SetActive(false);
            canRegen = false;
        }
        else if(alienType == alienTypes.Heavy)
        {
            maxHealth = 50;
            currHealth = maxHealth;
            speed = 1;
            damage = 20;
            attackSpeed = 0.5f;
            attackRange = 4f;
            isShooter = false;
            transform.localScale = new Vector3(3.5f, 3.5f, 3.5f);
            canRegen = true;
            regenRate = 1;
        }
        else if(alienType == alienTypes.Boss)
        {
            maxHealth = 250;
            currHealth = maxHealth;
            speed = 1.5f;
            damage = 25;
            attackSpeed = 0.5f;
            attackRange = 6f;
            isShooter = true;
            fireRate = 5f;
            reloadTime = 1f;
            transform.localScale = new Vector3(4f, 4f, 4f);
            canRegen = true;
            regenRate = 2;
        }

        healthBar.value = UpdateHealthBar();
        if(alienType == alienTypes.Boss)
            bossBar.value = UpdateHealthBar();
    }

    // Update is called once per frame
    void Update()
    {
        if(currHealth <= 0)
        {
            Debug.Log("Alien dead");
            if(alienType == alienTypes.Heavy)
            {
                Instantiate(theKey, transform.position + Vector3.down*1.5f, transform.rotation);
            }
            alienState = alienStates.Dead;

            // Destruction now happens in the room manager, better that way
            //Destroy(this.transform.parent.gameObject, 1f);
        }
        if(!isShooter || alienType == alienTypes.Boss)
        {
            StandardMovement();
            StandardAttack();
        }
        else
        {
            ShooterMovement();
            ShooterAttack();
            Gun.transform.Find("GunModel").GetComponent<MeshRenderer>().enabled = true;     // Enable render of Alien's gun
            UpdateAlienGun();
        }

        healthBar.value = UpdateHealthBar();
        if(alienType == alienTypes.Boss)
            bossBar.value = UpdateHealthBar();

        if(canRegen && alienType != alienTypes.Boss)
            TryRegen();

        if(alienType == alienTypes.Boss && currHealth <= 0.5 * maxHealth)
            bossRage = true;

        if(alienType == alienTypes.Boss && currHealth <= 0.1 * maxHealth)
            bossRage2 = true;

        if(bossRage)
        {
            TryRegen();
            ShooterAttack();
            UpdateAlienGun();
            Gun.transform.Find("GunModel").GetComponent<MeshRenderer>().enabled = true;     // Enable render of Alien's gun
            Gun2.transform.Find("GunModel").GetComponent<MeshRenderer>().enabled = true;     // Enable render of Alien's gun
        }
        if(bossRage2)
        {
            speed = 2;
            regenRate = 5;
            fireRate = 10f;
            reloadTime = 0f;
        }
    }

    private bool LineOfSight()
    {
        // Draw a Raycast to determine if the player is in the alien's line of sight
        Physics.Raycast(transform.position + new Vector3 (0f, 2f, 0f), player.transform.position - transform.position, out hit, (player.transform.position - (transform.position + new Vector3 (0f, 2f, 0f))).magnitude);
        Debug.DrawRay(transform.position + new Vector3 (0f, 2f, 0f), player.transform.position - transform.position, Color.yellow);

        // If the alien sees the player... return true, else... return false.
        if(hit.collider.tag == "Player")
            return true;
        else
            return false;

    }

    // Movement of non-Shooter Aliens
    private void StandardMovement()
    {
        // If the alien sees the player... the alien moves toward the player
        if(LineOfSight())
        {
            float targetRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float smoothedRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVel, turnTime);
            transform.rotation = Quaternion.Euler(0f, smoothedRotation, 0f);

            if((transform.position - player.transform.position).magnitude > 1)
            {
                moveDirection = (player.transform.position - transform.position).normalized;
                moveDirection.y = 0f;
                controller.Move(moveDirection * speed * Time.deltaTime);
            }
        }
    }

    // Movement of Shooter Alien
    private void ShooterMovement()
    {
        if(LineOfSight())
        {
            targetDirection = (player.transform.position - transform.position).normalized;
            float targetRotation = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            float smoothedRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVel, turnTime);
            transform.rotation = Quaternion.Euler(0f, smoothedRotation, 0f);

            if(Vector3.Distance(transform.position, player.transform.position) > stopDistance)
            {
                controller.Move((player.transform.position - transform.position).normalized * speed * Time.deltaTime);
            }
            else if(Vector3.Distance(transform.position, player.transform.position) <= stopDistance && Vector3.Distance(transform.position, player.transform.position) >= retreatDistance)
            {
                transform.position = transform.position;
            }
            else
                controller.Move((player.transform.position - transform.position).normalized * -speed * Time.deltaTime);

            transform.rotation = Quaternion.Euler(0f, transform.rotation.y, 0f);
        }
        else
        {
            transform.position = transform.position;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    // Attack of non-Shooter Aliens
    private void StandardAttack()
    {
        if(LineOfSight() && (transform.position - player.transform.position).magnitude < attackRange)
        {
            //Debug.Log("try attack");
            tryAttack();
        }
    }

    private void tryAttack()
    {
        if(Time.time >= nextTimeToAttack)
            canAttack = true;
        else
            canAttack = false;

        //Debug.Log("can attack = " + canAttack);
        if(canAttack)
        {
            GameObject.Find("/GameManager").GetComponent<AudioSource>().clip = GameObject.Find("/GameManager").GetComponent<MainGameManager>().enemySFX[0];

            if(alienType == alienTypes.Heavy || alienType == alienTypes.Boss)
            {
                GameObject.Find("/GameManager").GetComponent<AudioSource>().pitch = Random.Range(0.7f, 0.85f);
            }
            else
                GameObject.Find("/GameManager").GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.2f);

            GameObject.Find("/GameManager").GetComponent<AudioSource>().Play();
           
            playerHealth.currHealth -= damage;
            canAttack = false;
            nextTimeToAttack = Time.time + 1/attackSpeed;
        }
    }

    // Attack of Shooter Alien
    private void ShooterAttack()
    {
        if(LineOfSight())
        {
            tryShoot();
        }
        
        if(LineOfSight() && currAmmo == 0)
        {
            tryReload();
        }
    }

    // Gives the Alien's gun its proper position and angle
    private void UpdateAlienGun()
    {
        if(alienType == alienTypes.Boss)
        {
            // Sets proper direction for gun to look in
            gunToPlayerDirection = player.transform.position - Gun.transform.position;
            gunToPlayerDirection.y = 0f;

            gunToPlayerDirection2 = player.transform.position - Gun2.transform.position;
            gunToPlayerDirection2.y = 0f;

            // Rotates the gun so it looks in that direction
            Gun.transform.rotation = Quaternion.LookRotation(gunToPlayerDirection, Vector3.up);

            Gun2.transform.rotation = Quaternion.LookRotation(gunToPlayerDirection2, Vector3.up);

            // Gets the direction of the alien looking at the player
            alienToPlayerDirection = (player.transform.position - transform.position).normalized;

            // Sets the position of the gun to be an offset on the direction between the alien and the player
            Gun.transform.position = new Vector3(transform.position.x - 0.8f, transform.position.y, transform.position.z + 1.3f) + alienToPlayerDirection * 0.8f;
            Gun.transform.position = new Vector3(Gun.transform.position.x, 3f, Gun.transform.position.z);

            Gun2.transform.position = new Vector3(transform.position.x + 0.8f, transform.position.y, transform.position.z + 1.3f) + alienToPlayerDirection * 0.8f;
            Gun2.transform.position = new Vector3(Gun2.transform.position.x, 3f, Gun2.transform.position.z);
        }
        else
        {
            // Sets proper direction for gun to look in
            gunToPlayerDirection = player.transform.position - Gun.transform.position;
            gunToPlayerDirection.y = 0f;

            // Rotates the gun so it looks in that direction
            Gun.transform.rotation = Quaternion.LookRotation(gunToPlayerDirection, Vector3.up);

            // Gets the direction of the alien looking at the player
            alienToPlayerDirection = (player.transform.position - transform.position).normalized;

            // Sets the position of the gun to be an offset on the direction between the alien and the player
            Gun.transform.position = transform.position + alienToPlayerDirection * 1.2f;
            Gun.transform.position = new Vector3(Gun.transform.position.x, 3f, Gun.transform.position.z);
        }
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
                if(alienType == alienTypes.Boss)
                {
                    shootingSound.clip = pewPew[Random.Range(0, pewPew.Length)];
                    shootingSound.pitch = Random.Range(1.15f, 1.45f);
                    shootingSound.Play();
                    nextTimeToFire = Time.time + 1f/fireRate;
                    //randSpray = Quaternion.Euler(frontOfGun.rotation.x, frontOfGun.rotation.y + Random.Range(-1f, 2f), frontOfGun.rotation.z);
                    Instantiate(bullet, frontOfGun.position, frontOfGun.rotation);
                    Instantiate(bullet, frontOfGun2.position, frontOfGun2.rotation);
                    currAmmo--;
                }
                else
                {
                    shootingSound.clip = pewPew[Random.Range(0, pewPew.Length)];
                    shootingSound.pitch = Random.Range(1.15f, 1.45f);
                    shootingSound.Play();
                    nextTimeToFire = Time.time + 1f/fireRate;
                    //randSpray = Quaternion.Euler(frontOfGun.rotation.x, frontOfGun.rotation.y + Random.Range(-1f, 2f), frontOfGun.rotation.z);
                    Instantiate(bullet, frontOfGun.position, frontOfGun.rotation);
                    currAmmo--;
                }
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
            shootingSound.pitch = 1.5f;

            if(canPlayReloadSound && (alienType != alienTypes.Boss))
                shootingSound.Play();

            canPlayReloadSound = false;

            reloading = true;
            lightObject.SetActive(false);

            if(alienType == alienTypes.Boss)
                lightObject2.SetActive(false);
            //Debug.Log("Reloading...");
            nextTimeToFire = Time.time + reloadTime;
            StartCoroutine(setCurrentAmmoToMax());
        }
    }

    private void TryRegen()
    {
        if(Time.time >= nextTimeToRegen)
            regen = true;
        else
            regen = false;

        Debug.Log("can regen = " + regen);
        if(regen)
        {
            Debug.Log("regen occured");
            // --- TODO: do some attack animation ---
            if(currHealth < maxHealth)
            {
                if(currHealth + regenRate > maxHealth)
                    currHealth = maxHealth;
                else
                    currHealth += regenRate;
            }
            else if(currAmmo >= maxHealth)
                currHealth = maxHealth;
            regen = false;
            nextTimeToRegen = Time.time + regenTime;
        }
    }

    IEnumerator setCurrentAmmoToMax()
    {
        yield return new WaitForSeconds(reloadTime);
        currAmmo = maxAmmo;
        reloading = false;
        canPlayReloadSound = true;
        lightObject.SetActive(true);

        if(alienType == alienTypes.Boss)
            lightObject2.SetActive(true);
        //Debug.Log("Shoot");
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.tag);
        
        
        if(other.tag == "Bullet")
        {
            //Debug.Log("Hit by bullet");
            Destroy(other.gameObject);

            if(currHealth > 0)
                currHealth--;
        }
        
    }

    float UpdateHealthBar()
    {
        // Keep the health bar positioned over the alien at all times
        healthBar.transform.position = transform.position + new Vector3(0f, 6f, 1f);
        // return the percentage of health the alien has left
        return (currHealth * 1.0f) / (maxHealth * 1.0f);
    }
}
