using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public Animator astroAnimator;

    Vector3 userInput = Vector3.zero;
    Vector3 moveDirection = Vector3.zero;

    private float currSpeed = 0f;
    public float maxWalkSpeed = 6f;
    public float maxSprintSpeed = 10f;

    public float turnTime = 0.05f;
    float turnSmoothVel;

    public CharacterController controller;
    float musicVel = 0f;

    public bool inActive = false;
    public bool isDead = false;
    public bool gameOver = false;

    public Text timerText;
    private float startTime;
    private float totalTime;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(this.gameObject.tag == "Player")
        {
            userInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

            if(userInput.magnitude >= 0.1f && !isDead && !gameOver && !inActive)
            {
                if(Input.GetButton("Sprint"))
                    currSpeed = maxSprintSpeed;
                else
                    currSpeed = maxWalkSpeed;

                moveDirection = userInput * currSpeed;

                float targetRotation = Mathf.Atan2(userInput.x, userInput.z) * Mathf.Rad2Deg;
                float smoothedRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVel, turnTime);


                transform.rotation = Quaternion.Euler(0f, smoothedRotation, 0f);


                controller.Move(moveDirection * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, 2f, transform.position.z);

                astroAnimator.SetFloat("Speed", moveDirection.magnitude);
                astroAnimator.SetBool("Moving", true);
            }
            else
            {
                if(!inActive)
                    astroAnimator.SetBool("Moving", false);
            }

            if(!gameOver && playerHealth.currHealth <= 0f)
            {
                isDead = true;
            }
            if(isDead)
            {
                astroAnimator.SetTrigger("Death");
                isDead = false;
                gameOver = true;

                StartCoroutine(handleGameOver());
                // go back to main menu (maybe reload scene? idk)
            }
            if(gameOver)
            {
                AudioSource gameMusic = GameObject.Find("/Main Camera").GetComponent<AudioSource>();
                gameMusic.volume = Mathf.SmoothDamp(gameMusic.volume, 0f, ref musicVel, 1.5f);
            }

            if(!gameOver && !GameObject.Find("/GameManager").GetComponent<MainGameManager>().gameWon)
                totalTime = Time.time - startTime;

            string minutes = ((int) totalTime/60).ToString("00");
            string seconds = (totalTime % 60).ToString("00.00");

            timerText.text = minutes + ":" + seconds;
        }

    }

    IEnumerator handleGameOver()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("Go to main menu here");
        GameObject.Find("/GameManager").GetComponent<MainGameManager>().gameOver = true;
        //astroAnimator.SetTrigger("Death");

    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.tag);
        if(other.tag == "AlienBullet")
        {
            playerHealth.currHealth -= 10;
            Destroy(other.gameObject);
        }

        if(other.tag == "Key")
        {
            GameObject.Find("Level/R4").GetComponent<RoomManager>().usedKey = true;
            Destroy(other.gameObject);
        }
    }
}
