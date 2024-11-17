using Assets.classes;
using Mosframe;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Assets.classes.Relation;

public class dynamic_scoll_view_country_row : UIBehaviour, IDynamicScrollViewItem
{
    [SerializeField] private country_relations_table_manager country_relations_table_manager;
    [SerializeField] private diplomatic_actions_manager diplomatic_actions_manager;

    [SerializeField] private Image cn_in_country_color_img;
    [SerializeField] private TMP_Text country_name_text;
    [SerializeField] private TMP_Text their_opinion_text;
    [SerializeField] private TMP_Text our_opinion_text;
    [SerializeField] private GameObject relations_container;
    [SerializeField] private GameObject relation_type_img_prefab;

    [SerializeField] private Sprite war_sprite;
    [SerializeField] private Sprite alliance_sprite;
    [SerializeField] private Sprite truce_sprite;
    [SerializeField] private Sprite vassalage_sprite_1;
    [SerializeField] private Sprite vassalage_sprite_2;
    [SerializeField] private Sprite subsidies_sprite_1;
    [SerializeField] private Sprite subsidies_sprite_2;
    [SerializeField] private Sprite military_access_sprite_1;
    [SerializeField] private Sprite military_access_sprite_2;

    private int countryId;

    public void onUpdateItem(int index)
    {
        Country country = country_relations_table_manager.SortedCountries[index];
        Country currentPlayer = country_relations_table_manager.Map.CurrentPlayer;

        countryId = country.Id;

        country_name_text.text = country.Name;
        country.setCoatandColor(cn_in_country_color_img);
        int theirOpinion = country.Opinions.ContainsKey(currentPlayer.Id) ? country.Opinions[currentPlayer.Id] : 0;
        int ourOpinion = currentPlayer.Opinions.ContainsKey(countryId) ? currentPlayer.Opinions[countryId] : 0;

        SetOpinionText(their_opinion_text, theirOpinion);
        SetOpinionText(our_opinion_text, ourOpinion);

        foreach (Transform child in relations_container.transform)
        {
            Destroy(child.gameObject);
        }

        // Add relation icons, skipping War-type relations
        country_relations_table_manager.Map.Relations
            .Where(r => r.type != Relation.RelationType.War)
            .Where(r => r.Sides.Contains(currentPlayer) && r.Sides.Contains(country))
            .Select(r => new { relationSprite = GetRelationSpriteForSide(r.type, r.Sides[0] == currentPlayer) })
            .Where(r => r.relationSprite != null)
            .ToList()
            .ForEach(r => {
                GameObject relationImageObj = Instantiate(relation_type_img_prefab, relations_container.transform);
                relationImageObj.GetComponent<Image>().sprite = r.relationSprite;
            });

        // Add a war icon if currentPlayer and country are on opposite sides of the conflict
        country_relations_table_manager.Map.Relations
            .OfType<Relation.War>()
            .Where(warRelation =>
                (warRelation.participants1.Contains(currentPlayer) && warRelation.participants2.Contains(country)) ||
                (warRelation.participants2.Contains(currentPlayer) && warRelation.participants1.Contains(country))
            )
            .ToList()
            .ForEach(warRelation => {
                GameObject warImageObj = Instantiate(relation_type_img_prefab, relations_container.transform);
                warImageObj.GetComponent<Image>().sprite = war_sprite;
            });
    }

    void SetOpinionText(TMP_Text textElement, int opinion)
    {
        textElement.color = opinion == 0 ? Color.yellow : (opinion < 0 ? Color.red : Color.green);
        textElement.text = opinion == 0 ? "0" : (opinion > 0 ? "+" + opinion : opinion.ToString());
    }

    Sprite GetRelationSpriteForSide(RelationType relationType, bool isSide0)
    {
        switch (relationType)
        {
            case RelationType.War:
                return war_sprite;
            case RelationType.Alliance:
                return alliance_sprite;
            case RelationType.Truce:
                return truce_sprite;
            case RelationType.Vassalage:
                return isSide0 ? vassalage_sprite_1 : vassalage_sprite_2;
            case RelationType.Subsidies:
                return isSide0 ? subsidies_sprite_2 : subsidies_sprite_1;
            case RelationType.MilitaryAccess:
                return isSide0 ? military_access_sprite_2 : military_access_sprite_1;
            default:
                return null;
        }
    }

    public void OnCountryRowClick()
    {
        diplomatic_actions_manager.ShowDiplomaticActionsInterface(countryId);
    }
}