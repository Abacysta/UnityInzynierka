using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Army
{
    public int ownerId;
    public int count;
    public (int,int) position;
    public (int,int) destination;

    public Army(int ownerId, int count, (int, int) position, (int, int) destination)
    {
        this.ownerId = ownerId;
        this.count = count;
        this.position = position;
        this.destination = destination;
    }
    public int OwnerId { get => ownerId; set => ownerId = value; }
    public int Count { get => count; set => count = value; }
    public (int,int) Position { get => position; set => position = value;  }
    public (int,int) Destination { get => destination; set => destination = value; }

    public static Army makeSubarmy(Army army, int count) {
        army.count -= count;
        var army2 = army;
        army2.count = count;
        return army2;
    }

    public static Army mergeArmiesInProvince(List<Army> armies) {
        Army army = new Army(armies[0].OwnerId, 0, armies[0].Position, armies[0].Position);
        army.count = armies.Sum(a=> a.Count);
        return army;
    }

}
