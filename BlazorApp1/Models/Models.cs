// =====================================================================
// Models.cs  —  니트로그 앱 전체 데이터 모델
// =====================================================================
using System;
using System.Collections.Generic;

namespace KnitLog.Models
{
    // ─────────────────────────────────────────────
    // 공통: 카테고리 열거형
    // ─────────────────────────────────────────────
    public enum NeedleType { 대바늘, 코바늘 }
    public enum ProjectStatus { 진행중, 완료, 위시리스트 }
    public enum YarnWeight { 레이스, 핑거, 스포츠, DK, 워스티드, 벌키, 슈퍼벌키 }

    // ─────────────────────────────────────────────
    // 실 (Yarn)
    // ─────────────────────────────────────────────
    public class Yarn
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";          // 실 이름
        public string Brand { get; set; } = "";         // 브랜드
        public string Color { get; set; } = "";         // 색상명
        public string ColorCode { get; set; } = "#ffffff"; // 색상 코드
        public YarnWeight Weight { get; set; } = YarnWeight.워스티드;
        public string Material { get; set; } = "";      // 소재 (울, 면, 아크릴 등)
        public int WeightGram { get; set; }             // 무게(g)
        public int LengthMeter { get; set; }            // 길이(m)
        public string PurchasePlace { get; set; } = ""; // 판매처
        public decimal Price { get; set; }              // 가격
        public int Quantity { get; set; } = 1;          // 보유 수량(볼)
        public string Memo { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // ─────────────────────────────────────────────
    // 도구 (Tool - 바늘)
    // ─────────────────────────────────────────────
    public class KnitTool
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public NeedleType NeedleType { get; set; } = NeedleType.대바늘;
        public string Brand { get; set; } = "";
        public double SizeMm { get; set; }              // 몇 mm
        public string Material { get; set; } = "";      // 소재 (대나무, 알루미늄 등)
        public int LengthCm { get; set; }               // 길이(cm), 대바늘만
        public string Memo { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // ─────────────────────────────────────────────
    // 뜨개 과정 사진
    // ─────────────────────────────────────────────
    public class ProjectPhoto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; } = "";
        public string Base64Data { get; set; } = "";    // 이미지를 base64로 저장
        public string Caption { get; set; } = "";
        public DateTime TakenAt { get; set; } = DateTime.Now;
    }

    // ─────────────────────────────────────────────
    // 뜨개 프로젝트 (뜨케줄 / 완성작 / 위시리스트 공통)
    // ─────────────────────────────────────────────
    public class KnitProject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ProjectStatus Status { get; set; } = ProjectStatus.위시리스트;

        // 기본 정보
        public string Title { get; set; } = "";         // 작품명
        public string Description { get; set; } = "";   // 설명
        public string PatternName { get; set; } = "";   // 도안명
        public string PatternSource { get; set; } = ""; // 도안 출처 (URL or 책명)
        public string PatternMemo { get; set; } = "";   // 도안 메모

        // 날짜
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 사용 재료 (ID 참조)
        public List<ProjectYarnUsage> YarnUsages { get; set; } = new();
        public List<Guid> ToolIds { get; set; } = new();

        // 사진
        public List<ProjectPhoto> Photos { get; set; } = new();

        // 카운터 & 체크리스트
        public List<ProjectCounter> Counters { get; set; } = new();
        public List<ChecklistItem> Checklist { get; set; } = new();

        // 시간 기록
        public List<KnitSession> Sessions { get; set; } = new();

        // 메모
        public string Memo { get; set; } = "";

        // 위시리스트 전용
        public string WishMemo { get; set; } = "";
    }

    // 프로젝트에서 사용한 실 (실 ID + 사용량)
    public class ProjectYarnUsage
    {
        public Guid YarnId { get; set; }
        public int BallsUsed { get; set; }
        public string Memo { get; set; } = "";
    }

    // ─────────────────────────────────────────────
    // 단수 카운터
    // ─────────────────────────────────────────────
    public class ProjectCounter
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Label { get; set; } = "단수";  // 카운터 이름
        public int Value { get; set; } = 0;           // 현재 값
        public int Step { get; set; } = 1;            // +/- 단위
    }

    // ─────────────────────────────────────────────
    // 도안 체크리스트 항목
    // ─────────────────────────────────────────────
    public class ChecklistItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Text { get; set; } = "";
        public bool Done { get; set; } = false;
    }

    // ─────────────────────────────────────────────
    // 뜨개 시간 기록 (세션)
    // ─────────────────────────────────────────────
    public class KnitSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime StartTime { get; set; }        // 시작 시간
        public DateTime? EndTime { get; set; }         // 종료 시간 (진행 중이면 null)
        public string Memo { get; set; } = "";         // 세션 메모

        // 소요 시간 (분 단위로 반환)
        public int GetDurationMinutes()
        {
            var end = EndTime ?? DateTime.Now;
            return (int)(end - StartTime).TotalMinutes;
        }

        // 포맷팅된 시간 표시 (예: "1시간 30분")
        public string GetFormattedDuration()
        {
            var minutes = GetDurationMinutes();
            var hours = minutes / 60;
            var mins = minutes % 60;
            
            if (hours > 0)
                return mins > 0 ? $"{hours}시간 {mins}분" : $"{hours}시간";
            return $"{mins}분";
        }

        // 현재 진행 중인지 확인
        public bool IsActive => !EndTime.HasValue;
    }
}
