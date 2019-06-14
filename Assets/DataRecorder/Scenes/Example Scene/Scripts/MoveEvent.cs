using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEvent : RecordableEvent
{
    public override void AddDataBlock()
    {
        AddDataBlock("MoveEvent", transform.position);
    }
}
