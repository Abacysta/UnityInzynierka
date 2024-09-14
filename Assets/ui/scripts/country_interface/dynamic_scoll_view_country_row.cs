using Mosframe;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Assets.classes.Relation;

// Jeszcze nie skonczylem tu robiæ typow relacji
public class dynamic_scoll_view_country_row : UIBehaviour, IDynamicScrollViewItem
{
    [SerializeField] private country_relations_table_manager country_relations_table_manager;

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

    public void onUpdateItem(int index)
    {
        Country country = country_relations_table_manager.SortedCountries[index];
        Country currentPlayer = country_relations_table_manager.Map.CurrentPlayer;

        country_name_text.text = country.Name;
        cn_in_country_color_img.color = country.Color;
        int theirOpinion = country.Opinions.ContainsKey(currentPlayer.Id) ? country.Opinions[currentPlayer.Id] : 0;
        int ourOpinion = currentPlayer.Opinions.ContainsKey(country.Id) ? currentPlayer.Opinions[country.Id] : 0;

        SetOpinionText(their_opinion_text, theirOpinion);
        SetOpinionText(our_opinion_text, ourOpinion);

        /*
        List<Relation> relations = country_relations_table_manager.Map.Relations
            .Where(r => r.Countries.Contains(country) && r.Countries.Contains(map.CurrentPlayer))
            .ToList();

        foreach (var relation in relations)
        {
            Sprite relationSprite = GetResourceSprite(relation.Type);
            if (relationSprite != null)
            {
                GameObject relationImageObj = Instantiate(relation_type_img_prefab, relations_container.transform);
                relationImageObj.GetComponent<Image>().sprite = relationSprite;
            }
        }
        */
    }

    void SetOpinionText(TMP_Text textElement, int opinion)
    {
        textElement.color = opinion == 0 ? Color.yellow : (opinion < 0 ? Color.red : Color.green);
        textElement.text = opinion == 0 ? "0" : (opinion > 0 ? "+" + opinion : opinion.ToString());
    }

    Sprite GetResourceSprite(RelationType relationType)
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
                return vassalage_sprite_1;
            case RelationType.Subsidies:
                return subsidies_sprite_1;
            case RelationType.MilitaryAccess:
                return military_access_sprite_1;
            default:
                return null;
        }
    }
}