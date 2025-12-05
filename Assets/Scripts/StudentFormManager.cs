using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StudentFormManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField inputNome;
    [SerializeField] private TMP_Dropdown dropdownEscola;
    [SerializeField] private TMP_Dropdown dropdownAno;
    [SerializeField] private Button buttonContinuar;
    [SerializeField] private GameObject labelWarning;

    private void Start()
    {
        labelWarning?.SetActive(false);
        buttonContinuar.onClick.AddListener(OnContinuar);
    }

    private void OnContinuar()
    {
        string nome = inputNome.text.Trim();

        if (!IsNomeValido(nome))
        {
            if (labelWarning != null)
                labelWarning.SetActive(true);
            return;
        }

        string escola = dropdownEscola.options[dropdownEscola.value].text;
        string ano = dropdownAno.options[dropdownAno.value].text;

        PlayerPrefs.SetString("Aluno_Nome", nome);
        PlayerPrefs.SetString("Aluno_Escola", escola);
        PlayerPrefs.SetString("Aluno_Ano", ano);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Intro");
    }

    private bool IsNomeValido(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return false;

        var partes = nome.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

        // Pelo menos dois nomes
        if (partes.Length < 2)
            return false;

        return true;
    }
}
