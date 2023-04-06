using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
using System.Diagnostics;

public class FirebaseManager : MonoBehaviour
{
    public FirebaseAuth auth;
    public FirebaseUser user;

    public TMP_InputField usernameLoginField;
    public TMP_InputField passwordLoginField;

    public TMP_InputField usernameRegisterField;
    public TMP_InputField passwordRegisterField;

    private DatabaseReference databaseReference;

    private GameData gameData;

    private string loggedInUserId; // Add this line


    private void Start()
    {
        gameData = FindObjectOfType<GameData>();

        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                UnityEngine.Debug.Log("worked i think");
                FirebaseApp app = FirebaseApp.DefaultInstance;
                app.Options.DatabaseUrl = new System.Uri("https://your-firebase-project-url.firebaseio.com/");
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                UnityEngine.Debug.Log("def worked");
            }
            else
            {
                UnityEngine.Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    public async Task<bool> Login(string username, string password)
    {
        bool success = false;
        UnityEngine.Debug.Log("Username: " + username);
        UnityEngine.Debug.Log("Password: " + password);

        // Query the Accounts node in the database
        var snapshot = await databaseReference.Child("Accounts").GetValueAsync();

        if (snapshot != null && snapshot.Exists)
        {
            // Loop through each child node under the Accounts node
            foreach (var childSnapshot in snapshot.Children)
            {
                // Get the user name and password values for this child node
                var childData = childSnapshot.Value as Dictionary<string, object>;
                string dbUsername = childData["Username"].ToString();
                string dbPassword = childData["Password"].ToString();

                // If the username and password match, log in the user
                if (dbUsername == username && dbPassword == password)
                {
                    UnityEngine.Debug.Log("Logged in");
                    success = true;
                    loggedInUserId = childSnapshot.Key; // Store the account key (Player1, Player2, etc.)
                    break;
                }
            }
        }

        return success;
    }

    public async Task<int> GetDeathCount()
    {
        int deathCount = 0;

        if (loggedInUserId != null)
        {
            var snapshot = await databaseReference.Child("Accounts").Child(loggedInUserId).Child("Death").GetValueAsync();
            if (snapshot != null && snapshot.Exists)
            {
                deathCount = int.Parse(snapshot.Value.ToString());
                UnityEngine.Debug.Log("Death count from database: " + deathCount); // Add this line
            }
        }

        return deathCount;
    }

    public void OnLoginButtonClicked()
    {
        string username = usernameLoginField.text;
        string password = passwordLoginField.text;

        // Try to log in with the given username and password
        Login(username, password).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                UnityEngine.Debug.LogError("Failed to read account data from Firebase database");
                UnityEngine.Debug.LogError("Exception: " + task.Exception.ToString());
            }
            else if (task.IsCompleted)
            {
                bool success = task.Result;

                if (success)
                {
                    UnityEngine.Debug.Log("Login successful for player " + username);

                    // Get and display the death count after successful login
                    GetDeathCount().ContinueWith(deathCountTask =>
                    {
                        if (deathCountTask.IsCompleted)
                        {
                            int deathCount = deathCountTask.Result;
                            UnityEngine.Debug.Log("Logged in user's death count: " + deathCount);
                        }
                    });
                }
                else
                {
                    UnityEngine.Debug.Log("Login failed");
                }
            }
        });
    }

    public async Task<bool> Register(string username, string password)
    {
        bool success = false;

        // Get the next available account number
        var accountsSnapshot = await databaseReference.Child("Accounts").GetValueAsync();
        long nextAccountNumber = accountsSnapshot.ChildrenCount + 1;
        string newAccountKey = "Player" + nextAccountNumber;

        // Check if the username already exists in the database
        bool usernameExists = false;
        foreach (var childSnapshot in accountsSnapshot.Children)
        {
            var childData = childSnapshot.Value as Dictionary<string, object>;
            string dbUsername = childData["Username"].ToString();

            if (dbUsername == username)
            {
                usernameExists = true;
                break;
            }
        }

        if (!usernameExists)
        {
            // Create the new account
            Dictionary<string, object> newAccountData = new Dictionary<string, object>()
        {
            { "Username", username },
            { "Password", password },
            { "Death", "0" }
        };

            // Write the new account data to the Firebase database
            var writeTask = databaseReference.Child("Accounts").Child(newAccountKey).SetValueAsync(newAccountData);
            await writeTask;

            if (writeTask.IsCompleted)
            {
                success = true;
                UnityEngine.Debug.Log("Account created successfully for player " + username);
            }
            else
            {
                UnityEngine.Debug.LogError("Failed to create account in Firebase database");
            }
        }
        else
        {
            UnityEngine.Debug.Log("Username already exists. Please choose a different username.");
        }

        return success;
    }

    public void OnRegisterButtonClicked()
    {
        string username = usernameRegisterField.text;
        string password = passwordRegisterField.text;

        Register(username, password).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                UnityEngine.Debug.LogError("Failed to create account in Firebase database");
            }
            else if (task.IsCompleted)
            {
                bool success = task.Result;

                if (success)
                {
                    UnityEngine.Debug.Log("Account created successfully for player " + username);
                }
                else
                {
                    UnityEngine.Debug.Log("Account creation failed");
                }
            }
        });
    }

    public async void IncrementDeathCount()
    {
        if (loggedInUserId != null)
        {
            // Get the current death count from the database
            var snapshot = await databaseReference.Child("Accounts").Child(loggedInUserId).GetValueAsync();
            if (snapshot != null && snapshot.Exists)
            {
                // Parse the snapshot data into a Dictionary
                var accountData = snapshot.Value as Dictionary<string, object>;

                // Get the current death count from the Dictionary
                int currentDeathCount = int.Parse(accountData["Death"].ToString());

                // Increment the death count by 1
                currentDeathCount++;

                // Update the Death key in the Dictionary
                accountData["Death"] = currentDeathCount;

                // Update the account data in the database
                await databaseReference.Child("Accounts").Child(loggedInUserId).SetValueAsync(accountData);

                // Get the new death count from the database
                var newSnapshot = await databaseReference.Child("Accounts").Child(loggedInUserId).Child("Death").GetValueAsync();
                if (newSnapshot != null && newSnapshot.Exists)
                {
                    int deathCountFromDatabase = int.Parse(newSnapshot.Value.ToString());
                    UnityEngine.Debug.Log("Death count from database: " + deathCountFromDatabase); // Add this line
                    if (gameData != null)
                    {
                        gameData.SetDeathCount(deathCountFromDatabase);
                    }
                }
            }
        }
    }
}