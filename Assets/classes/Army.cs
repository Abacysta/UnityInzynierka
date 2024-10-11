using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Army
{
    private int ownerId;
    private int count;
    private (int,int) position;
    private (int,int) destination;
    public event Action<int> OnArmyCountChanged;

    public Army(int ownerId, int count, (int, int) position, (int, int) destination)
    {
        this.ownerId = ownerId;
        this.count = count;
        this.position = position;
        this.destination = destination;
    }

    public Army(Army army) {
        this.ownerId = army.ownerId;
        this.count = army.count;
        this.position = army.position;
        this.destination = army.destination;
    }

    public int OwnerId { get => ownerId; set => ownerId = value; }
    public int Count
    {
        get { return count; }
        set
        {
            if (count != value)
            {
                count = value;
                OnArmyCountChanged?.Invoke(count);
            }
        }
    }
    public (int,int) Position { get => position; set => position = value;  }
    public (int,int) Destination { get => destination; set => destination = value; }

    public static Army makeSubarmy(Army army, int count)
    {
        army.count -= count;
        Army army2 = new Army(army.OwnerId, count, army.position, army.destination);
        return army2;
    }

    public static Army mergeArmiesInProvince(List<Army> armies) {
        Army army = new Army(armies[0].OwnerId, 0, armies[0].Position, armies[0].Position);
        army.count = armies.Sum(a=> a.Count);
        return army;
    }

}
