using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FoxType
{
    Fox,
    TreasureChest,
    Racoon
}
public class Whackable : MonoBehaviour
{
    Animator animator;
    AudioSource soundFX;
    public bool isActive, isOpened;
    public int whackPoint, missPoint;
    public GameObject hitPrefab;
    public int requiredHits;
    int hits;
    public FoxType foxType;
    public AudioClip whackSfx, missSfx;
    BoxCollider2D boxCollider2D;
    public float revealTime;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        soundFX = GetComponent<AudioSource>();
        whackPoint = GameplayManager.Instance.whackValue[foxType];
        missPoint = GameplayManager.Instance.missValue[foxType];

        isActive = true;
    }

    public void AnimHideTrigger()
    {
        Debug.Log("HIDE !!!!!!!!");
        Invoke("Hide", revealTime);
    }

    public void Opened()
    {
        isOpened = true;
        Debug.Log("OPENED!!!");
        animator.SetTrigger("open");
    }

    void Hide()
    {
        if (isActive)
        {
            if (foxType != FoxType.TreasureChest) animator.SetTrigger("hide");
            else animator.SetTrigger("_hide");
            isActive = false;
        }
    }

    public void HitSfx() => soundFX.PlayOneShot(whackSfx);
    public void MissSfx() => soundFX.PlayOneShot(missSfx);

    public void Whack()
    {
        if (!boxCollider2D.enabled || !isActive)
            return;

        if (foxType != FoxType.Racoon)
        {
            if (foxType == FoxType.TreasureChest && !isOpened)
            {
                animator.SetTrigger("hide");
                return;
            }

            if (hits != requiredHits) hits++;
            if (hits == requiredHits)
            {
                Instantiate(hitPrefab, transform.position, Quaternion.identity);
                animator.SetTrigger("hit");
                isActive = false;
            }
        }
        else animator.SetTrigger("explode");

        Debug.Log("Hit Whackable!!!");
    }

    public void GetPoint()
    {
        //Get the point
        if (GameplayManager.Instance.playerPoint >= 0)
            GameplayManager.Instance.playerPoint += whackPoint;
        Debug.Log("Get Point");

        isActive = false;
        Destroy(gameObject);
    }

    public void Missed()
    {
        Debug.Log("Lost Point");
        //SubtractTime and missedPoint;
        if (GameplayManager.Instance.playerPoint >= 0)
        {
            if (foxType != FoxType.TreasureChest)
            {
                GameplayManager.Instance.playerPoint -= missPoint;
                isActive = false;
            }
        }
        Destroy(gameObject);
    }

    public void MissedHit()
    {
        Destroy(gameObject);
    }
}