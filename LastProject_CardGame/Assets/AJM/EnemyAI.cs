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

        // 남은 코스트를 최대한 사용하여 소환
        yield return StartCoroutine(SummonAsMuchAsPossible());

        // 스펠 카드 사용 시도 (코스트 허용 범위에서 다수)
        yield return StartCoroutine(PlaySpellsAsMuchAsPossible());

        yield return new WaitForSeconds(actionDelay);
    }

    /// <summary>
    /// 배틀 페이즈 처리 - 공격
    /// </summary>
    private IEnumerator HandleBattlePhase()
    {
        Debug.Log("[EnemyAI] 배틀 페이즈 시작");
        yield return new WaitForSeconds(thinkingTime);

        // 적 턴의 배틀 페이즈 보장
        if (GameManager.Instance != null && GameManager.Instance.IsEnemyTurn())
        {
            GameManager.Instance.StartEnemyBattlePhase();
        }

        // AI 필드의 모든 공격 가능한 몬스터로 공격 수행
        var enemyMonsters = GetEnemyMonsters();
        foreach (var monster in enemyMonsters)
        {
            if (monster != null && monster.gameObject != null && monster.isOnField && !monster.hasAttackedThisTurn)
            {
                // 매 공격자마다 최신 타깃 목록을 갱신하여 전투 결과 반영
                var currentPlayerMonsters = GetPlayerMonsters();
                yield return StartCoroutine(AttackWithMonster(monster, currentPlayerMonsters));
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
    private IEnumerator SummonAsMuchAsPossible()
    {
        if (OpponentCardManager.Instance == null || GameManager.Instance == null)
        {
            Debug.LogWarning("[EnemyAI] OpponentCardManager.Instance 또는 GameManager.Instance가 null입니다.");
            yield break;
        }

        bool summonedAtLeastOne = false;
        while (GameManager.Instance.enemyCurrentCost > 0)
        {
            // 적 몬스터 슬롯에 빈 자리가 없으면 소환 종료
            if (OpponentCardManager.Instance != null && !OpponentCardManager.Instance.HasEmptyEnemySlot())
            {
                Debug.Log("[EnemyAI] 적 몬스터 슬롯이 가득 찼습니다. 소환을 중단합니다.");
                break;
            }

            var handCards = OpponentCardManager.Instance.GetHandCards();
            if (handCards == null || handCards.Count == 0)
            {
                break;
            }

            // 현재 코스트로 낼 수 있는 '몬스터' 카드 중 가장 비싼 카드 우선 소환
            int currentCost = GameManager.Instance.enemyCurrentCost;
            var affordable = handCards
                .Where(c => c is MonsterCardData && c.cost <= currentCost)
                .OrderByDescending(c => c.cost)
                .ToList();

            if (affordable.Count == 0)
            {
                break;
            }

            var cardToSummon = affordable[0];
            Debug.Log($"[EnemyAI] 카드 소환 시도: {cardToSummon.cardName} (코스트 {cardToSummon.cost})");

            GameManager.Instance.enemyCurrentCost -= cardToSummon.cost;
            GameManager.Instance.UpdateCostUI();
            yield return StartCoroutine(OpponentCardManager.Instance.SummonCard(cardToSummon));
            Debug.Log($"[EnemyAI] {cardToSummon.cardName} 소환 완료. 남은 코스트: {GameManager.Instance.enemyCurrentCost}");

            summonedAtLeastOne = true;
            yield return new WaitForSeconds(actionDelay);
        }

        if (!summonedAtLeastOne)
        {
            Debug.Log("[EnemyAI] 소환할 수 있는 카드가 없습니다");
        }
    }

    // AI: 손패의 스펠(일반/지속)을 몬스터존 타겟 스팟에, 필드마법은 FieldZone에 사용
    private IEnumerator PlaySpellsAsMuchAsPossible()
    {
        if (OpponentCardManager.Instance == null) yield break;

        bool playedAny = false;
        while (true)
        {
            // 현재 손패 스냅샷
            var handCards = OpponentCardManager.Instance.GetHandCards();
            var spells = handCards.Where(c => c is SpellCardData).Cast<SpellCardData>().ToList();
            if (spells.Count == 0) break;

            bool playedThisLoop = false;

            foreach (var spell in spells.OrderByDescending(s => s.cost).ToList())
            {
                if (GameManager.Instance.enemyCurrentCost < spell.cost)
                    continue;

                // 손패 오브젝트 찾기
                var cardObj = OpponentCardManager.Instance.FindHandCardObject(spell);
                if (cardObj == null) continue;
                var ui = cardObj.GetComponent<CardUI>();
                if (ui == null) continue;

                // 필드 마법 처리
                if (spell.spellType == SpellType.Field)
                {
                    var fieldZoneGO = GameObject.Find("FieldZone");
                    var fieldZone = fieldZoneGO != null ? fieldZoneGO.GetComponent<FieldSpellZone>() : null;
                    if (fieldZone != null)
                    {
                        if (fieldZone.TryActivateFieldSpellForAI(cardObj))
                        {
                            playedThisLoop = true;
                            playedAny = true;
                            yield return new WaitForSeconds(actionDelay);
                            break; // 손패 목록이 변했으니 다시 스냅샷
                        }
                    }
                }
                else
                {
                    // 일반/지속 마법: 반드시 MonsterZone(SpellPlayTarget) 경로로 사용
                    var targets = FindObjectsByType<SpellPlayTarget>(FindObjectsSortMode.None);
                    SpellPlayTarget monsterZoneTarget = null;
                    foreach (var t in targets)
                    {
                        if (t == null) continue;
                        var n = t.gameObject.name;
                        if (!string.IsNullOrEmpty(n) && n.Contains("MonsterZone") && !n.StartsWith("E_"))
                        {
                            monsterZoneTarget = t;
                            break;
                        }
                    }
                    if (monsterZoneTarget != null)
                    {
                        if (monsterZoneTarget.TryPlaySpellFromCard(cardObj))
                        {
                            playedThisLoop = true;
                            playedAny = true;
                            yield return new WaitForSeconds(actionDelay);
                            break; // 손패 목록이 변했으니 다시 스냅샷
                        }
                    }
                }
            }

            if (!playedThisLoop) break;
        }

        if (!playedAny)
        {
            Debug.Log("[EnemyAI] 사용할 수 있는 스펠 카드가 없습니다");
        }
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
            // 전투 트리거(공격자/대상 설정을 즉시 실행) + BattleManager가 화살표를 타깃으로 향하게 처리
            BattleManager.Instance.SetAttacker(attacker.gameObject);
            BattleManager.Instance.SetTarget(target.gameObject);

            // 공격 완료까지 대기
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(actionDelay);
    }

    // (임시 라인) 필요 없어져서 제거

    /// <summary>
    /// 직접 공격
    /// </summary>
    private IEnumerator AttackDirectly(CardUI attacker)
    {
        Debug.Log($"[EnemyAI] {attacker.cardData.cardName} 직접 공격");

        // 플레이어 HitZone을 찾아 공격력만큼 HP 감소 처리
        var hitZones = FindObjectsByType<HitZone>(FindObjectsSortMode.None);
        HitZone playerHitZone = null;
        foreach (var hz in hitZones)
        {
            if (hz != null && hz.isPlayerZone)
            {
                playerHitZone = hz;
                break;
            }
        }

        if (playerHitZone != null)
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.SetAttacker(attacker.gameObject);
                BattleManager.Instance.DirectAttackHitZone(playerHitZone);
            }
            else
            {
                playerHitZone.OnHitByCard(attacker.gameObject);
            }
        }
        else
        {
            // HitZone을 못 찾은 경우: 상태만 정리
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.CancelAttack();
            }
        }

        yield return new WaitForSeconds(actionDelay);
    }


}
