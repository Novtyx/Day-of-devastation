using UnityEngine;
using UnityEngine.UI;
public class Screenshot : MonoBehaviour
{
    public string screenshotName = "Screenshot";
    public int screenshotNumber = 0; // Для уникального имени файла
    public string fileExtension = ".png"; // Другие варианты: ".jpg"
    public Color pystosh;
    public Color Need;
    public Color final;

    [ContextMenu("Multiply")]
    public void MultiplyColors()
    {
        Color color = pystosh * Need;
        final = color;
    }

    public void TakeScreenshot()
    {
        string filename = screenshotName + screenshotNumber.ToString("D3") + fileExtension; // D3 - форматирует число в три цифры (001, 002, и т.д.)
        ScreenCapture.CaptureScreenshot(filename);
        Debug.Log("Screenshot saved as: " + filename);
        screenshotNumber++; // Увеличиваем номер для следующего скриншота
    }

    // Пример использования (вызов по нажатию клавиши):
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TakeScreenshot();
        }
    }
}
