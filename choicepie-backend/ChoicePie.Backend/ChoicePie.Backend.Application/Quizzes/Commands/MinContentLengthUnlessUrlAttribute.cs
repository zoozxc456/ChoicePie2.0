using System.ComponentModel.DataAnnotations;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

/// <summary>
/// 純文字內容至少 <see cref="QuizContentInput.MinContentLength"/> 字元；網址輸入略過此規則
/// （短網址仍合法，抓取後再套用最小長度檢查）。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MinContentLengthUnlessUrlAttribute()
    : ValidationAttribute($"The field {{0}} must be a string with a minimum length of {QuizContentInput.MinContentLength}.")
{
    public override bool IsValid(object? value)
        => value is not string text
           || QuizContentInput.LooksLikeUrl(text)
           || text.Length >= QuizContentInput.MinContentLength;
}
