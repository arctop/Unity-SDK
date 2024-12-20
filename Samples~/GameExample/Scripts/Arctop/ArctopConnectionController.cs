using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using com.arctop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArctopConnectionController : MonoBehaviour
{
    [SerializeField] private ArctopNativeClient m_ArctopClient;
    [SerializeField] private TMP_InputField m_otpField;
    [SerializeField] private GameObject m_LoginPanel;
    [SerializeField] private GameObject m_ScanPanel;
    [SerializeField] private GameObject m_SplashPanel;
    [SerializeField] private TMP_Text m_MessagePanel;
    [SerializeField] private Button startPredictionButton;
    [SerializeField] private ArctopSDK.Predictions[] m_PredictionsToStart = {ArctopSDK.Predictions.ZONE};
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
            m_MessagePanel.text = "Verifying Calibrations...";
            m_ArctopClient.GetUserCalibrationStatus();
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
            m_ArctopClient.GetUserCalibrationStatus();
        }
        else
        {
            m_MessagePanel.text = "Login failed. Please try again.";
        }
        clearTextTimer = 3f;
        m_LoginPanel.SetActive(!success);
    }

    public void OnUserCalibrationStatus(ArctopSDK.UserCalibrationStatus status)
    {
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
                m_ScanPanel.SetActive(true);
                break;
            case ArctopSDK.UserCalibrationStatus.Blocked:
                clearTextTimer = 100f;
                m_MessagePanel.text = "User interaction is blocked. Please contact support";
                break;
        }
    }

    public void DisablePredictionButton(){
        startPredictionButton.interactable = false;
    }

    public void StartPrediction()
    {
        m_ArctopClient.StartMultiplePredictions(m_PredictionsToStart);
    }
}