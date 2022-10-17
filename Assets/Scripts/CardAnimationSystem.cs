using System.Threading.Tasks;
using Client;
using DG.Tweening;
using Leopotam.Ecs;
using UnityEngine;

internal class CardAnimationSystem : IEcsSystem
{
    private SceneConfiguration sceneConfiguration;

    public void AnimateGlow(Vector3 position)
    {
        sceneConfiguration
            .tutorialConfiguration.auraEffect.transform.position = position;
        
        sceneConfiguration
            .tutorialConfiguration.auraEffect.gameObject.SetActive(true);
    }
    
    public async Task AnimateCardAttackPosition(CardUI enemyCardUi, CardUI playerCardUi)
    {
        // enemyCardUi.animator.enabled = true;
        // enemyCardUi.animator.SetTrigger("Attack");
        // enemyCardUi.animator.enabled = false;

        await enemyCardUi.transform
            .DOLocalRotate(new Vector3(-40f, 0f, 0f), 0.05f)
            .AsyncWaitForCompletion();
            
        await enemyCardUi.transform
            .DOPunchPosition((playerCardUi.transform.localPosition)
                             - enemyCardUi.transform.localPosition
                , 0.5f, 1).AsyncWaitForCompletion();
        await enemyCardUi.transform
            .DOLocalRotate(new Vector3(0f, 0f, 0f), 0.05f)
            .AsyncWaitForCompletion();
            
        // enemyCardUi.animator.enabled = false;
    }
    
    public async Task AnimateDamageShake(CardUI playerCardUi, CardUI oppositeCard, Optional<Color> color)
    {
        var localRotation = playerCardUi.transform.localRotation;
        var tween =  playerCardUi.transform.DOShakeRotation(0.5f, 10f);
        playerCardUi.transform.localRotation = localRotation;
        var initColor = playerCardUi.cardFace.color;
        await DOTween.Sequence().Join(tween)
            .Join(playerCardUi.cardFace.DOColor(color?.Value ?? Color.red, 0.3f))
            .AsyncWaitForCompletion();
        await playerCardUi.cardFace.DOColor(initColor, 0.1f).AsyncWaitForCompletion();
    }
    
    public async Task AnimateChangeOfStat(TextMesh text, Color color, bool returnColor = false)
    {
        var initColor = text.color;
        
        var tw1 = DOTween.To(() =>
                text.color,
            x => text.color = x,
            color, 1f);
        var tw2 =
            text.transform.DOShakeScale(1f, 2f);

        var sequence = DOTween.Sequence()
            .Join(tw1)
            .Join(tw2);
        
        await sequence
            .AsyncWaitForCompletion();

        if (returnColor)
        {
            text.color = initColor;
        }
    }
    
    public async Task AnimateSkillUsed(CardUI cardUI, Sprite skillSprite)
    {
        GameObject go = new GameObject("skillUsed")
        {
            transform =
            {
                parent = cardUI.transform
            }
        };

        go.transform.position = new Vector3(0, 0, 0);
        go.transform.localPosition = new Vector3(0, 0, 0);
        go.transform.eulerAngles = new Vector3(90f, 0, 90f);
        go.transform.localScale = cardUI.skillUis[0].transform.localScale;
        
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = skillSprite;
        await go.transform.DOLocalMoveY(go.transform.localPosition.y + 0.7f, 0.7f)
            .OnComplete(() => go.SetActive(false)).AsyncWaitForCompletion();
    }

    public async Task AnimateCardIsDead(CardUI carenemyCardUi)
    {
        var initSacle = carenemyCardUi.transform.localScale;
        var initColor = carenemyCardUi.cardFace.color;
        var tw1 = carenemyCardUi.transform.DOScale(0.8f, 0.2f);
        var tw2 = carenemyCardUi.cardFace.DOColor(Color.grey, 0.2f);
        await DOTween.Sequence().Join(tw1).Join(tw2).AsyncWaitForCompletion();

        carenemyCardUi.transform.localScale = initSacle;
        carenemyCardUi.cardFace.color = initColor;
    }
}