using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Multiplayer.PlayMode;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using WebSocketSharp;

#region JoinSessionResult
public enum JoinSessionResultType
{
    Success,
    SessionNotFound,
    InvalidInput,
    TimeOut,
    AlreadyInSession,
    NetworkError,
    InCorrectPassword,
    UnknownError
}
public readonly struct JoinSessionResult
{
    public JoinSessionResultType Type { get; }
    public ISession Session { get; }
    public string DebugMessage { get; }
    public bool IsSuccess => Type == JoinSessionResultType.Success;

    private JoinSessionResult(JoinSessionResultType type,ISession session,string debugMessage)
    {
        Type = type;
        Session = session;
        DebugMessage = debugMessage;
    }
    public static JoinSessionResult Succeeded( ISession session)
    {
        return new JoinSessionResult(JoinSessionResultType.Success,session, null);
    }
    public static JoinSessionResult Failed(JoinSessionResultType type,string debugMessage = null)
    {
        return new JoinSessionResult(type,null,debugMessage);
    }

}
#endregion

public static class SessionPropertyKeys
{
    public const string PlayerCount = "PlayerCount";
    public const string JoinCode = "JoinCode";
    public const string IsReady = "IsReady";
}
public class SessionManager
{
    public ISession CurrentSession { get; private set; }

    public string SessionId => CurrentSession?.Id;
    public string JoinCode => CurrentSession?.Code;

    
    private int _lastPlayerCount;
    private bool _sessionEventsRegistered=false;

    public event Action OnPlayersChanged;
    public event Action OnRoomSettingsChanged;
    public event Action OnRemovedFromSession;
    public event Action OnHostChanged;
    public async Task<JoinSessionResult> CreateSessionAsync(string name,bool isprivate,string password)
    {
        if (CurrentSession != null) { return JoinSessionResult.Failed(JoinSessionResultType.AlreadyInSession);  }
        var options = _GetDefaultSessionOptions();
        options.Name = name;
        options.Password = password;
        options.IsPrivate = isprivate;
        try
        {
            CurrentSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            var hostSession = CurrentSession.AsHost();
            hostSession.SetProperty(
               SessionPropertyKeys.PlayerCount,
               new SessionProperty("1", VisibilityPropertyOptions.Public)
           );

            _RegisterSessionEvents();
            Debug.Log($"Session {SessionId} created! Join code: {JoinCode}");
            await SetPlayerReadyStatusAsync(false);
            return JoinSessionResult.Succeeded(CurrentSession);
        }
        catch (SessionException e)
        {
            Debug.Log(e);
            return JoinSessionResult.Failed(_MapSessionException(e), e.Message);
        }
        catch (Exception e) { Debug.Log(e); return JoinSessionResult.Failed(JoinSessionResultType.UnknownError, e.Message); }
        finally
        {
            _lastPlayerCount = 1;
        }
    }
    public async Task<JoinSessionResult> JoinSessionByCodeAsync(string code,string password=null)
    {
        if(CurrentSession != null) { return JoinSessionResult.Failed(JoinSessionResultType.AlreadyInSession); }
        try {
            JoinSessionOptions joinOptions = _GetJoinSessionOptions();
            joinOptions.Password = password;
            CurrentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code,joinOptions);
            _RegisterSessionEvents();
            Debug.Log("Join Session Success! Session: {CurrentSession.Id}");
            await SetPlayerReadyStatusAsync(false);
            return JoinSessionResult.Succeeded(CurrentSession);
        }
        catch (SessionException e) {
            if (e.Message.Contains("password", StringComparison.OrdinalIgnoreCase)) { 
               return JoinSessionResult.Failed(JoinSessionResultType.InCorrectPassword, e.Message);
            }
            return JoinSessionResult.Failed(_MapSessionException(e), e.Message);
        }
        catch (Exception e) { Debug.Log(e); return JoinSessionResult.Failed(JoinSessionResultType.UnknownError, e.Message); }
    }
    public async Task<JoinSessionResult> JoinSessonById(string sessionId,string password=null)
    {
        if(CurrentSession != null) { return JoinSessionResult.Failed(JoinSessionResultType.AlreadyInSession); }
        
        try
        {
            JoinSessionOptions joinOptions = _GetJoinSessionOptions();
            joinOptions.Password = password;
            CurrentSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId,joinOptions);
            _RegisterSessionEvents();
            Debug.Log("Join Session Success! Session: {CurrentSession.Id}");
            await SetPlayerReadyStatusAsync(false);
            return JoinSessionResult.Succeeded(CurrentSession);
        }
        catch (SessionException e) {
            if (e.Message.Contains("password", StringComparison.OrdinalIgnoreCase))
            {
                return JoinSessionResult.Failed(JoinSessionResultType.InCorrectPassword, e.Message);
            }
            return JoinSessionResult.Failed(_MapSessionException(e), e.Message);
        }
        catch (Exception e) { Debug.Log(e); return JoinSessionResult.Failed(JoinSessionResultType.UnknownError, e.Message); }
    }
    public async Task<JoinSessionResult> QuickJoinSessionAsync()
    {
        if(CurrentSession != null) { return JoinSessionResult.Failed(JoinSessionResultType.AlreadyInSession); }
        try
        {
            var quickjoinoptions = new QuickJoinOptions
            {
                Timeout = TimeSpan.FromSeconds(5),//Set a timeout for the quick join operation
                CreateSession = false //If no session is found, do not create a new one
            };
            var sessionOptions = _GetDefaultSessionOptions();

            CurrentSession = await MultiplayerService.Instance.MatchmakeSessionAsync(quickjoinoptions,sessionOptions);
            Debug.Log(  $"Quick Join Success! Session: {CurrentSession.Id}" );
            _RegisterSessionEvents();
            await SetPlayerReadyStatusAsync(false);
            return JoinSessionResult.Succeeded(CurrentSession);
        }
        catch (SessionException e) { 
            return JoinSessionResult.Failed(_MapSessionException(e), e.Message);
        }
        catch (Exception e) { Debug.Log(e); return JoinSessionResult.Failed(JoinSessionResultType.UnknownError, e.Message); }

    }
    public async Task LeaveSessionAsync()
    {
        try {
            _UnregisterSessionEvents();
            await CurrentSession.LeaveAsync();
            CurrentSession = null;
            _lastPlayerCount = 0;
            _sessionEventsRegistered = false;
            CurrentSession = null;
        }
        catch(SessionException e) 
        { Debug.Log(e); }
      
    }
    public async Task LeaveSessionAsHostAsync()
    {
        if (CurrentSession == null) { return; }
        var hostSession = CurrentSession.AsHost(); 
        var players = CurrentSession.Players.ToList();

        foreach (var player in players) {
            if (player.Id == CurrentSession.CurrentPlayer.Id)
                continue;
            try
            {
                await hostSession.RemovePlayerAsync(player.Id);
            }
            catch (Exception e) {
                Debug.Log(e);
            }
        }
        await LeaveSessionAsync();
    }
    public async Task KickPlayerAsync(string playerId)
    {
        if (CurrentSession == null) { return; }
        if (!NetworkManager.Singleton.IsHost) { return; }
        try
        {
            IHostSession hostSession = CurrentSession.AsHost();
            await hostSession.RemovePlayerAsync(playerId);
            Debug.Log($"Player {playerId} removed from session {CurrentSession.Id}");
        }
        catch (SessionException e)
        {
            Debug.Log($"Failed to remove player {playerId}: {e.Message}");
        }
    }
    public async Task ReconnectSessionAsync()
    {

    }
    public async Task UpdateSessionAsync(bool isprivate,string password)
    {
        if (CurrentSession == null) { Debug.Log("Not in a session."); return; }
        if (!NetworkManager.Singleton.IsHost) { return; }
        try
        {
            IHostSession hostSession = CurrentSession.AsHost();
            hostSession.IsPrivate = isprivate;
            hostSession.Password = password;
            await hostSession.SavePropertiesAsync();
            Debug.Log($"Session {CurrentSession.Id} updated! isPrivate: {isprivate}, password: {(string.IsNullOrEmpty(password) ? "null" : password)}");
        }
        catch(SessionException e) 
        { Debug.Log(e); }
        
    }

    public async Task<QuerySessionsResults> BrowseSessionsAsync()
    {
        var options = new QuerySessionsOptions
        {
            Count = 20,
            SortOptions = new List<SortOption>
            {
        new SortOption(
                    SortOrder.Descending,
                    SortField.CreationTime
                    )
            }
        };
        try
        {
            Debug.Log("Start Searching for Session");
            var results = await MultiplayerService.Instance.QuerySessionsAsync(options);
            Debug.Log("Query Session Success!");
            return results;
        }
        catch (SessionException e)
        {
            Debug.Log($"Query sessions failed: {e.Message}");
        }
        catch (Exception e) { 
        
        }
        return null;
    }
    public async Task SetPlayerReadyStatusAsync(bool isready)
    {
        if (CurrentSession == null) {  return; }
        try
        {
            var player = CurrentSession.CurrentPlayer;
            player.SetProperty(
                    SessionPropertyKeys.IsReady,
                    new PlayerProperty(isready ? "true" : "false", VisibilityPropertyOptions.Member)
             );
            await CurrentSession.SaveCurrentPlayerDataAsync();
            Debug.Log($"Player {player.Id} ready status set to {isready}");
        }
        catch (SessionException e)
        {
            Debug.Log($"Failed to set player ready status: {e.Message}");
        }
    }
    public bool IsPlayerReady(string playerId)
    {
        if (CurrentSession == null) { return false; }
        var player = CurrentSession.GetPlayer(playerId);
        if (player.Properties.TryGetValue(SessionPropertyKeys.IsReady, out PlayerProperty readyProperty))
        {
            return readyProperty.Value == "true";
        }
        return false;
    }
    public bool IsAllPlayersReady()
    {
        ISession session = GameBootstrap.Instance.Session.CurrentSession;
        if (session == null) { return false; }
        foreach (var player in session.Players)
        {
            if (!GameBootstrap.Instance.Session.IsPlayerReady(player.Id))
            {
                return false;
            }
        }
        return true;
    }
    public string GetMyPlayerName()
    {
        if (CurrentSession == null) { return "Unknown"; }
        string myPlayerId = AuthenticationService.Instance.PlayerId;
        foreach (var player in CurrentSession.Players)
        {
            if (player.Id == myPlayerId)
            {
                return player.GetPlayerName();
            }
        }
        return "Unknown";
    }
    private void _RegisterSessionEvents()
    {
        if (CurrentSession == null||_sessionEventsRegistered) { return; }

        _lastPlayerCount = CurrentSession.Players.Count;
        CurrentSession.Changed += _OnSessionChanged;
        CurrentSession.PlayerPropertiesChanged += _OnPlayerPropertiesChanged;
        CurrentSession.SessionPropertiesChanged += _OnSessionPropertiesChanged;
        CurrentSession.RemovedFromSession += _OnRemovedFromSession;
        CurrentSession.SessionHostChanged += _OnHostChanged;
        _sessionEventsRegistered = true;
    }
    private void _UnregisterSessionEvents()
    {
        if (CurrentSession == null||!_sessionEventsRegistered){ return; }

        CurrentSession.Changed -= _OnSessionChanged;
        CurrentSession.PlayerPropertiesChanged -= _OnPlayerPropertiesChanged;
        CurrentSession.SessionPropertiesChanged -= _OnSessionPropertiesChanged;
        CurrentSession.RemovedFromSession -= _OnRemovedFromSession;
        CurrentSession.SessionHostChanged -= _OnHostChanged;
        _sessionEventsRegistered = false;
    }
    private async void _OnSessionChanged()
    {
        int newCount = CurrentSession.Players.Count;
        if (newCount != _lastPlayerCount)
        {
            _lastPlayerCount = newCount;
            Debug.Log($"Player count changed: {newCount}");
            if (CurrentSession.IsHost)
            {
                var hostSession = CurrentSession.AsHost();
                hostSession.SetProperty("PlayerCount",new SessionProperty(
                newCount.ToString(),
                VisibilityPropertyOptions.Public));
                await hostSession.SavePropertiesAsync();
            }
            OnPlayersChanged?.Invoke();
        }
        OnRoomSettingsChanged?.Invoke();
        //if the passord or isprivate changed, invoke the event

    }
    private void _OnPlayerPropertiesChanged()
    {
        Debug.Log("Player properties changed.");
        OnPlayersChanged?.Invoke();
    }
    private void _OnSessionPropertiesChanged()
    {
        Debug.Log("Session properties changed.");
  
    }
    private void _OnRemovedFromSession()
    {
        Debug.Log("Removed from session.");
        _UnregisterSessionEvents();
        CurrentSession = null;
        _lastPlayerCount = 0;
        _sessionEventsRegistered = false;
        OnRemovedFromSession?.Invoke();
    }
    private void _OnHostChanged(string newHostId)
    {
        Debug.Log("Host changed.");
        OnHostChanged?.Invoke();
    }
    private SessionOptions _GetDefaultSessionOptions()
    {
        return new SessionOptions
        {
            MaxPlayers = 4
        }
        .WithPlayerName(VisibilityPropertyOptions.Member)
        .WithRelayNetwork();
    }
    private JoinSessionOptions _GetJoinSessionOptions()
    {
        return new JoinSessionOptions()
        .WithPlayerName(VisibilityPropertyOptions.Member);
    }
  
    private JoinSessionResultType _MapSessionException(SessionException e)
    {
        //TODO: Map the exception to a JoinSessionResultType
        switch(e.Error)
        {
            //session not found
            case SessionError.SessionNotFound:
                return JoinSessionResultType.SessionNotFound;
            case SessionError.SessionDeleted:
                return JoinSessionResultType.SessionNotFound;
            case SessionError.InvalidSessionIdentifier:
                return JoinSessionResultType.SessionNotFound;
            //invalid input
            case SessionError.InvalidParameter:
                return JoinSessionResultType.InvalidInput;
            case SessionError.InvalidSessionMetadata:
                return JoinSessionResultType.InvalidInput;
            case SessionError.InvalidCreateSessionOptions:
                return JoinSessionResultType.InvalidInput;
            case SessionError.InvalidPlayerName:
                return JoinSessionResultType.InvalidInput;
            //TimeOut
            case SessionError.MatchmakerAssignmentTimeout:
                return JoinSessionResultType.TimeOut;
            //Network Error
            case SessionError.NetworkManagerStartFailed:
                return JoinSessionResultType.NetworkError;
            case SessionError.NetworkSetupFailed:
                return JoinSessionResultType.NetworkError;
            case SessionError.TransportInvalid:
                 return JoinSessionResultType.NetworkError;
            case SessionError.MigrationDataRequestTimeout:
                return JoinSessionResultType.NetworkError;
            default:
                Debug.Log($"Unhandled session exception: {e.Error}");
                break;
        }
        return JoinSessionResultType.UnknownError;
    }
}
