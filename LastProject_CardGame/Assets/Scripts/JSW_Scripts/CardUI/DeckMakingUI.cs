using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class DeckMakingUI : MonoBehaviour
{
    public DeckBuilder deckBuilder;
    public Transform allCardsContent; // 전체 카드 리스트 Content
    public Transform mainDeckContent; // 메인덱 카드 리스트 Content
    public Transform extraDeckContent; // 엑스트라덱 카드 리스트 Content
    public GameObject CardThumbnailPrefab;
    public TMP_InputField deckNameInput;
    public Button saveButton;
    public Button backButton; // 뒤로가기 버튼
    public DeckSelectUI deckSelectUI; // 덱 선택 UI 참조
    public TMP_InputField searchBar;
    // 드롭다운 참조
    public TMP_Dropdown typeDropdown;      // 1번: 타입 필터
    public TMP_Dropdown sortDropdown;      // 2번: 정렬 기준
    public TMP_Dropdown orderDropdown;     // 3번: 오름/내림차순

    // 예시 데이터
    private readonly Dictionary<int, List<string>> filterOptionsBySort = new Dictionary<int, List<string>>()
    {
        // key: sortDropdown.value
        // value: filterDropdown 옵션 리스트
        { 0, new List<string> { "전체", "몬스터", "마법", "함정" } }, // 전체
        { 1, new List<string> { "전체", "일반", "효과", "융합", "싱크로", "엑시즈", "링크" } }, // 몬스터
        { 2, new List<string> { "전체", "일반 마법", "속공 마법", "장착 마법", "필드 마법", "지속 마법" } }, // 마법
        { 3, new List<string> { "전체", "일반 함정", "지속 함정", "카운터 함정" } }, // 함정
    };

    void Start()
    {
        // 현재 선택된 덱이 있으면 불러오고, 없으면 새 덱 생성
        if (CardManager.Instance.currentDeck != null)
        {
            deckBuilder.LoadDeck(CardManager.Instance.currentDeck);
            if (deckNameInput != null)
                deckNameInput.text = CardManager.Instance.currentDeck.deckName;
        }
        else
        {
            // 새 덱 생성
            string defaultDeckName = "새 덱";
            deckBuilder.CreateDeck(defaultDeckName);
            if (deckNameInput != null)
                deckNameInput.text = defaultDeckName;
        }
        
        // 최초 1회만 UI 갱신
        RefreshDeckList();
        RefreshAllCardList();
        
        saveButton.onClick.AddListener(() => {
            // 덱 이름 업데이트 및 저장
            if (deckBuilder.currentDeck != null && deckNameInput != null)
            {
                deckBuilder.currentDeck.deckName = deckNameInput.text;
                deckBuilder.SaveCurrentDeck();
            }
        });
        
        // 뒤로가기 버튼
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);

        if (searchBar != null)
            searchBar.onValueChanged.AddListener(_ => RefreshAllCardList());
        if (typeDropdown != null)
            typeDropdown.onValueChanged.AddListener(_ => RefreshAllCardList());
        if (sortDropdown != null)
            sortDropdown.onValueChanged.AddListener(_ => RefreshAllCardList());
        if (orderDropdown != null)
            orderDropdown.onValueChanged.AddListener(_ => RefreshAllCardList());

        // 드롭다운 옵션 세팅(최초 1회)
        if (typeDropdown != null)
            typeDropdown.ClearOptions();
        typeDropdown.AddOptions(new List<string> { "전체", "몬스터", "마법", "함정" });

        if (sortDropdown != null)
            sortDropdown.ClearOptions();
        sortDropdown.AddOptions(new List<string> { "카드이름", "레어도", "공격력", "체력" });

        if (orderDropdown != null)
            orderDropdown.ClearOptions();
        orderDropdown.AddOptions(new List<string> { "오름차순", "내림차순" });

        // 최초 옵션 세팅
        OnSortDropdownChanged(sortDropdown.value);
    }

    void OnEnable()
    {
        if (DeckBuilder.Instance != null)
            DeckBuilder.Instance.OnDeckChanged += OnDeckChangedHandler;
    }
    void OnDisable()
    {
        if (DeckBuilder.Instance != null)
            DeckBuilder.Instance.OnDeckChanged -= OnDeckChangedHandler;
    }

    // 덱 변경 이벤트 핸들러
    void OnDeckChangedHandler()
    {
        RefreshDeckList();
        RefreshAllCardList();
    }

    // 카드가 엑스트라덱 대상인지 판별 (Normal, Effect가 아니면 엑스트라덱)
    bool IsExtraDeckCard(BaseCardData card)
    {
        if (card is MonsterCardData m)
        {
            return m.monsterType != MonsterType.Normal && m.monsterType != MonsterType.Effect;
        }
        return false;
    }

    // 1. 카드리스트 UI 갱신 함수
    void RefreshAllCardList()
    {
        string keyword = searchBar != null ? searchBar.text.ToLower() : "";
        int typeFilter = typeDropdown != null ? typeDropdown.value : 0;
        int sortType = sortDropdown != null ? sortDropdown.value : 0;
        int orderType = orderDropdown != null ? orderDropdown.value : 0;

        var allCards = CardManager.Instance.GetAllCards();

        // 1. 검색
        var filtered = allCards.Where(card =>
            card.cardName.ToLower().Contains(keyword) ||
            card.description.ToLower().Contains(keyword)
        );

        // 2. 타입 필터
        if (typeFilter == 1)
            filtered = filtered.Where(card => card.cardType == CardType.Monster);
        else if (typeFilter == 2)
            filtered = filtered.Where(card => card.cardType == CardType.Spell);
        else if (typeFilter == 3)
            filtered = filtered.Where(card => card.cardType == CardType.Trap);

        // 3. 정렬
        IOrderedEnumerable<BaseCardData> ordered = null;
        switch (sortType)
        {
            case 0: // 카드이름
                ordered = orderType == 0
                    ? filtered.OrderBy(card => card.cardName)
                    : filtered.OrderByDescending(card => card.cardName);
                break;
            case 1: // 레어도
                ordered = orderType == 0
                    ? filtered.OrderBy(card => card.rarity)
                    : filtered.OrderByDescending(card => card.rarity);
                break;
            case 2: // 공격력
                ordered = orderType == 0
                    ? filtered.OrderBy(card => (card is MonsterCardData m ? m.attack : 0))
                    : filtered.OrderByDescending(card => (card is MonsterCardData m ? m.attack : 0));
                break;
            case 3: // 체력
                ordered = orderType == 0
                    ? filtered.OrderBy(card => (card is MonsterCardData m ? m.health : 0))
                    : filtered.OrderByDescending(card => (card is MonsterCardData m ? m.health : 0));
                break;
            default:
                ordered = filtered.OrderBy(card => card.cardName);
                break;
        }

        // 4. 리스트 UI 갱신
        foreach (Transform child in allCardsContent)
            Destroy(child.gameObject);

        foreach (var card in ordered)
        {
            GameObject obj = Instantiate(CardThumbnailPrefab, allCardsContent);
            CardThumbnail thumbnail = obj.GetComponent<CardThumbnail>();
            int owned = CardManager.Instance.GetOwnedCardCount(card);
            int available = CardManager.Instance.GetAvailableCardCount(card);
            thumbnail.SetCard(card, owned, available);

            if (available == 0)
                thumbnail.SetUnavailableVisual();

            // -------------------------------
            // 이벤트 등록(좌클릭: 상세, 우클릭: 덱에 추가)
            EventTrigger trigger = obj.GetComponent<EventTrigger>();
            if (trigger == null) trigger = obj.AddComponent<EventTrigger>();
            AddCardDetailEvent(trigger, card);

            // 우클릭: 덱에 추가
            EventTrigger.Entry rightClick = new EventTrigger.Entry();
            rightClick.eventID = EventTriggerType.PointerClick;
            var capturedCard = card;
            rightClick.callback.AddListener((data) => {
                PointerEventData ped = (PointerEventData)data;
                if (ped.button == PointerEventData.InputButton.Right && available > 0)
                {
                    bool isExtraDeck = IsExtraDeckCard(capturedCard);
                    bool added = false;
                    if (isExtraDeck)
                        added = deckBuilder.AddCardToExtra(capturedCard);
                    else
                        added = deckBuilder.AddCardToMain(capturedCard);
                    if (!added)
                        Debug.Log("카드 추가 실패: 조건 불충족");
                    // UI 갱신은 OnDeckChanged에서만!
                }
            });
            trigger.triggers.Add(rightClick);
            // -------------------------------
        }
    }

    // 2. 덱 UI 갱신 함수(덱에서 제거 시 카드리스트도 갱신 X)
    public void RefreshDeckList()
    {
        // 메인덱 UI 갱신
        foreach (Transform child in mainDeckContent)
            Destroy(child.gameObject);
        foreach (var entry in deckBuilder.currentDeck.mainDeck)
        {
            GameObject cardObj = Instantiate(CardThumbnailPrefab, mainDeckContent);
            CardThumbnail thumbnail = cardObj.GetComponent<CardThumbnail>();
            thumbnail.SetCard(entry.card, entry.count);

            EventTrigger trigger = cardObj.GetComponent<EventTrigger>();
            if (trigger == null) trigger = cardObj.AddComponent<EventTrigger>();
            // 좌클릭: 상세정보 보기
            AddCardDetailEvent(trigger, entry.card);
            // 우클릭: 덱에서 제거
            EventTrigger.Entry rightClick = new EventTrigger.Entry();
            rightClick.eventID = EventTriggerType.PointerClick;
            var capturedEntry = entry;
            rightClick.callback.AddListener((data) => {
                PointerEventData ped = (PointerEventData)data;
                if (ped.button == PointerEventData.InputButton.Right)
                {
                    deckBuilder.RemoveCardFromMain(capturedEntry.card);
                    // UI 갱신은 OnDeckChanged에서만!
                }
            });
            trigger.triggers.Add(rightClick);
        }
        // 엑스트라덱 UI 갱신
        foreach (Transform child in extraDeckContent)
            Destroy(child.gameObject);
        foreach (var entry in deckBuilder.currentDeck.extraDeck)
        {
            GameObject cardObj = Instantiate(CardThumbnailPrefab, extraDeckContent);
            CardThumbnail thumbnail = cardObj.GetComponent<CardThumbnail>();
            thumbnail.SetCard(entry.card, entry.count);

            EventTrigger trigger = cardObj.GetComponent<EventTrigger>();
            if (trigger == null) trigger = cardObj.AddComponent<EventTrigger>();
            // 좌클릭: 상세정보 보기
            AddCardDetailEvent(trigger, entry.card);
            // 우클릭: 덱에서 제거
            EventTrigger.Entry rightClick = new EventTrigger.Entry();
            rightClick.eventID = EventTriggerType.PointerClick;
            var capturedExtraEntry = entry;
            rightClick.callback.AddListener((data) => {
                PointerEventData ped = (PointerEventData)data;
                if (ped.button == PointerEventData.InputButton.Right)
                {
                    deckBuilder.RemoveCardFromExtra(capturedExtraEntry.card);
                    // UI 갱신은 OnDeckChanged에서만!
                }
            });
            trigger.triggers.Add(rightClick);
        }
    }

    // 덱 선택 화면에서 덱을 받아와서 UI를 갱신하는 함수
    public void OpenWithDeck(DeckData deck)
    {
        deckBuilder.LoadDeck(deck);
        if (deckNameInput != null)
            deckNameInput.text = deck.deckName;
        RefreshDeckList();
        RefreshAllCardList();
    }

    // 뒤로가기 버튼 클릭
    private void OnBackButtonClicked()
    {
        // 덱 이름 업데이트 및 저장 (뒤로가기 전에 저장)
        if (deckBuilder.currentDeck != null && deckNameInput != null)
        {
            deckBuilder.currentDeck.deckName = deckNameInput.text;
            deckBuilder.SaveCurrentDeck();
        }
        
        // 덱 선택 화면으로 돌아가기
        if (deckSelectUI != null)
        {
            deckSelectUI.gameObject.SetActive(true);
            deckSelectUI.RefreshDeckList(); // 덱 리스트 새로고침
        }
        else
        {
            Debug.LogWarning("DeckSelectUI 참조가 설정되지 않았습니다!");
        }
        
        gameObject.SetActive(false);
    }
    
    // 공통 함수로 분리
    void AddCardDetailEvent(EventTrigger trigger, BaseCardData card)
    {
        EventTrigger.Entry leftClick = new EventTrigger.Entry();
        leftClick.eventID = EventTriggerType.PointerClick;
        leftClick.callback.AddListener((data) => {
            PointerEventData ped = (PointerEventData)data;
            if (ped.button == PointerEventData.InputButton.Left)
            {
                CardDetailUI.Instance.SetCardDetail(card);
            }
        });
        trigger.triggers.Add(leftClick);
    }

    void OnSortDropdownChanged(int sortIndex)
    {
        // 드롭다운 옵션 동적 변경
        if (filterOptionsBySort.ContainsKey(sortIndex))
        {
            // This part is no longer needed as options are set in Start()
            // if (filterDropdown != null)
            // {
            //     filterDropdown.ClearOptions();
            //     filterDropdown.AddOptions(filterOptionsBySort[sortIndex]);
            //     filterDropdown.value = 0;
            //     filterDropdown.RefreshShownValue();
            // }
        }
        RefreshAllCardList();
    }
}
