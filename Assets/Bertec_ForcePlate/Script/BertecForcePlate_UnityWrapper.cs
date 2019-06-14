using System.Runtime.InteropServices;
using System;
using UnityEngine;


public class BertecForcePlate_UnityWrapper : MonoBehaviour
{
    const int BERTEC_MAX_CHANNELS = 32;
    const int BERTEC_MAX_DEVICES = 4;
    const int BERTEC_MAX_NAME_LENGTH = 16;

    [DllImport("BertecDevice", EntryPoint = "bertec_Init")]
    public static extern IntPtr bertec_Init();

    [DllImport("BertecDevice", EntryPoint = "bertec_Start")]
    public static extern int bertec_Start(IntPtr handle);

    [DllImport("BertecDevice", EntryPoint = "bertec_GetStatus")]
    public static extern int bertec_GetStatus(IntPtr handle);

    [DllImport("BertecDevice", EntryPoint = "bertec_ReadBufferedData")]
    public static extern int bertec_ReadBufferedData(IntPtr handle, ref bertec_DataFrame dataFrame, int dataFrameSize);

    [DllImport("BertecDevice", EntryPoint = "bertec_Stop")]
    public static extern int bertec_Stop(IntPtr handle);

    [DllImport("BertecDevice", EntryPoint = "bertec_Close")]
    public static extern void bertec_Close(IntPtr handle);

    [DllImport("BertecDevice", EntryPoint = "bertec_SetEnableAutozero")]
    public static extern int bertec_SetEnableAutozero(IntPtr handle, int enableFlag);

    [DllImport("BertecDevice", EntryPoint = "bertec_ZeroNow")]
    public static extern int bertec_ZeroNow(IntPtr handle);

    [DllImport("BertecDevice", EntryPoint = "bertec_GetAutozeroState")]
    public static extern int bertec_GetAutozeroState(IntPtr handle);


    ///** Additional data that is part of the frame of data. Note that this is only value on devices with firmware that support these
    //    (that is, hasInternalClock and hasAuxSyncPins are set). All other devices will return zero. */
    //struct bertec_AdditionalData
    //{
    //    int64_t timestamp;  // the plate's clock or "sequence number" for this frame of data. Devices without bertec_DeviceInfo.hasInternalClock will have this backfilled by computed sequence numbers.
    //    unsigned char auxData;    // rolling 8-bit value of the AUX pin status. MSB is the current value. Valid in all AUX pin modes.
    //    unsigned char syncData;   // rolling 8-bit value of the SYNC pin status. MSB is the current value. Valid in all SYNC pin modes.
    //};
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct bertec_AdditionalData
    {
        public long timestamp; // long is equivalent to int64_t
        public byte auxData;   // byte is equvalent to unsigned char
        public byte syncData;
    };

    ///** Channel data that is part of the frame of data. */
    //struct bertec_ChannelData
    //{
    //    int count;  // how much channel data is in this structure. Copied from bertec_DeviceInfo.channelCount
    //    float data[BERTEC_MAX_CHANNELS];
    //};
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct bertec_ChannelData
    {
        public int count;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = BERTEC_MAX_CHANNELS)]
        // 0: Fx
        // 1: Fy
        // 2: Fz
        // 3: Mx
        // 4: My
        // 5: Mz
        public float[] data;
    };

    ///** A single device's block of data, both the channel data and the additional timestamp/sync data. This is part of the bertec_DataFrame */
    //struct bertec_DeviceData
    //{
    //    bertec_ChannelData channelData;
    //    bertec_AdditionalData additionalData;
    //};
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct bertec_DeviceData
    {
        public bertec_ChannelData channelData;
        public bertec_AdditionalData additionalData;
    };

    ///** A single block of data as sent via bertec_DataCallback or retrieved via bertec_ReadBufferedData. The frame contains a single sample of data from all of the devices. */
    //struct bertec_DataFrame
    //{
    //    int deviceCount;   // same as bertec_DeviceCount
    //    bertec_DeviceData device[BERTEC_MAX_DEVICES];
    //};
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct bertec_DataFrame
    {
        public int deviceCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = BERTEC_MAX_DEVICES)]
        public bertec_DeviceData[] device;
    };

    /** Defined errors and status values */
    public enum bertec_StatusErrors
    {
        BERTEC_NOERROR = 0,/** Generic no error */
        BERTEC_NO_BUFFERS_SET = -2,/** no data buffers were allocated */
        BERTEC_DATA_BUFFER_OVERFLOW = -4,/** the internal buffer has become saturated; either data polling isn't occuring often/fast enough,
                                   or else your callback is blocking for too long. Old data is now lost. */


        BERTEC_NO_DEVICES_FOUND = -5,/** there are apparently no devices attached */

        BERTEC_DATA_READ_NOT_STARTED = -6,/** didn't start the data process - call Start */

        BERTEC_DATA_SYNCHRONIZING = -7,/** synchronizing, data not available yet */

        BERTEC_DATA_SYNCHRONIZE_LOST = -8,/** the plates have lost sync with each other - check sync cable */


        BERTEC_DATA_SEQUENCE_MISSED = -9,/** one or more plates have missing data sequence - data may be invalid */

        BERTEC_DATA_SEQUENCE_REGAINED = -10,/** the plates have regained their data sequence */


        BERTEC_NO_DATA_RECEIVED = -11,/** no data is being received from the devices, check the cables */


        BERTEC_DEVICE_HAS_FAULTED = -12,/** the device has failed in some manner - power off the device, check all connections, power back on */

        BERTEC_LOOKING_FOR_DEVICES = -45,/** the sdk is scanning for devices; the next status will be either BERTEC_NO_DEVICES_FOUND or BERTEC_DEVICES_READY */

        BERTEC_DEVICES_READY = -50,/** there are devices connected */

        BERTEC_AUTOZEROSTATE_WORKING = -51,/** currently finding the zero values */

        BERTEC_AUTOZEROSTATE_ZEROFOUND = -52,/** the zero leveling value was found */

        BERTEC_ERROR_INVALIDHANDLE = -100,/** handle is invalid */

        BERTEC_UNABLE_TO_LOCK_MUTEX = -101,/** internal logic error */

        BERTEC_UNSUPPORED_COMMAND = -200,/** the firmware doesn't support the command that was attempted to be used */

        BERTEC_INVALID_PARAMETER = -201,
        BERTEC_INDEX_OUT_OF_RANGE = -202,

        BERTEC_GENERIC_ERROR = -32767
    };

    public enum bertec_AutoZeroStates
    {
        AUTOZEROSTATE_NOTENABLED,
        AUTOZEROSTATE_WORKING,
        AUTOZEROSTATE_ZEROFOUND
    };

}

