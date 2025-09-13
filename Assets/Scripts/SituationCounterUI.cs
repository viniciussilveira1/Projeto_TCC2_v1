using UnityEngine;
using TMPro;

public class SituationCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;

    private void OnEnable()
    {
        if (SituationCounter.Instance != null) {
            SituationCounter.Instance.OnChanged += UpdateUI;
            UpdateUI(SituationCounter.Instance.Current, SituationCounter.Instance.Goal);
        }
    }

    private void OnDisable()
    {
        if (SituationCounter.Instance != null)
            SituationCounter.Instance.OnChanged -= UpdateUI;
    }

    private void UpdateUI(int current, int goal)
    {
        if (counterText != null)
            counterText.text = $"Situações: {current} / {goal}";
    }
}
