using UnityEngine;

/// <summary>
/// Связывает движение/атаку с Animator. Работает без контроллера — просто ничего не анимирует.
/// </summary>
[DisallowMultipleComponent]
public class CharacterAnimationBridge : MonoBehaviour
{
    static readonly int SpeedHash = Animator.StringToHash(GameArtSettings.AnimatorParameters.Speed);
    static readonly int MoveXHash = Animator.StringToHash(GameArtSettings.AnimatorParameters.MoveX);
    static readonly int MoveYHash = Animator.StringToHash(GameArtSettings.AnimatorParameters.MoveY);
    static readonly int IsAttackingHash = Animator.StringToHash(GameArtSettings.AnimatorParameters.IsAttacking);
    static readonly int AttackTriggerHash = Animator.StringToHash(GameArtSettings.AnimatorParameters.AttackTrigger);
    static readonly int DeathTriggerHash = Animator.StringToHash("Death");

    [SerializeField] UnityEngine.Animator animator;
    [SerializeField] Transform flipTarget;
    [SerializeField] bool flipOnHorizontalMovement = true;
    [SerializeField] bool snapToFourDirections = true;

    Vector2 lastFacing = Vector2.down;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<UnityEngine.Animator>(true);

        if (flipTarget == null)
        {
            var body = transform.Find("Visual");
            flipTarget = body != null ? body : transform;
        }

        ApplyAnimatorDefaults();
    }

    void Start()
    {
        ApplyAnimatorDefaults();
    }

    void ApplyAnimatorDefaults()
    {
        if (animator == null)
            return;

        lastFacing = SnapToFourDirections(lastFacing);
        animator.SetFloat(MoveXHash, lastFacing.x);
        animator.SetFloat(MoveYHash, lastFacing.y);
        animator.SetFloat(SpeedHash, 0f);
        animator.SetBool(IsAttackingHash, false);
        animator.Update(0f);
    }

    public void UpdateLocomotion(Vector2 movement, bool isAttacking)
    {
        if (animator == null)
            return;

        if (movement.sqrMagnitude > 0.0001f)
            lastFacing = movement.normalized;
        else if (snapToFourDirections)
            lastFacing = SnapToFourDirections(lastFacing);

        animator.SetFloat(SpeedHash, movement.sqrMagnitude);
        animator.SetFloat(MoveXHash, lastFacing.x);
        animator.SetFloat(MoveYHash, lastFacing.y);
        animator.SetBool(IsAttackingHash, isAttacking);

        ApplyFlip();
    }

    public void PlayAttack()
    {
        if (animator == null)
            return;

        if (HasParameter(AttackTriggerHash, AnimatorControllerParameterType.Trigger))
            animator.SetTrigger(AttackTriggerHash);
        else
            animator.SetBool(IsAttackingHash, true);
    }

    public void EndAttackAnimation()
    {
        if (animator == null)
            return;

        animator.SetBool(IsAttackingHash, false);
    }

    public void PlayDeath()
    {
        if (animator == null)
            return;

        if (HasParameter(DeathTriggerHash, AnimatorControllerParameterType.Trigger))
            animator.SetTrigger(DeathTriggerHash);
    }

    void ApplyFlip()
    {
        if (!flipOnHorizontalMovement || flipTarget == null || Mathf.Abs(lastFacing.x) < 0.01f)
            return;

        var scale = flipTarget.localScale;
        scale.x = Mathf.Abs(scale.x) * (lastFacing.x < 0f ? -1f : 1f);
        flipTarget.localScale = scale;
    }

    static Vector2 SnapToFourDirections(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return Vector2.down;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            return new Vector2(Mathf.Sign(direction.x), 0f);

        return new Vector2(0f, Mathf.Sign(direction.y == 0f ? -1f : direction.y));
    }

    bool HasParameter(int hash, AnimatorControllerParameterType type)
    {
        foreach (var p in animator.parameters)
        {
            if (p.nameHash == hash && p.type == type)
                return true;
        }
        return false;
    }
}
