using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get horizontal input (A/D or left/right arrow keys).
        float move = Input.GetAxisRaw("Horizontal");

        // Set the Speed parameter to the absolute value of the movement.
        animator.SetFloat("Speed", Mathf.Abs(move));

        // Set the FacingRight parameter.
        if (move < 0)
            animator.SetBool("FacingRight", false);
        else if (move > 0)
            animator.SetBool("FacingRight", true);
    }
}