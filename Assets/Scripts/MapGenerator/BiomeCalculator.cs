using Unity.Mathematics;
using Unity.Burst;
public struct BiomeSettings
{
    public float mountainThreshold;
    public float4 forestColor, forestColorSecond, radForestColor;
    public float4 pystoshColor, radPystoshColor, bolotoColor, radBolotoColor;
    public float4 mountainsColor, snowMountainsColor, snowyColor;
}
public struct BiomeCalculator
{
    public static void CalculateBiome(
    float height, float temperature, float moisture, float radiation,
    int worldX, int worldY,
    in BiomeSettings settings,
    out int tileType, out float4 color, out bool spawnTree, out int MaterialType, out float dyy, out float tresh)
    {
        float dx = (float)worldX / 8192f;
        float dy = (float)worldY / 4096f;
        dyy = dy;
        // Математика нисходящей дуги для снежной шапки
        float arcCurve = math.cos((dx + dx) * math.PI);
        float treshold = 0.75f + (arcCurve * 0.1f);
        tresh = treshold;
        if (height > settings.mountainThreshold)
        {
            float f = math.unlerp(0.8f, 1f, height);
            float4 gr = math.lerp(settings.mountainsColor, settings.snowMountainsColor, f);
            if (dy < treshold) { tileType = 7; color = gr; spawnTree = true; MaterialType = 1; return; }
            else { tileType = 7; color = math.lerp(gr, settings.snowyColor, 0.9f); spawnTree = true; MaterialType = 1; return; }
        }
        if (height > 0.7f)
        {
            float f = math.unlerp(0.7f, 0.8f, height);
            float4 gr = math.lerp(settings.pystoshColor, settings.mountainsColor, f);
            if (moisture > 0.6f && moisture < 0.8f)
            {
                gr = math.lerp(settings.forestColor, settings.mountainsColor, f);
            }
            if (dy < treshold) { tileType = 6; color = gr; spawnTree = true; MaterialType = 1; return; }
            else { tileType = 7; color = math.lerp(gr, settings.snowyColor, 0.9f); spawnTree = true; MaterialType = 1; return; }
        }
        if (moisture < 0.2f && radiation > 0.7f)
        {
            if (dy < treshold) { tileType = 3; color = settings.radPystoshColor; spawnTree = false; MaterialType = 1; return; }
            else { tileType = 17; color = math.lerp(settings.radPystoshColor, settings.snowyColor, 0.9f); spawnTree = false; MaterialType = 1; return; }
        }
        if (moisture > 0.8f && temperature > 0.4f && radiation > 0.7f)
        {
            if (dy < treshold) { tileType = 5; color = settings.radBolotoColor; spawnTree = false; MaterialType = 1; return; }
            else { tileType = 19; color = math.lerp(settings.radBolotoColor, settings.snowyColor, 0.9f); spawnTree = false; MaterialType = 1; return; }
        }
        if (moisture > 0.6f && moisture < 0.8f && radiation > 0.7f)
        {
            float f = math.unlerp(0.1f, 1f, radiation);
            float4 gr = settings.radForestColor;
            if (radiation < 0.6f) gr = math.lerp(settings.radForestColor, settings.forestColor, f);
            if (dy < treshold) { tileType = 1; color = gr; spawnTree = true; MaterialType = 0; return; }
            else { tileType = 15; color = math.lerp(gr, settings.snowyColor, 0.9f); spawnTree = true; MaterialType = 0; return; }
        }
        if (moisture > 0.8f && temperature > 0.4f)
        {
            float f = math.unlerp(0.4f, 0.5f, temperature);
            if (f < 0.7f)
            {
                float subF = math.unlerp(0f, 0.7f, f);
                float4 gr = math.lerp(settings.pystoshColor, settings.bolotoColor, subF);
                if (dy < treshold) { tileType = 4; color = gr; spawnTree = false; MaterialType = 2; return; }
                else { tileType = 18; color = math.lerp(gr, settings.snowyColor, 0.9f); spawnTree = false; MaterialType = 1; return; }
            }
            if (dy < treshold) { tileType = 4; color = settings.bolotoColor; spawnTree = false; MaterialType = 2; return; }
            else { tileType = 18; color = math.lerp(settings.bolotoColor, settings.snowyColor, 0.9f); spawnTree = false; MaterialType = 1; return; }
        }
        if (moisture > 0.6f && moisture < 0.8f)
        {
            float f = math.unlerp(0.6f, 0.8f, moisture);
            float4 gr;
            if (f > 0.7f)
            {
                float subF = math.unlerp(0.7f, 1f, f);
                gr = math.lerp(settings.forestColor, settings.forestColorSecond, subF);
            }
            else
            {
                float subF = math.unlerp(0f, 0.7f, f);
                gr = math.lerp(settings.forestColor, settings.forestColor, subF);
            }
            if (height > 0.63f) gr = math.lerp(settings.mountainsColor, settings.forestColor, f);
            if (dy < treshold) { tileType = 0; color = gr; spawnTree = true; MaterialType = 0; return; }
            else { tileType = 14; color = math.lerp(gr, settings.snowyColor, 0.9f); spawnTree = true; MaterialType = 0; return; }
        }
        if (moisture > 0.1f && moisture < 0.3f && radiation > 0.7f)
        {
            float f = math.unlerp(0.1f, 1f, radiation);
            float4 gr = (f > 0.7f)
            ? math.lerp(settings.pystoshColor, settings.radPystoshColor, math.unlerp(0.7f, 1f, f))
            : math.lerp(settings.pystoshColor, settings.pystoshColor, math.unlerp(0f, 0.7f, f));
            if (dy < treshold) { tileType = 2; color = gr; spawnTree = false; MaterialType = 2; return; }
            else { tileType = 16; color = math.lerp(gr, settings.snowyColor, 0.9f); spawnTree = false; MaterialType = 1; return; }
        }
        if (moisture < 0.7f && radiation > 0.6f)
        {
            float f = math.unlerp(0f, 0.7f, moisture);
            float4 gr = (f > 0.7f)
            ? math.lerp(settings.pystoshColor, settings.radForestColor, math.unlerp(0.7f, 1f, f))
            : math.lerp(settings.pystoshColor, settings.pystoshColor, math.unlerp(0f, 0.7f, f));
            if (dy < treshold) { tileType = 2; color = gr; spawnTree = false; MaterialType = 2; return; }
            else { tileType = 16; color = math.lerp(gr, settings.snowyColor, 0.9f); spawnTree = false; MaterialType = 1; return; }
        }
        if (moisture < 0.6f && moisture > 0.3f && temperature > 0.7f)
        {
            if (dy < treshold) { tileType = 11; color = settings.pystoshColor; spawnTree = false; MaterialType = 1; return; }
            else { tileType = 21; color = math.lerp(settings.pystoshColor, settings.snowyColor, 0.9f); spawnTree = false; MaterialType = 1; return; }
        }
        if (moisture < 0.6f)
        {
            float f = math.unlerp(0f, 0.6f, moisture);
            float4 gr = (f > 0.9f)
            ? math.lerp(settings.pystoshColor, settings.forestColor, math.unlerp(0.9f, 1f, f))
            : math.lerp(settings.pystoshColor, settings.pystoshColor, math.unlerp(0f, 0.9f, f));
            if (dy < treshold) { tileType = 2; color = gr; spawnTree = false; MaterialType = 2; return; }
            else { tileType = 16; color = math.lerp(gr, settings.snowyColor, 0.9f); spawnTree = false; MaterialType = 2; return; }
        }
        if (dy < treshold) { tileType = 2; color = settings.pystoshColor; spawnTree = false; MaterialType = 2; return; }
        else { tileType = 16; color = math.lerp(settings.pystoshColor, settings.snowyColor, 0.9f); spawnTree = false; MaterialType = 2; return; }
    }
}