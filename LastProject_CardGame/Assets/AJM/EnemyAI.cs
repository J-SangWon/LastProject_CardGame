using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance { get; private set; }

    [Header("AI 설정")]
    public float actionDelay = 1f; // 각 액션 사이의 지연 시간
    public float thinkingTime = 0.5f; // AI가 생각하는 시간

    private bool isAITurnActive = false;

        // 제한 존 이름들: 이 존들에 있는 카드는 공격 대상에서 제외
        private static readonly HashSet<string> restrictedZoneNames = new HashSet<string>
        {
            "FieldZone",
            "GraveZone",
            "EnemyGraveZone"
        };

        /// <summary>
        /// 주어진 트랜스폼이 제한된 존(FieldZone/GraveZone/EnemyGraveZone)의 하위에 있는지 확인
        /// </summary>
        private bool IsInRestrictedZone(Transform targetTransform)
        {
            var current = targetTransform;
            while (current != null)
            {
                if (restrictedZoneNames.Contains(current.name))
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// AI 턴 시작
    /// </summary>
    public void StartAITurn()
    {
        if (isAITurnActive) return;

        isAITurnActive = true;
        Debug.Log("[EnemyAI] AI 턴 시작 - 자동으로 진행합니다");
        // AI가 자동으로 턴을 진행
        StartCoroutine(AITurnCoroutine());
    }

    /// <summary>
    /// AI �� ����
    /// </summary>
    public void StopAITurn()
    {
        isAITurnActive = false;
    }

    /// <summary>
    /// AI 턴 코루틴 - 전체 턴을 자동으로 관리
    /// </summary>
    private IEnumerator AITurnCoroutine()
    {
        Debug.Log("[EnemyAI] AI 턴 자동 진행 시작");

        // 메인 페이즈
        yield return StartCoroutine(HandleMainPhase());

        // 배틀 페이즈
        yield return StartCoroutine(HandleBattlePhase());

        // 엔드 페이즈 - 턴 종료
        Debug.Log("[EnemyAI] 엔드 페이즈 - 턴 종료");
        yield return new WaitForSeconds(thinkingTime);

        // �� ����
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndPlayerTurn();
        }

        Debug.Log("[EnemyAI] AI 턴 완료");
        isAITurnActive = false;
    }



    /// <summary>
    /// 메인 페이즈 처리 - 카드 소환
    /// </summary>
    private IEnumerator HandleMainPhase()
    {
        Debug.Log("[EnemyAI] 메인 페이즈 시작");
        yield return new WaitForSeconds(thinkingTime);

        // AI가 소환할 수 있는 카드가 있는지 확인
        if (CanSummonCard())
        {
            yield return StartCoroutine(SummonRandomCard());
        }
        else
        {
            Debug.Log("[EnemyAI] 소환할 카드가 없습니다");
        }

        yield return new WaitForSeconds(actionDelay);
    }

    /// <summary>
    /// 배틀 페이즈 처리 - 공격
    /// </summary>
    private IEnumerator HandleBattlePhase()
    {
        Debug.Log("[EnemyAI] 배틀 페이즈 시작");
        yield return new WaitForSeconds(thinkingTime);

        // AI 필드의 몬스터들이 공격할 수 있는지 확인
        var enemyMonsters = GetEnemyMonsters();
        var playerMonsters = GetPlayerMonsters();

        foreach (var monster in enemyMonsters)
        {
            if (monster != null && !monster.hasAttackedThisTurn)
            {
                yield return StartCoroutine(AttackWithMonster(monster, playerMonsters));
                yield return new WaitForSeconds(actionDelay);
            }
        }

        yield return new WaitForSeconds(actionDelay);
    }



    /// <summary>
    /// 카드 소환 가능 여부 확인
    /// </summary>
    private bool CanSummonCard()
    {
        // 간단한 로직: 코스트가 있고, 핸드에 카드가 있으면 소환 가능
        return GameManager.Instance != null && GameManager.Instance.enemyCurrentCost > 0 && HasCardsInHand();
    }

    /// <summary>
    /// 핸드에 카드가 있는지 확인
    /// </summary>
    private bool HasCardsInHand()
    {
        // OpponentCardManager에서 핸드 카드 확인
        if (OpponentCardManager.Instance != null)
        {
            return OpponentCardManager.Instance.GetHandCardCount() > 0;
        }
        Debug.LogWarning("[EnemyAI] OpponentCardManager.Instance가 null입니다.");
        return false;
    }

    /// <summary>
    /// 랜덤 카드 소환
    /// </summary>
    private IEnumerator SummonRandomCard()
    {
        Debug.Log("[EnemyAI] 카드 소환 시도");

        // OpponentCardManager에서 핸드의 첫 번째 카드를 소환
        if (OpponentCardManager.Instance != null && GameManager.Instance != null)
        {
            var handCards = OpponentCardManager.Instance.GetHandCards();
            if (handCards.Count > 0)
            {
                var cardToSummon = handCards[0]; // 첫 번째 카드 선택

                // 코스트 확인 및 사용
                if (GameManager.Instance.enemyCurrentCost >= cardToSummon.cost)
                {
                    GameManager.Instance.enemyCurrentCost -= cardToSummon.cost;
                    GameManager.Instance.UpdateCostUI(); // UI 업데이트
                    yield return StartCoroutine(OpponentCardManager.Instance.SummonCard(cardToSummon));
                    Debug.Log($"[EnemyAI] {cardToSummon.cardName} 소환 완료");
                }
                else
                {
                    Debug.Log("[EnemyAI] 코스트 부족으로 소환 실패");
                }
            }
            else
            {
                Debug.Log("[EnemyAI] 핸드에 카드가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("[EnemyAI] OpponentCardManager.Instance 또는 GameManager.Instance가 null입니다.");
        }

        yield return new WaitForSeconds(actionDelay);
    }

    /// <summary>
    /// AI 필드의 몬스터들 가져오기
    /// </summary>
    private List<CardUI> GetEnemyMonsters()
    {
        var enemyMonsters = new List<CardUI>();
        var allCards = FindObjectsByType<CardUI>(FindObjectsSortMode.None);

        foreach (var card in allCards)
        {
            if (card.isOnField && card.cardData != null && card.ownerType == OwnerType.Opponent && !IsInRestrictedZone(card.transform))
            {
                enemyMonsters.Add(card);
            }
        }

        return enemyMonsters;
    }

    /// <summary>
    /// 플레이어 필드의 몬스터들 가져오기
    /// </summary>
    private List<CardUI> GetPlayerMonsters()
    {
        var playerMonsters = new List<CardUI>();
        var allCards = FindObjectsByType<CardUI>(FindObjectsSortMode.None);

        foreach (var card in allCards)
        {
            if (card.isOnField && card.cardData != null && card.ownerType == OwnerType.Player && !IsInRestrictedZone(card.transform))
            {
                playerMonsters.Add(card);
            }
        }

        return playerMonsters;
    }

    /// <summary>
    /// 몬스터로 공격
    /// </summary>
    private IEnumerator AttackWithMonster(CardUI attacker, List<CardUI> targets)
    {
        if (attacker == null) yield break;

        Debug.Log($"[EnemyAI] {attacker.cardData.cardName} 공격 시도");

        if (targets.Count > 0)
        {
            // 플레이어 몬스터가 있으면 가장 약한 몬스터 공격
            var weakestTarget = GetWeakestMonster(targets);
            if (weakestTarget != null)
            {
                yield return StartCoroutine(AttackMonster(attacker, weakestTarget));
            }
        }
        else
        {
            // 플레이어 몬스터가 없으면 직접 공격
            yield return StartCoroutine(AttackDirectly(attacker));
        }
    }

    /// <summary>
    /// 가장 약한 몬스터 찾기
    /// </summary>
    private CardUI GetWeakestMonster(List<CardUI> monsters)
    {
        if (monsters.Count == 0) return null;

        CardUI weakest = monsters[0];
        foreach (var monster in monsters)
        {
            if (monster.attack < weakest.attack)
            {
                weakest = monster;
            }
        }
        return weakest;
    }

    /// <summary>
    /// 몬스터 공격
    /// </summary>
    private IEnumerator AttackMonster(CardUI attacker, CardUI target)
    {
        // 안전장치: 타깃이 제한 존에 있다면 공격 취소
        if (target == null || IsInRestrictedZone(target.transform))
        {
            yield break;
        }
        Debug.Log($"[EnemyAI] {attacker.cardData.cardName} -> {target.cardData.cardName} 공격");

        // BattleManager를 통해 공격 실행
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SetAttacker(attacker.gameObject);
            yield return new WaitForSeconds(0.1f);
            BattleManager.Instance.SetTarget(target.gameObject);

            // 공격 완료까지 대기
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(actionDelay);
    }

    /// <summary>
    /// 직접 공격
    /// </summary>
    private IEnumerator AttackDirectly(CardUI attacker)
    {
        Debug.Log($"[EnemyAI] {attacker.cardData.cardName} 직접 공격");

        // BattleManager를 통해 직접 공격 실행
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SetAttacker(attacker.gameObject);
            yield return new WaitForSeconds(0.1f);
            // 직접 공격은 플레이어 생명력에 직접 데미지를 주는 방식으로 처리
            // 여기서는 간단히 로그만 출력
            Debug.Log($"[EnemyAI] {attacker.cardData.cardName} 직접 공격으로 플레이어에게 {attacker.attack} 데미지");
        }

        yield return new WaitForSeconds(actionDelay);
    }


}
