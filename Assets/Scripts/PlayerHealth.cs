using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public GameObject player;

    public Slider healthBar;

    public int maxHealth = 100;
    public int currHealth;

    // Start is called before the first frame update
    void Start()
    {
        currHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("current Health = " + currHealth);
        if(currHealth <= 0)
            PlayerDeath();

        healthBar.value = UpdateHealthBar();
    }

    void PlayerDeath()
    {

    }


    float UpdateHealthBar()
    {
        // return the percentage of health the player has left
        return (currHealth * 1.0f) / (maxHealth * 1.0f);
    }
}
