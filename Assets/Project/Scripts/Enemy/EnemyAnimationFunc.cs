using UnityEngine;

public class EnemyAnimationFunc : MonoBehaviour
{
    public void FinishCatchAnimation()
    {
        EnemyAI enemyAI = transform.parent.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.ResetEnemy();
        }
    }
}
