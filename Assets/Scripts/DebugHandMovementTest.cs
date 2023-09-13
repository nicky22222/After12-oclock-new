using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugHandMovementTest : MonoBehaviour
{
    private Animator animator;
    private static readonly int PlayCastingAnim = Animator.StringToHash("PlayCastingAnim");

    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            var isSpellCasting = animator.GetBool(PlayCastingAnim);
            Debug.Log($"Toggle Spell Cast Animation {!isSpellCasting}");
            animator.SetBool(PlayCastingAnim, !isSpellCasting);
        }
    }
}
