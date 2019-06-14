using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.Events;

/*
 * author: Fredson Laguna
 * Date: 2.4.2019
 * class: RecordableEvent
 * purpose: responsible for triggering events and exporting recorded data into a text file
 * How to use: attach this script into an empty game object
*/

public class DataRecorder : MonoBehaviour
{
    [Header("This controls the number of times to record data per second")]
    [SerializeField]
    int timesToRecordPerSec = 60;
    public int TimesToRecordPerSec
    {
        get { return timesToRecordPerSec; }
        set
        {
            timesToRecordPerSec = value;
            timesToRecordPerSec = Mathf.Clamp(timesToRecordPerSec, 30, 90);
            onTimesToRecordPerSecChanged.Invoke(timesToRecordPerSec);
        }
    }
    [System.Serializable]
    public class OnTimesToRecordPerSecChanged : UnityEvent<int> { };    
    public  OnTimesToRecordPerSecChanged onTimesToRecordPerSecChanged;

    public UnityEvent onDataExportingFinished; // event that fires off when data is finished exporting to text file

    public float recordCountDown { get { return (1 / (float)TimesToRecordPerSec); } }
    public string exportedFileName = "data.txt";                // the filename of the text file that the data will be exported to
    public bool exportDataOnClose = true;                       // if true, exports the recorded data when application is closed
    public bool overwriteExistingDatalog = false;               // if false, append new data to the end of existing data file
    public bool OverWriteExistingDataLog
    {
        get { return overwriteExistingDatalog; }
        set { overwriteExistingDatalog = value; }
    }
    [Header("Shows FPS for debugging purposes in the editor")]
    public bool showFPS = false;
    [Header("Displayed at the beginning of a data file")]
    public List<string> headingInfo = new List<string>();       // list contains heading info that is displayed at the beginning of the data file
    //   public bool includeTargetFrameRate = true;                  // if true, include target frame rate info in data file
    public bool includeRecordPerSec = true;                     // if true, includes the number of times data was recorded per second
    public bool includeDateTime = true;                         // if true, include date/time info in data file
    DateTime date;                                              // stores the date and time that the data was exported


    [Header("If true, includes the time of when an event starts and ends in the data file")]
    public bool includeEventTimestamp = true;

    public static DataRecorder Instance { get; private set; }   // singleton pattern; allows other classes to get access to this class' non-static public variables through its instance
    public static event Action<string> OnRecordData;            // the event that recordable objects will listen to and begin recording data when an event begins 
    public static event Action<string> OnRecordDataFinished;    // the event that recordable objects will listen to and finish recording data when an event ends

    List<RecordableEvent> recordableObjs = new List<RecordableEvent>();   // this contains all the gameobjects whose values we want recorded

    // outer list contains the individual repetitions during an experiment trial
    // inner list contains the actual data for every gameobject recorded for a single repetition
    [SerializeField]
    List<List<RecordableEvent.DataBlock>> dataBlocks2DList = new List<List<RecordableEvent.DataBlock>>();
    int dataBlockIndex = 0; // used by dataBlocks2Dlist to keep track of which position in the outer list that a new list of data can be added 

    [HideInInspector]
    public List<string> eventsList = new List<string> { "default" };    // a list containing the name of the events that objects can listen/subscribe to
    [HideInInspector]
    public List<EventInfo> eventsInfoList = new List<EventInfo>();     // a list containing information about each event repetition
    [HideInInspector]
    public List<int> eventsInfoIndex = new List<int>();                // used by EventsDropDown.cs, keeps track of drop down selections
    [HideInInspector]
    public List<string> eventsInfoDescription = new List<string>();    // used by EventsDropDown.cs, keeps track of drop down selections


    // dictionary that uses a DataBlock list as the key
    // value is the concatenation of the event name that the DataBlock list belongs to and the event repetition index (example value: someEvent2, where
    // someEvent is the event name while 2 indicates that it is the second repetition of someEvent)
    // this dictionary acts as a solution in order to keep track of which list of datablocks belong to which events if there are more than one.
    Dictionary<List<RecordableEvent.DataBlock>, string> dataBlockDict = new Dictionary<List<RecordableEvent.DataBlock>, string>();

    // uses the concatenation of the event name that the DataBlock list belongs to and the event repetition index as the key,
    // the value is the time in which the event started
    Dictionary<string, float> eventStartTimestampDict = new Dictionary<string, float>();

    // uses the concatenation of the event name that the DataBlock list belongs to and the event repetition index as the key,
    // the value is the time in which the event ended
    Dictionary<string, float> eventEndTimestampDict = new Dictionary<string, float>();

    public const string HEADINGCODE = "00 ";
    public const string RECOBJCODE = "0";
    public const string EVENTCODE = "2";

    // struct that is used to store information about an event/event repetition
    [System.Serializable]
    public struct EventInfo
    {
        public string eventName;
        public string eventInfo;
        public EventInfo(string eventName, string headerInfo)
        {
            this.eventName = eventName;
            this.eventInfo = headerInfo;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // prevent this game object from being destroyed if a new level is loaded
        }
        else
        {
            Destroy(gameObject);    // destroy this gameobject if one already exists
        }
    }

    private void Start()
    {

        // check the names of each event, throw an exception if an event name contains a numeric character
        foreach (var eventName in eventsList)
            if (eventName.Any(char.IsDigit))
                Debug.LogException(new Exception("event name " + eventName + " contains a numeric character, only use alpha characters"));

        if (eventsList.Count != eventsList.Distinct().Count())
            Debug.LogException(new Exception("there are events with the same name. Each event should have a unique name"));

        recordableObjs = GameObject.FindObjectsOfType<RecordableEvent>().ToList(); // find all recordable objects and save them in the list

        if(Application.isEditor)
            Debug.Log("# of recordable objects: " + recordableObjs.Count);


    }

    private void OnDestroy()
    {
        // user just needs to press the stop button in order to export the data into a text file.
        if (exportDataOnClose)
            ExportData();
    }


    /*
     * Used to begin an event. This method should be called inside of the class where the actual event starts.
     * inputs: a string representing the name of the event that will begin
     * fields used: includeEventTimestam;, eventsList, eventStartTimestampDict
    */
    public void BeginRecording(string eventName = "default")
    {
        if (!eventsList.Contains(eventName))
            Debug.LogError("The event name " + eventName + " does not exist");

        if (includeEventTimestamp)
        {
            // get the total number of dictionary entries that contain the given event name and increment it
            // so we can use it as part of the dictionary key for our new entry

            //int newKeyIndex = eventStartTimestampDict.Count(k => k.Key.Contains(eventName)) + 1;
            int newKeyIndex = eventStartTimestampDict.Count(k => Regex.Replace(k.Key, @"[\d-]", string.Empty) == eventName) + 1;
            eventStartTimestampDict.Add(eventName + newKeyIndex, Time.realtimeSinceStartup.ToMilliseconds());
        }

        OnRecordData?.Invoke(eventName);    // if there are game objects listening to this event, invoke it
    }

    /*
     * Used to end an event. This method should be called inside of the class where the actual event ends.
     * This method calls the UpdateDataBlocksList method in order save the data recorded during the event.
     * inputs: a string representing the name of the event that will end
     * fields used: includeEventTimestamp, eventsList, eventEndTimestampDict
    */
    public void FinishRecording(string eventName = "default")
    {
        Debug.Log(this.gameObject.name +  " finished recording");

        if (!eventsList.Contains(eventName))
            Debug.LogError("The event name " + eventName + " does not exist");

        UpdateDataBlocksList(eventName);
        OnRecordDataFinished?.Invoke(eventName);
        if (includeEventTimestamp)
        {
            // get the total number of dictionary entries that contain the given event name and increment it
            // so we can use it as part of the dictionary key for our new entry
            int newKeyIndex = eventEndTimestampDict.Count(k => Regex.Replace(k.Key, @"[\d-]", string.Empty) == eventName) + 1;
            //int newKeyIndex = eventEndTimestampDict.Count(k => k.Key.Contains(eventName)) + 1;
            eventEndTimestampDict.Add(eventName + newKeyIndex, Time.realtimeSinceStartup.ToMilliseconds());
        }
    }

    /*
     * Is responsible for exporting all the data recorded for each event into a text file
    */
    public void ExportData()
    {
        date = System.DateTime.Now;

        string path;
       // path = Application.dataPath + "/DataRecorder/RecordedData/" + exportedFileName;   // path where the text file will be exported to
        path = Application.streamingAssetsPath + "/RecordedData/" + exportedFileName;

       if (!File.Exists(path) || OverWriteExistingDataLog)  // create new text file if it doesn't exist or overwrite existing text file
            File.WriteAllText(path, string.Empty);

        foreach (string heading in headingInfo)  // writes all the heading information into the text file
            File.AppendAllText(path, HEADINGCODE + heading + "\n");

        //if(includeTargetFrameRate)              // write the target framerate used as part of the heading information
        //    File.AppendAllText(path, HEADINGCODE + "Data Recording Sample Rate: " + targetFrameRate.ToString() + "\n");
        if (includeRecordPerSec)
            File.AppendAllText(path, HEADINGCODE + "Data was recorded " + timesToRecordPerSec + " times per second" + "\n");

        if (includeDateTime)  // write the date/time of when the app started as part of the heading information
            File.AppendAllText(path, HEADINGCODE + "Date and Time: " + date.ToString() + "\n\n");

        foreach (var obj in recordableObjs)     // write the object code associated with each recordable object as part of the heading information
            File.AppendAllText(path, HEADINGCODE + " " + obj.objName + " = " + RECOBJCODE + obj.Id + "\n");

        int i = 1;
        foreach (var item in eventsList)    // write the event code associated with each event as part of the heading information
        {
            File.AppendAllText(path, HEADINGCODE + " " + item + " = " + EVENTCODE + i + "\n");
            i++;
        }

        File.AppendAllText(path, "\n");

        foreach (List<RecordableEvent.DataBlock> dataBlockList in dataBlocks2DList) // go through all the lists of recorded data
        {
            // use the actual list as the key to get the name of the event and repetition that the data belongs to (value example: someEvent2
            // where someEvent is the name of the event while 2 indicates that it is the second repetition of someEvent)
            string v = dataBlockDict[dataBlockList];

            i = Int32.Parse(Regex.Replace(v, "[^0-9]", "")) - 1;            // i is used to get the correct index. (example: if v == someEvent2, then i = 2 - 1 = 1, we are on the first repetition of someEvent)
            string eventName = Regex.Replace(v, @"[\d-]", string.Empty);    // removes the numerical characters in v, so that only the event name remains (example: if v == someEvent2, then eventName = someEvent)

            // create a temporary list of EventInfo that belongs to the event given by v (example: if v == someEvent2, then eventInfoTemp will only include event information on all repetitions for someEvent)
            //  List<EventInfo> eventInfoTemp = eventsInfoList.Where(e => v.Contains(e.eventName)).ToList();
            List<EventInfo> eventInfoTemp = eventsInfoList.Where(e => eventName == e.eventName).ToList();

            //Debug.Log("v: " + v);
            //foreach (var item in eventInfoTemp)
            //    Debug.Log(item.eventName + "-" + item.eventInfo);

            string startTime = string.Empty;
            if (includeEventTimestamp) // if true, include the timeStamp of when the event/event repetition began
            {
                string key = eventName + (i + 1).ToString();
                startTime = eventStartTimestampDict[key].ToString();
                startTime = " timestamp(ms):" + startTime;
            }

            //foreach (var item in eventsList)
            //    Debug.Log("eventsList: " + item);

            // Debug.Log("eventName: " + eventInfoTemp[i].eventName);
            // Debug.Log("Index:" + eventsList.IndexOf(eventInfoTemp[i].eventName));

            if (i >= eventInfoTemp.Count)   // this means that the current event/event repetition is missing information, so "MISSING HEADER INFO" will be used as a substitute
            {
                eventInfoTemp.Add(new EventInfo(eventName, "MISSING HEADER INFO"));
                eventsInfoList.Add(new EventInfo(eventName, "MISSING HEADER INFO"));
            }

            //Debug.Log("eventName: " + eventName + "\neventinfotemp count: " + eventInfoTemp.Count + "\ni: " + i + "eventInfoList count: " + eventsInfoList.Count);
            string eventCode = EVENTCODE + (eventsList.IndexOf(eventName) + 1); // (example: if someEvent was the 3rd event added, the event code will be 23)
            File.AppendAllText(path, eventCode + startTime + " " + eventInfoTemp[i].eventInfo + " rep:" + (i + 1) + "\n"); // include the event code, event start timestamp, relevant event info and the current repetition in the text file

            // go through each block of data in the current event/event repetition and include the object code, the time in which the data was recorded and the name of the data and its value in the text file
            foreach (RecordableEvent.DataBlock block in dataBlockList)
                File.AppendAllText(path, RECOBJCODE + block.objID + " timestamp(ms):" + block.timeStamp.ToMilliseconds() + " " + block.dataName + ":" + block.dataValue + "\n");

            string endTime = string.Empty;
            if (includeEventTimestamp)  // if true, include the timeStamp of when the event/event repetition ended
            {
                endTime = eventEndTimestampDict[eventInfoTemp[i].eventName + (i + 1).ToString()].ToString();
                endTime = " timestamp(ms):" + endTime;
            }

            File.AppendAllText(path, eventCode + "E" + endTime + " " + eventInfoTemp[i].eventInfo + " rep:" + (i + 1) + "\n");  // include this line in the text file to indicate that the current event/event repetition has ended
        }

        File.AppendAllText(path, "\n");  // include this line in the text file to indicate that the current event/event repetition has ended
        onDataExportingFinished.Invoke();

        // clear all the lists and dictionary so that old data doesn't get duplicated on the next recording/exporting phase.
        eventsInfoList.Clear();
        dataBlockDict.Clear();
        eventStartTimestampDict.Clear();
        eventEndTimestampDict.Clear();
        dataBlockIndex = 0;
        dataBlocks2DList.Clear();
        headingInfo.Clear();
    }

    /*
     * This gets called when an event/event repetition ends and stores the new data in dataBlocks2DList
     * inputs: a string representing the name of the event that just ended
     * fields used: dataBlocks2DList, dataBlockDict, dataBlockIndex
    */
    private void UpdateDataBlocksList(string eventName)
    {
        dataBlocks2DList.Add(new List<RecordableEvent.DataBlock>());


        foreach (RecordableEvent recObj in recordableObjs.Where(e => e.SelectedEvent == eventName))
        {
            foreach (RecordableEvent.DataBlock dataBlock in recObj.DataList)
            {
                dataBlocks2DList[dataBlockIndex].Add(dataBlock);
            }
        }


        // an event may have multiple repetitions, so we will use the datablock list itself as the key
        // int newKeyIndex = dataBlockDict.Count(v => v.Value.Contains(eventName)) + 1;
        int newKeyIndex = dataBlockDict.Count(v => Regex.Replace(v.Value, @"[\d-]", string.Empty) == eventName) + 1;
        dataBlockDict.Add(dataBlocks2DList[dataBlockIndex], eventName + newKeyIndex);
        dataBlockIndex++;
    }

    /*
    * Adds a new event
    * inputs: a string representing the name of the event that is to be added
   */
    public void AddEvent(string eventName)
    {
        eventsList.Add(eventName);
    }

    /*
     * Removes an event from the event list at the given index
     * inputs: an int representing the index of the list
     * fields used: eventsList
    */
    public void RemoveEventAt(int index)
    {
        eventsList.RemoveAt(index);
    }

    /*
     * Is used to add new event information in eventsInfoList. This method should be used if the user wants to add event information through code and not through the inspector
     * inputs: a string representing the name of the event that the event info will be for
     * fields used: eventsInfoList, eventsInfoDescription
    */
    public void AddEventInfo(string eventName, string eventInfo)
    {
        eventsInfoList.Add(new EventInfo(eventName, eventInfo));
        eventsInfoIndex.Add(eventsList.IndexOf(eventName));
        eventsInfoDescription.Add(eventInfo);
    }

    public void SetTimesToRec(int amount)
    {
        this.TimesToRecordPerSec = amount;
    }

    public void SetTimesToRec(string amount)
    {
        Debug.Log("amount: " + amount);
        SetTimesToRec(int.Parse(amount));
    }

#if UNITY_EDITOR
    // Writes the rendertime and actual FPS for debug purposes in the editor
    private void OnGUI()
    {
        if (showFPS)
        {
            GUI.contentColor = Color.black;
            GUI.Label(new Rect(0, 100, 200, 50), new GUIContent("Frames per second: " + 1 / Time.deltaTime));
        }
    }
#endif
}
