using UnityEngine;

public class UI : MonoBehaviour
{
    private bool isVisible;
    public void Toggle()
    {
        if (isVisible)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        isVisible = true;
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        isVisible = false;
    }
}
