using UnityEngine;
using UnityEngine.UI;

public class StartPanel : MonoBehaviour
{
    public GameObject startPanel;
    public Button closeButton;

    void Start()
    {
        if (PlayerPrefs.GetInt("HasPlayedBefore", 0) == 0)
        {
            startPanel.SetActive(true);
            Time.timeScale = 0f; 
            PlayerPrefs.SetInt("HasPlayedBefore", 1);
            PlayerPrefs.Save();
        }
        else
        {
            startPanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    void ClosePanel()
    {
        startPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
