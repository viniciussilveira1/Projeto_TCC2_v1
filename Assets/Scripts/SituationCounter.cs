using UnityEngine;
using System;

public class SituationCounter : MonoBehaviour
{
    public static SituationCounter Instance { get; private set; }

    [Header("Meta")]
    [SerializeField] private int goal = 10;
    public int Goal => goal;

    public int Current { get; private set; }

    // Placar
    public int Correct { get; private set; }
    public int Neutral { get; private set; }
    public int Wrong { get; private set; }

    public event Action<int, int> OnChanged;
    public event Action OnGoalReached;

    private bool goalAlreadyFired = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // dispara um OnChanged inicial para a UI sincronizar ao carregar a cena
        OnChanged?.Invoke(Current, goal);
    }

    public void SetGoal(int value)
    {
        goal = Mathf.Max(1, value);
        OnChanged?.Invoke(Current, goal);
    }

    // Se você quiser contar resposta + progresso no mesmo clique:
    public void AddCorrect() { Correct++; Increment(1); }
    public void AddNeutral() { Neutral++; Increment(1); }
    public void AddWrong() { Wrong++; Increment(1); }

    public void RegisterAnswer(int index)
    {
        if (index == 0) Correct++;
        else if (index == 1) Neutral++;
        else Wrong++;
    }

    public void Increment(int amount)
    {
        int before = Current;
        Current = Mathf.Clamp(Current + amount, 0, goal);
        if (Current != before)
        {
            OnChanged?.Invoke(Current, goal);

            if (!goalAlreadyFired && Current >= goal)
            {
                goalAlreadyFired = true;
                OnGoalReached?.Invoke();
            }
        }
    }

    public void ResetAll()
    {
        Current = 0;
        Correct = 0;
        Neutral = 0;
        Wrong = 0;
        goalAlreadyFired = false;
        OnChanged?.Invoke(Current, goal);
    }

    public void Register(NPCDialogue npc)
    {
        if (npc == null) return;
        Increment(1);
    }
}
