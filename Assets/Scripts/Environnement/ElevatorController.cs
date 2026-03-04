using System.Collections;
using UnityEngine;

/// <summary>
/// Gère le cycle complet de l'ascenseur.
/// La détection du player est déléguée à un enfant ElevatorTriggerZone.
/// </summary>
public class ElevatorController : MonoBehaviour
{
    private enum ElevatorState
    {
        IdleAtBottom,
        Opening,
        OpenAtBottom,
        ClosingAtBottom,
        MovingUp,
        OpenAtTop,
        ClosingAtTop,
        MovingDown
    }

    private const float PositionThreshold = 0.001f;

    [Header("Portes")]
    [SerializeField] private Transform _doorLeft;
    [SerializeField] private Transform _doorRight;
    [SerializeField] private Vector3 _doorLeftOpenOffset = new Vector3(-1.5f, 0f, 0f);
    [SerializeField] private Vector3 _doorRightOpenOffset = new Vector3(1.5f, 0f, 0f);
    [SerializeField] private float _doorSpeed = 2f;

    [Header("Timing")]
    [SerializeField] private float _closeDelay = 1.5f;
    [SerializeField] private float _moveDelay = 1f;

    [Header("Déplacement")]
    [SerializeField] private Transform _destinationMarker;
    [SerializeField] private float _elevatorSpeed = 3f;

    private ElevatorState _state = ElevatorState.IdleAtBottom;
    private Vector3 _bottomPosition;
    private Vector3 _topPosition;
    private Vector3 _doorLeftClosedLocalPos;
    private Vector3 _doorRightClosedLocalPos;
    private Coroutine _sequenceCoroutine;
    private Transform _playerTransform;

    private void Start()
    {
        _bottomPosition = transform.position;
        _topPosition = _destinationMarker.position;
        _doorLeftClosedLocalPos = _doorLeft.localPosition;
        _doorRightClosedLocalPos = _doorRight.localPosition;
    }

    /// <summary>Appelé par ElevatorTriggerZone quand le player entre.</summary>
    public void OnPlayerEntered(Transform player)
    {
        _playerTransform = player;

        if (_state == ElevatorState.IdleAtBottom)
        {
            if (_sequenceCoroutine != null) StopCoroutine(_sequenceCoroutine);
            _sequenceCoroutine = StartCoroutine(AscendSequence());
        }
    }

    /// <summary>Appelé par ElevatorTriggerZone quand le player sort.</summary>
    public void OnPlayerExited()
    {
        if (_state == ElevatorState.OpenAtTop)
        {
            if (_sequenceCoroutine != null) StopCoroutine(_sequenceCoroutine);
            _sequenceCoroutine = StartCoroutine(DescendSequence());
        }
    }

    private IEnumerator AscendSequence()
    {
        _state = ElevatorState.Opening;
        yield return MoveDoors(open: true);

        _state = ElevatorState.OpenAtBottom;
        yield return new WaitForSeconds(_closeDelay);

        _state = ElevatorState.ClosingAtBottom;
        yield return MoveDoors(open: false);

        yield return new WaitForSeconds(_moveDelay);

        _state = ElevatorState.MovingUp;
        SetPlayerParented(true);
        yield return MoveElevator(_topPosition);
        SetPlayerParented(false);

        _state = ElevatorState.Opening;
        yield return MoveDoors(open: true);

        _state = ElevatorState.OpenAtTop;
    }

    private IEnumerator DescendSequence()
    {
        yield return new WaitForSeconds(_closeDelay);

        _state = ElevatorState.ClosingAtTop;
        yield return MoveDoors(open: false);

        _state = ElevatorState.MovingDown;
        yield return MoveElevator(_bottomPosition);

        _state = ElevatorState.IdleAtBottom;
    }

    private IEnumerator MoveDoors(bool open)
    {
        Vector3 leftTarget = _doorLeftClosedLocalPos + (open ? _doorLeftOpenOffset : Vector3.zero);
        Vector3 rightTarget = _doorRightClosedLocalPos + (open ? _doorRightOpenOffset : Vector3.zero);

        while (Vector3.Distance(_doorLeft.localPosition, leftTarget) > PositionThreshold)
        {
            _doorLeft.localPosition = Vector3.MoveTowards(
                _doorLeft.localPosition, leftTarget, _doorSpeed * Time.deltaTime);
            _doorRight.localPosition = Vector3.MoveTowards(
                _doorRight.localPosition, rightTarget, _doorSpeed * Time.deltaTime);
            yield return null;
        }

        _doorLeft.localPosition = leftTarget;
        _doorRight.localPosition = rightTarget;
    }

    private IEnumerator MoveElevator(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > PositionThreshold)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target, _elevatorSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
    }

    /// <summary>Parenté le player à la cabine pendant le déplacement.</summary>
    private void SetPlayerParented(bool parented)
    {
        if (_playerTransform == null) return;
        _playerTransform.SetParent(parented ? transform : null);
    }
}
