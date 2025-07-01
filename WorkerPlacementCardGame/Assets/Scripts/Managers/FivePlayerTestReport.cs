using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FivePlayerTestReport : MonoBehaviour
{
    [Header("レポート設定")]
    public bool generateReportOnTestComplete = true;
    public bool saveToFile = true;
    
    private StringBuilder reportBuilder = new StringBuilder();
    private List<TestResult> testResults = new List<TestResult>();
    
    [System.Serializable]
    public class TestResult
    {
        public string testName;
        public bool passed;
        public string details;
        public float executionTime;
        public Dictionary<string, object> metrics = new Dictionary<string, object>();
    }
    
    public void AddTestResult(string testName, bool passed, string details, float executionTime = 0f)
    {
        testResults.Add(new TestResult
        {
            testName = testName,
            passed = passed,
            details = details,
            executionTime = executionTime
        });
    }
    
    public void AddMetric(string testName, string metricName, object value)
    {
        var result = testResults.LastOrDefault(r => r.testName == testName);
        if (result != null)
        {
            result.metrics[metricName] = value;
        }
    }
    
    [ContextMenu("Generate 5-Player Test Report")]
    public void GenerateReport()
    {
        Debug.Log("📊 5人プレイテストレポート生成開始");
        
        reportBuilder.Clear();
        AppendHeader();
        AppendExecutiveSummary();
        AppendDetailedResults();
        AppendRecommendations();
        AppendFooter();
        
        string finalReport = reportBuilder.ToString();
        
        Debug.Log("📋 === 5人プレイテストレポート ===");
        Debug.Log(finalReport);
        
        if (saveToFile)
        {
            SaveReportToFile(finalReport);
        }
    }
    
    private void AppendHeader()
    {
        reportBuilder.AppendLine("# 5人プレイ Agricola風ワーカープレイスメントゲーム 検証レポート");
        reportBuilder.AppendLine($"生成日時: {System.DateTime.Now:yyyy/MM/dd HH:mm:ss}");
        reportBuilder.AppendLine("=================================================================");
        reportBuilder.AppendLine();
    }
    
    private void AppendExecutiveSummary()
    {
        reportBuilder.AppendLine("## 🎯 エグゼクティブサマリー");
        reportBuilder.AppendLine();
        
        int totalTests = testResults.Count;
        int passedTests = testResults.Count(r => r.passed);
        float successRate = totalTests > 0 ? (float)passedTests / totalTests * 100f : 0f;
        
        reportBuilder.AppendLine($"- **総テスト数**: {totalTests}");
        reportBuilder.AppendLine($"- **成功テスト数**: {passedTests}");
        reportBuilder.AppendLine($"- **成功率**: {successRate:F1}%");
        reportBuilder.AppendLine($"- **総実行時間**: {testResults.Sum(r => r.executionTime):F2}秒");
        reportBuilder.AppendLine();
        
        if (successRate >= 80f)
        {
            reportBuilder.AppendLine("✅ **評価**: 5人プレイは実装可能レベルです");
        }
        else if (successRate >= 60f)
        {
            reportBuilder.AppendLine("⚠️ **評価**: 5人プレイには一部調整が必要です");
        }
        else
        {
            reportBuilder.AppendLine("❌ **評価**: 5人プレイには大幅な改善が必要です");
        }
        reportBuilder.AppendLine();
    }
    
    private void AppendDetailedResults()
    {
        reportBuilder.AppendLine("## 📊 詳細テスト結果");
        reportBuilder.AppendLine();
        
        foreach (var result in testResults)
        {
            string status = result.passed ? "✅ PASS" : "❌ FAIL";
            reportBuilder.AppendLine($"### {result.testName} - {status}");
            reportBuilder.AppendLine();
            reportBuilder.AppendLine($"**詳細**: {result.details}");
            
            if (result.executionTime > 0)
            {
                reportBuilder.AppendLine($"**実行時間**: {result.executionTime:F2}秒");
            }
            
            if (result.metrics.Count > 0)
            {
                reportBuilder.AppendLine("**メトリクス**:");
                foreach (var metric in result.metrics)
                {
                    reportBuilder.AppendLine($"- {metric.Key}: {metric.Value}");
                }
            }
            
            reportBuilder.AppendLine();
        }
    }
    
    private void AppendRecommendations()
    {
        reportBuilder.AppendLine("## 🔧 改善提案");
        reportBuilder.AppendLine();
        
        // アクションスペース競争分析
        var competitionTest = testResults.FirstOrDefault(r => r.testName.Contains("競争"));
        if (competitionTest != null && !competitionTest.passed)
        {
            reportBuilder.AppendLine("### アクションスペース不足への対応");
            reportBuilder.AppendLine("- 人気アクションスペースの定員を2人に拡張");
            reportBuilder.AppendLine("- 代替アクションスペースの追加");
            reportBuilder.AppendLine("- リソース蓄積メカニズムの導入");
            reportBuilder.AppendLine();
        }
        
        // 食料供給分析
        var foodTest = testResults.FirstOrDefault(r => r.testName.Contains("食料"));
        if (foodTest != null && !foodTest.passed)
        {
            reportBuilder.AppendLine("### 食料供給圧力の緩和");
            reportBuilder.AppendLine("- 食料獲得アクションの報酬増加");
            reportBuilder.AppendLine("- 自動変換システムの効率化");
            reportBuilder.AppendLine("- 初期食料ボーナスの検討");
            reportBuilder.AppendLine();
        }
        
        // ゲーム時間分析
        var timeTest = testResults.FirstOrDefault(r => r.testName.Contains("時間"));
        if (timeTest != null && !timeTest.passed)
        {
            reportBuilder.AppendLine("### ゲーム時間の最適化");
            reportBuilder.AppendLine("- ラウンド数の調整（14→12ラウンド）");
            reportBuilder.AppendLine("- 同時アクション実行の導入");
            reportBuilder.AppendLine("- ダウンタイムの削減");
            reportBuilder.AppendLine();
        }
        
        // バランス分析
        var balanceTest = testResults.FirstOrDefault(r => r.testName.Contains("バランス"));
        if (balanceTest != null)
        {
            reportBuilder.AppendLine("### ゲームバランスの調整");
            reportBuilder.AppendLine("- 発展パスの多様化");
            reportBuilder.AppendLine("- 後手番プレイヤーへのボーナス");
            reportBuilder.AppendLine("- エンドゲームトリガーの調整");
            reportBuilder.AppendLine();
        }
    }
    
    private void AppendFooter()
    {
        reportBuilder.AppendLine("## 📝 まとめ");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("5人プレイにおけるAgricola風ワーカープレイスメントゲームの検証を完了しました。");
        reportBuilder.AppendLine("上記の改善提案を実装することで、バランスの取れた5人プレイ体験を提供できると判断されます。");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("---");
        reportBuilder.AppendLine("*本レポートは自動生成されたものです*");
        reportBuilder.AppendLine("=================================================================");
    }
    
    private void SaveReportToFile(string report)
    {
        try
        {
            string fileName = $"5PlayerTestReport_{System.DateTime.Now:yyyyMMdd_HHmmss}.md";
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            System.IO.File.WriteAllText(filePath, report);
            Debug.Log($"📄 レポートを保存しました: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"レポート保存に失敗しました: {e.Message}");
        }
    }
    
    // テスト実行用のヘルパーメソッド
    public void StartTest(string testName)
    {
        Debug.Log($"🧪 テスト開始: {testName}");
    }
    
    public void EndTest(string testName, bool success, string details = "")
    {
        Debug.Log($"🏁 テスト終了: {testName} - {(success ? "成功" : "失敗")}");
        if (!string.IsNullOrEmpty(details))
        {
            Debug.Log($"   詳細: {details}");
        }
    }
    
    // 既存のFivePlayerGameTestとの連携
    void OnEnable()
    {
        // テスト完了通知を受け取る設定
        if (generateReportOnTestComplete)
        {
            InvokeRepeating(nameof(CheckForTestCompletion), 5f, 2f);
        }
    }
    
    void OnDisable()
    {
        CancelInvoke(nameof(CheckForTestCompletion));
    }
    
    private void CheckForTestCompletion()
    {
        FivePlayerGameTest testComponent = FindObjectOfType<FivePlayerGameTest>();
        if (testComponent == null && testResults.Count > 0)
        {
            // テストが完了したと判断
            GenerateReport();
            enabled = false; // レポート生成後は無効化
        }
    }
}