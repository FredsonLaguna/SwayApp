using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class BertecForcePlate
{
    public IntPtr handle = IntPtr.Zero;
    public BertecForcePlate_UnityWrapper.bertec_DataFrame dataFrame;
    public bool enableAutoZeroFlag = true;
    public bool startWithZeroLoadFlag = true;

    public BertecForcePlate(bool enableFlag = true, bool startWithZeroLoadFlag = true)
    {
        Debug.Log("handle before Init: " + handle);
        handle = BertecForcePlate_UnityWrapper.bertec_Init();
        Debug.Log("handle after Init: " + handle);
        dataFrame = new BertecForcePlate_UnityWrapper.bertec_DataFrame();
        this.enableAutoZeroFlag = enableFlag;
        BertecForcePlate_UnityWrapper.bertec_SetEnableAutozero(handle, (enableFlag == true)? 1 : 0);
        this.startWithZeroLoadFlag = startWithZeroLoadFlag;
    }

    public int Start()
    {
        int returnCode = BertecForcePlate_UnityWrapper.bertec_Start(handle);
        Debug.Log("return code for Start(): " + returnCode + ":" + (BertecForcePlate_UnityWrapper.bertec_StatusErrors)returnCode);
        return returnCode;
    }

    public IEnumerator ZeroNow()
    {
        BertecForcePlate_UnityWrapper.bertec_ZeroNow(handle);
        while (BertecForcePlate_UnityWrapper.bertec_GetAutozeroState(handle) != (int)BertecForcePlate_UnityWrapper.bertec_AutoZeroStates.AUTOZEROSTATE_ZEROFOUND)
            yield return null;
    }

    public int GetStatus()
    {
        int returnCode = BertecForcePlate_UnityWrapper.bertec_GetStatus(handle);
        Debug.Log("return code for GetStatus(): " + returnCode + ":" + (BertecForcePlate_UnityWrapper.bertec_StatusErrors)returnCode);
        return returnCode;
    }


    // returns the number of data blocks, not a return code
    public int ReadBufferedData()
    {
        return BertecForcePlate_UnityWrapper.bertec_ReadBufferedData(handle, ref dataFrame, Marshal.SizeOf(dataFrame));
    }

    public int Stop()
    {
        int returnCode = BertecForcePlate_UnityWrapper.bertec_Stop(handle);
        Debug.Log("return code for Stop(): " + returnCode + ":" + (BertecForcePlate_UnityWrapper.bertec_StatusErrors)returnCode);
        return returnCode;
    }

    public void Close()
    {
        BertecForcePlate_UnityWrapper.bertec_Close(handle);
        handle = IntPtr.Zero;
    }
}
