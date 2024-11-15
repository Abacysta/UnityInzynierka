using Mirror;
using UnityEngine;

public class PlayerLobbyCommands : NetworkBehaviour
{
    [Command]
    public void CmdAssignCountryToPlayer(int countryId)
    {
        var lobby = FindObjectOfType<MultiplayerLobby>();
        if (lobby != null)
        {
            lobby.AssignCountryToPlayer(countryId, netIdentity); 
        }
    }
}






