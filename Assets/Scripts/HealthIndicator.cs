using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthIndicator : MonoBehaviour
{
    [SerializeField] public Image healthBar;
    [SerializeField] public Color[] colorStages = {Color.red, Color.yellow, Color.green};
    private Entity entity;
    private float targetHealthBarValue;

    void Start()
    {
        StartCoroutine(UpdateHealthBar());
    }

    public void SetEntity(Entity newEntity)
    {
        entity = newEntity;
    }

    private IEnumerator UpdateHealthBar()
    {
        while (true)
        {
            if(healthBar == null)
            {
                yield break;
            }
            if(entity != null)
            {
                float health = entity.Health;
                float maxHealth = entity.MaxHealth;
                float percentage = health / maxHealth;

                int stage = Mathf.FloorToInt(percentage * (colorStages.Length - 1));
                int nextStage = Mathf.Clamp(stage + 1, 0, colorStages.Length - 1);
                float stagePercentage = (percentage * (colorStages.Length - 1)) - stage;

                healthBar.color = Color.Lerp(colorStages[stage], colorStages[nextStage], stagePercentage);
                targetHealthBarValue = percentage;
            }
            else 
            {
                targetHealthBarValue = 1;
            }

            healthBar.fillAmount = Mathf.MoveTowards(healthBar.fillAmount, targetHealthBarValue, Time.deltaTime);
            yield return null;
        }
    }


}
