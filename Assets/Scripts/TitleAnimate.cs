using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleAnimate : MonoBehaviour
{
    public float rotateSpeed = 35f;
    public bool allowMoving = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.down * rotateSpeed * Time.deltaTime);

        if(allowMoving)
            transform.Translate(Vector3.forward * Mathf.Cos(Time.deltaTime) * Time.deltaTime);
    }
}
