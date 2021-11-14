using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public GameObject door;
    public GameObject player;

    private bool endOfGameFlag = false;

    public GameObject[] enemies;
    public Transform[] enemySpawnLocations;

    public GameObject[] spawnedEnemies;
    private int spawnedEnemyCount = 0;
    private int deadEnemyCount;

    private Vector3 scaleVel;
    private Vector3 doorScaleVel;

    public bool usedKey = false;
    public AudioClip[] clips;
    private bool playClip = true;

    private bool canPlayKeySFX = true;

    public bool beginLoadingThisRoom = false;
    private bool popInEnemies = false;
    private bool defaultRoomState = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("/Player");
    }

    // Update is called once per frame
    void Update()
    {
        // turn beginLoadingThisRoom to true in this room's trigger manager
        
        if(beginLoadingThisRoom)
        {
            beginLoadingThisRoom = false;
            //Debug.Log("Begin loading room");

            for(int i = 0; i < enemies.Length; ++i)
            {
                spawnedEnemies[spawnedEnemyCount] = Instantiate(enemies[i], enemySpawnLocations[i].transform.position, enemySpawnLocations[i].transform.rotation);
                spawnedEnemyCount++;
            }

            popInEnemies = true;

            // For testing, remove this from here later
            defaultRoomState = true;
            
        }

        if(popInEnemies)
        {
            //Debug.Log("Popping in enemies");
            StartCoroutine(turnToHostile());
        }

        if(defaultRoomState)
        {
            //Debug.Log("Room is default right now");
            //Debug.Log("Dead enemy count = " + deadEnemyCount);

            for(int i = 0; i < spawnedEnemies.Length; ++i)
            {
                if(spawnedEnemies[i] != null && spawnedEnemies[i].GetComponentInChildren<Alien>().alienState == Alien.alienStates.Dead)
                {
                    Destroy(spawnedEnemies[i]);
                    //spawnedEnemyCount--;
                    deadEnemyCount++;                                      

                    GetComponent<AudioSource>().clip = clips[1];
                    GetComponent<AudioSource>().Play();
                }
            }
            // If all enemies are in the DEAD state, remove the door

            if(deadEnemyCount >= enemies.Length)
            {
                if(door != null && GetComponentInChildren<RoomTriggerManager>().thisRoomID != RoomTriggerManager.roomIDs.R4)
                    openDoor();

                if(GetComponentInChildren<RoomTriggerManager>().thisRoomID == RoomTriggerManager.roomIDs.RB)
                {
                    if(endOfGameFlag == false)
                        GetComponent<BossRoomManager>().endBoss = true;
                    endOfGameFlag = true;

                    if(playClip == true)
                    {
                        GetComponent<AudioSource>().clip = clips[0];
                        GetComponent<AudioSource>().Play();
                    }

                    playClip = false;

                }
            }
        }

        if(usedKey == true && GetComponentInChildren<RoomTriggerManager>().thisRoomID == RoomTriggerManager.roomIDs.R4)
        {
            if(canPlayKeySFX)
            {
                canPlayKeySFX = false;
                GameObject.Find("/GameManager").GetComponent<AudioSource>().clip = GameObject.Find("/GameManager").GetComponent<MainGameManager>().enemySFX[1];
                GameObject.Find("/GameManager").GetComponent<AudioSource>().Play();
            }
            openDoor();
        }

        
    }

    void openDoor()
    {
        door.transform.localScale = Vector3.SmoothDamp(door.transform.localScale, Vector3.zero, ref doorScaleVel, 0.2f);
        GetComponent<AudioSource>().clip = clips[1];

        if(playClip == true)
            GetComponent<AudioSource>().Play();
        
        playClip = false;
    }

    IEnumerator turnToHostile()
    {
        yield return new WaitForSeconds(1.5f);

        foreach(GameObject enemy in enemies)
        {
            popInEnemies = false;
            defaultRoomState = true;
            enemy.GetComponentInChildren<Alien>().alienState = Alien.alienStates.Hostile;
        }
    }
}
