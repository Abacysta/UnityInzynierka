using Assets.classes;
using Assets.classes.subclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Assets.map.scripts;
using Codice.Client.BaseCommands;
using static Assets.classes.Relation;

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
    [SerializeField] private List<CountryController> countryControllers = new();
    [SerializeField] private List<army_view> armyViews = new();
    [SerializeField] private HashSet<Relation> relations = new();
    [SerializeField] private int currentPlayerId;
    [SerializeField] private int turnCounter = 0;
    [SerializeField] diplomatic_relations_manager diplomacy;

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
    public int TurnCnt { get => turnCounter; set => turnCounter = value; }
    internal diplomatic_relations_manager Diplomacy { get => diplomacy; set => diplomacy = value; }

    public void AddCountry(Country country, CountryController ptype)
    {
        countries.Add(country);
        countryControllers.Add(ptype);
    }

    public void InitCountryOpinions() {

        for (int i = 1; i < countries.Count; i++) {
            for (int j = 1; j < countries.Count; j++) {
                if (i < j) {
                    countries[i].Opinions.Add(j, 0);
                    countries[j].Opinions.Add(i, 0);
                }
            }
        }
    }

    public void RemoveCountry(Country country) {
        var idx = countries.FindIndex(c => c == country);
        countryControllers.RemoveAt(idx);
        countries.RemoveAt(idx);
    }

    public void KillCountry(Country country) {
        var armiess = armies.Where(c => c.OwnerId == country.Id).ToHashSet();
        foreach(var a in armiess) {
            RemoveArmy(a);
        }

        var rels = relations.Where(r => r.Sides.Contains(country)).ToHashSet();
        foreach(var r in rels) {
            diplomacy.EndRelation(r);
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

    public Province GetProvince(int x, int y) {
        return Provinces.Find(p => p.X == x && p.Y == y);
    }

    public Province GetProvince((int, int) coordinates) {
        return GetProvince(coordinates.Item1, coordinates.Item2);
    }

    public int GetProvinceIndex((int, int) coordinates) {
        return Provinces.FindIndex(p => p.X == coordinates.Item1 && p.Y == coordinates.Item2);
    }

    public int GetProvinceIndex(int x, int y) {
        return Provinces.FindIndex(p => p.X == x && p.Y == y);
    }

    public void CalcPopulationExtremes() {
        int min = Provinces.Min(p => p.Population), max = Provinces.Max(p => p.Population);
        PopExtremes = (min, max);
    }

    public void AssignProvince((int, int) coordinates, int id) {
        var p = GetProvince(coordinates);
        AssignProvince(p, id);
    }

    public void AssignProvince(Province province, int id) {
        if (!countries[id].AssignProvince(province)) {
            var c = countries.Find(c => c.Id == province.OwnerId);
            c.UnassignProvince(province);
            countries[id].AssignProvince(province);
        }
    }

    public void AddArmy(Army army) {
        armies.Add(army);
        CreateArmyView(army);
    }

    public void RemoveArmy(Army army) {
        armies.Remove(army);
        DestroyArmyView(army);
    }

    public void ReloadArmyView(Army army) {
        DestroyArmyView(army);
        CreateArmyView(army);
    }

    public void CreateArmyView(Army army) {
        var rtype = GetHardRelationType(CurrentPlayer, countries[army.OwnerId]);
        GameObject armyObject = Instantiate(army_prefab, 
            new Vector3(army.Position.Item1, army.Position.Item2, 0), Quaternion.identity);
        army_view armyView = armyObject.GetComponent<army_view>();

        if (army.OwnerId != 0)
            armyView.Initialize(army, rtype);
        else armyView.Initialize(army, Relation.RelationType.Rebellion);

        armyViews.Add(armyView);
    }

    public void DestroyArmyView(Army army) {
        army_view armyView = armyViews.Find(view => view.ArmyData == army);
        if (armyView != null) {
            armyViews.Remove(armyView);
            Destroy(armyView.gameObject);
        }
    }

    public void RecruitArmy((int, int) coordinates, int amount) {
        var province = GetProvince(coordinates);
        var exitsing = armies.Find(a => a.Position == coordinates && a.Position == a.Destination);
        if (province.RecruitablePopulation >= amount) {
            province.Population -= amount;
            province.RecruitablePopulation -= amount;
            if (exitsing == null) {
                AddArmy(new(province.OwnerId, amount, coordinates, coordinates));
            }
            else {
                exitsing.Count += amount;
            }
        }
    }

    public void DisbandArmy(Army army, int amount)
    {
        var province = GetProvince(army.Position);

        if (army.Count == amount) {
            RemoveArmy(army);
        }
        else {
            army.Count -= amount;
        }
        province.Population += amount;
    }

    public void UndoDisbandArmy(Army army, int amount)
    {
        var province = GetProvince(army.Position);

        if (army.Count == amount) {
            AddArmy(army);
        }
        else {
            army.Count += amount;
        }

        province.Population -= amount;
    }

    public army_view GetView(Army army) {
        return armyViews.Find(view => view.ArmyData == army);
    }
    public void UpdateArmyPosition(Army army, (int, int) coordinates) {
        army_view armyView = armyViews.Find(view => view.ArmyData == army);

        if (armyView != null) {
            armyView.MoveTo(coordinates);
        }
    }

    public void UpdateArmyDestination(Army army, (int, int) coordinates) {
        army.Destination = coordinates;
    }

    public float CalcArmyCombatPower(Army army) {
        var stats = countries[army.OwnerId].TechStats;
        return army.Count + (army.Count * stats.ArmyPower);
    }

    public void MoveArmies() {
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
        UpdateArmyPosition(army, army.Destination);
        UpdateArmyDestination(army, army.Position);
    }

    public void UndoSetMoveArmy(Army army) {
        army_view view = GetView(army);
        if (view != null) {
            view.ReturnTo(army.Position);
        }
        MergeArmiesInProvince(GetProvince(army.Position), army);
    }

    public Army SetMoveArmy(Army army, int count, (int, int) destination) {
        if (count <= army.Count) {
            Army moved_army;

            if (count == army.Count) {
                UpdateArmyDestination(army, destination);
                moved_army = army;
            }
            else {
                moved_army = new Army(army) {
                    Destination = destination
                };
                army.Count -= count;
                moved_army.Count = count;

                AddArmy(moved_army);

            }
            var armyView = armyViews.Find(view => view.ArmyData == moved_army);
            if (armyView != null) {
                armyView.PrepareToMoveTo(destination);
            }
            return moved_army;
        }
        return army;
    }

    public void MergeArmies(Country country) {
        List<Army> ar = armies.Where(a => a.OwnerId == country.Id).ToList();
        var grouped = ar.GroupBy(a => a.Position)
            .Select(gr => new {
                pos = gr.Key,
                count = gr.Sum(a => a.Count),
                ars = gr.ToList()
            }).ToList();
        foreach (var group in grouped) {
            foreach (var army in group.ars) {
                RemoveArmy(army);
            }

            Army merged = new(country.Id, group.count, group.pos, group.pos);
            AddArmy(merged);
        }
    }

    private void MergeArmiesInProvince(Province province, Army to_merge) {
        Army base_ = armies.Find(a => a.OwnerId == to_merge.OwnerId && 
            a.Position == province.Coordinates && a.Destination == a.Position);

        if (base_ != null) {
            base_.Count += to_merge.Count;
            RemoveArmy(to_merge);
        }
        else {
            UpdateArmyDestination(to_merge, province.Coordinates);
        }
    }

    public List<(int, int)> GetPossibleMoveCells(Army army)
    {
        (int startX, int startY) = army.Position;
        Country country = Countries.FirstOrDefault(c => c.Id == army.OwnerId);
        return GetPossibleMoveCells(startX, startY, country.TechStats.MoveRange, country.TechStats.WaterMoveFactor);
    }

    public List<(int, int)> GetPossibleMoveCells(Province province, int range)
    {
        return GetPossibleMoveCells(province.X, province.Y, range, range);
    }

    public List<(int, int)> GetPossibleMoveCells(int startX, int startY, int moveRangeLand, double waterMoveFactor)
    {
        List<(int, int)> possibleCells = new List<(int, int)>();
        bool isStartTerrainLand = GetProvince(startX, startY).IsLand;
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
                bool isNeighborTerrainLand = GetProvince(neighborX, neighborY).IsLand;

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

    public List<Army> GetCountryArmies(Country country)
    {
        return armies.FindAll(a => a.OwnerId == country.Id);
    }

    public bool IsValidPosition(int x, int y)
    {
        Province province = GetProvince(x, y);

        return province != null;
    }

    public void ManageOccupationDuration(Province province)
    {
        if (province.OccupationInfo != null && province.OccupationInfo.IsOccupied)
        {
            province.OccupationInfo.OccupationCount--;

            if (province.OccupationInfo.OccupationCount <= 0) {
                OccupationChangeOwner(province);
            }
        }
    }

    private void OccupationChangeOwner(Province province) {
        if (!province.IsLand) return;
        int previousOwnerId = province.OwnerId;
        int newOwnerId = province.OccupationInfo.OccupyingCountryId;

        Countries.FirstOrDefault(c => c.Id == previousOwnerId).RemoveProvince(province);

        AssignProvince(province, newOwnerId);
        CancelOccupation(province);
    }

    public void ManageOccupation()
    {
        foreach (var army in armies)
        {
            ManageArmyOccupation(army);
        }
    }

    public void ManageArmyOccupation(Army army)
    {
        Province province = GetProvince(army.Position);
        if (!province.IsLand) return;

        if (IsProvinceFriendly(province, army))
        {
            if (IsEnemyOccupier(province, army))
            {
                CancelOccupation(province);
            }
        }
        else if (IsProvinceHostile(province, army))
        {
            if (!IsFriendOrArmyOwnerOccupier(province, army.OwnerId))
            {
                CancelOccupation(province);
                AddOccupation(province, army);
            }
        }
        else
        {
            return;
        }
    }

    // A province is friendly if:
    // - The army owner is the province owner.
    // - The army owner is in an alliance with the province owner.
    // - The army owner is in a vassalage relation with the province owner (regardless of the side)
    private bool IsProvinceFriendly(Province province, Army army)
    {
        Country provinceOwner = Countries.FirstOrDefault(c => c.Id == province.OwnerId);
        Country armyOwner = Countries.FirstOrDefault(c => c.Id == army.OwnerId);

        if (provinceOwner != null && armyOwner != null)
        {
            return province.OwnerId == army.OwnerId ||
                HasRelationOfType(armyOwner, provinceOwner, RelationType.Alliance) ||
                HasRelationOfType(armyOwner, provinceOwner, RelationType.Vassalage);
        }
        return false;
    }

    //  The province is considered occupied by the enemy if it is occupied by:
    // - A country with which the army owner is at war on the opposite side or
    // - Rebels
    private bool IsEnemyOccupier(Province province, Army army)
    {
        if (province.OccupationInfo != null && province.OccupationInfo.IsOccupied)
        {
            Country occupyingCountry = Countries.FirstOrDefault(c => c.Id == province.OccupationInfo.OccupyingCountryId);
            Country armyOwner = Countries.FirstOrDefault(c => c.Id == army.OwnerId);

            if (occupyingCountry != null && armyOwner != null)
            {
                return AreCountriesOpposingInTheSameWar(armyOwner, occupyingCountry) || occupyingCountry.Id == 0;
            }
        }
        return false;
    }

    // A province is hostile if:
    // - The army owner is at war with the province owner on the opposite side.
    // - The province is tribal.
    // - The army owner is tribal.
    private bool IsProvinceHostile(Province province, Army army)
    {
        Country provinceOwner = Countries.FirstOrDefault(c => c.Id == province.OwnerId);
        Country armyOwner = Countries.FirstOrDefault(c => c.Id == army.OwnerId);

        if (provinceOwner != null && armyOwner != null)
        {
            return AreCountriesOpposingInTheSameWar(armyOwner, provinceOwner) || province.OwnerId == 0 || army.OwnerId == 0;
        }
        return false;
    }

    // Determines if the province is already occupied by the army owner, an ally, or the army owner's senior
    private bool IsFriendOrArmyOwnerOccupier(Province province, int armyOwnerId)
    {
        if (province.OccupationInfo != null && province.OccupationInfo.IsOccupied)
        {
            Country occupyingCountry = Countries.FirstOrDefault(c => c.Id == province.OccupationInfo.OccupyingCountryId);
            Country armyOwner = Countries.FirstOrDefault(c => c.Id == armyOwnerId);

            if (occupyingCountry != null && armyOwner != null)
            {
                return armyOwnerId == province.OccupationInfo.OccupyingCountryId ||
                    HasRelationOfType(armyOwner, occupyingCountry, RelationType.Alliance) ||
                    HasOrderedRelationOfType(occupyingCountry, armyOwner, RelationType.Vassalage);
            }
        }
        return false;
    }

    public void AddOccupation(Province province, Army army)
    {
        Country country = Countries.FirstOrDefault(c => c.Id == army.OwnerId);
        Occupation occupationStatus = null;
        Country master = GetMaster(country);

        if (province.OwnerId == 0 || army.OwnerId == 0)
        {
            occupationStatus = new Occupation(1, army.OwnerId);
        }
        else
        {
            Country provinceOwner = Countries.FirstOrDefault(c => c.Id == province.OwnerId);
            bool atWar = false;

            if (master != null)
            {
                atWar = AreCountriesOpposingInTheSameWar(master, provinceOwner);
            }
            else
            {
                atWar = AreCountriesOpposingInTheSameWar(country, provinceOwner);
            }

            if (atWar)
            {
                if (master != null)
                {
                    occupationStatus = new Occupation(country.TechStats.OccTime, master.Id);
                }
                else
                {
                    occupationStatus = new Occupation(country.TechStats.OccTime, army.OwnerId);
                }
            }
        }

        if (occupationStatus != null && province.IsLand)
        {
            province.AddStatus(occupationStatus);
            province.OccupationInfo = new OccupationInfo(true, occupationStatus.Duration + 1, army.OwnerId);
        }
    }

    public void CancelOccupation(Province province)
    {
        if (!province.IsLand) return;
        province.OccupationInfo.IsOccupied = false;
        province.OccupationInfo.OccupationCount = 0;
        province.OccupationInfo.OccupyingCountryId = -1;
        province.Statuses.RemoveAll(status => status is Occupation);
    }

    public HashSet<Relation> GetRelationsOfType(Country country, Relation.RelationType type) {
        HashSet<Relation> result = new();
        foreach (var r in relations) {
            if (r.Type == type && r.Sides.Contains(country))
                result.Add(r);
        }
        return result;
    }

    public HashSet<Relation> GetWarRelations(Country country)
    {
        HashSet<Relation> result = new();
        foreach (Relation r in relations) {
            if (r is Relation.War war && (war.Participants1.Contains(country) 
                || war.Participants2.Contains(country))) {
                result.Add(r);
            }
        }
        return result;
    }

    public bool AreCountriesOpposingInTheSameWar(Country c1, Country c2)
    {
        return relations
            .OfType<Relation.War>()
            .Any(warRelation =>
                (warRelation.Participants1.Contains(c1) && warRelation.Participants2.Contains(c2)) ||
                (warRelation.Participants2.Contains(c1) && warRelation.Participants1.Contains(c2))
            );
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

    public Country GetSeniorIfExists(Country country) {
        Relation.Vassalage senior = relations.FirstOrDefault(r => r.Sides[1] == country) as Relation.Vassalage;
        if (senior != null) {
            return senior.Sides[0];
        }
        else return null;
    }

    public bool HasRelationOfType(Country c1, Country c2, Relation.RelationType type) {
        if (relations.Any(r => r.Sides.Contains(c1) && r.Sides.Contains(c2) && r.Type == type)) return true;
        return false;
    }

    public bool HasOrderedRelationOfType(Country c1, Country c2, RelationType type)
    {
        return relations.Any(rel => rel.Type == type &&
            rel.Sides[0] == c1 && rel.Sides[1] == c2);
    }

    public Country GetMaster(Country country) {
        foreach (var relation in Relations) {
            if (relation.Type == Relation.RelationType.Vassalage && relation.Sides[1] == country) {
                return relation.Sides[0];
            }
        }
        return null;
    }

    public Map GetSaveData() {
        return (Map)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(this, 
            Formatting.Indented, new JsonSerializerSettings {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        }));
    }

    public void ReloadArmyViews() {
        foreach (var av in armyViews) {
            Destroy(av.gameObject);
        }
        armyViews = null;
        foreach (var a in armies) {
            CreateArmyView(a);
        }
    }

    public void DestroyAllArmyViews() {
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
        public static Relation.War GetWar(Map map, Country c1, Country c2) {
            return map.GetRelationsOfType(c1, Relation.RelationType.War).First(r => r.Sides.Contains(c2)) as Relation.War;
        }

        public static Relation.War GetWar(Map map, int c1, int c2) {
            return GetWar(map, map.Countries[c1], map.Countries[c2]);
        }

        public static (int, int) GetSidePowers(Map map, Relation.War war) {
            int atkP = 1, defP = 1;
            if(war.Participants1!=null) foreach (var a in war.Participants1) {
                atkP += map.GetCountryArmies(a).Count;
            }
            if(war.Participants2!=null) foreach (var d in war.Participants2){
                defP += map.GetCountryArmies(d).Count;
            }
            return (atkP, defP);
        }

        public static bool IsAttackersStronger(Map map, Relation.War war) {
            var stats = GetSidePowers(map, war);
            return stats.Item1 > stats.Item2;
        }

        public static bool IsAttacker(Map map, Country c, Relation.War war) {
            return war.Participants1.Contains(c);
        }

        public static HashSet<Relation.War> GetAllWars(Map map, Country c) => 
            map.GetWarRelations(c).Cast<Relation.War>().ToHashSet();

        public static HashSet<Army> GetEnemyArmies(Map map, Country c) {
            HashSet<Army> enemy = new();

            foreach (var war in GetAllWars(map, c)) {
                var opposingParticipants = war.Participants1.Contains(c) ? war.Participants2 : war.Participants1;
                foreach (var country in opposingParticipants) {
                    enemy.UnionWith(map.GetCountryArmies(country));
                }
            }

            return enemy;
        }

        public static HashSet<int> GetEnemyIds(Map map, Country c) {
            HashSet<int> ids = new();
            foreach (var war in GetAllWars(map, c)) {
                var opposingParticipants = war.Participants1.Contains(c) ? war.Participants2 : war.Participants1;
                foreach (var country in opposingParticipants) {
                    ids.Add(country.Id);
                }
            }
            return ids;
        }

        public static HashSet<Army> GetEnemyArmiesInProvince(Map map, Country c, Province p) 
            => GetEnemyArmies(map, c).Where(a => a.Position == p.Coordinates).ToHashSet();

        public static HashSet<Province> GetEnemyProvinces(Map map, Country c) {
            HashSet<Province> prov = new();
            foreach(var id in GetEnemyIds(map, c)) {
                prov.UnionWith(map.Countries[id].Provinces);
            }
            return prov;
        }

        public static bool IsAtWarWIth(Map map, Country c1, Country c2)=> GetEnemyIds(map, c1).Contains(c2.Id);

        public static HashSet<Country> GetAllies(Map map, Country c)
            => map.GetRelationsOfType(c, Relation.RelationType.Alliance)
            .Cast<Relation.Alliance>().Select(a => a.Sides.First(s => s != c)).ToHashSet();
    }

    /// <summary>
    /// utilites for managing borders and armies
    /// </summary>
    internal class LandUtilites {
        public static HashSet<Country> BorderingCountries(Map map, Country country) {
            var bordering = new HashSet<Country>();
            //...
            return bordering;
        }

        public static HashSet<Province> GetViableArmyTargets(Map map, Country country) {
            var provinces = new HashSet<Province>();
            //...
            return provinces;
        }

        // likely need to replace numbers with something else
        public static List<Province> GetUnhappyProvinces(Country country) {
            return country.Provinces.Where(p=>p.Happiness<40 && !p.Statuses.Any(s=>s.Id == 1)).ToList();
        }

        public static List<Province> GetGrowable(Country c) {
            return c.Provinces.Where(p => (p.ResourceType == Resource.Gold || p.Population < 400) 
                && !p.Statuses.Any(s=>s.Id== 2)).ToList();
        }

        public static List<Province> GetOptimalRecruitmentProvinces(Country c) {
            return c.Provinces.Where(p=>p.RecruitablePopulation >= 50).OrderByDescending(p=>p.Population).ToList();
        }

        public static bool RecruitAllAvailable(Country c, Province p) {
            if (c.Resources[Resource.AP] >= 1) {
                c.Actions.AddAction(new TurnAction.ArmyRecruitment(p.Coordinates,
                    p.RecruitablePopulation*c.TechStats.ArmyCost <= c.Resources[Resource.Gold] ? p.RecruitablePopulation 
                    : (int)Math.Floor(c.Resources[Resource.Gold]/c.TechStats.ArmyCost), c.TechStats));
            }
            return p.RecruitablePopulation == 0;
        }

        public static HashSet<Province> GetUnpassableProvinces(Map map, Country c) {
            HashSet<Province> unpassable = new();

            if (!c.TechStats.CanBoat) {
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
        public static bool IsArmyStronger(Map map, Country c1, Country c2) {
            return map.Armies.FindAll(a => a.OwnerId == c1.Id).Sum(a=>a.Count) * 
                c1.TechStats.ArmyPower > map.Armies.FindAll(a => a.OwnerId == c2.Id).Sum(a => a.Count) * c2.TechStats.ArmyPower;
        }

        public static float HowArmyStronger(Map map, Country c1, Country c2) {
            return (map.Armies.FindAll(a => a.OwnerId == c1.Id).Sum(a => a.Count) * c1.TechStats.ArmyPower) 
                / (map.Armies.FindAll(a => a.OwnerId == c2.Id).Sum(a => a.Count) * c2.TechStats.ArmyPower);
        }

        public static int GetOpinion(Country of, Country from) {
            return from.Opinions[of.Id];
        }

        public static Dictionary<Resource, float> GetGain(Map map, Country country) {
            var gain = new Dictionary<Resource, float> {
                { Resource.Gold, 0 },
                { Resource.Wood, 0 },
                { Resource.Iron, 0 },
                { Resource.SciencePoint, 0 },
                { Resource.AP, 0 }
            };

            var tax = GetTaxGain(country);
            var prod = country.GetResourcesGain(map);

            foreach (var res in gain.Keys.ToList()) {
                if (res == Resource.Gold) gain[res] += tax;
                gain[res] += prod[res];
                gain[res] = (float)Math.Round(gain[res], 1);
            }

            return gain;
        }

        internal static float GetTaxGain(Country country) {
            var tax = 0f;
            foreach (var prov in country.Provinces) {
                tax += (prov.Population / 10) * country.Tax.ProjectedGold;
            }
            tax *= country.TechStats.TaxFactor;

            return (float)Math.Round(tax, 1);
        }

        public static float GetGoldGain(Map map, Country c) {
            return GetGain(map, c)[Resource.Gold]; 
        }

        public static float GetArmyUpkeep(Map map, Country c) {
            float res = 0;
            foreach (var army in map.GetCountryArmies(map.CurrentPlayer)) {
                 res -= (army.Count / 10 + 1) * map.Countries[c.Id].TechStats.ArmyUpkeep;
            }
            return res;
        }

        public static HashSet<Country> GetVassals(Map map, Country c) => 
            map.Relations.Where(r => r.Type == Relation.RelationType.Vassalage && r.Sides[0] == c)
            .Cast<Relation.Vassalage>().Select(v => v.Sides[0]).ToHashSet();

        public static Country GetSenior(Map map, Country c) => map.Relations
            .Where(r => r.Type == Relation.RelationType.Vassalage && r.Sides[1] == c)
            .Cast<Relation.Vassalage>().Select(v => v.Sides[0]).First();

        public static HashSet<Relation.Vassalage> GetVassalRelations(Map map, Country c) => 
            map.Relations.Where(r => r.Type == Relation.RelationType.Vassalage && r.Sides[0] == c)
                .Cast<Relation.Vassalage>().ToHashSet();

        public static Relation.Vassalage GetVassalage(Map map, Country c) => map.Relations
            .Where(r => r.Type == Relation.RelationType.Vassalage && r.Sides.Any(s => s.Equals(c)))
            .Cast<Relation.Vassalage>().First();

        public static HashSet<Country> GetWeakCountries(Map map, Country c) {
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
                if(HowArmyStronger(map, c, cc) > 2) {
                    weaklings.Add(cc);
                    continue;
                }
            }

            return weaklings;
        }    
    }
}
