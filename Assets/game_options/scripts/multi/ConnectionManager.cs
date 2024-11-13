using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class ConnectionManager : MonoBehaviour
{
    public TMP_InputField playerNameInput; // Field to enter player name
    public TMP_InputField joinCodeInput;   // Field to enter join code
    public Button hostButton;              // Button to host the game
    public Button joinButton;              // Button to join a game
    public Button nextButton;              // Button to start the game (only host can use)
    public TMP_Text joinCodeDisplay;       // Displays join code when hosting
    private bool isHost = false;           // Track if current player is the host

    // Static player name to persist between scenes
    public static string playerName;

    private void Start()
    {
        nextButton.interactable = false; // Disable Next button initially

        hostButton.onClick.AddListener(HostGame);
        joinButton.onClick.AddListener(JoinGame);
        nextButton.onClick.AddListener(StartGame);
    }

    public void HostGame()
    {
        playerName = playerNameInput.text;  // Gracz wpisuje swoj¹ nazwê
        // Set as host, start the server and display join code
        isHost = true;
        NetworkManager.singleton.StartHost();
        string joinCode = GenerateJoinCode();
        joinCodeDisplay.text = $"Join Code: {joinCode}";
        nextButton.interactable = true; // Enable Next button for host only
    }

    public void JoinGame()
    {
        // Save player name for use in other scenes
        playerName = playerNameInput.text;

        // Set network address from join code and start client
        string joinCode = joinCodeInput.text;
        NetworkManager.singleton.networkAddress = GetAddressFromJoinCode(joinCode);
        NetworkManager.singleton.StartClient();
    }

    public void StartGame()
    {
        if (isHost)
        {
            // Host proceeds to the game scene
            NetworkManager.singleton.ServerChangeScene("multiplayer"); // Replace with actual game scene name
        }
    }

    private string GenerateJoinCode()
    {
        // For simplicity, generate a hardcoded address (for local testing)
        return "localhost"; // Replace with actual logic for unique join codes if needed
    }

    private string GetAddressFromJoinCode(string code)
    {
        return "localhost"; // For now, all clients connect to localhost (use actual IP for real setup)
    }
}
