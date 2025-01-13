namespace Darjeeling.Models.LodestoneEntities;

public class LodestoneFCMember
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CharacterId { get; set; }

    public string FullName => FirstName + " " + LastName;
}