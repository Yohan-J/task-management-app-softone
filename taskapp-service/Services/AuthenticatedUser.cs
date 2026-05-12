namespace backend.Services;

/// <summary>
/// User identity returned after successful credential validation (no secrets).
/// </summary>
public sealed record AuthenticatedUser(int Id, string Username);
