using UnityEngine;

public class EnemyHeadshot : MonoBehaviour
{
    private Enemy parentEnemy;
    
    void Start()
    {
        parentEnemy = GetComponentInParent<Enemy>();
    }
    
    public void TakeDamage(int damage)
    {
        if (parentEnemy != null)
        {
            parentEnemy.TakeDamage(damage, true);
        }
    }
}
