using UnityEngine;
using System;

public class SituationCounter : MonoBehaviour
{
    public static SituationCounter Instance { get; private set; }

    [Header("Meta")]
    [SerializeField] private int goal = 10;
    public int Goal => goal;

    public int Current { get; private set; }

    public event Action<int, int> OnChanged;
    public event Action OnGoalReached;

    private bool goalAlreadyFired = false;

    public int Score   { get; private set; }   // <—— NOVO

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

    public void RegisterAnswer(int index)
    {
        switch (index)
        {
            case 0:
                Score += 10;
                break;
            case 1:
                break;
            default:
                break;
        }
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
        Score = 0;
        goalAlreadyFired = false;
        OnChanged?.Invoke(Current, goal);
    }

    public void Register(NPCDialogue npc)
    {
        if (npc == null) return;
        Increment(1);
    }
}
