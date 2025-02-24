using System.Collections.Generic;
using UnityEngine;

public class LecternManager : MonoBehaviour
{
    // Singleton instance for global access.
    public static LecternManager Instance;

    [Header("Lore Configuration")]
    [Tooltip("Number of floors that will receive lore entries (e.g., 16).")]
    public int floorsWithLore = 16;

    // Hardcoded lore entries; the key here is used as the title (including a trailing colon)
    // and the value is the body text. 
    private Dictionary<string, string> _allLoreEntries = new Dictionary<string, string>
    {
        {"Lore Entry 1:", "In the beginning, the tower was built by ancient hands."},
        {"Lore Entry 2:", "Legends tell of a forgotten king who ruled these lands."},
        {"Lore Entry 3:", "The shadows whisper secrets of lost civilizations."},
        {"Lore Entry 4:", "Long ago, magic flowed freely in these halls."},
        {"Lore Entry 5:", "Many have sought the treasure hidden within these walls."},
        {"Lore Entry 6:", "The tower has seen countless battles and sacrifices."},
        {"Lore Entry 7:", "Myths say a great power lies dormant in its depths."},
        {"Lore Entry 8:", "The stone corridors echo with the cries of the past."},
        {"Lore Entry 9:", "Only the brave dare to enter where darkness reigns."},
        {"Lore Entry 10:", "Forgotten gods once blessed this place with miracles."},
        {"Lore Entry 11:", "A curse was cast upon these walls, and none have broken it."},
        {"Lore Entry 12:", "The ancient runes foretell a time of reckoning."},
        {"Lore Entry 13:", "Beneath the tower, the earth trembles with secrets."},
        {"Lore Entry 14:", "The spirits of old guard the relics of lost eras."},
        {"Lore Entry 15:", "Through the ages, the tower has been both sanctuary and prison."},
        {"Lore Entry 16:", "Every stone here tells a story of glory and despair."},
        {"Lore Entry 17:", "A forgotten era, now sealed behind these massive gates."},
        {"Lore Entry 18:", "The echo of footsteps reminds you that history is alive."},
        {"Lore Entry 19:", "In hidden chambers, the whispers of the past grow louder."},
        {"Lore Entry 20:", "Ancient battles and heroes are etched into every wall."},
        {"Lore Entry 21:", "A labyrinth of secrets awaits those who dare to explore."},
        {"Lore Entry 22:", "The tower is said to change its shape with the passing of time."},
        {"Lore Entry 23:", "Legends speak of a power that can bend reality."},
        {"Lore Entry 24:", "Every floor holds a piece of the story, lost to time."},
        {"Lore Entry 25:", "The tapestry of history is woven into these corridors."},
        {"Lore Entry 26:", "Dark forces once ruled here, and their legacy remains."},
        {"Lore Entry 27:", "The art of war and peace is balanced in this tower."},
        {"Lore Entry 28:", "Some say the tower is alive, breathing with ancient energy."},
        {"Lore Entry 29:", "Remnants of forgotten rituals linger in the air."},
        {"Lore Entry 30:", "The secrets of the past are written in the runes on these walls."},
        {"Lore Entry 31:", "Only those with a pure heart may unlock the tower’s mysteries."},
        {"Lore Entry 32:", "The final chapter of this ancient saga awaits beyond these doors."}
    };

    // Dictionary mapping floor number (1 to floorsWithLore) to a lore entry.
    private Dictionary<int, string> floorLore = new Dictionary<int, string>();

    void Awake()
    {
        // Singleton setup.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DistributeLore();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Shuffles the available lore entries and assigns one unique entry per floor.
    /// Uses RandomSeed.RNG for reproducibility.
    /// </summary>
    public void DistributeLore()
    {
        floorLore.Clear();

        if (_allLoreEntries == null || _allLoreEntries.Keys.Count < floorsWithLore)
        {
            Debug.LogError("Not enough lore entries provided! You must supply at least " + floorsWithLore + " lore entries.");
            return;
        }

        // Create a list of indices for all entries.
        List<int> indices = new List<int>();
        for (int i = 0; i < _allLoreEntries.Keys.Count; i++)
        {
            indices.Add(i);
        }

        // Shuffle using RandomSeed.RNG.
        int n = indices.Count;
        while (n > 1)
        {
            n--;
            int k = RandomSeed.RNG.Next(n + 1);
            int temp = indices[k];
            indices[k] = indices[n];
            indices[n] = temp;
        }

        // Convert the values (lore texts) to a list.
        List<string> loreValues = new List<string>(_allLoreEntries.Values);

        // Assign the first floorsWithLore entries.
        for (int floor = 1; floor <= floorsWithLore; floor++)
        {
            int index = indices[floor - 1];
            floorLore[floor] = loreValues[index];
        }
    }

    /// <summary>
    /// Returns the lore entry for a given floor number.
    /// </summary>
    public string GetLoreForFloor(int floor)
    {
        if (floorLore.ContainsKey(floor))
            return floorLore[floor];
        return "";
    }
}
