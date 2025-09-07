using UnityEngine;
using UnityEngine.Events;

public class NPCDialogue : MonoBehaviour
{
    [Header("Tema/Situação (ex.: Bullying, Violência, Drogas)")]
    public string situation = "Bullying";

    [Header("Descrição/Contexto")]
    [TextArea(3, 6)]
    public string description;

    [Header("Respostas (ordem: Certa, Neutra, Errada)")]
    public string optionA; // Certa
    public string optionB; // Neutra
    public string optionC; // Errada

    [Header("Eventos opcionais ao escolher")]
    public UnityEvent onChooseCorrect;
    public UnityEvent onChooseNeutral;
    public UnityEvent onChooseWrong;

    [Header("Progresso")]
    public bool countsForProgress = true;   // este NPC conta para o objetivo?
    [HideInInspector] public bool progressCounted = false; // interno: já contou?

}
