using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    private AudioSource finishSound;
    private bool finished = false;

    private FirebaseManager firebaseManager;
    private GameData gameData;

    private void Start()
    {
        finishSound = GetComponent<AudioSource>();
        firebaseManager = FindObjectOfType<FirebaseManager>();
        gameData = FindObjectOfType<GameData>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player" && !finished)
        {
            finishSound.Play();
            finished = true;
            Invoke("CompleteLevel", 2f);

            if (firebaseManager != null)
            {
                firebaseManager.IncrementDeathCount();
            }
        }
    }

    private void CompleteLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
