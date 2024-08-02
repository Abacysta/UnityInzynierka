using UnityEngine;

[System.Serializable]
public class Province {
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
    [SerializeField] private bool occupation;
    [SerializeField] private int occupation_id;
    [SerializeField] private int occupation_duration;
    [SerializeField] private int owner_id;

    public Province(string id, string name, int x, int y, string type, string resources, int resources_amount, int population, int recruitablePopulation, int happiness, bool is_coast, bool is_possible_to_recruit, bool occupation, int occupation_id, int occupation_duration, int owner_id) {
        this.id = id;
        this.name = name;
        this.x = x;
        this.y = y;
        this.type = type;
        this.resources = resources;
        this.resources_amount = resources_amount;
        this.population = population;
        this.recruitablePopulation = recruitablePopulation;
        this.happiness = happiness;
        this.is_coast = is_coast;
        this.occupation = occupation;
        this.occupation_id = occupation_id;
        this.occupation_duration = occupation_duration;
        this.owner_id = owner_id;
    }

    public string Id { get => id; set => id = value; }
    public string Name { get => name; set => name = value; }
    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public string Type { get => type; set => type = value; }
    public string Resources { get => resources; set => resources = value; }
    public int Resources_amount { get => resources_amount; set => resources_amount = value; }
    public int Population { get => population; set => population = value; }
    public int RecruitablePopulation { get => recruitablePopulation; set => recruitablePopulation = value; }
    public int Happiness { get => happiness; set => happiness = value; }
    public bool Is_coast { get => is_coast; set => is_coast = value; }
    public bool Occupation { get => occupation; set => occupation = value; }
    public int Occupation_id { get => occupation_id; set => occupation_id = value; }
    public int Occupation_duration { get => occupation_duration; set => occupation_duration = value; }
    public int Owner_id { get => owner_id; set => owner_id = value; }
    public (int, int) coordinates { get => (x, y); }
}
