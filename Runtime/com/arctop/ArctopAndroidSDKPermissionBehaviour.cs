using System;
using UnityEngine;
using UnityEngine.Android;
namespace com.arctop
{
    // Behaviour is used simple to request permission and if granted, create the SDK instance (Bind)
    // On Android devices this will pop up the permission dialog, or immediately create the SDK if permission has been granted already
    // On iOS devices, the SDK will be created immediately on Start()
    // This is just a convenience behaviour. You can extend it to provide other functionality when permissions are denied
    // You can also implement the whole thing yourself.
    public class ArctopAndroidSDKPermissionBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Connect your arctopClient object here
        /// </summary>
        [SerializeField] protected ArctopNativeClient arctopClient;
        /// <summary>
        /// should this component auto create the SDK once permission has been granted?
        /// </summary>
        [SerializeField] protected bool createSDKOnPermissionGranted = true;
        protected void Start()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (Permission.HasUserAuthorizedPermission(ArctopSDK.ARCTOP_PERMISSION))
            {
                if (createSDKOnPermissionGranted)
                {
                    arctopClient?.CreateArctopSDK();
                }
            }
            else
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
                callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
                callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
                Permission.RequestUserPermission(ArctopSDK.ARCTOP_PERMISSION, callbacks);
            }
            #else
            arctopClient?.CreateArctopSDK();
            #endif
        }
        
        protected void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
        {
            Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
        }
        protected void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
            if (permissionName.Equals(ArctopSDK.ARCTOP_PERMISSION))
            {
                if (createSDKOnPermissionGranted)
                {
                    arctopClient?.CreateArctopSDK();
                }
            }
        }
        protected void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
        }
    }
}