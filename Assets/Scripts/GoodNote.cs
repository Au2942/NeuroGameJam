using UnityEngine;

public class GoodNote : RainObject
{
    [SerializeField] private float moveSpeed = 300f;
    Vector2 moveDirection;

    public override void ReturnToPool()
    {
        base.ReturnToPool();
        PlayerManager.Instance.TakeDamage(-1);
    }
    public override void OnSpawn()
    {
        base.OnSpawn();
        moveDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    void Update()
    {
        transform.localPosition += (Vector3)(moveDirection * Time.deltaTime * moveSpeed);
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
