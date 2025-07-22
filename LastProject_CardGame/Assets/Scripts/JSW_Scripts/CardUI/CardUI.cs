using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    public Image imageBack;
    public Image imageFront;
    public Image imageArtwork;
    public TMP_Text textCardName;
    public TMP_Text textCost;
    public TMP_Text textDescription;
    public TMP_Text textAttack;
    public TMP_Text textHealth;
    public TMP_Text textRace;

    private bool isFront = true;

    // 카드 앞/뒷면 전환
    public void SetFace(bool showFront)
    {
        imageBack.gameObject.SetActive(!showFront);
        imageFront.gameObject.SetActive(showFront);
        imageArtwork.gameObject.SetActive(showFront);
        textCardName.gameObject.SetActive(showFront);
        textCost.gameObject.SetActive(showFront);
        textDescription.gameObject.SetActive(showFront);
        textAttack.gameObject.SetActive(showFront);
        textHealth.gameObject.SetActive(showFront);
    }

    public void SetCard(BaseCardData data)
    {
        textCardName.text = data.cardName;
        imageArtwork.sprite = data.artwork;
        textCost.text = data.cost.ToString();
        textDescription.text = data.description;
        
        if(data is MonsterCardData m)
        {
            switch (m.race)
            {
                case Race.wizard:
                    textRace.text = "마법사";
                    break;
                case Race.Warrior:
                    textRace.text = "전사";
                    break;
                case Race.Undead:
                    textRace.text = "언데드";
                    break;
                case Race.Dragon:
                    textRace.text = "드래곤";
                    break;
                case Race.Fiend:
                    textRace.text = "악마";
                    break;
                case Race.Fairy:
                    textRace.text = "정령";
                    break;
                case Race.Fish:
                    textRace.text = "어류";
                    break;
                case Race.Insect:
                    textRace.text = "곤충";
                    break;
                case Race.Beast:
                    textRace.text = "야수";
                    break;
                case Race.Plant:
                    textRace.text = "식물";
                    break;
                case Race.Machine:
                    textRace.text = "기계";
                    break;
                case Race.Angel:
                    textRace.text = "천사";
                    break;
                default:
                    textRace.text = "";
                    break;

            }

        }

        // 몬스터 카드일 때만 공격력/체력 표시
        if (data is MonsterCardData monsterData)
        {
            textAttack.text = monsterData.attack.ToString();
            textHealth.text = monsterData.health.ToString();
            textRace.gameObject.SetActive(true);
            textAttack.gameObject.SetActive(true);
            textHealth.gameObject.SetActive(true);
        }
        else
        {
            textAttack.text = "";
            textHealth.text = "";
            textRace.gameObject.SetActive(false);
            textAttack.gameObject.SetActive(false);
            textHealth.gameObject.SetActive(false);
        }
    }

    public void FlipCard(bool showFront)
    {
        // 0.15초 동안 Y축 90도 회전 → 앞/뒷면 교체 → 다시 0도로 회전
        transform.DORotate(new Vector3(0, 90, 0), 0.15f)
            .OnComplete(() => {
                SetFace(showFront); // 앞/뒷면 교체
                transform.DORotate(Vector3.zero, 0.15f);
            });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isFront = !isFront;
        // SetFace(isFront);
        FlipCard(isFront);
    }
}
