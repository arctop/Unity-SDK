using UnityEngine;
using System.Runtime.InteropServices;
using AOT;

namespace com.arctop
{
    public class ArctopNativePlugin
    {
        // delegate from success
        public delegate void SuccessCallback();
        // delegate for success with an int return
        public delegate void SuccessWithIntCallback(int status);
        // delegate for failure
        public delegate void FailureWithCodeCallback(int error);
        // delegate for is user logged in
        public delegate void IsUserLoggedInCallback(bool loggedIn);
        // delegate for scan result
        public delegate void ScanResultCallback(string deviceId);
        // delegate for connection status
        public delegate void ConnectionStatusCallback(int previous, int current);
        // delegate for realtime values
        public delegate void ValueChangedCallback(string key, float value);
        // delegate for qa
        public delegate void QAStatusCallback(bool passed, int errorCode);
        // delegate for signalQuality
        public delegate void SignalQualityCallback(string signalQuality);
        // delegate for returning JSON of Allowed Predictions IDs
        public delegate void AllowedPredictionsDataCallback(string predictionData);
        // delegate for returning JSON of ArctopPredictionData
        public delegate void UserPredictionDataCallback(string predictionData);
#if UNITY_IOS
        [DllImport("__Internal")]
        public static extern void arctopSDKInit(string apiKey, string bundleId, SuccessCallback onSuccess, FailureWithCodeCallback onFailure);
        
        [DllImport("__Internal")]
        public static extern void arctopSDKShutdown();

        [DllImport("__Internal")]
        public static extern void arctopSDKLogin(string otp, SuccessCallback onSuccess, FailureWithCodeCallback onFailure);

        [DllImport("__Internal")]
        public static extern void arctopSDKLogout(SuccessCallback onSuccess, FailureWithCodeCallback onFailure);
        
        [DllImport("__Internal")]
        public static extern void arctopSDKIsUserLoggedIn(IsUserLoggedInCallback isLoggedIn);

        [DllImport("__Internal")]
        public static extern void arctopSDKGetAvailablePredictions(AllowedPredictionsDataCallback onSuccess);

        [DllImport("__Internal")]
        public static extern void arctopSDKGetUserPredictionData(UserPredictionDataCallback onSuccess);

        [DllImport("__Internal")]
        public static extern void arctopSDKRequestPermissions(SuccessWithIntCallback onSuccess);

        [DllImport("__Internal")]
        public static extern void arctopSDKScanForDevices(ScanResultCallback callback);

        [DllImport("__Internal")]
        public static extern void arctopSDKConnectToDeviceId(string deviceId);

        [DllImport("__Internal")]
        public static extern void arctopSDKDisconnectDevice();
        
        [DllImport("__Internal")]
        public static extern void arctopSDKStartPredictions(string predictionId, SuccessCallback onSuccess,
            FailureWithCodeCallback onFailure);
        
        [DllImport("__Internal")]
        public static extern void arctopSDKStartMultiplePredictions(string predictionsIds, SuccessCallback onSuccess,
            FailureWithCodeCallback onFailure);

        [DllImport("__Internal")]
        public static extern void arctopSDKEndPrediction(SuccessCallback onSuccess,
            FailureWithCodeCallback onFailure, bool disconnectDevice = true);

        [DllImport("__Internal")]
        public static extern void arctopSDKWriteUserMarker(string markerData);
        
        [DllImport("__Internal")]
        public static extern void arctopSDKSetConnectionCallback(ConnectionStatusCallback connectionStatusCallback);

        [DllImport("__Internal")]
        public static extern void arctopSDKSetQAStatusCallback(QAStatusCallback qaStatusCallback);

        [DllImport("__Internal")]
        public static extern void arctopSDKSetValueChangedCallback(ValueChangedCallback valueChangedCallback);
        
        [DllImport("__Internal")]
        public static extern void arctopSDKSetSignalQualityCallback(SignalQualityCallback signalQualityCallback);
#elif UNITY_ANDROID
        // Using constants here for since it compares easier to the Native side.
        // The ArctopNativeClient will send out proper enums though instead.
        private const int UserLoggedIn = 1;
        private const int Success = 0;
        // C# side representation of the Java API bridge
        private static AndroidJavaObject mArctopSdkBridge;
        // In Android, we first need to bind to the service, and only then can we start working with it
        // This includes initialization. So there is an extra call here to create.
        public static void arctopSDKCreate(SuccessCallback onSuccess, FailureWithCodeCallback onFailure)
        {
            AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            mArctopSdkBridge = new AndroidJavaObject("com.arctop.unity.ArctopUnityBridge");
            var callbacks = new ArctopServiceBindCallback();
            callbacks.OnBindSuccess += () =>
            {
                onSuccess();
            };
            callbacks.OnBindFailure += (error) =>
            {
#if DEVELOPMENT_BUILD
                Debug.Log($"Bind on failure called with {error}");
#endif
                onFailure((int)error);
            };
            // Before anything, we need the context of the activity, so this is the entry point,
            // after which internally, the native bridge will bind, and create the service
            mArctopSdkBridge.Call("setUnityActivity", unityActivity,callbacks);
        }
        // We set up a callback delegate here. All responses from the service are directed to this delegate
        public static void setSDKCallback(ArctopSDKCallback callback)
        {
            mArctopSdkBridge?.Call("setSdkCallback", callback);
        }
        // once bound, we can initialize the SDK with the api key
        public static void arctopSDKInit(string apiKey, SuccessCallback onSuccess, FailureWithCodeCallback onFailure)
        {
            var response = mArctopSdkBridge.Call<int>("arctopSDKInit" , apiKey);
            switch (response)
            {
                case Success:
                    onSuccess();
                    break;
                default:
                    onFailure(response);
                    break;
            }
        }
        // Shuts down the SDK. After this call, the entire loop (create -> set callback -> init) needs to happen
        // if you want to use the SDK again.
        public static void arctopSDKShutdown()
        {
            mArctopSdkBridge.Call("arctopSDKShutdown");
        }
       
        // Launches an activity that will log the user into the arctop app
        // Normally, there is no "failure" callback, but this is here for future-proof
        public static void arctopSDKLogin(SuccessCallback onSuccess,
            FailureWithCodeCallback onFailure)
        {
            var callback = new ArctopSDKSuccessOrFailureCallback();
            callback.OnSuccess += () =>
            {
#if DEVELOPMENT_BUILD
                Debug.Log("login success");
#endif
                onSuccess();
            };
            callback.OnFailure += (error) =>
            {
#if DEVELOPMENT_BUILD
                Debug.Log($"login failure called with {error}");
#endif
                onFailure(error);
            };
            mArctopSdkBridge.Call("arctopLaunchLogin",callback);
        }

        public static void arctopSDKLogout(SuccessCallback onSuccess,FailureWithCodeCallback onFailure)
        {
            var response = mArctopSdkBridge.Call<int>("arctopSDKLogoutUser");
            switch (response)
            {
                case >= 0:
                    onSuccess();
                    break;
                default:
                    onFailure(response);
                    break;
            }
        }

        // Checks if the user is logged into the arctop servers.
        public static void arctopSDKIsUserLoggedIn(IsUserLoggedInCallback isLoggedIn)
        {
            var response = mArctopSdkBridge.Call<int>("arctopSDKIsUserLoggedIn");
#if DEVELOPMENT_BUILD
            Debug.Log($"Response : {response}");
#endif
            isLoggedIn(response == UserLoggedIn);
        }
        
        // checks the user's calibration status
        public static void arctopSDKGetUserCalibrationStatus(SuccessWithIntCallback onSuccess,
            FailureWithCodeCallback onFailure)
        {
            var response = mArctopSdkBridge.Call<int>("arctopSDKGetUserCalibrationStatus");
            switch (response)
            {
                case >= 0:
                    onSuccess(response);
                    break;
                default:
                    onFailure(response);
                    break;
            }
        }
        
        // Calls the SDK to scan for sensor devices that can be used with the SDK
        public static void arctopSDKScanForDevices()
        {
            mArctopSdkBridge.Call("arctopSDKScanForDevices");
        }
        
        // Connects to a device ID that was received from the scan callback by name
        public static void arctopSDKConnectToDeviceId(string deviceId)
        {
            mArctopSdkBridge.Call("arctopSDKConnectToDeviceId" , deviceId);
        }
        
        // Disconnects the currently connected device
        public static void arctopSDKDisconnectDevice()
        {
            mArctopSdkBridge.Call("arctopSDKDisconnectDevice");
        }

        // Starts the process of a realtime prediction 
        public static void arctopSDKStartPredictions(string predictionName, SuccessCallback onSuccess,
            FailureWithCodeCallback onFailure)
        {
            var response = mArctopSdkBridge.Call<int>("arctopSDKStartPredictions",predictionName);
            switch (response)
            {
                case >= 0:
                    onSuccess();
                    break;
                default:
                    onFailure(response);
                    break;
            }
        }
        
        public static void arctopSDKStartMultiplePredictions(string predictionsNames, SuccessCallback onSuccess,
            FailureWithCodeCallback onFailure)
        {
            var response = mArctopSdkBridge.Call<int>("arctopSDKStartMultiplePredictions",predictionsNames);
            switch (response)
            {
                case >= 0:
                    onSuccess();
                    break;
                default:
                    onFailure(response);
                    break;
            }
        }
        
        

        // Stops the currently running prediction. This will also disconnect the device and upload the data to the server
        public static void arctopSDKEndPrediction(SuccessCallback onSuccess,
            FailureWithCodeCallback onFailure)
        {
            var response = mArctopSdkBridge.Call<int>("arctopSDKEndPrediction");
            switch (response)
            {
                case >= 0:
                    onSuccess();
                    break;
                default:
                    onFailure(response);
                    break;
            }
        }

        // Writes a user marker at the current timestamp with the markerData provided
        public static void arctopSDKWriteUserMarker(string markerData)
        {
            mArctopSdkBridge.Call("arctopSDKWriteUserMarker",markerData);
        }
        
        #endif

    }
}