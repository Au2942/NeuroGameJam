using UnityEngine;
using UnityEngine.UI;

public class HealthIndicator : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Material healthBarMaterial;
    [SerializeField] private int frequency = 20;
    [SerializeField] private Color[] colors = {Color.green, Color.blue, Color.red};
    private Entity entity;
    private Material tempMaterial;

    void Start()
    {
        tempMaterial = new Material(healthBarMaterial);
        healthBar.material = tempMaterial;
        healthBar.materialForRendering.SetFloat("_Seed", Time.time);
        healthBar.materialForRendering.SetFloat("_Frequency", frequency);
        healthBar.materialForRendering.SetColor("_Color1", colors[0]);
        healthBar.materialForRendering.SetColor("_Color2", colors[2]);
        healthBar.materialForRendering.SetFloat("_Fill", 0);
    }

    public void SetEntity(Entity newEntity)
    {
        entity = newEntity;
    }

    public void SetSeed(float seed)
    {
        healthBar.materialForRendering.SetFloat("_Seed", seed);
        //maybe set new seed when entity exit glitched state
    }

    void Update()
    {
        if(entity != null)
        {
            if(!entity.Glitched)
            {
                healthBar.materialForRendering.SetColor("_Color1", colors[0]);
                healthBar.materialForRendering.SetColor("_Color2", colors[2]);
                float percentage = entity.HealthPercentage();
                healthBar.materialForRendering.SetFloat("_Fill", 1-percentage);
            }
            else
            {
                healthBar.materialForRendering.SetColor("_Color1", colors[1]);
                healthBar.materialForRendering.SetColor("_Color2", colors[2]);
                healthBar.materialForRendering.SetFloat("_Fill", entity.Corruption/entity.MaxHealth); //corruption goes from maxhealth - health to 0
            }
            
        }
    }

    void OnDestroy()
    {
        Destroy(tempMaterial);
    }


}
