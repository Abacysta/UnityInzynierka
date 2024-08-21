using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/MapData", order = 1)]
public class Map:ScriptableObject {
    [SerializeField] private string map_name;
    [SerializeField] private string file_name;
    [SerializeField] private List<Province> provinces;
    [SerializeField] (int, int) selected_province;
    [SerializeField] (int, int) pop_extremes;
    [SerializeField] private List<Country> countries = new List<Country>();
    [SerializeField] private List<Army> armies = new List<Army>();
    [SerializeField] private GameObject army_prefab;
    private List<army_view> armyViews = new List<army_view>();

    public string Map_name { get => map_name; set => map_name = value; }
    public string File_name { get => file_name; set => file_name = value; }
    public List<Province> Provinces { get => provinces; set => provinces = value; }

    public List<Country> Countries { get => countries; set => countries = value; }

    public (int, int) Selected_province { get => selected_province; set => selected_province = value; }
    public (int, int) Pop_extremes { get => pop_extremes; set => pop_extremes = value; }
    public List<Army> Armies { get => armies; set => armies = value; }

    public Province getProvince(int x, int y) {
        return Provinces.Find(p => p.X == x && p.Y == y);
    }

    public Province getProvince((int, int) coordinates) {
        return getProvince(coordinates.Item1, coordinates.Item2);
    }

    public int getProvinceIndex((int, int) coordinates) {
        return Provinces.FindIndex(p => p.X == coordinates.Item1 && p.Y == coordinates.Item2);
    }

    public int getProvinceIndex(int x, int y) { 
        return Provinces.FindIndex(p=>p.X == x && p.Y == y);
    }

    public void calcPopExtremes() {
        int min = Provinces.Min(p=>p.Population), max = Provinces.Max(p=>p.Population);
        Pop_extremes=(min,max);
    }

    public void growPop((int, int) coordinates) {
        int prov = getProvinceIndex(coordinates);
        var stats = countries[Provinces[prov].Owner_id].techStats;
        Provinces[prov].Population += (int)Math.Floor(Provinces[prov].Population * stats.popGrowth);
    }

    public void calcRecruitablePop((int, int) coordinates) {
        int prov = getProvinceIndex(coordinates);
        var stats = countries[Provinces[prov].Owner_id].techStats;
        Provinces[prov].RecruitablePopulation = (int)Math.Floor(Provinces[prov].Population * stats.recPop);
    }

    public void growHap((int, int) coordinates, int value) {
        int prov = getProvinceIndex(coordinates);
        Provinces[prov].Happiness += value;
    }

    
    //jebnij sie w leb

    //public void addBuilding((int,int) coordinates, Building building)
    //{
    //    int prov = getProvinceIndex(coordinates);
    //    bool buildingExist = Provinces[prov].Buildings.Any(b => b.BuildingType == building.BuildingType);
    //    if(!buildingExist)
    //    {
    //        Provinces[prov].Buildings.Add(building);
    //    }
    //    else
    //    {
    //        Debug.Log("Building of this type already exists!");
    //    }
    //}

    public void upgradeBuilding((int,int) coordinates, BuildingType buildingType)
    {
        int prov = getProvinceIndex(coordinates);
        Provinces[prov].Buildings.Find(b => b.BuildingType == buildingType).Upgrade();
        Debug.Log(getProvince(coordinates).Buildings.ToString());
    }
    public void downgradeBuilding((int,int) coordinates, BuildingType buildingType)
    {
        int prov = getProvinceIndex(coordinates);
        Provinces[prov].Buildings.Find(b => b.BuildingType == buildingType).Downgrade();
        Debug.Log(getProvince(coordinates).Buildings.ToString());
    }

    public void assignProvince((int, int) coordinates, int id) {
        getProvince(coordinates).Owner_id = id;
        if(!countries[id].Provinces.Contains(coordinates)){
            countries[id].addProvince(coordinates);
        }
    }

    public (Resource, float) calcResources((int, int) coordinates, int id, float factor) {
        var p = getProvince(coordinates);
        return (p.ResourcesT, p.Resources_amount);
    }

    public void addArmy(Army army)
    {
        armies.Add(army);
        createArmyView(army);
    }
    public void removeArmy(Army army)
    {
        armies.Remove(army);
        destroyArmyView(army);
    }
    private void createArmyView(Army army)
    {
        GameObject armyObject = Instantiate(army_prefab, new Vector3(army.Position.Item1, army.Position.Item2, 0), Quaternion.identity);
        army_view armyView = armyObject.GetComponent<army_view>();
        armyView.Initialize(army);
        armyViews.Add(armyView);
    }
    private void destroyArmyView(Army army)
    {
        army_view armyView = armyViews.Find(view => view.ArmyData == army);
        if(armyView != null)
        {
            armyViews.Remove(armyView);
            Destroy(armyView.gameObject);
        }
    }

    public void recArmy((int, int) coordinates, int amount) {
        var province = getProvince(coordinates);
        if(province.RecruitablePopulation <= amount) { 
            addArmy(new(province.Owner_id, amount, coordinates, coordinates));
            province.Population -= amount;
            province.RecruitablePopulation -= amount;
        }
    }

    public army_view getView(Army army) {
        return armyViews.Find(view => view.ArmyData == army);
    }

    public void updateArmyPosition(Army army, (int,int) coordinates) // narazie dziala to tak ¿e jak armia wejdzie na panstwo odrazu jest przypisywana do owner id kontrollera armii
    {
        Province previousProvince = getProvince(coordinates);
        if (previousProvince != null)
        {
            Country previousOwner = Countries.FirstOrDefault(c => c.Id == previousProvince.Owner_id);
            if (previousOwner != null)
            {
                previousOwner.removeProvince(coordinates); // ususniecie prowincji z listy posiadanych prowincji orginalnego posiadacza prowincji
            }
        }

        army_view armyView = getView(army);
        if(armyView != null)
        {
            armyView.MoveTo(coordinates);
        }
        assignProvince(coordinates, army.OwnerId);  // dodanie prowincji do kontrollera armii 
    }
    public void updateArmyDestination(Army army, (int,int) coordinates)
    {
        army.destination = coordinates;
    }
    public float calcArmyCombatPower(Army army)
    {
        var stats = countries[army.OwnerId].techStats;
        return army.count + (army.count * stats.armyPower);
    }
    public void moveArmies()
    {
        int it = 0;
        foreach(var army in armies)
        {
            
            if(army.position != army.destination)
            {
                MoveArmy(army);   
            }
            Debug.Log(army.position != army.destination ? ("army" + it++ + "in" + army.position.ToString() + "hasn't moved") : ("army" + it++ + "in" + army.position.ToString() + "has moved to" + army.destination.ToString()));

        }
    }

    public void MoveArmy(Army army) {
        updateArmyPosition(army, army.destination);
        updateArmyDestination(army, army.position);
    }

    public void disbandArmy(Army army, int count) {
        if(count <= army.count) {
            if(count == army.count) {
                removeArmy(army);
                return;
            }
            var province = getProvince(army.position);
            province.Population += army.count / 2;
            var army_dest = Army.makeSubarmy(army, count);
        }
    }

    public void setMoveArmy(Army army, int count, (int, int) destination)
    {
        if (count <= army.count)
        {
            Army moved_army;

            if (count == army.count)
            {
                updateArmyDestination(army, destination);
                moved_army = army;
            }
            else
            {
                moved_army = new Army(army)
                {
                    destination = destination
                };
                army.count -= count;
                moved_army.count = count;

                addArmy(moved_army);
            }

            var armyView = armyViews.Find(view => view.ArmyData == moved_army);

            if (armyView != null)
            {
                armyView.PrepareToMoveTo(destination);
            }
        }
    }

    //I LOVE LINQ
    public void mergeArmies(Country country) {
        List<Army> ar = armies.Where(a => a.OwnerId == country.Id).ToList();
        var grouped =  ar.GroupBy(a => a.Position)
            .Select(gr => new {
                pos = gr.Key,
                count = gr.Sum(a => a.Count),
                ars = gr.ToList()
            }).ToList();
        foreach(var group in grouped) {
            foreach(var army in group.ars) { 
                removeArmy(army);
            }

            Army merged = new(country.Id, group.count, group.pos, group.pos);
            addArmy(merged);
        }
    }



    public List<(int, int)> getPossibleMoveCells(Army army)
    {
        List<(int, int)> possibleCells = new List<(int, int)>();
        (int startX, int startY) = army.position;
        Country country = Countries.FirstOrDefault(c => c.Id == army.ownerId);
        int moveRangeLand = country.techStats.moveRange;
        int moveRangeWater = (int)Math.Floor(country.techStats.moveRange + country.techStats.moveRange * country.techStats.waterMoveFactor);

        HexUtils.Cube startCube = HexUtils.OffsetToCube(startX, startY);
        Queue<(HexUtils.Cube, int, string)> frontier = new Queue<(HexUtils.Cube, int, string)>();
        frontier.Enqueue((startCube, 0, getProvince(startX, startY).Type));

        HashSet<(int, int)> visited = new HashSet<(int, int)>();
        visited.Add((startX, startY));

        while (frontier.Count > 0)
        {
            var (current, currentDistance, currentTerrain) = frontier.Dequeue();
            (int currentX, int currentY) = HexUtils.CubeToOffset(current);
            possibleCells.Add((currentX, currentY));

            int currentMoveRange = currentTerrain == "land" ? moveRangeLand : moveRangeWater;

            if (currentDistance < currentMoveRange)
            {
                for (int dir = 0; dir < 6; dir++)
                {
                    HexUtils.Cube neighbor = HexUtils.CubeNeighbor(current, dir);
                    (int neighborX, int neighborY) = HexUtils.CubeToOffset(neighbor);

                    if (!IsValidPosition(neighborX, neighborY)) continue;

                    string neighborTerrain = getProvince(neighborX, neighborY).Type;
                    int nextDistance = currentTerrain == neighborTerrain ? currentDistance + 1 : currentDistance + 1;

                    if (!visited.Contains((neighborX, neighborY)) && nextDistance <= currentMoveRange)
                    {
                        visited.Add((neighborX, neighborY));
                        frontier.Enqueue((neighbor, nextDistance, neighborTerrain));
                    }
                }
            }
        }

        return possibleCells;
    }
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x <= 79 && y >= 0 && y <= 79;
    }
}
