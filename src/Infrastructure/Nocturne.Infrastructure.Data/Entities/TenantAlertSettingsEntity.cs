using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nocturne.Infrastructure.Data.Entities;

/// <summary>
/// Tenant-level alert configuration: Do Not Disturb (manual + scheduled). One row per
/// tenant; the row is created on first access. The scheduled DND window is interpreted
/// in the patient's IANA timezone (<see cref="V4.PatientRecordEntity.Timezone"/>) — not
/// stored here. DND has two activation paths that share the same allowlist semantics —
/// the per-rule <see cref="AlertRuleEntity.AllowThroughDnd"/> bypass applies to both.
/// </summary>
[Table("tenant_alert_settings")]
public class TenantAlertSettingsEntity : ITenantScoped
{
    /// <summary>Unique identifier for the row. PK; one row per tenant via the unique index.</summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>Identifier of the tenant this configuration belongs to.</summary>
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    // ---- Manual DND ----

    /// <summary>True when the user has manually toggled DND on.</summary>
    [Column("dnd_manual_active")]
    public bool DndManualActive { get; set; }

    /// <summary>
    /// UTC instant at which a manually-activated DND auto-expires. Null when DND is on
    /// indefinitely. The engine treats DND as off when <c>now &gt;= dnd_manual_until</c>.
    /// </summary>
    [Column("dnd_manual_until")]
    public DateTime? DndManualUntil { get; set; }

    /// <summary>UTC instant at which DND was most recently activated. Used to anchor
    /// <c>do_not_disturb</c> conditions with a sustained <c>for_minutes</c>.</summary>
    [Column("dnd_manual_started_at")]
    public DateTime? DndManualStartedAt { get; set; }

    // ---- Scheduled DND ----

    /// <summary>True when a recurring scheduled DND window is configured.</summary>
    [Column("dnd_schedule_enabled")]
    public bool DndScheduleEnabled { get; set; }

    /// <summary>Local-time start of the scheduled DND window (in the patient's timezone).</summary>
    [Column("dnd_schedule_start")]
    public TimeOnly? DndScheduleStart { get; set; }

    /// <summary>Local-time end of the scheduled DND window. Cross-midnight windows are allowed
    /// (start &gt; end interpreted as wrapping over midnight).</summary>
    [Column("dnd_schedule_end")]
    public TimeOnly? DndScheduleEnd { get; set; }

    /// <summary>When the row was created.</summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the row was last updated.</summary>
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
