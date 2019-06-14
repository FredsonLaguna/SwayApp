using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerSway : RecordableEvent
{
    [SerializeField]
    TrackerCalibrator trackCal;
    ApplicationPanel applicationPanel;


    protected override void Start()
    {
        applicationPanel = GameObject.FindObjectOfType<ApplicationPanel>();
    }

    protected override void Awake()
    {
        base.Awake();
        trackCal = this.gameObject.GetComponent<TrackerCalibrator>();
    }


    public override void AddDataBlock()
    {
        Vector3 rot = transform.InspectorNegativeEulerAngles(); // get the tracker's rotation as displayed in the inspector
        rot = trackCal.SubtractFromAverage(rot);                // gets how much the tracker has rotated relative to its starting rotation

        double x = System.Math.Round(rot.x, 3);
        double y = System.Math.Round(rot.y, 3);
        double z = System.Math.Round(rot.z, 3);

        AddDataBlock("rotation(x,y,z)",  x + "," + y + "," + z );
        applicationPanel.UpdateTrackerRotationText(x.ToString(), y.ToString(), z.ToString());
    }
}
