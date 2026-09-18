using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instance { get; private set; }

    public AuthenticationManager Authentication { get; private set; }
    public SessionManager Session { get; private set; }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);

        Authentication = new AuthenticationManager();
        Session = new SessionManager();
    }
    private void OnEnable(){
        
       
    }
    async void Start()
    {
        try
        {
            await Authentication.InitializeAsync();
            Debug.Log($"IsSignedIn: {Authentication.IsSignedIn}");
            Debug.Log($"PlayerId: {Authentication.PlayerId}");
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
