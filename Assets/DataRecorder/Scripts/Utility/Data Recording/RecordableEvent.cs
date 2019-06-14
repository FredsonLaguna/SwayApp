using System.Collections.Generic;
using UnityEngine;

/*
 * author: Fredson Laguna
 * Date: 2.4.2019
 * class: RecordableEvent
 * purpose: allows any information about the object that the child class of this class is attached to be recorded
 * How to use: Create a child class that inherits from RecordableEvent and override abstract method AddDataBlock, attach child class to game object.
*/

public abstract class RecordableEvent : MonoBehaviour
{
    [Header("Name that will be used when recording data. Will use GO name if left empty")]
    public string objName = string.Empty; // name that will be used when recording data. Useful when multiple game objects have the same name.
    [SerializeField]
    List<DataBlock> dataList = new List<DataBlock>();   // a list that stores all data recorded from a single repetition of an event.
    public List<DataBlock> DataList                     
    {
        get { return dataList; }
        private set { dataList = value; }
    }

    [HideInInspector]
    public int eventsIndex = 0;                         // used to store the index of the selected event for the events list drop down in the inspector
    [SerializeField]
    private string selectedEvent;                       // the name of the selected event
    public string SelectedEvent
    { 
       get
       {
            return selectedEvent;
       }
       set
       {
            selectedEvent = value;
       }
    }

    public bool canRecordData = false;                         // if true, begin recording data

    static int totalCount = 0;                          // total number of recordable objects  
    [Header("recordable object ID -- DO NOT CHANGE")]   // left this exposed in the inspector for debugging purposes only
    [SerializeField]
    private int id = -1;                                // unique id's for each recordable object
    public int Id
    {
        get { return id; }
        private set { id = value; }
    }                                    // can read this object's unique id through this property

    [SerializeField]
    private int numOfTimesRecorded = 0;                 // checks to see how many times data was recorded per second
    private float time = 0f;

    protected virtual void OnEnable()
    {

        DataRecorder.OnRecordDataFinished += FinishRecording;   // if this event gets triggered, execute FinishRecording
        DataRecorder.OnRecordData += StartRecording;            // if this event gets triggred, execute StartRecording
    }

    protected virtual void OnDisable()
    {

        DataRecorder.OnRecordDataFinished -= FinishRecording;    // unsubscribe to these events if this game object is disabled/destroyed
        DataRecorder.OnRecordData -= StartRecording;
    }

    protected virtual void Awake()
    {


        SetID();    // assign object a unique id

        if (string.IsNullOrEmpty(objName))        // if the obj name text field in the inspector is left blank, use the game object's name instead
            objName = this.gameObject.name;
    }

    protected virtual void Start()
    {
    }

    //protected virtual IEnumerator Start()
    //{  
    //    while (true)
    //    {
    //        if (canRecordData)
    //        {
    //            AddDataBlock();
    //            numOfTimesRecorded++;
    //            yield return new WaitForSeconds(  1 / (float)DataRecorder.Instance.timesToRecordPerSec);
    //        }
    //        else
    //            yield return null;
    //    }
    //}

    protected virtual void Update()
    {
        if (canRecordData)                  // create and add a new block of data each frame
        {
            if (Time.time >= time)
            {
                AddDataBlock();
                numOfTimesRecorded++;
                time += DataRecorder.Instance.recordCountDown;
                if (Time.time >= time && numOfTimesRecorded <= DataRecorder.Instance.TimesToRecordPerSec)
                    time = Time.time + DataRecorder.Instance.recordCountDown;

            }   
        }
       
    }

    // a struct that contains several pieces of information about a recorded object
    [System.Serializable]
    public struct DataBlock
    {
        public string objName;      // name of the recorded game object
        public string dataName;     // name of the data we want recorded (ex: rotation, color, scale, etc.)
        public string dataValue;    // the string representation of the actual data being recorded
        public float timeStamp;     // the time the data was recorded
        public int objID;           // the recorded object's id
        public DataBlock(string dataName, string dataValue, string objName, int objID)
        {
            this.objName = objName;
            this.dataName = dataName;
            this.dataValue = dataValue;
            this.timeStamp = Time.realtimeSinceStartup;
            this.objID = objID;
        }
    }

    /*
     * used to create a Datablock struct filled with recorded information and then saved into the data list
     * this method MUST BE CALLED INSIDE AddDataBlock()
     * inputs: a string representing the name of the game object being recorded
     *         a generic value of type T that represents the actual data value being recorded
     * fields used: objName
    */
    protected virtual void AddDataBlock<T>(string dataName, T value)
    {
        DataBlock db = new DataBlock(dataName, value.ToString(), this.objName, this.Id);
        //Debug.Log("db: " + value.ToString());
        DataList.Add(db);
    }

    /*
     * any class that inherits from this class must define this method
     * The purpose of this method is to call AddDataBlock(string dataName, T value) and define what type T will be, this will allow any type to be recorded as data
    */
    public abstract void AddDataBlock();

    /*
     * when an event begins, the name of the event gets passed as an argument and this method will check if the event that began is the event that this object is subscribed/listening to and begin recording data
     * inputs: a string representing the name of the event that has begun
     * fields used: selectedEvent, canRecordData
    */
    private void StartRecording(string eventName)
    {
        //Debug.Log("eventName:" + eventName);
        //Debug.Log("SelectedEvent:" + SelectedEvent);
        if (SelectedEvent == eventName)
        {
            canRecordData = true;
        }
        //else
        //    Debug.Log("event name and selected event do not match");

    }

    /*
     * when an event ends, the name of the event gets passed as an argument and this method will check if the event that ended is the event that this object is subscribed/listening to
     * and will clear the list of all recorded data, since at this point, all data has been passed into the DataRecorder class
     * inputs: a string representing the name of the event that has ended
     * fields used: selectedEvent, canRecordData, DataList, numOfTimesRecorded
    */
    private void FinishRecording(string eventName)
    {
        if (selectedEvent == eventName)
        {
            canRecordData = false;
            numOfTimesRecorded = 0;
            DataList.Clear();
        }
    }

    /*
     * used to assign a unique ID to the recordable object
     * fields used: totalCount, Id;
    */
    private void SetID()
    {
        if (Id < 0) // an Id value of -1 means that the object has not yet been assigned an Id
        {
            totalCount++;
            Id = totalCount;
        }
    }

   

}

