using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using com.arctop;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ArctopConnectionController : MonoBehaviour
{
    [SerializeField] private ArctopNativeClient m_ArctopClient;
    [SerializeField] private TMP_InputField m_otpField;
    [SerializeField] private GameObject m_LoginPanel;
    [SerializeField] private GameObject m_ScanPanel;
    [SerializeField] private GameObject m_SplashPanel;
    [SerializeField] private GameObject m_PredictionsPanel;
    [SerializeField] private GameObject m_permissionsPanel;
    [SerializeField] private GameObject m_requestPermissionText;
    [SerializeField] private GameObject m_permissionDeniedText;
    [SerializeField] private TMP_Text m_MessagePanel;
    [SerializeField] private Button startPredictionButton;
    [SerializeField] private UnityEvent<ArctopSDK.ArctopPredictionData[]> OnCalibratedPredictions;
    private ArctopSDK.UnityPredictionIds m_AvailablePredictions;
    private ArctopSDK.ArctopPredictionData[] m_predictionsData;
    private string[] m_PredictionsToStart;
    private float clearTextTimer = 0;

    private void Start()
    {
        // Android currently logs you in from a separate activity, so we don't need the otp code field here
#if UNITY_ANDROID
        m_otpField.gameObject.SetActive(false);
#endif
    }

    void Update(){
        if (clearTextTimer < 0){
            m_MessagePanel.text = "";
        }

        clearTextTimer -= Time.deltaTime;
    }

    public void SDKInitCallback(bool success)
    {
        if (success)
        {
            clearTextTimer = 5f;
            m_MessagePanel.text = "Checking User's status";
            m_ArctopClient.CheckUserLoggedIn();
        }
        else
        {
            clearTextTimer = 10000000f;
            m_MessagePanel.text = "SDK Init Failed";
        }
        m_SplashPanel.SetActive(false);
    }

	public void SetInfoMessage(String text)
	{
		clearTextTimer = 5f;
        m_MessagePanel.text = text;
	}

    public void OnLoginButtonClicked()
    {
        m_ArctopClient.LoginUser(m_otpField.text);
    }

    public void OnLoginCheckResponse(bool success)
    {
        if (success)
        {
            m_MessagePanel.text = "Loading predictions...";
            m_ArctopClient.GetAvailablePredictions();
            
        }
        clearTextTimer = 3f;
        m_LoginPanel.SetActive(!success);
    }

    public void OnLoginResponse(bool success)
    {
        Debug.Log($"OnLoginResponse {success}");
        if (success)
        {
            m_MessagePanel.text = "User Logged in!";
            m_ArctopClient.GetAvailablePredictions();
        }
        else
        {
            m_MessagePanel.text = "Login failed. Please try again.";
        }
        clearTextTimer = 3f;
        m_LoginPanel.SetActive(!success);
    }

    public void OnAvailablePredictions(ArctopSDK.UnityPredictionIds preds)
    {
        m_AvailablePredictions = preds;
        m_MessagePanel.text = "Verifying Calibrations...";
        m_ArctopClient.GetUserPredictionDataStatus();
    }

    public void OnUserPredictionData(ArctopSDK.ArctopPredictionData[] statusArray)
    {
        m_predictionsData = statusArray;
        var onlyCalibrated =
            m_predictionsData.Where(x =>
                x.calibrationStatus == ArctopSDK.UserCalibrationStatus.ModelsAvailable).ToArray();
        OnCalibratedPredictions?.Invoke(onlyCalibrated);
        m_PredictionsPanel.SetActive(true);
    }

    public void OnUserCalibrationStatus(ArctopSDK.UserCalibrationStatus status)
    {
        /*
            TODO: This is currently rigged only for android. this is legacy and for backwards compatibility only
        */
        #if UNITY_ANDROID
        switch (status)
        {
            case ArctopSDK.UserCalibrationStatus.NeedsCalibration:
                clearTextTimer = 30f;
                m_MessagePanel.text = "User is not calibrated. Please use the Arctop app to calibrate.";
                break;
            case ArctopSDK.UserCalibrationStatus.CalibrationDone:
                clearTextTimer = 30f;
                m_MessagePanel.text = "User models are being calibrated. Please check again in a few minutes.";
                break;
            case ArctopSDK.UserCalibrationStatus.ModelsAvailable:
                clearTextTimer = 3f;
                m_MessagePanel.text = "User models available!";
                // setup the prediction here.
                m_PredictionsToStart = new[] { ArctopSDK.ZONE_PREDICTION };
                m_ScanPanel.SetActive(true);
                break;
            case ArctopSDK.UserCalibrationStatus.Blocked:
                clearTextTimer = 100f;
                m_MessagePanel.text = "User interaction is blocked. Please contact support";
                break;
        }
        #endif
    }

    public void DisablePredictionButton(){
        startPredictionButton.interactable = false;
    }

    public void SetPredictionsToStart(string[] ids)
    {
        m_PredictionsToStart = ids;
        m_PredictionsPanel.SetActive(false);
        if (allPredictionsHavePermissions())
        {
            m_ScanPanel.SetActive(true);
        }
        else
        {
            ShowPermissionRequestPanel();
        }
    }

    private bool allPredictionsHavePermissions()
    {
        return m_predictionsData.Where(x => m_PredictionsToStart.Contains(x.predictionId))
            .All(x => x.predictionPermissionStatus);
    }

    private void ShowPermissionDeniedPanel()
    {
        m_requestPermissionText.SetActive(false);
        m_permissionDeniedText.SetActive(true);
        m_permissionsPanel.SetActive(true);
    }

    private void ShowPermissionRequestPanel()
    {
        m_requestPermissionText.SetActive(true);
        m_permissionDeniedText.SetActive(false);
        m_permissionsPanel.SetActive(true);
    }

    public void OnPermissionRequestResponse(ArctopSDK.PermissionRequestResult response)
    {
        switch (response)
        {
            case ArctopSDK.PermissionRequestResult.Denied:
                ShowPermissionDeniedPanel();
                break;
            case ArctopSDK.PermissionRequestResult.Customized:
                if (allPredictionsHavePermissions())
                {
                    m_permissionsPanel.SetActive(false);
                    m_ScanPanel.SetActive(true);
                }
                else
                {
                    ShowPermissionDeniedPanel();
                }
                break;
            case ArctopSDK.PermissionRequestResult.FailedNoUserDataAvailable:
                // TODO: Handle this error
                break;
            case ArctopSDK.PermissionRequestResult.AllPredictionsAlreadyGranted:
            case ArctopSDK.PermissionRequestResult.Granted:
                m_permissionsPanel.SetActive(false);
                m_ScanPanel.SetActive(true);
                break;
        }
    }
    public void StartPrediction()
    {
        ArctopSDK.UnityPredictionIds ids = new ArctopSDK.UnityPredictionIds
        {
            predictionsIds = m_PredictionsToStart
        };
        m_ArctopClient.StartMultiplePredictions(ids);
    }
}