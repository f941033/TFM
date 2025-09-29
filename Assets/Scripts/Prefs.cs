using UnityEngine;

public static class Prefs
{
    public static class Keys
    {
        public const string MulliganSeenAction  = "mulligan_seen_action";
    }

    public static bool GetBool(string key, bool def=false)
        => PlayerPrefs.GetInt(key, def ? 1 : 0) == 1;

    public static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static int GetInt(string key, int def=0)
        => PlayerPrefs.GetInt(key, def);

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }
}
