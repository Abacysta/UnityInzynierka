using System;
using System.Collections.Generic;
using System.Linq;
using Assets.classes.subclasses;
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

    public static Cube OffsetToCube((int, int) xy) {
        return OffsetToCube(xy.Item1, xy.Item2);
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

    // float interpolation
    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
    // Interpolation between two hexes
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


    // Rounding Cube to the nearest hex, taken from https://www.redblobgames.com/grids/hexagons/#rounding 06.11.2024
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
    // // Generates a list of hexes on the line between two hexes - https://www.redblobgames.com/grids/hexagons/#line-drawing 06.11.2024
    public static List<(int, int)> CubeLineDraw(Cube start, Cube end)
    {
        int N = CubeDistance(start, end);

        // 2D list
        List<(int, int)> results = new List<(int, int)>();

        for (int i = 0; i <= N; i++)
        {
            Cube interpolated = CubeLerp(start, end, 1.0f / N * i);

            // from 3d to 2d
            var (x, y) = CubeToOffset(interpolated);

            results.Add((x, y));
        }

        return results;
    }

    public static List<(int, int)> CubeLineDraw((int, int) start, (int, int) end) {
        return CubeLineDraw(OffsetToCube(start), OffsetToCube(end));
    }
    public static List<(int, int)> CubeLineDraw(Province start, Province end) {
        return CubeLineDraw(start.coordinates, end.coordinates);
    }

    public static void hexPathingTest() {
        var start = new HexUtils.Cube(0, 0, 0); // Starting point
        var end = new HexUtils.Cube(4, -4, 0); // End point

        // Using CubeLineDraw to generate a path between start and end
        List<(int, int)> path = HexUtils.CubeLineDraw(start, end);

        // Displaying results in the console
        Debug.Log("Linia miêdzy punktami startowym i koñcowym:");
        foreach (var hex in path) {
            Debug.Log($"Hex: x:{hex.Item1}, y:{hex.Item2}");
        }
        // Additional check to ensure the path has the correct length
        int expectedDistance = HexUtils.CubeDistance(start, end);
        bool correctLength = (path.Count - 1 == expectedDistance);

        Debug.Log(correctLength
            ? "Test przeszed³ pomyœlnie: d³ugoœæ œcie¿ki jest poprawna."
            : $"B³¹d: oczekiwano d³ugoœci {expectedDistance + 1}, otrzymano {path.Count}");
    }

    public static int getProvinceDistance(Province p1, Province p2) {
        Cube c1 = OffsetToCube(p1.X, p2.Y), c2=OffsetToCube(p2.X, p2.Y);
        return CubeDistance(c1, c2);
    }
    //A* algo - like Djikstra but better
    //also using imported from normal .net PriorityQueue cuz pretty good for this
    public static List<Province> getBestPathProvinces(Map map, Country country, Province start, Province end) {
        return getBestPathProvinces(map, country, (start.X, start.Y), (end.X, end.Y));
    }
    public static bool isPathPossible(Map map, Country country, Province start, Province end) {
        return getBestPathProvinces(map, country, start, end) != null;
    }
    public static List<Province> getBestPathProvinces(Map map, Country country, (int, int) start, (int, int) end) {
        var provinces = map.Provinces.Select(p=>(p.X, p.Y)).ToHashSet();
        var unavailable = Map.LandUtilites.getUnpassableProvinces(map, country).Select(p=>(p.X, p.Y)).ToHashSet();
        var best = getBestPath(provinces, unavailable, new(), start, end);
        //for now no misc costs but in future maybe
        if (best != null) return best.Select(cord => map.getProvince(cord)).Where(province => province != null).ToList();
        else return null;
    }
    public static List<Province> getBestPathProvinces(Map map, Country country, HashSet<(int, int)> unpassable, (int, int) start, (int, int) end) {
        var provinces = map.Provinces.Select(p => (p.X, p.Y)).ToHashSet();
        var best = getBestPath(provinces, unpassable, new(), start, end);
        if (best != null) return getBestPath(provinces, unpassable, new(), start, end).Select(cord => map.getProvince(cord)).Where(province => province != null).ToList();
        else return null;
    }
    public static List<Province> getBestPathProvinces(Map map, Country country, HashSet<Province> unpassable, Province start, Province end) {
        return getBestPathProvinces(map, country, unpassable.Select(p=>p.coordinates).ToHashSet(), (start.X, start.Y), (end.X, end.Y));
    }
    private static List<(int, int)> getBestPath(
        HashSet<(int, int)> provinces,
        HashSet<(int, int)> unavailable,
        Dictionary<(int, int), int> miscCosts,
        (int, int) start, (int, int) end) {
        if (miscCosts == null) miscCosts = new();
        var startCube = OffsetToCube(start.Item1, start.Item2);
        var endCube = OffsetToCube (end.Item1, end.Item2);
        var prioQ = new PriorityQueue<Cube, int>();
        var costSoFar= new Dictionary<Cube, int>();
        var cameFrom=new Dictionary<Cube, Cube>();
        prioQ.Enqueue(startCube, 0);
        costSoFar[startCube] = 0;
        while (prioQ.Count > 0) { 
            var current = prioQ.Dequeue();
            if (current.Equals(endCube)) {
                return ReconstructPath(cameFrom, startCube, endCube).Select(CubeToOffset).ToList();
            }
            for (int dir = 0; dir < 6; dir++){
                var neighbour = CubeNeighbor(current, dir);
                var (nx, ny) = CubeToOffset(neighbour);
                if (unavailable.Contains((nx, ny)) || !provinces.Contains((nx, ny)))
                    continue;
                int newCost = costSoFar[current] + 1 + (miscCosts.TryGetValue((nx, ny), out var additional) ? additional : 0);

                if (!costSoFar.ContainsKey(neighbour) || newCost < costSoFar[neighbour]) {
                    costSoFar[neighbour] = newCost;
                    cameFrom[neighbour] = current;
                    int prio = newCost + CubeDistance(neighbour, endCube);
                    prioQ.Enqueue(neighbour, prio);
                }
            }
        }
        return null;
    }
    private static List<Cube> ReconstructPath(Dictionary<Cube, Cube> cameFrom, Cube start, Cube end) {
        var path = new List<Cube>();
        var current = end;

        while (!current.Equals(start)) {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }
    public static Province getNearestProvince(Map map, (int,int) start, HashSet<int> ids) {
        var startCube = OffsetToCube(start.Item1, start.Item2);
        Queue<Cube> front = new Queue<Cube>();
        front.Enqueue(startCube);
        HashSet<Cube> visited = new HashSet<Cube>();
        visited.Add(startCube);
        while (front.Count > 0) {
            var current = front.Dequeue();
            var (x, y) = CubeToOffset(current);
            Province province = map.getProvince(x, y);
            if (province != null && ids.Contains(province.OwnerId))
                return province;

            for (int dir = 0; dir > 6; dir++) {
                var neighbour = CubeNeighbor(current, dir);
                if (!visited.Contains(neighbour)) {
                    var (nx, ny) = CubeToOffset(neighbour);
                    if (map.IsValidPosition(nx, ny)) {
                        visited.Add(neighbour);
                        front.Enqueue(neighbour);
                    }
                }
            }
        }
        return null;
    }
}