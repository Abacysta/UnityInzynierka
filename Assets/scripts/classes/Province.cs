using UnityEngine;

[System.Serializable]
public class Province
{
    [SerializeField] private string id;
    [SerializeField] private string name;
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private string type;
    [SerializeField] private string resources;
    [SerializeField] private int resources_amount;
    [SerializeField] private int population;
    [SerializeField] private int recruitablePopulation;
    [SerializeField] private int happiness;
    [SerializeField] private bool is_coast;
    [SerializeField] private bool is_possible_to_recruit;

    public Province() { }

    public Province(string id, string name, int x, int y, string type, string resources, int resources_amount, int population, int happiness, bool is_coast, int recruitablePopulation,bool is_possible_to_recruit)
    {
        this.Id = id;
        this.Name = name;
        this.X = x;
        this.Y = y;
        this.Type = type;
        this.Resources = resources;
        this.Resources_amount = resources_amount;
        this.Population = population;
        this.Happiness = happiness;
        this.Is_coast = is_coast;
        this.recruitablePopulation = recruitablePopulation;
        this.is_possible_to_recruit = is_possible_to_recruit;
    }

    public string Id { get => id; set => id = value; }
    public string Name { get => name; set => name = value; }
    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public string Type { get => type; set => type = value; }
    public string Resources { get => resources; set => resources = value; }
    public int Resources_amount { get => resources_amount; set => resources_amount = value; }
    public int Population { get => population; set => population = value; }
    public int Happiness { get => happiness; set => happiness = value; }
    public bool Is_coast { get => is_coast; set => is_coast = value; }
    public int RecruitablePopulation { get => recruitablePopulation; set => recruitablePopulation = value; }
    public bool Is_possible_to_recruit { get => is_possible_to_recruit; set => is_possible_to_recruit = value; }
}
