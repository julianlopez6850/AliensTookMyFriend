using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTriggerManager : MonoBehaviour
{
    public Camera mainCam;    
    public Transform roomCameraPos;

    private bool roomJustLoaded;

    private bool triggerEnterFlag = true;

    private bool closeBossEntrance = false;
    
    private float camSmoothTime = 0.3f;

    Vector3 camVel = Vector3.zero;

    public enum roomIDs
    {
        R0,
        R1,
        R2,
        R3,
        R4,
        RK,
        R5,
        RB,
    }

    public roomIDs thisRoomID;

    private Vector3 bossDoorVel = Vector3.zero;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(closeBossEntrance == true)
        {
            GameObject bossEntranceDoor = GameObject.Find("/Level/RB/RBDoor");

            bossEntranceDoor.transform.localScale = Vector3.SmoothDamp(bossEntranceDoor.transform.localScale, new Vector3(1f, 1.5f, 1f), ref bossDoorVel, 0.2f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && triggerEnterFlag == true)
        {
            triggerEnterFlag = false;
            GetComponentInParent<RoomManager>().beginLoadingThisRoom = true;

            if(thisRoomID == roomIDs.RB)
                closeBossEntrance = true;
        }
    }

    private void OnTriggerStay(Collider other) {
        if(other.tag == "Player")
        {
            //Debug.Log("Colliding");
            mainCam.transform.position = Vector3.SmoothDamp(mainCam.transform.position, roomCameraPos.position, ref camVel, camSmoothTime);
            roomJustLoaded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            mainCam.transform.position = Vector3.SmoothDamp(mainCam.transform.position, roomCameraPos.position, ref camVel, camSmoothTime);
            roomJustLoaded = false;
        }
    }

    IEnumerator undoRoomJustLoaded()
    {
        yield return new WaitForSeconds(0.5f);
        roomJustLoaded = false;
    }    

}
