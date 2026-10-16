using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Specifications;

public sealed class PublishedQuizzesByCreatorSpecification(Guid creatorId)
    : Specification<Quiz>(q => q.CreatorId == creatorId && q.Status == QuizStatus.Published);
