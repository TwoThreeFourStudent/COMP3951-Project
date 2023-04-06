using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using System.Diagnostics;

public class PlayerLife : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    [SerializeField] private AudioSource deathSound;
    private int death_count = 0;
    private FirebaseManager firebaseManager;
    private GameData gameData; // Add this line

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        firebaseManager = FindObjectOfType<FirebaseManager>();
        gameData = FindObjectOfType<GameData>();

        if (firebaseManager != null)
        {
            firebaseManager.GetDeathCount().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    int deathCountFromDatabase = task.Result;
                    gameData.SetDeathCount(deathCountFromDatabase);
                }
            });
        }
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            Die();
        }
        if (collision.gameObject.CompareTag("MovingTerrain"))
        {
            MovingTerrain movingTerrain = collision.gameObject.GetComponent<MovingTerrain>();
            if (movingTerrain != null)
            {
                movingTerrain.MoveDown();
            }
        }
    }

    private void Die()
    {
        rb.bodyType = RigidbodyType2D.Static;
        anim.SetTrigger("death");

        if (firebaseManager != null)
        {
            firebaseManager.IncrementDeathCount();
        }

        if (gameData != null)
        {
            gameData.IncrementDeathCount();
            UnityEngine.Debug.Log("Death count: " + gameData.DeathCount);
        }
    }




    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}