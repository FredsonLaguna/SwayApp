using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    public bool begin = false;
    public float countDownAmnt = 2.0f;
    float savedTime;
    public float countDown;
    bool startRecording;
    public int numOfRepetitions = 3;

    // Update is called once per frame
    void Update()
    {
        if (begin)
        {
            savedTime = Time.realtimeSinceStartup;
            begin = false;
            startRecording = true;
        }

        if (startRecording)
        {
            DataRecorder.Instance.BeginRecording("MoveEvent");
            DataRecorder.Instance.BeginRecording("ScaleEvent");
            DataRecorder.Instance.BeginRecording("MoveScaleEvent");
            countDown = savedTime + countDownAmnt - Time.realtimeSinceStartup;
            if (countDown < 0)
            {
                startRecording = false;
                DataRecorder.Instance.FinishRecording("MoveEvent");
                DataRecorder.Instance.FinishRecording("ScaleEvent");
                DataRecorder.Instance.FinishRecording("MoveScaleEvent");
                numOfRepetitions--;
                if (numOfRepetitions > 0)
                {
                    begin = true;
                }
            }
        }
    }
}
