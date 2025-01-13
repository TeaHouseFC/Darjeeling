namespace Darjeeling.Models.LodestoneEntities;

public class LodestoneFCMemberList
{
    public bool Success { get; set; }
    public string Error { get; set; }
    public List<LodestoneFCMember> Members { get; set; }
}