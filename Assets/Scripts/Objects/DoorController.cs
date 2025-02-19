using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Sprite closedSprite;
    public Sprite openSprite;
    public float openDistance = 3f;  // Distance at which the door opens

    private SpriteRenderer _sr;
    private Collider2D _col;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();

        // Ensure collider is active initially
        _col.enabled = true;
        _sr.sprite = closedSprite;
    }

    void Update()
    {
        bool shouldOpen = false;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && Vector3.Distance(transform.position, player.transform.position) <= openDistance)
            shouldOpen = true;

        GameObject enemy = GameObject.FindWithTag("Enemy");
        if (enemy != null && Vector3.Distance(transform.position, enemy.transform.position) <= openDistance)
            shouldOpen = true;

        if (shouldOpen)
        {
            _sr.sprite = openSprite;
            if (_col.enabled)
                _col.enabled = false;
        }
        else
        {
            _sr.sprite = closedSprite;
            if (!_col.enabled)
                _col.enabled = true;
        }
    }
}