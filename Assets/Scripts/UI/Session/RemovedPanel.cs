using UnityEngine;
using UnityEngine.UI;
public class RemovedPanel : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] 
    private Button _confirmButton;


    private void Awake()
    {
        _confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
    }
    private void _OnConfirmButtonClicked()
    {
        UIManager.Instance.ShowMain();
    }
}
