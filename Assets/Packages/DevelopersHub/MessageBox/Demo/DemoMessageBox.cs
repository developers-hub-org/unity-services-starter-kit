using System.Collections;
using System.Collections.Generic;
using DevelopersHub.MessageBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DemoMessageBox : MonoBehaviour
{

    [SerializeField] private Button _msgButton = null;
    [SerializeField] private Button _strButton = null;
    [SerializeField] private Button _bolButton = null;
    [SerializeField] private TextMeshProUGUI _logText = null;

    private void Start()
    {
        _msgButton.onClick.AddListener(ShowMessage);
        _strButton.onClick.AddListener(StringQuestion);
        _bolButton.onClick.AddListener(BoolQuestion);
    }

    private void ShowMessage()
    {
        MessageBox.Show("Test", "This is just a test.", "OK", ShowMessageResponse);
    }

    private void ShowMessageResponse()
    {
        _logText.text = "Message box closed";
    }

    private void StringQuestion()
    {
        QuestionBoxString.Show("Test", "How tall are you?", "", "Height (cm) ...", 1, 3, "Confirm", "Cancel", TMP_InputField.ContentType.IntegerNumber, StringQuestionResponse);
    }

    private void StringQuestionResponse(bool confirmed, string response)
    {
        if (confirmed)
        {
            _logText.text = "Question box response: " + response + " cm";
        }
        else
        {
            _logText.text = "Question box closed";
        }
    }

    private void BoolQuestion()
    {
        QuestionBoxBool.Show("Test", "You are a gamer?", "Yes", "No", BoolQuestionResponse);
    }

    private void BoolQuestionResponse(bool response)
    {
        _logText.text = "Question box response: " + response.ToString();
    }

}
