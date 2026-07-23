using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Enums;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Specifications;

public sealed class QuizAttemptInProgressByQuizAndMemberSpecification(Guid quizId, Guid memberId)
    : Specification<QuizAttempt>(a => a.QuizId == quizId && a.MemberId == memberId && a.Status == AttemptStatus.InProgress);
