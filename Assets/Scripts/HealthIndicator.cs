using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthIndicator : MonoBehaviour
{
    [SerializeField] public Image healthBar;
    [SerializeField] public Color[] colorStages = {Color.red, Color.yellow, Color.green};
    [SerializeField] private int entityIndex; 
    private float targetHealthBarValue;

    void Start()
    {
        StartCoroutine(UpdateHealthBar());
    }

    public void SetEntityIndex(int index)
    {
        entityIndex = index;
    }

    private IEnumerator UpdateHealthBar()
    {
        while (true)
        {
            if(healthBar == null)
            {
                yield break;
            }
            Entity entity = GameManager.Instance.ChannelData.GetChannelEntity(entityIndex);

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
