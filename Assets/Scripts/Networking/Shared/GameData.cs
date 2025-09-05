using System;

public enum GameMode
{
    Default
    // Deathmatch,
    // TeamDeathmatch,
    // CaptureTheFlag
}

public enum GameQueue
{
    Solo,
    Team
}

[Serializable]
public class UserData
{
    public string userName;
    public string userAuthId;
    public GameData userGamePreferences;
}

public class GameData
{
    // public Map map; // yet to setup the map system
    public GameMode gameMode;
    public GameQueue gameQueue;

    public string ToMultiplayQueue()
    {
        return "";
    }
}