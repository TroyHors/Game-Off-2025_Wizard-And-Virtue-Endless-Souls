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
    #region 新功能测试

    [ContextMenu("测试1: 负波生成")]
    public void TestGenerateNegativeWave()
    {
        Debug.Log("========== [测试1] 负波生成 ==========");
        
        Wave originalWave = new Wave();
        originalWave.AddPeak(0, 10, true);
        originalWave.AddPeak(1, -5, true);
        originalWave.AddPeak(2, 3, true);
        PrintWaveDetails(originalWave, "原始波");
        
        Wave negativeWave = originalWave.GenerateNegativeWave();
        PrintWaveDetails(negativeWave, "负波");
        
        Debug.Log("验证:");
        Debug.Log($"  波峰数量是否相同: {originalWave.PeakCount == negativeWave.PeakCount}");
        Debug.Log($"  方向是否相同: {originalWave.AttackDirection == negativeWave.AttackDirection}");
        
        bool allPeaksNegated = true;
        foreach (var position in originalWave.Positions)
        {
            WavePeak originalPeak = originalWave.GetPeak(position);
            WavePeak negativePeak = negativeWave.GetPeak(position);
            bool isNegated = negativePeak != null && negativePeak.Value == -originalPeak.Value;
            Debug.Log($"  位置{position}: 原始={originalPeak.Value}, 负波={negativePeak.Value}, 是否取反={isNegated}");
            if (!isNegated)
            {
                allPeaksNegated = false;
            }
        }
        
        bool pass = originalWave.PeakCount == negativeWave.PeakCount &&
                   originalWave.AttackDirection == negativeWave.AttackDirection &&
                   allPeaksNegated &&
                   originalWave != negativeWave;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试2: 设置攻击方向")]
    public void TestSetAttackDirection()
    {
        Debug.Log("========== [测试2] 设置攻击方向 ==========");
        
        // 测试空波设置方向
        Wave emptyWave = new Wave();
        Debug.Log("空波初始方向: " + (emptyWave.AttackDirection.HasValue ? emptyWave.AttackDirection.Value.ToString() : "null"));
        bool set1 = emptyWave.SetAttackDirection(true);
        Debug.Log($"设置方向为true: 成功={set1}, 当前方向={emptyWave.AttackDirection}");
        
        // 测试有波峰的波修改方向
        Wave wave = new Wave();
        wave.AddPeak(0, 10, true);
        wave.AddPeak(1, 5, true);
        PrintWaveDetails(wave, "初始波（方向=true）");
        
        bool set2 = wave.SetAttackDirection(false);
        Debug.Log($"修改方向为false: 成功={set2}");
        PrintWaveDetails(wave, "修改方向后");
        
        // 验证所有波峰的方向都已更新
        bool allPeaksUpdated = true;
        foreach (var position in wave.Positions)
        {
            WavePeak peak = wave.GetPeak(position);
            if (peak.AttackDirection != false)
            {
                allPeaksUpdated = false;
                Debug.LogError($"位置{position}的波峰方向未正确更新: {peak.AttackDirection}");
            }
        }
        
        // 再次修改回true
        bool set3 = wave.SetAttackDirection(true);
        Debug.Log($"再次修改方向为true: 成功={set3}");
        PrintWaveDetails(wave, "再次修改方向后");
        
        bool pass = set1 && set2 && set3 && 
                   wave.AttackDirection == true &&
                   allPeaksUpdated &&
                   wave.GetPeak(0).AttackDirection == true &&
                   wave.GetPeak(1).AttackDirection == true;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试3: 设置波峰强度")]
    public void TestSetPeakValue()
    {
        Debug.Log("========== [测试3] 设置波峰强度 ==========");
        
        Wave wave = new Wave();
        wave.AddPeak(0, 10, true);
        wave.AddPeak(1, 5, true);
        PrintWaveDetails(wave, "初始波");
        
        bool set1 = wave.SetPeakValue(0, 20);
        Debug.Log($"设置位置0的强度为20: 成功={set1}");
        PrintWaveDetails(wave, "设置后");
        
        bool set2 = wave.SetPeakValue(99, 100);
        Debug.Log($"设置不存在位置99的强度: 成功={set2}, 预期=false");
        
        WavePeak peak0 = wave.GetPeak(0);
        bool pass = set1 && !set2 && peak0 != null && peak0.Value == 20;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试4: 移动波峰位置")]
    public void TestMovePeak()
    {
        Debug.Log("========== [测试4] 移动波峰位置 ==========");
        
        Wave wave = new Wave();
        wave.AddPeak(0, 10, true);
        wave.AddPeak(1, 5, true);
        wave.AddPeak(2, 3, true);
        PrintWaveDetails(wave, "初始波");
        
        bool move1 = wave.MovePeak(0, 5);
        Debug.Log($"移动位置0到位置5: 成功={move1}");
        PrintWaveDetails(wave, "移动后");
        
        bool move2 = wave.MovePeak(99, 100);
        Debug.Log($"移动不存在位置99: 成功={move2}, 预期=false");
        
        bool move3 = wave.MovePeak(1, 1);
        Debug.Log($"移动位置1到位置1（相同位置）: 成功={move3}");
        
        bool pass = move1 && !move2 && move3 &&
                   !wave.HasPeakAt(0) && wave.HasPeakAt(5) &&
                   wave.GetPeak(5).Value == 10;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试5: 批量设置波峰强度")]
    public void TestSetPeakValues()
    {
        Debug.Log("========== [测试5] 批量设置波峰强度 ==========");
        
        Wave wave = new Wave();
        wave.AddPeak(0, 10, true);
        wave.AddPeak(1, 5, true);
        wave.AddPeak(2, 3, true);
        PrintWaveDetails(wave, "初始波");
        
        Dictionary<int, int> newValues = new Dictionary<int, int>
        {
            { 0, 20 },
            { 1, 15 },
            { 99, 100 }  // 不存在的位置
        };
        
        int successCount = wave.SetPeakValues(newValues);
        Debug.Log($"批量设置: 成功设置{successCount}个波峰");
        PrintWaveDetails(wave, "批量设置后");
        
        bool pass = successCount == 2 &&
                   wave.GetPeak(0).Value == 20 &&
                   wave.GetPeak(1).Value == 15 &&
                   wave.GetPeak(2).Value == 3;  // 位置2未改变
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试6: 综合修改测试")]
    public void TestWaveModification()
    {
        Debug.Log("========== [测试6] 综合修改测试 ==========");
        
        Wave wave = new Wave();
        wave.AddPeak(0, 10, true);
        wave.AddPeak(1, 5, true);
        wave.AddPeak(2, 3, true);
        PrintWaveDetails(wave, "初始波");
        
        // 修改强度
        wave.SetPeakValue(0, 20);
        Debug.Log("修改位置0的强度为20");
        
        // 移动位置
        wave.MovePeak(1, 10);
        Debug.Log("移动位置1到位置10");
        
        // 批量修改
        Dictionary<int, int> newValues = new Dictionary<int, int>
        {
            { 2, 30 },
            { 10, 50 }
        };
        wave.SetPeakValues(newValues);
        Debug.Log("批量修改位置2和10的强度");
        
        PrintWaveDetails(wave, "最终波");
        
        bool pass = wave.GetPeak(0).Value == 20 &&
                   !wave.HasPeakAt(1) &&
                   wave.GetPeak(2).Value == 30 &&
                   wave.GetPeak(10).Value == 50;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    #endregion

    #region WavePairing测试

    [ContextMenu("测试7: 配对-同位置同方向")]
    public void TestPairingSamePositionSameDirection()
    {
        Debug.Log("========== [测试7] 配对-同位置同方向 ==========");
        
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

    [ContextMenu("测试8: 配对-同位置反方向")]
    public void TestPairingSamePositionOppositeDirection()
    {
        Debug.Log("========== [测试8] 配对-同位置反方向 ==========");
        
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

    [ContextMenu("测试9: 配对-完全抵消")]
    public void TestPairingSamePositionOppositeDirectionCancel()
    {
        Debug.Log("========== [测试9] 配对-完全抵消 ==========");
        
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

    [ContextMenu("测试10: 配对-反方向不同强度")]
    public void TestPairingSamePositionOppositeDirectionDifferentStrength()
    {
        Debug.Log("========== [测试10] 配对-反方向不同强度 ==========");
        
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

    [ContextMenu("测试11: 配对-不同位置")]
    public void TestPairingDifferentPositions()
    {
        Debug.Log("========== [测试11] 配对-不同位置 ==========");
        
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

    [ContextMenu("测试12: 配对-空波")]
    public void TestPairingEmptyWaves()
    {
        Debug.Log("========== [测试12] 配对-空波 ==========");
        
        Wave waveA = new Wave();
        Wave waveB = new Wave();
        Debug.Log("波A和波B都是空波");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        Debug.Log($"预期: 0个波");
        
        bool pass = results.Count == 0;
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试13: 配对-一个空波")]
    public void TestPairingOneEmptyWave()
    {
        Debug.Log("========== [测试13] 配对-一个空波 ==========");
        
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

    [ContextMenu("测试14: 配对-零值波峰")]
    public void TestPairingZeroValuePeak()
    {
        Debug.Log("========== [测试14] 配对-零值波峰 ==========");
        
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

    [ContextMenu("测试15: 配对-多波峰复杂场景")]
    public void TestPairingMultiplePeaks()
    {
        Debug.Log("========== [测试15] 配对-多波峰复杂场景 ==========");
        
        // 波A：所有波峰方向为true（攻向玩家）
        Wave waveA = new Wave();
        waveA.AddPeak(0, 10, true);
        waveA.AddPeak(1, 5, true);
        waveA.AddPeak(2, 3, true);  // 改为true方向，以符合方向一致性
        PrintWaveDetails(waveA, "波A");
        
        // 波B：所有波峰方向为false（不攻向玩家）
        Wave waveB = new Wave();
        waveB.AddPeak(0, -5, false);
        waveB.AddPeak(1, 3, false);  // 改为false方向，以符合方向一致性
        waveB.AddPeak(3, 7, false);  // 改为false方向，以符合方向一致性
        PrintWaveDetails(waveB, "波B");
        
        Debug.Log("配对计算:");
        Debug.Log("  位置0: 10(true) + -5(false) = 5(true) [绝对值10>5，方向继承true]");
        Debug.Log("  位置1: 5(true) + 3(false) = 8(true) [绝对值5>3，方向继承true]");
        Debug.Log("  位置2: 3(true) [单独保留]");
        Debug.Log("  位置3: 7(false) [单独保留]");
        Debug.Log("  预期: 生成2个波，一个包含位置0,1,2（true方向），一个包含位置3（false方向）");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        
        for (int i = 0; i < results.Count; i++)
        {
            PrintWaveDetails(results[i], $"结果波{i + 1}");
        }
        
        Wave waveTrue = results.Find(w => w.HasPeakAt(0));
        Wave waveFalse = results.Find(w => w.HasPeakAt(3));
        
        bool pass = results.Count == 2 && 
                   waveTrue != null && waveTrue.GetPeak(0).Value == 5 && 
                   waveTrue.GetPeak(1).Value == 8 && waveTrue.GetPeak(2).Value == 3 &&
                   waveFalse != null && waveFalse.GetPeak(3).Value == 7;
        
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试16: 配对-方向分类")]
    public void TestPairingDirectionClassification()
    {
        Debug.Log("========== [测试16] 配对-方向分类 ==========");
        
        // 波A：所有波峰方向为true
        Wave waveA = new Wave();
        waveA.AddPeak(0, 10, true);
        waveA.AddPeak(1, 5, true);  // 改为true方向，以符合方向一致性
        PrintWaveDetails(waveA, "波A");
        
        // 波B：所有波峰方向为false
        Wave waveB = new Wave();
        waveB.AddPeak(0, 3, false);  // 改为false方向，以符合方向一致性
        waveB.AddPeak(1, -2, false);
        PrintWaveDetails(waveB, "波B");
        
        Debug.Log("注意：配对后位置0和1的结果方向会根据绝对值大小决定");
        
        Debug.Log("配对计算:");
        Debug.Log("  位置0: 10(true) + 3(false) = 13(true) [绝对值10>3，方向继承true]");
        Debug.Log("  位置1: 5(true) + -2(false) = 3(true) [绝对值5>2，方向继承true]");
        Debug.Log("  预期: 生成1个波，包含位置0和1（都是true方向）");
        
        List<Wave> results = WavePairing.PairWaves(waveA, waveB);
        Debug.Log($"配对结果: 生成 {results.Count} 个新波");
        
        for (int i = 0; i < results.Count; i++)
        {
            PrintWaveDetails(results[i], $"结果波{i + 1}");
        }
        
        Wave waveTrue = results.Find(w => w.HasPeakAt(0));
        
        bool pass = results.Count == 1 && 
                   waveTrue != null && waveTrue.GetPeak(0).Value == 13 &&
                   waveTrue.GetPeak(1).Value == 3;
        
        Debug.Log(pass ? "<color=green>✓ 测试通过</color>" : "<color=red>✗ 测试失败</color>");
    }

    [ContextMenu("测试17: 波方向一致性检查")]
    public void TestWaveDirectionConsistency()
    {
        Debug.Log("========== [测试17] 波方向一致性检查 ==========");
        
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

    [ContextMenu("测试18: 波特定位置波峰插入")]
    public void TestWavePeakInsertion()
    {
        Debug.Log("========== [测试18] 波特定位置波峰插入 ==========");
        
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

    [ContextMenu("测试19: 根据给定数据生成波")]
    public void TestWaveFromData()
    {
        Debug.Log("========== [测试19] 根据给定数据生成波 ==========");
        
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
