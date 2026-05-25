using UnityEditor;
using UnityEngine;

namespace Editor
{
    public partial class FieldBluePrintEditor
    {
        private void HandleInput()
        {
            var e = Event.current;

            // グリッドエリア内でのみマウス入力を処理
            Rect gridRect = new Rect(0, 0, position.width - 300, position.height);
            if (!gridRect.Contains(e.mousePosition) && e.type != EventType.ScrollWheel)
                return;

            // ズーム処理
            if (e.type == EventType.ScrollWheel)
            {
                float oldScale = drawScale;

                // スクロール方向を逆にして、乗算で倍率を変更
                float zoomFactor = 1.1f; // 10%の変化
                if (e.delta.y < 0)
                {
                    drawScale = drawScale * zoomFactor;
                }
                else if (e.delta.y > 0)
                {
                    drawScale = drawScale / zoomFactor;
                }

                drawScale = Mathf.Clamp(drawScale, 1f, 100f);

                // ズーム中心をマウス位置に設定
                Vector2 mousePos = e.mousePosition;
                Vector2 worldPos = (mousePos + (Vector2)viewOffset) / oldScale;
                viewOffset = Vector2Int.RoundToInt(worldPos * drawScale - mousePos);

                Repaint();
                e.Use();
                return;
            }

            // パン処理（中ボタンドラッグ）
            if (e.type == EventType.MouseDown && e.button == 2)
            {
                // 中ボタンでパン開始
                e.Use();
            }
            if (e.type == EventType.MouseDrag && e.button == 2)
            {
                // クリック位置を固定してパン
                viewOffset -= Vector2Int.RoundToInt(e.delta);
                Repaint();
                e.Use();
                return;
            }

            // スケール逆変換（viewOffsetを考慮）
            Vector2 mouse;

            mouse = (e.mousePosition + viewOffset) / drawScale;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                selectedSection = null;
                resizing = false;
                resizingArea = false;

                // コネクション作成中の場合は完了を試行
                if (creatingConnection && connectionStartSection != null)
                {
                    foreach (var section in sections)
                    {
                        if (section.rect.Contains(Vector2Int.RoundToInt(mouse)) && section != connectionStartSection)
                        {
                            // コネクション完成
                            CreateConnection(connectionStartSection, section);
                            creatingConnection = false;
                            connectionStartSection = null;
                            e.Use();
                            return;
                        }
                    }
                    // コネクション開始をキャンセル
                    creatingConnection = false;
                    connectionStartSection = null;
                    // ここではreturnしない - 通常のRoom生成処理に続く
                }

                // エリアのリサイズハンドルをチェック（描画座標に変換）
                Rect areaDrawRect = new Rect(
                    -viewOffset.x,
                    -viewOffset.y,
                    areaSize.x * drawScale,
                    areaSize.y * drawScale
                );

                Vector2Int areaHandleType = GetHandleType(e.mousePosition, areaDrawRect);
                if (areaHandleType == new Vector2Int(1, 1))
                {
                    resizingArea = true;
                    resizeHandleType = areaHandleType;
                    e.Use();
                    return;
                }

                foreach (var section in sections)
                {
                    // リサイズハンドルを描画座標に変換
                    Rect drawRect = new Rect(
                        section.rect.x * drawScale - viewOffset.x,
                        section.rect.y * drawScale - viewOffset.y,
                        section.rect.width * drawScale,
                        section.rect.height * drawScale
                    );

                    Vector2Int sectionHandleType = GetHandleType(e.mousePosition, drawRect);
                    if (sectionHandleType != Vector2Int.zero)
                    {
                        selectedSection = section;
                        resizing = true;
                        resizeHandleType = sectionHandleType;
                        e.Use();
                        return;
                    }

                    if (section.rect.Contains(Vector2Int.RoundToInt(mouse)))
                    {
                        selectedSection = section;
                        dragOffset = mouse - section.rect.position;
                        e.Use();
                        return;
                    }
                }

                e.Use();
            }

            // 右クリックでコンテキストメニュー表示
            if (e.type == EventType.MouseDown && e.button == 1)
            {
                // コネクション削除をチェック
                foreach (var connection in connections)
                {
                    if (connection.fromSection != null && connection.toSection != null)
                    {
                        Vector2 fromPos = new Vector2(
                            connection.fromSection.rect.center.x * drawScale - viewOffset.x,
                            connection.fromSection.rect.center.y * drawScale - viewOffset.y
                        );
                        Vector2 toPos = new Vector2(
                            connection.toSection.rect.center.x * drawScale - viewOffset.x,
                            connection.toSection.rect.center.y * drawScale - viewOffset.y
                        );

                        // マウス位置がコネクション線の近くにあるかチェック
                        if (IsPointNearLine(e.mousePosition, fromPos, toPos, 10f))
                        {
                            connections.Remove(connection);
                            e.Use();
                            return;
                        }
                    }
                }

                // Room上での右クリック
                SectionRect clickedSection = null;
                foreach (var section in sections)
                {
                    if (section.rect.Contains(Vector2Int.RoundToInt(mouse)))
                    {
                        clickedSection = section;
                        break;
                    }
                }

                // コンテキストメニューを表示
                ShowContextMenu(e.mousePosition, clickedSection);
                e.Use();
            }

            if (e.type == EventType.MouseDrag)
            {
                if (resizingArea)
                {
                    // エリアのリサイズ
                    AdjustAreaResizeToConstraints(resizeHandleType, Vector2Int.RoundToInt(mouse));
                    Repaint();
                    e.Use();
                }
                else if (selectedSection != null)
                {
                    if (resizing)
                    {
                        // セクションのリサイズ
                        AdjustResizeToConstraints(selectedSection, resizeHandleType, Vector2Int.RoundToInt(mouse));
                    }
                    else
                    {
                        // ドラッグ位置を制約に沿って調整
                        Vector2Int targetPosition = Vector2Int.RoundToInt(mouse - dragOffset);
                        selectedSection.rect.position = AdjustPositionToConstraints(targetPosition, selectedSection);
                    }

                    Repaint();
                    e.Use();
                }
            }

            // コネクション作成中のマウス移動でプレビューを更新
            if (creatingConnection && connectionStartSection != null)
            {
                if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
                {
                    Repaint();
                }
            }

            if (e.type == EventType.MouseUp)
            {
                resizing = false;
                resizingArea = false;

                // コネクション作成をキャンセル
                if (creatingConnection)
                {
                    creatingConnection = false;
                    connectionStartSection = null;
                }
            }

            // Deleteキーで選択中のルームを削除
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete && selectedSection != null)
            {
                sections.Remove(selectedSection);
                selectedSection = null;
                e.Use();
            }

            // Escapeキーでコネクション作成をキャンセル
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                if (creatingConnection)
                {
                    creatingConnection = false;
                    connectionStartSection = null;
                    e.Use();
                }
            }
        }

        private void ShowContextMenu(Vector2 mousePosition, SectionRect clickedSection)
        {
            GenericMenu menu = new GenericMenu();

            if (clickedSection != null)
            {
                menu.AddItem(new GUIContent("Delete Section"), false, () => DeleteSection(clickedSection));
                menu.AddItem(new GUIContent("Start Connection"), false, () =>
                {
                    creatingConnection = true;
                    connectionStartSection = clickedSection;
                });
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Copy Section"), false, () => CopySection(clickedSection));
            }
            else
            {
                // 空きスペースでの右クリック
                menu.AddItem(new GUIContent("Create Section"), false, () => CreateSectionAtPosition(mousePosition));
                if (copiedSection != null)
                {
                    menu.AddItem(new GUIContent("Paste Section"), false, () => PasteSectionAtPosition(mousePosition));
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Paste Section"));
                }
            }

            menu.ShowAsContext();
        }

        private void CreateSectionAtPosition(Vector2 screenPosition)
        {
            // スクリーン座標をワールド座標に変換（Y座標を反転）
            Vector2 worldPos = (screenPosition + viewOffset) / drawScale;
            Vector2Int clickPosition = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));

            // 配置可能な位置を探索（クリック位置を中心に）
            Vector2Int? validPosition = FindValidPositionForSection(defaultMinSectionSize, clickPosition);

            if (validPosition.HasValue)
            {
                var newSection = new SectionRect
                (
                    new RectInt(validPosition.Value.x, validPosition.Value.y, defaultMinSectionSize.x, defaultMinSectionSize.y),
                    RoomGenerationType.Random,
                    defaultMinRoomSize
                );
                sections.Add(newSection);
                selectedSection = newSection;
            }
        }

        private void CopySection(SectionRect section)
        {
            copiedSection = section.CopyWith();
        }

        private void PasteSectionAtPosition(Vector2 screenPosition)
        {
            if (copiedSection == null) return;

            // スクリーン座標をワールド座標に変換
            Vector2 worldPos = (screenPosition + viewOffset) / drawScale;
            Vector2Int clickPosition = Vector2Int.RoundToInt(worldPos);

            // 配置可能な位置を探索（クリック位置を中心に）
            Vector2Int? validPosition = FindValidPositionForSection(copiedSection.rect.size, clickPosition);

            if (validPosition.HasValue)
            {
                var newSection = copiedSection.CopyWith(new RectInt(validPosition.Value.x, validPosition.Value.y, copiedSection.rect.width, copiedSection.rect.height));
                sections.Add(newSection);
                selectedSection = newSection;
            }
        }

        private void DeleteSection(SectionRect section)
        {
            // 関連するコネクションも削除
            connections.RemoveAll(c => c.fromSection == section || c.toSection == section);

            sections.Remove(section);
            if (selectedSection == section)
            {
                selectedSection = null;
            }
        }

        private void CreateConnection(SectionRect fromSection, SectionRect toSection)
        {
            // 既存のコネクションをチェック
            foreach (var connection in connections)
            {
                if ((connection.fromSection == fromSection && connection.toSection == toSection) ||
                    (connection.fromSection == toSection && connection.toSection == fromSection))
                {
                    return; // 既に存在する
                }
            }

            // 新しいコネクションを作成
            var newConnection = new Connection
            {
                fromSection = fromSection,
                toSection = toSection
            };

            connections.Add(newConnection);
        }

        private bool IsPointNearLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, float threshold)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLength = line.magnitude;

            if (lineLength < 0.001f) return false;

            Vector2 lineDir = line / lineLength;
            Vector2 pointToStart = point - lineStart;

            float projection = Vector2.Dot(pointToStart, lineDir);
            projection = Mathf.Clamp(projection, 0f, lineLength);

            Vector2 closestPoint = lineStart + lineDir * projection;
            float distance = Vector2.Distance(point, closestPoint);

            return distance <= threshold;
        }

        private Vector2Int GetHandleType(Vector2 mousePosition, Rect drawRect)
        {
            if (!drawRect.Contains(mousePosition))
                return Vector2Int.zero;

            // X軸のハンドル判定
            int xHandle = 0;
            if (mousePosition.x >= drawRect.xMax - handleSize && mousePosition.x <= drawRect.xMax)
                xHandle = 1; // 右側
            else if (mousePosition.x >= drawRect.xMin && mousePosition.x <= drawRect.xMin + handleSize)
                xHandle = -1; // 左側

            // Y軸のハンドル判定
            int yHandle = 0;
            if (mousePosition.y >= drawRect.yMax - handleSize && mousePosition.y <= drawRect.yMax)
                yHandle = 1; // 上側ハンドル
            else if (mousePosition.y >= drawRect.yMin && mousePosition.y <= drawRect.yMin + handleSize)
                yHandle = -1; // 下側ハンドル

            return new Vector2Int(xHandle, yHandle);
        }
    }
}
