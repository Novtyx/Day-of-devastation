using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public static class Dotweenext
{
    public static Tween DOIntensity(this Light2D light, float endValue, float duration)
        => DOTween.To(() => light.intensity, t => light.intensity = t, endValue, duration);
    public static Tween DOColor(this Light2D light, Color endValue, float duration)
    => DOTween.To(() => light.color, t => light.color = t, endValue, duration);
}
