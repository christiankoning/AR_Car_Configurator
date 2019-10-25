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
        StartCoroutine(SwitchTips());
        RandomTip(Random.Range(0,5));
    }

    void RandomTip(int random)
    {
        switch(random)
        {
            case 0:
                tipsText.text = "Tip:" + "After scaling you can modify the model.";
                break;
            case 1:
                tipsText.text = "Tip:" + "If you want to replace the model. Click on the bin and you will be able to replace it.";
                break;
            case 2:
                tipsText.text = "Tip:" + "This is a demo. Only 2 car models are usable.";
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
