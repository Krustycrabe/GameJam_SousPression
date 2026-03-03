using UnityEngine;

/// <summary>Notifie exactement quand le state Climb quitte l'Animator, avant l'application du root motion.</summary>
public class ClimbExitBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerEvents.RaiseClimbAnimationEnd();
    }
}
