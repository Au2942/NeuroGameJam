using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntegrityIndicator : MonoBehaviour
{
    [SerializeField] public Image integrityBar;
    [SerializeField] public Color[] integrityColorStages = {Color.red, Color.yellow, Color.green};
    [SerializeField] private int entityIndex; 
    private float targetIntegrityBarValue;

    void Start()
    {
        StartCoroutine(UpdateIntegrityBar());
    }

    public void SetEntityIndex(int index)
    {
        entityIndex = index;
    }

    private IEnumerator UpdateIntegrityBar()
    {
        while (true)
        {
            Entity entity = GameManager.Instance.ChannelData.GetChannelEntity(entityIndex);

            if(entity != null)
            {
                float integrity = entity.Integrity;
                float maxIntegrity = entity.MaxIntegrity;
                float percentage = integrity / maxIntegrity;

                int stage = Mathf.FloorToInt(percentage * (integrityColorStages.Length - 1));
                int nextStage = Mathf.Clamp(stage + 1, 0, integrityColorStages.Length - 1);
                float stagePercentage = (percentage * (integrityColorStages.Length - 1)) - stage;

                integrityBar.color = Color.Lerp(integrityColorStages[stage], integrityColorStages[nextStage], stagePercentage);
                targetIntegrityBarValue = percentage;
            }
            else 
            {
                targetIntegrityBarValue = 1;
            }

            integrityBar.fillAmount = Mathf.MoveTowards(integrityBar.fillAmount, targetIntegrityBarValue, Time.deltaTime);
            yield return null;
        }
    }


}
