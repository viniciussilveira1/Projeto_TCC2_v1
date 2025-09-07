using UnityEngine;
using TMPro;
using System;

public class SituationCounter : MonoBehaviour
{
    public static SituationCounter Instance { get; private set; }

    [Header("Meta")]
    [SerializeField] private int goal = 10;

    [Header("UI")]
    [SerializeField] private TMP_Text counterText;

    public int Current { get; private set; }

    public event Action<int,int> OnChanged;
    public event Action OnGoalReached;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        UpdateUI();
    }

    public void SetGoal(int value)
    {
        goal = Mathf.Max(1, value);
        UpdateUI();
    }

    /// <summary>
    /// Registra a situação se ainda não foi contada para esse NPC.
    /// </summary>
    public void Register(NPCDialogue npc)
    {
        if (npc == null || npc.progressCounted) return;
        npc.progressCounted = true;
        Increment(1);
    }

    public void Increment(int amount)
    {
        int before = Current;
        Current = Mathf.Clamp(Current + amount, 0, goal);
        if (Current != before)
        {
            UpdateUI();
            OnChanged?.Invoke(Current, goal);
            if (Current >= goal)
                OnGoalReached?.Invoke();
        }
    }

    private void UpdateUI()
    {
        if (counterText != null)
            counterText.text = $"Situações: {Current} / {goal}";
    }
}
