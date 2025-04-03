using System;
using UnityEngine;

namespace com.arctop
{
#if UNITY_ANDROID
    
    // Android proxy class.
    // Implements an interface on the Android Java side and relays data
    // back to the C# side.
    public class ArctopSDKCallback : AndroidJavaProxy
    {
        public event Action<bool> OnIsUserLoggedInCallback;
        public event Action<String> OnScanResultCallback;
        public event Action<int,int> OnConnectionStatusCallback;
        public event Action<String,float> OnValueChangedCallback;
        public event Action<bool,int> OnQAStatusCallback;
        public event Action<String> OnSignalQualityCallback;
        public event Action OnSessionCompleteCallback;
        
        public ArctopSDKCallback()
            : base("com.arctop.unity.IArctopSdkCallback")
        {
        }

        public void IsUserLoggedInCallback(bool loggedIn)
        {
            OnIsUserLoggedInCallback?.Invoke(loggedIn);
        }

        public void ScanResultCallback(String device)
        {
            OnScanResultCallback?.Invoke(device);
        }
        // delegate for connection status
        public void ConnectionStatusCallback(int previous, int current)
        {
            OnConnectionStatusCallback?.Invoke(previous,current);
        }
        // delegate for realtime values
        public void ValueChangedCallback(String key, float value)
        {
            OnValueChangedCallback?.Invoke(key,value);
        }
        // delegate for qa
        public void QAStatusCallback(Boolean passed, int errorCode)
        {
            OnQAStatusCallback?.Invoke(passed,errorCode);
        }
        // delegate for signalQuality
        public void SignalQualityCallback(String signalQuality)
        {
            OnSignalQualityCallback?.Invoke(signalQuality);
        }
        // delegate for session complete
        public void SessionCompleteCallback()
        {
            OnSessionCompleteCallback?.Invoke();
        }
    }
#endif
}