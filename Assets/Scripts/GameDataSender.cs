using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class GameDataSender : MonoBehaviour
{
    public static GameDataSender Instance { get; private set; }

    private const string ApiUrl = "https://herois-da-cidadania.onrender.com/api/assessment";

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

    public IEnumerator SendAssessment()
    {
        var tracker = AssessmentTracker.Instance;
        if (tracker == null)
        {
            Debug.LogError("[GameDataSender] AssessmentTracker não encontrado.");
            yield break;
        }

        int finalScore = 0;
        if (SituationCounter.Instance != null)
            finalScore = SituationCounter.Instance.Score;

        string studentName = PlayerPrefs.GetString("Aluno_Nome", "Sem Nome");
        string schoolName  = PlayerPrefs.GetString("Aluno_Escola", "Sem Escola");
        string gradeYear   = PlayerPrefs.GetString("Aluno_Ano", "Sem Ano");

        string timestamp = DateTime.UtcNow.ToString("o"); 

        var data = new AssessmentData
        {
            studentName = studentName,
            schoolName  = schoolName,
            gradeYear   = gradeYear,
            finalScore  = finalScore,
            sentAt      = timestamp,
            responses   = new List<ResponseData>(tracker.Responses)
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log("[GameDataSender] Enviando JSON: " + json);

        using (var request = new UnityWebRequest(ApiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[GameDataSender] Enviado com sucesso: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("[GameDataSender] Erro ao enviar: " + request.error + " | " + request.downloadHandler.text);
            }
        }
    }
}
