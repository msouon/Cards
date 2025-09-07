using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    void Start()
    {
        var animator = GetComponent<Animator>();
        float length = 0f;
        if (animator != null &&
            animator.runtimeAnimatorController != null &&
            animator.runtimeAnimatorController.animationClips.Length > 0)
        {
            length = animator.runtimeAnimatorController.animationClips[0].length;
        }
        Destroy(gameObject, length);
    }
}
