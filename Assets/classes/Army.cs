using UnityEngine;

public class Army
{
    public int ownerId;
    public int count;
    public (int,int) position;
    public (int,int) destination;
    public int moveRangeLand;
    public int moveRangeWater;
    public Army(int ownerId, int count, (int, int) position, (int, int) destination, int moveRangeLand, int moveRangeWater)
    {
        this.ownerId = ownerId;
        this.count = count;
        this.position = position;
        this.destination = destination;
        this.moveRangeLand = moveRangeLand;
        this.moveRangeWater = moveRangeWater;
    }
    public int OwnerId { get => ownerId; set => ownerId = value; }
    public int Count { get => count; set => count = value; }
    public (int,int) Position { get => position; set => position = value;  }
    public (int,int) Destination { get => destination; set => destination = value; }
    public int MoveRangeLand { get => moveRangeLand; set => moveRangeLand = value; }
    public int MoveRangeWater { get => moveRangeWater; set => moveRangeWater = value; }

}
