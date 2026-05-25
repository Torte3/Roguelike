using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public partial class FieldBluePrintEditor
    {
        private void DrawGridArea()
        {
            // グリッドエリア内で描画
            DrawGrid();
            DrawArea();
            DrawConnections();
            DrawSections();
        }

        private void DrawGrid()
        {
            float scaledGrid = gridSize * drawScale;
            float startX = (-viewOffset.x % scaledGrid + scaledGrid) % scaledGrid;
            float startY = (-viewOffset.y % scaledGrid + scaledGrid) % scaledGrid;

            // グリッドの太さを倍数に応じて調整
            float baseGridThickness = CalculateGridThickness(scaledGrid);
            float maxGridThickness = 3f;

            Handles.color = new Color(1, 1, 1, 0.1f);

            for (float x = startX; x < position.width; x += scaledGrid)
            {
                // 実際の座標での位置を計算（0,0を起点）
                int actualX = Mathf.RoundToInt((x + viewOffset.x) / drawScale);
                float gridThickness;
                // 100の倍数グリッドは最も太く
                if (actualX % 100 == 0)
                {
                    gridThickness = baseGridThickness * 100;
                }
                // 10の倍数グリッドは少し太く
                else if (actualX % 10 == 0)
                {
                    gridThickness = baseGridThickness * 10;
                }
                else
                {
                    gridThickness = baseGridThickness;
                }
                gridThickness = Mathf.RoundToInt(Mathf.Min(gridThickness, maxGridThickness));
                Handles.DrawAAPolyLine(gridThickness, new Vector2(x, 0), new Vector2(x, position.height));
            }

            for (float y = startY; y < position.height; y += scaledGrid)
            {
                // 実際の座標での位置を計算（0,0を起点）
                int actualY = Mathf.RoundToInt((y + viewOffset.y) / drawScale);
                float gridThickness;
                // 100の倍数グリッドは最も太く
                if (actualY % 100 == 0)
                {
                    gridThickness = baseGridThickness * 100;
                }
                // 10の倍数グリッドは少し太く
                else if (actualY % 10 == 0)
                {
                    gridThickness = baseGridThickness * 10;
                }
                else
                {
                    gridThickness = baseGridThickness;
                }
                gridThickness = Mathf.RoundToInt(Mathf.Min(gridThickness, maxGridThickness));
                Handles.DrawAAPolyLine(gridThickness, new Vector2(0, y), new Vector2(position.width, y));
            }
        }

        private float CalculateGridThickness(float scaledGrid)
        {
            float gridDensity = scaledGrid;

            // 密度に基づいて太さを計算（密度が低いほど細く、高すぎる場合は非表示）
            float thickness = gridDensity / 10f;

            return thickness;
        }

        private Color GetSectionColor(RoomGenerationType roomGenerationType, bool isOutsideArea, bool isSelected)
        {
            if (isOutsideArea)
            {
                return new Color(1f, 0.2f, 0.2f, 0.5f); // 赤色（エリア外）
            }

            float alpha = isSelected ? 0.7f : 0.4f; // 選択時は明るく、非選択時は薄く

            switch (roomGenerationType)
            {
                case RoomGenerationType.Always:
                    return new Color(0.2f, 1f, 0.2f, alpha); // 緑色（必ず作る）
                case RoomGenerationType.Random:
                    return new Color(0.2f, 0.6f, 1f, alpha); // 青色（ランダム）
                case RoomGenerationType.Never:
                    return new Color(0.5f, 0.5f, 0.5f, alpha); // グレー（作らない）
                default:
                    throw new Exception("Invalid RoomGenerationType");
            }
        }

        private void DrawArea()
        {
            Rect drawAreaRect = new Rect(
                -viewOffset.x,
                -viewOffset.y,
                areaSize.x * drawScale,
                areaSize.y * drawScale
            );

            // エリアの背景
            EditorGUI.DrawRect(drawAreaRect, new Color(0.8f, 0.8f, 0.8f, 0.2f));

            // エリアの境界線
            Handles.color = Color.gray;
            Handles.DrawWireCube(drawAreaRect.center, drawAreaRect.size);

            // エリアのサイズテキストを表示
            string areaSizeText = $"Area: {areaSize.x} x {areaSize.y}";
            Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(areaSizeText));
            Rect textRect = new Rect(drawAreaRect.center.x - textSize.x / 2, drawAreaRect.center.y - textSize.y / 2, textSize.x, textSize.y);

            // テキスト色を設定
            Color originalColor = GUI.color;
            GUI.color = Color.black;
            GUI.Label(textRect, areaSizeText);
            GUI.color = originalColor;

            // エリアのリサイズハンドル
            var handleRect = new Rect(drawAreaRect.xMax - handleSize, drawAreaRect.yMax - handleSize, handleSize, handleSize);
            EditorGUI.DrawRect(handleRect, Color.white);
        }

        private void DrawConnections()
        {
            Handles.color = Color.green;

            foreach (var connection in connections)
            {
                if (connection.fromSection != null && connection.toSection != null)
                {
                    // Sectionの現在の中心座標を使用
                    Vector2 fromPos = connection.fromSection.rect.center * drawScale - viewOffset;
                    Vector2 toPos = connection.toSection.rect.center * drawScale - viewOffset;

                    // 太い線で描画
                    Handles.DrawAAPolyLine(5, fromPos, toPos);
                }
            }

            // コネクション作成中の線を描画
            if (creatingConnection && connectionStartSection != null)
            {
                Vector2 startPos = connectionStartSection.rect.center * drawScale - viewOffset;

                // マウス位置を正確に取得
                Vector2 mousePos = Event.current.mousePosition;

                // グリッドエリア内でのみ描画
                Rect gridRect = new Rect(0, 0, position.width - 300, position.height);
                if (mousePos.x >= 0 && mousePos.x < position.width - 300 && mousePos.y >= 0 && mousePos.y < position.height)
                {
                    Handles.color = Color.yellow;
                    // 太い線で描画
                    Handles.DrawAAPolyLine(5, startPos, mousePos);
                }
            }
        }

        private void DrawSections()
        {
            foreach (var section in sections)
            {
                Rect drawRect = new Rect(
                    section.rect.x * drawScale - viewOffset.x,
                    section.rect.y * drawScale - viewOffset.y,
                    section.rect.width * drawScale,
                    section.rect.height * drawScale
                );

                // エリア外のルームは赤色で表示
                bool isOutsideArea = !IsSectionInsideArea(section.rect);
                bool isSelected = section == selectedSection;
                Color sectionColor = GetSectionColor(section.roomGenerationType, isOutsideArea, isSelected);

                EditorGUI.DrawRect(drawRect, sectionColor);

                // 枠線を描画
                Handles.color = Color.black;
                Handles.DrawWireCube(drawRect.center, drawRect.size);

                // サイズテキストを表示
                string sizeText = $"{section.rect.width} x {section.rect.height}";
                Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(sizeText));
                Rect textRect = new Rect(drawRect.center.x - textSize.x / 2, drawRect.center.y - textSize.y / 2, textSize.x, textSize.y);

                // テキスト色を黒に設定
                Color originalColor = GUI.color;
                GUI.color = Color.black;
                GUI.Label(textRect, sizeText);
                GUI.color = originalColor;
            }
        }
    }
}
