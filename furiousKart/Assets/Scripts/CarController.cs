using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
//using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using UnityEngine;

using Photon.Realtime;
using Photon.Pun;
using Unity.VisualScripting;
using System;



public class CarController : MonoBehaviourPunCallbacks
{

    //time shite
    private float currentFrame;
    private float nextFrameComparison;

    //declarations
    private Rigidbody rb;
    public WheelColliders colliders;
    public WheelMeshes wheelMeshes;

    public Collider kartCollider;

    // wheel particle variable and objects.
    public WheelSmoke wheelSmoke;
    public GameObject SmokePrefab;

    //Three float variables to hold keyboard input values between -1 and 1
    public float throttleInput;
    public float steeringInput;
    public float brakeInput;


    //Variable controlling how strong much power the engine will provide to the rear wheels.
    
    public float engineStrength = 1200f;
    public float brakeStrength;

    //generated values
    public float slipAngle;
    public float movingDirection;

   
    private float speed;

    WheelFrictionCurve wfcRearS = new WheelFrictionCurve();
    WheelFrictionCurve wfcRearF = new WheelFrictionCurve();
    WheelFrictionCurve wfcFrontF = new WheelFrictionCurve();
    WheelFrictionCurve wfcFrontS = new WheelFrictionCurve();

    // checkpoint variables
    public int nextCheckpoint;
    public int currentLap;

    public canvasManager canvasScreen;  // reference to canvas

    public float currentLapTime;
    public float bestLapTime;

    public bool slipDown = false;


    //Animation curve to change the steering sensitivity based on speed to allow for smoother wheel movements.
    //dynamically changing it
    public AnimationCurve steeringCurve;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        //InstantiateSmoke();
        photonView.RPC("syncSmoke", RpcTarget.All,gameObject.GetComponent<PhotonView>().ViewID);
        resetWheelValues();
    }

    [PunRPC]
    void syncSmoke(int id)
    {
        CarController kartController = PhotonView.Find(id).gameObject.GetComponent<CarController>();
        kartController.InstantiateSmoke();
    }
    //function to copy smoke onto wheels.
    void InstantiateSmoke()
    {
        //copies smoke prefab to position of each wheel.
        wheelSmoke.FRwheel = Instantiate(SmokePrefab, colliders.FRwheel.transform.position, Quaternion.identity, colliders.FRwheel.transform)
    .GetComponent < ParticleSystem>();
        wheelSmoke.FLwheel = Instantiate(SmokePrefab, colliders.FLwheel.transform.position, Quaternion.identity, colliders.FLwheel.transform)
    .GetComponent<ParticleSystem>();
        wheelSmoke.RRwheel = Instantiate(SmokePrefab, colliders.RRwheel.transform.position, Quaternion.identity, colliders.RRwheel.transform)
    .GetComponent<ParticleSystem>();
        wheelSmoke.RLwheel = Instantiate(SmokePrefab, colliders.RLwheel.transform.position, Quaternion.identity, colliders.RLwheel.transform)
    .GetComponent<ParticleSystem>();
    }


    void resetWheelValues()
    {
        rearWheelColliderValues();
        frontWheelColliderValues();

        /*Quality of life; when importing new 
         * vehicles cars will have the same wheel collider
         * values/saves time when adding new cars instead of manually changing values
         * when powerups change wheelstiffness,
         * it will st everything else to 0, the values changed will need
         * to be reset.
        */

    }
    void rearWheelColliderValues()
    {
        wfcRearS.extremumSlip = 0.2f;
        wfcRearS.extremumValue = 0.5f;
        wfcRearS.asymptoteSlip = 0.3f;
        wfcRearS.asymptoteValue = 0.4f;
        wfcRearS.stiffness = 1f;
        //Declaring RearWheelSideways Values
        colliders.RLwheel.sidewaysFriction = wfcRearS;
        colliders.RRwheel.sidewaysFriction = wfcRearS;
        //applying to rearwheels as sideways friction


        wfcRearF.extremumSlip = 0.4f;
        wfcRearF.extremumValue = 1f;
        wfcRearF.asymptoteSlip = 0.8f;
        wfcRearF.asymptoteValue = 0.5f;
        wfcRearF.stiffness = 1f;
        //Declaring RearWheelForward Values

        colliders.RLwheel.forwardFriction = wfcRearF;
        colliders.RRwheel.forwardFriction = wfcRearF;
        //applying to rearwheels as forward friction

    }
    void frontWheelColliderValues()
    {
        wfcFrontF.extremumSlip = 0.4f;
        wfcFrontF.extremumValue = 1f;
        wfcFrontF.asymptoteSlip = 0.8f;
        wfcFrontF.asymptoteValue = 0.5f;
        wfcFrontF.stiffness = 1.35f;
        //Declaring FrontWheelForward values

        colliders.FLwheel.forwardFriction = wfcFrontF;
        colliders.FRwheel.forwardFriction = wfcFrontF;
        //Applying FrontWheels as forward Friction

        wfcFrontS.extremumSlip = 0.2f;
        wfcFrontS.extremumValue = 1f;
        wfcFrontS.asymptoteSlip = 0.5f;
        wfcFrontS.asymptoteValue = 0.75f;
        wfcFrontS.stiffness = 1.3f;
        //Declaring FrontWheelSideways values 

        colliders.FLwheel.sidewaysFriction = wfcFrontS;
        colliders.FRwheel.sidewaysFriction = wfcFrontS;
        //Applying FrontWheels as sideways Friction
    }


    // Update is called once per frame
    void Update()
    {
        if (!SpawnCars.instance.disableCarMovement) // If the startingRace is false, meaning the race has started, the cars are allowed to drive.
        {

            currentLapTime += Time.deltaTime; // every frame call, time will go up. 

            var timeSpan = System.TimeSpan.FromSeconds(currentLapTime); // converts currentLapTime to timespan
            canvasScreen.currentLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds); // formatting time and setting it on the canvas

            speed = rb.velocity.magnitude;

            Vector3 velocityV = rb.velocity;

            checkInput();
            applyPower();
            applySteering();
            applyBrake();
            CheckParticles();
            UpdateWheelPos();
            bannanaSlip();       
        }

        void checkInput()
        {
            //checks for keyboard inputs between -1 and 1
            throttleInput = Input.GetAxis("Vertical"); // checks W and S or Up arrow and Down arrow
            steeringInput = Input.GetAxis("Horizontal");// checks A or D or right arrow and left arrow

            //Drift angle stuff
            slipAngle = Vector3.Angle(transform.forward, rb.velocity - transform.forward);

            /**Vector3.dot calcultes the product of two vectors;
            in this case it takes transform.forward, the dirction the object is facing
            and rb.velocity, representing the speed and dirction of the rigidbody's movement
            By calculating the product, it tells us how the velocity -
            is aligned to the dirction of the rigid body
            If movingDirection is positive, we know the object moves in same-
            direction that the rigidbody is facing
            If negative, we know its going against the way the-
            rigidbody is facing and so its movng backward.
            **/
            movingDirection = Vector3.Dot(transform.forward, rb.velocity);

            //Braking checks
            if (movingDirection < -0.5f && throttleInput > 0) //If movingDirection is negative (moving backward) & a positive throttle input is detected
            {
                //The scalar value for throttle input is set equal to the brake input
                //So when moving backward, 'W' a positive throttle input, acts as the brake and will slow the vehicle down
                brakeInput = Mathf.Abs(throttleInput);
            }
            else if (movingDirection > 0.5f && throttleInput < 0) //If moving forward and a negative throttle input is detected
            {

                //the scalar value of throttle input is set to equal break input
                //so when moving forward, 'S' a negative throttle input (or reverse) acts as a brake and will slow the vehicle down.
                brakeInput = Mathf.Abs(throttleInput);
            }
            else
            {
                //If neither of the above cases are met, no braking should occur and so brakeInput value is reset to 0.
                //as this code runs in update, it will be checking every frame.
                brakeInput = 0;
            }


            if(Input.GetKeyDown(KeyCode.E)) // checks for input E
            {
                Debug.Log("helo");
                resetKart(); // calls function resetKart;
            }
        }
    }
    void resetKart()
    {
        int lastCheckpoint = nextCheckpoint - 1;  // getting the last Checkpoint
        if (lastCheckpoint < 0) // if the next checkpoint is zero in the array, the value will be negative.
        {
            lastCheckpoint = checkpointManager.instance.checkPointArray.Length - 1; // this will get the value of the last checkpoint.
        }
        transform.position = checkpointManager.instance.checkPointArray[lastCheckpoint].transform.position; // moving the position of the model and rigidbody to the checkpoint location. 
        rb.transform.position = transform.position = checkpointManager.instance.checkPointArray[lastCheckpoint].transform.position;

        rb.velocity = Vector3.zero; // sets velocity to zero to account for momentum that the car may have when reset.

    }
    /*
    void bannanaSlip()
    {
        currentFrame = Time.frameCount;
        nextFrameComparison = currentFrame + 250;
        wfcRearS.stiffness = 1f;
     
        colliders.RLwheel.sidewaysFriction = wfcRearS;
        colliders.RRwheel.sidewaysFriction = wfcRearS;

    }
    // write about how you had to instanstiate wheelcollider settings
    void bannanaCount()
    {
        if (Time.frameCount > nextFrameComparison)
        {
            wfcRearS.stiffness = 1f;
            colliders.RLwheel.sidewaysFriction = wfcRearS;
            colliders.RRwheel.sidewaysFriction = wfcRearS;
        }
    }

    */

    void bannanaSlip()
    {
        if(slipDown)
        {
            //setting rear wheel friction really low, so kart is hard to control
            wfcRearS.stiffness = 0.5f;

            colliders.RLwheel.sidewaysFriction = wfcRearS;
            colliders.RRwheel.sidewaysFriction = wfcRearS;
        }
        else
        {
            // once off, values for wheels reset.
            resetWheelValues();
        }
    }
    void applyBrake()
    {
        /*
         * brakeTorque is a propery of WheelCollider
         * the value of brakeTorque is updated for each collider when a brakeInput is generated
         * multiplied by a constant and then a wheel constant
         * allowing the car to slow down and then sto
         */

        colliders.FRwheel.brakeTorque = brakeInput * brakeStrength * 0.7f;
        colliders.FLwheel.brakeTorque = brakeInput * brakeStrength * 0.7f;
        colliders.RRwheel.brakeTorque = brakeInput * brakeStrength * 0.3f;
        colliders.RLwheel.brakeTorque = brakeInput * brakeStrength * 0.3f;

    }

    void applyPower() 
    {
        //motorTorque is a property of WheelCollider
        //value of motorTorque updated for each collider when a throttleInput is detected
        //times by the constant engineStrength variable 
        //allowing the rearwheels to get power and move the vehicle

        colliders.RLwheel.motorTorque = engineStrength * throttleInput;
        colliders.RRwheel.motorTorque = engineStrength * throttleInput;
    }

    void applySteering()
    {


        //generates steeringAngle based on the speed and steeringInput
        //steeringcurve.evaluate(speed) will dynamically adjust steering sensitivity
        //based ont the speed of the rigidbody.
        //as steeringInput is a range from -1 and 1, the angles will adjust for the ranges between total left and right.

        float steeringAngle = steeringInput * steeringCurve.Evaluate(speed);


        if (movingDirection>0 && colliders.RLwheel.motorTorque>0) //Validates movement to check if vehicle is going forward and drifting.
        {
            //using the singed angle, we can work out the angle measurement of both magnitude of rotation, and also direction.
            //tells us how much the car is deviating from its intended direction then adds it to the steering angle to counter it.
            steeringAngle += Vector3.SignedAngle(transform.forward, rb.velocity + transform.forward, Vector3.up);
            //keeps the steering angle between -90 and 90 to prevent overcorrections.
            steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);
        }

        //WheelCollider has the property of steerAngle
        //steerAngle for each collider on the front two wheels is set to the calculated steeringAngle 
        //when a steeringInput is detected

        colliders.FRwheel.steerAngle = steeringAngle;
        colliders.FLwheel.steerAngle = steeringAngle;
    }


    //checks for drift and applies particles.
    void CheckParticles()
    {
        //array to see if wheels hit ground.
        WheelHit[] wheelHits = new WheelHit[4];
        colliders.FRwheel.GetGroundHit(out wheelHits[0]);
        colliders.FLwheel.GetGroundHit(out wheelHits[1]);

        colliders.RRwheel.GetGroundHit(out wheelHits[2]);
        colliders.RLwheel.GetGroundHit(out wheelHits[3]);

        //how much the wheels are allowed to slip before particles are applied.
        float slipAllowance = 1.5f;


        // if the wheels touching grounds slip is larger than the slip allowence, the drift particles are played.
        if ((Mathf.Abs(wheelHits[0].sidewaysSlip) + Mathf.Abs(wheelHits[0].forwardSlip) > slipAllowance))
        {
            wheelSmoke.FRwheel.Play();
        }
        else 
        {
            wheelSmoke.FRwheel.Stop(); // otherwise, it is stopped. 
        }

        //repeated for each wheel.

        if ((Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipAllowance))
        {
            wheelSmoke.FLwheel.Play();
        }
        else
        {
            wheelSmoke.FLwheel.Stop();
        }


        if ((Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipAllowance))
        {
            wheelSmoke.RRwheel.Play();
        }
        else
        {
            wheelSmoke.RRwheel.Stop();
        }


        if ((Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipAllowance))
        {
            wheelSmoke.RLwheel.Play();
        }
        else
        {
            wheelSmoke.RLwheel.Stop();
        }
    }
    void UpdateWheelPos()
    {
        //using function to move wheels to given position
        UpdateWheel(colliders.FRwheel, wheelMeshes.FRwheel);
        UpdateWheel(colliders.FLwheel, wheelMeshes.FLwheel);
        UpdateWheel(colliders.RRwheel, wheelMeshes.RRwheel);
        UpdateWheel(colliders.RLwheel, wheelMeshes.RLwheel);
    }
    void UpdateWheel(WheelCollider wheelC, MeshRenderer wheelM) //Wheelcollider wheelC represents the physics wheel,
                                                                //meshrender reprents the visual model of the wheel. 
    {
        Quaternion quat; //rotation of the wheel
        Vector3 position;  //position of the wheel 
        wheelC.GetWorldPose(out position, out quat); //unity function to get rotation and position of wheels
        wheelM.transform.position = position;  //matches wheels to position and rotation according to getwoldpos.
        wheelM.transform.rotation = quat;      //matches the wheelcollider rotation to the visual model of the wheel. 
    }


    public void checkPointHit(int checkPointNumber) // takes an integer parameter as the checkPointNumber
    {
        Debug.Log(checkPointNumber);
        if(checkPointNumber == nextCheckpoint) // checking whether next checkpoint is the expected one
        {
            nextCheckpoint++; // increment nextCheckpoint
            if(nextCheckpoint == checkpointManager.instance.checkPointArray.Length) // checking if the next checkpoint is the end.
            {
                nextCheckpoint = 0; // reset nextCheckpoint therefore lap completed
                lapComplete();
            }
        }
    }

    public void lapComplete()
    {
        currentLap++; // Lap incremented as all checkpoints have been hit.

        if(currentLapTime < bestLapTime || bestLapTime == 0) //  if the lap time is larger than the best time or best time is 0, best time is set equal to the current lap time.
        {
            bestLapTime = currentLapTime;
        }



        if (currentLap <= checkpointManager.instance.totalLapCount) // checking if there are still laps to be completed.
        {
            currentLapTime = 0; //  reset to zero as lap has been completed.
            var timeSpan = System.TimeSpan.FromSeconds(bestLapTime); // converts bestLapTime to timespan
            canvasScreen.bestLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds); // formatting time and setting it as bestTime on canvas

            canvasScreen.LapDisplay.text = currentLap + "/" + checkpointManager.instance.totalLapCount; // Changing text on canvas
        }
        else
        {
            SpawnCars.instance.disableCarMovement = true; // if all laps have been crossed, end race. 
            brakeInput = 1f;
            kartCollider.enabled = false;
            
        }
    }
}

//class to hold wheels, serializable field to make it editble in unity.
[System.Serializable]
public class WheelColliders //holds wheelcolliders
{
    public WheelCollider FRwheel;
    public WheelCollider FLwheel;
    public WheelCollider RRwheel;
    public WheelCollider RLwheel;
}
[System.Serializable]
public class WheelMeshes //holds wheelmeshes or the art
{
    public MeshRenderer FRwheel;
    public MeshRenderer FLwheel;
    public MeshRenderer RRwheel;
    public MeshRenderer RLwheel;
}


//class to hold particles for wheels.
[System.Serializable]
public class WheelSmoke
{
    public ParticleSystem FRwheel;
    public ParticleSystem FLwheel;
    public ParticleSystem RRwheel;
    public ParticleSystem RLwheel;
}
