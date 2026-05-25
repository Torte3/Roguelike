using UnityEngine;

namespace Editor
{
    public partial class FieldBluePrintEditor
    {
        private bool IsOverlapping(RectInt sectionRect, SectionRect excludeSection)
        {
            foreach (var otherSection in sections)
            {
                if (otherSection == excludeSection) continue;

                if (sectionRect.Overlaps(otherSection.rect))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsSectionInsideArea(RectInt sectionRect)
        {
            return IsSectionInsideArea(sectionRect, new RectInt(Vector2Int.zero, areaSize));
        }

        private bool IsSectionInsideArea(RectInt sectionRect, RectInt areaRect)
        {
            // エリアの境界上も含めて有効にする
            return sectionRect.xMin >= areaRect.xMin &&
                   sectionRect.yMin >= areaRect.yMin &&
                   sectionRect.xMax <= areaRect.xMax &&
                   sectionRect.yMax <= areaRect.yMax;
        }

        private Vector2Int AdjustPositionToConstraints(Vector2Int targetPosition, SectionRect currentSection)
        {
            // ドラッグ位置そのものが配置可能かチェック
            RectInt targetRect = new RectInt(targetPosition.x, targetPosition.y, currentSection.rect.width, currentSection.rect.height);
            if (IsValidRect(targetRect, currentSection))
            {
                return targetPosition; // 配置可能ならそのまま返す
            }

            int adjustedX = FindMaxValidX(targetPosition.x, currentSection);
            int adjustedY = FindMaxValidY(targetPosition.y, currentSection, adjustedX);

            return new Vector2Int(adjustedX, adjustedY);
        }

        private void AdjustResizeToConstraints(SectionRect currentSection, Vector2Int handleType, Vector2Int targetPosition)
        {
            var adjustedRect = new RectInt(currentSection.rect.x, currentSection.rect.y, currentSection.rect.width, currentSection.rect.height);
            if (handleType.x == 1)
            {
                var width = Mathf.Max(1, targetPosition.x - currentSection.rect.xMin);
                var tempRect = new RectInt(
                    currentSection.rect.x,
                    currentSection.rect.y,
                    width,
                    currentSection.rect.height
                );
                if (IsValidRect(tempRect, currentSection))
                {
                    adjustedRect.x = tempRect.x;
                    adjustedRect.width = tempRect.width;
                }
            }
            else if (handleType.x == -1)
            {
                var width = Mathf.Max(1, currentSection.rect.xMax - targetPosition.x);
                var tempRect = new RectInt(
                    currentSection.rect.xMax - width,
                    currentSection.rect.y,
                    width,
                    currentSection.rect.height
                );
                if (IsValidRect(tempRect, currentSection))
                {
                    adjustedRect.x = tempRect.x;
                    adjustedRect.width = tempRect.width;
                }
            }

            if (handleType.y == 1)
            {
                var height = Mathf.Max(1, targetPosition.y - currentSection.rect.yMin);
                var tempRect = new RectInt(
                    currentSection.rect.x,
                    currentSection.rect.y,
                    currentSection.rect.width,
                    height
                );
                if (IsValidRect(tempRect, currentSection))
                {
                    adjustedRect.y = tempRect.y;
                    adjustedRect.height = tempRect.height;
                }
            }
            else if (handleType.y == -1)
            {
                var height = Mathf.Max(1, currentSection.rect.yMax - targetPosition.y);
                var tempRect = new RectInt(
                    currentSection.rect.x,
                    currentSection.rect.yMax - height,
                    currentSection.rect.width,
                    height
                );
                if (IsValidRect(tempRect, currentSection))
                {
                    adjustedRect.y = tempRect.y;
                    adjustedRect.height = tempRect.height;
                }
            }

            currentSection.rect = adjustedRect;
        }

        private int FindMaxValidX(int targetX, SectionRect currentSection)
        {
            int currentX = currentSection.rect.x;
            int lastValidX = currentX;

            if (targetX > currentX) // 右方向ドラッグ
            {
                for (int x = currentX; x <= targetX; x++)
                {
                    RectInt testRect = new RectInt(x, currentSection.rect.y, currentSection.rect.width, currentSection.rect.height);
                    if (IsValidRect(testRect, currentSection))
                    {
                        lastValidX = x; // 有効な位置を記録
                    }
                }
            }
            else if (targetX < currentX) // 左方向ドラッグ
            {
                for (int x = currentX; x >= targetX; x--)
                {
                    RectInt testRect = new RectInt(x, currentSection.rect.y, currentSection.rect.width, currentSection.rect.height);
                    if (IsValidRect(testRect, currentSection))
                    {
                        lastValidX = x; // 有効な位置を記録
                    }
                }
            }

            return lastValidX;
        }

        private int FindMaxValidY(int targetY, SectionRect currentSection, int x)
        {
            int currentY = currentSection.rect.y;
            int lastValidY = currentY; // 最後に見つかった有効な位置

            if (targetY > currentY) // 下方向ドラッグ
            {
                for (int y = currentY; y <= targetY; y++)
                {
                    RectInt testRect = new RectInt(x, y, currentSection.rect.width, currentSection.rect.height);
                    if (IsValidRect(testRect, currentSection))
                    {
                        lastValidY = y; // 有効な位置を記録
                    }
                }
            }
            else if (targetY < currentY) // 上方向ドラッグ
            {
                for (int y = currentY; y >= targetY; y--)
                {
                    RectInt testRect = new RectInt(x, y, currentSection.rect.width, currentSection.rect.height);
                    if (IsValidRect(testRect, currentSection))
                    {
                        lastValidY = y; // 有効な位置を記録
                    }
                }
            }

            return lastValidY;
        }

        private void AdjustAreaResizeToConstraints(Vector2Int handleType, Vector2Int targetPosition)
        {
            var currentAreaRect = new RectInt(Vector2Int.zero, areaSize);
            var adjustedRect = new RectInt(Vector2Int.zero, areaSize);
            if (handleType.x == 1)
            {
                var width = Mathf.Max(1, targetPosition.x - currentAreaRect.xMin);
                var tempRect = new RectInt(
                    currentAreaRect.x,
                    currentAreaRect.y,
                    width,
                    currentAreaRect.height
                );
                if (IsValidAreaRect(tempRect))
                {
                    adjustedRect.x = tempRect.x;
                    adjustedRect.width = tempRect.width;
                }
            }
            else if (handleType.x == -1)
            {
                var width = Mathf.Max(1, currentAreaRect.xMax - targetPosition.x);
                var tempRect = new RectInt(
                    currentAreaRect.xMax - width,
                    currentAreaRect.y,
                    width,
                    currentAreaRect.height
                );
                if (IsValidAreaRect(tempRect))
                {
                    adjustedRect.x = tempRect.x;
                    adjustedRect.width = tempRect.width;
                }
            }

            if (handleType.y == 1)
            {
                var height = Mathf.Max(1, targetPosition.y - currentAreaRect.yMin);
                var tempRect = new RectInt(
                    currentAreaRect.x,
                    currentAreaRect.y,
                    currentAreaRect.width,
                    height
                );
                if (IsValidAreaRect(tempRect))
                {
                    adjustedRect.y = tempRect.y;
                    adjustedRect.height = tempRect.height;
                }
            }
            else if (handleType.y == -1)
            {
                var height = Mathf.Max(1, currentAreaRect.yMax - targetPosition.y);
                var tempRect = new RectInt(
                    currentAreaRect.x,
                    currentAreaRect.yMax - height,
                    currentAreaRect.width,
                    height
                );
                if (IsValidAreaRect(tempRect))
                {
                    adjustedRect.y = tempRect.y;
                    adjustedRect.height = tempRect.height;
                }
            }

            MoveAllSections(-adjustedRect.position);
            areaSize = adjustedRect.size;
        }

        private void MoveAllSections(Vector2Int delta)
        {
            foreach (var section in sections)
            {
                section.rect.position += delta;
            }
        }

        private bool IsValidAreaRect(RectInt rect)
        {
            // 最小サイズチェック
            if (rect.width < gridSize || rect.height < gridSize)
                return false;

            // すべてのセクションが含まれているかチェック
            foreach (var section in sections)
            {
                if (!IsSectionInsideArea(section.rect, rect))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsValidRect(RectInt rect, SectionRect currentSection)
        {
            Vector2Int minSectionSize = currentSection.minRoomSize + new Vector2Int(4, 4);
            if (rect.width < minSectionSize.x || rect.height < minSectionSize.y)
            {
                return false;
            }

            // エリア内かチェック
            if (rect.xMin < 0 || rect.xMax > areaSize.x ||
                rect.yMin < 0 || rect.yMax > areaSize.y)
            {
                return false;
            }

            // 他のルームと重ならないかチェック
            foreach (var section in sections)
            {
                if (section == currentSection) continue;
                if (rect.Overlaps(section.rect))
                {
                    return false;
                }
            }

            return true;
        }

        private Vector2Int? FindValidPositionForSection(Vector2Int targetSize, Vector2Int clickPosition)
        {
            // クリック位置がルーム内に含まれるような配置位置を探索
            // クリック位置から width と height の範囲内でルームを配置できる

            // 探索範囲：クリック位置を中心として、ルームのサイズ分の範囲
            for (int offsetX = -targetSize.x + 1; offsetX <= 0; offsetX++)
            {
                for (int offsetY = -targetSize.y + 1; offsetY <= 0; offsetY++)
                {
                    int x = clickPosition.x + offsetX;
                    int y = clickPosition.y + offsetY;

                    RectInt testRect = new RectInt(x, y, targetSize.x, targetSize.y);

                    // クリック位置がルーム内に含まれるかチェック
                    if (testRect.Contains(clickPosition))
                    {
                        // 配置可能かチェック
                        if (!IsOverlapping(testRect, null) && IsSectionInsideArea(testRect))
                        {
                            return testRect.position;
                        }
                    }
                }
            }

            return null; // 配置可能な位置が見つからない
        }

        private bool IsValidMap()
        {
            // エリアサイズのチェック
            if (areaSize.x <= 0 || areaSize.y <= 0)
            {
                return false;
            }

            // Sectionの存在チェック
            if (sections.Count == 0)
            {
                return false;
            }

            // すべてのSectionがエリア内にあるかチェック
            foreach (var section in sections)
            {
                if (!IsSectionInsideArea(section.rect))
                {
                    return false;
                }
            }

            // Sectionの重複チェック
            for (int i = 0; i < sections.Count; i++)
            {
                for (int j = i + 1; j < sections.Count; j++)
                {
                    if (sections[i].rect.Overlaps(sections[j].rect))
                    {
                        return false;
                    }
                }
            }

            // パラメータの妥当性チェック
            if (minRoomNum < 1 || minRoomNum > sections.Count)
            {
                return false;
            }

            if (maxRoomNum < minRoomNum || maxRoomNum > sections.Count)
            {
                return false;
            }

            if (minRandomBranchNum < 0)
            {
                return false;
            }

            if (maxRandomBranchNum < minRandomBranchNum)
            {
                return false;
            }

            return true;
        }
    }
}
