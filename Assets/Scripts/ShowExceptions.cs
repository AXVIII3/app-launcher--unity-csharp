using UnityEngine;
using TMPro;

public class ShowExceptions : MonoBehaviour
{
    public static ShowExceptions Singleton;

    [Header("References")]
    [SerializeField] private GameObject errorDialogBox;
    [SerializeField] private TMP_Text headingText;
    [SerializeField] private TMP_Text bodyText;

    private void Awake() 
    {
        Singleton = this;    
    }

    public void ShowError (string heading, string body)
    {
        headingText.text = heading;
        bodyText.text = body;
        errorDialogBox.SetActive(true);
    }

    public void ClickOK ()
    {
        headingText.text = "";
        bodyText.text = "";
        errorDialogBox.SetActive(false);
    }
}