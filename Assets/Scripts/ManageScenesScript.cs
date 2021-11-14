using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManageScenesScript : MonoBehaviour
{
    public string mainScene = "SampleScene";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene(mainScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
