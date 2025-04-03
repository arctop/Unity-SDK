
using System;
using UnityEngine;

namespace com.arctop
{
#if UNITY_ANDROID
    //Android success or failure callback proxy.
    //Used on the Android native side
    public class ArctopSDKSuccessOrFailureCallback : AndroidJavaProxy
    {
        public event Action OnSuccess;
        public event Action<int> OnFailure;
        public ArctopSDKSuccessOrFailureCallback()
            : base("com.arctop.unity.IArctopSdkSuccessOrFailureCallback")
        {
        }

        void onSuccess()
        {
            OnSuccess?.Invoke();
        }

        void onFailure(int error)
        {
            OnFailure?.Invoke(error);
        }
    }
#endif
}