using UnityEngine;

public static class SessionUtility
{
    public static string ToDisplayName(string playername) { 
         if (string.IsNullOrEmpty(playername)) { return "Unknown Player"; }
         int index = playername.LastIndexOf('#');
         return index >= 0 ? playername.Substring(0, index) : playername;
    }
    public static bool IsValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return true;
        return password.Length == 8;// Unity 有效密码至少8位

    }
}
