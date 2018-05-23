using UnityEngine;

[ExecuteInEditMode]
public class Colorizer : MonoBehaviour
{
    public Color TintColor;
    public bool UseInstanceWhenNotEditorMode = true;

    private Color m_OldColor;

    private void Update()
    {
        if (m_OldColor != TintColor) ChangeColor(gameObject, TintColor);
        m_OldColor = TintColor;
    }

    private void ChangeColor(GameObject effect, Color color)
    {
        var rend = effect.GetComponentsInChildren<Renderer>();
        foreach (var r in rend)
        {
#if UNITY_EDITOR
            var mat = r.sharedMaterial;
#else
			var mat = UseInstanceWhenNotEditorMode ? r.material : r.sharedMaterial;
			#endif

            if (mat == null || !mat.HasProperty("_TintColor")) continue;
            var oldColor = mat.GetColor("_TintColor");
            color.a = oldColor.a;
            mat.SetColor("_TintColor", color);
        }
        var childLight = effect.GetComponentInChildren<Light>();
        if (childLight != null) childLight.color = color;
    }
}
