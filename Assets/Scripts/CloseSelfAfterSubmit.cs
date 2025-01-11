using UnityEngine;

public class CloseSelfAfterSubmit : MonoBehaviour
{
    void Update()
    {
        if (InputManager.Instance.Submit.triggered || InputManager.Instance.Click.triggered)
        {
            gameObject.SetActive(false);
        }
    }
}
