using Darjeeling.Helpers;
using Darjeeling.Interfaces;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Darjeeling.CommandModules.Interactions;

public class GetCharacterFreeCompany : ApplicationCommandModule<SlashCommandContext>
{
    private readonly ILogger<GetCharacterFreeCompany> _logger;
    private readonly ILodestoneApi _lodestoneApi;
    public GetCharacterFreeCompany(ILogger<GetCharacterFreeCompany> logger, ILodestoneApi lodestoneApi)
    {
        _logger = logger;
        _lodestoneApi = lodestoneApi;
    }
    [SlashCommand("getfreecompany", "Returns the Free Company of a user")]
    public async Task ReturnCharacterFreeCompany([SlashCommandParameter(Name = "firstname", Description = "First Name")] string firstName ,
        [SlashCommandParameter(Name = "lastname", Description = "Last Name")] string lastName,
        [SlashCommandParameter(Name = "world", Description = "World")] string world)
    
    {
        try {
            _logger.LogActionTraceStart(Context, "ReturnCharacterFreeCompany");
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            
            var webResult = await _lodestoneApi.GetLodestoneFreeCompanyAsync(firstName, lastName, world);

            if (webResult.Success == false)
            {
                await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
                {
                    Content = webResult.ResultValue
                });
            }
            else
            {
                await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
                {
                    Content = $"Free Company for {firstName} {lastName} on {world} is: {webResult.ResultValue}"
                });
            }
            
            _logger.LogActionTraceFinish(Context, "ReturnCharacterFreeCompany");
        } catch (Exception error) {
            _logger.LogExceptionError(Context, "ReturnCharacterFreeCompany", error);
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = $"Error occured when running getfreecompany command"
            });
        }
    }
}