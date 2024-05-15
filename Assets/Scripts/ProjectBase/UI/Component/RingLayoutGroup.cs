using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/Ring Layout Group", 150)]
public class RingLayoutGroup : LayoutGroup
{
    // �����������Զ�������Ժ��ֶΣ���뾶����ʼ�Ƕȵ�  
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

        float angleStep = 360f / rectChildren.Count; // ����ÿ����Ԫ��֮��ĽǶȲ�  
        float currentAngle = startAngle;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            Vector2 childSize = child.sizeDelta;
            Vector2 childPosition;

            // ������Ԫ���ڻ��β����е�λ��  
            float x = Mathf.Cos(Mathf.Deg2Rad * currentAngle) * (radius - childSize.x / 2f);
            float y = Mathf.Sin(Mathf.Deg2Rad * currentAngle) * (radius - childSize.y / 2f);
            childPosition = new Vector2(x, y);

            // Ӧ��λ�ò�����ê�������㣨�����Ҫ��  
            child.anchoredPosition = childPosition;
            child.pivot = new Vector2(0.5f, 0.5f); // ���������Ϊ����  

            // ���µ�ǰ�Ƕ���׼����һ����Ԫ��  
            currentAngle += angleStep;

            // ��Ӽ�ࣨ�����Ҫ��  
            currentAngle += spacing / radius * Mathf.Rad2Deg;
        }
    }
}