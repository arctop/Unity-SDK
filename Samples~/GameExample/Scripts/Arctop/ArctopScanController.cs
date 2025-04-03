using System;
using com.arctop;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ArctopScanController : MonoBehaviour
{
    [SerializeField] private ArctopNativeClient m_ArctopClient;
    [SerializeField] private GameObject[] m_DeviceButtons;
    [SerializeField] private TMP_Text m_ConnectionDisplay;
    [SerializeField] private UnityEvent m_OnDeviceConnected;
    [SerializeField] private GameObject m_infoText;
    [SerializeField] private string nextSceneName;

    void Update(){
        if (clearTextTimer < 0){
            m_ConnectionDisplay.text = "";
        }

        clearTextTimer -= Time.deltaTime;
    }
    private float clearTextTimer = 0;

    public void OnScanClicked()
    {
        m_ArctopClient.ScanForDevices();
        m_infoText.SetActive(false);
    }

    public void OnDeviceButtonClicked(TMP_Text text)
    {
        m_ArctopClient.ConnectToDeviceId(text.text);
    }

    public void OnScanResult(string[] devices)
    {
        for (int i = 0; i < m_DeviceButtons.Length; i++)
        {
            if (i < devices.Length)
            {
                var text = m_DeviceButtons[i].GetComponentInChildren<TMP_Text>();
                text.text = devices[i];
                m_DeviceButtons[i].SetActive(true);
            }
            else
            {
                m_DeviceButtons[i].SetActive(false);
            }
        }
    }

    public void OnPredictionStartSuccess()
    {
        // go to next scene
        SceneManager.LoadScene(nextSceneName);
    }

    public void OnPredictionStartFailed()
    {
        m_ConnectionDisplay.text = "Failed to start prediction";
        clearTextTimer = 3f;
    }

    public void OnConnectionStatusChanged(ArctopSDK.ConnectionState prev, ArctopSDK.ConnectionState curr)
    {
        switch (curr)
        {
            case ArctopSDK.ConnectionState.Unknown:
                break;
            case ArctopSDK.ConnectionState.Connecting:
                m_ConnectionDisplay.text = "Connecting...";
                break;
            case ArctopSDK.ConnectionState.Connected:
                m_ConnectionDisplay.text = "Connected!";
                m_OnDeviceConnected.Invoke();
                break;
            case ArctopSDK.ConnectionState.ConnectionFailed:
                m_ConnectionDisplay.text = "Connection Failed";
                break;
            case ArctopSDK.ConnectionState.Disconnected:
                m_ConnectionDisplay.text = "Connection Lost";
                break;
            case ArctopSDK.ConnectionState.DisconnectedUponRequest:
                break;
        }

        clearTextTimer = 3f;
    }
}