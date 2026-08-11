namespace MediPro.Api.Configuration;

public sealed class NotificationOptions
{
    public const string SectionName = "Notifications";

    /// <summary>When true and SMTP is configured, admin alert emails are sent.</summary>
    public bool EmailEnabled { get; set; }

    /// <summary>Extra recipients in addition to distributor admin user emails.</summary>
    public string[] AdditionalAdminEmails { get; set; } = [];

    /// <summary>Used in email bodies for admin console links (no trailing slash).</summary>
    public string AppBaseUrl { get; set; } = "http://localhost:5173";

    public string SmtpHost { get; set; } = "";
    public int SmtpPort { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string FromEmail { get; set; } = "noreply@medipro.local";
    public string FromDisplayName { get; set; } = "MediPro";

    /// <summary>Hours a store may stay pending before a one-time reminder is sent.</summary>
    public int PendingStoreReminderHours { get; set; } = 24;

    public bool PendingStoreReminderEnabled { get; set; } = true;

    /// <summary>How often the background job scans for overdue pending stores.</summary>
    public int ReminderCheckIntervalMinutes { get; set; } = 15;

    /// <summary>Free WhatsApp alerts via CallMeBot to one admin phone (personal use).</summary>
    public bool WhatsAppEnabled { get; set; }

    /// <summary>International phone without +, e.g. 923001234567.</summary>
    public string? CallMeBotPhone { get; set; }

    public string? CallMeBotApiKey { get; set; }
}
