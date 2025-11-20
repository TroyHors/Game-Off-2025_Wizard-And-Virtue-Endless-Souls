using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WaveSystem;

/// <summary>
/// WaveSystem测试类
/// 在Unity编辑器中运行测试，验证WaveSystem的所有功能
/// 每个测试都可以在Inspector中单独运行，查看详细的数值变化
/// </summary>
public class WaveSystemTester : MonoBehaviour
{
    #region WavePeak测试

    [ContextMenu("测试1: WavePeak创建")]
    public void TestWavePeakCreation()
    {
        Debug.Log("========== [测试1] WavePeak创建 ==========");
        
        WavePeak peak = new WavePeak(10, true);
        
        Debug.Log($"创建波峰: Value={peak.Value}, AttackDirection={peak.AttackDirection}");
        Debug.Log($"预期: Value=10, AttackDirection=true");
        
        bool pass = peak.Value == 10 && peak.AttackDirection == true;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试2: WavePeak克隆")]
    public void TestWavePeakClone()
    {
        Debug.Log("========== [测试2] WavePeak克隆 ==========");
        
        WavePeak original = new WavePeak(-7, false);
        Debug.Log($"原始波峰: Value={original.Value}, AttackDirection={original.AttackDirection}");
        
        WavePeak cloned = original.Clone();
        Debug.Log($"克隆波峰: Value={cloned.Value}, AttackDirection={cloned.AttackDirection}");
        
        bool pass = cloned.Value == original.Value && 
                   cloned.AttackDirection == original.AttackDirection &&
                   cloned != original;
        
        Debug.Log($"值是否相同: {cloned.Value == original.Value && cloned.AttackDirection == original.AttackDirection}");
        Debug.Log($"是否为不同对象: {cloned != original}");
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    #endregion

    #region Wave测试

    [ContextMenu("测试3: Wave创建")]
    public void TestWaveCreation()
    {
        Debug.Log("========== [测试3] Wave创建 ==========");
        
        Wave wave = new Wave();
        Debug.Log($"创建新波: PeakCount={wave.PeakCount}, IsEmpty={wave.IsEmpty}");
        Debug.Log($"预期: PeakCount=0, IsEmpty=true");
        
        bool pass = wave.IsEmpty && wave.PeakCount == 0;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试4: Wave添加波峰")]
    public void TestWaveAddPeak()
    {
        Debug.Log("========== [测试4] Wave添加波峰 ==========");
        
        Wave wave = new Wave();
        Debug.Log($"初始状态: PeakCount={wave.PeakCount}");
        
        wave.AddPeak(0, 10, true);
        Debug.Log($"添加波峰1: Position=0, Value=10, Direction=true");
        Debug.Log($"当前状态: PeakCount={wave.PeakCount}, HasPeakAt(0)={wave.HasPeakAt(0)}");
        
        wave.AddPeak(1, -5, false);
        Debug.Log($"添加波峰2: Position=1, Value=-5, Direction=false");
        Debug.Log($"当前状态: PeakCount={wave.PeakCount}, HasPeakAt(1)={wave.HasPeakAt(1)}");
        
        PrintWaveDetails(wave, "最终波状态");
        
        bool pass = wave.PeakCount == 2 && wave.HasPeakAt(0) && wave.HasPeakAt(1) && !wave.HasPeakAt(2);
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试5: Wave移除波峰")]
    public void TestWaveRemovePeak()
    {
        Debug.Log("========== [测试5] Wave移除波峰 ==========");
        
        Wave wave = new Wave();
        wave.AddPeak(0, 10, true);
        wave.AddPeak(1, 5, false);
        Debug.Log("初始状态:");
        PrintWaveDetails(wave, "移除前");
        
        bool removed = wave.RemovePeak(0);
        Debug.Log($"移除位置0的波峰: 返回值={removed}");
        PrintWaveDetails(wave, "移除后");
        
        bool notRemoved = wave.RemovePeak(99);
        Debug.Log($"尝试移除不存在的波峰(位置99): 返回值={notRemoved}");
        
        bool pass = removed && !notRemoved && wave.PeakCount == 1 && !wave.HasPeakAt(0) && wave.HasPeakAt(1);
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试6: Wave查询波峰")]
    public void TestWaveQuery()
    {
        Debug.Log("========== [测试6] Wave查询波峰 ==========");
        
        Wave wave = new Wave();
        wave.AddPeak(5, 10, true);
        PrintWaveDetails(wave, "初始状态");
        
        WavePeak peak = wave.GetPeak(5);
        Debug.Log($"GetPeak(5): {(peak != null ? $"Value={peak.Value}" : "null")}");
        
        bool found = wave.TryGetPeak(5, out WavePeak peak2);
        Debug.Log($"TryGetPeak(5): found={found}, Value={(peak2 != null ? peak2.Value.ToString() : "null")}");
        
        bool notFound = wave.TryGetPeak(99, out WavePeak peak3);
        Debug.Log($"TryGetPeak(99): found={notFound}, Value={(peak3 != null ? peak3.Value.ToString() : "null")}");
        
        bool pass = peak != null && peak.Value == 10 && found && !notFound && peak3 == null;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试7: Wave空状态")]
    public void TestWaveEmpty()
    {
        Debug.Log("========== [测试7] Wave空状态 ==========");
        
        Wave wave = new Wave();
        Debug.Log($"初始状态: IsEmpty={wave.IsEmpty}, PeakCount={wave.PeakCount}");
        
        wave.AddPeak(0, 10, true);
        Debug.Log($"添加波峰后: IsEmpty={wave.IsEmpty}, PeakCount={wave.PeakCount}");
        
        wave.Clear();
        Debug.Log($"清空后: IsEmpty={wave.IsEmpty}, PeakCount={wave.PeakCount}");
        
        bool pass = wave.IsEmpty && wave.PeakCount == 0;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试8: Wave克隆")]
    public void TestWaveClone()
    {
        Debug.Log("========== [测试8] Wave克隆 ==========");
        
        Wave original = new Wave();
        original.AddPeak(0, 10, true);
        original.AddPeak(1, -5, false);
        PrintWaveDetails(original, "原始波");
        
        Wave cloned = original.Clone();
        PrintWaveDetails(cloned, "克隆波");
        
        Debug.Log($"是否为不同对象: {cloned != original}");
        Debug.Log($"波峰数量是否相同: {cloned.PeakCount == original.PeakCount}");
        
        WavePeak originalPeak = original.GetPeak(0);
        WavePeak clonedPeak = cloned.GetPeak(0);
        Debug.Log($"波峰是否为不同对象: {clonedPeak != originalPeak}");
        Debug.Log($"波峰值是否相同: {clonedPeak.Value == originalPeak.Value}");
        
        bool pass = cloned != original && cloned.PeakCount == original.PeakCount && 
                   clonedPeak != originalPeak && clonedPeak.Value == originalPeak.Value;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试9: Wave排序")]
    public void TestWaveSortedPeaks()
    {
        Debug.Log("========== [测试9] Wave排序 ==========");
        
        Wave wave = new Wave();
        wave.AddPeak(3, 10, true);
        wave.AddPeak(1, 5, false);
        wave.AddPeak(5, -3, true);
        PrintWaveDetails(wave, "原始波（无序添加）");
        
        var sorted = wave.GetSortedPeaks();
        Debug.Log("排序后的波峰列表:");
        for (int i = 0; i < sorted.Count; i++)
        {
            Debug.Log($"  [{i}] Position={sorted[i].position}, Value={sorted[i].peak.Value}");
        }
        
        bool pass = sorted.Count == 3 && sorted[0].position == 1 && sorted[1].position == 3 && sorted[2].position == 5;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试10: Wave范围查询")]
    public void TestWavePeaksInRange()
    {
        Debug.Log("========== [测试10] Wave范围查询 ==========");
        
        Wave wave = new Wave();
        wave.AddPeak(0, 10, true);
        wave.AddPeak(3, 5, false);
        wave.AddPeak(5, -3, true);
        wave.AddPeak(7, 8, false);
        PrintWaveDetails(wave, "原始波");
        
        var inRange = wave.GetPeaksInRange(3, 6);
        Debug.Log("范围[3, 6]内的波峰:");
        foreach (var (position, peak) in inRange)
        {
            Debug.Log($"  Position={position}, Value={peak.Value}");
        }
        
        bool pass = inRange.Count == 2 && inRange[0].position == 3 && inRange[1].position == 5;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试11: Wave最小最大位置")]
    public void TestWaveMinMaxPosition()
    {
        Debug.Log("========== [测试11] Wave最小最大位置 ==========");
        
        Wave wave = new Wave();
        Debug.Log($"空波: MinPosition={wave.GetMinPosition()}, MaxPosition={wave.GetMaxPosition()}");
        
        wave.AddPeak(5, 10, true);
        wave.AddPeak(1, 5, false);
        wave.AddPeak(10, -3, true);
        PrintWaveDetails(wave, "添加波峰后");
        
        Debug.Log($"MinPosition={wave.GetMinPosition()}, MaxPosition={wave.GetMaxPosition()}");
        Debug.Log($"预期: MinPosition=1, MaxPosition=10");
        
        bool pass = wave.GetMinPosition() == 1 && wave.GetMaxPosition() == 10;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    #endregion

    #region WavePairing测试

    [ContextMenu("测试12: 配对-同位置同方向")]
    public void TestPairingSamePositionSameDirection()
    {
        Debug.Log("========== [测试12] 配对-同位置同方向 ==========");
        
        Wave waveA = new Wave();
        waveA.AddPeak(0, 10, true);
        PrintWaveDetails(waveA, "波A");
        
        Wave waveB = new Wave();
        waveB.AddPeak(0, 5, true);
        PrintWaveDetails(waveB, "波B");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        
        for (int i = 0; i < results.Count; i++)
        {
            PrintWaveDetails(results[i], $"结果波{i + 1}");
        }
        
        WavePeak result = results[0].GetPeak(0);
        Debug.Log($"位置0的结果: Value={result.Value}, Direction={result.AttackDirection}");
        Debug.Log($"预期: Value=15 (10+5), Direction=true");
        
        bool pass = results.Count == 1 && result != null && result.Value == 15 && result.AttackDirection == true;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试13: 配对-同位置反方向")]
    public void TestPairingSamePositionOppositeDirection()
    {
        Debug.Log("========== [测试13] 配对-同位置反方向 ==========");
        
        Wave waveA = new Wave();
        waveA.AddPeak(0, 10, true);
        PrintWaveDetails(waveA, "波A");
        
        Wave waveB = new Wave();
        waveB.AddPeak(0, -5, false);
        PrintWaveDetails(waveB, "波B");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        
        for (int i = 0; i < results.Count; i++)
        {
            PrintWaveDetails(results[i], $"结果波{i + 1}");
        }
        
        WavePeak result = results[0].GetPeak(0);
        Debug.Log($"位置0的结果: Value={result.Value}, Direction={result.AttackDirection}");
        Debug.Log($"预期: Value=5 (10+(-5)), Direction=true (因为10的绝对值>5)");
        
        bool pass = results.Count == 1 && result != null && result.Value == 5 && result.AttackDirection == true;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试14: 配对-完全抵消")]
    public void TestPairingSamePositionOppositeDirectionCancel()
    {
        Debug.Log("========== [测试14] 配对-完全抵消 ==========");
        
        Wave waveA = new Wave();
        waveA.AddPeak(0, 10, true);
        PrintWaveDetails(waveA, "波A");
        
        Wave waveB = new Wave();
        waveB.AddPeak(0, -10, false);
        PrintWaveDetails(waveB, "波B");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        
        for (int i = 0; i < results.Count; i++)
        {
            PrintWaveDetails(results[i], $"结果波{i + 1}");
        }
        
        WavePeak result = results[0].GetPeak(0);
        Debug.Log($"位置0的结果: Value={result.Value}");
        Debug.Log($"预期: Value=0 (10+(-10)), 波峰仍会保留");
        
        bool pass = results.Count == 1 && result != null && result.Value == 0;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试15: 配对-反方向不同强度")]
    public void TestPairingSamePositionOppositeDirectionDifferentStrength()
    {
        Debug.Log("========== [测试15] 配对-反方向不同强度 ==========");
        
        Wave waveA = new Wave();
        waveA.AddPeak(0, 5, true);
        PrintWaveDetails(waveA, "波A");
        
        Wave waveB = new Wave();
        waveB.AddPeak(0, -10, false);
        PrintWaveDetails(waveB, "波B");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        
        for (int i = 0; i < results.Count; i++)
        {
            PrintWaveDetails(results[i], $"结果波{i + 1}");
        }
        
        WavePeak result = results[0].GetPeak(0);
        Debug.Log($"位置0的结果: Value={result.Value}, Direction={result.AttackDirection}");
        Debug.Log($"预期: Value=-5 (5+(-10)), Direction=false (因为10的绝对值>5)");
        
        bool pass = results.Count == 1 && result != null && result.Value == -5 && result.AttackDirection == false;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试16: 配对-不同位置")]
    public void TestPairingDifferentPositions()
    {
        Debug.Log("========== [测试16] 配对-不同位置 ==========");
        
        Wave waveA = new Wave();
        waveA.AddPeak(0, 10, true);
        PrintWaveDetails(waveA, "波A");
        
        Wave waveB = new Wave();
        waveB.AddPeak(1, 5, false);
        PrintWaveDetails(waveB, "波B");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        
        for (int i = 0; i < results.Count; i++)
        {
            PrintWaveDetails(results[i], $"结果波{i + 1}");
        }
        
        Wave waveWith0 = results.Find(w => w.HasPeakAt(0));
        Wave waveWith1 = results.Find(w => w.HasPeakAt(1));
        
        Debug.Log($"包含位置0的波: {(waveWith0 != null ? $"Value={waveWith0.GetPeak(0).Value}" : "不存在")}");
        Debug.Log($"包含位置1的波: {(waveWith1 != null ? $"Value={waveWith1.GetPeak(1).Value}" : "不存在")}");
        Debug.Log($"预期: 生成2个波，分别包含位置0和位置1");
        
        bool pass = results.Count == 2 && waveWith0 != null && waveWith1 != null && 
                   waveWith0.GetPeak(0).Value == 10 && waveWith1.GetPeak(1).Value == 5;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试17: 配对-空波")]
    public void TestPairingEmptyWaves()
    {
        Debug.Log("========== [测试17] 配对-空波 ==========");
        
        Wave waveA = new Wave();
        Wave waveB = new Wave();
        Debug.Log("波A和波B都是空波");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        Debug.Log($"预期: 0个波");
        
        bool pass = results.Count == 0;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试18: 配对-一个空波")]
    public void TestPairingOneEmptyWave()
    {
        Debug.Log("========== [测试18] 配对-一个空波 ==========");
        
        Wave waveA = new Wave();
        waveA.AddPeak(0, 10, true);
        PrintWaveDetails(waveA, "波A");
        
        Wave waveB = new Wave();
        Debug.Log("波B是空波");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        
        for (int i = 0; i < results.Count; i++)
        {
            PrintWaveDetails(results[i], $"结果波{i + 1}");
        }
        
        Debug.Log($"预期: 1个波，包含位置0，Value=10");
        
        bool pass = results.Count == 1 && results[0].GetPeak(0) != null && results[0].GetPeak(0).Value == 10;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试19: 配对-零值波峰")]
    public void TestPairingZeroValuePeak()
    {
        Debug.Log("========== [测试19] 配对-零值波峰 ==========");
        
        Wave waveA = new Wave();
        waveA.AddPeak(0, 0, true);
        PrintWaveDetails(waveA, "波A");
        
        Wave waveB = new Wave();
        waveB.AddPeak(0, 0, false);
        PrintWaveDetails(waveB, "波B");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        
        for (int i = 0; i < results.Count; i++)
        {
            PrintWaveDetails(results[i], $"结果波{i + 1}");
        }
        
        WavePeak result = results[0].GetPeak(0);
        Debug.Log($"位置0的结果: Value={result.Value}");
        Debug.Log($"预期: Value=0 (0+0), 波峰仍会保留");
        
        bool pass = results.Count == 1 && result != null && result.Value == 0;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试20: 配对-多波峰复杂场景")]
    public void TestPairingMultiplePeaks()
    {
        Debug.Log("========== [测试20] 配对-多波峰复杂场景 ==========");
        
        Wave waveA = new Wave();
        waveA.AddPeak(0, 10, true);
        waveA.AddPeak(1, 5, true);
        waveA.AddPeak(2, -3, false);
        PrintWaveDetails(waveA, "波A");
        
        Wave waveB = new Wave();
        waveB.AddPeak(0, -5, false);
        waveB.AddPeak(1, 3, true);
        waveB.AddPeak(3, 7, true);
        PrintWaveDetails(waveB, "波B");
        
        Debug.Log("配对计算:");
        Debug.Log("  位置0: 10(true) + -5(false) = 5(true) [绝对值10>5，方向继承true]");
        Debug.Log("  位置1: 5(true) + 3(true) = 8(true)");
        Debug.Log("  位置2: -3(false) [单独保留]");
        Debug.Log("  位置3: 7(true) [单独保留]");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        
        for (int i = 0; i < results.Count; i++)
        {
            PrintWaveDetails(results[i], $"结果波{i + 1}");
        }
        
        Wave waveTrue = results.Find(w => w.HasPeakAt(0));
        Wave waveFalse = results.Find(w => w.HasPeakAt(2));
        
        bool pass = results.Count == 2 && 
                   waveTrue != null && waveTrue.GetPeak(0).Value == 5 && 
                   waveTrue.GetPeak(1).Value == 8 && waveTrue.GetPeak(3).Value == 7 &&
                   waveFalse != null && waveFalse.GetPeak(2).Value == -3;
        
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试21: 配对-方向分类")]
    public void TestPairingDirectionClassification()
    {
        Debug.Log("========== [测试21] 配对-方向分类 ==========");
        
        Wave waveA = new Wave();
        waveA.AddPeak(0, 10, true);
        waveA.AddPeak(1, 5, false);
        PrintWaveDetails(waveA, "波A");
        
        Wave waveB = new Wave();
        waveB.AddPeak(0, 3, true);
        waveB.AddPeak(1, -2, false);
        PrintWaveDetails(waveB, "波B");
        
        Debug.Log("配对计算:");
        Debug.Log("  位置0: 10(true) + 3(true) = 13(true)");
        Debug.Log("  位置1: 5(false) + -2(false) = 3(false)");
        Debug.Log("  预期: 生成2个波，分别包含true和false方向的波峰");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        
        for (int i = 0; i < results.Count; i++)
        {
            PrintWaveDetails(results[i], $"结果波{i + 1}");
        }
        
        Wave waveTrue = results.Find(w => w.HasPeakAt(0));
        Wave waveFalse = results.Find(w => w.HasPeakAt(1));
        
        bool pass = results.Count == 2 && 
                   waveTrue != null && waveTrue.GetPeak(0).Value == 13 &&
                   waveFalse != null && waveFalse.GetPeak(1).Value == 3;
        
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试22: 波方向一致性检查")]
    public void TestWaveDirectionConsistency()
    {
        Debug.Log("========== [测试22] 波方向一致性检查 ==========");
        
        Wave wave = new Wave();
        wave.AddPeak(0, 10, true);
        PrintWaveDetails(wave, "添加第一个波峰（方向=true）");
        Debug.Log($"波的方向: {(wave.AttackDirection.HasValue ? (wave.AttackDirection.Value ? "→玩家" : "→其他") : "未定义")}");
        
        // 尝试添加相同方向的波峰（应该成功）
        wave.AddPeak(1, 5, true);
        PrintWaveDetails(wave, "添加相同方向的波峰（应该成功）");
        
        // 尝试添加不同方向的波峰（应该失败）
        Debug.Log("尝试添加不同方向的波峰（方向=false）...");
        wave.AddPeak(2, 3, false);
        PrintWaveDetails(wave, "尝试添加不同方向后");
        Debug.Log($"预期: 位置2的波峰不应该被添加，PeakCount应该仍为2");
        
        bool pass = wave.PeakCount == 2 && !wave.HasPeakAt(2);
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试23: 波特定位置波峰插入")]
    public void TestWavePeakInsertion()
    {
        Debug.Log("========== [测试23] 波特定位置波峰插入 ==========");
        
        Wave wave = new Wave();
        Debug.Log("初始状态: 空波");
        
        // 在不同位置插入波峰
        wave.AddPeak(5, 10, true);
        wave.AddPeak(2, 5, true);
        wave.AddPeak(8, -3, true);
        wave.AddPeak(1, 7, true);
        wave.AddPeak(10, 15, true);
        
        Debug.Log("插入波峰后:");
        PrintWaveDetails(wave, "插入后");
        
        // 以列表形式打印波的状态
        var sortedPeaks = wave.GetSortedPeaks();
        Debug.Log("--- 波状态（列表形式）---");
        Debug.Log($"波峰数量: {sortedPeaks.Count}");
        Debug.Log($"波的方向: {(wave.AttackDirection.HasValue ? (wave.AttackDirection.Value ? "→玩家" : "→其他") : "未定义")}");
        Debug.Log("波峰列表（按位置排序）:");
        for (int i = 0; i < sortedPeaks.Count; i++)
        {
            var (position, peak) = sortedPeaks[i];
            Debug.Log($"  [{i}] 位置={position}, 强度={peak.Value}, 方向={(peak.AttackDirection ? "→玩家" : "→其他")}");
        }
        
        // 以数组形式打印
        var peaksArray = sortedPeaks.ToArray();
        Debug.Log("--- 波状态（数组形式）---");
        Debug.Log($"数组长度: {peaksArray.Length}");
        for (int i = 0; i < peaksArray.Length; i++)
        {
            var (position, peak) = peaksArray[i];
            Debug.Log($"  peaksArray[{i}] = Position:{position}, Value:{peak.Value}");
        }
        
        bool pass = sortedPeaks.Count == 5 && 
                   sortedPeaks[0].position == 1 && 
                   sortedPeaks[1].position == 2 &&
                   sortedPeaks[2].position == 5 &&
                   sortedPeaks[3].position == 8 &&
                   sortedPeaks[4].position == 10;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试24: 根据给定数据生成波")]
    public void TestWaveFromData()
    {
        Debug.Log("========== [测试24] 根据给定数据生成波 ==========");
        
        // 准备波数据
        WaveData waveData = new WaveData(true);
        waveData.AddPeak(0, 10);
        waveData.AddPeak(2, 5);
        waveData.AddPeak(4, -3);
        waveData.AddPeak(6, 8);
        waveData.AddPeak(8, 12);
        
        Debug.Log("给定的波数据:");
        Debug.Log($"  方向: {(waveData.AttackDirection ? "→玩家" : "→其他")}");
        Debug.Log($"  波峰数量: {waveData.PeakCount}");
        foreach (var kvp in waveData.PeakData)
        {
            Debug.Log($"  位置{kvp.Key}: 强度={kvp.Value}");
        }
        
        // 使用FromData方法生成波
        Wave wave = Wave.FromData(waveData);
        PrintWaveDetails(wave, "生成的波");
        
        Debug.Log($"生成的波包含 {wave.PeakCount} 个波峰");
        Debug.Log($"波的方向: {(wave.AttackDirection.HasValue ? (wave.AttackDirection.Value ? "→玩家" : "→其他") : "未定义")}");
        
        // 验证所有波峰都被正确添加
        bool allPeaksAdded = true;
        foreach (var kvp in waveData.PeakData)
        {
            int position = kvp.Key;
            int expectedValue = kvp.Value;
            WavePeak wavePeak = wave.GetPeak(position);
            if (wavePeak == null || 
                wavePeak.Value != expectedValue || 
                wavePeak.AttackDirection != waveData.AttackDirection)
            {
                allPeaksAdded = false;
                Debug.LogError($"波峰位置{position}未正确添加或值不匹配");
            }
        }
        
        // 测试不同方向的波数据（应该失败）
        Debug.Log("--- 测试不同方向的波数据（应该失败）---");
        WaveData mixedDirectionData = new WaveData(true);
        mixedDirectionData.AddPeak(0, 10);
        mixedDirectionData.AddPeak(1, 5);
        // 注意：WaveData本身只存储一个方向，所以这里测试的是在创建波后尝试添加不同方向的波峰
        Wave waveMixed = Wave.FromData(mixedDirectionData);
        Debug.Log($"从混合方向数据生成的波: PeakCount={waveMixed.PeakCount}");
        Debug.Log("尝试添加不同方向的波峰...");
        waveMixed.AddPeak(2, 3, false);  // 尝试添加不同方向
        Debug.Log($"添加后: PeakCount={waveMixed.PeakCount} (应该仍为2，因为方向不同被拒绝)");
        PrintWaveDetails(waveMixed, "混合方向的波");
        
        bool pass = allPeaksAdded && wave.PeakCount == waveData.PeakCount && 
                   wave.AttackDirection == true &&
                   waveMixed.PeakCount == 2; // 只添加了前两个波峰，第三个被拒绝
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 打印波的详细信息
    /// </summary>
    private void PrintWaveDetails(Wave wave, string label)
    {
        Debug.Log($"--- {label} ---");
        Debug.Log($"  PeakCount: {wave.PeakCount}, IsEmpty: {wave.IsEmpty}");
        Debug.Log($"  波的方向: {(wave.AttackDirection.HasValue ? (wave.AttackDirection.Value ? "→玩家" : "→其他") : "未定义（空波）")}");
        
        if (wave.IsEmpty)
        {
            Debug.Log("  (空波)");
        }
        else
        {
            var sortedPeaks = wave.GetSortedPeaks();
            foreach (var (position, peak) in sortedPeaks)
            {
                string direction = peak.AttackDirection ? "→玩家" : "→其他";
                Debug.Log($"  位置{position}: 强度={peak.Value}, 方向={direction}");
            }
        }
    }

    #endregion
}
