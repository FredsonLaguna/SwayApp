using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleMoveEvent : RecordableEvent
{
    public override void AddDataBlock()
    {
        AddDataBlock("MoveEvent", transform.position);
        AddDataBlock("Scale", transform.lossyScale);
    }

}
