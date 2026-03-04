using UnityEngine;

/// <summary>
/// Gère le ramassage, le lâcher et le handoff du lancer.
/// Référence centrale à la BriefcaseItem.
/// </summary>
public class PlayerBriefCaseController : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Transform _rightHandBone;
    [SerializeField] private BriefcaseItem _initialBriefcase;  // drag la malette de la scène
    [SerializeField] private float _pickupRange = 2f;

    private BriefcaseItem _briefcase;
    private bool _isHolding;
    public bool IsHolding => _isHolding;

    private void Start()
    {
        BriefcaseItem found = _rightHandBone.GetComponentInChildren<BriefcaseItem>();

        if (found != null)
        {
            _briefcase = found;
            _isHolding = true;
            PlayerEvents.RaiseBriefcasePickedUp(_briefcase.transform);
        }
    }

    private void OnEnable()
    {
        PlayerEvents.OnPickupInput += HandlePickupInput;
        PlayerEvents.OnThrowExecuted += HandleThrowExecuted;
        PlayerEvents.OnSweepFallStarted += HandleSweepFallStarted;
    }

    private void OnDisable()
    {
        PlayerEvents.OnPickupInput -= HandlePickupInput;
        PlayerEvents.OnThrowExecuted -= HandleThrowExecuted;
        PlayerEvents.OnSweepFallStarted -= HandleSweepFallStarted;
    }

    private void HandleSweepFallStarted()
    {
        if (_isHolding) Drop();
    }

    private void HandlePickupInput()
    {
        if (_isHolding) Drop();
        else TryPickup();
    }

    private void TryPickup()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, _pickupRange);
        BriefcaseItem closest = null;
        float closestDist = float.MaxValue;

        foreach (Collider col in nearby)
        {
            // GetComponentInParent remonte la hiérarchie : malette → BriefCaseItem ✓
            BriefcaseItem item = col.GetComponentInParent<BriefcaseItem>();
            if (item == null || item.IsHeld) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < closestDist) { closestDist = dist; closest = item; }
        }

        if (closest != null) Attach(closest);
    }

    private void Attach(BriefcaseItem item)
    {
        _briefcase = item;
        _briefcase.AttachToHand(_rightHandBone);
        _isHolding = true;
        PlayerEvents.RaiseBriefcasePickedUp(_briefcase.transform);
    }

    private void Drop()
    {
        _briefcase.Detach();
        _isHolding = false;
        PlayerEvents.RaiseBriefcaseDropped();
        _briefcase = null;
    }

    private void HandleThrowExecuted(Vector3 velocity)
    {
        if (!_isHolding) return;
        _briefcase.Launch(velocity);
        _isHolding = false;
        PlayerEvents.RaiseBriefcaseDropped();
        _briefcase = null;
    }
}
