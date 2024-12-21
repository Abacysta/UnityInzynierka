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

        country_name_text.text = country.Name +
            (country_relations_table_manager.Map.Controllers[country.Id] == Map.CountryController.Ai ? " (AI)" : "");

        country.SetCoatandColor(cn_in_country_color_img);
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
            .Where(r => r.Type != Relation.RelationType.War)
            .Where(r => r.Sides.Contains(currentPlayer) && r.Sides.Contains(country))
            .Select(r => {
                var spriteInfo = GetRelationSpriteForSide(r.Type, r.Sides[0] == currentPlayer);
                return new { relationSprite = spriteInfo.Item1, tooltipText = spriteInfo.Item2 };
            })
            .Where(r => r.relationSprite != null && r.tooltipText != null)
            .ToList()
            .ForEach(r => {
                GameObject relationImageObj = Instantiate(relation_type_img_prefab, relations_container.transform);
                Image imageComponent = relationImageObj.GetComponent<Image>();
                imageComponent.sprite = r.relationSprite;
                imageComponent.preserveAspect = true;
                help_tooltip_trigger trigger = relationImageObj.AddComponent<help_tooltip_trigger>();
                trigger.TooltipText = r.tooltipText;
            });

        // Add a war icon if currentPlayer and country are on opposite sides of the conflict
        var warRelation = country_relations_table_manager.Map.Relations
            .OfType<Relation.War>()
            .FirstOrDefault(warRelation =>
                (warRelation.Participants1.Contains(currentPlayer) && warRelation.Participants2.Contains(country)) ||
                (warRelation.Participants2.Contains(currentPlayer) && warRelation.Participants1.Contains(country))
            );

        if (warRelation != null)
        {
            var spriteInfo = GetRelationSpriteForSide(Relation.RelationType.War);
            if (spriteInfo.Item1 != null)
            {
                GameObject warImageObj = Instantiate(relation_type_img_prefab, relations_container.transform);
                Image imageComponent = warImageObj.GetComponent<Image>();
                imageComponent.sprite = spriteInfo.Item1;
                imageComponent.preserveAspect = true;
                help_tooltip_trigger trigger = warImageObj.AddComponent<help_tooltip_trigger>();
                trigger.TooltipText = spriteInfo.Item2;
            }
        }
    }

    void SetOpinionText(TMP_Text textElement, int opinion)
    {
        textElement.color = opinion == 0 ? Color.yellow : (opinion < 0 ? Color.red : Color.green);
        textElement.text = opinion == 0 ? "0" : (opinion > 0 ? "+" + opinion : opinion.ToString());
    }

    (Sprite, string) GetRelationSpriteForSide(RelationType relationType, bool isCurrentPlayerSide0 = true)
    {
        switch (relationType)
        {
            case RelationType.War:
                return (war_sprite, "War");
            case RelationType.Alliance:
                return (alliance_sprite, "Alliance"); 
            case RelationType.Truce:
                return (truce_sprite, "Truce");
            case RelationType.Vassalage:
                return isCurrentPlayerSide0 
                    ? (vassalage_sprite_1, "Your vassal") 
                    : (vassalage_sprite_2, "Your liege lord");
            case RelationType.Subsidies:
                return isCurrentPlayerSide0 
                    ? (subsidies_sprite_2, "Your subsidy beneficiary") 
                    : (subsidies_sprite_1, "Your subsidizer");
            case RelationType.MilitaryAccess:
                return isCurrentPlayerSide0 
                    ? (military_access_sprite_2, "Your military access beneficiary") 
                    : (military_access_sprite_1, "Your military access provider");
            default:
                return (null, null);
        }
    }

    public void OnCountryRowClick()
    {
        diplomatic_actions_manager.ShowDiplomaticActionsInterface(countryId);
        sound_manager.instance.playSwitch();
    }
}