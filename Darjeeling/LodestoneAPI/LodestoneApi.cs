using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Darjeeling.Interfaces;
using Darjeeling.Models;
using Darjeeling.Models.LodestoneEntities;

namespace Darjeeling.Helpers.LodestoneHelpers;

// Currently just uses mostly css selector parsing for elements
// TODO - Refactor to use AngleSharp Parsing

// TODO - Create FC Name Getter while using FCID Only to Inject into GetFreeCompanyMemberList
public class LodestoneApi : ILodestoneApi
{
    
    private string GenerateCharacterSearchQueryURL(string firstName, string lastName, string world)
    {
        // Example Query In Browser
        // https://na.finalfantasyxiv.com/lodestone/character/?q=Art+Bayard&worldname=Carbuncle&classjob=&race_tribe=&blog_lang=ja&blog_lang=en&blog_lang=de&blog_lang=fr&order=
        var baseCharacterSearchURL = "https://na.finalfantasyxiv.com/lodestone/character/?q=";
        var endQueryURL = $"&worldname={world}&classjob=&race_tribe=&blog_lang=ja&blog_lang=en&blog_lang=de&blog_lang=fr&order=";
        return $"{baseCharacterSearchURL}{firstName}+{lastName}{endQueryURL}";
    }

    private string GenerateFreeCompanySearchQueryURL(string freeCompanyId)
    {
        // Example Query In Browser
        // https://na.finalfantasyxiv.com/lodestone/freecompany/9229705223830889096/member/?page=1
        return $"https://na.finalfantasyxiv.com/lodestone/freecompany/{freeCompanyId}/member/?page=";
    }

    public async Task<WebResult> GetLodestoneFreeCompanyAsync(string firstName, string lastName, string world)
    {
        
        try
        {
            var queryUrl = GenerateCharacterSearchQueryURL(firstName, lastName, world);
            var content = await GetLodestoneWebPageContentAsync(queryUrl);

            if (string.IsNullOrEmpty(content))
            {
                return new WebResult { Success = false, ResultValue = "Unable to get web content" };
            }

            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(content));

            var linkElement = document.QuerySelector("a.entry__link");
            if (linkElement == null)
            {
                return new WebResult { Success = false, ResultValue = "No Character found with this name on the specified world" };
            }

            var href = linkElement.GetAttribute("href");
            if (string.IsNullOrEmpty(href))
            {
                return new WebResult { Success = false, ResultValue = "Error Getting Character URL" };
            }

            var match = Regex.Match(href, @"/lodestone/character/(\d+)/");
            if (!match.Success)
            {
                return new WebResult { Success = false, ResultValue = "Error Parsing Character ID" };
            }
            
            var freeCompanyElement = document.QuerySelector("a.entry__freecompany__link span");
            if (freeCompanyElement == null)
            {
                return new WebResult { Success = false, ResultValue = "Character not in a free company" };
            }

            string freeCompany = freeCompanyElement.TextContent;
            return new WebResult { Success = true, ResultValue = freeCompany };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error getting Free Company: " + ex);
            return new WebResult { Success = false, ResultValue = $"Error: {ex.Message}" };
        }
    }

    public async Task<WebResult> GetLodestoneCharacterIdAsync(string firstName, string lastName, string world)
    {
        
        try
        {
            var queryUrl = GenerateCharacterSearchQueryURL(firstName, lastName, world);
            var content = await GetLodestoneWebPageContentAsync(queryUrl);

            if (string.IsNullOrEmpty(content))
            {
                return new WebResult { Success = false, ResultValue = "Unable to get web content" };
            }

            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(content));

            var linkElement = document.QuerySelector("a.entry__link");
            if (linkElement == null)
            {
                return new WebResult { Success = false, ResultValue = "No Character found with this name on the specified world" };
            }

            var href = linkElement.GetAttribute("href");
            var match = Regex.Match(href, @"/lodestone/character/(\d+)/");

            if (!match.Success)
            {
                return new WebResult { Success = false, ResultValue = "Error Parsing Character ID" };
            }

            return new WebResult { Success = true, ResultValue = match.Groups[1].Value };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Exception getting Character ID: " + ex);
            return new WebResult { Success = false, 
                ResultValue = $"Error Exception getting character ID, please check logs for more information" };
        }
    }

    public async Task<LodestoneFCMemberList> GetLodestoneFreeCompanyMembersAsync(string fcid)
    {
        var memberList = new List<LodestoneFCMember>();
        string initialUrl = GenerateFreeCompanySearchQueryURL(fcid);

        string content = await GetLodestoneWebPageContentAsync(initialUrl);
        if (string.IsNullOrEmpty(content))
        {
            return new LodestoneFCMemberList { Success = false, Error = "Error getting initial Free Company page", Members = new List<LodestoneFCMember>() };
        }

        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(content));

        int totalPageCount = CalculateTotalMemberPageCount(document);

        if (totalPageCount == 0)
        {
            return new LodestoneFCMemberList { Success = false, Error = "Error calculating page count", Members = new List<LodestoneFCMember>() };
        }

        for (int currentPage = 1; currentPage <= totalPageCount; currentPage++)
        {
            string urlQuery = $"{initialUrl}{currentPage}";
            memberList.AddRange(await GetMembersFromFreeCompanyProfileAsync(urlQuery));
        }

        return new LodestoneFCMemberList { Success = true, Error = "", Members = memberList };
    }

    private async Task<List<LodestoneFCMember>> GetMembersFromFreeCompanyProfileAsync(string url)
    {
        var members = new List<LodestoneFCMember>();

        string content = await GetLodestoneWebPageContentAsync(url);
        if (string.IsNullOrEmpty(content))
        {
            return members;
        }

        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(content));

        foreach (var element in document.QuerySelectorAll("a.entry__bg"))
        {
            var href = element.GetAttribute("href");
            if (string.IsNullOrEmpty(href)) continue;

            var nameElement = element.QuerySelector(".entry__name");
            if (nameElement == null) continue;

            var fullName = nameElement.TextContent;
            var names = fullName.Split(' ');
            if (names.Length < 2) continue;

            var match = Regex.Match(href, @"/lodestone/character/(\d+)/");
            if (!match.Success) continue;

            members.Add(new LodestoneFCMember
            {
                FirstName = names[0],
                LastName = names[1],
                CharacterId = match.Groups[1].Value
            });
        }

        return members;
    }

    private int CalculateTotalMemberPageCount(IDocument document)
    {
        var pagerElement = document.QuerySelector("li.btn__pager__current");
        if (pagerElement == null) return 0;

        var match = Regex.Match(pagerElement.TextContent, @"Page \d+ of (\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private async Task<string> GetLodestoneWebPageContentAsync(string url)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching web content: " + ex);
            return "";
        }
    }



}