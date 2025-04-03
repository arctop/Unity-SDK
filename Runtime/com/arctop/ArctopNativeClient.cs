using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AOT;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace com.arctop
{
    /// <summary>
    /// Arctop Native Client. This is the gateway between Unity and The Native Arctop SDK
    /// </summary>
    public class ArctopNativeClient : MonoBehaviour
    {
        #region Inspector Variables and Callbacks
        /// <summary>
        /// Each client requires an API key from Arctop before being able to initialize and operate.
        /// Enter your key here.
        /// </summary>
        [SerializeField] private string m_ApiKey;
        /// <summary>
        /// Should this component initialize the SDK on Awake?
        /// </summary>
        [SerializeField] private bool HandleSDKInit;
        /// <summary>
        /// Should this component shutdown and release the SDK on Destroy?
        /// </summary>
        [SerializeField] private bool HandleSDKDestroy;
        
        // Debug Only Section, allows for testing of different functionalities in editor
        
        /// <summary>
        /// Should this component simulate realtime values
        /// </summary>
        [Header("In Editor Debug Only")]
        [SerializeField]
        private bool m_SimulateRealtime;
        /// <summary>
        /// Should this component simulate signal quality values in editor
        /// </summary>
        [SerializeField]
        private bool m_SimulateSignalQuality;
        /// <summary>
        /// Determines the response in editor to a call for UserLoginStatus
        /// </summary>
        [SerializeField] 
        private ArctopSDK.LoginStatus m_SimulatedUserLoggedInResponse;
        /// <summary>
        /// Determines the response in editor for CheckUserCalibrationStatus
        /// </summary>
        [SerializeField] 
        private ArctopSDK.UserCalibrationStatus m_SimulatedUserCalibrationResponse;
        /// <summary>
        /// Should a call to LoginUser in editor succeed or fail
        /// </summary>
        [SerializeField]
        private bool m_LoginSuccessfullyInEditor = true;
        /// <summary>
        /// Should the call to LogoutUser in editor succeed or fail
        /// </summary>
        [SerializeField]
        private bool m_LogoutSuccessfullyInEditor = true;
        /// <summary>
        /// In Editor headband connection status
        /// </summary>
        [SerializeField] [Tooltip("Allows you to change the connection status in editor")]
        private ArctopSDK.ConnectionState m_currentDebugConnection = ArctopSDK.ConnectionState.Unknown;

        [SerializeField] [Tooltip("Setup your debug data here")]
        private ArctopSDK.ArctopPredictionData[] m_debugPredictionData = new []
        {
            new ArctopSDK.ArctopPredictionData{
             PredictionId = "id1",
             PredictionName = "Fake Prediction 1",
             CalibrationStatus = ArctopSDK.UserCalibrationStatus.ModelsAvailable,
             iconKey = String.Empty
            },
            new ArctopSDK.ArctopPredictionData{
                PredictionId = "id2",
                PredictionName = "Fake Prediction 2",
                CalibrationStatus = ArctopSDK.UserCalibrationStatus.ModelsAvailable,
                iconKey = String.Empty
            },
            new ArctopSDK.ArctopPredictionData{
                PredictionId = "id3",
                PredictionName = "Fake Prediction 3",
                CalibrationStatus = ArctopSDK.UserCalibrationStatus.ModelsAvailable,
                iconKey = String.Empty
            },
        };
        // Response events. Use these to react to SDK messages
        
        /// <summary>
        /// Called when the user has successfully logged in after a call to LoginUser
        /// </summary>
        [Header("Events")]
        [SerializeField] private UnityEvent OnUserLoggedIn;
        /// <summary>
        /// Called when the user login failed
        /// </summary>
        [SerializeField] private UnityEvent<ArctopSDK.ResponseCodes> OnUserLoginFailed;
        /// <summary>
        /// Callback for UserLoggedInStatus
        /// </summary>
        [SerializeField] private UnityEvent<bool> IsUserLoggedIn;
        /// <summary>
        /// Callback for successfully logging out a user
        /// </summary>
        [SerializeField] private UnityEvent OnUserLogout;
        /// <summary>
        /// Callback for failed call to LogoutUser
        /// </summary>
        [SerializeField] private UnityEvent<ArctopSDK.ResponseCodes> OnUserLogoutFailed;
        /// <summary>
        /// Callback to receive the list of available predictions
        /// </summary>
        [SerializeField] private UnityEvent<ArctopSDK.UnityPredictionIds> OnAvailablePredictions;
        /// <summary>
        /// Callback for user calibration status
        /// </summary>
        [SerializeField] private UnityEvent<ArctopSDK.UserCalibrationStatus> OnCalibrationStatus;
        /// <summary>
        /// Callback for user calibration status when asking for multiple prediction s
        /// </summary>
        [SerializeField] private UnityEvent<ArctopSDK.ArctopPredictionData[]> OnCalibrationsStatus;
        /// <summary>
        /// Callback when an error occured trying to retrieve user calibration status
        /// </summary>
        [SerializeField] private UnityEvent<ArctopSDK.ResponseCodes> OnCalibrationStatusError;
        /// <summary>
        /// Callback with the new device list containing all discoverd headbands
        /// </summary>
        [SerializeField] private UnityEvent<string[]> OnDeviceListUpdated;
        /// <summary>
        /// Callback for a successful initialization of SDK
        /// </summary>
        [SerializeField] private UnityEvent OnSDKInitSuccess;
        /// <summary>
        /// Callback when an error occured trying to initialize the SDK
        /// </summary>
        [SerializeField] private UnityEvent<ArctopSDK.ResponseCodes> OnSDKInitFailed;
        /// <summary>
        /// Callback for a successful prediction start
        /// </summary>
        [SerializeField] private UnityEvent OnPredictionStart;
        /// <summary>
        /// Callback when an error occured trying to start a prediction
        /// </summary>
        [SerializeField] private UnityEvent<ArctopSDK.ResponseCodes> OnPredictionStartFailed;
        /// <summary>
        /// Callback for when the headband connection status has changed.
        /// 1st parameter is the previous connection status
        /// 2nd parameter is the current connection status
        /// </summary>
        [SerializeField] private UnityEvent<ArctopSDK.ConnectionState, ArctopSDK.ConnectionState> OnConnectionStateChanged;
        /// <summary>
        /// Callback for a realtime value receieved from the prediction.
        /// 1st parameter is the name of the value
        /// 2nd parameter is the value
        /// </summary>
        [SerializeField] private UnityEvent<string, float> OnRealtimeValue;
        /// <summary>
        /// Callback for realtime QA status.
        /// 1st parameter is QA passed / failed
        /// 2nd parameter is the QA error in the case of a failure
        /// </summary>
        [SerializeField] private UnityEvent<bool, ArctopSDK.QAFailureType> OnQAStatus;
        /// <summary>
        /// Callback for a successful prediction end
        /// </summary>
        [SerializeField] private UnityEvent OnPredictionEnd;
        /// <summary>
        /// Callback when an error occured trying to end a prediction
        /// </summary>
        [SerializeField] private UnityEvent<ArctopSDK.ResponseCodes> OnPredictionEndFailed;
        /// <summary>
        /// Callback with signal quality values
        /// </summary>
        [SerializeField] private UnityEvent<string> OnSignalQuality;
        /// <summary>
        /// Callback for session complete
        /// </summary>
        [SerializeField] private UnityEvent OnSessionComplete;

        /// <summary>
        /// Android only, callback for when an error binding to the service occured
        /// </summary>
        [Header("Android only Events")]
        [SerializeField] private UnityEvent<ArctopSDK.BindError> OnSDKCreateFailed;
        #endregion
        
        #region Public API

        /// <summary>
        /// Creates the SDK instance.
        /// On Android -> Binds to the SDK Service
        /// On iOS -> Initializes the SDK
        /// In Editor -> Calls on SDK Init callback
        /// </summary>
        public void CreateArctopSDK()
        {
#if UNITY_EDITOR
            onSDKInit();
#elif UNITY_IOS
            ArctopNativePlugin.arctopSDKInit(m_ApiKey ,Application.identifier, onSDKInit, onSDKInitFailed);
#elif UNITY_ANDROID
            ArctopNativePlugin.arctopSDKCreate(onSDKCreate, onSDKCreateFailed);
#endif
        }
        
        /// <summary>
        /// Checks if there is a user logged into the system
        /// </summary>
        public void CheckUserLoggedIn()
        {
#if UNITY_EDITOR
            onUserLoggedInCheck(m_SimulatedUserLoggedInResponse == ArctopSDK.LoginStatus.LoggedIn);
#elif UNITY_IOS || UNITY_ANDROID
            ArctopNativePlugin.arctopSDKIsUserLoggedIn(onUserLoggedInCheck);
#endif
        }
        
        /// <summary>
        /// Gets the current user's calibration status for this prediction
        /// </summary>
        public void GetUserCalibrationStatus(string predictionId)
        {
#if UNITY_EDITOR
            AddAction(() => { instance.OnCalibrationStatus.Invoke(m_SimulatedUserCalibrationResponse); });
#elif UNITY_IOS
            ArctopNativePlugin.arctopSDKGetUserCalibrationStatus(predictionId , onCalibrationStatusSuccess);
#elif UNITY_ANDROID 
            // TODO: This will need to be adjusted for android 1.14
            ArctopNativePlugin.arctopSDKGetUserCalibrationStatus(onCalibrationStatusSuccess, onCalibrationStatusFailure );
#endif
        }
        
        /// <summary>
        /// Gets the current user's calibration status for multiple predictions
        /// </summary>
        public void GetUserCalibrationsStatus(ArctopSDK.UnityPredictionIds forPredictionsIds)
        {
 #if UNITY_EDITOR
            #if UNITY_IOS
            var array = new ArctopSDK.ArctopPredictionDataArray
            {
                
                dataArray = m_debugPredictionData
            };
            onCalibrationsStatusSuccess(JsonUtility.ToJson(array));
            #elif UNITY_ANDROID
            GetUserCalibrationStatus(ArctopSDK.ZONE_PREDICTION);
            #endif
#elif UNITY_IOS
            var jsonData = JsonUtility.ToJson(forPredictionsIds);
            ArctopNativePlugin.arctopSDKGetUserCalibrationsStatus(jsonData, onCalibrationsStatusSuccess);
#elif UNITY_ANDROID
            //TODO: Current workaround for android until we update to 1.14
            GetUserCalibrationStatus(forPredictionsIds.predictionsIds[0]);
#endif
        }
        
        public void GetAvailablePredictions()
        {
#if UNITY_EDITOR
            var ids = new string[m_debugPredictionData.Length];
            for (int i = 0; i < m_debugPredictionData.Length; i++)
            {
                ids[i] = m_debugPredictionData[i].PredictionId;
            }
            var data = new ArctopSDK.UnityPredictionIds
            {
                predictionsIds = ids
            };
            onAvailablePredictions(JsonUtility.ToJson(data));
#elif UNITY_IOS
            ArctopNativePlugin.arctopSDKGetAvailablePredictions(onAvailablePredictions);
#elif UNITY_ANDROID
            //TODO: Temp workaround for Android until we upgrade it to 1.14
            // we aren't calling native here, just returning this, since technically it's the same 
            // calibration status we used to have, so you can run either.
            var predictions = new ArctopSDK.UnityPredictionIds
            {
                predictionsIds = new string[]{ ArctopSDK.ZONE_PREDICTION }
            };
            onAvailablePredictions(JsonUtility.ToJson(predictions));
#endif
        }
        
        /// <summary>
        /// Logs a user in using either OTP (iOS) or launching the login page in Android
        /// </summary>
        /// <param name="otp"></param>
        public void LoginUser(string otp = "")
        {
#if UNITY_EDITOR
            if (m_LoginSuccessfullyInEditor)
            {
                m_SimulatedUserLoggedInResponse = ArctopSDK.LoginStatus.LoggedIn;
                onLoginSuccess();
            }
            else
            {
                m_SimulatedUserLoggedInResponse = ArctopSDK.LoginStatus.NotLoggedIn;
                onLoginFailed((int)ArctopSDK.ResponseCodes.UnknownError);
            }
#elif UNITY_IOS 
            ArctopNativePlugin.arctopSDKLogin(otp,onLoginSuccess,onLoginFailed);
#elif UNITY_ANDROID
            ArctopNativePlugin.arctopSDKLogin(onLoginSuccess,onLoginFailed);
#endif
        }
        
        /// <summary>
        /// Logs the user out of the application
        /// </summary>
        public void LogoutUser()
        {
#if UNITY_EDITOR
            if (m_LogoutSuccessfullyInEditor)
            {
                onUserLogout();
                m_SimulatedUserLoggedInResponse = ArctopSDK.LoginStatus.NotLoggedIn;
            }
            else
            {
                onUserLogoutFailed((int)ArctopSDK.ResponseCodes.UnknownError);
            }
#elif UNITY_IOS || UNITY_ANDROID
            ArctopNativePlugin.arctopSDKLogout(onUserLogout,onUserLogoutFailed);
#endif
        }
        
        /// <summary>
        /// Begins a scan for headband devices 
        /// </summary>
        public void ScanForDevices()
        {
#if UNITY_EDITOR
            StartCoroutine(scanForMockDevices());
#elif UNITY_IOS
            ArctopNativePlugin.arctopSDKScanForDevices(onDeviceDetected);
#elif UNITY_ANDROID
            ArctopNativePlugin.arctopSDKScanForDevices();
#endif
        }
        
        /// <summary>
        /// Performs a connection to the selected device
        /// </summary>
        /// <param name="deviceId">The device ID to connect to</param>
        public void ConnectToDeviceId(string deviceId)
        {
#if UNITY_EDITOR
            StartCoroutine(connectToMockDevice());
#elif UNITY_IOS || UNITY_ANDROID
            ArctopNativePlugin.arctopSDKConnectToDeviceId(deviceId);
#endif
        }
        /// <summary>
        /// Disconnects from a currently connected headband device
        /// </summary>
        public void DisconnectDevice()
        {
#if UNITY_EDITOR
            StartCoroutine(disconnectMockDevice());
#elif UNITY_IOS || UNITY_ANDROID
            ArctopNativePlugin.arctopSDKDisconnectDevice();
#endif
        }
        
        /// <summary>
        /// Starts a realtime prediction
        /// </summary>
        /// <param name="prediction">The desired prediction id to start</param>
        public void StartPrediction(string prediction)
        {
#if UNITY_EDITOR
            onPredictionStartSuccess();
            m_lastPredictionFrame = Time.frameCount;
#elif UNITY_IOS || UNITY_ANDROID
            ArctopNativePlugin.arctopSDKStartPredictions(prediction, onPredictionStartSuccess, onPredictionStartFailed);
#endif
        }
        
        /// <summary>
        /// Starts multiple realtime predictions
        /// </summary>
        /// <param name="predictions">The desired prediction ids to start</param>
        public void StartMultiplePredictions(ArctopSDK.UnityPredictionIds predictions)
        {
#if UNITY_EDITOR
            onPredictionStartSuccess();
            m_lastPredictionFrame = Time.frameCount;
#elif UNITY_IOS
            var jsonData = JsonUtility.ToJson(predictions);
            ArctopNativePlugin.arctopSDKStartMultiplePredictions(jsonData, onPredictionStartSuccess, onPredictionStartFailed);
#elif UNITY_ANDROID
            // TODO: This needs to be updated for 1.14
            StartPrediction(predictions.predictionsIds[0]);
#endif
        }
        
        /// <summary>
        /// Finishes the active prediction
        /// </summary>
        public void FinishPrediction()
        {
#if UNITY_EDITOR
            StartCoroutine(completeInEditorSession());
#elif UNITY_IOS || UNITY_ANDROID
            ArctopNativePlugin.arctopSDKEndPrediction(onPredictionEndSuccess, onPredictionEndFailed);
#endif
        }
        
        /// <summary>
        /// Sets a user marker at the current timestamp
        /// </summary>
        /// <param name="data">Data to write along with marker</param>
        public void SetUserMarker(string data)
        {
#if UNITY_EDITOR
            Debug.Log($"Set marker {data}");
#elif UNITY_IOS || UNITY_ANDROID
            ArctopNativePlugin.arctopSDKWriteUserMarker(data);
#endif
        }
        #endregion

        #region Private Implementation
#if UNITY_ANDROID
        private ArctopSDKCallback mAndroidSDKCallback;
#endif
        private static ArctopNativeClient instance;
        private ConcurrentQueue<Action> m_actions;
        private HashSet<string> m_headbandDevices;
        private void Awake()
        {
            instance = this;
            m_actions = new ConcurrentQueue<Action>();
            m_headbandDevices = new HashSet<string>();
#if UNITY_EDITOR

#elif UNITY_IOS
            ArctopNativePlugin.arctopSDKSetConnectionCallback(onConnectionChanged);
            ArctopNativePlugin.arctopSDKSetValueChangedCallback(onValueChangedCallback);
            ArctopNativePlugin.arctopSDKSetQAStatusCallback(onQAStatusCallback);
            ArctopNativePlugin.arctopSDKSetSignalQualityCallback(onSignalQualityCallback);
#elif UNITY_ANDROID
            mAndroidSDKCallback = new ArctopSDKCallback();
#endif
            if (instance.HandleSDKInit)
            {
                CreateArctopSDK();
            }
        }
        
        
#if UNITY_ANDROID
        private static void onSDKCreate()
        {
            instance.mAndroidSDKCallback.OnConnectionStatusCallback += onConnectionChanged;
            instance.mAndroidSDKCallback.OnScanResultCallback += onDeviceDetected;
            instance.mAndroidSDKCallback.OnSignalQualityCallback += onSignalQualityCallback;
            instance.mAndroidSDKCallback.OnValueChangedCallback += onValueChangedCallback;
            instance.mAndroidSDKCallback.OnQAStatusCallback += onQAStatusCallback;
            instance.mAndroidSDKCallback.OnIsUserLoggedInCallback += onUserLoggedInCheck;
            instance.mAndroidSDKCallback.OnSessionCompleteCallback += onSessionComplete;
            
            ArctopNativePlugin.setSDKCallback(instance.mAndroidSDKCallback);
            AddAction(() => { instance.androidInitSDK(); });
        }

        private static void onSDKCreateFailed(int error)
        {
            var bindError = (ArctopSDK.BindError)Enum.ToObject(typeof(ArctopSDK.BindError), error);
            AddAction( ()=> { instance.OnSDKCreateFailed.Invoke(bindError); });
            onSDKInitFailed((int)ArctopSDK.ResponseCodes.NotInitialized);
        }
        
        private void androidInitSDK()
        {
            ArctopNativePlugin.arctopSDKInit(m_ApiKey, onSDKInit, onSDKInitFailed);
        }
        
        
#endif

#if UNITY_EDITOR

        private void LateUpdate()
        {
            if (m_SimulateRealtime)
            {
                realtimeSimulation();
            }

            if (m_SimulateSignalQuality)
            {
                signalQualitySimulation();
            }
        }
        /// <summary>
        /// Provides realtime simulation values for in editor testing
        /// </summary>
        private void realtimeSimulation()
        {
            if (m_currentDebugConnection != ArctopSDK.ConnectionState.Connected) return;
            if ((Time.frameCount - m_lastPredictionFrame) % 60 != 0) return;
            onValueChangedCallback("heart_rate" , Random.value * 60 + 60);
            onValueChangedCallback("focus" , Random.value * 60 + 40);
            onValueChangedCallback("enjoyment" , Random.value * 60 + 40);
            onQAStatusCallback(true , -1);
        }

        private float[] m_signalQualityValues = new[] { 113.0f, 113.0f, 113.0f, 113.0f };
        private int m_lastFrame = 0;
        private int m_lastPredictionFrame = 0;
        /// <summary>
        /// Provides signal quality values for in editor testing
        /// </summary>
        private void signalQualitySimulation()
        {
            if (m_currentDebugConnection == ArctopSDK.ConnectionState.Connected)
            {
                if ((Time.frameCount - m_lastFrame) % 120 != 0) return;
                for (int i = 0; i < m_signalQualityValues.Length; i++)
                {
                    m_signalQualityValues[i] -= 10.0f;
                    if (m_signalQualityValues[i] < 0)
                    {
                        m_signalQualityValues[i] = 0f;
                    }
                }

                onSignalQualityCallback(string.Join(',', m_signalQualityValues));
            }
            else
            {
                m_lastFrame = Time.frameCount;
                for (int i = 0; i < m_signalQualityValues.Length; i++)
                {
                    m_signalQualityValues[i] = 113.0f;
                }
                onSignalQualityCallback(string.Join(',', m_signalQualityValues));
            }

        }
#endif
        
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.SuccessCallback))]
        private static void onSDKInit()
        {
            AddAction( ()=> { instance.OnSDKInitSuccess.Invoke(); });
        }
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.FailureWithCodeCallback))]
        private static void onSDKInitFailed(int error)
        {
            AddAction( ()=> { instance.OnSDKInitFailed.Invoke(getResponse(error)); });
        }
        
        private void Update()
        {
            if (m_actions.TryDequeue(out var nextAction))
            {
                nextAction();
            }
        }

        private static void AddAction(Action next)
        {
            instance.m_actions.Enqueue(next);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
#if !UNITY_EDITOR
    #if UNITY_IOS
            ArctopNativePlugin.arctopSDKSetConnectionCallback(null);
            ArctopNativePlugin.arctopSDKSetValueChangedCallback(null);
            ArctopNativePlugin.arctopSDKSetQAStatusCallback(null);
            ArctopNativePlugin.arctopSDKSetSignalQualityCallback(null);
            if (HandleSDKDestroy)
            {
                ArctopNativePlugin.arctopSDKShutdown();
            }
    #elif UNITY_ANDROID
            if (HandleSDKDestroy)
            {
                ArctopNativePlugin.arctopSDKShutdown();
            }
    #endif
#endif
        }
        
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.IsUserLoggedInCallback))]
        private static void onUserLoggedInCheck(bool loggedIn)
        {
            AddAction( ()=> { instance.IsUserLoggedIn.Invoke(loggedIn); });
        }

        [MonoPInvokeCallback(typeof(ArctopNativePlugin.SuccessWithIntCallback))]
        private static void onCalibrationStatusSuccess(int status)
        {
            var response = (ArctopSDK.UserCalibrationStatus)Enum.ToObject(typeof(ArctopSDK.UserCalibrationStatus), status);
            AddAction(() => { instance.OnCalibrationStatus.Invoke(response); });
        }
        
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.FailureWithCodeCallback))]
        private static void onCalibrationStatusFailure(int error)
        {
            AddAction(() => { instance.OnCalibrationStatusError.Invoke(getResponse(error)); });
        }
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.SuccessCallback))]
        private static void onLoginSuccess()
        {
            AddAction(() => { instance.OnUserLoggedIn.Invoke(); });
        }
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.FailureWithCodeCallback))]
        private static void onLoginFailed(int error)
        {
            AddAction(() => { instance.OnUserLoginFailed.Invoke(getResponse(error)); });
        }
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.SuccessCallback))]
        private static void onUserLogout()
        {
            AddAction(() => { instance.OnUserLogout.Invoke(); });
        }
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.FailureWithCodeCallback))]
        private static void onUserLogoutFailed(int error)
        {
            AddAction(() => { instance.OnUserLogoutFailed.Invoke(getResponse(error)); });
        }
        
#if UNITY_EDITOR
        private IEnumerator scanForMockDevices()
        {
            yield return new WaitForSeconds(1f);
            onDeviceDetected("Mock 1");
            yield return new WaitForSeconds(0.5f);
            onDeviceDetected("Mock 2");
        }

        private IEnumerator connectToMockDevice()
        {
            yield return new WaitForSeconds(0.5f);
            /*Connecting = 1,
            Connected = 2,*/
            onConnectionChanged(0, 1);
            yield return new WaitForSeconds(0.5f);
            onConnectionChanged(1, 2);
        }
        private IEnumerator disconnectMockDevice()
        {
            yield return new WaitForSeconds(0.5f);
            onConnectionChanged(2, 5);
        }

        private IEnumerator completeInEditorSession()
        {
            DisconnectDevice();
            yield return new WaitForSeconds(1f);
            onPredictionEndSuccess();
            yield return new WaitForSeconds(2f);
            onSessionComplete();
        }
#endif
        [MonoPInvokeCallback((typeof(ArctopNativePlugin.ScanResultCallback)))]
        private static void onDeviceDetected(string deviceId)
        {
            AddAction(
                () =>
                {
                    instance.m_headbandDevices.Add(deviceId);
                    instance.OnDeviceListUpdated.Invoke(instance.m_headbandDevices.ToArray());
                });
        }
        [MonoPInvokeCallback((typeof(ArctopNativePlugin.ConnectionStatusCallback)))]
        private static void onConnectionChanged(int previous,int current)
        {
            var prev = (ArctopSDK.ConnectionState)Enum.ToObject(typeof(ArctopSDK.ConnectionState), previous);
            var curr = (ArctopSDK.ConnectionState)Enum.ToObject(typeof(ArctopSDK.ConnectionState), current);
#if UNITY_EDITOR
            instance.m_currentDebugConnection = curr;
#endif
            AddAction(
                () =>
                {
                    instance.OnConnectionStateChanged.Invoke(prev, curr);
                });
        }
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.SuccessCallback))]
        private static void onPredictionStartSuccess()
        {
            AddAction(() => { instance.OnPredictionStart.Invoke(); });
        }
        
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.CalibrationsStatusCallback))]
        private static void onCalibrationsStatusSuccess(string response)
        {
            var parsedData = JsonUtility.FromJson<ArctopSDK.ArctopPredictionDataArray>(response);
            AddAction(() => { instance.OnCalibrationsStatus.Invoke(parsedData.dataArray); });
        }
        
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.CalibrationsStatusCallback))]
        private static void onAvailablePredictions(string response)
        {
            var parsed = JsonUtility.FromJson<ArctopSDK.UnityPredictionIds>(response);
            AddAction(() => { instance.OnAvailablePredictions.Invoke(parsed); });
        }
        
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.FailureWithCodeCallback))]
        private static void onPredictionStartFailed(int error)
        {
            AddAction(() => { instance.OnPredictionStartFailed.Invoke(getResponse(error)); });
        }
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.SuccessCallback))]
        private static void onPredictionEndSuccess()
        {
            AddAction(() => { instance.OnPredictionEnd.Invoke(); });
            #if UNITY_IOS
            // On iOS, prediction end successfully also marks the end of the session
            // so to maintain code flow, this is called here
            onSessionComplete();
            #endif
        }
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.FailureWithCodeCallback))]
        private static void onPredictionEndFailed(int error)
        {
            AddAction(() => { instance.OnPredictionEndFailed.Invoke(getResponse(error)); });
        }
        private static void onSessionComplete()
        {
            AddAction( ()=> { instance.OnSessionComplete.Invoke(); });
        }
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.QAStatusCallback))]
        private static void onQAStatusCallback(bool passed, int errorCode)
        {
            var code = (ArctopSDK.QAFailureType)Enum.ToObject(typeof(ArctopSDK.QAFailureType), errorCode);
            AddAction(() => { instance.OnQAStatus.Invoke(passed,code); });
        }
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.ValueChangedCallback))]
        private static void onValueChangedCallback(string key, float value)
        {
            AddAction(() => { instance.OnRealtimeValue.Invoke(key,value); });
        }
        [MonoPInvokeCallback(typeof(ArctopNativePlugin.SignalQualityCallback))]
        private static void onSignalQualityCallback(string signalQuality)
        {
            AddAction(() => { instance.OnSignalQuality.Invoke(signalQuality); });
        }
        private static ArctopSDK.ResponseCodes getResponse(int value)
        {
            return (ArctopSDK.ResponseCodes)Enum.ToObject(typeof(ArctopSDK.ResponseCodes), value);
        }
        #endregion
    }
}