using System.Collections.Generic;
using UnityEngine;

public class AssessmentTracker : MonoBehaviour
{
    public static AssessmentTracker Instance { get; private set; }

    public List<ResponseData> Responses { get; private set; } = new List<ResponseData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterAnswer(string questionId, string choiceType)
    {
        Responses.Add(new ResponseData
        {
            questionId = questionId,
            choiceType = choiceType
        });

        Debug.Log($"[AssessmentTracker] {questionId} -> {choiceType}");
    }

    public void ResetAll()
    {
        Responses.Clear();
    }
}
