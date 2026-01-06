using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerUpManager : MonoBehaviour
{

    //array of powerup prefabs
    public GameObject[] powerUpFabs;
    //array of spawnpoints of powerups
    public Transform[] powerUpSpawnPoints;


    //start time
    private float time = 0f;
    //wait time
    public float waitPeriod = 43f;

    // Start is called before the first frame update
    void Start()
    {
        //iterates through spawnpoints 
        foreach (Transform t in powerUpSpawnPoints)
        {

            // creates a copy of a powerup, randomally chosen from the array of powerup prefabs.
            GameObject powerUp = Instantiate(powerUpFabs[Random.Range(0, powerUpFabs.Length)]);
            powerUp.transform.position = t.position; // settings the position and rotation of powerups to spawnpoints.
            powerUp.transform.rotation = t.rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // incrementing time every frame.
        time += Time.deltaTime;

        //once enough time has passed, a powerup will spawn.
        if (time >= waitPeriod)
        {
            time = 0f; // timer is reset.

            // creates a copy of a powerup, randomally chosen from the array of powerup prefabs.
            foreach (Transform t in powerUpSpawnPoints)
            {
                GameObject powerUp = Instantiate(powerUpFabs[Random.Range(0, powerUpFabs.Length)]);
                powerUp.transform.position = t.position; // settings the position and rotation of powerups to spawnpoints.
                powerUp.transform.rotation = t.rotation;
            }
        }
    }
}
