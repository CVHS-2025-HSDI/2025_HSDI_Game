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
        {"The Powers of the Destiny Totem:", "It is known that the Totem grants the user any wish they desire, but rumor says there is a catch."},
        {"Lord Carp\'s rise to power:", "Originally, his brother was the king, but one fateful day, he passed away under mysterious circumstances. Though the cause of his death was never revealed, many suspected the involvement of Lord Carp. With his brother\'s untimely demise, Lord Carp, second in line, ascended to the throne. From that moment on, a series of unfortunate events began to unfold."},
        {"Theory of the Evolution of the Uonan People:", "It is believed that the Shimanese people evolved from fish, much like the theory of human evolution. However, unlike their terrestrial counterparts, the Uonans remained in the water, retaining their fish-like traits while also advancing in other ways, developing languages, limbs, and more, much like the Shimanese."},
        {"The Uonan Crown:", "Passed down through generations of kings, the Uonan Crown bestows upon its wearer superhuman strength and endurance. Lord Carp currently holds it, but his greed knows no bounds. He craves more than just power—he seeks immortality."},
        {"The Ancient Oracle:", "The Ancient Oracle was the original ruler of the Shimanese. However, he mysteriously disappeared around the time of the first attack, leaving no trace behind. No one knows where he went, why he left, or what happened to him. His disappearance remains an unsolved mystery, and the Uonans continue to search for him to this day."},
        {"The Coexistence of the Shimanese and the Uonans:", "The Shimanese and Uonans once lived in harmony, with the Uonans providing seafood in exchange for protection from external threats. They shared the powerful Destiny Totem, symbolizing their peace, and coexisted for generations without conflict."},
        {"The Hate Toward the Uonans:", "The Uonans, long hated and outcast by other races for their fearsome reputation as vicious creatures, found refuge and protection from the Shimanese. While the outside world saw them as monsters, the Shimanese trusted them as allies, sharing the powerful Destiny Totem and ensuring their safety."},
        {"The First Uonan Attack:", "The first Uonan attack occurred when the Uonans failed to deliver the promised amount of food to the Shimanese, causing a major rift between the two peoples. Tensions escalated into a violent confrontation, during which the Ancient Oracle mysteriously disappeared. Amid the turmoil, the Uonans seized one half of the Destiny Totem, marking the beginning of a series of conflicts that would follow."},
        {"The Uonan\'s Weakness:", "The Uonans, formidable sea creatures, can only survive on land when their hearts continuously circulate water through their bodies, a vital process sustaining their life. However, this dependence on water is also their greatest vulnerability. The only way to truly kill an Uonan is to strike at their heart, the source of their life force—an act that can shatter their existence both on land and sea."},
        {"Shimanese Life after the Attack:", "After the first Uonan attack, Lord Orion had everyone take precautions and mostly stay inside their homes or small communities. Everyone was afraid to meet with others, and the town became extremely isolated. They were provided with rations of food and water to ensure that they were healthy, but other than that, they were stuck at home."},
        {"The Arid Sword:", "A powerful sword that the Uonans have since it is very powerful against them. Whenever it pierces an Uonan, it drains the water out of their body, making them extremely vulnerable."},
        {"The Tower of Aegis:", "Dates back 2,000 years, when the Uonans and Shimanese first met. Both groups worked together to build the tower, for it to be a symbol of peace between the two."},
        {"Kyogon:", "No one knows where it came from and what creature it became. Kyogon has the ability to expand oceans. In ancient times, Kyogon came into conflict with Groudre, a creature with the ability to expand continents."},
        {"Groudre:", "The creature that came into conflict with Kyogon in ancient times. Groudre has the power to expand continents and use the power of the volcano. It was also captured by the Uonans, using it\'s skin to make the Arid Sword."},
        {"The Flood Sword:", "The opposite of the Arid Sword and also possessed by the Uonans. When it penetrates the victim, it floods the victim\'s lungs with water, essentially drowning them."},
        {"The Food of Uonan\'s:", "The Uonans surprisingly eat other fish. But the fish they eat are smaller and weaker."},
        {"Origin of the Destiny Totem:", "The true origin of the Destiny Totem remains shrouded in mystery, lost to the echoes of time. Legends whisper that it was not crafted by mortal hands but instead emerged from a swirling, otherworldly portal that split the sky in an ancient era. Some say it was a gift from celestial beings, meant to guide and protect the land, while others fear it was left behind by an unknown force, its true purpose yet to be revealed. What is certain, however, is that its arrival forever changed the course of destiny for all who came into contact with it."},
        {"Anatomy of Uonans:", "Most of their anatomy is shared with fish. It shares limbs and mobility like Humans, but also has fins so they can swim."},
        {"Other Realms:", "Our protagonist is not the first thing to arrive to this world from another one. There have been some items that have been discovered that were not from this world. It is unknown how other worlds link to this one but the items are being studied by both people."},
        {"The Theory of the Connection to Other Realms:", "Although there is no evidence, some have their theories of how these worlds are connected. One big one is that it is thought that there is another world that has the technology to enter this world because most of the items discovered share very similar structures."},
        {"The Blue Juice:", "One item obtained by the Uonans from another universe is a mysterious light turquoise blue juice. It came in a large barrel with the label “Slurp Co.” on it. Unknown of its origin, the juice grants the consumer healing and also a boost in vulnerability. Because of the single barrel it came in there is a low supply. However, the Uonans are trying to replicate it, but have not been successful."},
        {"Lord Carp\'s Journal 1:", "I have successfully killed my brother. I was able to put a compound in his wine that dries up the water in his body. I will be assuming the throne in 3 days. After that I will start my next goal of attacking the Shimanese for the other half of the Destiny Totem.."},
        {"Lord Carp\'s Journal 2:", "Once I have the Destiny Totem, I will be unstoppable. I shall wish for the death of the Lord Orion, so I can take over the Shimanese and expand the Uonan empire. After that I will enslave the Shimanese people and start my conquest."},
        {"Cadet Trout\'s Journal", "Lord Carp is almost to his goal and he promised that I will get one wish with the Destiny Totem. I am thinking of either wishing for immortality or a loving spouse. If I get immortality, I won\'t be afraid of dying anymore, but if I get a spouse, I won\'t have to try to get a female to like me back."},
        {"Lady Koi\'s Journal:", "Father is close to using the Destiny Totem. Although he has my brother and I a wish with the totem, I do not think his intentions with it are morally right. Despite disagreeing with my father\'s plan, I must protect the tower and keep any intruders away from the totem."},
        {"Prince Nishikigo\'s Journal:", "After my mother's death, I have never felt the same. When father said he was going to obtain the Destiny Totem, I asked if we could use it to bring back mother, but he said that wanting mother back shows my weakness. He then said that in order to use the totem I must be strong. He proceeded to give my sister and Cadet Trout a wish but not me. I plan to overthrow him and use the totem to bring back my mother."},
        {"A Soldier\'s Letter:", "Lord Carp, There is an intruder in the tower. I am writing to warn you and request more troops down here. Please send them as soon as possible, as our men are quickly being lost."},
        {"Shimanese Prisoner Journal:", "These prisoner guards are cruel to us. They call us weak and unworthy of the half of the Destiny Totem. I hope that someone will come and save us before our execution. They say they will execute us by drowning us."},
        {"Creation of the World:", "It is thought that the universe was created by the “Big boot-up” where the world was a copy of another world that was destroyed. The current world they live in is an extension of the previous one and there is a possibility that the world could be destroyed like the old one."},
        {"Uonan Science:", "Chemistry: The Uonan are masters of Chemistry. They are able to replicate chemical compounds they find and even make them better. They have used this to advance their people and make them more powerful."},
        {"Lore Entry 31:", "Only those with a pure heart may unlock the towers mysteries."},
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
