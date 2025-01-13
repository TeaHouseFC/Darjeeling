using Darjeeling.Models;
using Darjeeling.Models.LodestoneEntities;

namespace Darjeeling.Interfaces;

public interface ILodestoneApi
{
    Task<WebResult> GetLodestoneFreeCompanyAsync(string firstName, string lastName, string world);
    Task<WebResult> GetLodestoneCharacterIdAsync(string firstName, string lastName, string world);
    Task<LodestoneFCMemberList> GetLodestoneFreeCompanyMembersAsync(string fcid);

}