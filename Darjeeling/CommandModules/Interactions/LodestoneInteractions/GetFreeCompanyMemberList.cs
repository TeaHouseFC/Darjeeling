using Darjeeling.Helpers;
using Darjeeling.Interfaces;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Darjeeling.CommandModules.Interactions;

public class GetFreeCompanyMemberList : ApplicationCommandModule<SlashCommandContext>
{
    private readonly ILogger<GetFreeCompanyMemberList> _logger;
    private readonly ILodestoneApi _lodestoneApi;
    public GetFreeCompanyMemberList(ILogger<GetFreeCompanyMemberList> logger, ILodestoneApi lodestoneApi)
    {
        _logger = logger;
        _lodestoneApi = lodestoneApi;
    }

    [SlashCommand("getfreecompanylist", "Returns a list of the members within the FC from the lodestone")]
    public async Task ReturnFreeCompanyMemberList([SlashCommandParameter(Name = "fcid", Description = "Free Company ID")] string fcid)
    {
        try
        {
            _logger.LogActionTraceStart(Context, "ReturnFreeCompanyMemberList");
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            
            var webResult = await _lodestoneApi.GetLodestoneFreeCompanyMembersAsync(fcid);
            
            if (webResult.Success == false)
            {
                await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
                {
                    Content = webResult.Error
                });
            }
            else
            {
                var combinedMemberNames = string.Join(", ", webResult.Members.Select(x => x.FullName));
                await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
                {
                    Content = $"Free Company Member List for {fcid} is: {combinedMemberNames}"
                });
            }
            _logger.LogActionTraceFinish(Context, "ReturnFreeCompanyMemberList");
        } catch (Exception error) 
        {
            _logger.LogExceptionError(Context, "ReturnFreeCompanyMemberList", error);
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = $"Error occured when running getfreecompanylist command"
            });
        }
    }
}
