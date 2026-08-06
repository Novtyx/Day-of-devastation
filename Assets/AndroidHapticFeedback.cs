using UnityEngine;

public static class AndroidHapticFeedback
{
    private static AndroidJavaObject _hapticHelper;
    private static bool _isInitialized = false;
    private static bool _isHapticSupported = false;

    // Стандартные константы из Android API
    public const int CONTEXT_CLICK = 0x00000001;      // HapticFeedbackConstants.CONTEXT_CLICK
    public const int LONG_PRESS = 0x00000000;         // HapticFeedbackConstants.LONG_PRESS
    public const int VIRTUAL_KEY = 0x00000008;        // HapticFeedbackConstants.VIRTUAL_KEY
    public const int CONFIRM = 0x00000010;            // HapticFeedbackConstants.CONFIRM (API 30+)

    // Инициализация
    public static void Initialize()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject view = currentActivity.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");

            _hapticHelper = new AndroidJavaObject(
                "com.yourcompany.yourapp.HapticFeedbackHelper",
                currentActivity,
                view
            );

            // Проверяем, поддерживает ли устройство HapticFeedbackConstants.CONFIRM (API 30+)
            AndroidJavaClass constants = new AndroidJavaClass("android.view.HapticFeedbackConstants");
            _isHapticSupported = constants != null;
            _isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Haptic Feedback не поддерживается: " + e.Message);
            _isHapticSupported = false;
            _isInitialized = true;
        }
#endif
    }

    // Универсальный метод для вибрации
    public static void TriggerHapticFeedback(int feedbackType)
    {
        if (!_isInitialized) Initialize();

#if UNITY_ANDROID && !UNITY_EDITOR
        if (_isHapticSupported)
        {
            try
            {
                _hapticHelper?.Call("performHapticFeedback", feedbackType);
            }
            catch
            {
                FallbackVibrate(feedbackType);
            }
        }
        else
        {
            FallbackVibrate(feedbackType);
        }
#else
        FallbackVibrate(feedbackType);
#endif
    }

    public static void PlayCustomPattern(long[] timings, int[] amplitudes)
    {
        if (_hapticHelper != null)
        {
            _hapticHelper.Call("playCustomPattern", timings, amplitudes);
        }
    }

    public static void PlayPrimitivePattern(int[] primitives, float[] scales, int[] delays)
    {
        if (_hapticHelper != null)
        {
            _hapticHelper.Call("playPrimitivePattern", primitives, scales, delays);
        }
    }

    // Fallback на стандартную вибрацию Unity
    private static void FallbackVibrate(int feedbackType)
    {
        switch (feedbackType)
        {
            case CONTEXT_CLICK:
                Vibrate(50); // 50ms
                break;
            case LONG_PRESS:
                Vibrate(200); // 200ms
                break;
            case CONFIRM:
                Vibrate(100); // 100ms (аналог CONFIRM)
                break;
            default:
                Vibrate(50);
                break;
        }
    }

    // Вибрация через Android API
    public static void Vibrate(long milliseconds, int amplitude = -1)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaObject vibrator = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
            .GetStatic<AndroidJavaObject>("currentActivity")
            .Call<AndroidJavaObject>("getSystemService", "vibrator"))
        {
            if (vibrator == null) return;

            // Для Android 8.0+ (API 26+) используем VibrationEffect
            AndroidJavaClass vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
            if (vibrationEffect != null && amplitude > 0)
            {
                AndroidJavaObject effect = vibrationEffect.CallStatic<AndroidJavaObject>(
                    "createOneShot",
                    milliseconds,
                    Mathf.Clamp(amplitude, 1, 255)
                );
                vibrator.Call("vibrate", effect);
            }
            else
            {
                vibrator.Call("vibrate", milliseconds);
            }
        }
#endif
    }
    // Геттеры для удобства
    public static int GetConstantClick() => CONTEXT_CLICK;
    public static int GetConstantLongPress() => LONG_PRESS;
    public static int GetConstantConfirm() => CONFIRM;
    public static int GetConstantVirtualKey() => VIRTUAL_KEY;

    public const int PRIMITIVE_CLICK = 1; // Четкий клик
    public const int PRIMITIVE_THUD = 2; // Глухой тяжелый удар
    public const int PRIMITIVE_SPIN = 3; // Вращение
    public const int PRIMITIVE_QUICK_RISE = 4; // Быстрое нарастание
    public const int PRIMITIVE_SLOW_RISE = 5; // Медленное нарастание
    public const int PRIMITIVE_TICK = 7; // Очень легкий, короткий тик
    public const int PRIMITIVE_LOW_TICK = 8; // Низкочастотный тик
}