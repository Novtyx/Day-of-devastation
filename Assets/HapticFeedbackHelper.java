package com.yourcompany.yourapp;

import android.view.View;
import android.content.Context;
import android.os.Vibrator;
import android.os.VibrationEffect;
import android.os.Build;

public class HapticFeedbackHelper {
    private final Context context;
    private final View view; // Нужен для performHapticFeedback()
	private Vibrator vibrator;

    public HapticFeedbackHelper(Context context, View view) {
        this.context = context;
        this.view = view;
		vibrator = (Vibrator) context.getSystemService(Context.VIBRATOR_SERVICE);
    }

    // Вызов стандартного тактильного отклика (не вибрации!)
    public void performHapticFeedback(int feedbackConstant) {
        if (view != null) {
            view.performHapticFeedback(feedbackConstant);
        }
    }
	
	// Метод для кастомных сценариев
	public void playCustomPattern(long[] timings, int[] amplitudes) {
		if (vibrator != null && vibrator.hasVibrator()) {
			if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
				// API 26+: Поддержка таймингов и точного контроля амплитуды (от 1 до 255)
				// -1 означает, что паттерн не будет зацикливаться
				VibrationEffect effect = VibrationEffect.createWaveform(timings, amplitudes, -1);
				vibrator.vibrate(effect);
			}
		}
	}
	
	// Метод для сборки паттерна из аппаратных кликов (только для Android 11+)
	public void playPrimitivePattern(int[] primitives, float[] scales, int[] delays) {
		if (vibrator != null && vibrator.hasVibrator()) {
			if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
				VibrationEffect.Composition composition = VibrationEffect.startComposition();
				// Собираем композицию из переданных массивов
				for (int i = 0; i < primitives.length; i++) {
					composition.addPrimitive(primitives[i], scales[i], delays[i]);
				}
				vibrator.vibrate(composition.compose());
			}
		}
	}

    // Пример констант (можно добавить больше)
    public static int getConstantClick() {
        return android.view.HapticFeedbackConstants.CONTEXT_CLICK;
    }

    public static int getConstantLongPress() {
        return android.view.HapticFeedbackConstants.LONG_PRESS;
    }

    public static int getConstantVirtualKey() {
        return android.view.HapticFeedbackConstants.VIRTUAL_KEY;
    }
}