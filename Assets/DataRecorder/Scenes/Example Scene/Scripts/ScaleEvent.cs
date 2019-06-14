using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleEvent : RecordableEvent
{
    public override void AddDataBlock()
    {
        AddDataBlock("Scale", transform.lossyScale);
    }

}
