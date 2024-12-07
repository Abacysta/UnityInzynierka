using Assets.classes;
using Assets.classes.subclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Assets.map.scripts;

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/MapData", order = 1)]
[Serializable]
public class Map : ScriptableObject 
{
    [Serializable]
    public enum CountryController 
    {
        Local,
        Ai
    }

    [SerializeField] private string fileName;
    [SerializeField] private List<Province> provinces;
    [SerializeField] private (int, int) popExtremes;
    [SerializeField] private List<Country> countries = new();
    [SerializeField] private List<Army> armies = new();
    [SerializeField] private GameObject army_prefab;
    [SerializeField] private int turnlimit;
    [SerializeField] private int resourceRate;
    [SerializeField] diplomatic_relations_manager diplomacy;
    private List<CountryController> countryControllers = new();
    private List<army_view> armyViews = new();
    private HashSet<Relation> relations = new();
    private int currentPlayerId;
    private int turnCnt = 0;

    public Map() 
    {
        fileName = null;
        provinces = new();
        countries = new();
        armies = new();
    }

    public string FileName { get => fileName; set => fileName = value; }
    public List<Province> Provinces { get => provinces; set => provinces = value; }
    public List<Country> Countries { get => countries; set => countries = value; }
    public (int, int) PopExtremes { get => popExtremes; set => popExtremes = value; }
    public int Turnlimit { get => turnlimit; set => turnlimit = value; }
    public int ResourceRate { get => resourceRate; set => resourceRate = value; }
    public List<Army> Armies { get => armies; set => armies = value; }
    public Country CurrentPlayer { get => countries[currentPlayerId]; }
    internal HashSet<Relation> Relations { get => relations; set => relations = value; }
    public List<CountryController> Controllers { get { return countryControllers; } set => countryControllers = value; }
    public List<army_view> ArmyViews { get => armyViews; set => armyViews = value; }
    public int CurrentPlayerId { get => currentPlayerId; set => currentPlayerId = value; }
    public int TurnCnt { get => turnCnt; set => turnCnt = value; }

    public void addCountry(Country country, CountryController ptype)
    {
        countries.Add(country);
        country.Opinions = new Dictionary<int, int>();
        countryControllers.Add(ptype);
    }

    public void initCountries() {

        for(int i = 1; i < countries.Count; i++) {
            for(int j = 1; j < countries.Count; j++) {
                if(i < j) {
                    countries[i].Opinions.Add(j, 0);
                    countries[j].Opinions.Add(i, 0);
                }
            }
        }
    }

    public void removeCountry(Country country) {
        var idx = countries.FindIndex(c => c == country);
        countryControllers.RemoveAt(idx);
        countries.RemoveAt(idx);
    }

    public void killCountry(Country country) {
        var armiess = armies.Where(c => c.OwnerId == country.Id).ToHashSet();
        foreach(var a in armiess) {
            removeArmy(a);
        }

        var rels = relations.Where(r => r.Sides.Contains(country)).ToHashSet();
        foreach(var r in rels) {
            diplomacy.endRelation(r);
        }

        var warsP = relations.Where(r => r.Type == Relation.RelationType.War).Cast<Relation.War>()
            .Where(w => w.Participants1.Contains(country) || w.Participants2.Contains(country)).ToHashSet();

        foreach(var w in warsP) {
            w.Participants1.Remove(country);
            w.Participants2.Remove(country);
        }

        countryControllers[country.Id] = CountryController.Ai;
        country = new(0, country.Name, country.Capital, country.Color, country.Coat, this);
    }

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
        return Provinces.FindIndex(p => p.X == x && p.Y == y);
    }

    public void calcPopExtremes() {
        int min = Provinces.Min(p => p.Population), max = Provinces.Max(p => p.Population);
        PopExtremes = (min, max);
    }

    public void growPop((int, int) coordinates) {
        Province province = getProvince(coordinates);
        var provinceOwnerTechStats = countries[province.OwnerId].techStats;

        if (province.OccupationInfo.IsOccupied)
        {
            var occupierTechStats = countries[province.OccupationInfo.OccupyingCountryId].techStats;
            province.Population = (int)Math.Floor(province.Population *
                ((provinceOwnerTechStats.PopGrowth * province.Modifiers.PopMod) - occupierTechStats.OccPenalty));
        }
        else
        {
            province.Population = (int)Math.Floor(province.Population *
                provinceOwnerTechStats.PopGrowth * province.Modifiers.PopMod);
        }

        province.Population += (int)Math.Floor(province.Modifiers.PopStatic);
    }

    public void calcRecruitablePop((int, int) coordinates) {
        Province province = getProvince(coordinates);
        var techStats = countries[province.OwnerId].techStats;

        province.RecruitablePopulation = (int)Math.Floor(province.Population * techStats.RecPop * province.Modifiers.RecPop);
    }

    public void calcRecruitablePop(Province p) {
        var techstats = countries[p.OwnerId].techStats;
        p.RecruitablePopulation = (int)Math.Floor(p.Population * techstats.RecPop * p.Modifiers.RecPop);
    }

    public void growHap((int, int) coordinates, int value) {
        Province province = getProvince(coordinates);

        if (province.OccupationInfo.IsOccupied)
        {
            var occupierTechStats = countries[province.OccupationInfo.OccupyingCountryId].techStats;
            province.Happiness += (int)Math.Floor(value * (province.Modifiers.HappMod - occupierTechStats.OccPenalty));
        }
        else province.Happiness += (int)Math.Floor(value * province.Modifiers.HappMod);

        province.Happiness += (int)Math.Floor(province.Modifiers.HappStatic);
    }

    public void assignProvince((int, int) coordinates, int id) {
        var p = getProvince(coordinates);
        assignProvince(p, id);
    }

    public void assignProvince(Province province, int id) {
        if (!countries[id].assignProvince(province)) {
            var c = countries.Find(c => c.Id == province.OwnerId);
            c.unassignProvince(province);
            countries[id].assignProvince(province);
        }
    }

    public (Resource, float) calcResources((int, int) coordinates, int id, float factor) {
        var p = getProvince(coordinates);
        return (p.ResourceType, p.ResourceAmount);
    }

    public void addArmy(Army army) {
        armies.Add(army);
        createArmyView(army);
    }

    public void removeArmy(Army army) {
        armies.Remove(army);
        destroyArmyView(army);
    }

    public void reloadArmyView(Army army) {
        destroyArmyView(army);
        createArmyView(army);
    }

    public Dictionary<Resource, float> getResourceGain(Country country) {
        var prod = new Dictionary<Resource, float> {
            { Resource.Gold, 0 },
            { Resource.Wood, 0 },
            { Resource.Iron, 0 },
            { Resource.SciencePoint, 0 },
            { Resource.AP, 0 }
        };

        foreach (var prov in country.Provinces) {
            prod[prov.ResourceType] += prov.ResourcesP;
            prod[Resource.AP] += 0.1f;

            if (prov.Buildings.ContainsKey(BuildingType.School) &&
                prov.GetBuildingLevel(BuildingType.School) < 4) {
                prod[Resource.SciencePoint] += prov.GetBuildingLevel(BuildingType.School) * 3;
            }
        }

        foreach (var type in prod.ToList()) {
            if (type.Key != Resource.AP)
                prod[type.Key] *= country.techStats.ProdFactor;
        }

        foreach (var army in getCountryArmies(CurrentPlayer)) {
            prod[Resource.Gold] -= (army.Count / 10 + 1) * country.techStats.ArmyUpkeep;
        }

        foreach (var type in prod.ToList()) {
            prod[type.Key] = (float)Math.Round(prod[type.Key], 1);
        }

        prod[Resource.AP] += 2.5f;
        return prod;
    }

    public void createArmyView(Army army) {
        var rtype = GetHardRelationType(CurrentPlayer, countries[army.OwnerId]);
        GameObject armyObject = Instantiate(army_prefab, 
            new Vector3(army.Position.Item1, army.Position.Item2, 0), Quaternion.identity);
        army_view armyView = armyObject.GetComponent<army_view>();

        if (army.OwnerId != 0)
            armyView.Initialize(army, rtype);
        else armyView.Initialize(army, Relation.RelationType.Rebellion);

        armyViews.Add(armyView);
    }

    public void destroyArmyView(Army army) {
        army_view armyView = armyViews.Find(view => view.ArmyData == army);
        if (armyView != null) {
            armyViews.Remove(armyView);
            Destroy(armyView.gameObject);
        }
    }

    public void recArmy((int, int) coordinates, int amount) {
        var province = getProvince(coordinates);
        var exitsing = armies.Find(a => a.Position == coordinates && a.Position == a.Destination);
        if (province.RecruitablePopulation >= amount) {
            province.Population -= amount;
            province.RecruitablePopulation -= amount;
            if (exitsing == null) {
                addArmy(new(province.OwnerId, amount, coordinates, coordinates));
            }
            else {
                exitsing.Count += amount;
            }
        }
    }

    public void DisbandArmy(Army army, int amount)
    {
        var province = getProvince(army.Position);

        if (army.Count == amount) {
            removeArmy(army);
        }
        else {
            army.Count -= amount;
        }
        province.Population += amount;
    }

    public void UndoDisbandArmy(Army army, int amount)
    {
        var province = getProvince(army.Position);

        if (army.Count == amount) {
            addArmy(army);
        }
        else {
            army.Count += amount;
        }

        province.Population -= amount;
    }

    public army_view getView(Army army) {
        return armyViews.Find(view => view.ArmyData == army);
    }
    public void updateArmyPosition(Army army, (int, int) coordinates) {
        Province currentProvince = getProvince(army.Position);
        army_view armyView = armyViews.Find(view => view.ArmyData == army);

        if (armyView != null) {
            if (currentProvince != null && army.OwnerId != 0) {
                if (ShouldCancelOccupation(currentProvince, army.OwnerId)) {
                    CancelOccupation(currentProvince);
                }
            }
            armyView.MoveTo(coordinates);
            if (army.OwnerId != 0 && ShouldCancelOccupation(getProvince(armyView.ArmyData.Position), army.OwnerId)) {
                CancelOccupation(getProvince(armyView.ArmyData.Position));
            }
        }
        AddOccupation(army);
    }
    private bool ShouldCancelOccupation(Province province, int armyOwnerId) {
        if (province.OccupationInfo != null && province.OccupationInfo.IsOccupied) {
            Country occupyingCountry = Countries.FirstOrDefault(c => c.Id == province.OccupationInfo.OccupyingCountryId);

            if (occupyingCountry != null) {
                bool isAllyOrVassal = false;
                Relation.RelationType? relation = GetHardRelationType(Countries[armyOwnerId], occupyingCountry);

                isAllyOrVassal = relation == Relation.RelationType.Alliance ||
                                relation == Relation.RelationType.Vassalage ||
                                armyOwnerId == province.OccupationInfo.OccupyingCountryId;

                return !isAllyOrVassal;
            }
        }
        return false;
    }

    public void updateArmyDestination(Army army, (int, int) coordinates) {
        army.Destination = coordinates;
    }

    public float calcArmyCombatPower(Army army) {
        var stats = countries[army.OwnerId].techStats;
        return army.Count + (army.Count * stats.ArmyPower);
    }

    public void moveArmies() {
        int it = 0;
        foreach (var army in armies) {

            if (army.Position != army.Destination) {
                MoveArmy(army);
            }
            Debug.Log(army.Position != army.Destination ? ("army" + it++ + "in" + army.Position.ToString() + "hasn't moved") 
                : ("army" + it++ + "in" + army.Position.ToString() + "has moved to" + army.Destination.ToString()));
        }
    }

    public void MoveArmy(Army army) {
        updateArmyPosition(army, army.Destination);
        updateArmyDestination(army, army.Position);
    }

    public void undoSetMoveArmy(Army army) {
        army_view view = getView(army);
        if (view != null) {
            view.ReturnTo(army.Position);
        }
        mergeToProvince(getProvince(army.Position), army);
    }

    public Army setMoveArmy(Army army, int count, (int, int) destination) {
        if (count <= army.Count) {
            Army moved_army;

            if (count == army.Count) {
                updateArmyDestination(army, destination);
                moved_army = army;
            }
            else {
                moved_army = new Army(army) {
                    Destination = destination
                };
                army.Count -= count;
                moved_army.Count = count;

                addArmy(moved_army);

            }
            var armyView = armyViews.Find(view => view.ArmyData == moved_army);
            if (armyView != null) {
                armyView.PrepareToMoveTo(destination);
            }
            return moved_army;
        }
        return army;
    }

    public void mergeArmies(Country country) {
        List<Army> ar = armies.Where(a => a.OwnerId == country.Id).ToList();
        var grouped = ar.GroupBy(a => a.Position)
            .Select(gr => new {
                pos = gr.Key,
                count = gr.Sum(a => a.Count),
                ars = gr.ToList()
            }).ToList();
        foreach (var group in grouped) {
            foreach (var army in group.ars) {
                removeArmy(army);
            }

            Army merged = new(country.Id, group.count, group.pos, group.pos);
            addArmy(merged);
        }
    }

    private void mergeToProvince(Province province, Army to_merge) {
        Army base_ = armies.Find(a => a.OwnerId == to_merge.OwnerId && 
            a.Position == province.coordinates && a.Destination == a.Position);

        if (base_ != null) {
            base_.Count += to_merge.Count;
            removeArmy(to_merge);
        }
        else {
            updateArmyDestination(to_merge, province.coordinates);
        }
    }

    public List<(int, int)> getPossibleMoveCells(Army army)
    {
        (int startX, int startY) = army.Position;
        Country country = Countries.FirstOrDefault(c => c.Id == army.OwnerId);
        return getPossibleMoveCells(startX, startY, country.techStats.MoveRange, country.techStats.WaterMoveFactor);
    }

    public List<(int, int)> getPossibleMoveCells(Province province, int range)
    {
        return getPossibleMoveCells(province.X, province.Y, range, range);
    }

    public List<(int, int)> getPossibleMoveCells(int startX, int startY, int moveRangeLand, double waterMoveFactor)
    {
        List<(int, int)> possibleCells = new List<(int, int)>();
        bool isStartTerrainLand = getProvince(startX, startY).IsLand;
        HexUtils.Cube startCube = HexUtils.OffsetToCube(startX, startY);

        int moveRangeWater = (int)Math.Floor(moveRangeLand + moveRangeLand * waterMoveFactor);

        Queue<(HexUtils.Cube, int)> frontier = new Queue<(HexUtils.Cube, int)>(); // queue with hex to be checked
        frontier.Enqueue((startCube, 0)); // define entry point to que with range 0

        HashSet<(int, int)> visited = new HashSet<(int, int)>();
        visited.Add((startX, startY));

        while (frontier.Count > 0) // it goes thru all possible hexes
        {
            var (current, currentDistance) = frontier.Dequeue(); // take current hex to do smth with it
            (int currentX, int currentY) = HexUtils.CubeToOffset(current); // 3d to 2d
            possibleCells.Add((currentX, currentY)); // adds to list couse it can move there

            int currentMoveRange = isStartTerrainLand ? moveRangeLand : moveRangeWater;

            for (int dir = 0; dir < 6; dir++) 
            {
                // calculate negighbor based on direction vector
                HexUtils.Cube neighbor = HexUtils.CubeNeighbor(current, dir);
                (int neighborX, int neighborY) = HexUtils.CubeToOffset(neighbor);

                if (!IsValidPosition(neighborX, neighborY)) continue; // we dont want to check outside of map

                // check type cause we want diffrent range "Land" and "water"
                bool isNeighborTerrainLand = getProvince(neighborX, neighborY).IsLand;

                if (!visited.Contains((neighborX, neighborY))) {
                    if (isNeighborTerrainLand == isStartTerrainLand) {
                        if (currentDistance + 1 <= currentMoveRange) {
                            visited.Add((neighborX, neighborY));
                            frontier.Enqueue((neighbor, currentDistance + 1));
                        }
                    }
                    else {
                        if (currentDistance < currentMoveRange) {
                            visited.Add((neighborX, neighborY));
                            frontier.Enqueue((neighbor, currentMoveRange));
                        }
                    }
                }
            }
        }

        return possibleCells;
    }

    public List<Army> getCountryArmies(Country country)
    {
        return armies.FindAll(a => a.OwnerId == country.Id);
    }

    public bool IsValidPosition(int x, int y)
    {
        Province province = getProvince(x, y);

        return province != null;
    }

    public void ManageOccupationDuration(Province province)
    {
        if (province.OccupationInfo.IsOccupied)
        {
            province.OccupationInfo.OccupationCount--;

            if (province.OccupationInfo.OccupationCount <= 0) {
                OccupationChangeOwner(province);
            }
        }
    }

    private void OccupationChangeOwner(Province province) {
        int previousOwnerId = province.OwnerId;
        int newOwnerId = province.OccupationInfo.OccupyingCountryId;

        Countries.FirstOrDefault(c => c.Id == previousOwnerId).removeProvince(province);

        assignProvince(province, newOwnerId);
        CancelOccupation(province);
    }

    private void AddOccupation(Army army) {
        Province province = getProvince(army.Position.Item1, army.Position.Item2);
        Country country = Countries.FirstOrDefault(c => c.Id == army.OwnerId);
        Occupation occupationStatus = null;
        Country master = getMaster(country);

        if (province.OccupationInfo != null && province.OccupationInfo.OccupyingCountryId == army.OwnerId)
        {
            return;
        }

        if (province.OwnerId == 0 && province.OccupationInfo.OccupyingCountryId != army.OwnerId) {
            CancelOccupation(province);
            occupationStatus = new Occupation(1, army.OwnerId);
        }
        else {
            Country provinceOwner = Countries.FirstOrDefault(c => c.Id == province.OwnerId);
            Relation.RelationType? relation = null;

            if (master != null) {
                relation = GetHardRelationType(master, provinceOwner);
            }
            else
            {
                relation = GetHardRelationType(country, provinceOwner);
            }

            if (relation == Relation.RelationType.War) {
                if (master != null) {
                    occupationStatus = new Occupation(country.techStats.OccTime, master.Id);
                }
                else {
                    occupationStatus = new Occupation(country.techStats.OccTime, army.OwnerId);
                }
            }
            else if (relation == Relation.RelationType.Rebellion) {
                CancelOccupation(province);
                return;
            }

            if (relation == Relation.RelationType.Vassalage) {
                CancelOccupation(province);
            }

            if (province.OccupationInfo.OccupyingCountryId == 0) {
                CancelOccupation(province);
            }
        }

        if (occupationStatus != null && province.IsLand) {
            province.addStatus(occupationStatus);
            province.OccupationInfo = new OccupationInfo(true, occupationStatus.Duration + 1, army.OwnerId);
        }
    }

    public void CancelOccupation(Province province)
    {
        province.OccupationInfo.IsOccupied = false;
        province.OccupationInfo.OccupationCount = 0;
        province.OccupationInfo.OccupyingCountryId = -1;
        province.Statuses.RemoveAll(status => status is Occupation);
    }

    public HashSet<Relation> getRelationsOfType(Country country, Relation.RelationType type) {
        HashSet<Relation> result = new HashSet<Relation>();
        foreach (var r in relations) {
            if (r.Type == type && r.Sides.Contains(country))
                result.Add(r);
        }
        return result;
    }

    public Relation.RelationType? GetHardRelationType(Country c1, Country c2) { // 0 master 1 slave
        var rr = relations.FirstOrDefault(r =>
            r.Sides.Contains(c1) && r.Sides.Contains(c2) &&
            (r.Type == Relation.RelationType.War ||
             r.Type == Relation.RelationType.Alliance ||
             r.Type == Relation.RelationType.Vassalage ||
             r.Type == Relation.RelationType.Truce)
        );

        if (rr != null) {
            if (rr.Type == Relation.RelationType.War) {
                var war = rr as Relation.War;

                if ((war.Participants1.Contains(c1) && war.Participants2.Contains(c2)) ||
                    (war.Participants2.Contains(c1) && war.Participants1.Contains(c2))) {
                    return Relation.RelationType.War;
                }
            }
            else {
                return rr.Type;
            }
        }

        return null;
    }

    public Country getSeniorIfExists(Country country) {
        Relation.Vassalage senior = relations.FirstOrDefault(r => r.Sides[1] == country) as Relation.Vassalage;
        if (senior != null) {
            return senior.Sides[0];
        }
        else return null;
    }

    public bool hasRelationOfType(Country c1, Country c2, Relation.RelationType type) {
        if (relations.Any(r => r.Sides.Contains(c1) && r.Sides.Contains(c2) && r.Type == type)) return true;
        return false;
    }

    public Country getMaster(Country country) {
        foreach (var relation in Relations) {
            if (relation.Type == Relation.RelationType.Vassalage && relation.Sides[1] == country) {
                return relation.Sides[0];
            }
        }
        return null;
    }
    public Map getSaveData() {
        return (Map)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(this, 
            Formatting.Indented, new JsonSerializerSettings {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        }));
    }

    public void reloadArmyViews() {
        foreach (var av in armyViews) {
            Destroy(av.gameObject);
        }
        armyViews = null;
        foreach (var a in armies) {
            createArmyView(a);
        }
    }

    public void destroyAllArmyViews() {
        armyViews = new();
    }

    public List<army_view> GetAllArmyViews() {
        return armyViews;
    }

    public void UpdateAllArmyViewOrders() {
        var allArmyViews = GetAllArmyViews();
        foreach (army_view armyView in allArmyViews) {
            if (armyView != null) 
            {
                armyView.UpdateArmyViewSortingOrder(armyView);
            }
            else {
                Debug.LogWarning("armyView is null in UpdateAllArmyViewOrders()");
            }
        }
    }

    /// <summary>
    /// utilites for gathering data about wars
    /// </summary>
    internal class WarUtilities{
        public static Relation.War getWar(Map map, Country c1, Country c2) {
            return map.getRelationsOfType(c1, Relation.RelationType.War).First(r => r.Sides.Contains(c2)) as Relation.War;
        }

        public static Relation.War getWar(Map map, int c1, int c2) {
            return getWar(map, map.Countries[c1], map.Countries[c2]);
        }

        public static (int, int) getSidePowers(Map map, Relation.War war) {
            int atkP = 1, defP = 1;
            if(war.Participants1!=null) foreach (var a in war.Participants1) {
                atkP += map.getCountryArmies(a).Count;
            }
            if(war.Participants2!=null) foreach (var d in war.Participants2){
                defP += map.getCountryArmies(d).Count;
            }
            return (atkP, defP);
        }

        public static bool isAttackersStronger(Map map, Relation.War war) {
            var stats = getSidePowers(map, war);
            return stats.Item1 > stats.Item2;
        }

        public static bool isAttacker(Map map, Country c, Relation.War war) {
            return war.Participants1.Contains(c);
        }

        public static HashSet<Relation.War> getAllWars(Map map, Country c) => 
            map.getRelationsOfType(c, Relation.RelationType.War).Cast<Relation.War>().ToHashSet();

        public static HashSet<Army> getEnemyArmies(Map map, Country c) {
            HashSet<Army> enemy = new();

            foreach (var war in getAllWars(map, c)) {
                var opposingParticipants = war.Participants1.Contains(c) ? war.Participants2 : war.Participants1;
                foreach (var country in opposingParticipants) {
                    enemy.UnionWith(map.getCountryArmies(country));
                }
            }

            return enemy;
        }

        public static HashSet<int> getEnemyIds(Map map, Country c) {
            HashSet<int> ids = new();
            foreach (var war in getAllWars(map, c)) {
                var opposingParticipants = war.Participants1.Contains(c) ? war.Participants2 : war.Participants1;
                foreach (var country in opposingParticipants) {
                    ids.Add(country.Id);
                }
            }
            return ids;
        }

        public static HashSet<Army> getEnemyArmiesInProvince(Map map, Country c, Province p) 
            => getEnemyArmies(map, c).Where(a => a.Position == p.coordinates).ToHashSet();

        public static HashSet<Province> getEnemyProvinces(Map map, Country c) {
            HashSet<Province> prov = new();
            foreach(var id in getEnemyIds(map, c)) {
                prov.UnionWith(map.Countries[id].Provinces);
            }
            return prov;
        }

        public static bool isAtWarWIth(Map map, Country c1, Country c2)=> getEnemyIds(map, c1).Contains(c2.Id);

        public static HashSet<Country> getAllies(Map map, Country c)
            => map.getRelationsOfType(c, Relation.RelationType.Alliance)
            .Cast<Relation.Alliance>().Select(a => a.Sides.First(s => s != c)).ToHashSet();
    }

    /// <summary>
    /// utilites for managing borders and armies
    /// </summary>
    internal class LandUtilites {
        public static HashSet<Country> borderingCountries(Map map, Country country) {
            var bordering = new HashSet<Country>();
            //...
            return bordering;
        }

        public static HashSet<Province> getViableArmyTargets(Map map, Country country) {
            var provinces = new HashSet<Province>();
            //...
            return provinces;
        }

        // likely need to replace numbers with something else
        public static List<Province> getUnhappyProvinces(Country country) {
            return country.Provinces.Where(p=>p.Happiness<40 && !p.Statuses.Any(s=>s.Id == 1)).ToList();
        }

        public static List<Province> getGrowable(Country c) {
            return c.Provinces.Where(p => (p.ResourceType == Resource.Gold || p.Population < 400) 
                && !p.Statuses.Any(s=>s.Id== 2)).ToList();
        }

        public static List<Province> getOptimalRecruitmentProvinces(Country c) {
            return c.Provinces.Where(p=>p.RecruitablePopulation >= 50).OrderByDescending(p=>p.Population).ToList();
        }

        public static bool recruitAllAvailable(Country c, Province p) {
            if (c.Resources[Resource.AP] >= 1) {
                c.Actions.addAction(new TurnAction.ArmyRecruitment(p.coordinates,
                    p.RecruitablePopulation*c.techStats.ArmyCost <= c.Resources[Resource.Gold] ? p.RecruitablePopulation 
                    : (int)Math.Floor(c.Resources[Resource.Gold]/c.techStats.ArmyCost), c.techStats));
            }
            return p.RecruitablePopulation == 0;
        }

        public static HashSet<Province> getUnpassableProvinces(Map map, Country c) {
            HashSet<Province> unpassable = new();

            if (!c.techStats.CanBoat) {
                unpassable.UnionWith(map.Provinces.FindAll(p => p.IsLand).ToHashSet());
            }

            HashSet<int> accessible = new();

            foreach(var r in map.Relations) {
                if (r.Type == Relation.RelationType.MilitaryAccess && r.Sides[1].Id == c.Id)
                    accessible.Add(r.Sides[1].Id);
                else if (r.Type == Relation.RelationType.Alliance || r.Type == Relation.RelationType.Vassalage)
                {
                    if (r.Sides.Any(rs => rs.Id == c.Id))
                    {
                        accessible.Add(r.Sides[0].Id);
                        accessible.Add(r.Sides[1].Id);
                    }
                    else if (r.Type == Relation.RelationType.War)
                    {
                        if ((r as Relation.War).Participants1.Contains(c) || (r as Relation.War).Participants2.Contains(c))
                        {
                            accessible.UnionWith((r as Relation.War).Participants1.Select(part => part.Id).ToHashSet());
                            accessible.UnionWith((r as Relation.War).Participants2.Select(part => part.Id).ToHashSet());
                        }
                    }
                }
            }

            accessible.Remove(c.Id);

            foreach(var province in map.Provinces) {
                if (province.OwnerId != c.Id && !accessible.Contains(province.OwnerId))
                    unpassable.Add(province);
            }

            return unpassable;
        }
        
    }

    /// <summary>
    /// utilities regarding power projection
    /// </summary>
    internal class PowerUtilites {
        public static bool isArmyStronger(Map map, Country c1, Country c2) {
            return map.Armies.FindAll(a => a.OwnerId == c1.Id).Sum(a=>a.Count) * 
                c1.techStats.ArmyPower > map.Armies.FindAll(a => a.OwnerId == c2.Id).Sum(a => a.Count) * c2.techStats.ArmyPower;
        }

        public static float howArmyStronger(Map map, Country c1, Country c2) {
            return (map.Armies.FindAll(a => a.OwnerId == c1.Id).Sum(a => a.Count) * c1.techStats.ArmyPower) 
                / (map.Armies.FindAll(a => a.OwnerId == c2.Id).Sum(a => a.Count) * c2.techStats.ArmyPower);
        }

        public static int getOpinion(Country of, Country from) {
            return from.Opinions[of.Id];
        }

        public static Dictionary<Resource, float> getGain(Map map, Country country) {
            var gain = new Dictionary<Resource, float> {
                { Resource.Gold, 0 },
                { Resource.Wood, 0 },
                { Resource.Iron, 0 },
                { Resource.SciencePoint, 0 },
                { Resource.AP, 0 }
            };

            var tax = getTaxGain(country);
            var prod = map.getResourceGain(country);

            foreach (var res in gain.Keys.ToList()) {
                if (res == Resource.Gold) gain[res] += tax;
                gain[res] += prod[res];
                gain[res] = (float)Math.Round(gain[res], 1);
            }

            return gain;
        }

        internal static float getTaxGain(Country country) {
            var tax = 0f;
            foreach (var prov in country.Provinces) {
                tax += (prov.Population / 10) * country.Tax.GoldP;
            }
            tax *= country.techStats.TaxFactor;

            return (float)Math.Round(tax, 1);
        }

        public static float getGoldGain(Map map, Country c) {
            return getGain(map, c)[Resource.Gold]; 
        }

        public static float getArmyUpkeep(Map map, Country c) {
            float res = 0;
            foreach (var army in map.getCountryArmies(map.CurrentPlayer)) {
                 res -= (army.Count / 10 + 1) * map.Countries[c.Id].techStats.ArmyUpkeep;
            }
            return res;
        }

        public static HashSet<Country> getVassals(Map map, Country c) => 
            map.Relations.Where(r => r.Type == Relation.RelationType.Vassalage && r.Sides[0] == c)
            .Cast<Relation.Vassalage>().Select(v => v.Sides[0]).ToHashSet();

        public static Country getSenior(Map map, Country c) => map.Relations
            .Where(r => r.Type == Relation.RelationType.Vassalage && r.Sides[1] == c)
            .Cast<Relation.Vassalage>().Select(v => v.Sides[0]).First();

        public static HashSet<Relation.Vassalage> getVassalRelations(Map map, Country c) => 
            map.Relations.Where(r => r.Type == Relation.RelationType.Vassalage && r.Sides[0] == c)
                .Cast<Relation.Vassalage>().ToHashSet();

        public static Relation.Vassalage getVassalage(Map map, Country c) => map.Relations
            .Where(r => r.Type == Relation.RelationType.Vassalage && r.Sides.Any(s => s.Equals(c)))
            .Cast<Relation.Vassalage>().First();

        public static HashSet<Country> getWeakCountries(Map map, Country c) {
            HashSet<Country> weaklings = new();

            for (int i = 1; i < map.Countries.Count; i++) { 
                if(i==c.Id) continue;
                if (c.Opinions[i] > 120) continue;
                if (c.Opinions[i] < -120) {
                    weaklings.Add(c);
                    continue;
                }
                var cc = map.Countries[i];
                if (c.Technologies[Technology.Military] > cc.Technologies[Technology.Military]+ 4) {
                    weaklings.Add(cc);
                    continue;
                }
                if(howArmyStronger(map, c, cc) > 2) {
                    weaklings.Add(cc);
                    continue;
                }
            }

            return weaklings;
        }    
    }
}
