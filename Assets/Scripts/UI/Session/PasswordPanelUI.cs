using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
public class PasswordPanelUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField]
    private TMP_InputField _passwordInputField;
    [Header("Buttons")]
    [SerializeField]
    private Button _submitButton;
    [SerializeField]
    private Button _cancelButton;

    private string _sessionId;
    private string _sessionCode;
    private void Awake()
    {
        _submitButton.onClick.AddListener(_OnSubmitClicked);
        _cancelButton.onClick.AddListener(_OnCancelClicked);
    }
    private void OnEnable()
    {
        _sessionId= null;
        _sessionCode= null;
        _passwordInputField.text = string.Empty;
    }
    public void SetSessionIDorCode(string sessionId, string sessionCode) 
    { 
        _sessionId = sessionId;
        _sessionCode = sessionCode;
    }
    private async void _OnSubmitClicked()
    {
        string password = _passwordInputField.text;

        if (!SessionUtility.IsValidatePassword(password))
        {
            //TODO: Show error message
            return;
        }
        if(!string.IsNullOrEmpty(_sessionId))
        {
            try
            {
                JoinSessionResult result = await GameBootstrap.Instance.Session.JoinSessonById(_sessionId, password);
                if (result.IsSuccess)
                {
                    UIManager.Instance.ShowCreateRoom();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to join session by ID: {e.Message}");
            }
            finally { 
            
            }
        }
        else if(!string.IsNullOrEmpty(_sessionCode))
        {
            try {
                JoinSessionResult result=await GameBootstrap.Instance.Session.JoinSessionByCodeAsync(_sessionCode,password);
                if (result.IsSuccess)
                {
                    UIManager.Instance.ShowCreateRoom();
                }
            }
            catch(System.Exception e)
            {
                Debug.LogError($"Failed to join session by Code: {e.Message}");
            }
        }


     }
    private void _OnCancelClicked()
    {
       UIManager.Instance.ShowBrowseRoom();
    }

}