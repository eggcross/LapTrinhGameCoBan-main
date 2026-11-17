
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class CardPairData
{
    public int pairId;
    public Sprite clubLogo;
}

public class CardGameManager : MonoBehaviour
{
    [Header("Du lieu cac cap the")]
    [SerializeField] private List<CardPairData> pairDataList =
        new List<CardPairData>();

    [Header("Tham chieu Prefab va Grid")]
    [SerializeField] private Transform gridContainer;
    [SerializeField] private CardItem cardPrefab;

    [Header("Tham chieu giao dien")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text footerHintText;

    [Header("Thong so man choi")]
    [SerializeField] private float levelTimeSeconds = 210f;
    [SerializeField] private int maxLives = 5;
    [SerializeField] private float compareDelay = 0.65f;

    private CardItem firstSelectedCard;
    private CardItem secondSelectedCard;

    private bool isCheckingMatch;
    private bool isGameActive;

    private int matchedPairsCount;
    private int totalPairsCount;
    private int currentLives;

    private float remainingTime;

    private void Start()
    {
        StartNewLevel();
    }

    private void Update()
    {
        if (!isGameActive) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            EndGame(false, "Het thoi gian!");
        }

        UpdateTimerUI();
    }

    public void StartNewLevel()
    {
        if (gridContainer == null || cardPrefab == null)
        {
            Debug.LogError("Chua gan Grid Container hoac Card Prefab!");
            return;
        }

        if (pairDataList.Count == 0)
        {
            Debug.LogError("Danh sach cap the dang trong!");
            return;
        }

        // Xoa cac the cu
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        // Khoi tao thong so
        totalPairsCount = pairDataList.Count;
        matchedPairsCount = 0;
        currentLives = maxLives;
        remainingTime = levelTimeSeconds;

        firstSelectedCard = null;
        secondSelectedCard = null;

        isCheckingMatch = false;
        isGameActive = true;

        // Tao bo bai: moi cap duoc them 2 lan
        List<CardPairData> deck = new List<CardPairData>();

        foreach (CardPairData pair in pairDataList)
        {
            deck.Add(pair);
            deck.Add(pair);
        }

        // Xao tron Fisher-Yates
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);

            CardPairData temp = deck[i];
            deck[i] = deck[rnd];
            deck[rnd] = temp;
        }

        // Sinh cac the vao Grid
        foreach (CardPairData data in deck)
        {
            CardItem newCard = Instantiate(cardPrefab, gridContainer);
            newCard.Initialize(data, this);
        }

        UpdateHudUI();
    }

    public void OnCardSelected(CardItem clickedCard)
    {
        if (!isGameActive || isCheckingMatch) return;

        if (clickedCard == null ||
            clickedCard.IsFlipped ||
            clickedCard.IsMatched)
        {
            return;
        }

        clickedCard.FlipOpen();

        if (firstSelectedCard == null)
        {
            firstSelectedCard = clickedCard;
        }
        else if (secondSelectedCard == null &&
                 clickedCard != firstSelectedCard)
        {
            secondSelectedCard = clickedCard;
            StartCoroutine(EvaluatePairRoutine());
        }
    }

    private IEnumerator EvaluatePairRoutine()
    {
        isCheckingMatch = true;

        yield return new WaitForSeconds(compareDelay);

        if (!isGameActive)
        {
            firstSelectedCard = null;
            secondSelectedCard = null;
            isCheckingMatch = false;
            yield break;
        }

        Debug.Log("The 1: " + firstSelectedCard.PairId);
        Debug.Log("The 2: " + secondSelectedCard.PairId);

        if (firstSelectedCard.PairId == secondSelectedCard.PairId)
        {
            // Ghep dung
            Debug.Log("GIONG CAP");
            firstSelectedCard.SetMatched();
            secondSelectedCard.SetMatched();

            matchedPairsCount++;

            UpdateHudUI();

            if (matchedPairsCount >= totalPairsCount)
            {
                EndGame(true, "Chuc mung! Ban da ghep het cac cap the!");
            }
        }
        else
        {
            // Ghep sai
            Debug.Log("KHAC CAP");
            firstSelectedCard.FlipClose();
            secondSelectedCard.FlipClose();

            currentLives--;

            UpdateHudUI();

            if (currentLives <= 0)
            {
                EndGame(false, "Ban da het luot thu!");
            }
        }

        firstSelectedCard = null;
        secondSelectedCard = null;

        isCheckingMatch = false;
    }

    private void UpdateHudUI()
    {
        if (livesText != null)
        {
            livesText.text = currentLives.ToString();
        }

        if (progressText != null)
        {
            progressText.text =
                "So cap da hoan thanh: " +
                matchedPairsCount + "/" + totalPairsCount;
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        timerText.text =
            minutes.ToString("00") + ":" +
            seconds.ToString("00");
    }

    private void EndGame(bool isWin, string message)
    {
        isGameActive = false;

        if (footerHintText != null)
        {
            footerHintText.text = message;
        }

        if (isWin)
        {
            Debug.Log("[VICTORY] " + message);
        }
        else
        {
            Debug.Log("[GAME OVER] " + message);
        }
    }
}
