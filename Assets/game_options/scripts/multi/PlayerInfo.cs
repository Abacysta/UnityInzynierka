using Mirror;
using UnityEngine;
public class PlayerInfo : NetworkBehaviour
{
    [SyncVar]
    public string playerName;
    [SyncVar]
    public int playerId;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CmdSetPlayerName(ConnectionManager.playerName);
    }

    [Command]
    private void CmdSetPlayerName(string name)
    {
        playerName = name;
        playerId = (int)netId;
    }

    public string GetPlayerName()
    {
        return playerName;
    }
    public void SetPlayerId(int id)
    {
        playerId = id;
    }

    public int GetPlayerId()
    {
        return playerId;
    }
}
