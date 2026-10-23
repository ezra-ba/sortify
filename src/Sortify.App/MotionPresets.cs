namespace Sortify.App;

/// <summary>Shared motion intent for controls that need physics-like feedback.</summary>
public sealed record MotionPreset(
    double Stiffness,
    double Damping,
    double InitialVelocity,
    double AnimationScale = 1d);

public static class MotionPresets
{
    public static MotionPreset QuickFeedback { get; } = new(420, 34, 0, 1);
    public static MotionPreset SoftSpring { get; } = new(260, 28, 0, 1);
    public static MotionPreset GentleSettle { get; } = new(210, 32, 0, 1);
    public static MotionPreset ReducedMotion { get; } = new(1, 1, 0, 0);
}
