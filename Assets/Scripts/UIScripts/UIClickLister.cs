using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickLister : MonoBehaviour
{
    public bool active;

    private void Update()
    {
        //List all the UI objects below mouse click that are set to receive raycasts (.raycastTarget == true)
        if (active && Input.GetMouseButtonDown(0)) {

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();

            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0) {
                string objectsClicked = "";
                foreach (RaycastResult result in results) {
                    objectsClicked += result.gameObject.name;

                    //If not the last element, add a comma
                    if (result.gameObject != results[^1].gameObject) {
                        objectsClicked += ", ";
                    }
                }
                Debug.Log("Clicked on: " + objectsClicked);
            }
        }
    }
}