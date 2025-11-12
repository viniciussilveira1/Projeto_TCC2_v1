using UnityEngine;
using UnityEngine.Events;
using System;

[DisallowMultipleComponent]
public class NPCDialogue : MonoBehaviour
{
    [Header("Identificação")]
    public string dialogueId;

    [Header("Descrição/Contexto")]
    [TextArea(3, 6)] public string description;

    [Header("Respostas (ordem interna: Certa, Neutra, Errada) — exibidas de forma ALEATÓRIA")]
    public string optionA; // Certa
    public string optionB; // Neutra
    public string optionC; // Errada

    [Header("Eventos opcionais ao escolher")]
    public UnityEvent onChooseCorrect;
    public UnityEvent onChooseNeutral;
    public UnityEvent onChooseWrong;

    [Header("Persistência (somente sessão)")]
    [Tooltip("ID único desta situação (gerado automaticamente)")]
    [SerializeField] private string situationId;

    [Tooltip("Raiz a ocultar ao recarregar (se Eliminada). Se vazio, usa este GameObject.")]
    [SerializeField] private GameObject hideOnReload;

    public bool IsResolved { get; private set; }
    public string SituationId => situationId;
    public GameObject HideTarget => hideOnReload ? hideOnReload : gameObject;

    public void MarkResolved() => IsResolved = true;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(situationId))
            situationId = Guid.NewGuid().ToString("N");
    }
}
