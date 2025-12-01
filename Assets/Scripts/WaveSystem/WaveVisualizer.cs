using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WaveSystem
{
    /// <summary>
    /// 波显示器
    /// 通用波显示功能，用于玩家和敌人波
    /// 负责获取波数据和container，使用波峰位置和强度值在container里显示波形
    /// </summary>
    public class WaveVisualizer : MonoBehaviour
    {
        [Header("显示设置")]
        [Tooltip("波显示容器（LineRenderer或UI Image将绘制在这里）")]
        [SerializeField] private RectTransform waveContainer;

        [Tooltip("波峰单位高度（强度为1的波峰高度，如果为0则根据容器高度自动计算）")]
        [SerializeField] private float peakUnitHeight = 0f;

        [Tooltip("容器内边距（上下各留出的空间）")]
        [SerializeField] private float padding = 20f;

        [Tooltip("最大波峰强度（用于自动计算单位高度，强度为这个值的波峰不会超过容器）")]
        [SerializeField] private int maxPeakIntensity = 5;

        [Tooltip("波峰宽度（每个波峰的宽度，如果为0则根据容器宽度自动计算）")]
        [SerializeField] private float peakWidth = 0f;

        [Tooltip("波宽度缩放因子（用于缩小波显示宽度，确保在容器内，0.9表示使用90%的容器宽度）")]
        [SerializeField] private float widthScale = 0.9f;

        [Tooltip("波显示线条宽度")]
        [SerializeField] private float lineWidth = 2f;

        [Tooltip("波显示线条颜色")]
        [SerializeField] private Color lineColor = Color.white;

        [Tooltip("是否翻转显示方向（左右翻转）")]
        [SerializeField] private bool reverseDirection = false;

        [Header("波位置范围（从HandWaveGridManager获取）")]
        [Tooltip("波的最小位置（slot的最小位置，从HandWaveGridManager获取）")]
        [SerializeField] private int minPosition;

        [Tooltip("波的最大位置（slot的最大位置，从HandWaveGridManager获取）")]
        [SerializeField] private int maxPosition;

        [Header("运行时状态")]
        [Tooltip("当前显示的波数据")]
        [SerializeField] private Wave currentWave;

        /// <summary>
        /// 计算后的波峰宽度（根据容器宽度自动计算）
        /// </summary>
        private float calculatedPeakWidth = 0f;

        /// <summary>
        /// 计算后的波峰单位高度（根据容器高度自动计算）
        /// </summary>
        private float calculatedPeakUnitHeight = 0f;

        /// <summary>
        /// 波显示容器
        /// </summary>
        public RectTransform WaveContainer
        {
            get => waveContainer;
            set => waveContainer = value;
        }

        /// <summary>
        /// 波峰单位高度
        /// </summary>
        public float PeakUnitHeight
        {
            get => peakUnitHeight;
            set => peakUnitHeight = Mathf.Max(0.1f, value);
        }

        /// <summary>
        /// 计算后的波峰单位高度（只读）
        /// </summary>
        public float CalculatedPeakUnitHeight => calculatedPeakUnitHeight;

        /// <summary>
        /// 波峰宽度
        /// </summary>
        public float PeakWidth
        {
            get => peakWidth;
            set => peakWidth = Mathf.Max(0.1f, value);
        }

        /// <summary>
        /// 是否翻转显示方向（左右翻转）
        /// </summary>
        public bool ReverseDirection
        {
            get => reverseDirection;
            set => reverseDirection = value;
        }


        /// <summary>
        /// 设置波的位置范围（从HandWaveGridManager获取slot范围）
        /// </summary>
        /// <param name="minPos">最小位置（slot的最小位置）</param>
        /// <param name="maxPos">最大位置（slot的最大位置）</param>
        public void SetPositionRange(int minPos, int maxPos)
        {
            minPosition = minPos;
            maxPosition = maxPos;
        }

        /// <summary>
        /// 显示波
        /// </summary>
        /// <param name="wave">要显示的波数据</param>
        public void DisplayWave(Wave wave)
        {
            if (waveContainer == null)
            {
                Debug.LogError("[WaveVisualizer] 波显示容器未设置，无法显示波");
                return;
            }

            // 计算波峰宽度（根据容器宽度自动计算，确保所有位置连续显示）
            CalculatePeakWidth();

            // 计算波峰单位高度（根据容器高度自动计算，确保最大强度波峰不超过容器）
            CalculatePeakUnitHeight();

            // 保存波数据（使用Clone确保数据不被修改）
            if (wave != null && !wave.IsEmpty)
            {
                currentWave = wave.Clone();
                Debug.Log($"[WaveVisualizer] 开始显示波，位置范围: {minPosition} 到 {maxPosition}，波峰数量: {currentWave.PeakCount}，波峰宽度: {calculatedPeakWidth}");
            }
            else
            {
                currentWave = wave; // 空波直接保存
                Debug.Log($"[WaveVisualizer] 开始显示空波，位置范围: {minPosition} 到 {maxPosition}，波峰宽度: {calculatedPeakWidth}");
            }

            // 清除旧的显示
            ClearWave();

            // 生成波显示
            GenerateWaveDisplay();
            
            Debug.Log($"[WaveVisualizer] 波显示完成，容器子对象数量: {waveContainer.childCount}");
        }

        /// <summary>
        /// 清除波显示（不清除currentWave，只清除UI对象）
        /// </summary>
        public void ClearWave()
        {
            if (waveContainer == null)
            {
                return;
            }

            // 清除所有子对象
            foreach (Transform child in waveContainer)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
#if UNITY_EDITOR
                    DestroyImmediate(child.gameObject);
#endif
                }
            }

            // 注意：不清除currentWave，因为GenerateWaveDisplay需要使用它
        }

        /// <summary>
        /// 计算波峰宽度（根据容器宽度自动计算，确保所有位置连续显示）
        /// </summary>
        private void CalculatePeakWidth()
        {
            if (waveContainer == null)
            {
                calculatedPeakWidth = peakWidth > 0 ? peakWidth : 100f; // 默认值
                return;
            }

            // 如果peakWidth为0，则根据容器宽度自动计算
            if (peakWidth <= 0)
            {
                float containerWidth = waveContainer.rect.width;
                int positionCount = maxPosition - minPosition + 1;
                
                if (positionCount > 0 && containerWidth > 0)
                {
                    // 计算每个位置的宽度，使用缩放因子确保在容器内
                    calculatedPeakWidth = (containerWidth * widthScale) / positionCount;
                }
                else
                {
                    calculatedPeakWidth = 100f; // 默认值
                }
            }
            else
            {
                // 使用手动设置的宽度，但确保不超过容器宽度
                float containerWidth = waveContainer.rect.width;
                int positionCount = maxPosition - minPosition + 1;
                float maxAllowedWidth = positionCount > 0 ? (containerWidth * widthScale) / positionCount : peakWidth;
                
                // 使用较小的值，确保不会超出容器
                calculatedPeakWidth = Mathf.Min(peakWidth, maxAllowedWidth);
            }
        }

        /// <summary>
        /// 计算波峰单位高度（根据容器高度自动计算，确保最大强度波峰不超过容器）
        /// </summary>
        private void CalculatePeakUnitHeight()
        {
            if (waveContainer == null)
            {
                calculatedPeakUnitHeight = peakUnitHeight > 0 ? peakUnitHeight : 50f; // 默认值
                return;
            }

            // 如果peakUnitHeight为0，则根据容器高度自动计算
            if (peakUnitHeight <= 0)
            {
                float containerHeight = waveContainer.rect.height;
                
                if (containerHeight > 0 && maxPeakIntensity > 0)
                {
                    // 计算可用高度（减去上下padding）
                    float availableHeight = containerHeight - (padding * 2f);
                    
                    if (availableHeight > 0)
                    {
                        // 强度为maxPeakIntensity的波峰应该正好填满可用高度
                        // 波峰高度 = peakUnitHeight * maxPeakIntensity
                        // 所以：peakUnitHeight = availableHeight / maxPeakIntensity
                        calculatedPeakUnitHeight = availableHeight / maxPeakIntensity;
                    }
                    else
                    {
                        calculatedPeakUnitHeight = 50f; // 默认值
                        Debug.LogWarning($"[WaveVisualizer] 容器高度不足（高度：{containerHeight}，padding：{padding * 2f}），使用默认单位高度");
                    }
                }
                else
                {
                    calculatedPeakUnitHeight = 50f; // 默认值
                }
            }
            else
            {
                // 使用手动设置的高度，但确保不超过容器高度
                float containerHeight = waveContainer.rect.height;
                float availableHeight = containerHeight - (padding * 2f);
                
                if (availableHeight > 0 && maxPeakIntensity > 0)
                {
                    // 计算最大允许的单位高度
                    float maxAllowedUnitHeight = availableHeight / maxPeakIntensity;
                    
                    // 使用较小的值，确保不会超出容器
                    calculatedPeakUnitHeight = Mathf.Min(peakUnitHeight, maxAllowedUnitHeight);
                }
                else
                {
                    calculatedPeakUnitHeight = peakUnitHeight;
                }
            }
        }

        /// <summary>
        /// 生成波显示
        /// </summary>
        private void GenerateWaveDisplay()
        {
            if (waveContainer == null)
            {
                Debug.LogError("[WaveVisualizer] 波显示容器未设置，无法生成显示");
                return;
            }

            // 检查位置范围是否有效
            if (maxPosition < minPosition)
            {
                Debug.LogError($"[WaveVisualizer] 位置范围无效: min={minPosition}, max={maxPosition}");
                return;
            }

            int positionCount = maxPosition - minPosition + 1;
            Debug.Log($"[WaveVisualizer] 生成波显示，位置数量: {positionCount}，波是否为空: {currentWave?.IsEmpty ?? true}");
            Debug.Log($"[WaveVisualizer] 容器大小: {waveContainer.rect.size}，容器激活状态: {waveContainer.gameObject.activeSelf}，波峰宽度: {calculatedPeakWidth}");

            // 波的长度始终是slot的数量范围（minPosition到maxPosition）
            // 为每个位置生成显示（从minPosition到maxPosition，包括所有slot位置）
            // 初始图像应该是一条直线（长度是波长），空位和强度为0的波峰都画成直线
            int lineCount = 0;
            int peakCount = 0;
            
            for (int position = minPosition; position <= maxPosition; position++)
            {
                // 检查是否有波峰
                if (currentWave != null && !currentWave.IsEmpty && currentWave.TryGetPeak(position, out WavePeak peak))
                {
                    // 有波峰
                    if (peak.Value == 0)
                    {
                        // 强度为0，绘制直线
                        DrawStraightLine(position, position);
                        lineCount++;
                    }
                    else
                    {
                        // 绘制正弦片段（局部正弦曲线）
                        DrawPeakSegment(position, peak.Value);
                        peakCount++;
                    }
                }
                else
                {
                    // 空位或波为空，绘制直线
                    DrawStraightLine(position, position);
                    lineCount++;
                }
            }
            
            Debug.Log($"[WaveVisualizer] 生成了 {lineCount} 条直线和 {peakCount} 个波峰片段，容器子对象数量: {waveContainer.childCount}");
            
            // 确保容器激活
            if (!waveContainer.gameObject.activeSelf)
            {
                Debug.LogWarning("[WaveVisualizer] 容器未激活，已激活");
                waveContainer.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 绘制直线（用于空位或强度为0的波峰）
        /// </summary>
        /// <param name="startPosition">起始位置</param>
        /// <param name="endPosition">结束位置</param>
        private void DrawStraightLine(int startPosition, int endPosition)
        {
            float startX = GetXPosition(startPosition);
            float endX = GetXPosition(endPosition);
            float y = 0f; // 基线位置（容器中心）

            // 创建UI Image作为线条
            GameObject lineObj = new GameObject($"Line_{startPosition}_{endPosition}");
            lineObj.transform.SetParent(waveContainer, false);

            RectTransform rectTransform = lineObj.AddComponent<RectTransform>();
            Image image = lineObj.AddComponent<Image>();
            
            // 设置Image的sprite（使用默认白色sprite）
            if (image.sprite == null)
            {
                // 创建一个简单的白色sprite
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                image.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            }
            
            image.color = lineColor;
            image.type = Image.Type.Simple;
            image.raycastTarget = false; // 不接收射线，提高性能

            // 计算线条位置和大小
            float lineLength = Mathf.Abs(endX - startX);
            if (lineLength < 0.1f)
            {
                lineLength = calculatedPeakWidth; // 最小宽度（一个波峰宽度）
            }

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(lineLength, lineWidth);
            rectTransform.anchoredPosition = new Vector2((startX + endX) / 2f, y);
            
            // 确保对象激活
            lineObj.SetActive(true);
        }

        /// <summary>
        /// 绘制波峰片段（局部正弦曲线）
        /// 一个波峰就是一个局部正弦片段，不是一个完整的sine周期
        /// </summary>
        /// <param name="position">波峰位置</param>
        /// <param name="value">波峰强度值</param>
        private void DrawPeakSegment(int position, int value)
        {
            float centerX = GetXPosition(position);
            float amplitude = calculatedPeakUnitHeight * Mathf.Abs(value); // 振幅（线性关系：强度1=基准高度，强度2=2倍高度）
            float direction = value > 0 ? 1f : -1f; // 方向（正值向上，负值向下）
            float halfWidth = calculatedPeakWidth / 2f;

            // 使用UI Image分段绘制正弦曲线
            int segmentCount = 30; // 正弦曲线的分段数（越多越平滑）
            float segmentWidth = calculatedPeakWidth / segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                float t1 = (float)i / segmentCount;
                float t2 = (float)(i + 1) / segmentCount;

                // 计算正弦值（0到π，一个波峰片段）
                // 使用sin函数从0到π，形成一个完整的波峰形状
                float y1 = Mathf.Sin(t1 * Mathf.PI) * amplitude * direction;
                float y2 = Mathf.Sin(t2 * Mathf.PI) * amplitude * direction;

                float x1 = centerX - halfWidth + t1 * calculatedPeakWidth;
                float x2 = centerX - halfWidth + t2 * calculatedPeakWidth;

                // 创建线段
                GameObject segmentObj = new GameObject($"PeakSegment_{position}_{i}");
                segmentObj.transform.SetParent(waveContainer, false);

                RectTransform rectTransform = segmentObj.AddComponent<RectTransform>();
                Image image = segmentObj.AddComponent<Image>();
                
                // 设置Image的sprite（使用默认白色sprite）
                if (image.sprite == null)
                {
                    // 创建一个简单的白色sprite
                    Texture2D texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                }
                
                image.color = lineColor;
                image.type = Image.Type.Simple;
                image.raycastTarget = false; // 不接收射线，提高性能

                // 计算线段长度和角度
                float segmentLength = Vector2.Distance(new Vector2(x1, y1), new Vector2(x2, y2));
                if (segmentLength < 0.1f)
                {
                    segmentLength = 0.1f; // 最小长度
                }
                float angle = Mathf.Atan2(y2 - y1, x2 - x1) * Mathf.Rad2Deg;

                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0f, 0.5f);
                rectTransform.sizeDelta = new Vector2(segmentLength, lineWidth);
                rectTransform.anchoredPosition = new Vector2(x1, y1);
                rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
                
                // 确保对象激活
                segmentObj.SetActive(true);
            }
        }

        /// <summary>
        /// 获取指定位置的X坐标
        /// </summary>
        /// <param name="position">波峰位置</param>
        /// <returns>X坐标</returns>
        private float GetXPosition(int position)
        {
            // 使用计算后的波峰宽度，确保连续显示
            int relativePosition;
            if (reverseDirection)
            {
                // 翻转方向：从maxPosition到minPosition
                relativePosition = maxPosition - position;
            }
            else
            {
                // 正常方向：从minPosition到maxPosition
                relativePosition = position - minPosition;
            }
            
            int positionCount = maxPosition - minPosition + 1;
            // 计算总宽度（使用缩放后的宽度，确保在容器内）
            float totalWidth = (positionCount - 1) * calculatedPeakWidth;
            // 从容器中心开始，向左偏移一半总宽度，然后加上相对位置
            // 确保不会超出容器边界
            float x = (relativePosition * calculatedPeakWidth) - (totalWidth / 2f);
            
            // 限制在容器范围内（考虑缩放因子）
            if (waveContainer != null)
            {
                float containerWidth = waveContainer.rect.width;
                float maxX = (containerWidth * widthScale) / 2f;
                float minX = -(containerWidth * widthScale) / 2f;
                x = Mathf.Clamp(x, minX, maxX);
            }
            
            return x;
        }

        /// <summary>
        /// 更新波显示（当波数据变化时调用）
        /// </summary>
        public void UpdateWave()
        {
            if (currentWave != null)
            {
                DisplayWave(currentWave);
            }
        }
    }
}

