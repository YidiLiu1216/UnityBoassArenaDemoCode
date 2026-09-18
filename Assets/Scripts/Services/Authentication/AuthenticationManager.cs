using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
public enum AuthenticationType
{
    Anonymous,
    Steam
}
public class AuthenticationManager
{
    public bool IsSignedIn =>
        AuthenticationService.Instance.IsSignedIn;
    public string PlayerId =>
       AuthenticationService.Instance.PlayerId;
    public AuthenticationType CurrentAuthenticationType { get; private set; }

    public AuthenticationManager() { 
    
    }
    public async Task InitializeAsync()
    {
        await UnityServices.InitializeAsync();
        await _SignInAnonymouslyAsync();
        //TODO: decide sign in account type based on if user is using the steam or other log in method
    }
    public async Task<bool> UpdatePlayerNameAsync(string newName)
    {
        try
        {
            Debug.Log($"Updating player name to {newName}");
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newName.Trim());
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return false;
        }
    }
    private async Task _SignInAnonymouslyAsync()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                await _InitializePlayerNameAsync();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            
            }
            
        }
    }
    //For Testing purpose, we will use the player id to generate a default player name.
    private async Task _InitializePlayerNameAsync()
    {
        string playerId =AuthenticationService.Instance.PlayerId;
        string shortId = playerId.Length >= 4? playerId.Substring(0, 4): playerId;
        string defaultName =$"Player_{shortId}";
        await AuthenticationService.Instance.UpdatePlayerNameAsync(defaultName);
        Debug.Log($"Initialized player name: " +$"{AuthenticationService.Instance.PlayerName}");
    }

}
