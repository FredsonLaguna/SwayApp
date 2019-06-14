using System;   
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; // must include this in order to reference Valve VR classes

/*
 * author: Fredson Laguna
 * Date: 1.27.2019
 * class: TrackerFinder
 * purpose: Finds a tracker device and sets "SteamVR_TrackedObject" index variable to it 
*/

[DisallowMultipleComponent]
[RequireComponent(typeof(SteamVR_TrackedObject))]
public class TrackerFinder : MonoBehaviour
{
    SteamVR_TrackedObject trackedObject;
    int numOfDevices = Enum.GetNames(typeof(SteamVR_TrackedObject.EIndex)).Length - 1;

    void Start()
    {
        trackedObject = transform.GetComponent<SteamVR_TrackedObject>();
        trackedObject.index = GetTrackerIndex();
    }

    /*
    * This method finds and returns the index of a found tracker device
    * solution by Liam Ferris: https://stackoverflow.com/questions/43184610/how-to-determine-whether-a-steamvr-trackedobject-is-a-vive-controller-or-a-vive
    */
    SteamVR_TrackedObject.EIndex GetTrackerIndex()
    {
        uint index = 0;                                         // index of device
        var error = ETrackedPropertyError.TrackedProp_Success;
        for (uint i = 0; i < 16; i++)
        {
            var result = new System.Text.StringBuilder((int)64);
            OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);
            //Debug.Log(result.ToString()); used to check the actual names given to each vive device
            if (result.ToString().Contains("tracker"))
            {
                index = i;
                break;
            }
        }
        return (SteamVR_TrackedObject.EIndex)index;
    }
}
