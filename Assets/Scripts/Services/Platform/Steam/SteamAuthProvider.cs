using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class SteamAuthProvider
{
    //TODO: 这里会向 Steam 获取 Session Ticket，并调用AuthenticationManager 来登陆游戏
    public async Task<string> GetSteamTicketAsync()
    {
        // Steamworks.NET:
        // GetAuthTicketForWebApi(...)
        // 等待 Callback
        // 返回 hex string ticket
        return "";
    }
}
