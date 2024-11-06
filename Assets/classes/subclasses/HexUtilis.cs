using System;
using System.Collections.Generic;
using UnityEngine;

public class HexUtils
{
    public struct Cube
    {
        public int q, r, s; // q + r + s = 0 
        public Cube(int q, int r, int s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
        }
        public Cube(float q, float r, float s)
        {
            this.q = (int)Mathf.Round(q);
            this.r = (int)Mathf.Round(r);
            this.s = (int)Mathf.Round(s);
        }
    }

    private static Cube[] cube_direction_vectors = new Cube[]
    {
        new Cube(+1, 0, -1), new Cube(+1, -1, 0), new Cube(0, -1, +1),
        new Cube(-1, 0, +1), new Cube(-1, +1, 0), new Cube(0, +1, -1)  // neighbors of point (0 , 0 , 0)
    };

    public static Cube CubeDirection(int direction)
    {
        return cube_direction_vectors[direction];   
    }

    public static Cube CubeAdd(Cube a, Cube b)
    {
        return new Cube(a.q + b.q, a.r + b.r, a.s + b.s); // we use it to move on grid it addes dircection vector to current hex, and we get for example top left hex.
    }

    public static Cube CubeNeighbor(Cube cube, int direction)
    {
        return CubeAdd(cube, CubeDirection(direction));
    }

    public static Cube OffsetToCube(int x, int y) // 2d to 3d
    {
        int q = x - (y - (y & 1)) / 2;            // https://www.redblobgames.com/grids/hexagons/#conversions-offset
        int r = y;
        int s = -q - r;                           
        return new Cube(q, r, s);
    }
    public static (int, int) CubeToOffset(Cube cube) // 3d to 2d
    {
        int x = cube.q + (cube.r - (cube.r & 1)) / 2;
        int y = cube.r;
        return (x, y);
    }
    public static List<Cube> CubeRange(Cube start, int range) // we goes thru all combinations in "range" of possible hexes
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

    // interpolacja float
    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
    // interpolacja miêdzy dwoma heksami
    private static Cube CubeLerp(Cube a, Cube b, float t)
    {
        return new Cube(
            Lerp(a.q, b.q, t),
            Lerp(a.r, b.r, t),
            Lerp(a.s, b.s, t)
        );
    }

    // https://www.redblobgames.com/grids/hexagons/#distances 06.11.2024
    public static Cube CubeSubtract(Cube a, Cube b)
    {
        return new Cube(a.q - b.q, a.r - b.r, a.s - b.s);
    }

    // dystans miêdzy dwoma heksami - https://www.redblobgames.com/grids/hexagons/#distances 06.11.2024
    public static int CubeDistance(Cube a, Cube b)
    {
        Cube vec = CubeSubtract(a, b);
        return Mathf.Max(Mathf.Abs(vec.q), Mathf.Abs(vec.r), Mathf.Abs(vec.s));
    }


    // zaokr¹glanie Cube do najbli¿szego heksa wziete z https://www.redblobgames.com/grids/hexagons/#rounding 06.11.2024
    private static Cube CubeRound(Cube cube)
    {
        int rq = Mathf.RoundToInt(cube.q);
        int rr = Mathf.RoundToInt(cube.r);
        int rs = Mathf.RoundToInt(cube.s);

        float qDiff = Mathf.Abs(rq - cube.q);
        float rDiff = Mathf.Abs(rr - cube.r);
        float sDiff = Mathf.Abs(rs - cube.s);

        if (qDiff > rDiff && qDiff > sDiff)
            rq = -rr - rs;
        else if (rDiff > sDiff)
            rr = -rq - rs;
        else
            rs = -rq - rr;

        return new Cube(rq, rr, rs);
    }
    // generuje listê heksów na linii miêdzy dwoma heksami - https://www.redblobgames.com/grids/hexagons/#line-drawing 06.11.2024
    public static List<(int, int)> CubeLineDraw(Cube start, Cube end)
    {
        int N = CubeDistance(start, end);

        // lista 2D
        List<(int, int)> results = new List<(int, int)>();

        for (int i = 0; i <= N; i++)
        {
            Cube interpolated = CubeLerp(start, end, 1.0f / N * i);

            // z 3d na 2d
            var (x, y) = CubeToOffset(interpolated);

            results.Add((x, y));
        }

        return results;
    }

    public static void hexPathingTest()
    {
        var start = new HexUtils.Cube(0, 0, 0); // Punkt startowy
        var end = new HexUtils.Cube(4, -4, 0);  // Punkt koñcowy 

        // U¿ycie CubeLineDraw, aby wygenerowaæ œcie¿kê miêdzy start a end
        List<(int,int)> path = HexUtils.CubeLineDraw(start, end);

        // Wyœwietlenie wyników na konsoli
        Debug.Log("Linia miêdzy punktami startowym i koñcowym:");
        foreach (var hex in path)
        {
            Debug.Log($"Hex: x:{hex.Item1}, y:{hex.Item2}");
        }
        // Dodatkowe sprawdzenie, czy œcie¿ka ma poprawn¹ d³ugoœæ
        int expectedDistance = HexUtils.CubeDistance(start, end);
        bool correctLength = (path.Count - 1 == expectedDistance);

        Debug.Log(correctLength
            ? "Test przeszed³ pomyœlnie: d³ugoœæ œcie¿ki jest poprawna."
            : $"B³¹d: oczekiwano d³ugoœci {expectedDistance + 1}, otrzymano {path.Count}");
    }
}