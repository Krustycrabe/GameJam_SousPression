using UnityEngine;

/// <summary>
/// Permet à un NPC de ramasser et lâcher la malette.
/// Fonctionne avec le même système que PlayerBriefCaseController.
/// </summary>
public class NPCBriefcaseController : MonoBehaviour
{
    [SerializeField] private Transform _rightHandBone;
    [SerializeField] private float _pickupRange = 1.5f;

    private BriefcaseItem _briefcase;

    public bool HasBriefcase => _briefcase != null;

    /// <summary>Cherche et ramasse la malette au sol la plus proche.</summary>
    public bool TryPickup()
    {
        if (HasBriefcase) return true;

        Collider[] nearby = Physics.OverlapSphere(transform.position, _pickupRange);
        BriefcaseItem closest = null;
        float closestDist = float.MaxValue;

        foreach (Collider col in nearby)
        {
            BriefcaseItem item = col.GetComponentInParent<BriefcaseItem>();
            if (item == null || item.IsHeld) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < closestDist) { closestDist = dist; closest = item; }
        }

        if (closest == null) return false;

        _briefcase = closest;
        _briefcase.AttachToHand(_rightHandBone);
        return true;
    }

    /// <summary>Lâche la malette instantanément — appelé lors du ragdoll.</summary>
    public void Drop()
    {
        if (!HasBriefcase) return;
        _briefcase.Detach();
        _briefcase = null;
    }
}
