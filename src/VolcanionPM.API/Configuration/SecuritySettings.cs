namespace VolcanionPM.API.Configuration;

public class SecuritySettings
{
    public CorsSettings Cors { get; set; } = new();
    public PasswordPolicySettings PasswordPolicy { get; set; } = new();
    public AccountLockoutSettings AccountLockout { get; set; } = new();
}

public class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public bool AllowCredentials { get; set; }
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
    public string[] ExposedHeaders { get; set; } = Array.Empty<string>();
    public int MaxAgeSeconds { get; set; }
}

public class PasswordPolicySettings
{
    public bool RequireDigit { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireNonAlphanumeric { get; set; }
    public int RequiredLength { get; set; }
    public int RequiredUniqueChars { get; set; }
}

public class AccountLockoutSettings
{
    public int MaxFailedAttempts { get; set; }
    public int LockoutDurationMinutes { get; set; }
    public int ResetFailedAttemptsAfterMinutes { get; set; }
}
