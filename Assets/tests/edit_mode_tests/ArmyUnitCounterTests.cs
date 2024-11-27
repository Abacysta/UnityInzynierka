using NUnit.Framework;
using TMPro;
using UnityEngine;

[TestFixture]
public class ArmyUnitCounterTests
{
    private GameObject armyViewObject;
    private army_view armyViewScript;
    private TMP_Text armyCountText;
    private Army armyData;

    // Arrange
    [SetUp]
    public void Setup()
    {
        armyViewObject = new GameObject();
        armyViewScript = armyViewObject.AddComponent<army_view>();

        armyViewScript.Army_count_text = armyViewObject.AddComponent<TextMeshProUGUI>();
        armyCountText = armyViewScript.Army_count_text;

        armyViewScript.ArmyData = new Army(1, 100, (0, 0), (0, 0));
        armyData = armyViewScript.ArmyData;
        armyViewScript.UpdateArmyCounter(armyData.Count);
        armyViewScript.ArmyData.OnArmyCountChanged += armyViewScript.UpdateArmyCounter;
    }

    [Test]
    public void GivenArmyWith100Units_WhenArmyCountIsChangedTo150_ThenUIShouldUpdateTo150()
    {
        Assert.AreEqual("100", armyCountText.text);

        // Act
        armyData.Count = 150;

        // Assert
        Assert.AreEqual("150", armyCountText.text);
    }

    [Test]
    public void GivenArmyWith100Units_WhenArmyCountRemains100_ThenUIShouldRemainUnchanged()
    {
        Assert.AreEqual("100", armyCountText.text);

        // Act
        armyData.Count = 100;

        // Assert
        Assert.AreEqual("100", armyCountText.text);
    }
}