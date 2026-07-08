#nullable enable
namespace Domain.Model.WorldEvents
{
    public record DeviceSkillSource(string DeviceName, DeviceKind Kind) : SkillSource;
}
