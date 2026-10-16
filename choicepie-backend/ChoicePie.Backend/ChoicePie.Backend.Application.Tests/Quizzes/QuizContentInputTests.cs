using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Quizzes;
using ChoicePie.Backend.Application.Quizzes.Commands;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class QuizContentInputTests
{
    [TestCase("https://example.com/a")]
    [TestCase("  http://example.com  ")]
    [TestCase("HTTPS://EXAMPLE.COM/PATH?q=1")]
    public void ParseUrlOrNull_GivenSingleHttpUrl_WhenParsed_ThenReturnsUri(string input)
    {
        Assert.That(QuizContentInput.ParseUrlOrNull(input), Is.Not.Null);
    }

    [TestCase("這是一段關於光合作用的文字，長度超過三十個字元，不是網址而是一般內容。")]
    [TestCase("看看 https://example.com/a 這個網站的內容怎麼樣，是不是很有趣呢哈哈哈")]
    [TestCase("https://example.com/a 後面還有其他文字")]
    [TestCase("")]
    public void ParseUrlOrNull_GivenPlainText_WhenParsed_ThenReturnsNull(string input)
    {
        Assert.That(QuizContentInput.ParseUrlOrNull(input), Is.Null);
    }

    [TestCase("ftp://example.com/x")]
    [TestCase("file:///etc/passwd")]
    [TestCase("http://")]
    [TestCase("javascript://x")]
    public void ParseUrlOrNull_GivenNonHttpUrlLikeInput_WhenParsed_ThenThrowsUnsupported(string input)
    {
        Assert.Throws<UnsupportedWebPageUrlException>(() => QuizContentInput.ParseUrlOrNull(input));
    }

    [TestCase("https://a.co/x", true)]
    [TestCase("short text", false)]
    [TestCase("exactly thirty characters long!", true)]
    public void MinContentLengthUnlessUrl_GivenContent_WhenValidated_ThenOnlyShortNonUrlTextFails(string content, bool expected)
    {
        var command = new GenerateQuizQuestionsCommand { Content = content, QuestionCount = 5, Difficulty = "beginner" };

        var valid = Validator.TryValidateObject(command, new ValidationContext(command), [], validateAllProperties: true);

        Assert.That(valid, Is.EqualTo(expected));
    }
}
