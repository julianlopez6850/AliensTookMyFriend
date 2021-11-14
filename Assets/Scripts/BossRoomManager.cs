using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomManager : MonoBehaviour
{
    public bool endBoss = false;
    public bool doCelebrate = false;
    public bool moveToPoint = false;

    private bool bothPlayersDone = false;

    public GameObject player;
    public GameObject friend;

    public GameObject winPos;
    public GameObject friendPos;

    private bool hitWinPos = false;

    // Start is called before the first frame update
    void Start()
    {
        //player = GameObject.Find("/Player");
        //friend = GameObject.Find("/Friend");
    }

    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.H))
            endBoss = true;*/

        if(endBoss)
        {
            friend.SetActive(true);

            player.GetComponent<PlayerMovement>().inActive = true;
            friend.GetComponent<PlayerMovement>().inActive = true;
            
            player.transform.LookAt(winPos.transform.position);
            friend.transform.LookAt(friendPos.transform.position);
            
            endBoss = false;
            moveToPoint = true;
        }

        if(moveToPoint)
        {
            if((winPos.transform.position - player.transform.position).magnitude > 0.1f)
            {
                player.GetComponent<CharacterController>().Move(player.transform.forward * Time.deltaTime * 4f);
                player.GetComponent<PlayerMovement>().astroAnimator.SetBool("Moving", true);
            }
            else
                player.GetComponent<PlayerMovement>().astroAnimator.SetBool("Moving", false);


            if((friendPos.transform.position - friend.transform.position).magnitude > 0.1f)
            {
                friend.GetComponent<CharacterController>().Move(friend.transform.forward * Time.deltaTime * 4f);
                friend.GetComponent<PlayerMovement>().astroAnimator.SetBool("Moving", true);
            }
            else
                friend.GetComponent<PlayerMovement>().astroAnimator.SetBool("Moving", false);

            if((winPos.transform.position - player.transform.position).magnitude <= 0.1f && (friendPos.transform.position - friend.transform.position).magnitude <= 0.1f)
                bothPlayersDone = true;

            if(bothPlayersDone)
            {
                moveToPoint = false;
                doCelebrate = true;
            }
        }

        if(doCelebrate)
        {
            doCelebrate = false;
            player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            friend.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            player.GetComponent<PlayerMovement>().astroAnimator.SetTrigger("Celebrate");
            friend.GetComponent<PlayerMovement>().astroAnimator.SetTrigger("Celebrate");

            GameObject.Find("/GameManager").GetComponent<MainGameManager>().gameWon = true;
        }
    }
}
