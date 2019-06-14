using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/*
 * author: Fredson Laguna
 * Date: 2.15.2019
 * class: TrackerCalibrator
 * purpose: Used to find the average rotation of the vive tracker when the user is standing still
 * How to use: attach this script to a vive tracker game object
*/

public class TrackerCalibrator : MonoBehaviour
{
    [Header("Controls how long calibration lasts for")]
    public float duration = 10.0f;          // how long calibration of tracker will last for
    float countDown;                        // how long until the next rotation is logged
    public int iterations = 20;             // number of times the tracker's rotational coordinates are recorded during calibration
    [SerializeField]
    Vector3[] rotationArr;                  // stores the trackers rotation                
    int index = 0;                          // used to iterate through rotationArr
    bool isRunning = false,
         recordTime = false;

    [SerializeField]
    Vector3 averageRotation = Vector3.zero; // is the average of all the rotations stored in rotationArr

    [SerializeField]
    float savedTime,        // stores the current Time.time
          timer;            // when timer reaches 0, log the current rotation of the tracker

    public UnityEvent onCalibrationStart;
    public UnityEvent onCalibrationFinished;

    void Start()
    {
        rotationArr = new Vector3[iterations];      // initialize array  
    }

    void Update()
    {
        if (recordTime)             // updates savedTime so that it can be used for a countdown
        {
            savedTime = Time.time;
            recordTime = false;
        }

        if (isRunning)
        {
            timer = savedTime + countDown - Time.time;  // if the timer is 0 log the current rotation of the vive tracker
            if (timer <= 0)
            {
                rotationArr[index] = transform.InspectorNegativeEulerAngles();
                isRunning = false;
            }
        }       
    }

    /*
     * This method is in charge of signaling when to start the timer
     * fields used: countDown, duration, iterations, index, recordTime, isRunning, averageRotation
    */
    public IEnumerator Calibrate()
    {
        onCalibrationStart.Invoke();
        countDown = duration / iterations;
        for (int i = 0; i < iterations; i++) // a loop to signal when to start the timer
        {
            index = i;
            recordTime = true;
            isRunning = true;
            while (isRunning)
            {
                yield return null;
            }
        }

        List<Vector3> temp = rotationArr.ToList();
        averageRotation = new Vector3(temp.Average(x => x.x), temp.Average(y => y.y), temp.Average(z => z.z)); // go through all of the stored rotations and get the average rotation   
        onCalibrationFinished.Invoke();
    }

    /*
     * This method is useful when you want to find the amount the tracker has rotated relative to the average rotation,
     * essentially it is used so that you can know how much the person has swayed from their neutral standing position
     * fields used: averageRotation
    */
    public Vector3 SubtractFromAverage(Vector3 rotation)
    {
        Vector3 result = (averageRotation.Abs() - rotation.Abs()).Abs();

        if (rotation.x < averageRotation.x)
            result.x *= -1;
        if (rotation.y < averageRotation.y)
            result.y *= -1;
        if (rotation.z < averageRotation.z)
            result.z *= -1;

        return result;
    }

}
