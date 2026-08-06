using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity : MonoBehaviour
{
    public Enemy enemy;

    public bool isDynamic = false;
    public Vector3 lastPosition;
    private float lastTime;
    public float interval = 10f;
    public bool CanMovie = false;
    public Vector3 TargetPosition;
    public float speed = 3f;

    public Disease diseaseOnLose;
    public Disease diseaseOnWin;
    public List<Item> itemsOnWin;
    public List<int> itemsOnWinCount;

    public void StartAttack()
    {
        Enemy enemy = new Enemy(this.enemy);
        AttackManager.instance.StartAttack(enemy);
    }
    private void Update()
    {
        if (!isDynamic) return;
        if (CanMovie) Move();
        else StanOnPlace();
    }

    void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, TargetPosition, speed * Time.deltaTime);
        if (transform.position == TargetPosition)
        {
            CanMovie = false;
        }
    }
    // Бездействие и новая позиция для движения при бездействии
    void StanOnPlace()
    {
        lastTime += Time.deltaTime;
        if (lastTime > interval)
        {
            SetNewTargetPosition();
            CanMovie = true;
            lastTime = 0;
        }
    }
    void SetNewTargetPosition()
    {
        if (Random.Range(0, 100) < 5)
        {
            float x = lastPosition.x;
            float y = lastPosition.y;
            TargetPosition = new Vector3(x, y, 0);
        }
        else
        {
            float x = lastPosition.x + Random.Range(-20, 20);
            float y = lastPosition.y + Random.Range(-20, 20);
            TargetPosition = new Vector3(x, y, 0);
        }
        if (TargetPosition.x > transform.position.x)
        {
            gameObject.GetComponent<SpriteRenderer>().flipX = true;
        }
        else gameObject.GetComponent<SpriteRenderer>().flipX = false;
    }
}
