using Mirror;
using UnityEngine;

public class PlayerLobbyCommands : NetworkBehaviour
{
    [Command]
    public void CmdAssignCountryToPlayer(int countryId)
    {
        Debug.Log($"CmdAssignCountryToPlayer called with countryId: {countryId}");
        var lobby = FindObjectOfType<MultiplayerLobby>();
        if (lobby != null)
        {
            int playerNumber = lobby.currentMaxPlayerNumber + 1;
            lobby.AssignCountryToPlayer(countryId, playerNumber);
        }
        else
        {
            Debug.LogError("MultiplayerLobby instance not found on the server.");
        }
    }

}
