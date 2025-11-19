using UnityEngine;
using UnityEngine.UI;
public class CollectibleMenuEntry : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameLabel;
    [SerializeField] private Button iconButton;
    [SerializeField] private Color unlockedNameColor = Color.white;
    [SerializeField] private Color lockedNameColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
    [SerializeField, Range(0f, 1f)] private float lockedIconAlpha = 0.4f;
    [SerializeField] private Graphic lockedOverlay;

    private Collectible.CollectibleInfo info;
    private System.Action<Collectible.CollectibleInfo> onClicked;
    private bool isUnlocked;
    private bool canInteract;

    public void Initialize(
        Collectible.CollectibleInfo collectibleInfo,
        bool unlocked,
        bool allowInteraction,
        System.Action<Collectible.CollectibleInfo> handleClick)
    {
        info = collectibleInfo;
        isUnlocked = unlocked;
        canInteract = allowInteraction && unlocked;
        onClicked = handleClick;
        if (iconImage != null)
        {
            iconImage.sprite = info.Icon;
            iconImage.enabled = info.Icon != null;
            Color iconColor = iconImage.color;
            iconColor.a = unlocked ? 1f : lockedIconAlpha;
            iconImage.color = iconColor;
        }
        if (nameLabel != null)
        {
            nameLabel.text = info.Name;
            nameLabel.color = unlocked ? unlockedNameColor : lockedNameColor;
        }
        if (iconButton != null)
        {
            iconButton.onClick.RemoveAllListeners();
            iconButton.onClick.AddListener(HandleClick);
            iconButton.interactable = canInteract;
        }

        if (lockedOverlay != null)
        {
            lockedOverlay.enabled = !unlocked;
        }
    }
    private void HandleClick()
    {
        if (!isUnlocked || !canInteract)
        {
            return;
        }

        onClicked?.Invoke(info);
    }
}
