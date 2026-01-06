using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Realtime;
using Photon.Pun;
using JetBrains.Annotations;




public class SpawnCars : MonoBehaviourPunCallbacks
{

    public static SpawnCars instance; // accessable from anywhere.



    private void Awake() // happens before start function. 
    {
        instance = this; // refers to object in scene.
    }

    public GameObject[] vehicleFabs; // creating an array for the kart prefabs
    public Transform[] spawnPoint; // creating an array for the spawn points



    GameObject pKart = null;

    public GameObject startRace; // reference to button. 

    int playerKart;
    public float waitSec = 0.5f;



    public bool startingRace; // boolean value if the race is starting.
    public float counterTime = 1f; // time between counts.
    private float startCounter; // starting count value; 
    public int countDown = 5; // how many counts there are.

    public bool disableCarMovement; // so cars cannot move before countDown is completed.


    // Start is called before the first frame update
    void Start()
    {
        // PhotonNetwork.AutomaticallySyncScene = true;
        playerKart = PlayerPrefs.GetInt("PlayerKart");
        int randomStartPos = Random.Range(0, spawnPoint.Length);
        Vector3 startPos = spawnPoint[randomStartPos].position;
        Quaternion startRot = spawnPoint[randomStartPos].rotation;



        startPos = spawnPoint[PhotonNetwork.CurrentRoom.PlayerCount - 1].position;
        startRot = spawnPoint[PhotonNetwork.CurrentRoom.PlayerCount - 1].rotation;

        StartCoroutine(delayInstantiation(startPos, startRot));
        disableCarMovement = true; // car movement is disabled on start.
        startRace.SetActive(false); // disable button for everyone.
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                startRace.SetActive(true); // button is enabled for the host. 
            }
        }
        else
        {
            startGame();
        }

    }


    IEnumerator delayInstantiation(Vector3 startPos, Quaternion startRot)
    {

        yield return new WaitForSeconds(waitSec);
        pKart = PhotonNetwork.Instantiate(vehicleFabs[playerKart].name, startPos, startRot, 0);
        Camera kartCamera = pKart.GetComponentInChildren<Camera>();
        pKart.GetComponent<CarController>().enabled = true;
        pKart.GetComponentInChildren<Camera>().enabled = true;
        pKart.GetComponentInChildren<Canvas>().enabled = true;

    }



    void Update()
    {
        if (startingRace)
        {
            startCounter -= Time.deltaTime; // as a second passes
            if (startCounter <= 0)
            {
                countDown--; // taking away one from countdown.
                startCounter = countDown; // start counter set to the countDown value. 

                UICountdoown.instance.countDownText.text = countDown.ToString(); // setting the UI countdown equal to the countDown value currently.


                if (countDown == 0)
                {
                    UICountdoown.instance.countDownText.text = "GO!";
                    startingRace = false; // As the countDown is now 0, the race starts. 
                    disableCarMovement = false; // car movement enabled.
                    StartCoroutine(delayGoText());

                }
            }



        }



    }

    public void BeginGame()
    {
        if (PhotonNetwork.IsMasterClient) // checks if its the host.
        {
            photonView.RPC("startGame", RpcTarget.All, null); // runs it on all connected clients
        }
    }


    [PunRPC]
    public void startGame()
    {
        startingRace = true; // countdown starts. 
        startCounter = counterTime; // start value set.
        startRace.SetActive(false); // Disable button once pressed.
        UICountdoown.instance.countDownText.text = countDown.ToString(); // setting the UI countdown equal to the countDown value currently.

    }

    IEnumerator delayGoText()
    {
        yield return new WaitForSeconds(1f); // waits one second before removing the go message.
        UICountdoown.instance.countDownText.text = "";
    }
}