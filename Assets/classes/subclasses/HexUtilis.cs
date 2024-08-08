using System;
using System.Collections.Generic;
using UnityEngine;

public class HexUtils
{
    public struct Cube
    {
        public int q, r, s;
        public Cube(int q, int r, int s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
        }
    }

    private static Cube[] cube_direction_vectors = new Cube[]
    {
        new Cube(+1, 0, -1), new Cube(+1, -1, 0), new Cube(0, -1, +1),
        new Cube(-1, 0, +1), new Cube(-1, +1, 0), new Cube(0, +1, -1)
    };

    public static Cube CubeDirection(int direction)
    {
        return cube_direction_vectors[direction];
    }

    public static Cube CubeAdd(Cube a, Cube b)
    {
        return new Cube(a.q + b.q, a.r + b.r, a.s + b.s);
    }

    public static Cube CubeNeighbor(Cube cube, int direction)
    {
        return CubeAdd(cube, CubeDirection(direction));
    }

    public static Cube OffsetToCube(int x, int y)
    {
        int q = x - (y - (y & 1)) / 2;
        int r = y;
        int s = -q - r;
        return new Cube(q, r, s);
    }

    public static (int, int) CubeToOffset(Cube cube)
    {
        int x = cube.q + (cube.r - (cube.r & 1)) / 2;
        int y = cube.r;
        return (x, y);
    }

    public static List<Cube> CubeRange(Cube start, int range)
    {
        List<Cube> results = new List<Cube>();
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = Mathf.Max(-range, -range - dx); dy <= Mathf.Min(range, range - dx); dy++)
            {
                int dz = -dx - dy;
                results.Add(new Cube(start.q + dx, start.r + dy, start.s + dz));
            }
        }
        return results;
    }
}