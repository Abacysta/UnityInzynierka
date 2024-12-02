using System;

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
}
