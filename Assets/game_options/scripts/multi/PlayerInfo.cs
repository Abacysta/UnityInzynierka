using Mirror;

public class PlayerInfo : NetworkBehaviour
{
    [SyncVar]
    public string playerName;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Set the player name on the server using a command
        CmdSetPlayerName(ConnectionManager.playerName);
    }

    [Command]
    private void CmdSetPlayerName(string name)
    {
        playerName = name;
    }
    public string GetPlayerName()
    {
        return playerName;
    }
}
