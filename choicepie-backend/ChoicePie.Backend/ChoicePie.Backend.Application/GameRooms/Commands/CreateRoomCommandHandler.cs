using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class CreateRoomCommandHandler(
    IQuizRepository quizRepository,
    IGameRoomRepository gameRoomRepository,
    TimeProvider timeProvider)
    : IRequestHandler<CreateRoomCommand, CreateRoomResultDto>
{
    public async Task<CreateRoomResultDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken)
                   ?? throw new QuizNotFoundException(request.QuizId);

        // 已發布的題庫任何人都能開房；草稿只有擁有者能開（試玩）。其餘狀態（封存、下架）一律視為不存在，
        // 與 GetQuizByIdQueryHandler 一致，避免透過 QuestionStart/QuestionEnd 洩漏題目與答案。
        var isOwner = quiz.OwnerId == request.HostUserId;
        var isHostable = quiz.Status == QuizStatus.Published || (isOwner && quiz.Status == QuizStatus.Draft);
        if (!isHostable)
        {
            throw new QuizNotFoundException(request.QuizId);
        }

        var selectedQuestions = request.QuestionIds.Count == 0
            ? quiz.Questions
            : quiz.Questions.Where(q => request.QuestionIds.Contains(q.Id)).ToList();

        var questions = selectedQuestions
            .Select(q => new GameQuestionSnapshot(q.Id, q.Text, q.Options, q.AnswerIndex, q.Explanation))
            .ToList();

        var createdAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        // 房號的唯一性由 TryAddAsync 原子保證：撞號時換一個房號重試，不會覆蓋別人的房間。
        GameRoom room;
        do
        {
            room = GameRoom.Create(
                request.HostUserId, RoomCodeGenerator.Generate(), quiz.Id, quiz.Title, quiz.Cover.ImageUrl,
                quiz.Cover.Emoji, quiz.Cover.Gradient, questions, request.TimeLimitSeconds, createdAtUtc);

            if (request.HostConnectionId is not null)
            {
                room.AttachHostConnection(request.HostUserId, request.HostConnectionId);
            }
        } while (!await gameRoomRepository.TryAddAsync(room, cancellationToken));

        return new CreateRoomResultDto(room.RoomCode);
    }
}
