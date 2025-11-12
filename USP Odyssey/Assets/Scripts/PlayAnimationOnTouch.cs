using System.Collections;
using UnityEngine;

public class PlayAnimationOnTouch : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject movementJoystickPanel;
    [SerializeField] private GameObject circleImage;
    [SerializeField] private GameObject playerImage;
    [SerializeField] private GameObject player;
    [SerializeField] private string triggerName = "StartAnimation";
    [SerializeField] private GameObject[] additionalUIToEnable;

    private bool animationTriggered;

    private void Start()
    {
        if (circleImage != null)
        {
            circleImage.SetActive(true);
        }
    }

    private void Update()
    {
        if (animationTriggered)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            BeginGameReveal();
        }
    }

    private void BeginGameReveal()
    {
        animationTriggered = true;

        if (playerImage != null)
        {
            playerImage.SetActive(false);
        }
        if (player != null)
        {
            player.SetActive(true);
        }
        if (movementJoystickPanel != null)
        {
            movementJoystickPanel.SetActive(true);
        }
        EnableAdditionalUI();
        if (animator != null)
        {
            animator.ResetTrigger(triggerName);
            animator.SetTrigger(triggerName);
            StartCoroutine(HideOverlayWhenAnimationFinishes());
        }
        else
        {
            HideOverlayInstant();
        }
    }

    private void EnableAdditionalUI()
    {
        if (additionalUIToEnable == null)
        {
            return;
        }

        for (int i = 0; i < additionalUIToEnable.Length; i++)
        {
            GameObject target = additionalUIToEnable[i];
            if (target != null)
            {
                target.SetActive(true);
            }
        }
    }

    private IEnumerator HideOverlayWhenAnimationFinishes()
    {
        // Allow the animator to enter the requested state this frame
        yield return null;

        if (animator == null)
        {
            HideOverlayInstant();
            yield break;
        }

        int layer = 0;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
        float timeout = stateInfo.length > 0f ? stateInfo.length + 0.1f : 2f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
            if (!animator.IsInTransition(layer) && stateInfo.normalizedTime >= 1f)
            {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        HideOverlayInstant();
        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    private void HideOverlayInstant()
    {
        if (circleImage != null)
        {
            circleImage.SetActive(false);
        }
    }
}
