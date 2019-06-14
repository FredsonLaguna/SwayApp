using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BertecForcePlate_Recorder : RecordableEvent
{
    BertecForcePlate forcePlate;
    //public bool continueToPoll = true;
    public bool enableAutoZero = true;
    public bool startWithZeroLoad = true;
    public float[] data;

    public UnityEvent onStartingForcePlate;
    [System.Serializable]
    public class StringEvent : UnityEvent<string> { };
    public StringEvent onFxChanged, onFyChanged, onFzChanged, onMxChanged, onMyChanged, onMzChanged;

    protected override void OnEnable()
    {
        base.OnEnable();
        DataRecorder.OnRecordDataFinished += StopAndClose;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        DataRecorder.OnRecordDataFinished -= StopAndClose;

    }


    public IEnumerator StartForcePlate()
    {
        forcePlate = new BertecForcePlate(enableAutoZero, startWithZeroLoad);
        forcePlate.Start();
        onStartingForcePlate.Invoke();
        while ( (BertecForcePlate_UnityWrapper.bertec_StatusErrors)forcePlate.GetStatus() != BertecForcePlate_UnityWrapper.bertec_StatusErrors.BERTEC_DEVICES_READY)
        {
            if(Application.isEditor)
                Debug.Log("Waiting for Devices To be Ready");
            yield return null;
        }

        if (startWithZeroLoad)
            yield return StartCoroutine(forcePlate.ZeroNow());

        yield return new WaitForSeconds(1);
    }

    void StopAndClose(string eventName)
    {
        if (SelectedEvent == eventName)
        {
            StopAndClose();
            if (Application.isEditor)
                Debug.Log("Stopping force plate"); 
        }
    }


    [ContextMenu("StopAndClose")]
    void StopAndClose()
    {
        if (forcePlate != null)
        {
            forcePlate.Stop();
            forcePlate.Close();
        }

        if (Application.isEditor)
            Debug.Log("stop and closed forceplate");
    }
    [ContextMenu("GetStatus")]
    void GetStatus()
    {
        if (forcePlate != null)
        {
            forcePlate.GetStatus();
        }
    }

    public override void AddDataBlock()
    {
        if (forcePlate.ReadBufferedData() > 0)
        {
            data = forcePlate.dataFrame.device[0].channelData.data;
            float fx = data[0];
            float fy = data[1];
            float fz = data[2];
            float mx = data[3];
            float my = data[4];
            float mz = data[5];
            AddDataBlock("force plate (fx, fy, fz, mx, my, mz)", fx.ToString() + "," + fy.ToString() + "," + fz.ToString() + "," + mx.ToString() + "," + my.ToString() + "," + mz.ToString());
            onFxChanged.Invoke(fx.ToString());
            onFyChanged.Invoke(fy.ToString());
            onFzChanged.Invoke(fz.ToString());
            onMxChanged.Invoke(mx.ToString());
            onMyChanged.Invoke(my.ToString());
            onMzChanged.Invoke(mz.ToString());
            if(Application.isEditor)
                 Debug.Log("adding data block");

        }
    }
}
