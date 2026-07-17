using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
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

        var selectedQuestions = request.QuestionIds.Count == 0
            ? quiz.Questions
            : quiz.Questions.Where(q => request.QuestionIds.Contains(q.Id)).ToList();

        var questions = selectedQuestions
            .Select(q => new GameQuestionSnapshot(q.Id, q.Text, q.Options, q.AnswerIndex, q.Explanation))
            .ToList();

        var roomCode = await GenerateUniqueRoomCodeAsync(cancellationToken);

        var room = GameRoom.Create(
            request.HostUserId, roomCode, quiz.Id, quiz.Title, quiz.Cover.Emoji, quiz.Cover.Gradient,
            questions, request.TimeLimitSeconds, timeProvider.GetUtcNow().UtcDateTime);

        await gameRoomRepository.SaveAsync(room, cancellationToken);

        return new CreateRoomResultDto(roomCode);
    }

    private async Task<string> GenerateUniqueRoomCodeAsync(CancellationToken ct)
    {
        string roomCode;
        do
        {
            roomCode = RoomCodeGenerator.Generate();
        } while (await gameRoomRepository.RoomCodeExistsAsync(roomCode, ct));

        return roomCode;
    }
}
