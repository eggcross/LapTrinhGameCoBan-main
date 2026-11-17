
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Animator), typeof(CanvasGroup))]
public class CardItem : MonoBehaviour
{
    [Header("Tham chieu hinh anh")]
    [SerializeField] private Image frontLogoImage;

    private Button cardButton;
    private Animator cardAnimator;
    private CanvasGroup canvasGroup;

    private CardPairData cardData;
    private CardGameManager gameManager;

    public int PairId => cardData != null ? cardData.pairId : -1;

    public bool IsFlipped { get; private set; }
    public bool IsMatched { get; private set; }

    private static readonly int IsFlippedHash =
        Animator.StringToHash("isFlipped");

    private void Awake()
    {
        cardButton = GetComponent<Button>();
        cardAnimator = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();

        cardButton.onClick.AddListener(HandleCardClick);
    }

    public void Initialize(CardPairData data, CardGameManager manager)
    {
        cardData = data;
        gameManager = manager;

        IsFlipped = false;
        IsMatched = false;

        if (frontLogoImage != null && data != null)
        {
            frontLogoImage.sprite = data.clubLogo;
        }

        cardAnimator.SetBool(IsFlippedHash, false);

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HandleCardClick()
    {
        if (gameManager != null)
        {
            gameManager.OnCardSelected(this);
        }
    }

    public void FlipOpen()
    {
        IsFlipped = true;
        cardAnimator.SetBool(IsFlippedHash, true);
    }

    public void FlipClose()
    {
        IsFlipped = false;
        cardAnimator.SetBool(IsFlippedHash, false);
    }

    public void SetMatched()
    {
        IsMatched = true;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(
                1f, 0f, elapsed / duration
            );

            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
