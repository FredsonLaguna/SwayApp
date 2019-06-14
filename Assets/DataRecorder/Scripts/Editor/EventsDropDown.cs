using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(RecordableEvent), true)]
public class EventsDropDown : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        UpdateGUI();
    }

    public void UpdateGUI()
    {
        RecordableEvent script = (RecordableEvent)target;
        GUIContent arrayLabel = new GUIContent("Events List");
        string[] eventList = GameObject.FindObjectOfType<DataRecorder>().eventsList.ToArray();
        script.eventsIndex = EditorGUILayout.Popup(arrayLabel, script.eventsIndex, eventList);
        script.SelectedEvent = eventList[script.eventsIndex];
        EditorUtility.SetDirty(target);
    }
}


[CustomEditor(typeof(DataRecorder), true)]
public class EventsInfoDropDown : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DataRecorder script = (DataRecorder)target;
        string[] eventList = script.eventsList.ToArray();


        GUILayout.Label("\nThe list of events that objects can be recorded into. Only use alpha characters", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Event"))
        {
            script.AddEvent("NewEvent");
        }
        GUILayout.Label("Number of events: " + script.eventsList.Count);
        for (int i = 0; i < script.eventsList.Count; i++)
        {
            GUILayout.BeginHorizontal();
            script.eventsList[i] = EditorGUILayout.TextField(string.Empty, script.eventsList[i]);
            if (GUILayout.Button("Remove") && script.eventsList.Count > 1)
            {
                while (script.eventsInfoList.Any(item => item.eventName == script.eventsList[i]))
                {
                    int index = script.eventsInfoList.FindLastIndex(n => n.eventName == script.eventsList[i]);
                    script.eventsInfoIndex.RemoveAt(index);
                    script.eventsInfoDescription.RemoveAt(index);
                    script.eventsInfoList.RemoveAt(index);
                }

                for (int j = 0; j < script.eventsInfoIndex.Count; j++)
                    if (script.eventsInfoIndex[j] > i)
                        script.eventsInfoIndex[j]--;

                script.RemoveEventAt(i);

            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Label("\n\nA list of event information. Add new information for each event/event repetition.\nInformation must be added in ascending order from the top", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Event Info"))
        {
            script.eventsInfoIndex.Add(0);
            script.eventsInfoDescription.Add(string.Empty);
        }
         
        for (int i = 0; i < script.eventsInfoIndex.Count; i++)
        {
            script.eventsInfoIndex[i] = EditorGUILayout.Popup("Event Name", script.eventsInfoIndex[i], eventList);
            script.eventsInfoDescription[i] = EditorGUILayout.TextField("Event Info", script.eventsInfoDescription[i]);


            if (i >= script.eventsInfoList.Count)
                script.eventsInfoList.Add(new DataRecorder.EventInfo(eventList[script.eventsInfoIndex[i]], script.eventsInfoDescription[i]));
            else
                script.eventsInfoList[i] = new DataRecorder.EventInfo(eventList[script.eventsInfoIndex[i]], script.eventsInfoDescription[i]);

            if (GUILayout.Button("Remove", GUILayout.Width(75), GUILayout.Height(25)))
            {
                script.eventsInfoIndex.RemoveAt(i);
                script.eventsInfoDescription.RemoveAt(i);
                script.eventsInfoList.RemoveAt(i);
            }

        }

        EditorUtility.SetDirty(target);
    }
}
