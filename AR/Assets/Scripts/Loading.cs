using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    private int tipsOrder;
    public Text tipsText;

    private void Start()
    {
        //Starts a routine that changes every 5 seconds the tip on the loading screen until the app is loaded
        StartCoroutine(SwitchTips());
        RandomTip(Random.Range(0,3));
    }
    void RandomTip(int random)
    {
        // Edit tips in here.
        // Add tips in here aswell. Dont forget to change the random range to the maximum amount of cases(tips).
        // The random ranges are placed in void Start & IEnumerator SwitchTips. Only change the last number in the Random.Range
        // The last number needs to be the total number of cases(tips) there are.
        // It will look like this: Random.Range(0,3); the first number is the minimum. of tips. This number you never have to change.
        // The second number is the max. This one you need to edit to the amount of tips there are.
        switch(random)
        {
            case 0:
                tipsText.text = "Tip:" + "After scaling you can modify the model.";
                break;
            case 1:
                tipsText.text = "Tip:" + "If you want to replace the model. Click on the bin and you will be able to replace it.";
                break;
            case 2:
                tipsText.text = "Reminder:" + "This is a demo. Only 2 car models are usable.";
                break;
        }
    }
    IEnumerator SwitchTips()
    {
        // Changes the tip shown on the loading screen every 5 seconds
        yield return new WaitForSeconds(5);
        tipsOrder = Random.Range(0, 3);
        RandomTip(tipsOrder);
        Debug.Log(tipsOrder);
        StartCoroutine(SwitchTips());
    }
}
