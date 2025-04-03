using System;
using UnityEngine;

namespace com.arctop
{
#if UNITY_ANDROID
    // Android proxy for native binding callback
    public class ArctopServiceBindCallback : AndroidJavaProxy
    {
        public event Action OnBindSuccess;

        public event Action<ArctopSDK.BindError> OnBindFailure;
        public ArctopServiceBindCallback()
            : base("com.arctop.unity.IArctopServiceBindCallback")
        {
        }

        void onSuccess()
        {
            OnBindSuccess?.Invoke();
        }

        void onFailure(int error)
        {
            OnBindFailure?.Invoke((ArctopSDK.BindError)error);
        }
    }
#endif
}