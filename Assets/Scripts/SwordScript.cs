using UnityEngine;

public class SwordScript : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Enemy")) return;

        var x = other.gameObject.GetComponentInParent<Animator>();
        x.enabled = false;
    }
}
