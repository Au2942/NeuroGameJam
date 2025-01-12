using UnityEngine;

public class GoodNote : Interactable
{
    [SerializeField] private float moveSpeed = 300f;
    [SerializeField] private float lifeTime = 15f;
    Vector2 moveDirection;

    public override void ReturnToPool()
    {
        base.ReturnToPool();
        PlayerManager.Instance.TakeDamage(-3);
    }
    public override void OnSpawn()
    {
        base.OnSpawn();
        moveDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    void Update()
    {
        transform.localPosition += (Vector3)(moveDirection * Time.deltaTime * moveSpeed);
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            InteractableSpawner.ReturnObjectToPool(gameObject);
        }
        ReachEdgeCheck();
    }

    private void ReachEdgeCheck()
    {
        if (transform.localPosition.x > 1536 / 2 || transform.localPosition.x < -1536 / 2)
        {
            moveDirection.x = -moveDirection.x;
        }
        if (transform.localPosition.y > 864 / 2 || transform.localPosition.y < -864 / 2)
        {
            moveDirection.y = -moveDirection.y;
        }
    }

}
