using System;
using System.Collections.Generic;
using System.Linq;
using com.arctop;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArctopRealtimeController : MonoBehaviour
{
    [SerializeField] private TMP_Text m_TextField;
    [SerializeField] private TMP_Text m_QAStatusLine;
    [SerializeField] private GameObject m_finishButton;
    [SerializeField] private GameObject m_exitButton;
    private Dictionary<string, float> m_values = new Dictionary<string, float>();
    private List<string> m_textList = new List<string>();
    public void OnValueChanged(string key, float value)
    {
        m_values[key] = value;
    }

    public void ExitToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    private void LateUpdate()
    {
        m_textList.Clear();
        foreach (var kvp in m_values)
        {
            m_textList.Add($"{kvp.Key} : {kvp.Value}");
        }
        m_TextField.text = string.Join('\n', m_textList);
    }

    public void OnQAStatus(bool passed, ArctopSDK.QAFailureType type)
    {
        m_QAStatusLine.text = $"QA: {(passed ? "Passed" : $"Failed Reason {type.ToString()}")}";
    }
    
    public void OnPredictionEnd()
    {
        m_finishButton.SetActive(false);
        m_QAStatusLine.text = "Session completed successfully";
    }

    public void OnSessionComplete(bool success)
    {
        m_QAStatusLine.text = $"Session completed {(success ? "successfully" : "but had errors uploading")}";
        m_exitButton.SetActive(true);
    }
}