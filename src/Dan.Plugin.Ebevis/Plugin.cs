using Dan.Common;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Ebevis.Config;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Dan.Plugin.Ebevis;

public class Plugin
{
    private readonly IEvidenceSourceMetadata _evidenceSourceMetadata;
    private readonly ILogger _logger;
    private readonly HttpClient _client;
    private readonly ApplicationSettings _settings;
    private readonly IEntityRegistryService _entityRegistryService;

    // These are not mandatory, but there should be a distinct error code (any integer) for all types of errors that can occur. The error codes does not have to be globally
    // unique. These should be used within either transient or permanent exceptions, see Plugin.cs for examples.
    private const int ERROR_UPSTREAM_UNAVAILBLE = 1001;
    private const int ERROR_INVALID_INPUT = 1002;
    private const int ERROR_NOT_FOUND = 1003;
    private const int ERROR_UNABLE_TO_PARSE_RESPONSE = 1004;
    private const string EBEVIS = "eBevis";

    public Plugin(
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        IOptions<ApplicationSettings> settings,
        IEvidenceSourceMetadata evidenceSourceMetadata,
        IEntityRegistryService entityRegistryService)
    {
        _client = httpClientFactory.CreateClient(Constants.SafeHttpClient);
        _logger = loggerFactory.CreateLogger<Plugin>();
        _settings = settings.Value;
        _evidenceSourceMetadata = evidenceSourceMetadata;
        _entityRegistryService = entityRegistryService;
    }

    [Function("CriterionSelectionSuitabilityTradeRegisterEnrolment")]
    public async Task<HttpResponseData> CriterionSelectionSuitabilityTradeRegisterEnrolment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CRITERION.SELECTION.SUITABILITY.TRADE_REGISTER_ENROLMENT")]
            HttpRequestData req,
            FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        evidenceHarvesterRequest.EvidenceCodeName = "UnitbasicInformation";

        return await EvidenceSourceResponse.CreateResponse(req, () => GetCriterionSelectionSuitabilityTradeRegisterEnrolment(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetCriterionSelectionSuitabilityTradeRegisterEnrolment(EvidenceHarvesterRequest req)
    {
        var response = await GetDataFromES_BR(req);
        return response.FindAll(x => x.EvidenceValueName == "IsInRegisterOfBusinessEnterprises");
    }

    [Function("CriterionSelectionEconomicFinanciaLStandingTurnoverSetUp")]
    public async Task<HttpResponseData> CriterionSelectionEconomicFinanciaLStandingTurnoverSetUp(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CRITERION.SELECTION.ECONOMIC_FINANCIAL_STANDING.TURNOVER.SET_UP")]
            HttpRequestData req,
        FunctionContext context)
    {

        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        evidenceHarvesterRequest.EvidenceCodeName = "UnitbasicInformation";
        return await EvidenceSourceResponse.CreateResponse(req, () => GetCriterionSelectionEconomicFinanciaLStandingTurnoverSetUp(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetCriterionSelectionEconomicFinanciaLStandingTurnoverSetUp(EvidenceHarvesterRequest req)
    {
        var response = await GetDataFromES_BR(req);
        string[] wantedFields = { "CreatedInCentralRegisterForLegalEntities", "Established" };
        return response.FindAll(x => wantedFields.Contains(x.EvidenceValueName));
    }

    [Function("CriterionSelectionTechnicalProfessionalAbilityManagementManagerialStaff")]
    public async Task<HttpResponseData> CriterionSelectionTechnicalProfessionalAbilityManagementManagerialStaff(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CRITERION.SELECTION.TECHNICAL_PROFESSIONAL_ABILITY.MANAGEMENT.MANAGERIAL_STAFF")]
            HttpRequestData req, FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        return await EvidenceSourceResponse.CreateResponse(req, () => GetCriterionSelectionTechnicalProfessionalAbilityManagementManagerialStaff(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetCriterionSelectionTechnicalProfessionalAbilityManagementManagerialStaff(EvidenceHarvesterRequest req)
    {
        req.EvidenceCodeName = "UnitBasicInformation";
        var task1 = GetDataFromES_BR(req);

        req.EvidenceCodeName = "CertificateOfRegistration";
        var task2 = GetDataFromES_BR(req);

        await Task.WhenAll(task1, task2);

        return new List<EvidenceValue>
            {
                task1.Result.Find(x => x.EvidenceValueName == "NumberOfEmployees"),
                task2.Result.Find(x => x.EvidenceValueName == "CertificateOfRegistrationPdfUrl")
            };
    }

    [Function("CriterionSelectionTechnicalProfessionalAbilityManagementAverageAnnualManpower")]
    public async Task<HttpResponseData> CriterionSelectionTechnicalProfessionalAbilityManagementAverageAnnualManpower(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CRITERION.SELECTION.TECHNICAL_PROFESSIONAL_ABILITY.MANAGEMENT.AVERAGE_ANNUAL_MANPOWER")]
            HttpRequestData req, FunctionContext context)
    {

        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();

        return await EvidenceSourceResponse.CreateResponse(req, () => GetCriterionSelectionTechnicalProfessionalAbilityManagementAverageAnnualManpower(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetCriterionSelectionTechnicalProfessionalAbilityManagementAverageAnnualManpower(EvidenceHarvesterRequest req)
    {
        req.EvidenceCodeName = "UnitBasicInformation";
        var task1 = GetDataFromES_BR(req);

        req.EvidenceCodeName = "CertificateOfRegistration";
        var task2 = GetDataFromES_BR(req);

        await Task.WhenAll(task1, task2);

        return new List<EvidenceValue>
            {
                task1.Result.Find(x => x.EvidenceValueName == "NumberOfEmployees"),
                task2.Result.Find(x => x.EvidenceValueName == "CertificateOfRegistrationPdfUrl")
            };
    }

    [Function("CriterionExclusionBusinessInsolvency")]
    public async Task<HttpResponseData> CriterionExclusionBusinessInsolvency(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CRITERION.EXCLUSION.BUSINESS.INSOLVENCY")]
            HttpRequestData req, FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        return await EvidenceSourceResponse.CreateResponse(req, () => GetCriterionExclusionBusinessInsolvency(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetCriterionExclusionBusinessInsolvency(EvidenceHarvesterRequest req)
    {
        req.EvidenceCodeName = "UnitbasicInformation";
        var response = await GetDataFromES_BR(req);

        string[] wantedFields = { "IsBeingDissolved", "IsBeingForciblyDissolved" };
        return response.FindAll(x => wantedFields.Contains(x.EvidenceValueName));
    }

    [Function("CriterionExclusionBusinessBankruptcy")]
    public async Task<HttpResponseData> CriterionExclusionBusinessBankruptcy(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CRITERION.EXCLUSION.BUSINESS.BANKRUPTCY")]
            HttpRequestData req, FunctionContext context)
    {

        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        evidenceHarvesterRequest.EvidenceCodeName = "UnitbasicInformation";

        return await EvidenceSourceResponse.CreateResponse(req, () => GetCriterionExclusionBusinessBankruptcy(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetCriterionExclusionBusinessBankruptcy(EvidenceHarvesterRequest req)
    {
        var response = await GetDataFromES_BR(req);
        return response.FindAll(x => x.EvidenceValueName == "IsUnderBankruptcy");
    }

    [Function("CriterionSelectionEconomicFinancialStandingTurnoverGeneralYearly")]
    public async Task<HttpResponseData> CriterionSelectionEconomicFinancialStandingTurnoverGeneralYearly(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CRITERION.SELECTION.ECONOMIC_FINANCIAL_STANDING.TURNOVER.GENERAL_YEARLY")]
            HttpRequestData req, FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        evidenceHarvesterRequest.EvidenceCodeName = "AnnualFinancialReport";

        return await EvidenceSourceResponse.CreateResponse(req, () => GetDataFromES_BR(evidenceHarvesterRequest));
    }

    [Function("BilpleieregisteretEbevis")]
    public async Task<HttpResponseData> BilpleieregisteretEbevis(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "BilpleieregisteretEbevis")]
        HttpRequestData req, FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        return await EvidenceSourceResponse.CreateResponse(req, () => GetBilpleieData(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetBilpleieData(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        var taskList = new List<Task<List<EvidenceValue>>>();

        taskList.Add(getDataFromES_ARBT(evidenceHarvesterRequest, "Bilpleieregisteret"));
        taskList.Add(getDataFromES_SVV(evidenceHarvesterRequest, "Verkstedregisteret"));
        await Task.WhenAll(taskList);
        return UnifyBilpleie(taskList);
    }

    private List<EvidenceValue> UnifyBilpleie(List<Task<List<EvidenceValue>>> taskList)
    {
        string orgnr = string.Empty;
        string godkjenningArbt = string.Empty;
        string registerstatusArbt = string.Empty;
        string godkjenningSVV = string.Empty;
        string godkjenningsnrSVV = string.Empty;
        bool approvedEbevis = false;

        foreach (var task in taskList )
        {
            var evidenceList = task.Result;
            //task has values
            if (evidenceList.Any(x => x.EvidenceValueName == "organisasjonsnummer"))
            {
                //task is arbt or svv
                if (evidenceList.Any(x => x.Source == "Arbeidstilsynet"))
                {
                    godkjenningArbt = evidenceList.First(x => x.EvidenceValueName == "godkjenningsstatus").Value.ToString();
                    orgnr = evidenceList.First(x => x.EvidenceValueName == "organisasjonsnummer").Value.ToString();
                    registerstatusArbt = evidenceList.First(x => x.EvidenceValueName == "registerstatusTekst").Value.ToString();
                }
                else if (evidenceList.Any(x => x.Source == "Statens vegvesen"))
                {
                    orgnr = evidenceList.First(x => x.EvidenceValueName == "organisasjonsnummer").Value.ToString();
                    godkjenningSVV = evidenceList.First(x => x.EvidenceValueName == "godkjenningstyper").Value.ToString();
                    godkjenningsnrSVV = evidenceList.First(x => x.EvidenceValueName == "godkjenningsnumre").Value.ToString();
                }
            }
        }

        approvedEbevis = godkjenningArbt.ToLower().Equals("godkjent") || godkjenningSVV.Length > 5;

        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, "BilpleieregisteretEbevis");
        ecb.AddEvidenceValue("organisasjonsnummer", orgnr, EBEVIS, false);
        ecb.AddEvidenceValue("godkjenningsstatusArbeidstilsynet", godkjenningArbt, "");
        ecb.AddEvidenceValue("registerstatusArbeidstilsynet", registerstatusArbt, ""); 
        ecb.AddEvidenceValue("godkjenningsstatusStatensVegvesen", godkjenningSVV, "");
        ecb.AddEvidenceValue("godkjenningsnumreStatensVegvesen", godkjenningsnrSVV, "");
        ecb.AddEvidenceValue("godkjentEbevis", approvedEbevis, EBEVIS);
        return ecb.GetEvidenceValues();
    }

    private async Task<List<EvidenceValue>> GetDataFromES_BR(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        string uri = _settings.ES_BREndpointUrl + evidenceHarvesterRequest.EvidenceCodeName + AddAuthorizationKey();
        return await GetDataFromOtherPlugin(evidenceHarvesterRequest, uri);
    }

    private async Task<List<EvidenceValue>> getDataFromES_SVV(EvidenceHarvesterRequest evidenceHarvesterRequest, string evidenceCodeName)
    {
        string uri = _settings.ES_SVVEndpointUrl + evidenceCodeName + AddAuthorizationKey();
        evidenceHarvesterRequest.EvidenceCodeName = evidenceCodeName;

        return await GetDataFromOtherPlugin(evidenceHarvesterRequest, uri, true);
    }
    private async Task<List<EvidenceValue>> getDataFromES_ARBT(EvidenceHarvesterRequest evidenceHarvesterRequest, string evidenceCodeName)
    {
        //ARBT only uses main units, so we switch the subject
        string uri = _settings.ES_ARBTEndpointUrl + evidenceCodeName + AddAuthorizationKey();
        var mainUnit = await _entityRegistryService.Get(evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber, false, true, false);
        evidenceHarvesterRequest.EvidenceCodeName = evidenceCodeName;
        evidenceHarvesterRequest.OrganizationNumber = mainUnit != null ? mainUnit.OrganizationNumber : evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber;
        evidenceHarvesterRequest.SubjectParty = new Party()
        {
            NorwegianOrganizationNumber = mainUnit != null ? mainUnit.OrganizationNumber : evidenceHarvesterRequest.SubjectParty.NorwegianOrganizationNumber
        };

        return await GetDataFromOtherPlugin(evidenceHarvesterRequest, uri, true);
    }

    private async Task<List<EvidenceValue>> GetDataFromOtherPlugin(EvidenceHarvesterRequest evidenceHarvesterRequest, string uri, bool meh = false)
    {
        try
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(evidenceHarvesterRequest), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
            request.Content = content;

            var response = await _client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<EvidenceValue>>(responseContent);
        }
        catch (Exception)
        {
            //For bilpleie one of the plugins will return exception based response, so we need to catch and ignore
            if (meh)
            {
                return new List<EvidenceValue>();
            }
            else
                throw;
        }

    }

    private string AddAuthorizationKey()
    {
        return $"?{_settings.FunctionKeyName}={_settings.FunctionKeyValue}";
    }

}
