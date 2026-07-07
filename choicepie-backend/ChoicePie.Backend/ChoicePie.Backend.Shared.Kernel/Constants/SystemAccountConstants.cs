namespace ChoicePie.Backend.Shared.Kernel.Constants;

public static class SystemAccountConstants
{
    // System Root
    public static readonly Guid SystemRootId = Guid.Parse("4CA517BD-A091-404B-94AC-DD1A70125CDD");
    
    public static bool IsRoot(Guid id) => id == SystemRootId;
}