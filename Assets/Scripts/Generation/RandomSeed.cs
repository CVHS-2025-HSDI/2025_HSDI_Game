using UnityEngine;

public static class RandomSeed
{
    public static System.Random RNG = new System.Random();

    public static void SetSeed(int seed)
    {
        RNG = new System.Random(seed);
    }

    public static int GetRandomInt(int min, int max)
    {
        return RNG.Next(min, max);
    }
}
