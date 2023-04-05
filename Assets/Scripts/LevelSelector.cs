using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] public string level;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SceneOpener()
    {
        SceneManager.LoadScene(level);
    }

}
