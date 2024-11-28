using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[TestFixture]
public class TaxButtonsTests
{
    private production_tab_manager productionTabManagerScript;
    private List<Toggle> taxToggles;
    private Map map;

    // Arrange
    [SetUp]
    public void Setup()
    {
        map = ScriptableObject.CreateInstance<Map>();

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

        productionTabManagerScript = new GameObject().AddComponent<production_tab_manager>();

        productionTabManagerScript.Map = map;
        productionTabManagerScript.Tax_text = new GameObject().AddComponent<TextMeshProUGUI>();
        productionTabManagerScript.Happ_text = new GameObject().AddComponent<TextMeshProUGUI>();

        productionTabManagerScript.ResourcePanel = new GameObject();

        TMP_Text goldText = new GameObject("gold_text").AddComponent<TextMeshProUGUI>();
        goldText.transform.SetParent(productionTabManagerScript.ResourcePanel.transform);

        TMP_Text woodText = new GameObject("wood_text").AddComponent<TextMeshProUGUI>();
        woodText.transform.SetParent(productionTabManagerScript.ResourcePanel.transform);

        TMP_Text ironText = new GameObject("iron_text").AddComponent<TextMeshProUGUI>();
        ironText.transform.SetParent(productionTabManagerScript.ResourcePanel.transform);

        TMP_Text sciencePointsText = new GameObject("science_points_text").AddComponent<TextMeshProUGUI>();
        sciencePointsText.transform.SetParent(productionTabManagerScript.ResourcePanel.transform);

        taxToggles = new();
        for (int i = 0; i < 5; i++)
        {
            Toggle taxToggle = new GameObject().AddComponent<Toggle>();
            taxToggles.Add(taxToggle);
        }

        productionTabManagerScript.TaxToggles = taxToggles;
        productionTabManagerScript.InitializeTaxToggles();
    }

    private (string TaxHapp, string TaxPercent, Color HappColor) GetExpectedTaxInfo()
    {
        string tax_happ = map.CurrentPlayer.Tax.HappP + "%";
        string tax_percent = map.CurrentPlayer.Tax.GoldP * 100 + "%";
        Color happ_color = map.CurrentPlayer.Tax.HappP > 0 ? Color.green : Color.red;

        return (tax_happ, tax_percent, happ_color);
    }

    [Test]
    public void TestTaxesButtons()
    {
        for (int i = 0; i < taxToggles.Count; i++)
        {
            // Arrange
            taxToggles[i].isOn = false;
            taxToggles[i].onValueChanged.Invoke(false);

            // Act
            taxToggles[i].isOn = true;
            taxToggles[i].onValueChanged.Invoke(true);

            // Assert
            var (TaxHapp, TaxPercent, HappColor) = GetExpectedTaxInfo();

            Assert.AreEqual(i, productionTabManagerScript.getTaxType());
            Assert.AreEqual(TaxPercent, productionTabManagerScript.Tax_text.text);
            Assert.AreEqual(TaxHapp, productionTabManagerScript.Happ_text.text);
            Assert.AreEqual(HappColor, productionTabManagerScript.Happ_text.color);
        }
    }
}