using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Shared.Kernel.Tests.Primitives;

public sealed record TestStatus(int Id, string Name) : Enumeration<TestStatus>(Id, Name)
{
    public static readonly TestStatus Active = new(1, "active");
    public static readonly TestStatus Inactive = new(2, "inactive");
}

[TestFixture]
public class EnumerationTests
{
    [Test]
    public void Enumerations_WhenRead_ThenContainsAllDeclaredStaticFields()
    {
        Assert.That(TestStatus.Enumerations.Values, Is.EquivalentTo(new[] { TestStatus.Active, TestStatus.Inactive }));
    }

    [Test]
    public void FromValue_GivenKnownId_WhenCalled_ThenReturnsMatchingInstance()
    {
        Assert.That(TestStatus.FromValue(1), Is.SameAs(TestStatus.Active));
    }

    [Test]
    public void FromValue_GivenUnknownId_WhenCalled_ThenReturnsNull()
    {
        Assert.That(TestStatus.FromValue(999), Is.Null);
    }

    [Test]
    public void FromName_GivenKnownNameDifferentCase_WhenCalled_ThenReturnsMatchingInstanceCaseInsensitively()
    {
        Assert.That(TestStatus.FromName("ACTIVE"), Is.SameAs(TestStatus.Active));
    }

    [Test]
    public void FromName_GivenUnknownName_WhenCalled_ThenReturnsNull()
    {
        Assert.That(TestStatus.FromName("nonexistent"), Is.Null);
    }

    [Test]
    public void ToString_GivenDerivedRecordWithAPrimaryConstructor_WhenCalled_ThenCompilerGeneratedRecordToStringWinsOverBaseOverride()
    {
        // Enumeration<TEnum>.ToString() 明確 override 成回傳 Name，但因為每個具體的列舉（TestStatus 這裡、
        // 或 product code 裡的 Difficulty/QuizStatus/AdminRole 等）都宣告成「帶 primary constructor 的
        // record」，編譯器會自動幫該 record 產生自己的 ToString()（印出 "TypeName { Prop = Value, ... }"），
        // 這個自動產生的版本會蓋掉 base class 手寫的 override——所以 Enumeration<TEnum>.ToString() 實際上
        // 從未真的被呼叫過。目前沒有 product code 依賴它印出純 Name，所以是無害的死程式碼，這裡把目前的
        // 真實行為釘住，而不是斷言原本（錯誤）以為的行為。
        Assert.That(TestStatus.Active.ToString(), Is.EqualTo("TestStatus { Id = 1, Name = active }"));
    }

    [Test]
    public void GetHashCode_GivenSameId_WhenCalled_ThenReturnsSameHashCode()
    {
        Assert.That(TestStatus.Active.GetHashCode(), Is.EqualTo(TestStatus.Active.Id.GetHashCode()));
    }
}
