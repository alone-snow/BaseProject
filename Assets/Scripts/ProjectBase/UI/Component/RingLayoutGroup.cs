using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/Ring Layout Group", 150)]
public class RingLayoutGroup : LayoutGroup
{
    // 这里可以添加自定义的属性和字段，如半径、起始角度等  
    public float radius = 100f;
    public float startAngle = 0f;
    public float spacing = 10f;

    public override void CalculateLayoutInputVertical()
    {
        
    }

    public override void SetLayoutHorizontal()
    {
        SetChildrenRects();
    }

    public override void SetLayoutVertical()
    {
        
    }

    protected void SetChildrenRects()
    {
        if (!IsActive() || rectTransform.rect.width <= 0 || rectTransform.rect.height <= 0)
            return;

        float angleStep = 360f / rectChildren.Count; // 计算每个子元素之间的角度差  
        float currentAngle = startAngle;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            Vector2 childSize = child.sizeDelta;
            Vector2 childPosition;

            // 计算子元素在环形布局中的位置  
            float x = Mathf.Cos(Mathf.Deg2Rad * currentAngle) * (radius - childSize.x / 2f);
            float y = Mathf.Sin(Mathf.Deg2Rad * currentAngle) * (radius - childSize.y / 2f);
            childPosition = new Vector2(x, y);

            // 应用位置并设置锚点和枢轴点（如果需要）  
            child.anchoredPosition = childPosition;
            child.pivot = new Vector2(0.5f, 0.5f); // 设置枢轴点为中心  

            // 更新当前角度以准备下一个子元素  
            currentAngle += angleStep;

            // 添加间距（如果需要）  
            currentAngle += spacing / radius * Mathf.Rad2Deg;
        }
    }
}