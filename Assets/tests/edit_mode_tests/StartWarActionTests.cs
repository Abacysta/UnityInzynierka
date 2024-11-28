using NUnit.Framework;
using UnityEngine;
using static Assets.classes.TurnAction;
using Assets.map.scripts;
using static Assets.classes.Event_.DiploEvent;
using Assets.classes;
using System.Linq;
using UnityEngine.UI;

[TestFixture]
public class StartWarActionTests
{
    private Country currentPlayer, receiverCountry;
    private Country currentPlayerVassal, receiverCountryVassal;
    private Map map;

    // Arrange
    [SetUp]
    public void Setup()
    {
        player_table playerTableScript = new GameObject().AddComponent<player_table>();
        playerTableScript.PlayerTable = playerTableScript.gameObject;
        playerTableScript.Dummy = new GameObject();
        GameObject mapOptionsGameObject = new();
        map_options mapOptionsScript = mapOptionsGameObject.AddComponent<map_options>();
        GameObject buttonObject = new("startgame");
        buttonObject.AddComponent<Button>();
        buttonObject.transform.SetParent(mapOptionsGameObject.transform);
        playerTableScript.OptionsTable = mapOptionsScript;

        playerTableScript.Map = ScriptableObject.CreateInstance<Map>();
        playerTableScript.LoadMap("Map1");
        playerTableScript.Controllers[1] = Map.CountryController.Local;
        playerTableScript.StartGame();
        map = playerTableScript.Map;

        diplomatic_relations_manager diplomatic_relations_manager = new GameObject().AddComponent<diplomatic_relations_manager>();
        diplomatic_relations_manager.Map = map;
        dialog_box_manager dialog_box = new GameObject().AddComponent<dialog_box_manager>();
        camera_controller camera_controller = new GameObject().AddComponent<camera_controller>();
        diplomatic_actions_manager diplomatic_actions_manager = new GameObject().AddComponent<diplomatic_actions_manager>();

        currentPlayer = map.Countries[1];
        receiverCountry = map.Countries[2];
        currentPlayerVassal = map.Countries[3];
        receiverCountryVassal = map.Countries[4];

        var action = new start_war(currentPlayer, receiverCountry, diplomatic_relations_manager, 
            dialog_box, camera_controller, diplomatic_actions_manager);
        currentPlayer.Actions.addAction(action);
    }

    [Test]
    public void WarIsDeclared_ReceiverIsAddedWarDeclaredEventWithProperSenderAndReceiver()
    {
        // Arrange
        receiverCountry.Events.Clear();

        // Act
        currentPlayer.Actions.execute();

        // Assert
        WarDeclared warDeclaredEvent = (WarDeclared)receiverCountry.Events.Find(e => e is WarDeclared);

        Assert.IsNotNull(warDeclaredEvent, "WarDeclared event should be added to receiver country.");
        Assert.AreEqual(receiverCountry, warDeclaredEvent.To);
        Assert.AreEqual(currentPlayer, warDeclaredEvent.From);
    }

    [Test]
    public void WarIsDeclared_WarRelationShouldBeAddedAndProperlyConfigured()
    {
        // Arrange
        map.Relations.Clear();
        currentPlayer.AtWar = false;
        receiverCountry.AtWar = false;
        map.Relations.Add(new Relation.Vassalage(currentPlayer, currentPlayerVassal));
        map.Relations.Add(new Relation.Vassalage(receiverCountry, receiverCountryVassal));

        // Act
        currentPlayer.Actions.execute();

        // Assert
        Relation.War warRelation = (Relation.War)map.Relations.ToList().Find(r => r is Relation.War);

        Assert.IsNotNull(warRelation, "War relation should be created.");
        Assert.AreEqual(currentPlayer, warRelation.Sides[0], "First side in the war relation should be the current player.");
        Assert.AreEqual(receiverCountry, warRelation.Sides[1], "Second side in the war relation should be the receiver country.");

        Assert.IsTrue(warRelation.participants1.Contains(currentPlayer));
        Assert.IsTrue(warRelation.participants1.Contains(currentPlayerVassal));
        Assert.IsTrue(warRelation.participants2.Contains(receiverCountry));
        Assert.IsTrue(warRelation.participants2.Contains(receiverCountryVassal));

        Assert.IsTrue(currentPlayer.AtWar);
        Assert.IsTrue(receiverCountry.AtWar);
    }
}