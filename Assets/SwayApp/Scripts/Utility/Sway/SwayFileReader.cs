using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.Events;

/*
 * author: Fredson Laguna
 * Date: 11.30.2018
 * class: SwayFileReader
 * purpose: searches for a .txt file in the Assets/Resources/Sway_Files folder that contains inputs for each stage that 
 * keeps track of the stage#, time period sway amount and sway duration
 * and will parse each input as int and store them in a struct called Stage.
 * Each Stage struct will be stored in a list, which is what will be used to access the data. 
*/

[DisallowMultipleComponent]
public class SwayFileReader : MonoBehaviour {

    private List<Stage> stages = new List<Stage>();
    public List<Stage> Stages { get { return stages; } }
    public UnityEvent OnReadingSwayFile;
    public UnityEvent OnReadingSwayFileError;


    public IEnumerator ReadSwayFile ()
    {
        stages.Clear();
        OnReadingSwayFile.Invoke();
        yield return new WaitForSeconds(1);
        //TextAsset swayFile = Resources.Load<TextAsset>("Sway_Files/Sway");  // the .txt file that contains inputs for stage#, time period, sway amount
        UnityWebRequest www = UnityWebRequest.Get("file:///" + Application.streamingAssetsPath + "/Sway_Files/Sway.txt");
        yield return www.SendWebRequest();

        //string[] swayStr = swayFile.text.Split('\n');                       // stores each line from .txt file into an array    
        string[] swayStr = www.downloadHandler.text.Split('\n');

        foreach (string strLine in swayStr){
            if (strLine != string.Empty && Regex.IsMatch(strLine[0].ToString(), "[0-9]")){  // if current line isn't empty and contains only the inputs needed
                string[] temp = strLine.Split(',');                                         // seperates input and stores each one into an array         
                if (temp.Length == 5){                                                      // verifies that each stage has exactly 4 inputs
                    Stage stage;                                                            // create a new Stage struct and parse the text data into int/float
                    stage.stageNum = int.Parse(temp[0]);
                    stage.timePeriod = float.Parse(temp[1]);
                    stage.sway = float.Parse(temp[2]);
                    stage.numOfSways = int.Parse(temp[3]);
                    stage.swaySidetoSide = (int.Parse(temp[4]) == 0)? true : false;
                    stages.Add(stage);
                }
                else{
                    Debug.Log("Error: Missing an input");
                    OnReadingSwayFileError.Invoke();
                    break;
                }
            }
        }

        // used to test if Stage structs were created correctly
        if (Application.isEditor)
        foreach (var stage in stages)
        {
            Debug.Log("stage #: " + stage.stageNum + "   time: " + stage.timePeriod + "   sway: " + stage.sway + "   swayDuration: " + stage.numOfSways);
        }
    }

    public struct Stage
    {
        public int stageNum;
        public float timePeriod;
        public float sway;
        public int numOfSways;
        public bool swaySidetoSide;
    }
}
