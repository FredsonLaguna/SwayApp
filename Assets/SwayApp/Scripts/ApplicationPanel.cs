using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ApplicationPanel : MonoBehaviour
{
    [SerializeField]
    SwayController swayController;

    [SerializeField]
    Text stageNum, numOfSways, timePeriod, targetSway, speed;
    [Space(20)]
    Text Mx;
    Text My, Mz, Fx, Fy, Fz;
    [Space(20)]
    Text trackerRot;
    Text raftRot;
    Text status;
    Text appPath;

    [SerializeField]
    InputField recordingFrequency;
    [SerializeField]
    Text experimentName,
    subjectID,
    additionalInfo;

    [System.Serializable]
    public class HeaderInfoEvent : UnityEvent<int, string> { };
    public HeaderInfoEvent onHeaderInfoHasChanged;

    // Start is called before the first frame update
    void Start()
    {
        swayController = FindObjectOfType<SwayController>();
        stageNum = GameObject.FindGameObjectWithTag("Stage").GetComponent<Text>();
        numOfSways = GameObject.FindGameObjectWithTag("NumOfSways").GetComponent<Text>();
        timePeriod = GameObject.FindGameObjectWithTag("TimePeriod").GetComponent<Text>();
        targetSway = GameObject.FindGameObjectWithTag("TargetSway").GetComponent<Text>();
        speed = GameObject.FindGameObjectWithTag("Speed").GetComponent<Text>();

        Mx = GameObject.FindGameObjectWithTag("Mx").GetComponent<Text>();
        My = GameObject.FindGameObjectWithTag("My").GetComponent<Text>();
        Mz = GameObject.FindGameObjectWithTag("Mz").GetComponent<Text>();
        Fx = GameObject.FindGameObjectWithTag("Fx").GetComponent<Text>();
        Fy = GameObject.FindGameObjectWithTag("Fy").GetComponent<Text>();
        Fz = GameObject.FindGameObjectWithTag("Fz").GetComponent<Text>();

        trackerRot = GameObject.FindGameObjectWithTag("TrackerRotation").GetComponent<Text>();
        raftRot = GameObject.FindGameObjectWithTag("RaftRotation").GetComponent<Text>();
        status = GameObject.FindGameObjectWithTag("Status").GetComponent<Text>();

        recordingFrequency = GameObject.FindGameObjectWithTag("RecordingFrequency").GetComponent<InputField>();
        appPath = GameObject.FindGameObjectWithTag("AppPath").GetComponent<Text>();
        experimentName = GameObject.FindGameObjectWithTag("ExperimentName").GetComponent<Text>();
        subjectID = GameObject.FindGameObjectWithTag("SubjectID").GetComponent<Text>();
        additionalInfo = GameObject.FindGameObjectWithTag("AdditionalInfo").GetComponent<Text>();
        appPath.text = Application.dataPath;
    }

    public void UpdateSwayControllerTexts()
    {
        stageNum.text = swayController.StageNum.ToString();
        numOfSways.text = swayController.NumOfSways.ToString();
        timePeriod.text = swayController.TimePeriod.ToString();
        targetSway.text = swayController.TargetSway.ToString();
        speed.text = swayController.Speed.ToString();      
    }

    public void UpdateTrackerRotationText(string x, string y, string z)
    {
        trackerRot.text = "(" + x + "," + y + "," + z + ")";
    }

    public void UpdateRaftRotationText(string x, string y, string z)
    {
        raftRot.text = "(" + x + "," + y + "," + z + ")";
    }

    public void UpdateForcePlateReadingsText(string Mx, string My, string Mz, string Fx, string Fy, string Fz)
    {
        this.Mx.text = Mx;
        this.My.text = My;
        this.Mz.text = Mz;
        this.Fx.text = Fx;
        this.Fy.text = Fy;
        this.Fz.text = Fz;
    }

    public void UpdateFrequencyInput(string text)
    {
        this.recordingFrequency.text = text;
    }

    public void UpdateFrequencyInput(int amount)
    {
        UpdateFrequencyInput(amount.ToString());
    }


    public void CloseApp()
    {
        Application.Quit();
    }

    public void UpdateDataRecorderHeaderInfo()
    {
        string textFileName = string.Empty; 

        if (experimentName.text != string.Empty)
        {
            DataRecorder.Instance.headingInfo.Add(experimentName.text);
            textFileName += experimentName.text;
        }

        if (subjectID.text != string.Empty)
        {
            DataRecorder.Instance.headingInfo.Add(subjectID.text);
            textFileName += subjectID.text;
        }

        if (additionalInfo.text != string.Empty)
        {
            DataRecorder.Instance.headingInfo.Add(additionalInfo.text);
            textFileName += additionalInfo.text;
        }

        ChangeDataTextFileName(textFileName);
    }

    public void ChangeDataTextFileName(string name)
    {
        DataRecorder.Instance.exportedFileName = name + ".txt";
    }
}
