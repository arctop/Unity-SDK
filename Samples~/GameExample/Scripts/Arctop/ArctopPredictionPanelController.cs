using System;
using System.Collections.Generic;
using System.Linq;
using com.arctop;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ArctopPredictionPanelController : MonoBehaviour
{
    [SerializeField] private Button m_startButton;
    [SerializeField] private GameObject[] predictionToggles;
    [SerializeField] private UnityEvent<string[]> setupPredictions;
    [SerializeField] private GameObject m_noCalibratedMessage;
    private List<Toggle> m_toggles;
    private ArctopSDK.ArctopPredictionData[] m_calibratedPredictions;
    public void SetupPredictions(ArctopSDK.ArctopPredictionData[] calibratedPredictions)
    {
        bool hasOne = false;
        m_calibratedPredictions = calibratedPredictions;
        for (int i = 0; i < predictionToggles.Length; i++)
        {
            if (m_calibratedPredictions.Length > i)
            {
                var text = predictionToggles[i].GetComponentInChildren<Text>();
                text.text = m_calibratedPredictions[i].predictionTitle;
                predictionToggles[i].SetActive(true);
                hasOne = true;
            }
            else
            {
                predictionToggles[i].SetActive(false);
            }
        }

        if (!hasOne)
        {
            m_noCalibratedMessage.gameObject.SetActive(true);
        }
    }

    public void OnStartButtonClicked()
    {
        var list = new List<string>();
        for (int i = 0; i < m_calibratedPredictions.Length; i++)
        {
            if (m_toggles[i].isOn)
            {
                list.Add(m_calibratedPredictions[i].predictionId);
            }
        }
        setupPredictions?.Invoke(list.ToArray());
    }
    
    private void Start()
    {
        m_toggles = new List<Toggle>();
        foreach (var go in predictionToggles)
        {
            m_toggles.Add(go.GetComponentInChildren<Toggle>());
        }
    }

    public void onPredictionToggle(bool isOn)
    {
        m_startButton.interactable = isOn || m_toggles.Any(x => x.isOn);
    }
}