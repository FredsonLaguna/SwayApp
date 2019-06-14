using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raft : RecordableEvent
{

    ApplicationPanel applicationPanel;
    protected override void Start()
    {
        applicationPanel = GameObject.FindObjectOfType<ApplicationPanel>();
    }

    public override void AddDataBlock()
    {
        double x = System.Math.Round(this.transform.InspectorNegativeEulerAngles().x, 3);
        double y = System.Math.Round(this.transform.InspectorNegativeEulerAngles().y, 3);
        double z = System.Math.Round(this.transform.InspectorNegativeEulerAngles().z, 3);

        AddDataBlock("rotation(x,y,z)",  x + "," + y + ","+ z);
        applicationPanel.UpdateRaftRotationText(x.ToString(), y.ToString(), z.ToString());
    }

    // used for debugging
    //public Vector3 rotation;
    //private void Update()
    //{
    //    this.rotation = this.transform.InspectorNegativeEulerAngles();
    //}
}
