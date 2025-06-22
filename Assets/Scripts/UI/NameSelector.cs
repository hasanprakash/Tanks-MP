using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSelector : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private Button connectButton;
    [SerializeField] private int minNameLength = 3;
    [SerializeField] private int maxNameLength = 12;

    public static string PlayerName = "PlayerName";

    private void Start()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        nameField.text = PlayerPrefs.GetString(PlayerName, string.Empty);
        // what's this PlayerPrefs?
        // PlayerPrefs is a Unity class used to store and access player preferences and settings.
        // It allows you to save simple data types like strings, integers, and floats.
        HandleInputChange();
    }

    public void HandleInputChange()
    {
        connectButton.interactable =
            nameField.text.Length >= minNameLength &&
            nameField.text.Length <= maxNameLength &&
            !string.IsNullOrWhiteSpace(nameField.text);
    }

    public void Connect()
    {
        PlayerPrefs.SetString(PlayerName, nameField.text);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
